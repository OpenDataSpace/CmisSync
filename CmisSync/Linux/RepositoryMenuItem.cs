//-----------------------------------------------------------------------
// <copyright file="RepositoryMenuItem.cs" company="GRAU DATA AG">
//
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General private License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//
//   This program is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//   GNU General private License for more details.
//
//   You should have received a copy of the GNU General private License
//   along with this program. If not, see http://www.gnu.org/licenses/.
//
// </copyright>
//-----------------------------------------------------------------------

namespace CmisSync {
    using System;

    using CmisSync;
    using CmisSync.Lib.Cmis;

    using Gtk;

    [CLSCompliant(false)]
    public class RepositoryMenuItem : ImageMenuItem {
        private StatusIconController controller;
        private ImageMenuItem openLocalFolderItem;
        private ImageMenuItem removeFolderFromSyncItem;
        private ImageMenuItem suspendItem;
        private ImageMenuItem editItem;
        private MenuItem separator1;
        private MenuItem separator2;
        private MenuItem statusItem;
        private Repository repository { get; set; }
        private SyncStatus status;
        private bool syncRequested;
        private int changesFound;
        private DateTime? changesFoundAt;
        private object counterLock = new object();
        private bool disposed = false;
        private bool successfulLogin = false;

        public RepositoryMenuItem(Repository repo, StatusIconController controller) : base(repo.Name) {
            this.SetProperty("always-show-image", new GLib.Value(true));
            this.repository = repo;
            this.controller = controller;
            this.Image = new Image(UIHelpers.GetIcon("dataspacesync-folder", 16));
            this.repository.PropertyChanged += (object sender, System.ComponentModel.PropertyChangedEventArgs e) => {
                if (e.PropertyName == "Status") {
                    this.Status = this.repository.Status;
                }

                if (e.PropertyName == "LastFinishedSync") {
                    this.changesFoundAt = this.repository.LastFinishedSync;
                    this.UpdateStatusText();
                }

                if (e.PropertyName == "NumberOfChanges") {
                    this.changesFound = this.repository.NumberOfChanges;
                    this.UpdateStatusText();
                }
            };

            this.openLocalFolderItem = new CmisSyncMenuItem(
                CmisSync.Properties_Resources.OpenLocalFolder) {
                Image = new Image(UIHelpers.GetIcon("dataspacesync-folder", 16))
            };

            this.openLocalFolderItem.Activated += this.OpenFolderDelegate();

            this.editItem = new CmisSyncMenuItem(CmisSync.Properties_Resources.Settings);
            this.editItem.Activated += this.EditFolderDelegate();

            this.suspendItem = new CmisSyncMenuItem(Properties_Resources.PauseSync);

            this.Status = this.repository.Status;

            this.suspendItem.Activated += this.SuspendSyncFolderDelegate();
            this.statusItem = new MenuItem(Properties_Resources.StatusSearchingForChanges) {
                Sensitive = false
            };

            this.removeFolderFromSyncItem = new CmisSyncMenuItem(
                CmisSync.Properties_Resources.RemoveFolderFromSync) {
                Image = new Image(UIHelpers.GetIcon("dataspacesync-deleted", 12))
            };
            this.removeFolderFromSyncItem.Activated += this.RemoveFolderFromSyncDelegate();
            this.separator1 = new SeparatorMenuItem();
            this.separator2 = new SeparatorMenuItem();

            var subMenu = new Menu();
            subMenu.Add(this.statusItem);
            subMenu.Add(this.separator1);
            subMenu.Add(this.openLocalFolderItem);
            subMenu.Add(this.suspendItem);
            subMenu.Add(this.editItem);
            subMenu.Add(this.separator2);
            subMenu.Add(this.removeFolderFromSyncItem);
            this.Submenu = subMenu;
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
                Application.Invoke(delegate {
                    switch (this.status)
                    {
                    case SyncStatus.Suspend:
                        (this.suspendItem.Child as Label).Text = Properties_Resources.ResumeSync;
                        this.suspendItem.Image = new Image(UIHelpers.GetIcon("dataspacesync-start", 12));
                        this.suspendItem.Sensitive = true;
                        break;
                    default:
                        (this.suspendItem.Child as Label).Text = Properties_Resources.PauseSync;
                        this.suspendItem.Image = new Image(UIHelpers.GetIcon("dataspacesync-pause", 12));
                        this.suspendItem.Sensitive = true;
                        break;
                    }
                });
            }
        }

        public string RepositoryName { get { return this.repository.Name; } }

        /// <summary>
        /// Releases all resource used by the <see cref="CmisSync.RepositoryMenuItem"/> object.
        /// </summary>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="CmisSync.RepositoryMenuItem"/>. The
        /// <see cref="Dispose"/> method leaves the <see cref="CmisSync.RepositoryMenuItem"/> in an unusable state.
        /// After calling <see cref="Dispose"/>, you must release all references to the
        /// <see cref="CmisSync.RepositoryMenuItem"/> so the garbage collector can reclaim the memory that the
        /// <see cref="CmisSync.RepositoryMenuItem"/> was occupying.</remarks>
        public void Dispose() {
            this.Dispose(true);
        }

        /// <summary>
        /// Dispose the specified Menu Item.
        /// </summary>
        /// <param name="disposing">If set to <c>true</c> disposing.</param>
        public void Dispose(bool disposing) {
            if (this.disposed) {
                return;
            }

            if (disposing) {
                if (this.editItem != null) {
                    this.editItem.Dispose();
                }

                if (this.statusItem != null) {
                    this.statusItem.Dispose();
                }

                if (this.suspendItem != null) {
                    this.suspendItem.Dispose();
                }

                if (this.openLocalFolderItem != null) {
                    this.openLocalFolderItem.Dispose();
                }

                if (this.separator1 != null) {
                    this.separator1.Dispose();
                }

                if (this.separator2 != null) {
                    this.separator2.Dispose();
                }

                if (this.removeFolderFromSyncItem != null) {
                    this.removeFolderFromSyncItem.Dispose();
                }
            }

            this.disposed = true;
        }

        private void UpdateStatusText() {
            string message;
            lock (this.counterLock) {
                if (this.syncRequested == true) {
                    if (this.changesFound > 0) {
                        message = string.Format(Properties_Resources.StatusSearchingForChangesAndFound, this.changesFound.ToString());
                    } else {
                        message = Properties_Resources.StatusSearchingForChanges;
                    }
                } else {
                    if (this.changesFound > 0) {
                        if (this.changesFoundAt == null) {
                            message = string.Format(Properties_Resources.StatusChangesDetected, this.changesFound.ToString());
                        } else {
                            message = string.Format(Properties_Resources.StatusChangesDetectedSince, this.changesFound.ToString(), this.changesFoundAt.Value);
                        }
                    } else {
                        if (this.changesFoundAt == null) {
                            message = string.Format(Properties_Resources.StatusNoChangeDetected);
                        } else {
                            message = string.Format(Properties_Resources.StatusNoChangeDetectedSince, this.changesFoundAt.Value);
                        }
                    }
                }
            }

            Application.Invoke(delegate {
                try {
                    (this.statusItem.Child as Label).Text = message;
                } catch(NullReferenceException) {
                }
            });
        }
    }
}