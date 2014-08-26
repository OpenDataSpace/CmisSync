using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CmisSync.Lib.Events;
using CmisSync.Lib.Config;


namespace CmisSync
{
    public class TransmissionItem : IDisposable
    {
        public string FullPath { get; private set; }
        public string Repo { get; private set; }
        public string Path { get; private set; }

        public string Status { get; set; }
        public string Progress { get; set; }

        private string State;
        private FileTransmissionType Type;

        private bool Disposed = false;

        private object lockController = new object();
        private TransmissionController Controller_;
        public TransmissionController Controller
        {
            get
            {
                lock (lockController)
                {
                    return Controller_;
                }
            }
            set
            {
                lock (lockController)
                {
                    Controller_ = value;
                }
            }
        }

        private FileTransmissionEvent Transmission;

        private static readonly double UpdateIntervalSeconds = 1;
        private DateTime UpdateTime;

        public TransmissionItem(FileTransmissionEvent transmission)
        {
            Transmission = transmission;
            UpdateTime = new DateTime(1970, 1, 1);

            FullPath = transmission.Path;
            Repo = string.Empty;
            Path = FullPath;
            foreach (RepoInfo folder in ConfigManager.CurrentConfig.Folders)
            {
                if (FullPath.StartsWith(folder.LocalPath))
                {
                    Repo = folder.DisplayName;
                    Path = FullPath.Substring(folder.LocalPath.Length);
                }
            }
            Type = transmission.Type;

            Update(this, transmission.Status);

            Transmission.TransmissionStatus += Update;
        }

        ~TransmissionItem()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                Transmission.TransmissionStatus -= Update;
                Disposed = true;
            }
        }

        private void Update(object sender, TransmissionProgressEventArgs status)
        {
            string oldState = State;

            State = string.Empty;
            switch (Type)
            {
                case FileTransmissionType.DOWNLOAD_NEW_FILE:
                    State = "Download";
                    break;
                case FileTransmissionType.DOWNLOAD_MODIFIED_FILE:
                    goto case FileTransmissionType.DOWNLOAD_NEW_FILE;
                case FileTransmissionType.UPLOAD_NEW_FILE:
                    State = "Upload";
                    break;
                case FileTransmissionType.UPLOAD_MODIFIED_FILE:
                    goto case FileTransmissionType.UPLOAD_NEW_FILE;
                default:
                    break;
            }
            if (status.Completed.GetValueOrDefault())
            {
                State += " Finished";
            }
            else if (status.Aborted.GetValueOrDefault())
            {
                State += " Aborted";
            }
            else if (status.Aborting.GetValueOrDefault())
            {
                State += " Aborting";
            }
            else if (status.Paused.GetValueOrDefault())
            {
                State += " Paused";
            }
            else if (status.Resumed.GetValueOrDefault())
            {
                State += " Resumed";
            }
            else if (status.Started.GetValueOrDefault())
            {
                State += " Started";
            }

            if (oldState == State)
            {
                TimeSpan diff = DateTime.Now - UpdateTime;
                if (diff.TotalSeconds < UpdateIntervalSeconds)
                {
                    return;
                }
            }

            UpdateTime = DateTime.Now;

            Status = State;
            long speed = status.BitsPerSecond.GetValueOrDefault();
            if (speed != 0)
            {
                Status += " (" + CmisSync.Lib.Utils.FormatBandwidth(speed) + ")";
            }

            if (status.Percent == null)
            {
                Progress = string.Empty;
            }
            else
            {
                double percent = status.Percent.GetValueOrDefault();
                long position = status.ActualPosition.GetValueOrDefault();
                long length = status.Length.GetValueOrDefault();
                Progress = CmisSync.Lib.Utils.FormatPercent(percent);
                Progress += "(";
                Progress += CmisSync.Lib.Utils.FormatSize(position);
                Progress += "/";
                Progress += CmisSync.Lib.Utils.FormatSize(length);
                Progress += ")";
            }

            lock (lockController)
            {
                if (Controller != null)
                {
                    Controller.UpdateTransmission(this);
                }
            }
        }
    }

    public class TransmissionController
    {
        public event Action ShowWindowEvent = delegate { };
        public event Action HideWindowEvent = delegate { };

        public event Action<TransmissionItem> InsertTransmissionEvent = delegate { };
        public event Action<TransmissionItem> UpdateTransmissionEvent = delegate { };
        public event Action<TransmissionItem> DeleteTransmissionEvent = delegate { };

        private List<TransmissionItem> TransmissionList = new List<TransmissionItem>();

        public TransmissionController()
        {
            Program.Controller.ShowTransmissionWindowEvent += ShowWindow;

            Program.Controller.OnTransmissionListChanged += Controller_OnTransmissionListChanged;
        }

        public void ShowWindow()
        {
            ShowWindowEvent();
        }

        public void HideWindow()
        {
            HideWindowEvent();
        }

        public void UpdateTransmission(TransmissionItem item)
        {
            UpdateTransmissionEvent(item);
        }

        private void Controller_OnTransmissionListChanged()
        {
            foreach (TransmissionItem item in TransmissionList)
            {
                //  un-register TransmissionController.UpdateTransmissionEvent
                item.Controller = null;

                DeleteTransmissionEvent(item);
            }
            foreach (TransmissionItem item in TransmissionList)
            {
                //  un-register FileTransmissionEvent.TransmissionStatus
                item.Dispose();
            }
            TransmissionList.Clear();

            List<FileTransmissionEvent> transmissions = Program.Controller.ActiveTransmissions();
            foreach (FileTransmissionEvent transmission in transmissions)
            {
                TransmissionItem item = new TransmissionItem(transmission);
                TransmissionList.Add(item);

                InsertTransmissionEvent(item);

                //  register TransmissionController.UpdateTransmissionEvent
                item.Controller = this;
            }
        }
    }
}
