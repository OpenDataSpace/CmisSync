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
﻿
namespace CmisSync {
    using System;
    using System.Windows.Forms;

    using CmisSync;
    using CmisSync.Lib.Cmis;
    using System.Windows;

    public class RepositoryMenuItem : ToolStripMenuItem {
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

        public RepositoryMenuItem(
            Repository repo,
            StatusIconController controller,
            Control parent) : base(repo.Name)
        {
            this.repository = repo;
            this.controller = controller;
            this.parent = parent;
            this.Image = UIHelpers.GetBitmap("folder");
            this.suspendItem = new ToolStripMenuItem(Properties_Resources.PauseSync, UIHelpers.GetBitmap("media_playback_pause"));
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

            this.openLocalFolderItem = new ToolStripMenuItem(Properties_Resources.OpenLocalFolder) {
                Image = UIHelpers.GetBitmap("folder")
            };

            this.openLocalFolderItem.Click += this.OpenFolderDelegate();

            this.editItem = new ToolStripMenuItem(Properties_Resources.Settings);
            this.editItem.Click += this.EditFolderDelegate();

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
                try {
                    this.parent.BeginInvoke((Action)delegate {
                        switch (this.status) {
                            case SyncStatus.Suspend:
                                this.suspendItem.Text = Properties_Resources.ResumeSync;
                                this.suspendItem.Image = UIHelpers.GetBitmap("media_playback_start");
                                break;
                            default:
                                this.suspendItem.Text = Properties_Resources.PauseSync;
                                this.suspendItem.Image = UIHelpers.GetBitmap("media_playback_pause");
                                break;
                        }
                    });
                } catch (InvalidOperationException e) {
                }
            }
        }

        public string RepositoryName { get { return this.repository.Name; } }

        private void UpdateStatusText() {
            string message;
            lock(this.counterLock) {
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

            this.parent.BeginInvoke((Action)delegate {
                this.statusItem.Text = message;
            });
        }
    }
}