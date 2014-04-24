//   CmisSync, a CMIS synchronization tool.
//   Copyright (C) 2012  Nicolas Raoul <nicolas.raoul@aegif.jp>
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
using DotCMIS.Client;
using DotCMIS.Client.Impl;
using DotCMIS;
using DotCMIS.Enums;
using DotCMIS.Exceptions;

namespace CmisSync.Lib.Sync
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using CmisSync.Lib.Cmis;
    using CmisSync.Lib.Events;

    using log4net;

    /// <summary>
    /// Current status of the synchronization.
    /// </summary>
    public enum SyncStatus
    {
        /// <summary>
        /// Normal operation.
        /// </summary>
        Idle,

        /// <summary>
        /// Synchronization is suspended.
        /// </summary>
        Suspend,

        /// <summary>
        /// Any sync conflict or warning happend
        /// </summary>
        Warning
    }

    public class CmisRepo : IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(CmisRepo));

        /// <summary>
        /// Affect a new <c>SyncStatus</c> value.
        /// </summary>
        public Action<SyncStatus> SyncStatusChanged { get; set; }

        /// <summary>
        /// Current status of the synchronization (paused or not).
        /// </summary>
        public SyncStatus Status { get; private set; }

        /// <summary>
        /// Gets the polling scheduler.
        /// </summary>
        /// <value>
        /// The scheduler.
        /// </value>
        public SyncScheduler Scheduler { get; private set; }

        /// <summary>
        /// Event Queue for this repository.
        /// Use this to notifiy events for this repository.
        /// </summary>
        public SyncEventQueue Queue { get; private set; }

        /// <summary>
        /// Event Manager for this repository.
        /// Use this for adding and removing SyncEventHandler for this repository.
        /// </summary>
        public SyncEventManager EventManager { get; private set; }

        /// <summary>
        /// Return the synchronized folder's information.
        /// </summary>
        protected RepoInfo RepoInfo { get; set; }

        /// <summary>
        /// Path of the local synchronized folder.
        /// </summary>
        public readonly string LocalPath;


        /// <summary>
        /// Name of the synchronized folder, as found in the CmisSync XML configuration file.
        /// </summary>
        public readonly string Name;


        /// <summary>
        /// URL of the remote CMIS endpoint.
        /// </summary>
        public readonly Uri RemoteUrl;

        
        /// <summary>
        /// Whether stopped, to control for machine sleep/wake power management.
        /// </summary>
        public bool Stopped { get; set; }

        /// <summary>
        /// Stop syncing momentarily.
        /// </summary>
        public void Suspend()
        {
            Status = SyncStatus.Suspend;
        }

        /// <summary>
        /// Restart syncing.
        /// </summary>
        public void Resume()
        {
            Status = SyncStatus.Idle;
        }

        /// <summary>
        /// Watches the local filesystem for changes.
        /// </summary>
        public CmisSync.Lib.Sync.Strategy.Watcher Watcher { get; private set; }

        /// <summary>
        /// The ignored folders filter.
        /// </summary>
        private Events.Filter.IgnoredFoldersFilter ignoredFoldersFilter;

        /// <summary>
        /// The ignored file name filter.
        /// </summary>
        private Events.Filter.IgnoredFileNamesFilter ignoredFileNameFilter;

        /// <summary>
        /// The ignored folder name filter.
        /// </summary>
        private Events.Filter.IgnoredFolderNameFilter ignoredFolderNameFilter;

        /// <summary>
        /// Track whether <c>Dispose</c> has been called.
        /// </summary>
        private bool disposed = false;

        /// <summary>
        /// The auth provider.
        /// </summary>
        private IDisposableAuthProvider authProvider;

        /// <summary>
        /// Session to the CMIS repository.
        /// </summary>
        private ISession session;

        /// <summary>
        /// The session factory.
        /// </summary>
        private SessionFactory sessionFactory;


        /// <summary>
        /// Constructor.
        /// </summary>
        public CmisRepo(RepoInfo repoInfo, IActivityListener activityListener)
        {
            this.sessionFactory = SessionFactory.NewInstance();

            this.authProvider = AuthProviderFactory.CreateAuthProvider(repoInfo.AuthType, repoInfo.Address, null);
            EventManager = new SyncEventManager(repoInfo.Name);
            EventManager.AddEventHandler(new DebugLoggingHandler());
            Queue = new SyncEventQueue(EventManager);
            RepoInfo = repoInfo;
            LocalPath = repoInfo.TargetDirectory;
            Name = repoInfo.Name;
            RemoteUrl = repoInfo.Address;
            ignoredFoldersFilter = new Events.Filter.IgnoredFoldersFilter(Queue){IgnoredPaths= new List<string>(repoInfo.GetIgnoredPaths())};
            ignoredFileNameFilter = new Events.Filter.IgnoredFileNamesFilter(Queue){Wildcards = ConfigManager.CurrentConfig.IgnoreFileNames};
            ignoredFolderNameFilter = new Events.Filter.IgnoredFolderNameFilter(Queue) {Wildcards = ConfigManager.CurrentConfig.IgnoreFolderNames};
            EventManager.AddEventHandler(ignoredFoldersFilter);
            EventManager.AddEventHandler(ignoredFileNameFilter);
            EventManager.AddEventHandler(ignoredFolderNameFilter);
            EventManager.AddEventHandler(new GenericSyncEventHandler<RepoConfigChangedEvent>(0, RepoInfoChanged));
            Scheduler = new SyncScheduler(Queue, repoInfo.PollInterval);
            EventManager.AddEventHandler(Scheduler);

            SyncStatusChanged += delegate(SyncStatus status)
            {
                Status = status;
            };
            #if __COCOA__
            this.Watcher = new CmisSync.Lib.Sync.Strategy.MacWatcher(LocalPath, Queue);
            #else
            this.Watcher = new CmisSync.Lib.Sync.Strategy.NetWatcher( new FileSystemWatcher(LocalPath), Queue);
            #endif

            this.Watcher.EnableEvents = true;
        }

        private bool RepoInfoChanged(ISyncEvent e)
        {
            if (e is RepoConfigChangedEvent)
            {
                this.RepoInfo = (e as RepoConfigChangedEvent).RepoInfo;
                this.ignoredFoldersFilter.IgnoredPaths = new List<string>(this.RepoInfo.GetIgnoredPaths());
                this.ignoredFileNameFilter.Wildcards = ConfigManager.CurrentConfig.IgnoreFileNames;
                this.ignoredFolderNameFilter.Wildcards = ConfigManager.CurrentConfig.IgnoreFolderNames;
                return true;
            }
            else
            {
                // This should never ever happen!
                return false;
            }
        }

        /// <summary>
        /// Destructor.
        /// </summary>
        ~CmisRepo()
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
            if (!this.disposed)
            {
                if (disposing)
                {
                    this.Scheduler.Dispose();
                    this.Watcher.Dispose();
                    this.Queue.StopListener();
                    int timeout = 500;
                    if(!this.Queue.WaitForStopped(timeout))
                        Logger.Debug(String.Format("Event Queue is of {0} has not been closed in {1} miliseconds", RemoteUrl.ToString(), timeout));
                    this.Queue.Dispose();
                }
                this.disposed = true;
            }
        }

        /// <summary>
        /// Initialize the scheduled background sync processes.
        /// </summary>
        public void Initialize()
        {
            this.Connect();


            // Sync up everything that changed
            // since we've been offline
            // start full crawl sync on beginning
            this.Queue.AddEvent(new StartNextSyncEvent(true));

            // start scheduler for event based sync mechanisms
            this.Scheduler.Start();
        }

        /// <summary>
        /// Connect to the CMIS repository.
        /// </summary>
        private void Connect()
        {
            using(log4net.ThreadContext.Stacks["NDC"].Push("Connect"))
            {
                try
                {
                    // Create session.
                    this.session = this.sessionFactory.CreateSession(this.GetCmisParameter(this.RepoInfo), null, this.authProvider, null);

                    this.session.DefaultContext = this.CreateDefaultContext();
                    this.Queue.AddEvent(new SuccessfulLoginEvent(this.RepoInfo.Address));
                    //reconnect = false;
                }
                catch (DotCMIS.Exceptions.CmisPermissionDeniedException e)
                {
                    Logger.Info(string.Format("Failed to connect to server {0}", this.RepoInfo.Address.AbsoluteUri), e);
                    this.Queue.AddEvent(new PermissionDeniedEvent(e));
                }
                catch (CmisRuntimeException e)
                {
                    if(e.Message == "Proxy Authentication Required")
                    {
                        this.Queue.AddEvent(new ProxyAuthRequiredEvent(e));
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
                    Logger.Error("Failed to create session to remote " + this.RepoInfo.Address.ToString() + ": ", e);
                }
            }
        }

        private Config.SyncConfig.Folder GetFolderConfig()
        {
            return ConfigManager.CurrentConfig.getFolder(this.RepoInfo.Name);
        }

        /// <summary>
        /// Detect whether the repository has the ChangeLog capability.
        /// </summary>
        /// <returns>
        /// <c>true</c> if this feature is available, otherwise <c>false</c>
        /// </returns>
        private bool AreChangeEventsSupported()
        {
            try
            {
                return (this.session.RepositoryInfo.Capabilities.ChangesCapability == CapabilityChanges.All ||
                        this.session.RepositoryInfo.Capabilities.ChangesCapability == CapabilityChanges.ObjectIdsOnly) &&
                    this.GetFolderConfig().SupportedFeatures.GetContentChangesSupport != false;
            }
            catch(NullReferenceException)
            {
                return false;
            }
        }

        private bool IsGetDescendantsSupported()
        {
            try
            {
                return this.session.RepositoryInfo.Capabilities.IsGetDescendantsSupported != false && this.GetFolderConfig().SupportedFeatures.GetDescendantsSupport == true;
            }
            catch(NullReferenceException)
            {
                return false;
            }
        }

        private IOperationContext CreateDefaultContext()
        {
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
            return this.session.CreateOperationContext(filters, false, true, false, IncludeRelationshipsFlag.None, renditions, true, null, true, 100);
        }

        /// <summary>
        /// Parameter to use for all CMIS requests.
        /// </summary>
        /// <returns>
        /// The cmis parameter.
        /// </returns>
        /// <param name='repoInfo'>
        /// The repository infos.
        /// </param>
        private Dictionary<string, string> GetCmisParameter(RepoInfo repoInfo)
        {
            Dictionary<string, string> cmisParameters = new Dictionary<string, string>();
            cmisParameters[SessionParameter.BindingType] = BindingType.AtomPub;
            cmisParameters[SessionParameter.AtomPubUrl] = repoInfo.Address.ToString();
            cmisParameters[SessionParameter.User] = repoInfo.User;
            cmisParameters[SessionParameter.Password] = repoInfo.Password.ToString();
            cmisParameters[SessionParameter.RepositoryId] = repoInfo.RepoID;

            // Sets the Connect Timeout to infinite
            cmisParameters[SessionParameter.ConnectTimeout] = "-1";

            // Sets the Read Timeout to infinite
            cmisParameters[SessionParameter.ReadTimeout] = "-1";
            cmisParameters[SessionParameter.DeviceIdentifier] = ConfigManager.CurrentConfig.DeviceId.ToString();
            cmisParameters[SessionParameter.UserAgent] = Utils.CreateUserAgent();
            cmisParameters[SessionParameter.Compression] = bool.TrueString;
            return cmisParameters;
        }
    }
}
