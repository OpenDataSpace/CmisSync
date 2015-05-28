//-----------------------------------------------------------------------
// <copyright file="CrawlEventNotifier.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Producer.Crawler {
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;

    /// <summary>
    /// Crawl event notifier takes crawled events and notifies the queue about them.
    /// </summary>
    public class CrawlEventNotifier {
        private ISyncEventQueue queue;

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Producer.Crawler.CrawlEventNotifier"/> class.
        /// </summary>
        /// <param name="queue">Sync event queue which should be notified about the crawled events.</param>
        public CrawlEventNotifier(ISyncEventQueue queue) {
            if (queue == null) {
                throw new ArgumentNullException("queue");
            }

            this.queue = queue;
        }

        public void MergeEventsAndAddToQueue(CrawlEventCollection events) {
            if (events.creationEvents == null) {
                throw new ArgumentNullException("events", "Given creationEvents list is null");
            }

            if (events.mergableEvents == null) {
                throw new ArgumentNullException("events", "Given mergable events are null");
            }

            this.MergeAndSendEvents(events.mergableEvents);
            events.creationEvents.ForEach(e => this.queue.AddEvent(e));
        }

        private void MergeAndSendEvents(Dictionary<string, Tuple<AbstractFolderEvent, AbstractFolderEvent>> eventMap) {
            foreach (var entry in eventMap) {
                if (entry.Value == null) {
                    continue;
                } else if (entry.Value.Item1 == null && entry.Value.Item2 == null) {
                    continue;
                } else if (entry.Value.Item1 == null) {
                    if (entry.Value.Item2.Remote != MetaDataChangeType.NONE) {
                        this.queue.AddEvent(entry.Value.Item2);
                    }
                } else if (entry.Value.Item2 == null) {
                    if (entry.Value.Item1.Local != MetaDataChangeType.NONE) {
                        this.queue.AddEvent(entry.Value.Item1);
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

                    this.queue.AddEvent(newEvent);
                }
            }
        }
    }
}