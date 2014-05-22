//-----------------------------------------------------------------------
// <copyright file="SyncStrategyInitializer.cs" company="GRAU DATA AG">
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
namespace CmisSync.Lib
{
    using System;

    using CmisSync.Lib.Config;
    using CmisSync.Lib.Data;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Events.Filter;
    using CmisSync.Lib.Storage;
    using CmisSync.Lib.Sync.Strategy;

    using DotCMIS.Client;
    using DotCMIS.Enums;

    using log4net;

    /// <summary>
    /// Successful login handler. It handles the SuccessfulLoginEvent and registers
    /// the necessary handlers and registers the root folder to the MetaDataStorage.
    /// </summary>
    public class SyncStrategyInitializer : ReportingSyncEventHandler
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(SyncStrategyInitializer));

        private ContentChangeEventAccumulator ccaccumulator;
        private RepoInfo repoInfo;
        private IMetaDataStorage storage;
        private ContentChanges contentChanges;
        private RemoteObjectFetcher remoteFetcher;
        private Crawler crawler;
        private SyncMechanism mechanism;
        private IFileSystemInfoFactory fileSystemFactory;
        private IgnoreAlreadyHandledContentChangeEventsFilter alreadyHandledFilter;
        private RemoteObjectMovedOrRenamedAccumulator romaccumulator;
  
        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.SyncStrategyInitializer"/> class.
        /// </summary>
        /// <param name='queue'>
        /// The SyncEventQueue.
        /// </param>
        /// <param name='storage'>
        /// Storage for Metadata.
        /// </param>
        /// <param name='repoInfo'>
        /// Repo info.
        /// </param>
        /// <param name='fsFactory'>
        /// Fs factory.
        /// </param>
        /// <exception cref='ArgumentNullException'>
        /// Is thrown when an argument passed to a method is invalid because it is <see langword="null" /> .
        /// </exception>
        public SyncStrategyInitializer(ISyncEventQueue queue, IMetaDataStorage storage, RepoInfo repoInfo, IFileSystemInfoFactory fsFactory = null) : base(queue)
        {
            if (storage == null)
            {
                throw new ArgumentNullException("storage null");
            }

            if (repoInfo == null)
            {
                throw new ArgumentNullException("Repoinfo null");
            }

            if(fsFactory == null) {
                this.fileSystemFactory = new FileSystemInfoFactory();
            } else {
                this.fileSystemFactory = fsFactory;
            }

            this.repoInfo = repoInfo;
            this.storage = storage;
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
        public override bool Handle(ISyncEvent e)
        {
            if (e is SuccessfulLoginEvent) {
                var successfulLoginEvent = e as SuccessfulLoginEvent;
                var session = successfulLoginEvent.Session;
                var remoteRoot = successfulLoginEvent.Session.GetObjectByPath(this.repoInfo.RemotePath) as IFolder;

                // Remove former added instances from event Queue.EventManager
                if (this.ccaccumulator != null) {
                    this.Queue.EventManager.RemoveEventHandler(this.ccaccumulator);
                }

                if (this.contentChanges != null) {
                    this.Queue.EventManager.RemoveEventHandler(this.contentChanges);
                }

                if (this.alreadyHandledFilter != null) {
                    this.Queue.EventManager.RemoveEventHandler(this.alreadyHandledFilter);
                }

                if (this.AreChangeEventsSupported(session))
                {
                    // Add Accumulator
                    this.ccaccumulator = new ContentChangeEventAccumulator(session, this.Queue);
                    this.Queue.EventManager.AddEventHandler(this.ccaccumulator);

                    // Add Content Change sync algorithm
                    this.contentChanges = new ContentChanges(session, this.storage, this.Queue);
                    this.Queue.EventManager.AddEventHandler(this.contentChanges);

                    // Add Filter of already handled change events
                    this.alreadyHandledFilter = new IgnoreAlreadyHandledContentChangeEventsFilter(this.storage, session);
                    this.Queue.EventManager.AddEventHandler(this.alreadyHandledFilter);
                }

                // Add remote object fetcher
                if (this.remoteFetcher != null) {
                    this.Queue.EventManager.RemoveEventHandler(this.remoteFetcher);
                }

                this.remoteFetcher = new RemoteObjectFetcher(session, this.storage);
                this.Queue.EventManager.AddEventHandler(this.remoteFetcher);

                // Add crawler
                if (this.crawler != null) {
                    this.Queue.EventManager.RemoveEventHandler(this.crawler);
                }

                this.crawler = new Crawler(this.Queue, remoteRoot, this.fileSystemFactory.CreateDirectoryInfo(this.repoInfo.LocalPath), this.storage, this.fileSystemFactory);
                this.Queue.EventManager.AddEventHandler(this.crawler);

                // Add remote object moved accumulator
                if (this.romaccumulator != null) {
                    this.Queue.EventManager.RemoveEventHandler(this.romaccumulator);
                }

                this.romaccumulator = new RemoteObjectMovedOrRenamedAccumulator(this.Queue, this.storage, this.fileSystemFactory);
                this.Queue.EventManager.AddEventHandler(this.romaccumulator);

                // Add sync mechanism
                if (this.mechanism != null) {
                    this.Queue.EventManager.RemoveEventHandler(this.mechanism);
                }

                var localDetection = new LocalSituationDetection();
                var remoteDetection = new RemoteSituationDetection();

                this.mechanism = new SyncMechanism(localDetection, remoteDetection, this.Queue, session, this.storage);
                this.Queue.EventManager.AddEventHandler(this.mechanism);

                var rootFolder = new MappedObject("/", remoteRoot.Id, MappedObjectType.Folder, null, remoteRoot.ChangeToken);

                Logger.Debug("Saving Root Folder to DataBase");
                this.storage.SaveMappedObject(rootFolder);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Detect whether the repository has the ChangeLog capability.
        /// </summary>
        /// <param name="session">The Cmis Session</param>
        /// <returns>
        /// <c>true</c> if this feature is available, otherwise <c>false</c>
        /// </returns>
        private bool AreChangeEventsSupported(ISession session)
        {
            try
            {
                return (session.RepositoryInfo.Capabilities.ChangesCapability == CapabilityChanges.All ||
                        session.RepositoryInfo.Capabilities.ChangesCapability == CapabilityChanges.ObjectIdsOnly) &&
                    (this.repoInfo.SupportedFeatures == null ||
                    this.repoInfo.SupportedFeatures.GetContentChangesSupport != false);
            }
            catch(NullReferenceException)
            {
                return false;
            }
        }
    }
}