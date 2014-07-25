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
    using System.Threading.Tasks;

    using CmisSync.Lib.Cmis.ConvenienceExtenders;
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
        private IFolder remoteFolder;
        private IDirectoryInfo localFolder;
        private IMetaDataStorage storage;
        private IFileSystemInfoFactory fsFactory;
        private IFilterAggregator filter;
        private IActivityListener activityListener;
        private bool dropNextSyncEvents = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Sync.Strategy.DescendantsCrawler"/> class.
        /// </summary>
        /// <param name="queue">Sync Event Queue.</param>
        /// <param name="remoteFolder">Remote folder.</param>
        /// <param name="localFolder">Local folder.</param>
        /// <param name="storage">Meta data storage.</param>
        /// <param name="filter">Aggregated filter.</param>
        /// <param name ="activityListener">Activity listner.</param>
        /// <param name="fsFactory">File system info factory.</param>
        public DescendantsCrawler(
            ISyncEventQueue queue,
            IFolder remoteFolder,
            IDirectoryInfo localFolder,
            IMetaDataStorage storage,
            IFilterAggregator filter,
            IActivityListener activityListener,
            bool serverSupportsHashes = false,
            IFileSystemInfoFactory fsFactory = null)
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

            this.storage = storage;
            this.remoteFolder = remoteFolder;
            this.localFolder = localFolder;
            this.filter = filter;
            this.activityListener = activityListener;

            if (fsFactory == null) {
                this.fsFactory = new FileSystemInfoFactory();
            } else {
                this.fsFactory = fsFactory;
            }
        }

        /// <summary>
        /// Handles StartNextSync events.
        /// </summary>
        /// <param name="e">The event to handle.</param>
        /// <returns>true if handled</returns>
        public override bool Handle(ISyncEvent e)
        {
            if(e is StartNextSyncEvent) {
                if (this.dropNextSyncEvents) {
                    return true;
                }

                Logger.Debug("Starting DecendantsCrawlSync upon " + e);
                try {
                    using (var activity = new ActivityListenerResource(this.activityListener)) {
                        this.CrawlDescendants();
                    }
                } finally {
                    this.dropNextSyncEvents = true;
                    Queue.AddEvent(new ResetStartNextCrawlSyncFilterEvent());
                }

                this.Queue.AddEvent(new FullSyncCompletedEvent(e as StartNextSyncEvent));
                return true;
            }

            if(e is ResetStartNextCrawlSyncFilterEvent) {
                this.dropNextSyncEvents = false;
                return true;
            }

            return false;
        }

        private static AbstractFolderEvent GetCorrespondingRemoteEvent(Dictionary<string, Tuple<AbstractFolderEvent, AbstractFolderEvent>> eventMap, IMappedObject storedMappedChild)
        {
            AbstractFolderEvent correspondingRemoteEvent = null;
            Tuple<AbstractFolderEvent, AbstractFolderEvent> tuple;
            if (eventMap.TryGetValue(storedMappedChild.RemoteObjectId, out tuple)) {
                correspondingRemoteEvent = tuple.Item2;
            }

            return correspondingRemoteEvent;
        }

        private void CrawlDescendants()
        {
            IObjectTree<IMappedObject> storedTree = null;
            IObjectTree<IFileSystemInfo> localTree = null;
            IObjectTree<IFileableCmisObject> remoteTree = null;

            // Request 3 trees in parallel
            Task[] tasks = new Task[3];
            tasks[0] = Task.Factory.StartNew(() => storedTree = this.storage.GetObjectTree());
            tasks[1] = Task.Factory.StartNew(() => localTree = GetLocalDirectoryTree(this.localFolder, this.filter));
            tasks[2] = Task.Factory.StartNew(() => remoteTree = GetRemoteDirectoryTree(this.remoteFolder, this.remoteFolder.GetDescendants(-1), this.filter));

            // Wait until all tasks are finished
            Task.WaitAll(tasks);

            List<IMappedObject> storedObjectsForRemote = storedTree.ToList();
            List<IMappedObject> storedObjectsForLocal = new List<IMappedObject>(storedObjectsForRemote);

            Dictionary<string, Tuple<AbstractFolderEvent, AbstractFolderEvent>> eventMap = new Dictionary<string, Tuple<AbstractFolderEvent, AbstractFolderEvent>>();
            this.CreateRemoteEvents(storedObjectsForRemote, remoteTree, eventMap);
            this.CreateLocalEvents(storedObjectsForLocal, localTree, eventMap);

            Dictionary<string, IFileSystemInfo> removedLocalObjects = new Dictionary<string, IFileSystemInfo>();
            Dictionary<string, IFileSystemInfo> removedRemoteObjects = new Dictionary<string, IFileSystemInfo>();

            storedObjectsForLocal.Remove(storedTree.Item);
            storedObjectsForRemote.Remove(storedTree.Item);

            foreach (var localDeleted in storedObjectsForLocal) {
                string path = this.storage.GetLocalPath(localDeleted);
                IFileSystemInfo info = localDeleted.Type == MappedObjectType.File ? (IFileSystemInfo)this.fsFactory.CreateFileInfo(path) : (IFileSystemInfo)this.fsFactory.CreateDirectoryInfo(path);
                removedLocalObjects.Add(localDeleted.RemoteObjectId, info);
            }

            foreach (var remoteDeleted in storedObjectsForRemote) {
                string path = this.storage.GetLocalPath(remoteDeleted);
                IFileSystemInfo info = remoteDeleted.Type == MappedObjectType.File ? (IFileSystemInfo)this.fsFactory.CreateFileInfo(path) : (IFileSystemInfo)this.fsFactory.CreateDirectoryInfo(path);
                removedRemoteObjects.Add(remoteDeleted.RemoteObjectId, info);
            }

            this.MergeAndSendEvents(eventMap);

            this.FindReportAndRemoveMutualDeletedObjects(removedRemoteObjects, removedLocalObjects);

            // Send out Events to queue
            this.InformAboutRemoteObjectsDeleted(removedRemoteObjects.Values);
            this.InformAboutLocalObjectsDeleted(removedLocalObjects.Values);
        }

        private AbstractFolderEvent CreateLocalEventBasedOnStorage(IFileSystemInfo fsObject, IMappedObject storedParent, IMappedObject storedMappedChild)
        {
            AbstractFolderEvent createdEvent = null;
            if (storedMappedChild.ParentId == storedParent.RemoteObjectId) {
                // Renamed, Updated or Equal
                if (fsObject.Name == storedMappedChild.Name && fsObject.LastWriteTimeUtc == storedMappedChild.LastLocalWriteTimeUtc) {
                    // Equal
                    createdEvent = FileOrFolderEventFactory.CreateEvent(null, fsObject, localChange: MetaDataChangeType.NONE, src: this);
                } else {
                    // Updated or Renamed
                    createdEvent = FileOrFolderEventFactory.CreateEvent(null, fsObject, localChange: MetaDataChangeType.CHANGED, src: this);
                }
            } else {
                // Moved
                IFileSystemInfo oldLocalPath = fsObject is IFileInfo ? (IFileSystemInfo)this.fsFactory.CreateFileInfo(this.storage.GetLocalPath(storedMappedChild)) : (IFileSystemInfo)this.fsFactory.CreateDirectoryInfo(this.storage.GetLocalPath(storedMappedChild));
                createdEvent = FileOrFolderEventFactory.CreateEvent(null, fsObject, localChange: MetaDataChangeType.MOVED, oldLocalObject: oldLocalPath, src: this);
            }

            return createdEvent;
        }

        private void CreateLocalEvents(
            List<IMappedObject> storedObjects,
            IObjectTree<IFileSystemInfo> localTree,
            Dictionary<string, Tuple<AbstractFolderEvent, AbstractFolderEvent>> eventMap)
        {
            var parent = localTree.Item;
            IMappedObject storedParent = null;
            Guid guid;

            if (this.TryGetExtendedAttribute(parent, out guid)) {
                storedParent = storedObjects.Find(o => o.Guid.Equals(guid));
            }

            foreach (var child in localTree.Children) {
                Guid childGuid;
                IMappedObject storedMappedChild = null;
                if (this.TryGetExtendedAttribute(child.Item, out childGuid)) {
                    storedMappedChild = storedObjects.Find(o => o.Guid == childGuid);
                    if (storedMappedChild != null) {
                        // Moved, Renamed, Updated or Equal
                        AbstractFolderEvent correspondingRemoteEvent = GetCorrespondingRemoteEvent(eventMap, storedMappedChild);
                        AbstractFolderEvent createdEvent = this.CreateLocalEventBasedOnStorage(child.Item, storedParent, storedMappedChild);

                        eventMap[storedMappedChild.RemoteObjectId] = new Tuple<AbstractFolderEvent, AbstractFolderEvent>(createdEvent, correspondingRemoteEvent);
                    } else {
                        // Added
                        AbstractFolderEvent addEvent = FileOrFolderEventFactory.CreateEvent(null, child.Item, localChange: MetaDataChangeType.CREATED, src: this);
                        Queue.AddEvent(addEvent);
                    }
                } else {
                    // Added
                    AbstractFolderEvent addEvent = FileOrFolderEventFactory.CreateEvent(null, child.Item, localChange: MetaDataChangeType.CREATED, src: this);
                    Queue.AddEvent(addEvent);
                }

                this.CreateLocalEvents(storedObjects, child, eventMap);
                if (storedMappedChild != null) {
                    storedObjects.Remove(storedMappedChild);
                }
            }
        }

        private AbstractFolderEvent CreateRemoteEventBasedOnStorage(IFileableCmisObject cmisObject, IMappedObject storedParent, IMappedObject storedMappedChild)
        {
            AbstractFolderEvent newEvent = null;
            if (storedMappedChild.ParentId == storedParent.RemoteObjectId) {
                // Renamed or Equal
                if (storedMappedChild.Name == cmisObject.Name) {
                    // Equal or property update
                    if (storedMappedChild.LastChangeToken != cmisObject.ChangeToken) {
                        // Update
                        newEvent = FileOrFolderEventFactory.CreateEvent(cmisObject, null, MetaDataChangeType.CHANGED, src: this);
                        AddRemoteContentChangeTypeToFileEvent(newEvent as FileEvent, storedMappedChild, cmisObject as IDocument);
                    } else {
                        // Equal
                        newEvent = FileOrFolderEventFactory.CreateEvent(cmisObject, null, MetaDataChangeType.NONE, src: this);
                    }
                } else {
                    // Renamed
                    newEvent = FileOrFolderEventFactory.CreateEvent(cmisObject, null, MetaDataChangeType.CHANGED, src: this);
                    AddRemoteContentChangeTypeToFileEvent(newEvent as FileEvent, storedMappedChild, cmisObject as IDocument);
                }
            } else {
                // Moved
                newEvent = FileOrFolderEventFactory.CreateEvent(cmisObject, null, MetaDataChangeType.MOVED, oldRemotePath: this.storage.GetRemotePath(storedMappedChild), src: this);
                AddRemoteContentChangeTypeToFileEvent(newEvent as FileEvent, storedMappedChild, cmisObject as IDocument);
            }

            return newEvent;
        }

        private void CreateRemoteEvents(List<IMappedObject> storedObjects, IObjectTree<IFileableCmisObject> remoteTree, Dictionary<string, Tuple<AbstractFolderEvent, AbstractFolderEvent>> eventMap)
        {
            var storedParent = storedObjects.Find(o => o.RemoteObjectId == remoteTree.Item.Id);

            foreach (var child in remoteTree.Children) {
                var storedMappedChild = storedObjects.Find(o => o.RemoteObjectId == child.Item.Id);
                if (storedMappedChild != null) {
                    AbstractFolderEvent newEvent = this.CreateRemoteEventBasedOnStorage(child.Item, storedParent, storedMappedChild);
                    eventMap[child.Item.Id] = new Tuple<AbstractFolderEvent, AbstractFolderEvent>(null, newEvent);
                } else {
                    // Added
                    AbstractFolderEvent addEvent = FileOrFolderEventFactory.CreateEvent(child.Item, null, MetaDataChangeType.CREATED, src: this);
                    this.Queue.AddEvent(addEvent);
                }

                this.CreateRemoteEvents(storedObjects, child, eventMap);
                if (storedMappedChild != null) {
                    storedObjects.Remove(storedMappedChild);
                }
            }
        }

        private bool TryGetExtendedAttribute(IFileSystemInfo fsInfo, out Guid guid) {
            string ea = fsInfo.GetExtendedAttribute(MappedObject.ExtendedAttributeKey);
            if (!string.IsNullOrEmpty(ea) &&
                Guid.TryParse(ea, out guid)) {
                return true;
            } else {
                guid = Guid.Empty;
                return false;
            }
        }

        private static void AddRemoteContentChangeTypeToFileEvent(FileEvent fileEvent, IMappedObject obj, IDocument remoteDoc) {
            if (fileEvent == null || obj == null || remoteDoc == null) {
                return;
            }

            byte[] remoteHash = remoteDoc.ContentStreamHash(obj.ChecksumAlgorithmName);
            if (remoteHash != null && remoteHash.SequenceEqual(obj.LastChecksum)) {
                fileEvent.RemoteContent = ContentChangeType.NONE;
            } else {
                fileEvent.RemoteContent = ContentChangeType.CHANGED;
            }
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

                    if (this.IsSymmetricNoneTuple(localEvent, remoteEvent)) {
                        continue;
                    }

                    var newEvent = FileOrFolderEventFactory.CreateEvent(
                        remoteEvent is FileEvent ? (IFileableCmisObject)(remoteEvent as FileEvent).RemoteFile : (IFileableCmisObject)(remoteEvent as FolderEvent).RemoteFolder,
                        localEvent is FileEvent ? (IFileSystemInfo)(localEvent as FileEvent).LocalFile : (IFileSystemInfo)(localEvent as FolderEvent).LocalFolder,
                        remoteEvent.Remote,
                        localEvent.Local,
                        remoteEvent.Remote == MetaDataChangeType.MOVED ? (remoteEvent is FileMovedEvent ? (remoteEvent as FileMovedEvent).OldRemoteFilePath : (remoteEvent as FolderMovedEvent).OldRemoteFolderPath) : null,
                        localEvent.Local == MetaDataChangeType.MOVED ? (localEvent is FileMovedEvent ? (IFileSystemInfo)(localEvent as FileMovedEvent).OldLocalFile : (IFileSystemInfo)(localEvent as FolderMovedEvent).OldLocalFolder) : null,
                        this);
                    this.Queue.AddEvent(newEvent);
                }
            }
        }

        private bool IsSymmetricNoneTuple(AbstractFolderEvent local, AbstractFolderEvent remote) {
            if (local.Local != MetaDataChangeType.NONE) {
                return false;
            }

            if (remote.Remote != MetaDataChangeType.NONE) {
                return false;
            }

            FileEvent localFileEvent = local as FileEvent;
            if (localFileEvent != null) {
                if (localFileEvent.LocalContent != ContentChangeType.NONE) {
                    return false;
                }
            }

            FileEvent remoteFileEvent = remote as FileEvent;
            if (remoteFileEvent != null) {
                if (remoteFileEvent.RemoteContent != ContentChangeType.NONE) {
                    return false;
                }
            }

            return true;
        }

        private void InformAboutRemoteObjectsAdded(IList<IFileableCmisObject> objects) {
            foreach (var addedRemotely in objects) {
                if (addedRemotely is IFolder) {
                    this.Queue.AddEvent(new FolderEvent(null, addedRemotely as IFolder, this) { Remote = MetaDataChangeType.CREATED });
                } else if (addedRemotely is IDocument) {
                    this.Queue.AddEvent(new FileEvent(null, addedRemotely as IDocument) { Remote = MetaDataChangeType.CREATED });
                }
            }
        }

        private void InformAboutLocalObjectsAdded(IList<IFileSystemInfo> objects) {
            foreach (var addedLocally in objects) {
                if (addedLocally is IDirectoryInfo) {
                    this.Queue.AddEvent(new FolderEvent(addedLocally as IDirectoryInfo, null, this) { Local = MetaDataChangeType.CREATED });
                } else if (addedLocally is IFileInfo) {
                    this.Queue.AddEvent(new FileEvent(addedLocally as IFileInfo, null) { Local = MetaDataChangeType.CREATED });
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

        /// <summary>
        /// Gets the local directory tree.
        /// </summary>
        /// <returns>The local directory tree.</returns>
        /// <param name="parent">Parent directory.</param>
        /// <param name="filter">Filter for files.</param>
        public static IObjectTree<IFileSystemInfo> GetLocalDirectoryTree(IDirectoryInfo parent, IFilterAggregator filter) {
            var children = new List<IObjectTree<IFileSystemInfo>>();
            foreach (var child in parent.GetDirectories()) {
                string reason;
                if (!filter.InvalidFolderNamesFilter.CheckFolderName(child.Name, out reason) && !filter.FolderNamesFilter.CheckFolderName(child.Name, out reason)) {
                    children.Add(GetLocalDirectoryTree(child, filter));
                } else {
                    Logger.Debug(reason);
                }
            }

            foreach (var file in parent.GetFiles()) {
                string reason;
                if (!filter.FileNamesFilter.CheckFile(file.Name, out reason)) {
                    children.Add(new ObjectTree<IFileSystemInfo> {
                        Item = file,
                        Children = new List<IObjectTree<IFileSystemInfo>>()
                    });
                } else {
                    Logger.Debug(reason);
                }
            }

            IObjectTree<IFileSystemInfo> tree = new ObjectTree<IFileSystemInfo> {
                Item = parent,
                Children = children
            };
            return tree;
        }

        /// <summary>
        /// Gets the remote directory tree.
        /// </summary>
        /// <returns>The remote directory tree.</returns>
        /// <param name="parent">Parent folder.</param>
        /// <param name="descendants">Descendants of remote object.</param>
        /// <param name="filter">Filter of ignored or invalid files and folder</param>
        public static IObjectTree<IFileableCmisObject> GetRemoteDirectoryTree(IFolder parent, IList<ITree<IFileableCmisObject>> descendants, IFilterAggregator filter) {
            IList<IObjectTree<IFileableCmisObject>> children = new List<IObjectTree<IFileableCmisObject>>();
            if (descendants != null) {
                foreach (var child in descendants) {
                    if (child.Item is IFolder) {
                        string reason;
                        if (!filter.FolderNamesFilter.CheckFolderName(child.Item.Name, out reason) && !filter.InvalidFolderNamesFilter.CheckFolderName(child.Item.Name, out reason)) {
                            children.Add(GetRemoteDirectoryTree(child.Item as IFolder, child.Children, filter));
                        } else {
                            Logger.Debug(reason);
                        }
                    } else if (child.Item is IDocument) {
                        string reason;
                        if (!filter.FileNamesFilter.CheckFile(child.Item.Name, out reason)) {
                            children.Add(new ObjectTree<IFileableCmisObject> {
                                Item = child.Item,
                                Children = new List<IObjectTree<IFileableCmisObject>>()
                            });
                        } else {
                            Logger.Debug(reason);
                        }
                    }
                }
            }

            var tree = new ObjectTree<IFileableCmisObject> {
                Item = parent,
                Children = children
            };

            return tree;
        }

        private class ResetStartNextCrawlSyncFilterEvent : ISyncEvent, IRemoveFromLoggingEvent {
            public override string ToString()
            {
                return string.Format("[ResetStartNextCrawlSyncFilterEvent]");
            }
        }
    }
}
