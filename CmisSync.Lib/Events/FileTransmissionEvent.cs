using System;
using System.Collections.Generic;
using System.Text;

namespace CmisSync.Lib.Events
{
    public class FileTransmissionEvent: ISyncEvent
    {
        public FileTransmissionType Type { get; private set; }

        public string Path { get; private set; }

        public delegate void TransmissionEventHandler(object sender, TransmissionProgressEventArgs e);

        public event TransmissionEventHandler TransmissionStatus = delegate { };

        private TransmissionProgressEventArgs status;
        public TransmissionProgressEventArgs Status { get {return this.status;} private set { this.status = value; } }

        public FileTransmissionEvent(FileTransmissionType type, string path)
        {
            if(path == null) {
                throw new ArgumentNullException("Argument null in FSEvent Constructor","path");
            }
            Type = type;
            Path = path;
            status = new TransmissionProgressEventArgs();
        }

        public override string ToString()
        {
            return string.Format("FileTransmissionEvent with type \"{0}\" on path \"{1}\"", Type, Path);
        }

        public void ReportProgress(TransmissionProgressEventArgs status)
        {
                Status.Aborted = (status.Aborted != null) ? status.Aborted : Status.Aborted;
                Status.ActualPosition = (status.ActualPosition != null) ? status.ActualPosition : Status.ActualPosition;
                Status.Length = (status.Length != null) ? status.Length : Status.Length;
                Status.Completed = (status.Completed != null) ? status.Completed : Status.Completed;
                Status.BitsPerSecond = (status.BitsPerSecond != null) ? status.BitsPerSecond : Status.BitsPerSecond;
            if (TransmissionStatus != null)
                TransmissionStatus(this, Status);
        }
    }

    public class TransmissionProgressEventArgs
    {
        public long? BitsPerSecond { get; set; }
        public double? Percent { get{
                if(Length==null || ActualPosition == null || ActualPosition < 0 || Length < 0)
                    return null;
                if(Length == 0)
                    return 100d;
                return ((double)ActualPosition*100d)/(double)Length;
            } }
        public long? Length { get; set; }
        public long? ActualPosition { get; set; }
        public bool? Paused { get; set; }
        public bool? Resumed { get; set; }
        public bool? Aborted{ get; set; }
        public bool? Completed { get; set; }
        public Exception FailedException { get; set;}
        public static long? CalcBitsPerSecond(DateTime start, DateTime end, long bytes){
            if(end < start)
                throw new ArgumentException("The end of a transmission must be higher than the start");
            if(start == end){
                return null;
            }
            TimeSpan difference = end - start;
            return (bytes*8) / (difference.Seconds);
        }

        public TransmissionProgressEventArgs()
        {
            BitsPerSecond = null;
            Length = null;
            ActualPosition = null;
            Paused = null;
            Resumed = null;
            Aborted = null;
            FailedException = null;
        }

        public  override bool Equals(System.Object obj) {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }
            TransmissionProgressEventArgs e = obj as TransmissionProgressEventArgs;
            if ((System.Object)e == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (Length == e.Length) &&
                (BitsPerSecond == e.BitsPerSecond) &&
                    (ActualPosition == e.ActualPosition) &&
                    (Paused == e.Paused) &&
                    (Resumed == e.Resumed) &&
                    (Aborted == e.Aborted) &&
                    (FailedException == e.FailedException);
        }

        public override int GetHashCode ()
        {
            return base.GetHashCode ();
        }
    }

    public enum FileTransmissionType
    {
        UPLOAD_NEW_FILE,
        UPLOAD_MODIFIED_FILE,
        DOWNLOAD_NEW_FILE,
        DOWNLOAD_MODIFIED_FILE
    }
}
