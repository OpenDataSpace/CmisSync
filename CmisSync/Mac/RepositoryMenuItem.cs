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
    using System.Drawing;
    using System.IO;
    using System.Collections.Generic;
    using System.Text;

    using Mono.Unix.Native;

    using log4net;

    using MonoMac.Foundation;
    using MonoMac.AppKit;
    using MonoMac.ObjCRuntime;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Cmis;
    using CmisSync.Lib.Config;

    [CLSCompliant(false)]
    public class RepositoryMenuItem : NSMenuItem {
        private StatusIconController controller;
        private NSMenuItem openLocalFolderItem;
        private NSMenuItem removeFolderFromSyncItem;
        private NSMenuItem suspendItem;
        private NSMenuItem editItem;
        private NSMenuItem statusItem;
        private Repository repository;
        private SyncStatus status;
        private bool syncRequested;
        private int changesFound;
        private DateTime? changesFoundAt;
        private object counterLock = new object();
        private NSImage pauseImage = new NSImage(UIHelpers.GetImagePathname("media_playback_pause"));
        private NSImage resumeImage = new NSImage(UIHelpers.GetImagePathname ("media_playback_start"));
        private NSImage removeImage = new NSImage(UIHelpers.GetImagePathname("process-syncing-error"));
        private NSImage folderImage = new NSImage(UIHelpers.GetImagePathname ("cmissync-folder", "icns"));

        public RepositoryMenuItem(Repository repo, StatusIconController controller) : base(repo.Name) {
            this.repository = repo;
            this.controller = controller;
            this.Image = this.folderImage;
            this.Image.Size = new SizeF(16, 16);
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
            this.openLocalFolderItem = new NSMenuItem(Properties_Resources.OpenLocalFolder) {
                Image = this.folderImage
            };
            this.openLocalFolderItem.Image.Size = new SizeF(16, 16);

            this.openLocalFolderItem.Activated += this.OpenFolderDelegate();

            this.editItem = new NSMenuItem(Properties_Resources.Settings);
            this.editItem.Activated += this.EditFolderDelegate();

            this.suspendItem = new NSMenuItem(Properties_Resources.PauseSync);

            this.Status = repo.Status;

            this.suspendItem.Activated += this.SuspendSyncFolderDelegate();
            this.statusItem = new NSMenuItem(Properties_Resources.StatusSearchingForChanges) {
                Enabled = false
            };

            this.removeFolderFromSyncItem = new NSMenuItem(Properties_Resources.RemoveFolderFromSync);
            this.removeFolderFromSyncItem.Activated += this.RemoveFolderFromSyncDelegate();

            var subMenu = new NSMenu();
            subMenu.AddItem(this.statusItem);
            subMenu.AddItem(NSMenuItem.SeparatorItem);
            subMenu.AddItem(this.openLocalFolderItem);
            subMenu.AddItem(this.suspendItem);
            subMenu.AddItem(this.editItem);
            subMenu.AddItem(NSMenuItem.SeparatorItem);
            subMenu.AddItem(this.removeFolderFromSyncItem);
            this.Submenu = subMenu;
        }

        private EventHandler RemoveFolderFromSyncDelegate() {
            return delegate {
                NSAlert alert = NSAlert.WithMessage(Properties_Resources.RemoveSyncQuestion,"No, please continue syncing","Yes, stop syncing",null,"");
                alert.Icon = this.removeImage;
                alert.Window.OrderFrontRegardless();
                int i = alert.RunModal();
                if (i == 0) {
                    this.controller.RemoveFolderFromSyncClicked(this.repository.Name);
                }
            };
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
        public SyncStatus Status {
            get {
                return this.status;
            }

            set {
                this.status = value;
                switch (this.status)
                {
                    case SyncStatus.Idle:
                        this.suspendItem.Title = Properties_Resources.PauseSync;
                        this.suspendItem.Image = this.pauseImage;
                        break;
                    case SyncStatus.Suspend:
                        this.suspendItem.Title = Properties_Resources.ResumeSync;
                        this.suspendItem.Image = this.resumeImage;
                        break;
                }
            }
        }

        public string RepositoryName { get { return this.repository.Name; } }

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

            InvokeOnMainThread (delegate {
                this.statusItem.Title = message;
            });
        }
    }
}