//-----------------------------------------------------------------------
// <copyright file="DescendantsCrawler.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Producer.Crawler
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Filter;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;

    using log4net;

    /// <summary>
    /// Decendants crawler.
    /// </summary>
    public class DescendantsCrawler : ReportingSyncEventHandler
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(DescendantsCrawler));
        private IActivityListener activityListener;
        private IDescendantsTreeBuilder treebuilder;
        private CrawlEventGenerator eventGenerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="DescendantsCrawler"/> class.
        /// </summary>
        /// <param name="queue">Sync Event Queue.</param>
        /// <param name="remoteFolder">Remote folder.</param>
        /// <param name="localFolder">Local folder.</param>
        /// <param name="storage">Meta data storage.</param>
        /// <param name="filter">Aggregated filter.</param>
        /// <param name="activityListener">Activity listner.</param>
        public DescendantsCrawler(
            ISyncEventQueue queue,
            IFolder remoteFolder,
            IDirectoryInfo localFolder,
            IMetaDataStorage storage,
            IFilterAggregator filter,
            IActivityListener activityListener)
            : base(queue)
        {
            if (remoteFolder == null) {
                throw new ArgumentNullException("Given remoteFolder is null");
            }

            if (localFolder == null) {
                throw new ArgumentNullException("Given localFolder is null");
            }

            if (storage == null) {
                throw new ArgumentNullException("Given storage is null");
            }

            if (filter == null) {
                throw new ArgumentNullException("Given filter is null");
            }

            if (activityListener == null) {
                throw new ArgumentNullException("Given activityListener is null");
            }

            this.activityListener = activityListener;
            this.treebuilder = new DescendantsTreeBuilder(storage, remoteFolder, localFolder, filter);
            this.eventGenerator = new CrawlEventGenerator(storage);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Producer.Crawler.DescendantsCrawler"/> class based on its internal classes.
        /// This is mostly usefull for Unit Testing
        /// </summary>
        /// <param name='queue'>
        /// The event queue.
        /// </param>
        /// <param name='builder'>
        /// The DescendantsTreeBuilder.
        /// </param>
        /// <param name='generator'>
        /// The CrawlEventGenerator.
        /// </param>
        /// <param name='activityListener'>
        /// Activity listener.
        /// </param>
        /// <exception cref='ArgumentNullException'>
        /// <attribution license="cc4" from="Microsoft" modified="false" /><para>The exception that is thrown when a
        /// null reference (Nothing in Visual Basic) is passed to a method that does not accept it as a valid argument. </para>
        /// </exception>
        public DescendantsCrawler(
            ISyncEventQueue queue,
            IDescendantsTreeBuilder builder,
            CrawlEventGenerator generator,
            IActivityListener activityListener)
            : base(queue)
        {
            if (activityListener == null) {
                throw new ArgumentNullException("Given activityListener is null");
            }

            this.activityListener = activityListener;
            this.treebuilder = builder;
            this.eventGenerator = generator;
        }

        /// <summary>
        /// Handles StartNextSync events.
        /// </summary>
        /// <param name="e">The event to handle.</param>
        /// <returns>true if handled</returns>
        public override bool Handle(ISyncEvent e)
        {
            if(e is StartNextSyncEvent) {
                Logger.Debug("Starting DecendantsCrawlSync upon " + e);
                using (var activity = new ActivityListenerResource(this.activityListener)) {
                    this.CrawlDescendants();
                }

                this.Queue.AddEvent(new FullSyncCompletedEvent(e as StartNextSyncEvent));
                return true;
            }

            return false;
        }

        private void CrawlDescendants()
        {
            DescendantsTreeCollection trees = this.treebuilder.BuildTrees();

            CrawlEventCollection events = this.eventGenerator.GenerateEvents(trees);

            this.MergeAndSendEvents(events.mergableEvents);

            this.FindReportAndRemoveMutualDeletedObjects(events.removedRemoteObjects, events.removedLocalObjects);

            // Send out Events to queue
            this.InformAboutRemoteObjectsDeleted(events.removedRemoteObjects.Values);
            this.InformAboutLocalObjectsDeleted(events.removedLocalObjects.Values);
            events.creationEvents.ForEach(e => this.Queue.AddEvent(e));
        }

        private void MergeAndSendEvents(Dictionary<string, Tuple<AbstractFolderEvent, AbstractFolderEvent>> eventMap)
        {
            foreach (var entry in eventMap) {
                if (entry.Value == null) {
                    continue;
                } else if (entry.Value.Item1 == null && entry.Value.Item2 == null) {
                    continue;
                } else if (entry.Value.Item1 == null) {
                    if (entry.Value.Item2.Remote != MetaDataChangeType.NONE) {
                        this.Queue.AddEvent(entry.Value.Item2);
                    }
                } else if (entry.Value.Item2 == null) {
                    if (entry.Value.Item1.Local != MetaDataChangeType.NONE) {
                        this.Queue.AddEvent(entry.Value.Item1);
                    }
                } else {
                    var localEvent = entry.Value.Item1;
                    var remoteEvent = entry.Value.Item2;

                    var newEvent = FileOrFolderEventFactory.CreateEvent(
                        remoteEvent is FileEvent ? (IFileableCmisObject)(remoteEvent as FileEvent).RemoteFile : (IFileableCmisObject)(remoteEvent as FolderEvent).RemoteFolder,
                        localEvent is FileEvent ? (IFileSystemInfo)(localEvent as FileEvent).LocalFile : (IFileSystemInfo)(localEvent as FolderEvent).LocalFolder,
                        remoteEvent.Remote,
                        localEvent.Local,
                        remoteEvent.Remote == MetaDataChangeType.MOVED ? (remoteEvent is FileMovedEvent ? (remoteEvent as FileMovedEvent).OldRemoteFilePath : (remoteEvent as FolderMovedEvent).OldRemoteFolderPath) : null,
                        localEvent.Local == MetaDataChangeType.MOVED ? (localEvent is FileMovedEvent ? (IFileSystemInfo)(localEvent as FileMovedEvent).OldLocalFile : (IFileSystemInfo)(localEvent as FolderMovedEvent).OldLocalFolder) : null,
                        this);
                    if (newEvent is FileEvent) {
                        (newEvent as FileEvent).LocalContent = (localEvent as FileEvent).LocalContent;
                        (newEvent as FileEvent).RemoteContent = (remoteEvent as FileEvent).RemoteContent;
                    }

                    this.Queue.AddEvent(newEvent);
                }
            }
        }

        private void InformAboutLocalObjectsDeleted(IEnumerable<IFileSystemInfo> objects) {
            foreach (var deleted in objects) {
                if (deleted is IDirectoryInfo) {
                    this.Queue.AddEvent(new FolderEvent(deleted as IDirectoryInfo, null, this) { Local = MetaDataChangeType.DELETED });
                } else if (deleted is IFileInfo) {
                    this.Queue.AddEvent(new FileEvent(deleted as IFileInfo) { Local = MetaDataChangeType.DELETED });
                }
            }
        }

        private void InformAboutRemoteObjectsDeleted(IEnumerable<IFileSystemInfo> objects) {
            foreach (var deleted in objects) {
                AbstractFolderEvent deletedEvent = FileOrFolderEventFactory.CreateEvent(null, deleted, MetaDataChangeType.DELETED, src: this);
                this.Queue.AddEvent(deletedEvent);
            }
        }

        private void FindReportAndRemoveMutualDeletedObjects(IDictionary<string, IFileSystemInfo> removedRemoteObjects, IDictionary<string, IFileSystemInfo> removedLocalObjects) {
            IEnumerable<string> intersect = removedRemoteObjects.Keys.Intersect(removedLocalObjects.Keys);
            IList<string> mutualIds = new List<string>();
            foreach (var id in intersect) {
                AbstractFolderEvent deletedEvent = FileOrFolderEventFactory.CreateEvent(null, removedLocalObjects[id], MetaDataChangeType.DELETED, MetaDataChangeType.DELETED, src: this);
                mutualIds.Add(id);
                this.Queue.AddEvent(deletedEvent);
            }

            foreach(var id in mutualIds) {
                removedLocalObjects.Remove(id);
                removedRemoteObjects.Remove(id);
            }
        }
    }
}
