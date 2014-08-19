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
        private IMetaDataStorage storage;
        private IFileSystemInfoFactory fsFactory;
        private IActivityListener activityListener;
        private IDescendantsTreeBuilder treebuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="DescendantsCrawler"/> class.
        /// </summary>
        /// <param name="queue">Sync Event Queue.</param>
        /// <param name="remoteFolder">Remote folder.</param>
        /// <param name="localFolder">Local folder.</param>
        /// <param name="storage">Meta data storage.</param>
        /// <param name="filter">Aggregated filter.</param>
        /// <param name="activityListener">Activity listner.</param>
        /// <param name="serverSupportsHashes">Indicates whether this server supports content hashes.</param>
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
            this.activityListener = activityListener;
            this.treebuilder = new DescendantsTreeBuilder(storage, remoteFolder, localFolder, filter);

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
                Logger.Debug("Starting DecendantsCrawlSync upon " + e);
                using (var activity = new ActivityListenerResource(this.activityListener)) {
                    this.CrawlDescendants();
                }

                this.Queue.AddEvent(new FullSyncCompletedEvent(e as StartNextSyncEvent));
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
        
        private Dictionary<string, IFileSystemInfo> TransformToFileSystemInfoDict(List<IMappedObject> storedObjectList) {
            Dictionary<string, IFileSystemInfo> ret = new Dictionary<string, IFileSystemInfo>();
            foreach (var localDeleted in storedObjectList) {
                string path = this.storage.GetLocalPath(localDeleted);
                IFileSystemInfo info = localDeleted.Type == MappedObjectType.File ? (IFileSystemInfo)this.fsFactory.CreateFileInfo(path) : (IFileSystemInfo)this.fsFactory.CreateDirectoryInfo(path);
                ret.Add(localDeleted.RemoteObjectId, info);
            }
            return ret;
        }

        private void CrawlDescendants()
        {
            DescendantsTreeCollection trees = this.treebuilder.BuildTrees();
            IObjectTree<IMappedObject> storedTree = trees.StoredTree;
            IObjectTree<IFileSystemInfo> localTree = trees.LocalTree;
            IObjectTree<IFileableCmisObject> remoteTree = trees.RemoteTree;

            List<IMappedObject> storedObjectsForRemote = storedTree.ToList();
            List<IMappedObject> storedObjectsForLocal = new List<IMappedObject>(storedObjectsForRemote);

            Dictionary<string, Tuple<AbstractFolderEvent, AbstractFolderEvent>> eventMap = new Dictionary<string, Tuple<AbstractFolderEvent, AbstractFolderEvent>>();
            List<AbstractFolderEvent> remoteCreationEvents = this.CreateRemoteEvents(storedObjectsForRemote, remoteTree, eventMap);
            List<AbstractFolderEvent> localCreationEvents = this.CreateLocalEvents(storedObjectsForLocal, localTree, eventMap);

            IMappedObject rootNode = storedTree.Item;
            storedObjectsForLocal.Remove(rootNode);
            storedObjectsForRemote.Remove(rootNode);
            
            Dictionary<string, IFileSystemInfo> removedLocalObjects = this.TransformToFileSystemInfoDict(storedObjectsForLocal); 

            Dictionary<string, IFileSystemInfo> removedRemoteObjects = this.TransformToFileSystemInfoDict(storedObjectsForRemote);

            this.MergeAndSendEvents(eventMap);

            this.FindReportAndRemoveMutualDeletedObjects(removedRemoteObjects, removedLocalObjects);

            // Send out Events to queue
            this.InformAboutRemoteObjectsDeleted(removedRemoteObjects.Values);
            this.InformAboutLocalObjects(removedLocalObjects.Values, MetaDataChangeType.DELETED);
            remoteCreationEvents.ForEach( e => this.Queue.AddEvent(e));
            localCreationEvents.ForEach( e => this.Queue.AddEvent(e));
        }

        private AbstractFolderEvent CreateLocalEventBasedOnStorage(IFileSystemInfo fsObject, IMappedObject storedParent, IMappedObject storedMappedChild)
        {
            AbstractFolderEvent createdEvent = null;
            if (storedMappedChild.ParentId == storedParent.RemoteObjectId) {
                // Renamed, Updated or Equal
                if (fsObject.Name == storedMappedChild.Name && fsObject.LastWriteTimeUtc == storedMappedChild.LastLocalWriteTimeUtc) {
                    // Equal
                    createdEvent = null; 
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

        private List<AbstractFolderEvent> CreateLocalEvents(
            List<IMappedObject> storedObjects,
            IObjectTree<IFileSystemInfo> localTree,
            Dictionary<string, Tuple<AbstractFolderEvent, AbstractFolderEvent>> eventMap)
        {
            List<AbstractFolderEvent> creationEvents = new List<AbstractFolderEvent>();
            var parent = localTree.Item;
            IMappedObject storedParent = null;
            Guid guid;

            if (this.TryGetExtendedAttribute(parent, out guid)) {
                storedParent = storedObjects.Find(o => o.Guid.Equals(guid));
            }

            foreach (var child in localTree.Children) {
                bool removeStoredMappedChild = false;
                
                IMappedObject storedMappedChild = this.FindStoredObjectByFileSystemInfo(storedObjects, child.Item);
                if (storedMappedChild != null) {
                    var localPath = this.storage.GetLocalPath(storedMappedChild);
                    if((!localPath.Equals(child.Item.FullName)) && this.fsFactory.IsDirectory(localPath) != null) {
                        // Copied
                        creationEvents.Add(this.GenerateCreatedEvent(child.Item));
                    } else {
                        // Moved, Renamed, Updated or Equal
                        AbstractFolderEvent correspondingRemoteEvent = GetCorrespondingRemoteEvent(eventMap, storedMappedChild);
                        AbstractFolderEvent createdEvent = this.CreateLocalEventBasedOnStorage(child.Item, storedParent, storedMappedChild);

                        eventMap[storedMappedChild.RemoteObjectId] = new Tuple<AbstractFolderEvent, AbstractFolderEvent>(createdEvent, correspondingRemoteEvent);
                        removeStoredMappedChild = true;
                    }
                } else {
                    // Added
                    creationEvents.Add(this.GenerateCreatedEvent(child.Item));
                }

                creationEvents.AddRange(this.CreateLocalEvents(storedObjects, child, eventMap));

                if(removeStoredMappedChild) {
                    storedObjects.Remove(storedMappedChild);
                }
            }
            return creationEvents;
        }

        private IMappedObject FindStoredObjectByFileSystemInfo(List<IMappedObject> storedObjects, IFileSystemInfo fsInfo) {
            Guid childGuid;
            if (this.TryGetExtendedAttribute(fsInfo, out childGuid)) {
               return storedObjects.Find(o => o.Guid == childGuid);
            }

            return null;
        }

        private AbstractFolderEvent GenerateCreatedEvent(IFileSystemInfo fsInfo) {
            return FileOrFolderEventFactory.CreateEvent(null, fsInfo, localChange: MetaDataChangeType.CREATED, src: this);
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
                        newEvent = null;
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

        private List<AbstractFolderEvent> CreateRemoteEvents(List<IMappedObject> storedObjects, IObjectTree<IFileableCmisObject> remoteTree, Dictionary<string, Tuple<AbstractFolderEvent, AbstractFolderEvent>> eventMap)
        {
            List<AbstractFolderEvent> createdEvents = new List<AbstractFolderEvent>();
            var storedParent = storedObjects.Find(o => o.RemoteObjectId == remoteTree.Item.Id);

            foreach (var child in remoteTree.Children) {
                var storedMappedChild = storedObjects.Find(o => o.RemoteObjectId == child.Item.Id);
                if (storedMappedChild != null) {
                    AbstractFolderEvent newEvent = this.CreateRemoteEventBasedOnStorage(child.Item, storedParent, storedMappedChild);
                    eventMap[child.Item.Id] = new Tuple<AbstractFolderEvent, AbstractFolderEvent>(null, newEvent);
                } else {
                    // Added
                    AbstractFolderEvent addEvent = FileOrFolderEventFactory.CreateEvent(child.Item, null, MetaDataChangeType.CREATED, src: this);
                    createdEvents.Add(addEvent);
                }

                createdEvents.AddRange(this.CreateRemoteEvents(storedObjects, child, eventMap));
                if (storedMappedChild != null) {
                    storedObjects.Remove(storedMappedChild);
                }
            }
            return createdEvents;
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

        private void InformAboutLocalObjects(IEnumerable<IFileSystemInfo> objects, MetaDataChangeType changeType) {
            foreach (var deleted in objects) {
                if (deleted is IDirectoryInfo) {
                    this.Queue.AddEvent(new FolderEvent(deleted as IDirectoryInfo, null, this) { Local = changeType });
                } else if (deleted is IFileInfo) {
                    this.Queue.AddEvent(new FileEvent(deleted as IFileInfo) { Local = changeType });
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
