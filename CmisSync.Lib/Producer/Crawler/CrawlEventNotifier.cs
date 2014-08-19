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
namespace CmisSync.Lib.Producer.Crawler
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
 
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Storage.FileSystem;
    
    using DotCMIS.Client;

    public class CrawlEventNotifier
    {
        private ISyncEventQueue Queue {get; set;}
        
        public CrawlEventNotifier(ISyncEventQueue queue)
        {
            this.Queue = queue;
        }
        
        public void MergeEventsAndAddToQueue(CrawlEventCollection events) {
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

