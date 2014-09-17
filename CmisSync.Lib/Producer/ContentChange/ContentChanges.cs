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

namespace CmisSync.Lib.Producer.ContentChange
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    using CmisSync.Lib.Cmis;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.Database;

    using DotCMIS.Client;
    using DotCMIS.Exceptions;

    using log4net;

    /// <summary>
    /// Content changes are collected and published to the queue.
    /// </summary>
    public class ContentChanges : ReportingSyncEventHandler
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ContentChanges));
        private ISession session;
        private IMetaDataStorage storage;
        private int maxNumberOfContentChanges;
        private IChangeEvent lastChange;
        private bool isPropertyChangesSupported;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentChanges"/> class.
        /// </summary>
        /// <param name="session">Cmis Session.</param>
        /// <param name="storage">Meta Data Storage.</param>
        /// <param name="queue">Event Queue.</param>
        /// <param name="maxNumberOfContentChanges">Max number of content changes.</param>
        /// <param name="isPropertyChangesSupported">If set to <c>true</c> is property changes supported.</param>
        public ContentChanges(
            ISession session,
            IMetaDataStorage storage,
            ISyncEventQueue queue,
            int maxNumberOfContentChanges = 100,
            bool isPropertyChangesSupported = false) : base(queue) {
            if (session == null) {
                throw new ArgumentNullException("Session instance is needed for the ChangeLogStrategy, but was null");
            }

            if (storage == null) {
                throw new ArgumentNullException("MetaDataStorage instance is needed for the ChangeLogStrategy, but was null");
            }

            if (maxNumberOfContentChanges <= 1) {
                throw new ArgumentException("MaxNumberOfContentChanges must be greater then one");
            }

            this.session = session;
            this.storage = storage;
            this.maxNumberOfContentChanges = maxNumberOfContentChanges;
            this.isPropertyChangesSupported = isPropertyChangesSupported;
        }

        /// <summary>
        /// Handle the specified e.
        /// </summary>
        /// <param name="e">The event to handle.</param>
        /// <returns>true if handled</returns>
        public override bool Handle(ISyncEvent e)
        {
            StartNextSyncEvent syncEvent = e as StartNextSyncEvent;
            if(syncEvent != null)
            {
                if(syncEvent.FullSyncRequested)
                {
                    // Get last change log token on server side.
                    string lastRemoteChangeLogTokenBeforeFullCrawlSync = this.session.Binding.GetRepositoryService().GetRepositoryInfo(this.session.RepositoryInfo.Id, null).LatestChangeLogToken;
                    if (this.storage.ChangeLogToken == null) {
                        syncEvent.LastTokenOnServer = lastRemoteChangeLogTokenBeforeFullCrawlSync;
                    }

                    // Use fallback sync algorithm
                    return false;
                }
                else
                {
                    Logger.Debug("Starting ContentChange Sync");
                    bool result = this.StartSync();
                    return result;
                }
            }

            // The above started full sync is finished.
            FullSyncCompletedEvent syncCompleted = e as FullSyncCompletedEvent;
            if(syncCompleted != null) {
                string lastTokenOnServer = syncCompleted.StartEvent.LastTokenOnServer;
                if(!string.IsNullOrEmpty(lastTokenOnServer))
                {
                    this.storage.ChangeLogToken = lastTokenOnServer;
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
        private bool StartSync() {
            try {
                string lastTokenOnClient = this.storage.ChangeLogToken;

                // Get last change log token on server side.
                this.session.Binding.GetRepositoryService().GetRepositoryInfos(null);    // refresh
                string lastTokenOnServer = this.session.Binding.GetRepositoryService().GetRepositoryInfo(this.session.RepositoryInfo.Id, null).LatestChangeLogToken;

                if (lastTokenOnClient != lastTokenOnServer) {
                    this.Sync();
                }

                return true;
            } catch(CmisRuntimeException e) {
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
            this.session.Binding.GetRepositoryService().GetRepositoryInfos(null);    // refresh
            string lastTokenOnServer = this.session.Binding.GetRepositoryService().GetRepositoryInfo(this.session.RepositoryInfo.Id, null).LatestChangeLogToken;

            // Get last change token that had been saved on client side.
            string lastTokenOnClient = this.storage.ChangeLogToken;

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
                IChangeEvents changes = this.session.GetContentChanges(lastTokenOnClient, this.isPropertyChangesSupported, this.maxNumberOfContentChanges);

                // Replicate each change to the local side.
                bool first = true;
                foreach (IChangeEvent change in changes.ChangeEventList)
                {
                    // ignore first event when lists overlapp
                    if(first) {
                        first = false;
                        if(this.lastChange != null &&
                           (this.lastChange.ChangeType == DotCMIS.Enums.ChangeType.Created
                         || this.lastChange.ChangeType == DotCMIS.Enums.ChangeType.Deleted))
                        {
                            if (change != null && change.ChangeType == this.lastChange.ChangeType && change.ObjectId == this.lastChange.ObjectId) {
                                continue;
                            }
                        }
                    }

                    this.lastChange = change;

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

                this.storage.ChangeLogToken = lastTokenOnClient;

                // refresh
                this.session.Binding.GetRepositoryService().GetRepositoryInfos(null);
                lastTokenOnServer = this.session.Binding.GetRepositoryService().GetRepositoryInfo(this.session.RepositoryInfo.Id, null).LatestChangeLogToken;
            }
            while (!lastTokenOnServer.Equals(lastTokenOnClient));
        }
    }
}