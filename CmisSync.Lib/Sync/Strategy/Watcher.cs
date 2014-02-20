using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Data.Common;

using DotCMIS;
using DotCMIS.Client;

using CmisSync.Lib.Events;
using CmisSync.Lib.Storage;

using log4net;

namespace CmisSync.Lib.Sync.Strategy
{
    /// <summary>
    /// Watcher sync.
    /// </summary>
    public class Watcher : ReportingSyncEventHandler
    {

        public static readonly int DEFAULT_FS_WATCHER_SYNC_STRATEGY_PRIORITY = 1;
        private FileSystemWatcher FsWatcher;

        /// <summary>
        /// Enables the FSEvent report
        /// </summary>
        public bool EnableEvents { get { return FsWatcher.EnableRaisingEvents; } set { FsWatcher.EnableRaisingEvents = value; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Sync.Strategy.Watcher"/> class.
        /// </summary>
        /// <param name='watcher'>
        /// A FileSystemWatcher Instance with an initialized Path Property.
        /// </param>
        /// <param name='queue'>
        /// Queue where the FSEvents and also the FileEvents and FolderEvents are reported.
        /// </param>
        /// <param name='fsFactory'>
        /// Factory for everyThing FileSystem related. Null leaves the default which is fine.
        /// </param>
        public Watcher (FileSystemWatcher watcher, ISyncEventQueue queue, FileSystemInfoFactory fsFactory = null) : base(queue)
        {
            if (watcher == null)
                throw new ArgumentNullException ("The given fs watcher must not be null");
            if (String.IsNullOrEmpty (watcher.Path))
                throw new ArgumentException ("The given watcher must contain a path, where it is listening");
            FsWatcher = watcher;
            FsWatcher.IncludeSubdirectories = true;
            FsWatcher.Filter = "*";
            FsWatcher.InternalBufferSize = 4 * 1024 * 16;
            FsWatcher.NotifyFilter = NotifyFilters.Size | NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite | NotifyFilters.Security;
            //FsWatcher.Error += new ErrorEventHandler (OnError);
            FsWatcher.Created += new FileSystemEventHandler (OnCreatedChangedDeleted);
            FsWatcher.Deleted += new FileSystemEventHandler (OnCreatedChangedDeleted);
            FsWatcher.Changed += new FileSystemEventHandler (OnCreatedChangedDeleted);
            
            if(fsFactory == null){
                this.fsFactory = new FileSystemInfoFactory();
            }else{
                this.fsFactory = fsFactory;
            }FsWatcher.Renamed += new RenamedEventHandler (OnRenamed);
        }

        /// <summary>
        /// Handles FSEvents.
        /// </summary>
        /// <param name='e'>
        /// All not yet filtered events. Only FSEvents are handled.
        /// </param>
        public override bool Handle (ISyncEvent e)
        {
            var fsevent = e as FSEvent;
            if (fsevent == null)
                return false;

            if (fsevent.IsDirectory ()) {
                handleFolderEvents (fsevent);
            } else {
                handleFileEvents (fsevent);
            }
            return true;
        }

        /// <summary>
        /// Handles the FSEvent of folder and creates FolderEvents.
        /// </summary>
        /// <param name='e'>
        /// FSEvent.
        /// </param>
        private void handleFolderEvents (FSEvent e)
        {
            var movedEvent = e as FSMovedEvent;
            FolderEvent folderEvent;
            if (movedEvent != null) {
                folderEvent = new FolderMovedEvent (
                    fsFactory.CreateDirectoryInfo (movedEvent.OldPath),
                    fsFactory.CreateDirectoryInfo (movedEvent.Path),
                    null, null) {Local = MetaDataChangeType.MOVED};
            } else {
                folderEvent = new FolderEvent (fsFactory.CreateDirectoryInfo (e.Path), null);
                switch (e.Type) {
                case WatcherChangeTypes.Created:
                    folderEvent.Local = MetaDataChangeType.CREATED;
                    break;
                case WatcherChangeTypes.Changed:
                    folderEvent.Local = MetaDataChangeType.CHANGED;
                    break;
                case WatcherChangeTypes.Deleted:
                    folderEvent.Local = MetaDataChangeType.DELETED;
                    break;
                default:
                    // This should never ever happen
                    return;
                }
            }
            Queue.AddEvent (folderEvent);
        }

        /// <summary>
        /// Handles the FSEvents of files and creates FileEvents.
        /// </summary>
        /// <param name='e'>
        /// FSEvent.
        /// </param>
        private void handleFileEvents (FSEvent e)
        {
            var movedEvent = e as FSMovedEvent;
            if (movedEvent != null) {
                var oldfile = new FileInfo (movedEvent.OldPath);
                var newfile = new FileInfo (movedEvent.Path);
                var newEvent = new FileMovedEvent (
                    oldfile,
                    newfile,
                    null, newfile.Directory,
                    null, null);
                Queue.AddEvent (newEvent);
            } else {
                var file = new FileInfo (e.Path);
                var newEvent = new FileEvent (file, file.Directory, null);
                switch (e.Type) {
                case WatcherChangeTypes.Created:
                    newEvent.Local = MetaDataChangeType.CREATED;
                    newEvent.LocalContent = ContentChangeType.CREATED;
                    break;
                case WatcherChangeTypes.Changed:
                    newEvent.LocalContent = ContentChangeType.CHANGED;
                    break;
                case WatcherChangeTypes.Deleted:
                    newEvent.Local = MetaDataChangeType.DELETED;
                    newEvent.LocalContent = ContentChangeType.DELETED;
                    break;
                }
                Queue.AddEvent (newEvent);
            }

        }

        /// <summary>
        /// Returns 1. Cannot be changed during runtime.
        /// </summary>
        /// <value>
        /// The priority.
        /// </value>
        public override int Priority {
            get {
                return DEFAULT_FS_WATCHER_SYNC_STRATEGY_PRIORITY;
            }
        }

        /// <summary>
        /// Raises the created/changed/deleted event as FSEvent.
        /// </summary>
        /// <param name='source'>
        /// Source file system watcher.
        /// </param>
        /// <param name='e'>
        /// Reported changes.
        /// </param>
        private void OnCreatedChangedDeleted (object source, FileSystemEventArgs e)
        {
            Queue.AddEvent (new FSEvent (e.ChangeType, e.FullPath));
        }

        /// <summary>
        /// Raises the renamed event as FSMovedEvent.
        /// </summary>
        /// <param name='source'>
        /// Source file system watcher.
        /// </param>
        /// <param name='e'>
        /// Reported renaming.
        /// </param>
        private void OnRenamed (object source, RenamedEventArgs e)
        {
            string oldname = e.OldFullPath;
            string newname = e.FullPath;
            if (oldname.StartsWith (FsWatcher.Path) && newname.StartsWith (FsWatcher.Path)) {
                Queue.AddEvent (new FSMovedEvent (oldname, newname));
            } else if (oldname.StartsWith (FsWatcher.Path)) {
                Queue.AddEvent (new FSEvent (WatcherChangeTypes.Deleted, oldname));
            } else if (newname.StartsWith (FsWatcher.Path)) {
                Queue.AddEvent (new FSEvent (WatcherChangeTypes.Created, newname));
            }
        }

/*        private void Sync (string remoteFolder, string localFolder)
        {
            Logger.Debug (remoteFolder + " : " + localFolder);
            foreach (string pathname in repo.Watcher.GetChangeList()) {

                Watcher.ChangeTypes change = repo.Watcher.GetChangeType (pathname);
                string name = pathname.Substring (localFolder.Length + 1);
                string[] folderNames = name.Split (Path.DirectorySeparatorChar);

                switch (change) {
                case Watcher.ChangeTypes.Created:
                case Watcher.ChangeTypes.Changed:
                    WatcherSyncUpdate (remoteFolder, localFolder, pathname);
                    break;
                case Watcher.ChangeTypes.Deleted:
                    WatcherSyncDelete (remoteFolder, localFolder, pathname);
                    break;
                default:
                    Debug.Assert (false, String.Format ("Invalid change {0} for pathname {1}.", change, pathname));
                    break;
                }
            }
        } */

        /*private void WatcherSyncUpdate (FSEvent fsevent)
        {
            string pathname = fsevent.Path;
            string name = pathname.Substring (localPath.Length + 1);
            string remotePathname = Path.Combine (remotePath, name).Replace ('\\', '/');

            IFolder remoteBase = null;
            if (File.Exists (pathname) || Directory.Exists (pathname)) {
                string remoteBaseName = Path.GetDirectoryName (remotePathname).Replace ('\\', '/');
                try {
                    remoteBase = (IFolder)session.GetObjectByPath (remoteBaseName);
                } catch (Exception ex) {
                    Logger.Warn (String.Format ("Exception when query remote {0}: {1}", remoteBaseName, Utils.ToLogString (ex)));
                }
                if (null == remoteBase) {
                    Logger.Warn (String.Format ("The remote base folder {0} for local {1} does not exist, ignore for the update action", remoteBaseName, pathname));
                    return;
                }
            } else {
                Logger.Info (String.Format ("The file/folder {0} is deleted, ignore for the update action", pathname));
                return;
            }

            try {
                if (File.Exists (pathname)) {
                    bool success = false;
                    if (database.ContainsFile (pathname)) {
                        if (database.LocalFileHasChanged (pathname)) {
                            success = UpdateFile (pathname, remoteBase);
                            Logger.Info (String.Format ("Update {0}: {1}", pathname, success));
                        } else {
                            success = true;
                            Logger.Info (String.Format ("File {0} remains unchanged, ignore for the update action", pathname));
                        }
                    } else {
                        success = UploadFile (pathname, remoteBase);
                        Logger.Info (String.Format ("Upload {0}: {1}", pathname, success));
                    }
                    if (!success) {
                        Logger.Warn ("Failure to update: " + pathname);
                    }
                    return;
                }
            } catch (Exception e) {
                Logger.Warn (String.Format ("Exception while sync to update file {0}: {1}", pathname, Utils.ToLogString (e)));
                return;
            }

            try {
                if (Directory.Exists (pathname)) {
                    if (database.ContainsFolder (pathname)) {
                        Logger.Info (String.Format ("Database exists for {0}, ignore for the update action", pathname));
                    } else {
                        if (UploadFolderRecursively (remoteBase, pathname)) {
                            Logger.Info ("Upload local folder on server: " + pathname);
                        } else {
                            Logger.Warn ("Failure to upload local folder on server: " + pathname);
                        }
                    }
                    return;
                }
            } catch (Exception e) {
                Logger.Warn (String.Format ("Exception while sync to update folder {0}: {1}", pathname, Utils.ToLogString (e)));
                return;
            }

            Logger.Info (String.Format ("The file/folder {0} is deleted, ignore for the update action", pathname));
        }

        private void WatcherSyncDelete (FSEvent fsevent)
        {
            string pathname = fsevent.Path;
            string name = pathname.Substring (localPath.Length + 1);
            string remoteName = Path.Combine (remotePath, name).Replace ('\\', '/');
            DbTransaction transaction = null; 
            try {
                transaction = database.BeginTransaction ();
                if (database.ContainsFile (pathname)) {
                    Logger.Info ("Removing locally deleted file on server: " + pathname);
                    try {
                        IDocument remote = (IDocument)session.GetObjectByPath (remoteName);
                        if (remote != null) {
                            remote.DeleteAllVersions ();
                        }
                    } catch (Exception ex) {
                        Logger.Warn (String.Format ("Exception when operate remote {0}: {1}", remoteName, Utils.ToLogString (ex)));
                    }
                    database.RemoveFile (pathname);
                } else if (database.ContainsFolder (pathname)) {
                    Logger.Info ("Removing locally deleted folder on server: " + pathname);
                    try {
                        IFolder remote = (IFolder)session.GetObjectByPath (remoteName);
                        if (remote != null) {
                            remote.DeleteTree (true, null, true);
                        }
                    } catch (Exception ex) {
                        Logger.Warn (String.Format ("Exception when operate remote {0}: {1}", remoteName, Utils.ToLogString (ex)));
                    }
                    database.RemoveFolder (pathname);
                } else {
                    Logger.Info ("Ignore the delete action for the local created and deleted file/folder: " + pathname);
                }
                transaction.Commit ();
            } catch (Exception e) {
                if (transaction != null) {
                    transaction.Rollback ();
                }
                Logger.Warn (String.Format ("Exception while sync to delete file/folder {0}: {1}", pathname, Utils.ToLogString (e)));
                return;
            } finally {
                if (transaction != null) {
                    transaction.Dispose ();
                }
            }
            return;
        }*/
    }
}
