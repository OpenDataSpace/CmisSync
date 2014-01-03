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


using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;

using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.ObjCRuntime;

using CmisSync.Lib;
using CmisSync.Lib.Events;

using log4net;

namespace CmisSync {

	public class Controller : ControllerBase {

        private NSUserNotificationCenter notificationCenter;
        
		public Controller () : base ()
        {
            using (var a = new NSAutoreleasePool ())
            {
                NSApplication.Init ();
            }

            // We get the Default notification Center
            notificationCenter = NSUserNotificationCenter.DefaultUserNotificationCenter;

            notificationCenter.DidDeliverNotification += (s, e) => 
            {
                Console.WriteLine("Notification Delivered");
            };

            notificationCenter.DidActivateNotification += (s, e) => 
            {
                Console.WriteLine("Notification Touched");
            };

            // If we return true here, Notification will show up even if your app is TopMost.
            notificationCenter.ShouldPresentNotification = (c, n) => { return true; };

            OnTransmissionListChanged += delegate {

                using (var a = new NSAutoreleasePool()) {
                    notificationCenter.InvokeOnMainThread(delegate {
                        List<FileTransmissionEvent> transmissions = ActiveTransmissions();
                        NSUserNotification[] notifications = notificationCenter.DeliveredNotifications;
                        foreach (NSUserNotification notification in notifications) {
                            FileTransmissionEvent transmission = transmissions.Find( (FileTransmissionEvent e)=>{return (e.Path == notification.InformativeText);});
                            if (transmission == null) {
                                notificationCenter.RemoveDeliveredNotification(notification);
                            } else {
                                transmissions.Remove(transmission);
                            }
                        }
                        foreach (FileTransmissionEvent transmission in transmissions) {
                            NSUserNotification notification = new NSUserNotification();
                            notification.Title = Path.GetFileName (transmission.Path);
                            string type = "Unknown";
                            switch (transmission.Type) {
                            case FileTransmissionType.UPLOAD_NEW_FILE:
                                type = "Upload new file";
                                break;
                            case FileTransmissionType.UPLOAD_MODIFIED_FILE:
                                type = "Update remote file";
                                break;
                            case FileTransmissionType.DOWNLOAD_NEW_FILE:
                                type = "Download new file";
                                break;
                            case FileTransmissionType.DOWNLOAD_MODIFIED_FILE:
                                type = "Update local file";
                                break;
                            }
                            notification.Subtitle = TransmissionStatus(transmission);
                            notification.InformativeText = transmission.Path;
                            notification.SoundName = NSUserNotification.NSUserNotificationDefaultSoundName;
                            transmission.TransmissionStatus += TransmissionReport;
                            notification.DeliveryDate = NSDate.Now;
                            notificationCenter.DeliverNotification (notification);
                        }
                    });
                }
            };
		}

        private string TransmissionStatus(FileTransmissionEvent transmission)
        {
            string type = "Unknown";
            switch (transmission.Type) {
            case FileTransmissionType.UPLOAD_NEW_FILE:
                type = "Upload new file";
                break;
            case FileTransmissionType.UPLOAD_MODIFIED_FILE:
                type = "Update remote file";
                break;
            case FileTransmissionType.DOWNLOAD_NEW_FILE:
                type = "Download new file";
                break;
            case FileTransmissionType.DOWNLOAD_MODIFIED_FILE:
                type = "Update local file";
                break;
            }
            return String.Format("{0} ({1:###.#}% {2})",
                type,
                Math.Round (transmission.Status.Percent.GetValueOrDefault(), 1),
                CmisSync.Lib.Utils.FormatBandwidth ((long)transmission.Status.BitsPerSecond.GetValueOrDefault()));
        }

        private void TransmissionReport(object sender, TransmissionProgressEventArgs e)
        {
            FileTransmissionEvent transmission = sender as FileTransmissionEvent;
            if (transmission != null) {
                if ((e.Aborted == true || e.Completed == true || e.FailedException != null)) {
                    transmission.TransmissionStatus -= TransmissionReport;
                }
                NSUserNotification[] notifications = notificationCenter.DeliveredNotifications;
                foreach (NSUserNotification notification in notifications) {
                    if (notification.InformativeText == transmission.Path) {
                        TimeSpan diff = NSDate.Now - (DateTime)notification.DeliveryDate;
                        if (diff.Seconds < 1) {
                            return;
                        }
                        notificationCenter.RemoveDeliveredNotification (notification);
                        notification.DeliveryDate = NSDate.Now;
                        notification.Subtitle = TransmissionStatus (transmission);
                        notificationCenter.DeliverNotification (notification);
                        return;
                    }
                }
            }
        }

		public override void CreateStartupItem ()
		{
            // There aren't any bindings in MonoMac to support this yet, so
            // we call out to an applescript to do the job
            Process process = new Process ();
            process.StartInfo.FileName               = "osascript";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute        = false;

            process.StartInfo.Arguments = "-e 'tell application \"System Events\" to " +
                "make login item at end with properties {path:\"" + NSBundle.MainBundle.BundlePath + "\", hidden:false}'";

            process.Start ();
            process.WaitForExit ();
		}

		// Adds the CmisSync folder to the user's
		// list of bookmarked places
		public override void AddToBookmarks ()
        {
			/*
            NSMutableDictionary sidebar_plist = NSMutableDictionary.FromDictionary (
                NSUserDefaults.StandardUserDefaults.PersistentDomainForName ("com.apple.sidebarlists"));

            // Go through the sidebar categories
            foreach (NSString sidebar_category in sidebar_plist.Keys) {

                // Find the favorites
                if (sidebar_category.ToString ().Equals ("favorites")) {

                    // Get the favorites
                    NSMutableDictionary favorites = NSMutableDictionary.FromDictionary(
                        (NSDictionary) sidebar_plist.ValueForKey (sidebar_category));

                    // Go through the favorites
                    foreach (NSString favorite in favorites.Keys) {

                        // Find the custom favorites
                        if (favorite.ToString ().Equals ("VolumesList")) {

                            // Get the custom favorites
                            NSMutableArray custom_favorites = (NSMutableArray) favorites.ValueForKey (favorite);

                            NSMutableDictionary properties = new NSMutableDictionary ();
                            properties.SetValueForKey (new NSString ("1935819892"), new NSString ("com.apple.LSSharedFileList.TemplateSystemSelector"));

                            NSMutableDictionary new_favorite = new NSMutableDictionary ();
							new_favorite.SetValueForKey (new NSString ("DataSpace Sync"),  new NSString ("Name"));

                            new_favorite.SetValueForKey (NSData.FromString ("ImgR SYSL fldr"),  new NSString ("Icon"));

							new_favorite.SetValueForKey (NSData.FromString (ConfigManager.CurrentConfig.FoldersPath),
                                new NSString ("Alias"));

                            new_favorite.SetValueForKey (properties, new NSString ("CustomItemProperties"));

                            // Add to the favorites
                            custom_favorites.Add (new_favorite);
                            favorites.SetValueForKey ((NSArray) custom_favorites, new NSString (favorite.ToString ()));
                            sidebar_plist.SetValueForKey (favorites, new NSString (sidebar_category.ToString ()));
                        }
                    }

                }
            }

            NSUserDefaults.StandardUserDefaults.SetPersistentDomain (sidebar_plist, "com.apple.sidebarlists");
            */
		}


		public override bool CreateCmisSyncFolder ()
		{

            if (!Directory.Exists (Program.Controller.FoldersPath)) {
                Directory.CreateDirectory (Program.Controller.FoldersPath);
                return true;

            } else {
                return false;
            }
		}

		public void OpenCmisSyncFolder (string reponame)
		{
			foreach(CmisSync.Lib.RepoBase repo in Program.Controller.Repositories)
			{
				if(repo.Name.Equals(reponame))
				{
					LocalFolderClicked(repo.LocalPath);
					break;
				}
			}
		}

		public void ShowLog (string str)
		{
			System.Diagnostics.Process.Start("/usr/bin/open", "-a Console " + str);
		}

		public void LocalFolderClicked (string path)
		{
			NSWorkspace.SharedWorkspace.OpenFile (path);
		}
		

        public void OpenFile (string path)
        {
            path = Uri.UnescapeDataString (path);
            NSWorkspace.SharedWorkspace.OpenFile (path);
        }
	}
}
