//-----------------------------------------------------------------------
// <copyright file="Controller.cs" company="GRAU DATA AG">
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
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using Forms = System.Windows.Forms;

    using CmisSync.Lib.Cmis;
    using CmisSync.Lib.Config;

    using Microsoft.Win32;

    /// <summary>
    /// Windows-specific part of the main CmisSync controller.
    /// </summary>
    public class Controller : ControllerBase {
        /// <summary>
        /// Constructor.
        /// </summary>
        public Controller()
            : base() {
                SystemEvents.PowerModeChanged += delegate(object sender, PowerModeChangedEventArgs args) {
                    switch (args.Mode) {
                        case PowerModes.Suspend:
                            Logger.Debug("Suspend");
                            this.StopAll();
                            Logger.Debug("Suspended");
                        break;
                        case PowerModes.Resume:
                            Logger.Debug("Resume");
                            this.StartAll();
                            Logger.Debug("Resumed");
                        break;
                    }
                };
        }

        /// <summary>
        /// Initialize the controller
        /// </summary>
        /// <param name="firstRun">Whether it is the first time that CmisSync is being run.</param>
        public override void Initialize(Boolean firstRun) {
            base.Initialize(firstRun);
        }


        /// <summary>
        /// Add CmisSync to the list of programs to be started up when the user logs into Windows.
        /// </summary>
        public override void CreateStartupItem() {
            string startup_folder_path = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            string shortcut_path = Path.Combine(startup_folder_path, "DataSpace Sync.lnk");

            if (File.Exists(shortcut_path))
                File.Delete(shortcut_path);

            string shortcut_target = Forms.Application.ExecutablePath;

            Shortcut shortcut = new Shortcut();
            shortcut.Create(shortcut_target, shortcut_path);
        }


        /// <summary>
        /// Add CmisSync to the user's Windows Explorer bookmarks.
        /// </summary>
        public override void AddToBookmarks() {
            string user_profile_path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string shortcut_path = Path.Combine(user_profile_path, "Links", "DataSpace Sync.lnk");

            if (File.Exists(shortcut_path))
                File.Delete(shortcut_path);

            Shortcut shortcut = new Shortcut();

            shortcut.Create(FoldersPath, shortcut_path, Forms.Application.ExecutablePath, 0);
        }


        /// <summary>
        /// Create the user's CmisSync settings folder.
        /// This folder contains databases, the settings file and the log file.
        /// </summary>
        /// <returns></returns>
        public override bool CreateCmisSyncFolder() {
            if (Directory.Exists(FoldersPath)) {
                File.SetAttributes(FoldersPath, File.GetAttributes(FoldersPath) & ~FileAttributes.System);
                return false;
            }

            Directory.CreateDirectory(FoldersPath);
            File.SetAttributes(FoldersPath, File.GetAttributes(FoldersPath) & ~FileAttributes.System);

            Logger.Info("Config | Created '" + FoldersPath + "'");

            string app_path = Path.GetDirectoryName(Forms.Application.ExecutablePath);
            string icon_file_path = Path.Combine(app_path, "Pixmaps", "cmissync-folder.ico");

            if (!File.Exists(icon_file_path)) {
                icon_file_path = Assembly.GetExecutingAssembly().Location;
            }

            string ini_file_path = Path.Combine(FoldersPath, "desktop.ini");

            string ini_file = "[.ShellClassInfo]\r\n" +
                    "IconFile=" + icon_file_path + "\r\n" +
                    "IconIndex=0\r\n" +
                    "InfoTip="+ Properties_Resources.ApplicationName +"\r\n" +
                    "IconResource=" + icon_file_path + ",0\r\n" +
                    "[ViewState]\r\n" +
                    "Mode=\r\n" +
                    "Vid=\r\n" +
                    "FolderType=Generic\r\n";

            try {
                File.WriteAllText(ini_file_path, ini_file);

                File.SetAttributes(ini_file_path,
                    File.GetAttributes(ini_file_path) | FileAttributes.Hidden | FileAttributes.System);
            } catch (IOException e) {
                Logger.Info("Config | Failed setting icon for '" + FoldersPath + "': " + e.Message);
            }

            return true;
        }


        /// <summary>
        /// With Windows Explorer, open the folder where the local synchronized folders are.
        /// </summary>
        public void OpenCmisSyncFolder() {
            Utils.OpenFolder(ConfigManager.CurrentConfig.GetFoldersPath());
        }


        /// <summary>
        /// With Windows Explorer, open the local folder of a CmisSync synchronized folder.
        /// </summary>
        /// <param name="name">Name of the synchronized folder</param>
        public void OpenCmisSyncFolder(string name) {
            RepoInfo folder = ConfigManager.CurrentConfig.GetRepoInfo(name);
            if (folder != null) {
                Utils.OpenFolder(folder.LocalPath);
            } else if (String.IsNullOrWhiteSpace(name)) {
                OpenCmisSyncFolder();
            } else {
                Logger.Warn("Could not find requested config for \"" + name + "\"");
            }
        }

        /// <summary>
        /// Quit CmisSync.
        /// </summary>
        public override void Quit() {
            base.Quit();
        }

        /// <summary>
        /// Open the log file so that the user can check what is going on, and send it to developers.
        /// </summary>
        /// <param name="path">Path to the log file</param>
        public void ShowLog(string path) {
            Process.Start("notepad.exe", path);
        }
    }
}