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

    using CmisSync.Lib.Cmis;
    using CmisSync.Lib.Config;
    using CmisSync.Notifications;

    public class Controller : ControllerBase {

        public Controller() : base() {
        }

        /// <summary>
        /// Initialize the controller
        /// </summary>
        /// <param name="firstRun">Whether it is the first time that CmisSync is being run.</param>
        public override void Initialize(bool firstRun) {
            this.ProxyAuthReqired += delegate(string reponame) {
                NotificationUtils.NotifyAsync(reponame, Properties_Resources.NetworkProxyLogin);
            };

            this.ShowChangePassword += delegate(string reponame) {
                NotificationUtils.NotifyAsync(reponame, string.Format(Properties_Resources.NotificationCredentialsError, reponame));
            };

            this.ShowException += delegate(string title, string msg) {
                NotificationUtils.NotifyAsync(title, msg);
            };

            this.AlertNotificationRaised += (string title, string message) => {
                NotificationUtils.NotifyAsync(title, message);
            };

            base.Initialize(firstRun);
        }

        // Creates a .desktop entry in autostart folder to
        // start CmisSync automatically at login
        public override void CreateStartupItem() {
            string autostart_path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "autostart");
            string desktopfile_path = Path.Combine(autostart_path, "dataspacesync.desktop");
            if (!Directory.Exists(autostart_path)) {
                Directory.CreateDirectory(autostart_path);
            }

            if (!File.Exists(desktopfile_path)) {
                try {
                    File.WriteAllText(
                        desktopfile_path,
                        "[Desktop Entry]\n" +
                        "Type=Application\n" +
                        "Name=DataSpace Sync\n" +
                        "Exec=dataspacesync start\n" +
                        "Icon=dataspacesync-app\n" +
                        "Terminal=false\n" +
                        "X-GNOME-Autostart-enabled=true\n" +
                        "Categories=Network");
                    Logger.Info("Added DataSpace Sync to login items");
                } catch (Exception e) {
                    Logger.Info("Failed adding DataSpace Sync to login items: " + e.Message);
                }
            }
        }

        /// <summary>
        /// Adds the DataSpace folder to the user's list of bookmarked places
        /// </summary>
        public override void AddToBookmarks() {
            string bookmarks_file_path   = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                ".gtk-bookmarks");

            // newer nautilus version using a different path then older ones
            string bookmarks_file_path_gtk3 = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                ".config",
                "gtk-3.0",
                "bookmarks");

            // if the new path is found, take the new one, otherwise the old one
            if (File.Exists(bookmarks_file_path_gtk3)) {
                bookmarks_file_path = bookmarks_file_path_gtk3;
            }

            string cmissync_bookmark = "file://" + FoldersPath.Replace(" ", "%20");
            if (File.Exists(bookmarks_file_path)) {
                string bookmarks = File.ReadAllText(bookmarks_file_path);
                if (!bookmarks.Contains(cmissync_bookmark)) {
                    File.AppendAllText(bookmarks_file_path, cmissync_bookmark);
                }
            } else {
                File.WriteAllText(bookmarks_file_path, cmissync_bookmark);
            }
        }

        /// <summary>
        /// Creates the DataSpace folder in the user's home folder
        /// </summary>
        /// <returns><c>true</c>, if cmis sync folder was created, <c>false</c> otherwise.</returns>
        public override bool CreateCmisSyncFolder() {
            if (!Directory.Exists(this.FoldersPath)) {
                Directory.CreateDirectory(this.FoldersPath);
                Logger.Info("Created '" + this.FoldersPath + "'");

                string iconName = "dataspacesync-folder.png";
                string iconSrc = Path.Combine(Defines.ASSETS_DIR, "icons", "hicolor", "256x256", "places", iconName);
                string iconDst = Path.Combine(FoldersPath, "." + iconName);
                if (File.Exists(iconSrc)) {
                    File.Copy(iconSrc, iconDst);
                }

                string gvfs_command_path = Path.Combine(
                    Path.VolumeSeparatorChar.ToString(),
                    "usr",
                    "bin",
                    "gvfs-set-attribute");

                // Add a special icon to the CmisSync folder
                if (File.Exists(gvfs_command_path)) {
                    Process process = new Process();
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.FileName = "gvfs-set-attribute";

                    // Clear the custom (legacy) icon path
                    process.StartInfo.Arguments = "-t unset \"" +
                        FoldersPath.Replace(" ", "\\ ") + "\" metadata::custom-icon";

                    process.Start();
                    process.WaitForExit();

                    // Give the CmisSync folder an icon name, so that it scales
                    process.StartInfo.Arguments = FoldersPath.Replace(" ", "\\ ") +
                        " metadata::custom-icon-name 'dataspacesync-folder'";

                    process.Start();
                    process.WaitForExit();

                    if (File.Exists(iconDst)) {
                        process.StartInfo.Arguments = FoldersPath.Replace(" ", "\\ ") +
                            " metadata::custom-icon '.dataspacesync-folder.png'";
                        process.Start();
                        process.WaitForExit();
                    }
                }

                string kde_directory_path = Path.Combine(FoldersPath, ".directory");
                string kde_directory_content = "[Desktop Entry]\nIcon=dataspacesync-folder\n";
                try {
                    File.WriteAllText(kde_directory_path, kde_directory_content);
                } catch (IOException e) {
                    Logger.Info("Config | Failed setting kde icon for '" + this.FoldersPath + "': " + e.Message);
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Opens the default sync target folder.
        /// </summary>
        public void OpenCmisSyncFolder() {
            Utils.OpenFolder(ConfigManager.CurrentConfig.GetFoldersPath());
        }

        /// <summary>
        /// Opens the sync target folder for the repository with the given name.
        /// </summary>
        /// <param name="name">Name of the repository.</param>
        public void OpenCmisSyncFolder(string name) {
            var f = ConfigManager.CurrentConfig.GetRepoInfo(name);
            if (f != null) {
                Utils.OpenFolder(f.LocalPath);
            } else if (string.IsNullOrWhiteSpace(name)) {
                this.OpenCmisSyncFolder();
            } else {
                Logger.Warn("Folder not found: " + name);
            }
        }

        /// <summary>
        /// Opens the given file as log in default console.
        /// </summary>
        /// <param name="path">Absolute path to the log file.</param>
        public void ShowLog(string path) {
            Process process = new Process();
            process.StartInfo.FileName  = "x-terminal-emulator";
            process.StartInfo.Arguments = "-title \"DataSpace Sync Log\" -e tail -f \"" + path + "\"";
            process.Start();
        }
    }
}