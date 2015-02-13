//-----------------------------------------------------------------------
// <copyright file="Repository.cs" company="GRAU DATA AG">
//
//   Copyright (C) 2012  Nicolas Raoul &lt;nicolas.raoul@aegif.jp&gt;
//   Copyright (C) 2014 GRAU DATA &lt;info@graudata.com&gt;
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

namespace CmisSync.Lib.Cmis {
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;

    using CmisSync.Lib;
    using CmisSync.Lib.Accumulator;
    using CmisSync.Lib.Cmis;
    using CmisSync.Lib.Config;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Filter;
    using CmisSync.Lib.PathMatcher;
    using CmisSync.Lib.Producer.ContentChange;
    using CmisSync.Lib.Producer.Watcher;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.SelectiveIgnore;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;

    using DBreeze;

    using DotCMIS;
    using DotCMIS.Client;
    using DotCMIS.Client.Impl;
    using DotCMIS.Enums;
    using DotCMIS.Exceptions;

    using log4net;

    /// <summary>
    /// Current status of the synchronization.
    /// </summary>
    public enum SyncStatus {
        /// <summary>
        /// Normal operation.
        /// </summary>
        Idle,

        /// <summary>
        /// Synchronization is suspended.
        /// </summary>
        Suspend,

        /// <summary>
        /// Connection is offline.
        /// </summary>
        Offline,

        /// <summary>
        /// Actually changes are synchronized.
        /// </summary>
        Synchronizing,

        /// <summary>
        /// Any sync conflict or warning happend
        /// </summary>
        Warning
    }

    /// <summary>
    /// Synchronized CMIS repository.
    /// </summary>
    public class Repository : IDisposable, INotifyPropertyChanged {
        /// <summary>
        /// Name of the synchronized folder, as found in the CmisSync XML configuration file.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// URL of the remote CMIS endpoint.
        /// </summary>
        public readonly Uri RemoteUrl;

        /// <summary>
        /// Path of the local synchronized folder.
        /// </summary>
        public readonly string LocalPath;

        /// <summary>
        /// The storage.
        /// </summary>
        protected MetaDataStorage storage;

        /// <summary>
        /// The file transmission storage.
        /// </summary>
        protected FileTransmissionStorage fileTransmissionStorage;

        /// <summary>
        /// The connection scheduler.
        /// </summary>
        protected ConnectionScheduler connectionScheduler;

        private static readonly ILog Logger = LogManager.GetLogger(typeof(Repository));

        /// <summary>
        /// The ignored folders filter.
        /// </summary>
        private IgnoredFoldersFilter ignoredFoldersFilter;

        /// <summary>
        /// The ignored file name filter.
        /// </summary>
        private IgnoredFileNamesFilter ignoredFileNameFilter;

        /// <summary>
        /// The ignored folder name filter.
        /// </summary>
        private IgnoredFolderNameFilter ignoredFolderNameFilter;

        /// <summary>
        /// The invalid folder name filter.
        /// </summary>
        private InvalidFolderNameFilter invalidFolderNameFilter;

        /// <summary>
        /// The already added objects filter.
        /// </summary>
        private IgnoreAlreadyHandledFsEventsFilter alreadyAddedFilter;

        /// <summary>
        /// The reporting filter.
        /// </summary>
        private ReportingFilter reportingFilter;

        private FilterAggregator filters;

        private IIgnoredEntitiesStorage ignoredStorage;

        /// <summary>
        /// Track whether <c>Dispose</c> has been called.
        /// </summary>
        private bool disposed = false;

        /// <summary>
        /// The auth provider.
        /// </summary>
        private IDisposableAuthProvider authProvider;

        /// <summary>
        /// The session factory.
        /// </summary>
        private ISessionFactory sessionFactory;

        private ContentChangeEventTransformer transformer;

        private DBreezeEngine db;

        private IFileSystemInfoFactory fileSystemFactory;

        private ActivityListenerAggregator activityListener;

        static Repository() {
            DBreezeInitializerSingleton.Init();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Repository"/> class.
        /// </summary>
        /// <param name="repoInfo">Repo info.</param>
        /// <param name="activityListener">Activity listener.</param>
        public Repository(RepoInfo repoInfo, ActivityListenerAggregator activityListener) : this(repoInfo, activityListener, false, CreateQueue()) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Repository"/> class.
        /// </summary>
        /// <param name="repoInfo">Repo info.</param>
        /// <param name="activityListener">Activity listener.</param>
        /// <param name="inMemory">If set to <c>true</c> in memory.</param>
        /// <param name="queue">Event Queue.</param>
        protected Repository(RepoInfo repoInfo, ActivityListenerAggregator activityListener, bool inMemory, ICountingQueue queue) {
            if (repoInfo == null) {
                throw new ArgumentNullException("Given repoInfo is null");
            }

            if (activityListener == null) {
                throw new ArgumentNullException("Given activityListener is null");
            }

            this.fileSystemFactory = new FileSystemInfoFactory();
            this.activityListener = activityListener;

            // Initialize local variables
            this.RepoInfo = repoInfo;
            this.LocalPath = repoInfo.LocalPath;
            this.Name = repoInfo.DisplayName;
            this.RemoteUrl = repoInfo.Address;

            if (!this.fileSystemFactory.CreateDirectoryInfo(this.LocalPath).IsExtendedAttributeAvailable()) {
                throw new ExtendedAttributeException("Extended Attributes are not available on the local path: " + this.LocalPath);
            }

            this.Queue = queue;
            this.Queue.EventManager.AddEventHandler(new DebugLoggingHandler());

            // Create Database connection
            this.db = new DBreezeEngine(new DBreezeConfiguration {
                DBreezeDataFolderName = inMemory ? string.Empty : repoInfo.GetDatabasePath(),
                Storage = inMemory ? DBreezeConfiguration.eStorage.MEMORY : DBreezeConfiguration.eStorage.DISK
            });

            // Create session dependencies
            this.sessionFactory = SessionFactory.NewInstance();
            this.authProvider = AuthProviderFactory.CreateAuthProvider(repoInfo.AuthenticationType, repoInfo.Address, this.db);

            // Initialize storage
            this.storage = new MetaDataStorage(this.db, new PathMatcher(this.LocalPath, this.RepoInfo.RemotePath));
            this.fileTransmissionStorage = new FileTransmissionStorage(this.db);

            // Add ignore file/folder filter
            this.ignoredFoldersFilter = new IgnoredFoldersFilter { IgnoredPaths = new List<string>(repoInfo.GetIgnoredPaths()) };
            this.ignoredFileNameFilter = new IgnoredFileNamesFilter { Wildcards = ConfigManager.CurrentConfig.IgnoreFileNames };
            this.ignoredFolderNameFilter = new IgnoredFolderNameFilter { Wildcards = ConfigManager.CurrentConfig.IgnoreFolderNames };
            this.invalidFolderNameFilter = new InvalidFolderNameFilter();
            this.filters = new FilterAggregator(this.ignoredFileNameFilter, this.ignoredFolderNameFilter, this.invalidFolderNameFilter, this.ignoredFoldersFilter);
            this.reportingFilter = new ReportingFilter(
                this.Queue,
                this.ignoredFoldersFilter,
                this.ignoredFileNameFilter,
                this.ignoredFolderNameFilter,
                this.invalidFolderNameFilter);
            this.Queue.EventManager.AddEventHandler(this.reportingFilter);
            this.alreadyAddedFilter = new IgnoreAlreadyHandledFsEventsFilter(this.storage, this.fileSystemFactory);
            this.Queue.EventManager.AddEventHandler(this.alreadyAddedFilter);

            // Add handler for repo config changes
            this.Queue.EventManager.AddEventHandler(new GenericSyncEventHandler<RepoConfigChangedEvent>(0, this.RepoInfoChanged));

            // Add periodic sync procedures scheduler
            this.Scheduler = new SyncScheduler(this.Queue, repoInfo.PollInterval);
            this.Queue.EventManager.AddEventHandler(this.Scheduler);

            // Add File System Watcher
            #if __COCOA__
            this.WatcherProducer = new CmisSync.Lib.Producer.Watcher.MacWatcher(LocalPath, Queue);
            #else
            this.WatcherProducer = new NetWatcher(new FileSystemWatcher(this.LocalPath), this.Queue, this.storage);
            #endif
            this.WatcherConsumer = new WatcherConsumer(this.Queue);
            this.Queue.EventManager.AddEventHandler(this.WatcherConsumer);

            // Add transformer
            this.transformer = new ContentChangeEventTransformer(this.Queue, this.storage, this.fileSystemFactory);
            this.Queue.EventManager.AddEventHandler(this.transformer);

            // Add local fetcher
            var localFetcher = new LocalObjectFetcher(this.storage.Matcher, this.fileSystemFactory);
            this.Queue.EventManager.AddEventHandler(localFetcher);

            this.SyncStatusChanged += delegate(SyncStatus status)
            {
                this.Status = status;
            };

            this.ignoredStorage = new IgnoredEntitiesStorage(new IgnoredEntitiesCollection(), this.storage);

            this.Queue.EventManager.AddEventHandler(new EventManagerInitializer(this.Queue, this.storage, this.fileTransmissionStorage, this.ignoredStorage, this.RepoInfo, this.filters, activityListener, this.fileSystemFactory));

            this.Queue.EventManager.AddEventHandler(new DelayRetryAndNextSyncEventHandler(this.Queue));

            this.connectionScheduler = new ConnectionScheduler(this.RepoInfo, this.Queue, this.sessionFactory, this.authProvider);

            this.Queue.EventManager.AddEventHandler(this.connectionScheduler);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="Repository"/> class and releases unmanaged
        /// resources and performs other cleanup operations before the is reclaimed by garbage collection.
        /// </summary>
        ~Repository() {
            this.Dispose(false);
        }

        /// <summary>
        /// Occurs when property changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets or sets the sync status. Affect a new <c>SyncStatus</c> value.
        /// </summary>
        /// <value>The sync status changed.</value>
        public Action<SyncStatus> SyncStatusChanged { get; set; }

        /// <summary>
        /// Gets the current status of the synchronization (paused or not).
        /// </summary>
        /// <value>The status.</value>
        public SyncStatus Status { get; private set; }

        /// <summary>
        /// Gets the polling scheduler.
        /// </summary>
        /// <value>
        /// The scheduler.
        /// </value>
        public SyncScheduler Scheduler { get; private set; }

        /// <summary>
        /// Gets or sets the Event Queue for this repository.
        /// Use this to notifiy events for this repository.
        /// </summary>
        /// <value>The queue.</value>
        public ICountingQueue Queue { get; protected set; }

        /// <summary>
        /// Gets the watcherproducer of the local filesystem for changes.
        /// </summary>
        public IWatcherProducer WatcherProducer { get; private set; }

        /// <summary>
        /// Gets the watcherconsumer of the local filesystem for changes.
        /// </summary>
        public WatcherConsumer WatcherConsumer { get; private set; }

        /// <summary>
        /// Gets or sets the synchronized folder's information.
        /// </summary>
        protected RepoInfo RepoInfo { get; set; }

        /// <summary>
        /// Stop syncing momentarily.
        /// </summary>
        public void Suspend() {
            this.Status = SyncStatus.Suspend;
            this.Scheduler.Stop();
            this.Queue.Suspend();
        }

        /// <summary>
        /// Restart syncing.
        /// </summary>
        public void Resume() {
            this.Status = SyncStatus.Idle;
            this.Queue.Continue();
            this.Scheduler.Start();
        }

        /// <summary>
        /// Implement IDisposable interface.
        /// </summary>
        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Initialize the scheduled background sync processes.
        /// </summary>
        public virtual void Initialize() {
            this.connectionScheduler.Start();

            // Enable FS Watcher events
            this.WatcherProducer.EnableEvents = true;

            // start scheduler for event based sync mechanisms
            this.Scheduler.Start();
        }

        /// <summary>
        /// Dispose the managed and unmanaged resources if disposing is <c>true</c>.
        /// </summary>
        /// <param name="disposing">If set to <c>true</c> disposing.</param>
        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {
                if (disposing) {
                    bool transmissionRun = false;
                    do {
                        transmissionRun = false;
                        List<FileTransmissionEvent> transmissionEvents = this.activityListener.TransmissionManager.ActiveTransmissionsAsList();
                        foreach (FileTransmissionEvent transmissionEvent in transmissionEvents) {
                            string localFolder = this.RepoInfo.LocalPath;
                            if (!localFolder.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString())) {
                                localFolder = localFolder + System.IO.Path.DirectorySeparatorChar.ToString();
                            }

                            if (transmissionEvent.Path.StartsWith(localFolder)) {
                                transmissionRun = true;
                                transmissionEvent.ReportProgress(new TransmissionProgressEventArgs { Aborting = true });
                            }
                        }
                    } while (transmissionRun);

                    this.connectionScheduler.Dispose();
                    this.Scheduler.Dispose();
                    this.WatcherProducer.Dispose();
                    this.Queue.StopListener();
                    int timeout = 500;
                    if (!this.Queue.WaitForStopped(timeout)) {
                        Logger.Debug(string.Format("Event Queue is of {0} has not been closed in {1} miliseconds", this.RemoteUrl.ToString(), timeout));
                    }

                    this.Queue.Dispose();
                    this.authProvider.Dispose();
                    if (this.db != null) {
                        this.db.Dispose();
                    }
                }

                this.disposed = true;
            }
        }

        private static ICountingQueue CreateQueue() {
            var manager = new SyncEventManager();
            return new SyncEventQueue(manager);
        }

        private bool RepoInfoChanged(ISyncEvent e) {
            if (e is RepoConfigChangedEvent) {
                this.RepoInfo = (e as RepoConfigChangedEvent).RepoInfo;
                this.ignoredFoldersFilter.IgnoredPaths = new List<string>(this.RepoInfo.GetIgnoredPaths());
                this.ignoredFileNameFilter.Wildcards = ConfigManager.CurrentConfig.IgnoreFileNames;
                this.ignoredFolderNameFilter.Wildcards = ConfigManager.CurrentConfig.IgnoreFolderNames;
                this.authProvider.DeleteAllCookies();
                return true;
            }

            return false;
        }

        /// <summary>
        /// This method is called by the Set accessor of each property.
        /// </summary>
        /// <param name="propertyName">Property name.</param>
        private void NotifyPropertyChanged(string propertyName) {
            if (string.IsNullOrEmpty(propertyName)) {
                throw new ArgumentNullException("Given property name is null");
            }

            var handler = this.PropertyChanged;
            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}