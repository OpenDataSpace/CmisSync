using System;
using System.IO;
using CmisSync.Lib.Events;

namespace CmisSync.Lib
{
    namespace Streams
    {
        public class ProgressStream : Stream
        {
            private FileTransmissionEvent TransmissionEvent;
            private Stream Stream;
            private long readpos = 0;
            private long writepos = 0;
            private DateTime start = DateTime.Now;
            private long bytesTransmittedSinceLastSecond = 0;

            public ProgressStream (Stream stream, FileTransmissionEvent e)
            {
                if (stream == null)
                    throw new ArgumentNullException ("The stream which progress should be reported cannot be null");
                if (e == null)
                    throw new ArgumentNullException ("The event, where to publish the prgress cannot be null");
                Stream = stream;
                TransmissionEvent = e;
                e.Status.Length = stream.Length;
            }

            public override bool CanRead {
                get {
                    return this.Stream.CanRead;
                }
            }

            public override bool CanSeek {
                get {
                    return this.Stream.CanSeek;
                }
            }

            public override bool CanWrite {
                get {
                    return this.Stream.CanWrite;
                }
            }

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

            public override void Flush ()
            {
                this.Stream.Flush ();
            }

            public override IAsyncResult BeginRead (byte[] buffer, int offset, int count, AsyncCallback callback, object state)
            {
                return this.Stream.BeginRead (buffer, offset, count, callback, state);
            }

            public override long Seek (long offset, SeekOrigin origin)
            {
                long result = this.Stream.Seek (offset, origin);
                this.TransmissionEvent.ReportProgress (new TransmissionProgressEventArgs () {ActualPosition = this.Stream.Position});
                return result;
            }

            public override int Read (byte[] buffer, int offset, int count)
            {
                int result = this.Stream.Read (buffer, offset, count);
                readpos+=result;
                CalculateBandwidth(result, readpos);
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
                this.Stream.Write (buffer, offset, count);
                writepos += offset + count;
                CalculateBandwidth(count, writepos);
            }

            protected override void Dispose (bool disposing)
            {
                base.Dispose (disposing);
            }

            private void CalculateBandwidth(int transmittedBytes, long pos) {
                this.bytesTransmittedSinceLastSecond+=transmittedBytes;
                TimeSpan diff = DateTime.Now - start ;
                if(diff.Seconds >= 1) {
                    long? result = TransmissionProgressEventArgs.CalcBitsPerSecond(start,DateTime.Now, bytesTransmittedSinceLastSecond);
                    this.TransmissionEvent.ReportProgress (new TransmissionProgressEventArgs () {ActualPosition = pos, BitsPerSecond = result});
                    this.bytesTransmittedSinceLastSecond = 0;
                    start = start + diff;
                }
            }
        }
    }
}

