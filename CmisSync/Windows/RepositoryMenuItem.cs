
namespace CmisSync {
    using System;
    using System.Windows.Forms;

    using CmisSync;
    using CmisSync.Lib.Cmis;
    using System.Windows;

    public class RepositoryMenuItem : ToolStripMenuItem, IObserver<Tuple<string, int>> {
        private StatusIconController controller;
        private ToolStripMenuItem openLocalFolderItem;
        private ToolStripMenuItem removeFolderFromSyncItem;
        private ToolStripMenuItem suspendItem;
        private ToolStripMenuItem editItem;
        private ToolStripMenuItem statusItem;
        private Repository repository;
        private SyncStatus status;
        private bool syncRequested;
        private int changesFound;
        private DateTime? changesFoundAt;
        private object counterLock = new object();
        private Control parent;

        public RepositoryMenuItem(Repository repo, StatusIconController controller, Control parent)
            : base(repo.Name) {
            this.repository = repo;
            this.controller = controller;
            this.parent = parent;
            this.Image = UIHelpers.GetBitmap("folder");

            this.openLocalFolderItem = new ToolStripMenuItem(Properties_Resources.OpenLocalFolder) {
                Image = UIHelpers.GetBitmap("folder")
            };

            this.openLocalFolderItem.Click += this.OpenFolderDelegate();

            this.editItem = new ToolStripMenuItem(Properties_Resources.Settings);
            this.editItem.Click += this.EditFolderDelegate();

            this.suspendItem = new ToolStripMenuItem(Properties_Resources.PauseSync);

            this.Status = repo.Status;

            this.suspendItem.Click += this.SuspendSyncFolderDelegate();
            this.statusItem = new ToolStripMenuItem("Searching for changes") {
                Enabled = false
            };

            this.removeFolderFromSyncItem = new ToolStripMenuItem(Properties_Resources.RemoveFolderFromSync);
            this.removeFolderFromSyncItem.Click += this.RemoveFolderFromSyncDelegate();

            this.DropDownItems.Add(this.statusItem);
            this.DropDownItems.Add(new ToolStripSeparator());
            this.DropDownItems.Add(this.openLocalFolderItem);
            this.DropDownItems.Add(this.suspendItem);
            this.DropDownItems.Add(this.editItem);
            this.DropDownItems.Add(new ToolStripSeparator());
            this.DropDownItems.Add(this.removeFolderFromSyncItem);

            this.repository.Queue.Subscribe(this);
        }

        // A method reference that makes sure that opening the
        // event log for each repository works correctly
        private EventHandler OpenFolderDelegate() {
            return delegate {
                this.controller.LocalFolderClicked(this.repository.Name);
            };
        }

        private EventHandler EditFolderDelegate() {
            return delegate {
                this.controller.EditFolderClicked(this.repository.Name);
            };
        }

        private EventHandler SuspendSyncFolderDelegate() {
            return delegate {
                this.controller.SuspendSyncClicked(this.repository.Name);
            };
        }

        private EventHandler RemoveFolderFromSyncDelegate() {
            return delegate {
                if (System.Windows.MessageBox.Show(
                    CmisSync.Properties_Resources.RemoveSyncQuestion,
                    CmisSync.Properties_Resources.RemoveSyncTitle,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question,
                    MessageBoxResult.No
                    ) == MessageBoxResult.Yes) {
                    this.controller.RemoveFolderFromSyncClicked(this.repository.Name);
                }
            };
        }

        public SyncStatus Status {
            get {
                return this.status;
            }

            set {
                this.status = value;
                switch (this.status)
                {
                case SyncStatus.Idle:
                    this.suspendItem.Text = Properties_Resources.PauseSync;
                    this.suspendItem.Image = UIHelpers.GetBitmap("media_playback_pause");
                    break;
                case SyncStatus.Suspend:
                    this.suspendItem.Text = Properties_Resources.ResumeSync;
                    this.suspendItem.Image = UIHelpers.GetBitmap("media_playback_start");
                    break;
                }
            }
        }

        public string RepositoryName { get { return this.repository.Name; } }

        public void OnCompleted() {
        }

        public void OnError(Exception e) {
        }

        public virtual void OnNext(Tuple<string, int> changeCounter) {
            if (changeCounter.Item1 == "DetectedChange") {
                if (changeCounter.Item2 > 0) {
                    lock(this.counterLock) {
                        this.changesFound = changeCounter.Item2;
                    }
                } else {
                    lock(this.counterLock) {
                        this.changesFound = 0;
                        this.changesFoundAt = this.syncRequested ? this.changesFoundAt : DateTime.Now;
                    }
                }

                this.UpdateStatusText();
            } else if (changeCounter.Item1 == "SyncRequested") {
                if (changeCounter.Item2 > 0) {
                    lock(this.counterLock) {
                        this.syncRequested = true;
                    }
                } else {
                    lock(this.counterLock) {
                        this.syncRequested = false;
                        this.changesFoundAt = this.syncRequested ? this.changesFoundAt : DateTime.Now;
                    }
                }

                this.UpdateStatusText();
            }
        }

        private void UpdateStatusText() {
            string message;
            lock(this.counterLock) {
                string since = string.Format(" since {0}", this.changesFoundAt);
                if (this.syncRequested == true) {
                    message = string.Format("Searching for changes{0}", this.changesFound > 0 ? string.Format(" (actually {0} found)", this.changesFound) : string.Empty);
                } else {
                    message = string.Format("{0} Changes detected{1}", this.changesFound > 0 ? this.changesFound.ToString() : "No", this.changesFoundAt != null ? since : string.Empty);
                }
            }

            this.parent.BeginInvoke((Action)delegate {
                this.statusItem.Text = message;
            });
        }
    }
}