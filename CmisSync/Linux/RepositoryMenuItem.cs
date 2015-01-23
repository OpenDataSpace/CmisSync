
namespace CmisSync
{
    using System;

    using CmisSync;
    using CmisSync.Lib.Cmis;

    using Gtk;

    [CLSCompliant(false)]
    public class RepositoryMenuItem : ImageMenuItem, IObserver<Tuple<string, int>> {
        private StatusIconController controller;
        private ImageMenuItem openLocalFolderItem;
        private ImageMenuItem removeFolderFromSyncItem;
        private ImageMenuItem suspendItem;
        private ImageMenuItem editItem;
        private MenuItem statusItem;
        private Repository repository;
        private SyncStatus status;
        private bool syncRequested;
        private int changesFound;
        private DateTime? changesFoundAt;
        private object counterLock = new object();

        public RepositoryMenuItem(Repository repo, StatusIconController controller) : base(repo.Name) {
            this.SetProperty("always-show-image", new GLib.Value(true));
            this.repository = repo;
            this.controller = controller;
            this.Image = new Image(UIHelpers.GetIcon("dataspacesync-folder", 16));

            this.openLocalFolderItem = new CmisSyncMenuItem(
                CmisSync.Properties_Resources.OpenLocalFolder) {
                Image = new Image(UIHelpers.GetIcon("dataspacesync-folder", 16))
            };

            this.openLocalFolderItem.Activated += this.OpenFolderDelegate();

            this.editItem = new CmisSyncMenuItem(CmisSync.Properties_Resources.Settings);
            this.editItem.Activated += this.EditFolderDelegate();

            this.suspendItem = new CmisSyncMenuItem(Properties_Resources.PauseSync);

            this.Status = repo.Status;

            this.suspendItem.Activated += this.SuspendSyncFolderDelegate();
            this.statusItem = new MenuItem("Searching for changes") {
                Sensitive = false
            };

            this.removeFolderFromSyncItem = new CmisSyncMenuItem(
                CmisSync.Properties_Resources.RemoveFolderFromSync) {
                Image = new Image(UIHelpers.GetIcon("dataspacesync-deleted", 12))
            };
            this.removeFolderFromSyncItem.Activated += this.RemoveFolderFromSyncDelegate();

            var subMenu = new Menu();
            subMenu.Add(this.statusItem);
            subMenu.Add(new SeparatorMenuItem());
            subMenu.Add(this.openLocalFolderItem);
            subMenu.Add(this.suspendItem);
            subMenu.Add(this.editItem);
            subMenu.Add(new SeparatorMenuItem());
            subMenu.Add(this.removeFolderFromSyncItem);
            this.Submenu = subMenu;

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
                using (Dialog dialog = new Dialog(
                    string.Format(CmisSync.Properties_Resources.RemoveSyncTitle),
                    null,
                    Gtk.DialogFlags.DestroyWithParent))
                {
                    dialog.Modal = true;
                    using (var noButton = dialog.AddButton("No, please continue synchronizing", ResponseType.No))
                        using (var yesButton = dialog.AddButton("Yes, stop synchronizing permanently", ResponseType.Yes))
                    {
                        dialog.Response += delegate(object obj, ResponseArgs args) {
                            if (args.ResponseId == ResponseType.Yes) {
                                this.controller.RemoveFolderFromSyncClicked(this.repository.Name);
                            }
                        };
                        dialog.Run();
                        dialog.Destroy();
                    }
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
                    (this.suspendItem.Child as Label).Text = Properties_Resources.PauseSync;
                    this.suspendItem.Image = new Image(UIHelpers.GetIcon("dataspacesync-pause", 12));
                    break;
                case SyncStatus.Suspend:
                    (this.suspendItem.Child as Label).Text = Properties_Resources.ResumeSync;
                    this.suspendItem.Image = new Image(UIHelpers.GetIcon("dataspacesync-start", 12));
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
            Application.Invoke(delegate {
                (this.statusItem.Child as Label).Text = message;
            });
        }
    }
}