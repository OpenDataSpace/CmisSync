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
        public class SynchronizedFolder : IDisposable
        {
            // Log
            private static readonly ILog Logger = LogManager.GetLogger(typeof(SynchronizedFolder));

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
            /// Track whether <c>Dispose</c> has been called.
            /// </summary>
            private bool disposed = false;

            /// <summary>
            /// Track whether <c>Dispose</c> has been called.
            /// </summary>
            private Object disposeLock = new Object();

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

            /// <summary>
            /// The auth provider.
            /// </summary>
            private PersistentStandardAuthenticationProvider authProvider;

            /// <summary>
            /// EventQueue
            /// </summary>
            public SyncEventQueue Queue {get; private set;}

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
                        }
                        this.disposed = true;
                    }
                }
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
        }
    }
}
