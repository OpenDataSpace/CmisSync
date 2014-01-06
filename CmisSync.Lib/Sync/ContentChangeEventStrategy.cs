using System;
using System.Threading;
using System.Threading.Tasks;
using DotCMIS.Client;
using CmisSync.Lib.Cmis;
using CmisSync.Lib.Events;


namespace CmisSync.Lib
{
    public class ChangeLogStrategy : SyncEventHandler, IDisposable
    {
        private ISession session;
        private IDatabase db;
        private RepoInfo info;
        private SyncEventQueue queue;
        private int MaxNumberOfContentChanges;
        private bool IsPropertyChangesSupported;

        private string lastRemoteChangeLogTokenBeforeFullCrawlSync = null;

        private object syncLock = new object();

        private static readonly string FULL_SYNC_PARAM_NAME = "lastTokenOnServer";
        public static readonly int DEFAULT_PRIORITY = 1000;

        public override int Priority {
            get {
                return DEFAULT_PRIORITY;
            }
        }

        public ChangeLogStrategy(ISession session, IDatabase db, RepoInfo repoInfo, SyncEventQueue queue, int maxNumberOfContentChanges = 100, bool isPropertyChangesSupported = false) {
            if(session == null)
                throw new ArgumentNullException("Session instance is needed for the ChangeLogStrategy, but was null");
            if(db == null)
                throw new ArgumentNullException("Database instance is needed for the ChangeLogStrategy, but was null");
            if(repoInfo == null)
                throw new ArgumentNullException("RepoInfo instance is needed for the ChangeLogStrategy, but was null");
            if(queue == null)
                throw new ArgumentNullException("SyncEventQueue instance is needed for the ChangeLogStrategy, but was null");
            if(maxNumberOfContentChanges <= 0)
                throw new ArgumentException("MaxNumberOfContentChanges must be greater then zero");
            this.session = session;
            this.db = db;
            this.info = repoInfo;
            this.queue = queue;
            this.MaxNumberOfContentChanges = maxNumberOfContentChanges;
            this.IsPropertyChangesSupported = isPropertyChangesSupported;
        }

        public override bool Handle (ISyncEvent e)
        {
            StartNextSyncEvent syncEvent = e as StartNextSyncEvent;
            if(syncEvent != null)
            {
                if( syncEvent.FullSyncRequested)
                {
                    // Get last change log token on server side.
                    session.Binding.GetRepositoryService().GetRepositoryInfos(null);    //  refresh
                    lastRemoteChangeLogTokenBeforeFullCrawlSync = session.Binding.GetRepositoryService().GetRepositoryInfo(session.RepositoryInfo.Id, null).LatestChangeLogToken;
                    // Use fallback sync algorithm
                    return false;
                }
                else
                {
                    return startSync();
                }
            }

            FullSyncCompletedEvent syncCompleted = e as FullSyncCompletedEvent;
            if(syncCompleted != null) {
                string lastTokenOnServer;
                if(syncCompleted.StartEvent.TryGetParam(FULL_SYNC_PARAM_NAME, out lastTokenOnServer))
                {
                    db.SetChangeLogToken(lastTokenOnServer);
                }
            }

            return false;
        }
        /// <summary>
        /// Tries to start sync algorithm, if connection was successful, this routine returns with true and starts syncing in background, otherwise a fallback mechanism is used
        /// </summary>
        /// <returns>
        /// True if requested folder is available, otherwise false
        /// </returns>
        private bool startSync() {
            try {
                string lastTokenOnClient = db.GetChangeLogToken();

                // Get last change log token on server side.
                session.Binding.GetRepositoryService().GetRepositoryInfos(null);    //  refresh
                string lastTokenOnServer = session.Binding.GetRepositoryService().GetRepositoryInfo(session.RepositoryInfo.Id, null).LatestChangeLogToken;

                if(lastTokenOnClient != lastTokenOnServer)
                {
                    using(var syncTask = new Task((Action) delegate() {
                        if(Monitor.TryEnter(syncLock)) {
                            try{
                                Sync();
                            }finally {
                                Monitor.Exit(syncLock);
                            }
                        }
                    })) {
                        syncTask.Start();
                    }
                }
                // No changes or background process started
                return true;
            }catch(Exception) {
                // Use fallback sync algorithm
                return false;
            }
        }


        public void Dispose() {

        }

        private void Sync()
        {
/*            IFolder remoteFolder = session.GetObjectByPath(info.RemotePath) as IFolder;
            // Get last change log token on server side.
            session.Binding.GetRepositoryService().GetRepositoryInfos(null);    //  refresh
            string lastTokenOnServer = session.Binding.GetRepositoryService().GetRepositoryInfo(session.RepositoryInfo.Id, null).LatestChangeLogToken;

            // Get last change token that had been saved on client side.
            string lastTokenOnClient = db.GetChangeLogToken();

            if (lastTokenOnClient == null)
            {
                // Token is null, which means no content change sync has ever happened yet, so just sync everything from remote.
                // Force full sync with lastTokenOnServer as param
                var fullsyncevent = new StartNextSyncEvent(true);
                fullsyncevent.SetParam(FULL_SYNC_PARAM_NAME, lastTokenOnServer);
                queue.AddEvent(fullsyncevent);
                return;
            }

            do
            {
                // Check which files/folders have changed.
                IChangeEvents changes = session.GetContentChanges(lastTokenOnClient, IsPropertyChangesSupported, MaxNumberOfContentChanges);
                // Replicate each change to the local side.
                foreach (IChangeEvent change in changes.ChangeEventList)
                {
                    ICmisObject remoteObject = null;
                    if(change.ChangeType == DotCMIS.Enums.ChangeType.Created ||
                       change.ChangeType == DotCMIS.Enums.ChangeType.Updated) {
                        try{
                            remoteObject = session.GetObject(change.ObjectId);
                            IFolder folder = remoteObject as IFolder;
                            if(folder != null)

                        } catch(Exception) {
                            return;
                        }
                    }
                }

                // Save change log token locally.
                if (changes.HasMoreItems == true)
                {
                    lastTokenOnClient = changes.LatestChangeLogToken;
                }
                else
                {
                    lastTokenOnClient = lastTokenOnServer;
                }
                db.SetChangeLogToken(lastTokenOnClient);
                session.Binding.GetRepositoryService().GetRepositoryInfos(null);    //  refresh
                lastTokenOnServer = session.Binding.GetRepositoryService().GetRepositoryInfo(session.RepositoryInfo.Id, null).LatestChangeLogToken;
            }
            while (!lastTokenOnServer.Equals(lastTokenOnClient));
        }


            /// <summary>
            /// Apply a remote change for Created or Updated.
            /// </summary>
            private bool ApplyRemoteChangeUpdate(IChangeEvent change)
            {
                ICmisObject cmisObject = null;
                IFolder remoteFolder = null;
                IDocument remoteDocument = null;
                string remotePath = null;
                ICmisObject remoteObject = null;
                IFolder remoteParent = null;

                try
                {
                    cmisObject = session.GetObject(change.ObjectId);
                }
                catch(CmisObjectNotFoundException)
                {
                    Logger.Info("Ignore the missed object for " + change.ObjectId);
                    return true;
                }
                catch (Exception ex)
                {
                    Logger.Warn("Exception when GetObject for " + change.ObjectId + " : " + Utils.ToLogString(ex));
                    return false;
                }

                remoteDocument = cmisObject as IDocument;
                remoteFolder = cmisObject as IFolder;
                if (remoteDocument == null && remoteFolder == null)
                {
                    Logger.Info("Change in no sync object: " + change.ObjectId);
                    return true;
                }
                if (remoteDocument != null)
                {
                    if (!Utils.WorthSyncing(remoteDocument.Name))
                    {
                        Logger.Info("Change in remote unworth syncing file: " + remoteDocument.Paths);
                        return true;
                    }
                    if (remoteDocument.Paths.Count == 0)
                    {
                        Logger.Info("Ignore the unfiled object: " + remoteDocument.Name);
                        return true;
                    }
                    // TODO: Support Multiple Paths
                    remotePath = remoteDocument.Paths[0];
                    remoteParent = remoteDocument.Parents[0];
                }
                if (remoteFolder != null)
                {
                    remotePath = remoteFolder.Path;
                    remoteParent = remoteFolder.FolderParent;
                    foreach (string name in remotePath.Split('/'))
                    {
                        if (!String.IsNullOrEmpty(name) && Utils.IsInvalidFolderName(name))
                        {
                            Logger.Info(String.Format("Change in illegal syncing path name {0}: {1}", name, remotePath));
                            return true;
                        }
                    }
                }

                if (!remotePath.StartsWith(this.remoteFolderPath))
                {
                    Logger.Info("Change in unrelated path: " + remotePath);
                    return true;    // The change is not under the folder we care about.
                }

                if (this.repoinfo.isPathIgnored(remotePath))
                {
                    Logger.Info("Change in ignored path: " + remotePath);
                    return true;
                }

                string relativePath = remotePath.Substring(remoteFolderPath.Length);
                if (relativePath.Length <= 0)
                {
                    Logger.Info("Ignore change in root path: " + remotePath);
                    return true;
                }
                if (relativePath[0] == '/')
                {
                    relativePath = relativePath.Substring(1);
                }

                try
                {
                    remoteObject = session.GetObjectByPath(remotePath);
                }
                catch(CmisObjectNotFoundException)
                {
                    Logger.Info(String.Format("Ignore remote path {0} deleted from id {1}", remotePath, cmisObject.Id));
                    return true;
                }
                catch (Exception ex)
                {
                    Logger.Warn("Exception when GetObject for " + remotePath + " : " + Utils.ToLogString(ex));
                    return false;
                }

                if (remoteObject.Id != cmisObject.Id)
                {
                    Logger.Info(String.Format("Ignore remote path {0} changed from id {1} to id {2}", remotePath, cmisObject.Id, remoteObject.Id));
                    return true;
                }

                string localPath = Path.Combine(repoinfo.TargetDirectory, relativePath).Replace('/', Path.DirectorySeparatorChar);
                if (!DownloadFolder(remoteParent, Path.GetDirectoryName(localPath)))
                {
                    Logger.Warn("Failed to download the parent folder for " + localPath);
                    return false;
                }

                if (null != remoteDocument)
                {
                    Logger.Info(String.Format("New remote file ({0}) found.", remotePath));
                    //  check moveObject
                    string savedDocumentPath = database.GetFilePath(change.ObjectId);
                    if ((null != savedDocumentPath) && (savedDocumentPath != localPath))
                    {
                        if (File.Exists(localPath))
                        {
                            File.Delete(savedDocumentPath);
                            database.RemoveFile(savedDocumentPath);
                        }
                        else
                        {
                            if (File.Exists(savedDocumentPath))
                            {
                                if (!Directory.Exists(Path.GetDirectoryName(localPath)))
                                {
                                    Logger.Warn("Creating local directory: "+ localPath);
                                    Directory.CreateDirectory(Path.GetDirectoryName(localPath));
                                }
                                File.Move(savedDocumentPath, localPath);
                            }
                            database.MoveFile(savedDocumentPath, localPath);
                        }
                    }

                    return SyncDownloadFile(remoteDocument, Path.GetDirectoryName(localPath));
                }

                if (null != remoteFolder)
                {
                    Logger.Info(String.Format("New remote folder ({0}) found.", remotePath));
                    //  check moveObject
                    string savedFolderPath = database.GetFolderPath(change.ObjectId);
                    if ((null != savedFolderPath) && (savedFolderPath != localPath))
                    {
                        MoveFolderLocally(savedFolderPath, localPath);
                        return CrawlSync(remoteFolder, localPath);
                    }
                    else
                    {
                        if(SyncDownloadFolder(remoteFolder, Path.GetDirectoryName(localPath)))
                        {
                            return CrawlSync(remoteFolder,localPath);
                        }
                        else
                        {
                            return false;
                        }
                    }
                }

                return true;
            }


            /// <summary>
            /// Apply a remote change for Deleted.
            /// </summary>
            private bool ApplyRemoteChangeDelete(IChangeEvent change)
            {
                try
                {
                    ICmisObject remoteObject = session.GetObject(change.ObjectId);
                    if (null != remoteObject)
                    {
                        //  should be moveObject
                        Logger.Info("Ignore moveObject for id " + change.ObjectId);
                        return true;
                    }
                }
                catch (CmisObjectNotFoundException)
                {
                }
                catch (Exception ex)
                {
                    Logger.Warn("Exception when GetObject for " + change.ObjectId + " : " + Utils.ToLogString(ex));
                }

                string savedDocumentPath = database.GetFilePath(change.ObjectId);
                if (null != savedDocumentPath)
                {
                    Logger.Info("Remove local document: " + savedDocumentPath);
                    if(File.Exists(savedDocumentPath))
                        File.Delete(savedDocumentPath);
                    database.RemoveFile(savedDocumentPath);
                    Logger.Info("Removed local document: " + savedDocumentPath);
                    return true;
                }

                string savedFolderPath = database.GetFolderPath(change.ObjectId);
                if (null != savedFolderPath)
                {
                    Logger.Info("Remove local folder: " + savedFolderPath);
                    if(Directory.Exists(savedFolderPath)) {
                        Directory.Delete(savedFolderPath, true);
                        database.RemoveFolder(savedFolderPath);
                    }
                    Logger.Info("Removed local folder: " + savedFolderPath);
                    return true;
                }

                return true;
            }

            private bool DownloadFolder(IFolder remoteFolder, string localFolder)
            {
                if (Directory.Exists(localFolder))
                {
                    return true;
                }
                if (remoteFolder == null)
                {
                    return false;
                }
                if (!Directory.Exists(Path.GetDirectoryName(localFolder)))
                {
                    if (!DownloadFolder(remoteFolder.FolderParent, Path.GetDirectoryName(localFolder)))
                    {
                        return false;
                    }
                }
                return SyncDownloadFolder(remoteFolder, Path.GetDirectoryName(localFolder));*/
            }

    }
}

