//-----------------------------------------------------------------------
// <copyright file="EventManagerInitializer.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Queueing {
    using System;

    using CmisSync.Lib.Accumulator;
    using CmisSync.Lib.Cmis.ConvenienceExtenders;
    using CmisSync.Lib.Config;
    using CmisSync.Lib.Consumer;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.FileTransmission;
    using CmisSync.Lib.Filter;
    using CmisSync.Lib.Producer.ContentChange;
    using CmisSync.Lib.Producer.Crawler;
    using CmisSync.Lib.Producer.Watcher;
    using CmisSync.Lib.SelectiveIgnore;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;
    using DotCMIS.Enums;

    using log4net;

    /// <summary>
    /// Successful login handler. It handles the SuccessfulLoginEvent and registers
    /// the necessary handlers and registers the root folder to the MetaDataStorage.
    /// </summary>
    public class EventManagerInitializer : ReportingSyncEventHandler {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(EventManagerInitializer));

        private ContentChangeEventAccumulator ccaccumulator;
        private RepoInfo repoInfo;
        private IMetaDataStorage storage;
        private IFileTransmissionStorage fileTransmissionStorage;
        private ContentChanges contentChanges;
        private RemoteObjectFetcher remoteFetcher;
        private DescendantsCrawler crawler;
        private SyncMechanism mechanism;
        private IFileSystemInfoFactory fileSystemFactory;
        private IgnoreAlreadyHandledContentChangeEventsFilter alreadyHandledFilter;
        private RemoteObjectMovedOrRenamedAccumulator romaccumulator;
        private IFilterAggregator filter;
        private ActivityListenerAggregator activityListener;
        private IIgnoredEntitiesStorage ignoredStorage;
        private SelectiveIgnoreEventTransformer selectiveIgnoreTransformer;
        private SelectiveIgnoreFilter selectiveIgnoreFilter;
        private IgnoreFlagChangeDetection ignoreChangeDetector;
        private ITransmissionFactory transmissionFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventManagerInitializer"/> class.
        /// </summary>
        /// <param name='queue'>The SyncEventQueue.</param>
        /// <param name='storage'>Storage for Metadata.</param>
        /// <param name='fileTransmissionStorage'>Storage for file transmissions.</param>
        /// <param name='ignoredStorage'>Storage for ignored entities.</param>
        /// <param name='repoInfo'>Repo info.</param>
        /// <param name='filter'>Filter aggregation.</param>
        /// <param name='activityListener'>Listener for Sync activities.</param>
        /// <param name='transmissionFactory'>Transmission factory.</param>
        /// <param name='fsFactory'>File system factory.</param>
        /// <exception cref='ArgumentNullException'>
        /// Is thrown when an argument passed to a method is invalid because it is <see langword="null" /> .
        /// </exception>
        public EventManagerInitializer(
            ISyncEventQueue queue,
            IMetaDataStorage storage,
            IFileTransmissionStorage fileTransmissionStorage,
            IIgnoredEntitiesStorage ignoredStorage,
            RepoInfo repoInfo,
            IFilterAggregator filter,
            ActivityListenerAggregator activityListener,
            ITransmissionFactory transmissionFactory,
            IFileSystemInfoFactory fsFactory = null) : base(queue)
        {
            if (storage == null) {
                throw new ArgumentNullException("storage");
            }

            if (fileTransmissionStorage == null) {
                throw new ArgumentNullException("fileTransmissionStorage");
            }

            if (repoInfo == null) {
                throw new ArgumentNullException("repoInfo");
            }

            if (filter == null) {
                throw new ArgumentNullException("filter");
            }

            if (activityListener == null) {
                throw new ArgumentNullException("activityListener");
            }

            if (ignoredStorage == null) {
                throw new ArgumentNullException("ignoredStorage", "Given storage for ignored entries is null");
            }

            if (transmissionFactory == null) {
                throw new ArgumentNullException("transmissionFactory");
            }

            if (fsFactory == null) {
                this.fileSystemFactory = new FileSystemInfoFactory();
            } else {
                this.fileSystemFactory = fsFactory;
            }

            this.filter = filter;
            this.repoInfo = repoInfo;
            this.storage = storage;
            this.ignoredStorage = ignoredStorage;
            this.fileTransmissionStorage = fileTransmissionStorage;
            this.activityListener = activityListener;
            this.transmissionFactory = transmissionFactory;
        }

        /// <summary>
        /// Handle the specified e if it is a SuccessfulLoginEvent
        /// </summary>
        /// <param name='e'>
        /// The event.
        /// </param>
        /// <returns>
        /// true if handled.
        /// </returns>
        public override bool Handle(ISyncEvent e) {
            if (e is SuccessfulLoginEvent) {
                var successfulLoginEvent = e as SuccessfulLoginEvent;
                var session = successfulLoginEvent.Session;
                var remoteRoot = successfulLoginEvent.RootFolder;
                var eventManager = this.Queue.EventManager;

                // Remove former added instances from event Queue.EventManager
                if (this.ccaccumulator != null) {
                    eventManager.RemoveEventHandler(this.ccaccumulator);
                }

                if (this.contentChanges != null) {
                    eventManager.RemoveEventHandler(this.contentChanges);
                }

                if (this.alreadyHandledFilter != null) {
                    eventManager.RemoveEventHandler(this.alreadyHandledFilter);
                }

                if (this.selectiveIgnoreFilter != null) {
                    eventManager.RemoveEventHandler(this.selectiveIgnoreFilter);
                }

                if (this.selectiveIgnoreTransformer != null) {
                    eventManager.RemoveEventHandler(this.selectiveIgnoreTransformer);
                }

                if (this.ignoreChangeDetector != null) {
                    eventManager.RemoveEventHandler(this.ignoreChangeDetector);
                }

                if (successfulLoginEvent.ChangeEventsSupported &&
                    (this.repoInfo.SupportedFeatures == null || this.repoInfo.SupportedFeatures.GetContentChangesSupport != false)) {
                    Logger.Info("Session supports content changes");

                    // Add Accumulator
                    this.ccaccumulator = new ContentChangeEventAccumulator(session, this.Queue);
                    eventManager.AddEventHandler(this.ccaccumulator);

                    // Add Content Change sync algorithm
                    this.contentChanges = new ContentChanges(session, this.storage, this.Queue);
                    eventManager.AddEventHandler(this.contentChanges);

                    // Add Filter of already handled change events
                    this.alreadyHandledFilter = new IgnoreAlreadyHandledContentChangeEventsFilter(this.storage, session);
                    eventManager.AddEventHandler(this.alreadyHandledFilter);
                }

                if (successfulLoginEvent.SelectiveSyncSupported) {
                    // Transforms events of ignored folders
                    this.selectiveIgnoreTransformer = new SelectiveIgnoreEventTransformer(this.ignoredStorage, this.Queue);
                    eventManager.AddEventHandler(this.selectiveIgnoreTransformer);

                    // Filters events of ignored folders
                    this.selectiveIgnoreFilter = new SelectiveIgnoreFilter(this.ignoredStorage);
                    eventManager.AddEventHandler(this.selectiveIgnoreFilter);

                    // Detection if any ignored object has changed its state
                    this.ignoreChangeDetector = new IgnoreFlagChangeDetection(this.ignoredStorage, new PathMatcher.PathMatcher(this.repoInfo.LocalPath, this.repoInfo.RemotePath), this.Queue);
                    eventManager.AddEventHandler(this.ignoreChangeDetector);
                }

                // Add remote object fetcher
                if (this.remoteFetcher != null) {
                    eventManager.RemoveEventHandler(this.remoteFetcher);
                }

                this.remoteFetcher = new RemoteObjectFetcher(session, this.storage);
                eventManager.AddEventHandler(this.remoteFetcher);

                // Add crawler
                if (this.crawler != null) {
                    eventManager.RemoveEventHandler(this.crawler);
                }

                var localRootFolder = this.fileSystemFactory.CreateDirectoryInfo(this.repoInfo.LocalPath);

                this.crawler = new DescendantsCrawler(this.Queue, remoteRoot, localRootFolder, this.storage, this.filter, this.activityListener, this.ignoredStorage);
                eventManager.AddEventHandler(this.crawler);

                // Add remote object moved accumulator
                if (this.romaccumulator != null) {
                    eventManager.RemoveEventHandler(this.romaccumulator);
                }

                this.romaccumulator = new RemoteObjectMovedOrRenamedAccumulator(this.Queue, this.storage, this.fileSystemFactory);
                eventManager.AddEventHandler(this.romaccumulator);

                // Add sync mechanism
                if (this.mechanism != null) {
                    eventManager.RemoveEventHandler(this.mechanism);
                }

                var localDetection = new LocalSituationDetection();
                var remoteDetection = new RemoteSituationDetection();

                this.mechanism = new SyncMechanism(
                    localDetection,
                    remoteDetection,
                    this.Queue,
                    session,
                    this.storage,
                    this.fileTransmissionStorage,
                    this.activityListener,
                    this.filter,
                    this.transmissionFactory,
                    successfulLoginEvent.PrivateWorkingCopySupported);
                eventManager.AddEventHandler(this.mechanism);

                var rootUuid = localRootFolder.Uuid;
                if (rootUuid == null) {
                    try {
                        var storedRootFolder = this.storage.GetObjectByLocalPath(localRootFolder);
                        if (storedRootFolder != null && storedRootFolder.Guid != Guid.Empty) {
                            Logger.Info("Restored Guid of the local root folder");
                            localRootFolder.Uuid = storedRootFolder.Guid;
                        } else {
                            Logger.Info("Created and set new Guid for the local root folder");
                            localRootFolder.Uuid = Guid.NewGuid();
                        }
                    } catch (RestoreModificationDateException ex) {
                        Logger.Debug("Could not restore modification date", ex);
                    } catch (ExtendedAttributeException ex) {
                        Logger.Warn("Problem on setting Guid of the root path", ex);
                    } finally {
                        rootUuid = localRootFolder.Uuid ?? Guid.Empty;
                    }
                }

                var rootFolder = new MappedObject("/", remoteRoot.Id, MappedObjectType.Folder, null, remoteRoot.ChangeToken) {
                    LastRemoteWriteTimeUtc = remoteRoot.LastModificationDate,
                    Guid = (Guid)rootUuid
                };

                Logger.Debug("Saving Root Folder to DataBase");
                this.storage.SaveMappedObject(rootFolder);

                // Sync up everything that changed
                // since we've been offline
                // start full crawl sync on beginning
                this.Queue.AddEvent(new StartNextSyncEvent(true));
                return true;
            }

            return false;
        }
    }
}