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
    using System.Threading;

    using CmisSync.Lib;
    using CmisSync.Lib.Accumulator;
    using CmisSync.Lib.Cmis;
    using CmisSync.Lib.Cmis.ConvenienceExtenders;
    using CmisSync.Lib.Config;
    using CmisSync.Lib.Consumer;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Exceptions;
    using CmisSync.Lib.FileTransmission;
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
    /// Synchronized CMIS repository.
    /// </summary>
    public class Repository : AbstractNotifyingRepository, IDisposable, IObserver<Tuple<EventCategory, int>> {
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

        private RepositoryRootDeletedDetection rootFolderMonitor;

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
        private bool disposed;

        /// <summary>
        /// The auth provider.
        /// </summary>
        private IDisposableAuthProvider authProvider;

        /// <summary>
        /// The session factory.
        /// </summary>
        private ISessionFactory sessionFactory;

        private ITransmissionFactory transmissionFactory;

        private ContentChangeEventTransformer transformer;

        private DBreezeEngine db;

        private DBreezeConfiguration dbConfig;

        private IFileSystemInfoFactory fileSystemFactory;

        private ActivityListenerAggregator activityListener;

        private int connectionExceptionsFound;

        private IDisposable unsubscriber;

        private object counterLock = new object();

        private object connectionExceptionCounterLock = new object();

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
        protected Repository(RepoInfo repoInfo, ActivityListenerAggregator activityListener, bool inMemory, ICountingQueue queue) : base(repoInfo) {
            if (activityListener == null) {
                throw new ArgumentNullException("activityListener");
            }

            this.fileSystemFactory = new FileSystemInfoFactory();
            this.activityListener = activityListener;

            this.rootFolderMonitor = new RepositoryRootDeletedDetection(this.fileSystemFactory.CreateDirectoryInfo(this.LocalPath));
            this.rootFolderMonitor.RepoRootDeleted += this.RootFolderAvailablilityChanged;

            if (!this.fileSystemFactory.CreateDirectoryInfo(this.LocalPath).IsExtendedAttributeAvailable()) {
                throw new ExtendedAttributeException("Extended Attributes are not available on the local path: " + this.LocalPath);
            }

            this.Queue = queue;
            var eventManager = this.Queue.EventManager;
            new ConnectionInterruptedHandler(eventManager, this.Queue);
            eventManager.AddEventHandler(this.rootFolderMonitor);
            eventManager.AddEventHandler(new DebugLoggingHandler());

            // Create Database connection
            this.dbConfig = new DBreezeConfiguration {
                DBreezeDataFolderName = inMemory ? string.Empty : repoInfo.GetDatabasePath(),
                Storage = inMemory ? DBreezeConfiguration.eStorage.MEMORY : DBreezeConfiguration.eStorage.DISK
            };
            this.db = new DBreezeEngine(this.dbConfig);

            // Create session dependencies
            this.sessionFactory = SessionFactory.NewInstance();
            this.authProvider = AuthProviderFactory.CreateAuthProvider(repoInfo.AuthenticationType, repoInfo.Address, this.db);

            // Initialize storage
            this.storage = new MetaDataStorage(this.db, new PathMatcher(this.LocalPath, this.RepoInfo.RemotePath), inMemory);
            this.fileTransmissionStorage = new FileTransmissionStorage(this.db, RepoInfo.ChunkSize);

            // Add ignore file/folder filter
            this.ignoredFoldersFilter = new IgnoredFoldersFilter { IgnoredPaths = new List<string>(repoInfo.GetIgnoredPaths()) };
            this.ignoredFileNameFilter = new IgnoredFileNamesFilter { Wildcards = ConfigManager.CurrentConfig.IgnoreFileNames };
            this.ignoredFolderNameFilter = new IgnoredFolderNameFilter { Wildcards = ConfigManager.CurrentConfig.IgnoreFolderNames };
            this.invalidFolderNameFilter = new InvalidFolderNameFilter();
            var symlinkFilter = new SymlinkFilter();
            this.filters = new FilterAggregator(this.ignoredFileNameFilter, this.ignoredFolderNameFilter, this.invalidFolderNameFilter, this.ignoredFoldersFilter, symlinkFilter);
            this.reportingFilter = new ReportingFilter(
                this.Queue,
                this.ignoredFoldersFilter,
                this.ignoredFileNameFilter,
                this.ignoredFolderNameFilter,
                this.invalidFolderNameFilter,
                symlinkFilter);
            this.transmissionFactory = new TransmissionFactory(this, activityListener.TransmissionManager);
            eventManager.AddEventHandler(this.reportingFilter);
            this.alreadyAddedFilter = new IgnoreAlreadyHandledFsEventsFilter(this.storage, this.fileSystemFactory);
            eventManager.AddEventHandler(this.alreadyAddedFilter);

            // Add handler for repo config changes
            eventManager.AddEventHandler(new GenericSyncEventHandler<RepoConfigChangedEvent>(0, this.RepoInfoChanged));

            // Add periodic sync procedures scheduler
            this.Scheduler = new SyncScheduler(this.Queue, repoInfo.PollInterval);
            eventManager.AddEventHandler(this.Scheduler);

            // Add File System Watcher
            #if __COCOA__
            this.WatcherProducer = new CmisSync.Lib.Producer.Watcher.MacWatcher(LocalPath, Queue);
            #else
            this.WatcherProducer = new NetWatcher(new FileSystemWatcher(this.LocalPath), this.Queue, this.storage);
            #endif
            this.WatcherConsumer = new WatcherConsumer(this.Queue);
            eventManager.AddEventHandler(this.WatcherConsumer);

            // Add transformer
            this.transformer = new ContentChangeEventTransformer(this.Queue, this.storage, this.fileSystemFactory);
            eventManager.AddEventHandler(this.transformer);

            // Add local fetcher
            var localFetcher = new LocalObjectFetcher(this.storage.Matcher, this.fileSystemFactory);
            eventManager.AddEventHandler(localFetcher);

            this.ignoredStorage = new IgnoredEntitiesStorage(new IgnoredEntitiesCollection(), this.storage);

            eventManager.AddEventHandler(new EventManagerInitializer(this.Queue, this.storage, this.fileTransmissionStorage, this.ignoredStorage, this.RepoInfo, this.filters, activityListener, this.transmissionFactory, this.fileSystemFactory));

            eventManager.AddEventHandler(new DelayRetryAndNextSyncEventHandler(this.Queue));

            this.connectionScheduler = new ConnectionScheduler(this.RepoInfo, this.Queue, this.sessionFactory, this.authProvider);

            eventManager.AddEventHandler(this.connectionScheduler);
            eventManager.AddEventHandler(
                new GenericSyncEventHandler<SuccessfulLoginEvent>(
                10000,
                delegate(ISyncEvent e) {
                this.RepoStatusFlags.Connected = true;
                this.Status = this.RepoStatusFlags.Status;

                return false;
            }));
            eventManager.AddEventHandler(
                new GenericSyncEventHandler<ConfigurationNeededEvent>(
                10000,
                delegate(ISyncEvent e) {
                this.RepoStatusFlags.Warning = true;
                this.Status = this.RepoStatusFlags.Status;

                return false;
            }));
            this.unsubscriber = this.Queue.CategoryCounter.Subscribe(this);
            eventManager.OnException += (sender, e) => {
                ExceptionType type = ExceptionType.Unknown;
                if (e.Exception is VirusDetectedException) {
                    type = ExceptionType.FileUploadBlockedDueToVirusDetected;
                }

                this.PassExceptionToListener(ExceptionLevel.Warning, type, e.Exception);
            };
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="Repository"/> class and releases unmanaged
        /// resources and performs other cleanup operations before the is reclaimed by garbage collection.
        /// </summary>
        ~Repository() {
            this.Dispose(false);
        }

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
        /// Stop syncing momentarily.
        /// </summary>
        public void Suspend() {
            if (this.disposed) {
                throw new ObjectDisposedException(this.GetType().Name);
            }

            if (!this.RepoStatusFlags.Paused) {
                this.RepoStatusFlags.Paused = true;
                this.Status = this.RepoStatusFlags.Status;

                this.Scheduler.Stop();
                this.Queue.Suspend();
                foreach (var transmission in this.activityListener.TransmissionManager.ActiveTransmissionsAsList()) {
                    string localFolder = this.RepoInfo.LocalPath;
                    if (!localFolder.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString())) {
                        localFolder = localFolder + System.IO.Path.DirectorySeparatorChar.ToString();
                    }

                    if (transmission.Path.StartsWith(localFolder)) {
                        transmission.Pause();
                    }
                }
            }
        }

        /// <summary>
        /// Restart syncing.
        /// </summary>
        public void Resume() {
            if (this.disposed) {
                throw new ObjectDisposedException(this.GetType().Name);
            }

            if (this.RepoStatusFlags.Paused) {
                this.RepoStatusFlags.Paused = false;
                this.Status = this.RepoStatusFlags.Status;

                this.Queue.Continue();
                this.Scheduler.Start();
                foreach (var transmission in this.activityListener.TransmissionManager.ActiveTransmissionsAsList()) {
                    string localFolder = this.RepoInfo.LocalPath;
                    if (!localFolder.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString())) {
                        localFolder = localFolder + System.IO.Path.DirectorySeparatorChar.ToString();
                    }

                    if (transmission.Path.StartsWith(localFolder)) {
                        transmission.Resume();
                    }
                }
            }
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
            if (this.disposed) {
                throw new ObjectDisposedException(this.GetType().Name);
            }

            this.connectionScheduler.Start();

            // Enable FS Watcher events
            this.WatcherProducer.EnableEvents = true;

            // start scheduler for event based sync mechanisms
            this.Scheduler.Start();
        }

        /// <summary>
        /// Just ignores on completed events.
        /// </summary>
        public void OnCompleted() {
        }

        /// <summary>
        /// Ignores on error events.
        /// </summary>
        /// <param name="error">Error.</param>
        public void OnError(Exception error) {
        }

        /// <summary>
        /// Is called if any EventCategory counter is changed.
        /// </summary>
        /// <param name="value">The changed value.</param>
        public virtual void OnNext(Tuple<EventCategory, int> value) {
            if (this.disposed) {
                throw new ObjectDisposedException(this.GetType().Name);
            }

            if (value == null) {
                return;
            }

            if (value.Item1 == EventCategory.DetectedChange) {
                if (value.Item2 > 0) {
                    lock (this.counterLock) {
                        this.NumberOfChanges = value.Item2;
                    }
                } else {
                    lock (this.counterLock) {
                        this.NumberOfChanges = 0;
                        this.LastFinishedSync = this.Status == SyncStatus.Idle ? DateTime.Now : this.LastFinishedSync;
                    }
                }
            } else if (value.Item1 == EventCategory.SyncRequested || value.Item1 == EventCategory.PeriodicSync) {
                lock (this.counterLock) {
                    if (value.Item1 == EventCategory.SyncRequested) {
                        this.RepoStatusFlags.SyncRequested = value.Item2 > 0;
                        this.Status = this.RepoStatusFlags.Status;
                    }

                    if (value.Item2 <= 0 && this.Status == SyncStatus.Idle) {
                        this.LastFinishedSync = DateTime.Now;
                    }
                }
            } else if (value.Item1 == EventCategory.ConnectionException) {
                lock (this.connectionExceptionCounterLock) {
                    if (value.Item2 > this.connectionExceptionsFound) {
                        this.RepoStatusFlags.Connected = false;
                        this.Status = this.RepoStatusFlags.Status;
                    }

                    this.connectionExceptionsFound = value.Item2;
                }
            }
        }

        /// <summary>
        /// Dispose the managed and unmanaged resources if disposing is <c>true</c>.
        /// </summary>
        /// <param name="disposing">If set to <c>true</c> disposing.</param>
        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {
                this.Scheduler.Stop();
                this.Queue.Suspend();
                this.connectionScheduler.Dispose();
                this.Scheduler.Dispose();
                this.WatcherProducer.Dispose();
                this.Queue.StopListener();

                if (disposing) {
                    bool transmissionRun = false;

                    // Maximum timeout is 10 sec (5 for aborting transmissions and 5 for stopping queue)
                    int timeout = 5000;
                    do {
                        if (transmissionRun) {
                            Thread.Sleep(10);
                            timeout -= 10;
                        }

                        transmissionRun = false;
                        List<Transmission> transmissions = this.activityListener.TransmissionManager.ActiveTransmissionsAsList();
                        foreach (Transmission transmission in transmissions) {
                            string localFolder = this.RepoInfo.LocalPath;
                            if (!localFolder.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString())) {
                                localFolder = localFolder + System.IO.Path.DirectorySeparatorChar.ToString();
                            }

                            if (transmission.Path.StartsWith(localFolder)) {
                                transmissionRun = true;
                                transmission.Abort();
                            }
                        }
                    } while (transmissionRun && timeout > 0);

                    timeout = 5000;
                    if (!this.Queue.WaitForStopped(timeout)) {
                        Logger.Debug(string.Format("Event Queue of {0} has not been closed in {1} miliseconds", this.RemoteUrl.ToString(), timeout));
                    }

                    this.Queue.Dispose();
                    this.authProvider.Dispose();
                    if (this.db != null) {
                        this.db.Dispose();
                    }

                    if (this.dbConfig != null) {
                        this.dbConfig.Dispose();
                    }

                    if (this.unsubscriber != null) {
                        this.unsubscriber.Dispose();
                    }
                }

                this.disposed = true;
            }
        }

        /// <summary>
        /// Creates a default EventManager and Queue.
        /// </summary>
        /// <returns>The queue.</returns>
        protected static ICountingQueue CreateQueue() {
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
                this.Queue.EventManager.RemoveEventHandler(this.rootFolderMonitor);
                this.rootFolderMonitor.RepoRootDeleted -= this.RootFolderAvailablilityChanged;
                this.rootFolderMonitor = new RepositoryRootDeletedDetection(this.fileSystemFactory.CreateDirectoryInfo(this.RepoInfo.LocalPath));
                this.rootFolderMonitor.RepoRootDeleted += this.RootFolderAvailablilityChanged;
                return true;
            }

            return false;
        }

        private void RootFolderAvailablilityChanged(object sender, RepositoryRootDeletedDetection.RootExistsEventArgs e) {
            this.RepoStatusFlags.Deactivated = !e.RootExists;
            this.Status = this.RepoStatusFlags.Status;
            if (!e.RootExists) {
                this.PassExceptionToListener(ExceptionLevel.Fatal, ExceptionType.LocalSyncTargetDeleted);
            } else {
                this.Queue.AddEvent(new StartNextSyncEvent(fullSyncRequested: true));
            }
        }
    }
}