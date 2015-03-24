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
//   CmisSync, a collaboration and sharing tool.
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
    using System.Diagnostics;
    using System.Globalization;

    using CmisSync.Lib.Cmis;
    using CmisSync.Lib.Config;
    using CmisSync.Lib.Events;
    using CmisSync.Notifications;

#if HAVE_APP_INDICATOR
    using AppIndicator;
#endif
    using Gtk;

    using log4net;

    using Mono.Unix;

    public class StatusIcon : IDisposable {
        public StatusIconController Controller = new StatusIconController();

        private Gdk.Pixbuf[] animationFrames;

        private Menu menu;
        private MenuItem quitItem;
        private List<RepositoryMenuItem> repoItems = new List<RepositoryMenuItem>();

        bool disposed = false;

        private bool isHandleCreated = false;

#if HAVE_APP_INDICATOR
        private ApplicationIndicator indicator;
#else
        private Gtk.StatusIcon statusIcon;
#endif

        public StatusIcon() {
            this.CreateAnimationFrames();

#if HAVE_APP_INDICATOR
            this.indicator = new ApplicationIndicator ("dataspacesync",
                    "dataspacesync-process-syncing-i", Category.ApplicationStatus);

            this.indicator.Status = Status.Active;
#else
            this.statusIcon        = new Gtk.StatusIcon();
            this.statusIcon.Pixbuf = this.animationFrames[0];

            this.statusIcon.Activate  += delegate(object sender, EventArgs args) {
                this.Controller.LocalFolderClicked(null); // Primary mouse button click shows default folder
            };

            this.statusIcon.PopupMenu += this.ShowMenu; // Secondary mouse button click
#endif

            this.CreateMenu();

            this.Controller.UpdateIconEvent += delegate(int icon_frame) {
                Application.Invoke(delegate {
                        if (icon_frame > -1) {
#if HAVE_APP_INDICATOR
                        string icon_name = "dataspacesync-process-syncing-";
                        for (int i = 0; i <= icon_frame; i++)
                        icon_name += "i";

                        this.indicator.IconName = icon_name;

                        // Force update of the icon
                        this.indicator.Status = Status.Attention;
                        this.indicator.Status = Status.Active;
#else
                        this.statusIcon.Pixbuf = this.animationFrames[icon_frame];
#endif
                        } else {
#if HAVE_APP_INDICATOR
                        this.indicator.IconName = "dataspacesync-process-syncing-error";

                        // Force update of the icon
                        this.indicator.Status = Status.Attention;
                        this.indicator.Status = Status.Active;
#else
                        this.statusIcon.Pixbuf = UIHelpers.GetIcon("dataspacesync-process-syncing-error", 24);
#endif
                        }
                });
            };

            this.Controller.UpdateMenuEvent += delegate(IconState state) {
                Application.Invoke(delegate {
                    this.CreateMenu();
                });
            };

            this.Controller.UpdateSuspendSyncFolderEvent += delegate(string reponame) {
                if (!this.isHandleCreated) {
                    return;
                }

                Application.Invoke(delegate {
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
            };
        }

        public void CreateMenu() {
            this.menu = new Menu();
            if (this.repoItems != null) {
                foreach(var repoItem in this.repoItems) {
                    repoItem.Dispose();
                }
            }
            this.repoItems.Clear();

            // Folders Menu
            if (this.Controller.Folders.Length > 0) {
                foreach (var repo in Program.Controller.Repositories) {
                    var repoItem = new RepositoryMenuItem(repo, this.Controller);
                    this.repoItems.Add(repoItem);
                    this.menu.Add(repoItem);
                }

                this.menu.Add(new SeparatorMenuItem());
            }

            // Add Menu
            MenuItem add_item = new MenuItem(CmisSync.Properties_Resources.AddARemoteFolder);
            add_item.Activated += delegate {
                this.Controller.AddRemoteFolderClicked();
            };
            this.menu.Add(add_item);

            this.menu.Add(new SeparatorMenuItem());

            MenuItem settingsItem = new MenuItem(
                Properties_Resources.Settings);
            settingsItem.Activated += delegate {
                this.Controller.SettingClicked();
            };
            this.menu.Add(settingsItem);
            MenuItem transmissionsItem = new MenuItem(
                Properties_Resources.Transmission);
            transmissionsItem.Activated += delegate {
                this.Controller.TransmissionClicked();
            };
            this.menu.Add(transmissionsItem);

            // Log Menu
            MenuItem log_item = new MenuItem(
                    CmisSync.Properties_Resources.ViewLog);
            log_item.Activated += delegate {
                this.Controller.LogClicked();
            };
            this.menu.Add(log_item);

            // About Menu
            MenuItem about_item = new MenuItem(string.Format(CmisSync.Properties_Resources.About, Properties_Resources.ApplicationName));
            about_item.Activated += delegate {
                this.Controller.AboutClicked();
            };
            this.menu.Add(about_item);
            this.menu.Add(new SeparatorMenuItem());
            this.quitItem = new MenuItem(
                    CmisSync.Properties_Resources.Exit) {
                Sensitive = true
            };

            this.quitItem.Activated += delegate {
                this.Controller.QuitClicked();
            };

            this.menu.Add(this.quitItem);
            this.menu.ShowAll();

#if HAVE_APP_INDICATOR
            this.indicator.Menu = this.menu;
#endif
            this.isHandleCreated = true;
        }

        private void CreateAnimationFrames()
        {
            this.animationFrames = new Gdk.Pixbuf[] {
                UIHelpers.GetIcon("dataspacesync-process-syncing-i", 24),
                UIHelpers.GetIcon("dataspacesync-process-syncing-ii", 24),
                UIHelpers.GetIcon("dataspacesync-process-syncing-iii", 24),
                UIHelpers.GetIcon("dataspacesync-process-syncing-iiii", 24),
                UIHelpers.GetIcon("dataspacesync-process-syncing-iiiii", 24)
            };
        }

#if !HAVE_APP_INDICATOR
        // Makes the menu visible
        private void ShowMenu(object o, EventArgs args) {
            this.menu.Popup(null, null, this.SetPosition, 0, Global.CurrentEventTime);
        }

        // Makes sure the menu pops up in the right position
        private void SetPosition(Menu menu, out int x, out int y, out bool push_in) {
            Gtk.StatusIcon.PositionMenu(menu, out x, out y, out push_in, this.statusIcon.Handle);
        }
#endif

        public void Dispose() {
            if (this.disposed) {
                return;
            }

            if (this.repoItems != null) {
                foreach(var repoItem in this.repoItems) {
                    repoItem.Dispose();
                }
            }

            if (this.menu != null) {
                this.menu.Dispose();
            }

            if (this.quitItem != null) {
                this.quitItem.Dispose();
            }

            #if HAVE_APP_INDICATOR
            if (this.indicator != null) {
                this.indicator.Dispose();
            }
            #endif

            this.disposed = true;
        }
    }

    [CLSCompliant(false)]
    public class CmisSyncMenuItem : ImageMenuItem {
        public CmisSyncMenuItem(string text) : base(text) {
            this.SetProperty("always-show-image", new GLib.Value(true));
        }
    }
}