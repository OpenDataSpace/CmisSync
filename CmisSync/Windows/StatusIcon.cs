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
        /// Menu item that shows the state of CmisSync (up-to-date, etc).
        /// </summary>
        private ToolStripMenuItem stateItem;
        /*
        /// <summary>
        /// Menu item that shows the state of active transmissions (like up/downloads)
        /// </summary>
        private ToolStripMenuItem transmissionItem;*/

        /// <summary>
        /// Menu item that allows the user to exit CmisSync.
        /// </summary>
        private ToolStripMenuItem exitItem;

        /// <summary>
        /// Constructor.
        /// </summary>
        public StatusIcon() {
            // Create the menu.
            CreateAnimationFrames();
            CreateMenu();

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
                this.trayicon.ShowBalloonTip(
                    30000,
                    title,
                    message,
                    ToolTipIcon.Warning);
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
                            this.trayicon.Icon = animationFrames[icon_frame];
                            return;
                        }

                        if (Controller.Warning) {
                            this.trayicon.Icon = SystemIcons.Warning;
                            return;
                        }

                        this.trayicon.Icon = animationFrames[icon_frame];
                        return;
                    });
                }
            };

            // Status item.
            Controller.UpdateStatusItemEvent += delegate(string state_text) {
                if (IsHandleCreated) {
                    BeginInvoke((Action)delegate {
                        this.stateItem.Text = state_text;
                        this.trayicon.Text = String.Format("{0}\n{1}", Properties_Resources.ApplicationName, state_text);
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

            // Repo Submenu.
            Controller.UpdateSuspendSyncFolderEvent += delegate(string reponame) {
                if (IsHandleCreated) {
                    BeginInvoke((Action)delegate {
                        ToolStripMenuItem repoitem = (ToolStripMenuItem)this.traymenu.Items["tsmi" + reponame];
                        ToolStripMenuItem syncitem = (ToolStripMenuItem)repoitem.DropDownItems[2];
                        foreach (Repository aRepo in Program.Controller.Repositories) {
                            if (aRepo.Name == reponame) {
                                setSyncItemState(syncitem, aRepo.Status);
                                break;
                            }
                        }
                    });
                }
            };

            //Transmission Submenu.
            Controller.UpdateTransmissionMenuEvent += delegate {
                if (IsHandleCreated) {
                    BeginInvoke((Action)delegate {
                        this.stateItem.DropDownItems.Clear();
                        if (ConfigManager.CurrentConfig.Notifications) {
                            List<FileTransmissionEvent> transmissions = Program.Controller.ActiveTransmissions();
                            foreach (FileTransmissionEvent transmission in transmissions) {
                                ToolStripMenuItem transmission_sub_menu_item = new TransmissionMenuItem(transmission, this);
                                this.stateItem.DropDownItems.Add(transmission_sub_menu_item);
                            }

                            if (transmissions.Count > 0) {
                                this.stateItem.Enabled = true;
                            } else {
                                this.stateItem.Enabled = false;
                            }
                        } else {
                            this.stateItem.Enabled = false;
                        }
                    });
                }
            };
        }

        private void setSyncItemState(ToolStripMenuItem syncitem, SyncStatus status) {
            switch (status) {
                case SyncStatus.Idle:
                    syncitem.Text = CmisSync.Properties_Resources.PauseSync;
                    syncitem.Image = UIHelpers.GetBitmap("media_playback_pause");
                    break;
                case SyncStatus.Suspend:
                    syncitem.Text = CmisSync.Properties_Resources.ResumeSync;
                    syncitem.Image = UIHelpers.GetBitmap("media_playback_start");
                    break;
            }
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

            // Create the state menu item.
            this.stateItem = new ToolStripMenuItem() {
                Text = Controller.StateText,
                Enabled = false
            };
            this.traymenu.Items.Add(stateItem);
            this.trayicon.Text = String.Format("{0}\n{1}", Properties_Resources.ApplicationName, Controller.StateText);
            this.traymenu.Items.Add(new ToolStripSeparator());

            // Create a menu item per synchronized folder.
            if (Controller.Folders.Length > 0) {
                foreach (string folderName in Controller.Folders) {
                    // Main item.
                    ToolStripMenuItem subfolderItem = new ToolStripMenuItem() {
                        Text = folderName,
                        Name = "tsmi" + folderName,
                        Image = UIHelpers.GetBitmap("folder")
                    };

                    // Sub-item: open locally.
                    ToolStripMenuItem openLocalFolderItem = new ToolStripMenuItem() {
                        Text = CmisSync.Properties_Resources.OpenLocalFolder,
                        Image = UIHelpers.GetBitmap("folder")
                    };
                    openLocalFolderItem.Click += OpenLocalFolderDelegate(folderName);

                    // Sub-item: edit ignore folder.
                    ToolStripMenuItem editFolderItem = new ToolStripMenuItem() {
                        Text = CmisSync.Properties_Resources.Settings
                    };
                    editFolderItem.Click += EditFolderDelegate(folderName);
                    
                    // Sub-item: suspend sync.
                    ToolStripMenuItem suspendFolderItem = new ToolStripMenuItem();
                    setSyncItemState(suspendFolderItem, SyncStatus.Idle);
                    foreach (Repository aRepo in Program.Controller.Repositories) {
                        if (aRepo.Name.Equals(folderName)) {
                            setSyncItemState(suspendFolderItem, aRepo.Status);
                            break;
                        }
                    }

                    suspendFolderItem.Click += SuspendSyncFolderDelegate(folderName);

                    // Sub-item: remove folder from sync
                    ToolStripMenuItem removeFolderFromSyncItem = new ToolStripMenuItem() {
                        Text = Properties_Resources.RemoveFolderFromSync,
                        Tag = "remove",
                    };
                    removeFolderFromSyncItem.Click += RemoveFolderFromSyncDelegate(folderName);

                    // Add the sub-items.
                    subfolderItem.DropDownItems.Add(openLocalFolderItem);
                    //subfolderItem.DropDownItems.Add(openRemoteFolderItem);
                    subfolderItem.DropDownItems.Add(new ToolStripSeparator());
                    subfolderItem.DropDownItems.Add(suspendFolderItem);
                    subfolderItem.DropDownItems.Add(new ToolStripSeparator());
                    subfolderItem.DropDownItems.Add(editFolderItem);
                    subfolderItem.DropDownItems.Add(new ToolStripSeparator());
                    subfolderItem.DropDownItems.Add(removeFolderFromSyncItem);
                    // Add the main item.
                    this.traymenu.Items.Add(subfolderItem);
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
                UIHelpers.GetIcon ("process-syncing-i"),
                UIHelpers.GetIcon ("process-syncing-ii"),
                UIHelpers.GetIcon ("process-syncing-iii"),
                UIHelpers.GetIcon ("process-syncing-iiii"),
                UIHelpers.GetIcon ("process-syncing-iiiii")
            };
        }

        /// <summary>
        /// Delegate for opening the local folder.
        /// </summary>
        private EventHandler OpenLocalFolderDelegate(string reponame) {
            return delegate {
                Controller.LocalFolderClicked(reponame);
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

        /// <summary>
        /// Delegate for suspending sync.
        /// </summary>
        private EventHandler SuspendSyncFolderDelegate(string reponame) {
            return delegate {
                Controller.SuspendSyncClicked(reponame);
            };
        }

        private EventHandler RemoveFolderFromSyncDelegate(string reponame) {
            return delegate {
                if (System.Windows.MessageBox.Show(
                    CmisSync.Properties_Resources.RemoveSyncQuestion,
                    CmisSync.Properties_Resources.RemoveSyncTitle,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question,
                    MessageBoxResult.No
                    ) == MessageBoxResult.Yes) {
                    Controller.RemoveFolderFromSyncClicked(reponame);
                }
            };
        }

        private EventHandler EditFolderDelegate(string reponame) {
            return delegate {
                Controller.EditFolderClicked(reponame);
            };
        }
    }

    /// <summary>
    /// A specialized helper class for transmission menu items
    /// </summary>
    public class TransmissionMenuItem : ToolStripMenuItem {
        private FileTransmissionType Type { get; set; }
        private string Path { get; set; }
        private Control ParentControl;
        private FileTransmissionEvent transmissionEvent;

        private int updateInterval = 1;
        private DateTime updateTime;

        private object disposeLock = new object();
        private bool disposed = false;

        private string TransmissionStatus(TransmissionProgressEventArgs status) {
            double percent = (status.Percent != null) ? (double)status.Percent : 0;
            long? bitsPerSecond = status.BitsPerSecond;
            if (bitsPerSecond != null) {
                return String.Format("{0} ({1} {2})",
                    System.IO.Path.GetFileName(Path),
                    CmisSync.Lib.Utils.FormatPercent(percent),
                    CmisSync.Lib.Utils.FormatBandwidth((long)bitsPerSecond));
            } else {
                return String.Format("{0} ({1})",
                    System.IO.Path.GetFileName(Path),
                    CmisSync.Lib.Utils.FormatPercent(percent));
            }
        }

        /// <summary>
        /// Creates a new menu item, which updates itself on transmission events
        /// </summary>
        /// <param name="e">FileTransmissionEvent to listen to</param>
        /// <param name="parent">Parent control to avoid threading issues</param>
        public TransmissionMenuItem(FileTransmissionEvent e, Control parent) : base(e.Type.ToString()) {
            Path = e.Path;
            Type = e.Type;
            ParentControl = parent;
            transmissionEvent = e;
            switch (Type) {
                case FileTransmissionType.DOWNLOAD_NEW_FILE:
                    Image = UIHelpers.GetBitmap("Downloading");
                    break;
                case FileTransmissionType.UPLOAD_NEW_FILE:
                    Image = UIHelpers.GetBitmap("Uploading");
                    break;
                case FileTransmissionType.DOWNLOAD_MODIFIED_FILE:
                    goto case FileTransmissionType.UPLOAD_MODIFIED_FILE;
                case FileTransmissionType.UPLOAD_MODIFIED_FILE:
                    Image = UIHelpers.GetBitmap("Updating");
                    break;
            }

            Text = TransmissionStatus(transmissionEvent.Status);
            transmissionEvent.TransmissionStatus += TransmissionEvent;
            Click += TransmissionEventMenuItem_Click;
        }

        private void TransmissionEvent(object sender, TransmissionProgressEventArgs status) {
            lock (disposeLock) {
                if (disposed) {
                    return;
                }

                TimeSpan diff = DateTime.Now - updateTime;
                if (diff.Seconds < updateInterval) {
                    return;
                }

                updateTime = DateTime.Now;

                ParentControl.BeginInvoke((Action)delegate() {
                    lock (disposeLock) {
                        if (disposed) {
                            return;
                        }

                        Text = TransmissionStatus(status);
                    }
                });
            }
        }

        protected override void Dispose(bool disposing) {
            lock (disposeLock) {
                if (!disposed) {
                    transmissionEvent.TransmissionStatus -= TransmissionEvent;
                }

                disposed = true;
            }

            base.Dispose(disposing);
        }

        void TransmissionEventMenuItem_Click(object sender, EventArgs e) {
            Utils.OpenFolder(System.IO.Directory.GetParent(Path).FullName);
        }
    }
}