//-----------------------------------------------------------------------
// <copyright file="StatusIcon.cs" company="GRAU DATA AG">
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
//   CmisSync, an instant update workflow to Git.
//   Copyright (C) 2010  Hylke Bons <hylkebons@gmail.com>
//
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//
//   This program is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//   GNU General Public License for more details.
//
//   You should have received a copy of the GNU General Public License
//   along with this program. If not, see <http://www.gnu.org/licenses/>.

namespace CmisSync {
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Text;

    using CmisSync.Lib.Cmis;
    using CmisSync.Lib.Config;
    using CmisSync.Lib.Events;

    using log4net;

    using Mono.Unix.Native;

    using MonoMac.AppKit;
    using MonoMac.Foundation;
    using MonoMac.ObjCRuntime;

    [CLSCompliant(false)]
    public class StatusIcon : NSObject {
        public StatusIconController Controller = new StatusIconController();

        private NSMenu menu;

        private NSStatusItem status_item;

        private NSMenuItem add_item;
        private NSMenuItem about_item;
        private NSMenuItem quit_item;
        private NSMenuItem log_item;
        private NSMenuItem general_settings_item;
        private NSMenuItem transmission_item;

        private NSImage [] animation_frames;
        private NSImage [] animation_frames_active;
        private NSImage error_image;
        private NSImage error_image_active;
        private List<RepositoryMenuItem> repoItems;

        public StatusIcon() : base() {
            using (var a = new NSAutoreleasePool()) {
                this.CreateAnimationFrames();

                this.status_item = NSStatusBar.SystemStatusBar.CreateStatusItem(28);
                this.status_item.HighlightMode = true;
                this.status_item.Image = this.animation_frames[0];

                this.status_item.Image               = this.animation_frames[0];
                this.status_item.Image.Size          = new SizeF(16, 16);
                this.status_item.AlternateImage      = this.animation_frames_active[0];
                this.status_item.AlternateImage.Size = new SizeF(16, 16);

                this.CreateMenu();
            }

            this.Controller.UpdateIconEvent += delegate(int icon_frame) {
                using (var a = new NSAutoreleasePool()) {
                    BeginInvokeOnMainThread(delegate {
                        if (icon_frame > -1) {
                            this.status_item.Image               = this.animation_frames[icon_frame];
                            this.status_item.Image.Size          = new SizeF(16, 16);
                            this.status_item.AlternateImage      = this.animation_frames_active[icon_frame];
                            this.status_item.AlternateImage.Size = new SizeF(16, 16);
                        } else {
                            this.status_item.Image               = this.error_image;
                            this.status_item.AlternateImage      = this.error_image_active;
                            this.status_item.Image.Size          = new SizeF(16, 16);
                            this.status_item.AlternateImage.Size = new SizeF(16, 16);
                        }
                    });
                }
            };

            this.Controller.UpdateMenuEvent += delegate {
                using (var a = new NSAutoreleasePool()) {
                    this.InvokeOnMainThread(() => this.CreateMenu());
                }
            };

            this.Controller.UpdateSuspendSyncFolderEvent += delegate(string reponame) {
                using (var a = new NSAutoreleasePool()){
                    this.InvokeOnMainThread(delegate {
                        foreach (var repoItem in this.repoItems) {
                            if (repoItem.RepositoryName == reponame) {
                                foreach (var repo in Program.Controller.Repositories) {
                                    if (repo.Name.Equals(reponame)) {
                                        repoItem.Status = repo.Status;
                                        break;
                                    }
                                }

                                break;
                            }
                        }
                    });
                }
            };
        }

        private SyncStatus getSyncStatus(string reponame) {
            foreach (var repo in Program.Controller.Repositories) {
                if (repo.Name.Equals(reponame)) {
                    return repo.Status;
                }
            }

            return SyncStatus.Idle;
        }

        public void CreateMenu() {
            using (NSAutoreleasePool a = new NSAutoreleasePool()) {
                this.repoItems = new List<RepositoryMenuItem>();
                this.menu = new NSMenu();
                this.menu.AutoEnablesItems = false;

                this.general_settings_item = new NSMenuItem() {
                    Title = Properties_Resources.EditTitle
                };

                this.general_settings_item.Activated += delegate {
                    this.Controller.SettingClicked();
                };

                this.transmission_item = new NSMenuItem() {
                    Title = Properties_Resources.Transmission
                };

                this.transmission_item.Activated += delegate {
                    this.Controller.TransmissionClicked();
                };

                this.log_item = new NSMenuItem() {
                    Title = Properties_Resources.ViewLog
                };

                this.log_item.Activated += delegate {
                    this.Controller.LogClicked();
                };

                this.add_item = new NSMenuItem() {
                    Title   = Properties_Resources.AddARemoteFolder,
                    Enabled = true
                };

                this.add_item.Activated += delegate {
                    this.Controller.AddRemoteFolderClicked();
                };

                this.about_item = new NSMenuItem() {
                    Title   = string.Format(Properties_Resources.About, Properties_Resources.ApplicationName),
                    Enabled = true
                };

                this.about_item.Activated += delegate {
                    Controller.AboutClicked ();
                };

                this.quit_item = new NSMenuItem() {
                    Title   = Properties_Resources.Exit,
                    Enabled = true
                };

                this.quit_item.Activated += delegate {
                    Controller.QuitClicked();
                };

                var repos = Program.Controller.Repositories;
                if (repos.Length > 0) {
                    foreach (var repo in repos) {
                        var repoItem = new RepositoryMenuItem(repo, this.Controller);
                        this.repoItems.Add(repoItem);
                        this.menu.AddItem(repoItem);
                    };
                    this.menu.AddItem(NSMenuItem.SeparatorItem);
                }

                this.menu.AddItem(this.add_item);
                this.menu.AddItem(NSMenuItem.SeparatorItem);
                this.menu.AddItem(this.general_settings_item);
                this.menu.AddItem(this.transmission_item);
                this.menu.AddItem(this.log_item);
                this.menu.AddItem(this.about_item);
                this.menu.AddItem(NSMenuItem.SeparatorItem);
                this.menu.AddItem(this.quit_item);

                this.menu.Delegate = new StatusIconMenuDelegate();
                this.status_item.Menu = this.menu;
            }
        }

        private void CreateAnimationFrames() {
            this.animation_frames = new NSImage[] {
                new NSImage(UIHelpers.GetImagePathname("process-syncing-i")),
                new NSImage(UIHelpers.GetImagePathname("process-syncing-ii")),
                new NSImage(UIHelpers.GetImagePathname("process-syncing-iii")),
                new NSImage(UIHelpers.GetImagePathname("process-syncing-iiii")),
                new NSImage(UIHelpers.GetImagePathname("process-syncing-iiiii"))
            };

            this.animation_frames_active = new NSImage[] {
                new NSImage(UIHelpers.GetImagePathname("process-syncing-i-active")),
                new NSImage(UIHelpers.GetImagePathname("process-syncing-ii-active")),
                new NSImage(UIHelpers.GetImagePathname("process-syncing-iii-active")),
                new NSImage(UIHelpers.GetImagePathname("process-syncing-iiii-active")),
                new NSImage(UIHelpers.GetImagePathname("process-syncing-iiiii-active"))
            };

            this.error_image = new NSImage(UIHelpers.GetImagePathname("process-syncing-error"));
            this.error_image_active = new NSImage(UIHelpers.GetImagePathname ("process-syncing-error-active"));
        }
    }

    public class StatusIconMenuDelegate : NSMenuDelegate {
        public override void MenuWillHighlightItem(NSMenu menu, NSMenuItem item) {
        }

        public override void MenuWillOpen(NSMenu menu) {
            InvokeOnMainThread (delegate {
                NSApplication.SharedApplication.DockTile.BadgeLabel = null;
            });
        }
    }
}