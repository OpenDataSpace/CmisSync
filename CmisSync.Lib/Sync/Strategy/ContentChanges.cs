//-----------------------------------------------------------------------
// <copyright file="ContentChanges.cs" company="GRAU DATA AG">
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
namespace CmisSync.Lib.Sync.Strategy
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    using CmisSync.Lib.Cmis;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Storage;
    
    using DotCMIS.Client;
    using DotCMIS.Exceptions;

    using log4net;

    public class ContentChanges : ReportingSyncEventHandler
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ContentChanges));
        private ISession session;
        private IMetaDataStorage storage;
        private int maxNumberOfContentChanges;
        private bool isPropertyChangesSupported;

        private object syncLock = new object();

        private IChangeEvent lastChange;

        public ContentChanges(ISession session, IMetaDataStorage storage, ISyncEventQueue queue, int maxNumberOfContentChanges = 100, bool isPropertyChangesSupported = false) : base (queue) {
            if(session == null)
                throw new ArgumentNullException("Session instance is needed for the ChangeLogStrategy, but was null");
            if(storage == null)
                throw new ArgumentNullException("MetaDataStorage instance is needed for the ChangeLogStrategy, but was null");
            if(maxNumberOfContentChanges <= 1)
                throw new ArgumentException("MaxNumberOfContentChanges must be greater then one");
            this.session = session;
            this.storage = storage;
            this.maxNumberOfContentChanges = maxNumberOfContentChanges;
            this.isPropertyChangesSupported = isPropertyChangesSupported;
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
                    string lastRemoteChangeLogTokenBeforeFullCrawlSync = session.Binding.GetRepositoryService().GetRepositoryInfo(session.RepositoryInfo.Id, null).LatestChangeLogToken;
                    if(storage.ChangeLogToken == null) {
                        syncEvent.LastTokenOnServer = lastRemoteChangeLogTokenBeforeFullCrawlSync;
                    }
                    // Use fallback sync algorithm
                    return false;
                }
                else
                {
                    return startSync();
                }
            }

            // The above started full sync is finished.
            FullSyncCompletedEvent syncCompleted = e as FullSyncCompletedEvent;
            if(syncCompleted != null) {
                string lastTokenOnServer = syncCompleted.StartEvent.LastTokenOnServer;
                if(!String.IsNullOrEmpty(lastTokenOnServer))
                {
                    storage.ChangeLogToken = lastTokenOnServer;
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
                string lastTokenOnClient = storage.ChangeLogToken;

                // Get last change log token on server side.
                session.Binding.GetRepositoryService().GetRepositoryInfos(null);    //  refresh
                string lastTokenOnServer = session.Binding.GetRepositoryService().GetRepositoryInfo(session.RepositoryInfo.Id, null).LatestChangeLogToken;

                if(lastTokenOnClient != lastTokenOnServer)
                {
//                    using(var syncTask = new Task((Action) delegate() {
//                        if(Monitor.TryEnter(syncLock)) {
//                            try{
                                Sync();
//                            }finally {
//                                Monitor.Exit(syncLock);
//                            }
//                        }
//                    })) {
//                        syncTask.Start();
//                    }
                }
                // No changes or background process started
                return true;
            }catch(CmisRuntimeException e) {
                Logger.Warn("ContentChangeSync not successfull, fallback to CrawlSync");
                Logger.Debug(e.Message);
                Logger.Debug(e.StackTrace);
                // Use fallback sync algorithm
                return false;
            }
        }

        private void Sync()
        {
            // Get last change log token on server side.
            session.Binding.GetRepositoryService().GetRepositoryInfos(null);    //  refresh
            string lastTokenOnServer = session.Binding.GetRepositoryService().GetRepositoryInfo(session.RepositoryInfo.Id, null).LatestChangeLogToken;

            // Get last change token that had been saved on client side.
            string lastTokenOnClient = storage.ChangeLogToken;

            if (lastTokenOnClient == null)
            {
                // Token is null, which means no content change sync has ever happened yet, so just sync everything from remote.
                // Force full sync  
                var fullsyncevent = new StartNextSyncEvent(true);
                Queue.AddEvent(fullsyncevent);
                return;
            }

            do
            {
                // Check which files/folders have changed.
                IChangeEvents changes = session.GetContentChanges(lastTokenOnClient, isPropertyChangesSupported, maxNumberOfContentChanges);
                // Replicate each change to the local side.
                bool first = true;
                foreach (IChangeEvent change in changes.ChangeEventList)
                {
                    // ignore first event when lists overlapp
                    if(first) {
                        first = false;
                        if(lastChange != null && 
                                (lastChange.ChangeType == DotCMIS.Enums.ChangeType.Created
                                 || lastChange.ChangeType == DotCMIS.Enums.ChangeType.Deleted)
                          ) {
                            if (change != null && change.ChangeType == lastChange.ChangeType && change.ObjectId == lastChange.ObjectId) {
                                continue;
                            }
                        }
                    }
                    lastChange = change;

                    Queue.AddEvent(new ContentChangeEvent(change.ChangeType, change.ObjectId));
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
                storage.ChangeLogToken = lastTokenOnClient;
                session.Binding.GetRepositoryService().GetRepositoryInfos(null);    //  refresh
                lastTokenOnServer = session.Binding.GetRepositoryService().GetRepositoryInfo(session.RepositoryInfo.Id, null).LatestChangeLogToken;
            }
            while (!lastTokenOnServer.Equals(lastTokenOnClient));
        }

    }
}

