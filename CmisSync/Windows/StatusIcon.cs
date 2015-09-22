﻿//-----------------------------------------------------------------------
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

namespace CmisSync {
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Globalization;
    using System.Text;
    using System.Windows;
    using System.Windows.Forms;

    using CmisSync.Lib.Cmis;
    using CmisSync.Lib.Config;
    using CmisSync.Lib.Events;

    /// <summary>
    /// CmisSync icon in the Windows status bar.
    /// </summary>
    public class StatusIcon : Form {
        /// <summary>
        /// MVC controller for the the status icon.
        /// </summary>
        public StatusIconController Controller = new StatusIconController();

        /// <summary>
        /// Context menu that appears when right-clicking on the CmisSync icon.
        /// </summary>
        private ContextMenuStrip traymenu = new ContextMenuStrip();

        /// <summary>
        /// Windows object for the status icon.
        /// </summary>
        private NotifyIcon trayicon = new NotifyIcon();

        /// <summary>
        /// Frames of the animation used when a download/upload is going on.
        /// The first frame is the static frame used when no activity is going on.
        /// </summary>
        private Icon[] animationFrames;

        /// <summary>
        /// Menu item that allows the user to exit CmisSync.
        /// </summary>
        private ToolStripMenuItem exitItem;

        private List<RepositoryMenuItem> repoItems;

        /// <summary>
        /// Constructor.
        /// </summary>
        public StatusIcon() {
            // Create the menu.
            this.CreateAnimationFrames();
            this.CreateMenu();

            // Setup the status icon.
            this.trayicon.Icon = animationFrames[0];
            this.trayicon.Text = Properties_Resources.ApplicationName;
            this.trayicon.ContextMenuStrip = this.traymenu;
            this.trayicon.Visible = true;
            this.trayicon.MouseClick += NotifyIcon1_MouseClick;

            Program.Controller.ShowChangePassword += delegate(string reponame) {
                lock (repoCreditsErrorListLock) {
                    repoCreditsErrorList.Add(reponame);
                }

                Controller.Warning = true;
                this.trayicon.ShowBalloonTip(
                    30000,
                    String.Format(Properties_Resources.NotificationCredentialsError, reponame),
                    Properties_Resources.NotificationChangeCredentials,
                    ToolTipIcon.Warning);
            };

            Program.Controller.ShowException += delegate(string title, string message) {
                if (!string.IsNullOrEmpty(message)) {
                    BeginInvoke((Action)delegate {
                        this.trayicon.ShowBalloonTip(
                            30000,
                            title,
                            message,
                            ToolTipIcon.Warning);
                    });
                }
            };

            Program.Controller.AlertNotificationRaised += delegate(string title, string message) {
                this.trayicon.ShowBalloonTip(
                    60000,
                    title,
                    message,
                    ToolTipIcon.Error);
                Controller.Warning = true;
            };

            this.trayicon.BalloonTipClicked += trayicon_BalloonTipClicked;
        }

        private HashSet<string> repoCreditsErrorList = new HashSet<string>();

        private Object repoCreditsErrorListLock = new Object();

        private void trayicon_BalloonTipClicked(object sender, EventArgs e) {
            lock (repoCreditsErrorListLock) {
                while (repoCreditsErrorList.Count > 0) {
                    HashSet<string>.Enumerator i = repoCreditsErrorList.GetEnumerator();
                    if (i.MoveNext()) {
                        string reponame = i.Current;
                        repoCreditsErrorList.Remove(reponame);
                        Program.Controller.EditRepositoryCredentials(reponame);
                    } else {
                        break;
                    }
                }
            }

            Controller.Warning = false;
        }


        /// <summary>
        /// When form is loaded, 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e) {
            // Set up the controller to create menu elements on update.
            CreateInvokeMethods();

            Visible = false; // Hide form window.
            ShowInTaskbar = false; // Remove from taskbar.
            base.OnLoad(e);
        }


        /// <summary>
        /// Set up the controller to create menu elements on update.
        /// </summary>
        private void CreateInvokeMethods() {
            // Icon.
            Controller.UpdateIconEvent += delegate(int icon_frame) {
                if (IsHandleCreated) {
                    BeginInvoke((Action)delegate {
                        if (icon_frame < 0) {
                            this.trayicon.Icon = SystemIcons.Error;
                            return;
                        }

                        if (icon_frame > 0) {
                            this.trayicon.Icon = this.animationFrames[icon_frame < this.animationFrames.Length ? icon_frame : this.animationFrames.Length - 1];
                            return;
                        }

                        if (Controller.Warning) {
                            this.trayicon.Icon = SystemIcons.Warning;
                            return;
                        }

                        this.trayicon.Icon = this.animationFrames[icon_frame];
                        return;
                    });
                }
            };
            
            // Menu.
            Controller.UpdateMenuEvent += delegate(IconState state) {
                if (IsHandleCreated) {
                    BeginInvoke((Action)delegate {
                        CreateMenu();
                    });
                }
            };

            Controller.UpdateStatusItemEvent += delegate(string statusText) {
                if (IsHandleCreated) {
                    BeginInvoke((Action)delegate {
                        this.trayicon.Text = statusText;
                    });
                }
            };

            // Repo Submenu.
            Controller.UpdateSuspendSyncFolderEvent += delegate(string reponame) {
                if (IsHandleCreated) {
                    BeginInvoke((Action)delegate {
                        foreach (var repo in Program.Controller.Repositories) {
                            if (repo.Name == reponame) {
                                foreach (var item in this.repoItems) {
                                    if (item.RepositoryName == reponame) {
                                        item.Status = repo.Status;
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

        /// <summary>
        /// Dispose of the status icon UI elements.
        /// </summary>
        protected override void Dispose(bool isDisposing) {
            if (isDisposing) {
                // Release the icon resource.
                this.trayicon.Dispose();
            }

            base.Dispose(isDisposing);
        }

        /// <summary>
        /// Create the UI elements of the menu.
        /// </summary>
        private void CreateMenu() {
            // Reset existing items.
            this.traymenu.Items.Clear();
            this.repoItems = new List<RepositoryMenuItem>();

            this.trayicon.Text = string.Format("{0}\n{1}", Properties_Resources.ApplicationName, Controller.StateText);

            // Create a menu item per synchronized folder.
            if (Controller.Folders.Length > 0) {
                foreach (var repo in Program.Controller.Repositories) {
                    var repoItem = new RepositoryMenuItem(repo, this.Controller, this);
                    this.repoItems.Add(repoItem);
                    this.traymenu.Items.Add(repoItem);
                }

                this.traymenu.Items.Add(new ToolStripSeparator());
            }

            // Create the menu item that lets the user add a new synchronized folder.
            ToolStripMenuItem addFolderItem = new ToolStripMenuItem() {
                Text = CmisSync.Properties_Resources.AddARemoteFolder
            };
            addFolderItem.Click += delegate {
                Controller.AddRemoteFolderClicked();
            };
            this.traymenu.Items.Add(addFolderItem);
            this.traymenu.Items.Add(new ToolStripSeparator());

            // Create the menu item that lets the user view setting.
            ToolStripMenuItem setting_item = new ToolStripMenuItem() {
                Text = CmisSync.Properties_Resources.Settings
            };
            setting_item.Click += delegate {
                Controller.SettingClicked();
            };
            this.traymenu.Items.Add(setting_item);

            // Create the menu item that lets the uer view transmission.
            ToolStripMenuItem transmission_item = new ToolStripMenuItem() {
                Text = CmisSync.Properties_Resources.Transmission
            };
            transmission_item.Click += delegate {
                Controller.TransmissionClicked();
            };
            this.traymenu.Items.Add(transmission_item);

            // Create the menu item that lets the user view the log.
            ToolStripMenuItem log_item = new ToolStripMenuItem() {
                Text = CmisSync.Properties_Resources.ViewLog
            };
            log_item.Click += delegate {
                Controller.LogClicked();
            };
            this.traymenu.Items.Add(log_item);

            // Create the About menu.
            ToolStripMenuItem about_item = new ToolStripMenuItem() {
                Text = String.Format(Properties_Resources.About, Properties_Resources.ApplicationName)
            };
            about_item.Click += delegate {
                Controller.AboutClicked();
            };
            this.traymenu.Items.Add(about_item);

            // Create the exit menu.
            this.exitItem = new ToolStripMenuItem() {
                Text = CmisSync.Properties_Resources.Exit
            };
            this.exitItem.Click += delegate {
                this.trayicon.Dispose();
                Controller.QuitClicked();
            };
            this.traymenu.Items.Add(this.exitItem);
        }

        /// <summary>
        /// Create the animation frames from image files.
        /// </summary>
        private void CreateAnimationFrames() {
            this.animationFrames = new Icon[] {
                UIHelpers.GetIcon("process-syncing-i"),
                UIHelpers.GetIcon("process-syncing-ii"),
                UIHelpers.GetIcon("process-syncing-iii"),
                UIHelpers.GetIcon("process-syncing-iiii"),
                UIHelpers.GetIcon("process-syncing-iiiii")
            };
        }

        /// <summary>
        /// MouseEventListener function for opening the local folder.
        /// </summary>
        private void NotifyIcon1_MouseClick(Object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left) {
                trayicon_BalloonTipClicked(sender, e);
                Controller.LocalFolderClicked("");
            }
        }
    }
}