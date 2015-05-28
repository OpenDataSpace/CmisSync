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
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Collections.Generic;

    using MonoMac.Foundation;
    using MonoMac.AppKit;
    using MonoMac.ObjCRuntime;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Config;
    using CmisSync.Lib.FileTransmission;

    public class Controller : ControllerBase {
        private NSUserNotificationCenter notificationCenter;
        private Dictionary<string, DateTime> transmissionFiles = new Dictionary<string, DateTime>();
        private HashSet<string> startedTransmissions = new HashSet<string>();
        private Object transmissionLock = new object();
        private int notificationInterval = 5;
        private int notificationKeep = 5;
        private readonly string notificationType = "Type";
        private readonly string notificationTypeCredentials = "Credentials";
        private readonly string notificationTypeTransmission = "Transmission";
        private readonly string notificationTypeAlert = "Alert";

        private class ComparerNSUserNotification : IComparer<NSUserNotification> {
            public int Compare(NSUserNotification x, NSUserNotification y) {
                DateTime xDate = x.DeliveryDate;
                DateTime yDate = y.DeliveryDate;
                return xDate.CompareTo(yDate);
            }
        }

        public bool IsNotificationTransmission(NSUserNotification notification) {
            if (null != notification.UserInfo && notification.UserInfo.ContainsKey((NSString)notificationType)) {
                return notificationTypeTransmission == (string)(notification.UserInfo[notificationType] as NSString);
            }

            return false;
        }

        private bool IsNotificationCredentials(NSUserNotification notification) {
            if (null != notification.UserInfo && notification.UserInfo.ContainsKey((NSString)notificationType)) {
                return notificationTypeCredentials == (string)(notification.UserInfo[notificationType] as NSString);
            }

            return false;
        }

        private bool IsNotificationAWarning(NSUserNotification notification) {
            if (null != notification.UserInfo && notification.UserInfo.ContainsKey((NSString)notificationType)) {
                return notificationTypeAlert == (string)(notification.UserInfo[notificationType] as NSString);
            }

            return false;
        }

        private void RemoveNotificationCredentials(string reponame) {
            using (var a = new NSAutoreleasePool()) {
                notificationCenter.BeginInvokeOnMainThread(delegate {
                    NSUserNotification[] notifications = notificationCenter.DeliveredNotifications;
                    foreach (NSUserNotification notification in notifications) {
                        if (!IsNotificationCredentials(notification)) {
                            continue;
                        }

                        if (notification.Title == reponame) {
                            notificationCenter.RemoveDeliveredNotification(notification);
                        }
                    }
                });
            }
        }

        private void InsertNotificationCredentials(string reponame) {
            RemoveNotificationCredentials(reponame);
            using (var a = new NSAutoreleasePool()) {
                notificationCenter.BeginInvokeOnMainThread(delegate {
                    NSUserNotification notification = new NSUserNotification();
                    notification.Title = reponame;
                    notification.Subtitle = String.Format(Properties_Resources.NotificationCredentialsError, reponame);
                    notification.InformativeText = Properties_Resources.NotificationChangeCredentials;
                    NSMutableDictionary userInfo = new NSMutableDictionary();
                    userInfo.Add((NSString)notificationType, (NSString)notificationTypeCredentials);
                    notification.UserInfo = userInfo;
                    notification.DeliveryDate = NSDate.Now;
                    notificationCenter.DeliverNotification(notification);
                });
            }
        }

        private void InsertAlertNotification(string title, string msg) {
            using (var a = new NSAutoreleasePool()) {
                notificationCenter.BeginInvokeOnMainThread(delegate {
                    NSUserNotification notification = new NSUserNotification();
                    notification.Title = title;
                    notification.InformativeText = msg;
                    NSMutableDictionary userInfo = new NSMutableDictionary();
                    userInfo.Add((NSString)notificationType, (NSString)notificationTypeAlert);
                    notification.UserInfo = userInfo;
                    notification.DeliveryDate = NSDate.Now;
                    notificationCenter.DeliverNotification(notification);
                });
            }
        }

        public Controller() : base() {
            using (var a = new NSAutoreleasePool()) {
                NSApplication.Init ();
            }

            NSWorkspace.SharedWorkspace.NotificationCenter.AddObserver(
                NSWorkspace.WillSleepNotification,
                delegate {
                    Logger.Info(string.Format("Machine sleep event detected, stop all repositories"));
                    StopAll();
                }
            );

            NSWorkspace.SharedWorkspace.NotificationCenter.AddObserver(
                NSWorkspace.DidWakeNotification,
                delegate {
                    Logger.Info(string.Format("Machine sleep event detected, start all repositories"));
                    StartAll();
                }
            );

            // We get the Default notification Center
            notificationCenter = NSUserNotificationCenter.DefaultUserNotificationCenter;

            notificationCenter.DidDeliverNotification += (s, e) => {
                //Console.WriteLine("Notification Delivered");
            };

            notificationCenter.DidActivateNotification += (s, e) => {
                if (IsNotificationTransmission(e.Notification)) {
                    LocalFolderClicked (Path.GetDirectoryName (e.Notification.InformativeText));
                }

                if (IsNotificationCredentials(e.Notification)) {
                    //RemoveNotificationCredentials(e.Notification.Title);
                    EditRepositoryCredentials(e.Notification.Title);
                }

                if (IsNotificationAWarning(e.Notification)) {
                    OpenCmisSyncFolder(Program.Controller.FoldersPath);
                }
            };

            // If we return true here, Notification will show up even if your app is TopMost.
            notificationCenter.ShouldPresentNotification = (c, n) => {
                return IsNotificationAWarning(n);
            };

            ShowChangePassword += delegate(string reponame) {
                InsertNotificationCredentials(reponame);
            };

            SuccessfulLogin += delegate(string reponame) {
                RemoveNotificationCredentials(reponame);
            };

            AlertNotificationRaised += (title, message) => {
                InsertAlertNotification(title, message);
            };

            OnTransmissionListChanged += delegate {
                var count = this.ActiveTransmissions().Count;
                /*using (var a = new NSAutoreleasePool()) {
                    notificationCenter.BeginInvokeOnMainThread(delegate {
                    NSApplication.SharedApplication.DockTile.ShowsApplicationBadge = count > 0;
                    NSApplication.SharedApplication.DockTile.BadgeLabel = count > 0 ? count.ToString() : null;
                    });
                }*/

                if (!ConfigManager.CurrentConfig.Notifications) {
                    return;
                }

                using (var a = new NSAutoreleasePool()) {
                    notificationCenter.BeginInvokeOnMainThread(delegate {
                        lock (transmissionLock) {
                            var transmissions = ActiveTransmissions();
                            NSUserNotification[] notifications = notificationCenter.DeliveredNotifications;
                            List<NSUserNotification> finishedNotifications = new List<NSUserNotification>();
                            foreach (NSUserNotification notification in notifications) {
                                if (!IsNotificationTransmission(notification)) {
                                    continue;
                                }

                                var transmission = transmissions.Find((Transmission e) => { return e.Path == notification.InformativeText; });
                                if (transmission == null) {
                                    finishedNotifications.Add(notification);
                                } else {
                                    if (transmissionFiles.ContainsKey(transmission.Path)) {
                                        transmissions.Remove(transmission);
                                    } else {
                                        notificationCenter.RemoveDeliveredNotification(notification);
                                    }
                                }
                            }

                            finishedNotifications.Sort(new ComparerNSUserNotification());
                            for (int i = 0; i < (notifications.Length - notificationKeep) && i < finishedNotifications.Count; ++i) {
                                notificationCenter.RemoveDeliveredNotification (finishedNotifications[i]);
                            }

                            foreach (var transmission in transmissions) {
                                if (transmission.Status == TransmissionStatus.ABORTED) {
                                    continue;
                                }

                                if (startedTransmissions.Contains(transmission.Path)) {
                                    continue;
                                }

                                startedTransmissions.Add(transmission.Path);

                                NSUserNotification notification = new NSUserNotification();
                                notification.Title = Path.GetFileName (transmission.Path);
                                notification.Subtitle = GetTransmissionStatus(transmission);
                                notification.InformativeText = transmission.Path;
                                NSMutableDictionary userInfo = new NSMutableDictionary();
                                userInfo.Add((NSString)notificationType, (NSString)notificationTypeTransmission);
                                notification.UserInfo = userInfo;
                                notification.DeliveryDate = NSDate.Now;
                                notificationCenter.DeliverNotification (notification);

                                transmissionFiles.Add(transmission.Path, NSDate.Now);
                                transmission.PropertyChanged += TransmissionReport;
                            }
                        }
                    });
                }
            };
        }

        private string GetTransmissionStatus(Transmission transmission) {
            string type = "Unknown";
            switch (transmission.Type) {
            case TransmissionType.UPLOAD_NEW_FILE:
                type = Properties_Resources.NotificationFileUpload;
                break;
            case TransmissionType.UPLOAD_MODIFIED_FILE:
                type = Properties_Resources.NotificationFileUpdateRemote;
                break;
            case TransmissionType.DOWNLOAD_NEW_FILE:
                type = Properties_Resources.NotificationFileDownload;
                break;
            case TransmissionType.DOWNLOAD_MODIFIED_FILE:
                type = Properties_Resources.NotificationFileUpdateLocal;
                break;
            }

            string status = string.Empty;
            switch (transmission.Status) {
                case TransmissionStatus.ABORTED:
                    status = transmission.FailedException == null ? Properties_Resources.NotificationFileStatusAborted : Properties_Resources.NotificationFileStatusFailed;
                    break;
                case TransmissionStatus.FINISHED:
                    status = Properties_Resources.NotificationFileStatusCompleted;
                    break;
            }

            return String.Format("{0} {1}",
                type, status);
        }

        private void TransmissionReport(object sender, PropertyChangedEventArgs e) {
            using (var a = new NSAutoreleasePool()) {
                var transmission = sender as Transmission;
                if (transmission == null) {
                    return;
                }

                lock (transmissionLock) {
                    if (transmission.Done) {
                        transmission.PropertyChanged -= TransmissionReport;
                        transmissionFiles.Remove(transmission.Path);
                    } else {
                        TimeSpan diff = NSDate.Now - transmissionFiles[transmission.Path];
                        if (diff.Seconds < notificationInterval) {
                            return;
                        }

                        transmissionFiles[transmission.Path] = NSDate.Now;
                    }
                }

                notificationCenter.BeginInvokeOnMainThread(delegate {
                    lock (transmissionLock) {
                        NSUserNotification[] notifications = notificationCenter.DeliveredNotifications;
                        foreach (NSUserNotification notification in notifications) {
                            if (!IsNotificationTransmission(notification)) {
                                continue;
                            }

                            bool pathCorrect = notification.InformativeText == transmission.Path;
                            bool isCompleted = transmission.Status == TransmissionStatus.FINISHED;
                            bool isAlreadyStarted = startedTransmissions.Contains(transmission.Path);
                            if (pathCorrect && (!isAlreadyStarted || isCompleted)) {
                                notificationCenter.RemoveDeliveredNotification(notification);
                                notification.DeliveryDate = NSDate.Now;
                                notification.Subtitle = GetTransmissionStatus(transmission);
                                notificationCenter.DeliverNotification(notification);
                                return;
                            }
                        }
                    }
                });
            }
        }

        public override void CreateStartupItem() {
            // There aren't any bindings in MonoMac to support this yet, so
            // we call out to an applescript to do the job
            Process process = new Process();
            process.StartInfo.FileName               = "osascript";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute        = false;

            process.StartInfo.Arguments = "-e 'tell application \"System Events\" to " +
                "make login item at end with properties {path:\"" + NSBundle.MainBundle.BundlePath + "\", hidden:false}'";

            process.Start();
            process.WaitForExit();
        }

        // Adds the DataSpace folder to the user's
        // list of bookmarked places
        public override void AddToBookmarks() {
            /*
            NSMutableDictionary sidebar_plist = NSMutableDictionary.FromDictionary(
                NSUserDefaults.StandardUserDefaults.PersistentDomainForName("com.apple.sidebarlists"));
            // Go through the sidebar categories
            foreach (NSString sidebar_category in sidebar_plist.Keys) {
                // Find the favorites
                if (sidebar_category.ToString().Equals("favoriteitems")) {
                    // Get the favorites
                    NSMutableDictionary favorites = NSMutableDictionary.FromDictionary(
                        (NSDictionary)sidebar_plist.ValueForKey(sidebar_category));
                    // Go through the favorites
                    foreach (NSString favorite in favorites.Keys) {
                        // Find the custom favorites
                        if (favorite.ToString().Equals("CustomListItems")) {
                            // Get the custom favorites
                            NSMutableArray custom_favorites = (NSMutableArray)favorites.ValueForKey(favorite);
                            NSMutableDictionary new_favorite = new NSMutableDictionary();
                            new_favorite.SetValueForKey(new NSString("DataSpace"), new NSString("Name"));
                            Console.WriteLine(Program.Controller.FoldersPath);
                            using (NSUrl origUrl = new NSUrl(Program.Controller.FoldersPath, true)) {
                                NSError error;
                                NSData bookmarkdata = origUrl.CreateBookmarkData(NSUrlBookmarkCreationOptions.SuitableForBookmarkFile, new string[0], null, out error);
                                if (error == null && bookmarkdata != null) {
                                    new_favorite.SetValueForKey(bookmarkdata, new NSString("Alias"));

                                    // Add to the favorites
                                    custom_favorites.Add(new_favorite);
                                    favorites.SetValueForKey((NSArray)custom_favorites, new NSString(favorite.ToString()));
                                    sidebar_plist.SetValueForKey(favorites, new NSString(sidebar_category.ToString()));
                                }
                            }
                            break;
                        }
                    }

                    break;
                }
            }

            NSUserDefaults.StandardUserDefaults.SetPersistentDomain(sidebar_plist, "com.apple.sidebarlists");*/
        }

        public override bool CreateCmisSyncFolder() {
            if (!Directory.Exists(Program.Controller.FoldersPath)) {
                Directory.CreateDirectory(Program.Controller.FoldersPath);
                return true;
            } else {
                return false;
            }
        }

        public void OpenCmisSyncFolder(string reponame) {
            foreach (var repo in Program.Controller.Repositories) {
                if (repo.Name.Equals(reponame)) {
                    LocalFolderClicked(repo.LocalPath);
                    break;
                }
            }
        }

        public void ShowLog(string str) {
            System.Diagnostics.Process.Start("/usr/bin/open", "-a Console " + str);
        }

        public void LocalFolderClicked(string path) {
            notificationCenter.BeginInvokeOnMainThread(delegate {
                NSWorkspace.SharedWorkspace.OpenFile (path);
            });
        }

        public void OpenFile(string path) {
            path = Uri.UnescapeDataString(path);
            NSWorkspace.SharedWorkspace.BeginInvokeOnMainThread(delegate {
                NSWorkspace.SharedWorkspace.OpenFile (path);
            });
        }
    }
}