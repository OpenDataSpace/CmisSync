//-----------------------------------------------------------------------
// <copyright file="SuccessfulLoginHandler.cs" company="GRAU DATA AG">
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
        private ContentChangeEventAccumulator ccaccumulator;
        private RepoInfo repoInfo;
        private IMetaDataStorage storage;
        private SyncEventManager manager;
        private ContentChanges contentChanges;
        private RemoteObjectFetcher remoteFetcher;
        private Crawler crawler;
        private SyncMechanism mechanism;
        private IFileSystemInfoFactory fileSystemFactory;

        private static readonly ILog Logger = LogManager.GetLogger(typeof(SyncStrategyInitializer));
        
        public SyncStrategyInitializer(ISyncEventQueue queue, IMetaDataStorage storage, SyncEventManager manager, RepoInfo repoInfo, IFileSystemInfoFactory fsFactory = null) : base(queue)
        {
            if (storage == null)
            {
                throw new ArgumentNullException("storage null");
            }
            
            if (repoInfo == null)
            {
                throw new ArgumentNullException("Repoinfo null");
            }
            
            if(manager == null) 
            {
                throw new ArgumentNullException("Manager is null");
            }
            
            if(fsFactory == null) {
                this.fileSystemFactory = new FileSystemInfoFactory();
            } else {
                this.fileSystemFactory = fsFactory;
            }
            
            this.repoInfo = repoInfo;
            this.storage = storage;
            this.manager = manager;
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
                
                // Remove former added instances from event manager
                if (this.ccaccumulator != null)
                {
                    manager.RemoveEventHandler(this.ccaccumulator);
                }

                if (this.contentChanges != null)
                {
                    manager.RemoveEventHandler(this.contentChanges);
                }

                if (this.AreChangeEventsSupported(session))
                {

                    // Add Accumulator
                    this.ccaccumulator = new ContentChangeEventAccumulator(session, this.Queue);
                    manager.AddEventHandler(this.ccaccumulator);

                    // Add Content Change sync algorithm
                    this.contentChanges = new ContentChanges(session, this.storage, this.Queue);
                    manager.AddEventHandler(this.contentChanges);
                }

                // Add remote object fetcher
                if (this.remoteFetcher != null)
                {
                    manager.RemoveEventHandler(this.remoteFetcher);
                }

                this.remoteFetcher = new RemoteObjectFetcher(session, this.storage);
                manager.AddEventHandler(this.remoteFetcher);

                // Add crawler
                if (this.crawler != null)
                {
                    manager.RemoveEventHandler(this.crawler);
                }

                this.crawler = new Crawler(this.Queue, remoteRoot, this.fileSystemFactory.CreateDirectoryInfo(this.repoInfo.LocalPath), this.fileSystemFactory);
                manager.AddEventHandler(this.crawler);

                if (this.mechanism != null)
                {
                    manager.RemoveEventHandler(this.mechanism);
                }
    
                var localDetection = new LocalSituationDetection();
                var remoteDetection = new RemoteSituationDetection();
                
                this.mechanism = new SyncMechanism(localDetection, remoteDetection, this.Queue, session, this.storage);
                manager.AddEventHandler(this.mechanism);
                
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