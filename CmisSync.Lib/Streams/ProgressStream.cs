using System;
using System.IO;
using CmisSync.Lib.Events;
using System.Timers;

namespace CmisSync.Lib
{
    namespace Streams
    {
        public class ProgressStream : StreamWrapper
        {
            private FileTransmissionEvent TransmissionEvent;
            private DateTime start = DateTime.Now;
            private long bytesTransmittedSinceLastSecond = 0;
            private Timer blockingDetectionTimer;

            public ProgressStream (Stream stream, FileTransmissionEvent e) : base (stream)
            {
                if (e == null)
                    throw new ArgumentNullException ("The event, where to publish the prgress cannot be null");
                try{
                    e.Status.Length = stream.Length;
                }catch(NotSupportedException){
                    e.Status.Length = null;
                }
                TransmissionEvent = e;
                blockingDetectionTimer = new Timer(2000);
                blockingDetectionTimer.Elapsed += delegate(object sender, ElapsedEventArgs args) {
                    this.TransmissionEvent.ReportProgress(new TransmissionProgressEventArgs() { BitsPerSecond = (long) (bytesTransmittedSinceLastSecond/blockingDetectionTimer.Interval) });
                    bytesTransmittedSinceLastSecond = 0;
               };
            }

            #region overrideCode
            public override long Length {
                get {
                    long length = this.Stream.Length;
                    if (length > this.TransmissionEvent.Status.Length)
                        this.TransmissionEvent.ReportProgress (new TransmissionProgressEventArgs () {Length = length});
                    return length;
                }
            }

            public override long Position {
                get {
                    long pos = this.Stream.Position;
                    if (pos != this.TransmissionEvent.Status.ActualPosition)
                        this.TransmissionEvent.ReportProgress (new TransmissionProgressEventArgs () {ActualPosition = pos});
                    return pos;
                }
                set {
                    this.Stream.Position = value;
                    if (value != this.TransmissionEvent.Status.ActualPosition)
                        this.TransmissionEvent.ReportProgress (new TransmissionProgressEventArgs () {ActualPosition = value});
                }
            }

            public override long Seek (long offset, SeekOrigin origin)
            {
                long result = this.Stream.Seek (offset, origin);
                this.TransmissionEvent.ReportProgress (new TransmissionProgressEventArgs () {ActualPosition = this.Stream.Position});
                return result;
            }

            public override int Read (byte[] buffer, int offset, int count)
            {
                if (TransmissionEvent.Status.Aborting.GetValueOrDefault()) {
                    TransmissionEvent.ReportProgress(new TransmissionProgressEventArgs() { Aborting = false, Aborted = true });
                    throw new ContentTasks.AbortException(TransmissionEvent.Path);
                }
                int result = this.Stream.Read(buffer, offset, count);
                CalculateBandwidth(result);
                return result;
            }

            public override void SetLength (long value)
            {
                this.Stream.SetLength (value);
                if (this.TransmissionEvent.Status.Length == null || value > (long) this.TransmissionEvent.Status.Length)
                    this.TransmissionEvent.ReportProgress (new TransmissionProgressEventArgs () {Length = value});
            }

            public override void Write (byte[] buffer, int offset, int count)
            {
                if (TransmissionEvent.Status.Aborting.GetValueOrDefault()) {
                    TransmissionEvent.ReportProgress(new TransmissionProgressEventArgs() { Aborting = false, Aborted = true });
                    throw new ContentTasks.AbortException(TransmissionEvent.Path);
                }
                this.Stream.Write(buffer, offset, count);
                CalculateBandwidth(count);
            }
            #endregion

            private void CalculateBandwidth(int transmittedBytes) {
                this.bytesTransmittedSinceLastSecond+=transmittedBytes;
                TimeSpan diff = DateTime.Now - start ;
                long? pos;
                try{
                    pos = Stream.Position;
                }catch (NotSupportedException) {
                    pos = null;
                }
                if(diff.Seconds >= 1) {
                    long? result = TransmissionProgressEventArgs.CalcBitsPerSecond(start, DateTime.Now, bytesTransmittedSinceLastSecond);
                    this.TransmissionEvent.ReportProgress (new TransmissionProgressEventArgs () {ActualPosition = pos, BitsPerSecond = result});
                    this.bytesTransmittedSinceLastSecond = 0;
                    start = start + diff;
                    blockingDetectionTimer.Stop();
                    blockingDetectionTimer.Start();
                }else{
                    this.TransmissionEvent.ReportProgress (new TransmissionProgressEventArgs () {ActualPosition = pos});
                }
            }

            /// <summary>
            /// Close this instance and calculates the bandwidth of the last second.
            /// </summary>
            public override void Close ()
            {
                long? result = TransmissionProgressEventArgs.CalcBitsPerSecond(start, DateTime.Now.AddMilliseconds(1), bytesTransmittedSinceLastSecond);
                this.TransmissionEvent.ReportProgress(new TransmissionProgressEventArgs() { BitsPerSecond = result });
                base.Close ();
            }
        }
    }
}

