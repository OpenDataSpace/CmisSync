using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data.Common;

using DotCMIS;
using DotCMIS.Client;

using CmisSync.Lib.Events;

using log4net;

namespace CmisSync.Lib.Sync.Strategy
{
    /// <summary>
    /// Watcher sync.
    /// </summary>
    public class Watcher : ReportingSyncEventHandler
    {

        private static readonly ILog Logger = LogManager.GetLogger(typeof(Watcher));

        private static readonly int DEFAULT_FS_WATCHER_SYNC_STRATEGY_PRIORITY = 1;

        private DirectoryInfo LocalFolder;

        private ISession Session;

        private FileSystemWatcher FsWatcher;

        /// <summary>
        /// Enables the FSEvent report
        /// </summary>
        public bool EnableEvent { get; set; }

        public Watcher(DirectoryInfo localFolder, ISession session, SyncEventQueue queue) : base(queue)
        {
            if(session == null)
                throw new ArgumentNullException("The given session must not be null");
            if(localFolder == null)
                throw new ArgumentNullException("The given local folder must not be null");
            localFolder.Refresh();
            if(!localFolder.Exists)
                throw new ArgumentException("The given folder does not exist");
            Session = session;
            LocalFolder = localFolder;
            FsWatcher = new FileSystemWatcher();
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
            if(fsevent == null)
                return false;
            switch(fsevent.Type)
            {
            case WatcherChangeTypes.Created:
                goto case WatcherChangeTypes.Changed;
            case WatcherChangeTypes.Changed:
                break;
            case WatcherChangeTypes.Deleted:
                break;
            }
            return true;
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
