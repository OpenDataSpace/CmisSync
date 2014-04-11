using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;

using CmisSync.Lib.Cmis;
using CmisSync.Lib.Events;
using CmisSync.Lib.Streams;
using CmisSync.Lib.ContentTasks;

using DotCMIS;
using DotCMIS.Client.Impl;
using DotCMIS.Client;
using DotCMIS.Data.Impl;
using DotCMIS.Data;
using DotCMIS.Enums;
using DotCMIS.Exceptions;

using log4net;

namespace CmisSync.Lib.Sync
{
    public partial class CmisRepo : RepoBase
    {
        // Log.
        private static readonly ILog Logger = LogManager.GetLogger(typeof(CmisRepo));


        /// <summary>
        /// Synchronization with a particular CMIS folder.
        /// </summary>
        public partial class SynchronizedFolder : IDisposable
        {
            // Log
            private static readonly ILog Logger = LogManager.GetLogger(typeof(SynchronizedFolder));
            
            /// <summary>
            /// Whether sync is bidirectional or only from server to client.
            /// TODO make it a CMIS folder - specific setting
            /// </summary>
            private bool BIDIRECTIONAL = true;

            /// <summary>
            /// At which degree the repository supports Change Logs.
            /// See http://docs.oasis-open.org/cmis/CMIS/v1.0/os/cmis-spec-v1.0.html#_Toc243905424
            /// The possible values are actually none, objectidsonly, properties, all
            /// But for now we only distinguish between none (false) and the rest (true)
            /// </summary>
            private bool ChangeLogCapability;

            /// <summary>
            /// If the repository is able send a folder tree in one request, this is true,
            /// Otherwise the default behaviour is false
            /// </summary>
            private bool IsGetFolderTreeSupported = false;

            /// <summary>
            /// If the repository allows to request all Descendants of a folder or file,
            /// this is set to true, otherwise the default behaviour is false
            /// </summary>
            private bool IsGetDescendantsSupported = false;

            /// <summary>
            /// Is true, if the repository is able to return property changes.
            /// </summary>
            private bool IsPropertyChangesSupported = false;


            /// <summary>
            /// Session to the CMIS repository.
            /// </summary>
            private ISession session;


            /// <summary>
            /// Path of the root in the remote repository.
            /// Example: "/User Homes/nicolas.raoul/demos"
            /// </summary>
            private string remoteFolderPath;


            /// <summary>
            /// Backgound syncing flag.
            /// </summary>
            private bool backgroundSyncing = false;

            /// <summary>
            /// The background syncing lock for operations on the flag.
            /// </summary>
            private Object bgSyncingLock = new Object();

            /// <summary>
            /// The sync lock for executing only one sync process per instance
            /// </summary>
            private Object syncLock = new Object();


            /// <summary>
            /// Parameters to use for all CMIS requests.
            /// </summary>
            private Dictionary<string, string> cmisParameters;

            /// <summary>
            /// A storage to temporary save aborted uploads and its last successful state informations.
            /// </summary>
            private Dictionary<string, IDocument> uploadProgresses = new Dictionary<string, IDocument>();


            /// <summary>
            /// Track whether <c>Dispose</c> has been called.
            /// </summary>
            private bool disposed = false;


            /// <summary>
            /// Track whether <c>Dispose</c> has been called.
            /// </summary>
            private Object disposeLock = new Object();


            /// <summary>
            /// Database to cache remote information from the CMIS server.
            /// </summary>
            private Database database;


            /// <summary>
            /// Listener we inform about activity (used by spinner).
            /// </summary>
            private IActivityListener activityListener;


            /// <summary>
            /// Configuration of the CmisSync synchronized folder, as defined in the XML configuration file.
            /// </summary>
            private RepoInfo repoinfo;


            /// <summary>
            /// Link to parent object.
            /// </summary>
            private RepoBase repo;

            //private WatcherSync watcherStrategy;

            private PersistentStandardAuthenticationProvider authProvider;

            /// <summary>
            /// EventQueue
            /// </summary>
            public SyncEventQueue Queue {get; private set;}

            /// <summary>
            /// Track whether a full sync is done
            /// </summary>
            private bool syncFull = false;

            /// <summary>
            /// Changes on file system detected.
            /// </summary>
            private bool changesOnFileSystemDetected = true;

            /// <summary>
            /// If set to true, the session should be reconnected on next sync.
            /// </summary>
            private bool reconnect = false;
            
            /// <summary>
            ///  Constructor for Repo (at every launch of CmisSync)
            /// </summary>
            public SynchronizedFolder(RepoInfo repoInfo,
                IActivityListener listener, RepoBase repoCmis)
            {
                using(log4net.NDC.Push("Constructor: " + repoInfo.Name))
                {
                if (null == repoInfo || null == repoCmis)
                {
                    throw new ArgumentNullException("repoInfo");
                }

                this.repo = repoCmis;
                this.activityListener = listener;
                this.repoinfo = repoInfo;

                Queue = repoCmis.Queue;
                // Database is the user's AppData/Roaming
                database = new Database(repoinfo.CmisDatabase);
                authProvider = new PersistentStandardAuthenticationProvider(new CmisSync.Lib.Storage.TemporaryCookieStorage(){
                    Cookies = new CookieCollection()
                }, repoInfo.Address);
                // Get path on remote repository.
                remoteFolderPath = repoinfo.RemotePath;

                cmisParameters = new Dictionary<string, string>();
                UpdateCmisParameters();
                if (Logger.IsInfoEnabled)
                {
                    foreach (string ignoredFolder in repoinfo.GetIgnoredPaths())
                    {
                        Logger.Info("The folder \"" + ignoredFolder + "\" will be ignored");
                    }
                }
                //this.watcherStrategy = new WatcherSync(repoinfo, session);
                //repoCmis.EventManager.AddEventHandler(this.watcherStrategy);
                repoCmis.EventManager.AddEventHandler(new GenericSyncEventHandler<RepoConfigChangedEvent>(10, RepoInfoChanged));
                repoCmis.EventManager.AddEventHandler(new GenericSyncEventHandler<FSEvent>(0, delegate(ISyncEvent e) {
                    Logger.Debug("FSEvent found on Queue");
                    this.changesOnFileSystemDetected = true;
                    return true;
                }));
                }
            }

            /// <summary>
            /// This method is called, every time the config changes
            /// </summary>
            /// <param name="e"></param>
            /// <returns></returns>
            private bool RepoInfoChanged(ISyncEvent e)
            {
                if (e is RepoConfigChangedEvent)
                {
                    repoinfo = (e as RepoConfigChangedEvent).RepoInfo;
                    //this.repo.EventManager.RemoveEventHandler(this.watcherStrategy);
                    //this.watcherStrategy = new WatcherSync(repoinfo, session);
                    //this.repo.EventManager.AddEventHandler(this.watcherStrategy);
                    UpdateCmisParameters();
                    ForceFullSyncAtNextSync();
                }
                return false;
            }

            /// <summary>
            /// Loads the CmisParameter from repoinfo. If repoinfo has been changed, this method sets the new informations for the next session
            /// </summary>
            private void UpdateCmisParameters()
            {
                reconnect = true;
                cmisParameters[SessionParameter.BindingType] = BindingType.AtomPub;
                cmisParameters[SessionParameter.AtomPubUrl] = repoinfo.Address.ToString();
                cmisParameters[SessionParameter.User] = repoinfo.User;
                cmisParameters[SessionParameter.Password] = repoinfo.Password.ToString();
                cmisParameters[SessionParameter.RepositoryId] = repoinfo.RepoID;
                // Sets the Connect Timeout to infinite
                cmisParameters[SessionParameter.ConnectTimeout] = "-1";
                // Sets the Read Timeout to infinite
                cmisParameters[SessionParameter.ReadTimeout] = "-1";
                cmisParameters[SessionParameter.DeviceIdentifier] = ConfigManager.CurrentConfig.DeviceId.ToString();
                cmisParameters[SessionParameter.UserAgent] = Utils.CreateUserAgent();
                cmisParameters[SessionParameter.Compression] = Boolean.TrueString;
            }


            /// <summary>
            /// Destructor.
            /// </summary>
            ~SynchronizedFolder()
            {
                Dispose(false);
            }


            /// <summary>
            /// Implement IDisposable interface. 
            /// </summary>
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }


            /// <summary>
            /// Dispose pattern implementation.
            /// </summary>
            protected virtual void Dispose(bool disposing)
            {
                lock (disposeLock)
                {
                    if (!this.disposed)
                    {
                        if (disposing)
                        {
                            this.authProvider.Dispose();
                            this.database.Dispose();
                        }
                        this.disposed = true;
                    }
                }
            }

            /// <summary>
            /// Resets all the failed upload to zero.
            /// </summary>
            public void resetFailedOperationsCounter()
            {
                database.DeleteAllFailedOperations();
            }


            /// <summary>
            /// Connect to the CMIS repository.
            /// </summary>
            public void Connect()
            {
                using(log4net.ThreadContext.Stacks["NDC"].Push("Connect"))
                {
                try
                {
                    // Create session factory.
                    SessionFactory factory = SessionFactory.NewInstance();
                    session = factory.CreateSession(cmisParameters,null, authProvider, null);
                    // Detect whether the repository has the ChangeLog capability.
                    Logger.Debug("Created CMIS session: " + session.ToString());
                    ChangeLogCapability = session.RepositoryInfo.Capabilities.ChangesCapability == CapabilityChanges.All
                            || session.RepositoryInfo.Capabilities.ChangesCapability == CapabilityChanges.ObjectIdsOnly;
                    IsGetDescendantsSupported = session.RepositoryInfo.Capabilities.IsGetDescendantsSupported == true;
                    IsGetFolderTreeSupported = session.RepositoryInfo.Capabilities.IsGetFolderTreeSupported == true;
                    Config.SyncConfig.Folder folder = ConfigManager.CurrentConfig.getFolder(this.repoinfo.Name);
                    if (folder != null)
                    {
                        Config.Feature features = folder.SupportedFeatures;
                        if (features != null)
                        {
                            if (IsGetDescendantsSupported && features.GetDescendantsSupport == false)
                                IsGetDescendantsSupported = false;
                            if (IsGetFolderTreeSupported && features.GetFolderTreeSupport == false)
                                IsGetFolderTreeSupported = false;
                            if (ChangeLogCapability && features.GetContentChangesSupport == false)
                                ChangeLogCapability = false;
                            if(ChangeLogCapability && session.RepositoryInfo.Capabilities.ChangesCapability == CapabilityChanges.All 
                                || session.RepositoryInfo.Capabilities.ChangesCapability == CapabilityChanges.Properties)
                                IsPropertyChangesSupported = true;
                        }
                    }
                    Logger.Debug("ChangeLog capability: " + ChangeLogCapability.ToString());
                    Logger.Debug("Get folder tree support: " + IsGetFolderTreeSupported.ToString());
                    Logger.Debug("Get descendants support: " + IsGetDescendantsSupported.ToString());
                    if(repoinfo.ChunkSize>0) {
                        Logger.Debug("Chunked Up/Download enabled: chunk size = "+ repoinfo.ChunkSize.ToString() + " byte");
                    }else {
                        Logger.Debug("Chunked Up/Download disabled");
                    }
                    HashSet<string> filters = new HashSet<string>();
                    filters.Add("cmis:objectId");
                    filters.Add("cmis:name");
                    filters.Add("cmis:contentStreamFileName");
                    filters.Add("cmis:contentStreamLength");
                    filters.Add("cmis:lastModificationDate");
                    filters.Add("cmis:path");
                    filters.Add("cmis:changeToken");
                    HashSet<string> renditions = new HashSet<string>();
                    renditions.Add("cmis:none");
                    session.DefaultContext = session.CreateOperationContext(filters, false, true, false, IncludeRelationshipsFlag.None, renditions, true, null, true, 100);
                    Queue.AddEvent(new SuccessfulLoginEvent(repoinfo.Address));
                    reconnect = false;
                }
                catch (DotCMIS.Exceptions.CmisPermissionDeniedException e)
                {
                    Logger.Info(String.Format("Failed to connect to server {0}", repoinfo.Address.AbsoluteUri), e);
                    Queue.AddEvent(new PermissionDeniedEvent(e));
                }
                catch (CmisRuntimeException e)
                {
                    if(e.Message == "Proxy Authentication Required")
                    {
                        Queue.AddEvent(new ProxyAuthRequiredEvent(e));
                        Logger.Warn("Proxy Settings Problem", e);
                    }
                    else
                    {
                        Logger.Error("Connection to repository failed: ", e);
                    }
                }
                catch (CmisObjectNotFoundException e)
                {
                    Logger.Error("Failed to find cmis object: ", e);
                }
                catch (CmisBaseException e)
                {
                    Logger.Error("Failed to create session to remote " + this.repoinfo.Address.ToString() + ": ", e);
                }
                }

            }

            /// <summary>
            /// Forces the full sync independent of FS events or Remote events.
            /// </summary>
            public void ForceFullSync()
            {
                ForceFullSyncAtNextSync();
                Sync();
            }

            /// <summary>
            /// Forces the full sync at next sync.
            /// This can be used to ensure a full sync if fs or remote events where lost.
            /// </summary>
            public void ForceFullSyncAtNextSync()
            {
                syncFull = false;
            }


            /// <summary>
            /// Synchronize between CMIS folder and local folder.
            /// </summary>
            public void Sync()
            {
                lock(syncLock) {
                    using(log4net.ThreadContext.Stacks["NDC"].Push(String.Format("[{0}]Sync()", this.repoinfo.Name)))
                    {
                    // If not connected, connect.
                    if (session == null || reconnect)
                    {
                        Connect();
                    } else {
                        // Force to reset the cache for each Sync
                        session.Clear();
                    }
                    sleepWhileSuspended();

                    if (session == null)
                    {
                        Logger.Error("Could not connect to: " + cmisParameters[SessionParameter.AtomPubUrl]);
                        return; // Will try again at next sync.
                    }

                    IFolder remoteFolder = (IFolder)session.GetObjectByPath(remoteFolderPath);
                    string localFolder = repoinfo.TargetDirectory;

                    if (!syncFull)
                    {
                        Logger.Debug("Invoke a full crawl sync");
                        syncFull = CrawlSync(remoteFolder, localFolder);
                        return;
                    }

                    if (ChangeLogCapability)
                    {
                        Logger.Debug("Invoke a remote change log sync" + (changesOnFileSystemDetected? "Local Changes detected" : ""));
                        ChangeLogSync(remoteFolder);
                        if(changesOnFileSystemDetected)
                        {
                            Logger.Debug("Changes on the local file system detected => starting crawl sync");
                            changesOnFileSystemDetected = false;
                            if (!CrawlSync(remoteFolder, localFolder))
                                changesOnFileSystemDetected = true;
                        }
                    }
                    else
                    {
                        //  have to crawl remote
                        Logger.Debug("Invoke a remote crawl sync");
                        CrawlSync(remoteFolder, localFolder);
                    }
                    }
                }
            }


            /// <summary>
            /// Sync in the background.
            /// </summary>
            public void SyncInBackground()
            {
                lock(bgSyncingLock) {
                    if (backgroundSyncing)
                    {
                        Logger.Debug("Already executing a sync process in background");
                        return;
                    } else {
                        backgroundSyncing = true;
                    }
                }
                using (BackgroundWorker bw = new BackgroundWorker())
                {
                    bw.DoWork += new DoWorkEventHandler(
                        delegate(Object o, DoWorkEventArgs args)
                        {
                            Logger.Debug("Launching sync: " + repoinfo.TargetDirectory);
                            try
                            {
                                Sync();
                            }
                            catch (CmisBaseException e)
                            {
                                this.ForceFullSyncAtNextSync();
                                Logger.Error("CMIS exception while syncing:", e);
                            }
                            catch(ObjectDisposedException e)
                            {
                                this.ForceFullSyncAtNextSync();
                                Logger.Warn("Object disposed while syncing:", e);
                            }
                            catch(Exception e)
                            {
                                this.ForceFullSyncAtNextSync();
                                Logger.Warn("Execption thrown while syncing:", e);
                            }
                        }
                    );
                    bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(
                        delegate(object o, RunWorkerCompletedEventArgs args)
                        {
                            lock(bgSyncingLock)
                            {
                                this.backgroundSyncing = false;
                            }
                        }
                    );
                    bw.RunWorkerAsync();
                }
            }


            /// <summary>
            /// Download all content from a CMIS folder.
            /// </summary>
            private bool RecursiveFolderCopy(IFolder remoteFolder, string localFolder)
            {
                bool success = true;

                try
                {
                    // List all children.
                    foreach (ICmisObject cmisObject in remoteFolder.GetChildren())
                    {
                        sleepWhileSuspended();
                        if (cmisObject is DotCMIS.Client.Impl.Folder)
                        {
                            IFolder remoteSubFolder = (IFolder)cmisObject;
                            string localSubFolder = localFolder + Path.DirectorySeparatorChar.ToString() + cmisObject.Name;
                            if (!Utils.IsInvalidFolderName(remoteFolder.Name, ConfigManager.CurrentConfig.IgnoreFolderNames) && !repoinfo.IsPathIgnored(remoteSubFolder.Path))
                            {
                                // Create local folder.
                                Logger.Info("Creating local directory: "+ localSubFolder);
                                Directory.CreateDirectory(localSubFolder);

                                // Create database entry for this folder
                                    // TODO Add metadata
                                database.AddFolder(localSubFolder, remoteSubFolder.Id, remoteSubFolder.LastModificationDate);

                                // Recurse into folder.
                                success = RecursiveFolderCopy(remoteSubFolder, localSubFolder) && success;
                            }
                        }
                        else
                        {
                            if (Utils.WorthSyncing(cmisObject.Name, ConfigManager.CurrentConfig.IgnoreFileNames))
                                // It is a file, just download it.
                                success = DownloadFile((IDocument)cmisObject, localFolder) && success;
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Warn(String.Format("Exception while download to local folder {0}: {1}", localFolder, Utils.ToLogString(e)));
                    success = false;
                }

                return success;
            }

            /// <summary>
            /// Download a single folder from the CMIS server for sync.
            /// </summary>
            private bool SyncDownloadFolder(IFolder remoteSubFolder, string localFolder)
            {
                sleepWhileSuspended();

                string name = remoteSubFolder.Name;
                string remotePathname = remoteSubFolder.Path;
                string localSubFolder = Path.Combine(localFolder, name);
                if(!Directory.Exists(localFolder))
                {
                    // The target folder has been removed/renamed => relaunch sync
                    Logger.Warn("The target folder has been removed/renamed: "+ localFolder);
                    return false;
                }

                if (Directory.Exists(localSubFolder))
                {
                    return true;
                }

                if (database.ContainsFolder(localSubFolder))
                {
                    // If there was previously a folder with this name, it means that
                    // the user has deleted it voluntarily, so delete it from server too.

                    // Delete the folder from the remote server.
                    Logger.Debug(String.Format("CMIS::DeleteTree({0})",remoteSubFolder.Path));
                    try{
                        remoteSubFolder.DeleteTree(true, null, true);
                        // Delete the folder from database.
                        database.RemoveFolder(localSubFolder);
                    }catch(Exception)
                    {
                        Logger.Info("Remote Folder could not be deleted: "+ remoteSubFolder.Path);
                        // Just go on and try it the next time
                    }
                }
                else
                {
                    // The folder has been recently created on server, so download it.

                    // If there was previously a file with this name, delete it.
                    // TODO warn if local changes in the file.
                    if (File.Exists(localSubFolder))
                    {
                        Logger.Warn("Local file \"" + localSubFolder + "\" has been renamed to \"" + localSubFolder + ".conflict\"");
                        File.Move(localSubFolder, localSubFolder + ".conflict");
                        this.Queue.AddEvent(new FileConflictEvent(FileConflictType.REMOTE_ADDED_PATH_CONFLICTS_LOCAL_FILE, localFolder, localSubFolder + ".conflict"));
                    }

                    // Skip if invalid folder name. See https://github.com/nicolas-raoul/CmisSync/issues/196
                    if (Utils.IsInvalidFolderName(name, ConfigManager.CurrentConfig.IgnoreFolderNames))
                    {
                        Logger.Info("Skipping download of folder with illegal name: " + name);
                    }
                    else if (repoinfo.IsPathIgnored(remotePathname))
                    {
                        Logger.Info("Skipping dowload of ignored folder: " + remotePathname);
                    }
                    else
                    {
                        // Create local folder.remoteDocument.Name
                        Logger.Info("Creating local directory: " + localSubFolder);
                        Directory.CreateDirectory(localSubFolder);

                        // Create database entry for this folder.
                        // TODO - Yannick - Add metadata
                        database.AddFolder(localSubFolder, remoteSubFolder.Id, remoteSubFolder.LastModificationDate);
                    }
                }

                return true;
            }


            /// <summary>
            /// Download a single file from the CMIS server for sync.
            /// </summary>
            private bool SyncDownloadFile(IDocument remoteDocument, string localFolder, IList<string> remoteFiles = null)
            {
                sleepWhileSuspended();

                string fileName = remoteDocument.Name;
                string filePath = Path.Combine(localFolder, fileName);

                if (null != remoteFiles)
                {
                    remoteFiles.Add(fileName);
                }

                // Check if file extension is allowed
                if (!Utils.WorthSyncing(fileName, ConfigManager.CurrentConfig.IgnoreFileNames))
                {
                    Logger.Info("Ignore the unworth syncing remote file: " + fileName);
                    return true;
                }

                bool success = true;

                try
                {
                    if (File.Exists(filePath))
                    {
                        // Check modification date stored in database and download if remote modification date if different.
                        DateTime? serverSideModificationDate = ((DateTime)remoteDocument.LastModificationDate).ToUniversalTime();
                        DateTime? lastDatabaseUpdate = database.GetServerSideModificationDate(filePath);

                        if (lastDatabaseUpdate == null)
                        {
                            Logger.Info("Downloading file absent from database: " + filePath);
                            success = DownloadFile(remoteDocument, localFolder);
                        }
                        else
                        {
                            // If the file has been modified since last time we downloaded it, then download again.
                            if (serverSideModificationDate > lastDatabaseUpdate)
                            {
                                Logger.Info("Downloading modified file: " + fileName);
                                success = DownloadFile(remoteDocument, localFolder);
                            }
                            else if(serverSideModificationDate == lastDatabaseUpdate)
                            {
                                //  check chunked upload
                                FileInfo fileInfo = new FileInfo(filePath);
                                if (remoteDocument.ContentStreamLength < fileInfo.Length)
                                {
                                    success = UpdateFile(filePath, remoteDocument);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (database.ContainsFile(filePath))
                        {
                            long retries = database.GetOperationRetryCounter(filePath, Database.OperationType.DELETE);
                            if(retries <= repoinfo.MaxDeletionRetries) {
                                // File has been recently removed locally, so remove it from server too.
                                Logger.Info("Removing locally deleted file on server: " + filePath);
                                try{
                                    remoteDocument.DeleteAllVersions();
                                    // Remove it from database.
                                    database.RemoveFile(filePath);
                                    database.SetOperationRetryCounter(filePath, 0, Database.OperationType.DELETE);
                                } catch(CmisBaseException ex)
                                {
                                    Logger.Warn("Could not delete remote file: ", ex);
                                    database.SetOperationRetryCounter(filePath, retries+1, Database.OperationType.DELETE);
                                    throw;
                                }
                            } else {
                                Logger.Info(String.Format("Skipped deletion of remote file {0} because of too many failed retries ({1} max={2})", filePath, retries, repoinfo.MaxDeletionRetries));
                            }
                        }
                        else
                        {
                            // New remote file, download it.
                            Logger.Info("New remote file: " + filePath);
                            success = DownloadFile(remoteDocument, localFolder);
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Warn(String.Format("Exception while download file to {0}: {1}", filePath, Utils.ToLogString(e)));
                    success = false;
                }

                return success;
            }

            private void RequestFileDownload(IDocument remoteDocument, string localFolder) {
                this.Queue.AddEvent(new FileDownloadRequest(remoteDocument, localFolder));
            }

            /// <summary>
            /// Download a single file from the CMIS server.
            /// </summary>
            private bool DownloadFile(IDocument remoteDocument, string localFolder)
            {
                sleepWhileSuspended();

                RequestFileDownload(remoteDocument, localFolder);
                using (new ActivityListenerResource(activityListener))
                {
                    string fileName = remoteDocument.Name;
                    string filepath = Path.Combine(localFolder, fileName);
                    string tmpfilepath = filepath + ".sync";
                    FileTransmissionEvent transmissionEvent = new FileTransmissionEvent(FileTransmissionType.DOWNLOAD_NEW_FILE, filepath, tmpfilepath);

                    // Skip if invalid file name. See https://github.com/nicolas-raoul/CmisSync/issues/196
                    if (Utils.IsInvalidFileName(fileName))
                    {
                        Logger.Info("Skipping download of file with illegal filename: " + fileName);
                        return true;
                    }

                    try
                    {
                        long failedCounter = database.GetOperationRetryCounter(filepath,Database.OperationType.DOWNLOAD);
                        bool truncate = false;
                        if( failedCounter > repoinfo.MaxDownloadRetries)
                        {
                            Logger.Info(String.Format("Skipping download of file {0} because of too many failed ({1}) downloads", filepath, failedCounter));
                            return true;
                        }
                        // Break and warn if download target exists as folder.
                        if (Directory.Exists(filepath))
                        {
                            throw new IOException(String.Format("Cannot download file \"{0}\", because a folder with this name exists locally", filepath));
                        }

                        // Break and warn if download target exists as folder.
                        if (Directory.Exists(tmpfilepath))
                        {
                            throw new IOException(String.Format("Cannot download file \"{0}\", because a folder with this name exists locally", tmpfilepath));
                        }

                        if (File.Exists(tmpfilepath))
                        {
                            DateTime? remoteDate = remoteDocument.LastModificationDate;
                            if (null == remoteDate)
                            {
                                truncate = true;
                            }
                            else
                            {
                                remoteDate = ((DateTime)remoteDate).ToUniversalTime();
                                DateTime? serverDate = database.GetDownloadServerSideModificationDate(filepath);
                                if (remoteDate != serverDate)
                                {
                                    truncate = true;
                                }
                            }
                        }
                        database.SetDownloadServerSideModificationDate(filepath, remoteDocument.LastModificationDate);
                        IFileDownloader downloader = ContentTaskUtils.CreateDownloader(repoinfo.DownloadChunkSize);

                        // Download file.
                        Boolean success = false;
                        byte[] filehash = { };

                        try
                        {
                            HashAlgorithm hashAlg = new SHA1Managed();
                            Logger.Debug("Creating local download file: " + tmpfilepath);
                            using (FileStream file = new FileStream(tmpfilepath, (truncate) ? FileMode.Create : FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read))
                            {
                                this.Queue.AddEvent(transmissionEvent);
                                transmissionEvent.ReportProgress(new TransmissionProgressEventArgs() { Started = true });
                                transmissionEvent.ReportProgress(new TransmissionProgressEventArgs() { Started = false });
                                downloader.DownloadFile(remoteDocument, file, transmissionEvent, hashAlg);
                                file.Close();
                            }
                            filehash = hashAlg.Hash;
                            success = true;
                        }
                        catch (ObjectDisposedException ex)
                        {
                            Logger.Error(String.Format("Download aborted by dispose: {0}", fileName), ex);
                            transmissionEvent.ReportProgress(new TransmissionProgressEventArgs() { Aborted = true, FailedException = ex });
                            return false;
                        }
                        catch (AbortException ex)
                        {
                            Logger.Error(String.Format("Download aborted by user: {0}", fileName), ex);
                            transmissionEvent.ReportProgress(new TransmissionProgressEventArgs() { Aborted = true, FailedException = ex });
                            return false;
                        }
                        catch (System.IO.DirectoryNotFoundException ex)
                        {
                            Logger.Warn(String.Format("Download failed because of a missing folder in the file path: {0}", ex.Message));
                            success = false;
                        }
                        catch (Exception ex)
                        {
                            Logger.Error("Download failed: " + fileName + " " + ex);
                            success = false;
                            Logger.Debug("Removing temp download file: " + tmpfilepath);
                            File.Delete(tmpfilepath);
                            success = false;
                            if (ex is CmisBaseException)
                            {
                                database.SetOperationRetryCounter(filepath, database.GetOperationRetryCounter(filepath, Database.OperationType.DOWNLOAD) + 1, Database.OperationType.DOWNLOAD);
                            }
                        }

                        if (success)
                        {
                            Logger.Info(String.Format("Downloaded remote object({0}): {1}", remoteDocument.Id, fileName));

                            // Get metadata.
                            Dictionary<string, string[]> metadata = null;
                            try
                            {
                                metadata = CmisUtils.FetchMetadata(remoteDocument, session.GetTypeDefinition(remoteDocument.ObjectType.Id));
                            }
                            catch (Exception e)
                            {
                                Logger.Info("Exception while fetching metadata: " + fileName + " " + Utils.ToLogString(e));
                                // Remove temporary local document to avoid it being considered a new document.
                                Logger.Debug("Removing local temp file: " + tmpfilepath);
                                File.Delete(tmpfilepath);
                                transmissionEvent.ReportProgress(new TransmissionProgressEventArgs(){Aborted = true, FailedException = e});
                                return false;
                            }

                            // If file exists, check it.
                            if (File.Exists(filepath))
                            {
                                if (database.LocalFileHasChanged(filepath))
                                {
                                    Logger.Info("Conflict with file: " + fileName + ", backing up locally modified version and downloading server version");
                                    // Rename locally modified file.
                                    //String ext = Path.GetExtension(filePath);
                                    //String filename = Path.GetFileNameWithoutExtension(filePath);
                                    String dir = Path.GetDirectoryName(filepath);

                                    String newFileName = Utils.FindNextConflictFreeFilename(filepath, repoinfo.User);
                                    String newFilePath = Path.Combine(dir, newFileName);
                                    Logger.Debug(String.Format("Moving local file {0} file to new file {1}", filepath, newFilePath));
                                    File.Move(filepath, newFilePath);
                                    Queue.AddEvent(new FileConflictEvent(FileConflictType.CONTENT_MODIFIED,dir,newFilePath));
                                    Logger.Debug(String.Format("Moving temporary local download file {0} to target file {1}", tmpfilepath, filepath));
                                    File.Move(tmpfilepath, filepath);
                                    CmisUtils.SetLastModifiedDate(remoteDocument, filepath, metadata);
                                    Queue.AddEvent(new RecentChangedEvent(filepath));
                                    repo.OnConflictResolved();
                                }
                                else
                                {
                                    Logger.Debug("Removing local file: " + filepath);
                                    File.Delete(filepath);
                                    Logger.Debug(String.Format("Moving temporary local download file {0} to target file {1}", tmpfilepath, filepath));
                                    File.Move(tmpfilepath, filepath);
                                    CmisUtils.SetLastModifiedDate(remoteDocument, filepath, metadata);
                                }
                            }
                            else
                            {
                                Logger.Debug(String.Format("Moving temporary local download file {0} to target file {1}", tmpfilepath, filepath));
                                File.Move(tmpfilepath, filepath);
                                CmisUtils.SetLastModifiedDate(remoteDocument, filepath, metadata);
                            }

                            // Create database entry for this file.
                            database.AddFile(filepath, remoteDocument.Id, remoteDocument.LastModificationDate, metadata, filehash);
                            Queue.AddEvent(new RecentChangedEvent(filepath, remoteDocument.LastModificationDate));

                            Logger.Debug("Added to database: " + fileName);
                        }

                        if (success)
                        {
                            transmissionEvent.ReportProgress(new TransmissionProgressEventArgs() { Completed = true });
                        }
                        else
                        {
                            transmissionEvent.ReportProgress(new TransmissionProgressEventArgs() { Aborted = true });
                        }
                        return success;
                    }
                    catch (IOException e)
                    {
                        Logger.Warn("Exception while file operation: " + Utils.ToLogString(e));
                        transmissionEvent.ReportProgress(new TransmissionProgressEventArgs(){Aborted = true, FailedException = e});
                        return false;
                    }
                }
            }

            /// <summary>
            /// Upload a single file to the CMIS server.
            /// </summary>
            private bool UploadFile(string filePath, IFolder remoteFolder)
            {
                sleepWhileSuspended();

                using (new ActivityListenerResource(activityListener))
                {
                    long retries = database.GetOperationRetryCounter(filePath, Database.OperationType.UPLOAD);
                    if(retries > this.repoinfo.MaxUploadRetries) {
                        Logger.Info(String.Format("Skipping uploading file absent on repository, because of too many failed retries({0}): {1}", retries, filePath));
                        return true;
                    }
                    FileTransmissionEvent transmissionEvent = new FileTransmissionEvent(FileTransmissionType.UPLOAD_NEW_FILE, filePath);
                    try{
                        IDocument remoteDocument = null;
                        Boolean success = false;
                        byte[] filehash = { };
                        try
                        {
                            Logger.Info("Uploading: " + filePath);
                            this.Queue.AddEvent(transmissionEvent);
                            transmissionEvent.ReportProgress(new TransmissionProgressEventArgs(){Started = true});
                            transmissionEvent.ReportProgress(new TransmissionProgressEventArgs(){Started = false});
                            // Prepare properties
                            string fileName = Path.GetFileName(filePath);
                            Dictionary<string, object> properties = new Dictionary<string, object>();
                            properties.Add(PropertyIds.Name, fileName);
                            properties.Add(PropertyIds.ObjectTypeId, "cmis:document");
                            properties.Add(PropertyIds.CreationDate, ((long)(File.GetCreationTimeUtc(filePath) - new DateTime(1970, 1, 1)).TotalMilliseconds).ToString());

                            // Prepare content stream
                            using (SHA1 hashAlg = new SHA1Managed())
                            using (Stream file = File.OpenRead(filePath))
                            {
                                IFileUploader uploader = ContentTaskUtils.CreateUploader(repoinfo.ChunkSize);

                                try {
                                    try {
                                        // Create Document
                                        remoteDocument = remoteFolder.CreateDocument(properties, null, null);
                                        Logger.Debug(String.Format("CMIS::Document Id={0} Name={1}",
                                                                   remoteDocument.Id, fileName));
                                        Dictionary<string, string[]> metadata = CmisUtils.FetchMetadata(remoteDocument, session.GetTypeDefinition(remoteDocument.ObjectType.Id));
                                        // Create database entry for this empty file to force content update if setContentStream will fail.
                                        database.AddFile(filePath, remoteDocument.Id, remoteDocument.LastModificationDate, metadata, new byte[hashAlg.HashSize]);
                                    } catch(Exception) {
                                        string reason = Utils.IsValidISO88591(fileName)?String.Empty:" Reason: Upload perhaps failed because of an invalid ISO 8859-1 character";
                                        Logger.Info(String.Format("Could not create the remote document {0} as target for local document {1}{2}", fileName, filePath, reason));
                                        throw;
                                    }
                                    // Upload
                                    try{
                                        IDocument lastState;
                                        if(uploadProgresses.TryGetValue(filePath, out lastState))
                                        {
                                            if(lastState.ChangeToken == remoteDocument.ChangeToken && lastState.ContentStreamLength != null)
                                            {
                                                ContentTaskUtils.PrepareResume((long)lastState.ContentStreamLength, file, hashAlg);
                                            } else {
                                                uploadProgresses.Remove(filePath);
                                            }
                                        }
                                        uploader.UploadFile(remoteDocument, file, transmissionEvent, hashAlg);
                                    }catch(UploadFailedException uploadException) {
                                        // Check if upload was partly successful and save this state
                                        if(!uploadException.LastSuccessfulDocument.Equals(remoteDocument)) {
                                            uploadProgresses.Add(filePath, uploadException.LastSuccessfulDocument);
                                        }
                                        throw;
                                    }
                                    uploadProgresses.Remove(filePath);
                                    filehash = hashAlg.Hash;
                                    success = true;
                                }catch (Exception ex) {
                                    Logger.Fatal("Upload failed: " + filePath + " " + ex);
                                    throw;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            if (e is FileNotFoundException ||
                                e is IOException)
                            {
                                Logger.Warn("File deleted while trying to upload it, reverting.");
                                // File has been deleted while we were trying to upload/checksum/add.
                            // This can typically happen in Windows Explore when creating a new text file and giving it a name.
                            // In this case, revert the upload.
                                if (remoteDocument != null)
                                {
                                    remoteDocument.DeleteAllVersions();
                                }
                            }
                            else
                            {
                                throw;
                            }
                        }

                        // Metadata.
                        if (success)
                        {
                            Logger.Info("Uploaded: " + filePath);

                            // Get metadata. Some metadata has probably been automatically added by the server.
                            Dictionary<string, string[]> metadata = CmisUtils.FetchMetadata(remoteDocument, session.GetTypeDefinition(remoteDocument.ObjectType.Id));

                            // Create database entry for this file.
                            database.AddFile(filePath, remoteDocument.Id, remoteDocument.LastModificationDate, metadata, filehash);
                            CmisUtils.SetLastModifiedDate(remoteDocument, filePath, metadata);
                            Queue.AddEvent(new RecentChangedEvent(filePath));
                            transmissionEvent.ReportProgress(new TransmissionProgressEventArgs(){Completed = true});
                        } else {
                            transmissionEvent.ReportProgress(new TransmissionProgressEventArgs(){Aborted = true});
                        }
                        return success;
                    }
                    catch(Exception e)
                    {
                        retries++;
                        database.SetOperationRetryCounter(filePath, retries, Database.OperationType.UPLOAD);
                        Logger.Warn(String.Format("Uploading of {0} failed {1} times: ", filePath, retries), e);
                        transmissionEvent.ReportProgress(new TransmissionProgressEventArgs(){ Aborted=true, FailedException = e});
                        return false;
                    }
                }
            }


            /// <summary>
            /// Upload folder recursively.
            /// After execution, the hierarchy on server will be: .../remoteBaseFolder/localFolder/...
            /// </summary>
            private bool UploadFolderRecursively(IFolder remoteBaseFolder, string localFolder)
            {
                sleepWhileSuspended();

                // Create remote folder.
                Dictionary<string, object> properties = new Dictionary<string, object>();
                properties.Add(PropertyIds.Name, Path.GetFileName(localFolder));
                properties.Add(PropertyIds.ObjectTypeId, "cmis:folder");
                properties.Add(PropertyIds.CreationDate, "");
                properties.Add(PropertyIds.LastModificationDate,"");
                IFolder folder = null;
                try
                {
                    Logger.Debug(String.Format("Creating remote folder {0} for local folder {1}", Path.GetFileName(localFolder), localFolder));
                    folder = remoteBaseFolder.CreateFolder(properties);
                    Logger.Debug(String.Format("Created remote folder {0}({1}) for local folder {2}", Path.GetFileName(localFolder), folder.Id ,localFolder));
                }
                catch (CmisNameConstraintViolationException)
                {
                    foreach (ICmisObject cmisObject in remoteBaseFolder.GetChildren())
                    {
                        if (cmisObject.Name == Path.GetFileName(localFolder))
                        {
                            folder = cmisObject as IFolder;
                        }
                    }
                    if (folder == null)
                    {
                        Logger.Warn("Remote file conflict with local folder " + Path.GetFileName(localFolder));
//                        Queue.AddEvent(new FileConflictEvent(FileConflictType.ALREADY_EXISTS_REMOTELY, ));
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warn(String.Format("Exception when create remote folder for local folder {0}: {1}", localFolder, Utils.ToLogString(ex)));
                    return false;
                }

                // Create database entry for this folder
                // TODO Add metadata
                database.AddFolder(localFolder, folder.Id, folder.LastModificationDate);
                CmisUtils.SetLastModifiedDate(folder,localFolder, CmisUtils.FetchMetadata(folder, session.GetTypeDefinition(folder.ObjectType.Id)));
                bool success = true;
                try
                {
                    // Upload each file in this folder.
                    foreach (string file in Directory.GetFiles(localFolder))
                    {
                        if (Utils.WorthSyncing(file.Substring(file.LastIndexOf(Path.DirectorySeparatorChar)+1), ConfigManager.CurrentConfig.IgnoreFileNames))
                        {
                            Logger.Debug(String.Format("Invoke upload file {0} of folder {1}", file, localFolder));
                            success = UploadFile(file, folder) && success;
                        }
                    }

                    // Recurse for each subfolder in this folder.
                    foreach (string subfolder in Directory.GetDirectories(localFolder))
                    {
                        string path = subfolder.Substring(repoinfo.TargetDirectory.Length);
                        path = path.Replace("\\\\","/");
                        if (!Utils.IsInvalidFolderName(Path.GetFileName(subfolder), ConfigManager.CurrentConfig.IgnoreFolderNames) && !repoinfo.IsPathIgnored(path))
                        {
                            Logger.Debug("Start recursive upload of folder: " + subfolder);
                            success = UploadFolderRecursively(folder, subfolder) && success;
                        }
                    }
                }
                catch (Exception e)
                {
                    if (e is System.IO.DirectoryNotFoundException ||
                        e is IOException)
                    {
                        Logger.Warn("Folder deleted while trying to upload it, reverting.");
                        // Folder has been deleted while we were trying to upload/checksum/add.
                        // In this case, revert the upload.
                        folder.DeleteTree(true, null, true);
                    }
                    else
                    {
                        Logger.Warn("Exception on recursiv upload of folder: " + localFolder);
                        return false;
                    }
                }

                return success;
            }


            /// <summary>
            /// Upload new version of file.
            /// </summary>
            private bool UpdateFile(string filePath, IDocument remoteFile)
            {
                long retries = database.GetOperationRetryCounter(filePath, Database.OperationType.UPLOAD);
                if(retries >= repoinfo.MaxUploadRetries)
                {
                    Logger.Info(String.Format("Skipping updating file content on repository, because of too many failed retries({0}): {1}", retries, filePath));
                    return true;
                }
                FileTransmissionEvent transmissionEvent = new FileTransmissionEvent(FileTransmissionType.UPLOAD_MODIFIED_FILE, filePath);
                try
                {
                    Logger.Info("## Updating " + filePath);
                    using (Stream localfile = File.OpenRead(filePath))
                    {
                        // Ignore files with null or empty content stream.
                        if ((localfile == null) && (localfile.Length == 0))
                        {
                            Logger.Info("Skipping update of file with null or empty content stream: " + filePath);
                            return true;
                        }
                        this.Queue.AddEvent(transmissionEvent);
                        transmissionEvent.ReportProgress(new TransmissionProgressEventArgs(){Started = true});
                        transmissionEvent.ReportProgress(new TransmissionProgressEventArgs(){Started = false});
                        IFileUploader uploader = ContentTaskUtils.CreateUploader(repoinfo.ChunkSize);
                        using (var hashAlg = new SHA1Managed()) {
                            IDocument lastState;
                            if(uploadProgresses.TryGetValue(filePath, out lastState)){
                                if(lastState.ChangeToken == remoteFile.ChangeToken && lastState.ContentStreamLength != null) {
                                    ContentTaskUtils.PrepareResume((long) lastState.ContentStreamLength, localfile, hashAlg);
                                } else {
                                    uploadProgresses.Remove(filePath);
                                }
                            }
                            try{
                                uploader.UploadFile(remoteFile,localfile,transmissionEvent, hashAlg);
                                uploadProgresses.Remove(filePath);
                            }catch(UploadFailedException uploadException){
                                if(!uploadException.LastSuccessfulDocument.Equals(remoteFile)) {
                                    uploadProgresses.Add(filePath, uploadException.LastSuccessfulDocument);
                                }
                                throw;
                            }
                        }
                        // Update timestamp in database.
                        database.SetFileServerSideModificationDate(filePath, ((DateTime)remoteFile.LastModificationDate).ToUniversalTime());

                        // Update checksum
                        database.RecalculateChecksum(filePath);
                        transmissionEvent.ReportProgress(new TransmissionProgressEventArgs(){Completed = true});

                        // TODO Update metadata?
                    }
                    return true;
                }
                catch (Exception e)
                {
                    retries++;
                    database.SetOperationRetryCounter(filePath, retries, Database.OperationType.UPLOAD);
                    transmissionEvent.ReportProgress(new TransmissionProgressEventArgs() { Aborted = true, FailedException = e });
                    Logger.Warn(String.Format("Updating content of {0} failed {1} times: ", filePath, retries), e);
                    return false;
                }
            }

            /// <summary>
            /// Upload new version of file content.
            /// </summary>
            private bool UpdateFile(string filePath, IFolder remoteFolder)
            {
                using (new ActivityListenerResource(activityListener))
                {
                    Logger.Info("# Updating " + filePath);

                    // Find the document within the folder.
                    string fileName = Path.GetFileName(filePath);
                    IDocument document = null;
                    bool found = false;
                    foreach (ICmisObject obj in remoteFolder.GetChildren())
                    {
                        if (null != (document = obj as IDocument))
                        {
                            if (document.Name == fileName)
                            {
                                found = true;
                                break;
                            }
                        }
                    }

                    // If not found, it means the document has been deleted.
                    if (!found)
                    {
                        Logger.Info(filePath + " not found on server, must be uploaded instead of updated");
                        return UploadFile(filePath, remoteFolder);
                    }

                    // Update the document itself.
                    bool success = UpdateFile(filePath, document);

                    Logger.Info("# Updated " + filePath);

                    return success;
                }
            }


            /// <summary>
            /// Move folder from local filesystem and database.
            /// </summary>
            private void MoveFolderLocally(string oldFolderPath, string newFolderPath)
            {
                if (!Directory.Exists(oldFolderPath))
                {
                    return;
                }

                if (!Directory.Exists(newFolderPath))
                {
                    Directory.Move(oldFolderPath, newFolderPath);
                    database.MoveFolder(oldFolderPath, newFolderPath);
                    return;
                }

                foreach (FileInfo file in new DirectoryInfo(oldFolderPath).GetFiles())
                {
                    string oldFilePath = Path.Combine(oldFolderPath, file.Name);
                    string newFilePath = Path.Combine(newFolderPath, file.Name);
                    if (File.Exists(newFilePath))
                    {
                        // TODO Check file content before deleting anything!
                        File.Delete(oldFilePath);
                        database.RemoveFile(oldFilePath);
                    }
                    else
                    {
                        File.Move(oldFilePath, newFilePath);
                        database.MoveFile(oldFilePath, newFilePath);
                    }
                }

                foreach (DirectoryInfo folder in new DirectoryInfo(oldFolderPath).GetDirectories())
                {
                    MoveFolderLocally(Path.Combine(oldFolderPath, folder.Name), Path.Combine(newFolderPath, folder.Name));
                }

                Directory.Delete(oldFolderPath, true);
                database.RemoveFolder(oldFolderPath);

                return;
            }


            /// <summary>
            /// Remove folder from local filesystem and database.
            /// </summary>
            private bool RemoveFolderLocally(string folderPath)
            {
                // Folder has been deleted on server, delete it locally too.
                try
                {
                    Logger.Info("Removing remotely deleted folder: " + folderPath);
                    Directory.Delete(folderPath, true);
                }
                catch (IOException e)
                {
                    Logger.Warn(String.Format("Exception while delete tree {0}: {1}", folderPath, Utils.ToLogString(e)));
                    return false;
                }

                // Delete folder from database.
                if (!Directory.Exists(folderPath))
                {
                    database.RemoveFolder(folderPath);
                }

                return true;
            }
        }
    }
}
