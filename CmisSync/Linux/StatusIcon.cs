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

    public class StatusIcon {
        public StatusIconController Controller = new StatusIconController();

        private Gdk.Pixbuf[] animationFrames;

        private Menu menu;
        private MenuItem quitItem;
        private MenuItem stateItem;
        private List<RepositoryMenuItem> repoItems;
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

            this.statusIcon.Activate  += this.OpenFolderDelegate(null); // Primary mouse button click shows default folder
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

            this.Controller.UpdateStatusItemEvent += delegate(string state_text) {
                if (!this.isHandleCreated) {
                    return;
                }

                Application.Invoke(delegate {
                    (this.stateItem.Child as Label).Text = state_text;
                    this.stateItem.ShowAll();
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

            this.Controller.UpdateTransmissionMenuEvent += delegate {
                if (!this.isHandleCreated) {
                    return;
                }

                Application.Invoke(delegate {
                    List<FileTransmissionEvent> transmissionEvents = Program.Controller.ActiveTransmissions();
                    if (transmissionEvents.Count != 0) {
                        this.stateItem.Sensitive = true;

                        Menu submenu = new Menu();
                        this.stateItem.Submenu = submenu;

                        foreach (FileTransmissionEvent e in transmissionEvents) {
                            ImageMenuItem transmission_sub_menu_item = new TransmissionMenuItem(e);
                            submenu.Add(transmission_sub_menu_item);
                            this.stateItem.ShowAll();
                        }
                    } else {
                        this.stateItem.Submenu = null;
                        this.stateItem.Sensitive = false;
                    }
                });
            };
        }

        private void SetSyncItemState(ImageMenuItem syncitem, SyncStatus status) {
            switch (status)
            {
            case SyncStatus.Idle:
                (syncitem.Child as Label).Text = CmisSync.Properties_Resources.PauseSync;
                syncitem.Image = new Image(UIHelpers.GetIcon("dataspacesync-pause", 12));
                break;
            case SyncStatus.Suspend:
                (syncitem.Child as Label).Text = CmisSync.Properties_Resources.ResumeSync;
                syncitem.Image = new Image(UIHelpers.GetIcon("dataspacesync-start", 12));
                break;
            }
        }

        public void CreateMenu()
        {
            this.menu = new Menu();
            this.repoItems = new List<RepositoryMenuItem>();
            // State Menu
            this.stateItem = new MenuItem(this.Controller.StateText) {
                Sensitive = false
            };
            this.menu.Add(this.stateItem);
            this.menu.Add(new SeparatorMenuItem());

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
            this.menu.Add(new SeparatorMenuItem());

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
    }

    [CLSCompliant(false)]
    public class CmisSyncMenuItem : ImageMenuItem {
        public CmisSyncMenuItem(string text) : base(text) {
            this.SetProperty("always-show-image", new GLib.Value(true));
        }
    }

    [CLSCompliant(false)]
    public class TransmissionMenuItem : ImageMenuItem {
        private DateTime updateTime;
        private string typeString;

        public TransmissionMenuItem(FileTransmissionEvent e) : base(e.Type.ToString()) {
            this.Path = e.Path;
            this.Type = e.Type;
            this.typeString = this.Type.ToString();
            switch(this.Type) {
            case FileTransmissionType.DOWNLOAD_NEW_FILE:
                this.Image = new Image(UIHelpers.GetIcon("dataspacesync-downloading", 16));
                this.typeString = Properties_Resources.NotificationFileDownload;
                break;
            case FileTransmissionType.UPLOAD_NEW_FILE:
                this.Image = new Image(UIHelpers.GetIcon("dataspacesync-uploading", 16));
                this.typeString = Properties_Resources.NotificationFileUpload;
                break;
            case FileTransmissionType.DOWNLOAD_MODIFIED_FILE:
                this.typeString = Properties_Resources.NotificationFileUpdateLocal;
                this.Image = new Image(UIHelpers.GetIcon("dataspacesync-updating", 16));
                break;
            case FileTransmissionType.UPLOAD_MODIFIED_FILE:
                this.typeString = Properties_Resources.NotificationFileUpdateRemote;
                this.Image = new Image(UIHelpers.GetIcon("dataspacesync-updating", 16));
                break;
            }

            double percent = (e.Status.Percent == null) ? 0 : (double)e.Status.Percent;
            Label text = this.Child as Label;
            if (text != null) {
                text.Text = string.Format("{0}: {1} ({2})", this.typeString, System.IO.Path.GetFileName(this.Path), CmisSync.Lib.Utils.FormatPercent(percent));
            }

            if (ConfigManager.CurrentConfig.Notifications) {
                NotificationUtils.NotifyAsync(string.Format("{0}: {1}", this.typeString, System.IO.Path.GetFileName(this.Path)), this.Path);
            }

            e.TransmissionStatus += delegate(object sender, TransmissionProgressEventArgs status) {
                TimeSpan diff = DateTime.Now - this.updateTime;
                if (diff.Seconds < 1) {
                    return;
                }

                this.updateTime = DateTime.Now;

                percent = (status.Percent != null) ? (double)status.Percent : 0;
                long? bitsPerSecond = status.BitsPerSecond;
                if (status.Percent != null && bitsPerSecond != null && text != null) {
                    Application.Invoke(delegate {
                        text.Text = string.Format(
                            "{0}: {1} ({2} {3})",
                            this.typeString,
                            System.IO.Path.GetFileName(this.Path),
                            CmisSync.Lib.Utils.FormatPercent(percent),
                            CmisSync.Lib.Utils.FormatBandwidth((long)bitsPerSecond));
                    });
                }
            };
            this.Activated += delegate(object sender, EventArgs args) {
                Utils.OpenFolder(System.IO.Directory.GetParent(this.Path).FullName);
            };
            this.Sensitive = true;
        }

        public FileTransmissionType Type { get; private set; }

        public string Path { get; private set; }
    }
}