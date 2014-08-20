//-----------------------------------------------------------------------
// <copyright file="LocalEventGenerator.cs" company="GRAU DATA AG">
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

    /// <summary>
    /// Generates ChangeEvents from the local tree and a list of all stored objects.
    /// </summary>
    /// <exception cref='ArgumentNullException'>
    /// <attribution license="cc4" from="Microsoft" modified="false" /><para>The exception that is thrown when a null
    /// reference (Nothing in Visual Basic) is passed to a method that does not accept it as a valid argument. </para>
    /// </exception>
    public class LocalEventGenerator
    {
        private IMetaDataStorage storage;
        private IFileSystemInfoFactory fsFactory;

        public LocalEventGenerator(IMetaDataStorage storage, IFileSystemInfoFactory fsFactory = null)
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

        public List<AbstractFolderEvent> CreateLocalEvents(
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

        private IMappedObject FindStoredObjectByFileSystemInfo(List<IMappedObject> storedObjects, IFileSystemInfo fsInfo) {
            Guid childGuid;
            if (this.TryGetExtendedAttribute(fsInfo, out childGuid)) {
               return storedObjects.Find(o => o.Guid == childGuid);
            }

            return null;
        }
    }
}
