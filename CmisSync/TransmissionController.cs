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

        public string Status { get; private set; }
        public string Progress { get; private set; }

        public bool Done { get; private set; }

        public DateTime UpdateTime { get; private set; }

        private static readonly double UpdateIntervalSeconds = 1;

        private FileTransmissionEvent Transmission;

        private FileTransmissionType Type;
        private string State;

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
            string oldStatus = Status;
            string oldProgress = Progress;

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
                Done = true;
            }
            else if (status.Aborted.GetValueOrDefault())
            {
                State += " Aborted";
                Done = true;
            }
            else if (status.FailedException != null)
            {
                State += " Aborted";
                Done = true;
            }
            else if (status.Aborting.GetValueOrDefault())
            {
                State += " Aborting";
                Done = false;
            }
            else if (status.Paused.GetValueOrDefault())
            {
                State += " Paused";
                Done = false;
            }
            else if (status.Resumed.GetValueOrDefault())
            {
                State += " Resumed";
                Done = false;
            }
            else if (status.Started.GetValueOrDefault())
            {
                State += " Started";
                Done = false;
            }
            else
            {
                Done = false;
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

            if (oldStatus == Status && oldProgress == Progress)
            {
                return;
            }

            lock (lockController)
            {
                if (Controller != null)
                {
                    Controller.UpdateTransmission(this);
                    Controller.ShowTransmission(this);
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

        public event Action ShowTransmissionListEvent = delegate { };
        public event Action<TransmissionItem> ShowTransmissionEvent = delegate { };

        private List<TransmissionItem> TransmissionList = new List<TransmissionItem>();
        private int TransmissionLimit = 15;
        private HashSet<string> FullPathList = new HashSet<string>();

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

        public void ShowTransmissionList()
        {
            ShowTransmissionListEvent();
        }

        public void ShowTransmission(TransmissionItem item)
        {
            ShowTransmissionEvent(item);
        }

        public class TransmissionCompare : IComparer<TransmissionItem>
        {
            public int Compare(TransmissionItem x, TransmissionItem y)
            {
                if (x.UpdateTime == y.UpdateTime)
                {
                    return 0;
                }
                if (x.UpdateTime > y.UpdateTime)
                {
                    return -1;
                }
                return 1;
            }
        }

        private void Controller_OnTransmissionListChanged()
        {
            List<FileTransmissionEvent> transmissions = Program.Controller.ActiveTransmissions();
            foreach (FileTransmissionEvent transmission in transmissions)
            {
                string fullPath = transmission.Path;
                if (FullPathList.Contains(fullPath))
                {
                    continue;
                }

                FullPathList.Add(fullPath);
                TransmissionItem item = new TransmissionItem(transmission);
                TransmissionList.Insert(0, item);
                InsertTransmissionEvent(item);

                //  register TransmissionController.UpdateTransmissionEvent
                item.Controller = this;
            }

            if (FullPathList.Count > TransmissionLimit)
            {
                TransmissionList.Sort(new TransmissionCompare());
                for (int i = FullPathList.Count - 1; i >= 0 && TransmissionList.Count > TransmissionLimit; --i)
                {
                    TransmissionItem item = TransmissionList[i];
                    if (item.Done)
                    {
                        //  un-register TransmissionController.UpdateTransmissionEvent
                        item.Controller = null;

                        DeleteTransmissionEvent(TransmissionList[i]);
                        TransmissionList.RemoveAt(i);
                        FullPathList.Remove(item.FullPath);

                        //  un-register FileTransmissionEvent.TransmissionStatus
                        item.Dispose();
                    }
                }
            }

            ShowTransmissionListEvent();
        }
    }
}
