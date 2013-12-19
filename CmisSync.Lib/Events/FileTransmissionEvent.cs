using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CmisSync.Lib.Events
{
    public class FileTransmissionEvent: ISyncEvent
    {
        public FileTransmissionType Type { get; private set; }

        public string Path { get; private set; }

        public delegate void TransmissionEventHandler(object sender, TransmissionProgressEventArgs e);

        public event TransmissionEventHandler TransmissionStatus = delegate { };

        public FileTransmissionEvent(FileTransmissionType type, string path)
        {
            if(path == null) {
                throw new ArgumentNullException("Argument null in FSEvent Constructor","path");
            }
            Type = type;
            Path = path;
        }

        public override string ToString()
        {
            return string.Format("FileTransmissionEvent with type \"{0}\" on path \"{1}\"", Type, Path);
        }

        public void ReportProgress(TransmissionProgressEventArgs status)
        {
            if (TransmissionStatus != null)
                TransmissionStatus(this, status);
        }
    }

    public class TransmissionProgressEventArgs
    {
        public long? BitsPerSecond { get; set; }
        public double? Percent { get; set; }
        public double? Length { get; set; }
        public double? ActualPosition { get; set; }
        public bool? Paused { get; set; }
        public bool? Resumed { get; set; }
        public bool? Aborted{ get; set; }
        public bool? Completed { get; set; }

        public TransmissionProgressEventArgs()
        {
            BitsPerSecond = null;
            Percent = null;
            Length = null;
            ActualPosition = null;
            Paused = null;
            Resumed = null;
            Aborted = null;
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
					(Aborted == e.Aborted);
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
