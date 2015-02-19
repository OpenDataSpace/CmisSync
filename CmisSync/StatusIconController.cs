//-----------------------------------------------------------------------
// <copyright file="StatusIconController.cs" company="GRAU DATA AG">
//
//   Copyright (C) 2010  Hylke Bons <hylkebons@gmail.com>
//   Copyright (C) 2013  GRAUDATA AG <info@graudata.com>
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
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Timers;

    using CmisSync.Lib.Config;

    using Threading = System.Threading;

    /// <summary>
    /// State of the DataSpace Sync status icon.
    /// </summary>
    public enum IconState {
        Idle,
        SyncingUp,
        SyncingDown,
        Syncing,
        Error
    }

    /// <summary>
    /// MVC controller for the DataSpace Sync status icon.
    /// </summary>
    public class StatusIconController {

        // Event handlers.

        public event UpdateIconEventHandler UpdateIconEvent = delegate { };
        public delegate void UpdateIconEventHandler(int icon_frame);

        public event UpdateMenuEventHandler UpdateMenuEvent = delegate { };
        public delegate void UpdateMenuEventHandler(IconState state);

        public event UpdateStatusItemEventHandler UpdateStatusItemEvent = delegate { };
        public delegate void UpdateStatusItemEventHandler(string state_text);

        public event UpdateSuspendSyncFolderEventHandler UpdateSuspendSyncFolderEvent = delegate { };
        public delegate void UpdateSuspendSyncFolderEventHandler(string reponame);

        public event UpdateTransmissionMenuEventHandler UpdateTransmissionMenuEvent = delegate { };
        public delegate void UpdateTransmissionMenuEventHandler();

        /// <summary>
        /// Current state of the DataSpace Sync tray icon.
        /// </summary>
        public IconState CurrentState = IconState.Idle;

        /// <summary>
        /// Warn some anormal cases
        /// </summary>
        public bool Warning {
            get {
                return this.warning;
            }

            set {
                this.warning = value;
                if (this.CurrentState == IconState.Idle) {
                    this.UpdateIconEvent(0);
                }
            }
        }

        private bool warning;

        /// <summary>
        /// Short text shown at the top of the menu of the DataSpace Sync tray icon.
        /// </summary>
        public string StateText = string.Format(Properties_Resources.Welcome, Properties_Resources.ApplicationName);

        /// <summary>
        /// Maximum number of remote folders in the menu before the overflow menu appears.
        /// </summary>
        public readonly int MenuOverflowThreshold = 9;

        /// <summary>
        /// Minimum number of remote folders to populate the overflow menu.
        /// </summary>
        public readonly int MinSubmenuOverflowCount = 3;

        /// <summary>
        /// The list of remote folders to show in the DataSpace Sync tray menu.
        /// </summary>
        public string[] Folders {
            get {
                int overflow_count = Program.Controller.Folders.Count - this.MenuOverflowThreshold;

                if (overflow_count >= this.MinSubmenuOverflowCount) {
                    return Program.Controller.Folders.GetRange(0, this.MenuOverflowThreshold).ToArray();
                } else {
                    return Program.Controller.Folders.ToArray();
                }
            }
        }

        /// <summary>
        /// The list of remote folders to show in the DataSpace Sync tray's overflow menu.
        /// </summary>
        public string[] OverflowFolders {
            get {
                int overflow_count = Program.Controller.Folders.Count - this.MenuOverflowThreshold;

                if (overflow_count >= this.MinSubmenuOverflowCount) {
                    return Program.Controller.Folders.GetRange(this.MenuOverflowThreshold, overflow_count).ToArray();
                } else {
                    return new string[0];
                }
            }
        }

        /// <summary>
        /// Timer for the animation that appears when downloading/uploading a file.
        /// </summary>
        private Timer animation;

        /// <summary>
        /// Current frame of the animation being shown.
        /// First frame is the still icon.
        /// </summary>
        private int animation_frame_number;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public StatusIconController() {
            this.InitAnimation();

            // A remote folder has been added.
            Program.Controller.FolderListChanged += delegate {
                if (this.CurrentState != IconState.Error) {
                    this.CurrentState = IconState.Idle;

                    if (Program.Controller.Folders.Count == 0) {
                        this.StateText = string.Format(Properties_Resources.Welcome, Properties_Resources.ApplicationName);
                    } else {
                        this.StateText = Properties_Resources.FilesUpToDate;
                    }
                }

                this.UpdateStatusItemEvent(this.StateText);
                this.UpdateMenuEvent(this.CurrentState);
            };

            // No more download/upload.
            Program.Controller.OnIdle += delegate {
                if (this.CurrentState != IconState.Error) {
                    this.CurrentState = IconState.Idle;

                    if (Program.Controller.Folders.Count == 0) {
                        this.StateText = string.Format(Properties_Resources.Welcome, Properties_Resources.ApplicationName);
                    } else {
                        this.StateText = Properties_Resources.FilesUpToDate;
                    }
                }

                this.UpdateStatusItemEvent(this.StateText);

                this.animation.Stop();

                this.UpdateIconEvent(0);
            };

            Program.Controller.OnTransmissionListChanged += delegate {
                this.UpdateTransmissionMenuEvent();
            };

            // Syncing.
            Program.Controller.OnSyncing += delegate {
                if (this.CurrentState != IconState.Syncing) {
                    this.CurrentState = IconState.Syncing;
                    this.StateText = Properties_Resources.SyncingChanges;
                    this.UpdateStatusItemEvent(this.StateText);
                    this.animation.Start();
                }
            };
        }

        /// <summary>
        /// With the local file explorer, open the folder where the local synchronized folders are.
        /// </summary>
        public void LocalFolderClicked(string reponame) {
            Program.Controller.OpenCmisSyncFolder(reponame);
        }

        /// <summary>
        /// Open the remote folder addition wizard.
        /// </summary>
        public void AddRemoteFolderClicked() {
            Program.Controller.ShowSetupWindow(PageType.Add1);
        }
        
        /// <summary>
        /// Show the Setting dialog.
        /// </summary>
        public void SettingClicked() {
            Program.Controller.ShowSettingWindow();
        }

        /// <summary>
        /// Show the Transmission window.
        /// </summary>
        public void TransmissionClicked() {
            Program.Controller.ShowTransmissionWindow();
        }

        /// <summary>
        /// Open the DataSpace Sync log with a text file viewer.
        /// </summary>
        public void LogClicked() {
            Program.Controller.ShowLog(ConfigManager.CurrentConfig.GetLogFilePath());
        }

        /// <summary>
        /// Show the About dialog.
        /// </summary>
        public void AboutClicked() {
            Program.Controller.ShowAboutWindow();
        }

        /// <summary>
        /// Quit DataSpace Sync.
        /// </summary>
        public void QuitClicked() {
            Program.Controller.Quit();
        }

        /// <summary>
        /// Suspend synchronization for a particular folder.
        /// </summary>
        public void SuspendSyncClicked(string reponame) {
            Program.Controller.StartOrSuspendRepository(reponame);
            this.UpdateSuspendSyncFolderEvent(reponame);
        }

        /// <summary>
        /// Tries to remove a given repo from sync
        /// </summary>
        /// <param name="reponame"></param>
        public void RemoveFolderFromSyncClicked(string reponame) {
            Program.Controller.RemoveRepositoryFromSync(reponame);
        }

        /// <summary>
        /// Edit a particular folder.
        /// </summary>
        public void EditFolderClicked(string reponame) {
            Program.Controller.EditRepositoryFolder(reponame);
        }

        /// <summary>
        /// Start the tray icon animation.
        /// </summary>
        private void InitAnimation() {
            this.animation_frame_number = 0;

            this.animation = new Timer() {
                Interval = 100
            };

            this.animation.Elapsed += delegate {
                if (this.animation_frame_number < 4) {
                    this.animation_frame_number++;
                } else {
                    this.animation_frame_number = 0;
                }

                this.UpdateIconEvent(this.animation_frame_number);
            };
        }
    }
}