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


using System;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using System.Text;

using Mono.Unix.Native;

using log4net;

using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.ObjCRuntime;

using CmisSync.Lib.Events;
using CmisSync.Lib.Cmis;

namespace CmisSync {

    public class StatusIcon : NSObject {

        private static readonly ILog Logger = LogManager.GetLogger(typeof(ControllerBase));

        public StatusIconController Controller = new StatusIconController ();

        private NSMenu menu;

        private NSStatusItem status_item;
        private NSMenuItem state_item;

        private NSMenuItem add_item;
        private NSMenuItem about_item;
        private NSMenuItem quit_item;
        private NSMenuItem log_item;
        private NSMenuItem general_settings_item;

        private NSImage [] animation_frames;
        private NSImage [] animation_frames_active;
        private NSImage error_image;
        private NSImage error_image_active;
        private NSImage folder_image;
        private NSImage caution_image;
        private NSImage cmissync_image;
        private NSImage pause_image;
        private NSImage resume_image;
        private NSImage download_image;
        private NSImage upload_image;
        private NSImage update_image;

        private Dictionary<String, NSMenuItem> FolderItems;


        public StatusIcon () : base ()
        {
            using (var a = new NSAutoreleasePool ())
            {
                CreateAnimationFrames ();

                this.status_item = NSStatusBar.SystemStatusBar.CreateStatusItem (28);
                this.status_item.HighlightMode = true;
                this.status_item.Image = this.animation_frames [0];

                this.status_item.Image               = this.animation_frames [0];
                this.status_item.Image.Size          = new SizeF (16, 16);
                this.status_item.AlternateImage      = this.animation_frames_active [0];
                this.status_item.AlternateImage.Size = new SizeF (16, 16);

                CreateMenu ();
            }
            

            Controller.UpdateIconEvent += delegate (int icon_frame) {
                using (var a = new NSAutoreleasePool ())
                {
                    BeginInvokeOnMainThread (delegate {
                        if (icon_frame > -1) {
                            this.status_item.Image               = this.animation_frames [icon_frame];
                            this.status_item.Image.Size          = new SizeF (16, 16);
                            this.status_item.AlternateImage      = this.animation_frames_active [icon_frame];
                            this.status_item.AlternateImage.Size = new SizeF (16, 16);

                        } else {
                            this.status_item.Image               = this.error_image;
                            this.status_item.AlternateImage      = this.error_image_active;
                            this.status_item.Image.Size          = new SizeF (16, 16);
                            this.status_item.AlternateImage.Size = new SizeF (16, 16);
                        }
                    });
                }
            };

            Controller.UpdateStatusItemEvent += delegate (string state_text) {
                using (var a = new NSAutoreleasePool ())
                {
                    BeginInvokeOnMainThread (delegate {
                        this.state_item.Title = state_text;
                    });
                }
            };

            Controller.UpdateMenuEvent += delegate {
                using (var a = new NSAutoreleasePool ())
                {
                    InvokeOnMainThread (() => CreateMenu ());
                }
            };

            Controller.UpdateSuspendSyncFolderEvent += delegate(string reponame)
            {
                using (var a = new NSAutoreleasePool()){
                    InvokeOnMainThread(delegate {
                        NSMenuItem PauseItem;
                        if(FolderItems.TryGetValue(reponame,out PauseItem)){
                            setSyncItemState(PauseItem, getSyncStatus(reponame));
                        }
                    });
                }
            };

            // TODO Need to implement this method like the COCOA way to do it
            Controller.UpdateTransmissionMenuEvent += delegate
            {
                //  Put Program.Controller.ActiveTransmissions() before transmit from managed code to native code
                //  https://mantis.dataspace.cc/view.php?id=3781
                List<FileTransmissionEvent> transmissions = Program.Controller.ActiveTransmissions();

                using (var a = new NSAutoreleasePool()) {
                    BeginInvokeOnMainThread(delegate {
                        if(state_item.Submenu!=null){
                            foreach(NSMenuItem item in state_item.Submenu.ItemArray()){
                                item.Dispose();
                            }
                            state_item.Submenu.RemoveAllItems();
                        } else {
                            state_item.Submenu = new NSMenu();
                        }
                        foreach(FileTransmissionEvent transmission in transmissions) {
                            NSMenuItem transmissionItem = new TransmissionMenuItem(transmission);
                            state_item.Submenu.AddItem(transmissionItem);
                        }
                        if(transmissions.Count > 0) {
                            state_item.Enabled = true;
                        }else{
                            state_item.Enabled = false;
                        }
                    });
				}
            };
        }

        NSMenuItem CreateFolderMenuItem(string folder_name)
        {
            NSMenuItem folderitem = new NSMenuItem();
            folderitem.Image = this.folder_image;
            folderitem.Image.Size = new SizeF(16, 16);
            folderitem.Title = folder_name;
            NSMenu foldersubmenu = new NSMenu();
            NSMenuItem openitem = new NSMenuItem();
            openitem.Title = Properties_Resources.OpenLocalFolder;
            openitem.Activated += OpenFolderDelegate(folder_name);
            NSMenuItem pauseitem = new NSMenuItem();
            setSyncItemState(pauseitem, getSyncStatus(folder_name));
            FolderItems.Add(folder_name, pauseitem);
            pauseitem.Activated += PauseFolderDelegate(folder_name);
            NSMenuItem removeitem = new NSMenuItem();
            removeitem.Title = Properties_Resources.RemoveFolderFromSync;
            removeitem.Activated += RemoveFolderDelegate(folder_name);
            NSMenuItem settingsitem = new NSMenuItem();
            settingsitem.Title = Properties_Resources.EditTitle;
            settingsitem.Activated += OpenSettingsDialogDelegate(folder_name);
            foldersubmenu.AddItem(openitem);
            foldersubmenu.AddItem(pauseitem);
            foldersubmenu.AddItem(NSMenuItem.SeparatorItem);
            foldersubmenu.AddItem(settingsitem);
            foldersubmenu.AddItem(NSMenuItem.SeparatorItem);
            foldersubmenu.AddItem(removeitem);
            folderitem.Submenu = foldersubmenu;
            return folderitem;
        }

        private SyncStatus getSyncStatus(string reponame) {
            foreach (var repo in Program.Controller.Repositories)
            {
                if(repo.Name.Equals(reponame)){
                    return repo.Status;
                }
            }
            return SyncStatus.Idle;
        }

        private void setSyncItemState(NSMenuItem item, SyncStatus status) {
            switch (status)
            {
                case SyncStatus.Idle:
                    item.Title = Properties_Resources.PauseSync;
                    item.Image = this.pause_image;
                    break;
                case SyncStatus.Suspend:
                    item.Title = Properties_Resources.ResumeSync;
                    item.Image = this.resume_image;
                    break;
            }
            item.Image.Size = new SizeF(16, 16);
        }

        public void CreateMenu ()
        {
            using (NSAutoreleasePool a = new NSAutoreleasePool ())
            {
                this.menu                  = new NSMenu ();
                this.menu.AutoEnablesItems = false;

                this.FolderItems = new Dictionary<String, NSMenuItem>();

                this.state_item = new NSMenuItem () {
                    Title   = Controller.StateText,
                    Enabled = false
                };

                this.general_settings_item = new NSMenuItem()
                {
                    Title = Properties_Resources.EditTitle
                };

                this.general_settings_item.Activated += delegate
                {
                    Controller.SettingClicked();
                };

                this.log_item = new NSMenuItem () {
                    Title = Properties_Resources.ViewLog
                };

                this.log_item.Activated += delegate
                {
                    Controller.LogClicked();
                };

                this.add_item = new NSMenuItem () {
                    Title   = Properties_Resources.AddARemoteFolder,
                    Enabled = true
                };

                this.add_item.Activated += delegate {
                    Controller.AddRemoteFolderClicked ();
                };

                this.about_item = new NSMenuItem () {
                    Title   = String.Format(Properties_Resources.About, Properties_Resources.ApplicationName),
                    Enabled = true
                };

                this.about_item.Activated += delegate {
                    Controller.AboutClicked ();
                };

                this.quit_item = new NSMenuItem () {
                    Title   = Properties_Resources.Exit,
                    Enabled = true
                };

                this.quit_item.Activated += delegate {
                    Controller.QuitClicked ();
                };

                this.menu.AddItem (this.state_item);
                this.menu.AddItem (NSMenuItem.SeparatorItem);

                if (Controller.Folders.Length > 0) {
                    foreach (string folder_name in Controller.Folders) {
                        this.menu.AddItem(CreateFolderMenuItem(folder_name));
                    };
                    if (Controller.OverflowFolders.Length > 0)
                    {
                        NSMenuItem moreitem = new NSMenuItem();
                        moreitem.Title = "More Folder";
                        NSMenu moreitemsmenu = new NSMenu();
                        foreach (string folder_name in Controller.OverflowFolders) {
                            moreitemsmenu.AddItem(CreateFolderMenuItem(folder_name));
                        };
                        moreitem.Submenu = moreitemsmenu;
                        this.menu.AddItem(moreitem);
                    }
                    this.menu.AddItem (NSMenuItem.SeparatorItem);
                }

                this.menu.AddItem (this.add_item);
                this.menu.AddItem (NSMenuItem.SeparatorItem);
                this.menu.AddItem (this.general_settings_item);
                this.menu.AddItem (this.log_item);
                this.menu.AddItem (this.about_item);
                this.menu.AddItem (NSMenuItem.SeparatorItem);
                this.menu.AddItem (this.quit_item);

                this.menu.Delegate    = new StatusIconMenuDelegate ();
                this.status_item.Menu = this.menu;
            }
        }


        // A method reference that makes sure that opening the
        // event log for each repository works correctly
        private EventHandler OpenFolderDelegate (string name)
        {
            return delegate {
                Controller.LocalFolderClicked (name);
            };
        }

        private EventHandler PauseFolderDelegate ( string name)
        {
            return delegate
            {
                Controller.SuspendSyncClicked(name);
            };
        }

        private EventHandler RemoveFolderDelegate(string name)
        {
            return delegate
            {
                NSAlert alert = NSAlert.WithMessage(Properties_Resources.RemoveSyncQuestion,"No, please continue syncing","Yes, stop syncing",null,"");
                alert.Icon = this.caution_image;
                alert.Window.OrderFrontRegardless();
                int i = alert.RunModal();
                if(i == 0)
                    Controller.RemoveFolderFromSyncClicked(name);
            };
        }

        private EventHandler OpenSettingsDialogDelegate(string name)
        {
            return delegate
            {
                Controller.EditFolderClicked(name);
            };
        }


        private void CreateAnimationFrames ()
        {
            this.animation_frames = new NSImage [] {
                new NSImage (Path.Combine (NSBundle.MainBundle.ResourcePath, "Pixmaps", "process-syncing-i.png")),
                new NSImage (Path.Combine (NSBundle.MainBundle.ResourcePath, "Pixmaps", "process-syncing-ii.png")),
                new NSImage (Path.Combine (NSBundle.MainBundle.ResourcePath, "Pixmaps", "process-syncing-iii.png")),
                new NSImage (Path.Combine (NSBundle.MainBundle.ResourcePath, "Pixmaps", "process-syncing-iiii.png")),
                new NSImage (Path.Combine (NSBundle.MainBundle.ResourcePath, "Pixmaps", "process-syncing-iiiii.png"))
            };

            this.animation_frames_active = new NSImage [] {
                new NSImage (Path.Combine (NSBundle.MainBundle.ResourcePath, "Pixmaps", "process-syncing-i-active.png")),
                new NSImage (Path.Combine (NSBundle.MainBundle.ResourcePath, "Pixmaps", "process-syncing-ii-active.png")),
                new NSImage (Path.Combine (NSBundle.MainBundle.ResourcePath, "Pixmaps", "process-syncing-iii-active.png")),
                new NSImage (Path.Combine (NSBundle.MainBundle.ResourcePath, "Pixmaps", "process-syncing-iiii-active.png")),
                new NSImage (Path.Combine (NSBundle.MainBundle.ResourcePath, "Pixmaps", "process-syncing-iiiii-active.png"))
            };
            
            this.error_image = new NSImage (
                Path.Combine (NSBundle.MainBundle.ResourcePath, "Pixmaps", "process-syncing-error.png"));

            this.error_image_active = new NSImage (
                Path.Combine (NSBundle.MainBundle.ResourcePath, "Pixmaps", "process-syncing-error-active.png"));

            this.folder_image       = new NSImage (Path.Combine (NSBundle.MainBundle.ResourcePath, "cmissync-folder.icns"));
            this.caution_image      = new NSImage (Path.Combine (NSBundle.MainBundle.ResourcePath, "Pixmaps", "process-syncing-error.png"));
            this.cmissync_image     = new NSImage (Path.Combine (NSBundle.MainBundle.ResourcePath, "cmissync-app.icns"));
            this.pause_image        = new NSImage(Path.Combine(NSBundle.MainBundle.ResourcePath, "Pixmaps", "media_playback_pause.png"));
            this.resume_image       = new NSImage(Path.Combine(NSBundle.MainBundle.ResourcePath, "Pixmaps", "media_playback_start.png"));
        }
    }
    
    
    public class StatusIconMenuDelegate : NSMenuDelegate {
        
        public override void MenuWillHighlightItem (NSMenu menu, NSMenuItem item)
        {
        }

    
        public override void MenuWillOpen (NSMenu menu)
        {
            InvokeOnMainThread (delegate {
                NSApplication.SharedApplication.DockTile.BadgeLabel = null;
            });
        }
    }

    //TODO This isn't working well, please create a native COCOA like solution 
    public class TransmissionMenuItem : NSMenuItem {

        private FileTransmissionEvent transmissionEvent;
        private int updateInterval = 1;
        private DateTime updateTime;
        private bool run = false;
        private object disposeLock = new object ();
        private bool disposed = false;

        private string TransmissionStatus(TransmissionProgressEventArgs e)
        {
            double? percent = e.Percent;
            long? bitsPerSecond = e.BitsPerSecond;
            if (percent != null && bitsPerSecond != null) {
                return String.Format ("{0} ({1} {2})",
                    System.IO.Path.GetFileName (transmissionEvent.Path),
                    CmisSync.Lib.Utils.FormatPercent((double)percent),
                    CmisSync.Lib.Utils.FormatBandwidth ((long)bitsPerSecond));
            } else {
                return System.IO.Path.GetFileName (transmissionEvent.Path);
            }
        }

        private void TransmissionEvent(object sender, TransmissionProgressEventArgs e)
        {
            lock (disposeLock) {
                if (disposed) {
                    return;
                }
                TimeSpan diff = DateTime.Now - updateTime;
                if (diff.Seconds < updateInterval) {
                    return;
                }
                if (run) {
                    return;
                }

                run = true;
                updateTime = DateTime.Now;
                string title = TransmissionStatus (e);
                BeginInvokeOnMainThread (delegate
                {
                    lock(disposeLock) {
                        if (!disposed) {
                            Title = title;
                        }
                    }
                });
                run = false;
            }
        }

        public TransmissionMenuItem(FileTransmissionEvent transmission)
        {
            Activated += delegate
            {
                NSWorkspace.SharedWorkspace.OpenFile (System.IO.Directory.GetParent (transmission.Path).FullName);
            };

            transmissionEvent = transmission;
            updateTime = DateTime.Now;

            Title = TransmissionStatus (transmission.Status);
            switch (transmission.Type) {
            case FileTransmissionType.DOWNLOAD_NEW_FILE:
                Image = new NSImage (Path.Combine (NSBundle.MainBundle.ResourcePath, "Pixmaps", "Downloading.png"));
                break;
            case FileTransmissionType.UPLOAD_NEW_FILE:
                Image = new NSImage (Path.Combine (NSBundle.MainBundle.ResourcePath, "Pixmaps", "Uploading.png"));
                break;
            case FileTransmissionType.DOWNLOAD_MODIFIED_FILE:
                goto case FileTransmissionType.UPLOAD_MODIFIED_FILE;
            case FileTransmissionType.UPLOAD_MODIFIED_FILE:
                Image = new NSImage (Path.Combine (NSBundle.MainBundle.ResourcePath, "Pixmaps", "Updating.png"));
                break;
            }
            transmissionEvent.TransmissionStatus += TransmissionEvent;
        }

        protected override void Dispose (bool disposing)
        {
            lock (disposeLock) {
                if (!disposed) {
                    transmissionEvent.TransmissionStatus -= TransmissionEvent;
                }
                disposed = true;
            }
            base.Dispose (disposing);
        }
    }


}
