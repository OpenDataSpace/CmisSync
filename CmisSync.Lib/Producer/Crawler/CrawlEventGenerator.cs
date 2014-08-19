//-----------------------------------------------------------------------
// <copyright file="CrawlEventGenerator.cs" company="GRAU DATA AG">
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
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;

    public class CrawlEventGenerator
    {
        private IMetaDataStorage storage;
        private IFileSystemInfoFactory fsFactory;

        public CrawlEventGenerator(IMetaDataStorage storage, IFileSystemInfoFactory fsFactory = null)
        {
            if (storage == null) {
                throw new ArgumentNullException("Given storage is null");
            }

            this.storage = storage;
            if (fsFactory == null) {
                this.fsFactory = new FileSystemInfoFactory();
            } else {
                this.fsFactory = fsFactory;
            }
        }

        public CrawlEventCollection GenerateEvents(DescendantsTreeCollection trees) {
            IObjectTree<IMappedObject> storedTree = trees.StoredTree;
            IObjectTree<IFileSystemInfo> localTree = trees.LocalTree;
            IObjectTree<IFileableCmisObject> remoteTree = trees.RemoteTree;
            CrawlEventCollection createdEvents = new CrawlEventCollection();
            List<IMappedObject> storedObjectsForRemote = storedTree.ToList();
            List<IMappedObject> storedObjectsForLocal = new List<IMappedObject>(storedObjectsForRemote);

            Dictionary<string, Tuple<AbstractFolderEvent, AbstractFolderEvent>> eventMap = new Dictionary<string, Tuple<AbstractFolderEvent, AbstractFolderEvent>>();
            createdEvents.creationEvents = this.CreateRemoteEvents(storedObjectsForRemote, remoteTree, eventMap);
            createdEvents.creationEvents.AddRange(this.CreateLocalEvents(storedObjectsForLocal, localTree, eventMap));

            createdEvents.mergableEvents = eventMap;

            IMappedObject rootNode = storedTree.Item;
            storedObjectsForLocal.Remove(rootNode);
            storedObjectsForRemote.Remove(rootNode);

            createdEvents.removedLocalObjects = this.TransformToFileSystemInfoDict(storedObjectsForLocal);

            createdEvents.removedRemoteObjects = this.TransformToFileSystemInfoDict(storedObjectsForRemote);

            return createdEvents;
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

        private static AbstractFolderEvent GetCorrespondingRemoteEvent(Dictionary<string, Tuple<AbstractFolderEvent, AbstractFolderEvent>> eventMap, IMappedObject storedMappedChild)
        {
            AbstractFolderEvent correspondingRemoteEvent = null;
            Tuple<AbstractFolderEvent, AbstractFolderEvent> tuple;
            if (eventMap.TryGetValue(storedMappedChild.RemoteObjectId, out tuple)) {
                correspondingRemoteEvent = tuple.Item2;
            }

            return correspondingRemoteEvent;
        }

        private AbstractFolderEvent GenerateCreatedEvent(IFileSystemInfo fsInfo) {
            return FileOrFolderEventFactory.CreateEvent(null, fsInfo, localChange: MetaDataChangeType.CREATED, src: this);
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

        private IMappedObject FindStoredObjectByFileSystemInfo(List<IMappedObject> storedObjects, IFileSystemInfo fsInfo) {
            Guid childGuid;
            if (this.TryGetExtendedAttribute(fsInfo, out childGuid)) {
               return storedObjects.Find(o => o.Guid == childGuid);
            }

            return null;
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
    }
}
