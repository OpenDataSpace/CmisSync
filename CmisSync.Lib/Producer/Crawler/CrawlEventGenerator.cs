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
namespace CmisSync.Lib.Producer.Crawler {
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CmisSync.Lib.Cmis.ConvenienceExtenders;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;

    public class CrawlEventGenerator {
        private IMetaDataStorage storage;
        private IFileSystemInfoFactory fsFactory;
        private LocalEventGenerator localEventGenerator;
        private RemoteEventGenerator remoteEventGenerator;

        public CrawlEventGenerator(IMetaDataStorage storage, IFileSystemInfoFactory fsFactory = null) {
            if (storage == null) {
                throw new ArgumentNullException("storage");
            }

            this.storage = storage;
            if (fsFactory == null) {
                this.fsFactory = new FileSystemInfoFactory();
            } else {
                this.fsFactory = fsFactory;
            }

            this.localEventGenerator = new LocalEventGenerator(this.storage, this.fsFactory);
            this.remoteEventGenerator = new RemoteEventGenerator(this.storage);
        }

        public CrawlEventCollection GenerateEvents(DescendantsTreeCollection trees) {
            IObjectTree<IMappedObject> storedTree = trees.StoredTree;
            IObjectTree<IFileSystemInfo> localTree = trees.LocalTree;
            IObjectTree<IFileableCmisObject> remoteTree = trees.RemoteTree;
            CrawlEventCollection createdEvents = new CrawlEventCollection();
            List<IMappedObject> storedObjectsForRemote = storedTree.ToList();
            List<IMappedObject> storedObjectsForLocal = new List<IMappedObject>(storedObjectsForRemote);
            ISet<IMappedObject> handledLocalStoredObjects = new HashSet<IMappedObject>();
            ISet<IMappedObject> handledRemoteStoredObjects = new HashSet<IMappedObject>();
            Dictionary<string, Tuple<AbstractFolderEvent, AbstractFolderEvent>> eventMap = new Dictionary<string, Tuple<AbstractFolderEvent, AbstractFolderEvent>>();
            createdEvents.creationEvents = this.remoteEventGenerator.CreateEvents(storedObjectsForRemote, remoteTree, eventMap, handledRemoteStoredObjects);
            createdEvents.creationEvents.AddRange(this.localEventGenerator.CreateEvents(storedObjectsForLocal, localTree, eventMap, handledLocalStoredObjects));

            createdEvents.mergableEvents = eventMap;

            handledLocalStoredObjects.Add(storedTree.Item);
            handledRemoteStoredObjects.Add(storedTree.Item);

            foreach (var handledObject in handledLocalStoredObjects) {
                storedObjectsForLocal.Remove(handledObject);
            }

            foreach (var handledObject in handledRemoteStoredObjects) {
                storedObjectsForRemote.Remove(handledObject);
            }

            this.AddDeletedObjectsToMergableEvents(storedObjectsForLocal, eventMap, true);
            this.AddDeletedObjectsToMergableEvents(storedObjectsForRemote, eventMap, false);

            return createdEvents;
        }

        private void AddDeletedObjectsToMergableEvents(
            List<IMappedObject> storedObjectList,
            Dictionary<string, Tuple<AbstractFolderEvent, AbstractFolderEvent>> eventMap,
            bool areLocalEvents)
        {
            foreach (var deleted in storedObjectList) {
                string path = this.storage.GetLocalPath(deleted);
                if (path == null) {
                    continue;
                }

                IFileSystemInfo info = deleted.Type == MappedObjectType.File ? (IFileSystemInfo)this.fsFactory.CreateFileInfo(path) : (IFileSystemInfo)this.fsFactory.CreateDirectoryInfo(path);
                var newEvent = FileOrFolderEventFactory.CreateEvent(
                    null,
                    info,
                    areLocalEvents ? MetaDataChangeType.NONE : MetaDataChangeType.DELETED,
                    areLocalEvents ? MetaDataChangeType.DELETED : MetaDataChangeType.NONE,
                    areLocalEvents ? this.storage.GetRemotePath(deleted) : null,
                    areLocalEvents ? null : info,
                    src: this);
                if (!eventMap.ContainsKey(deleted.RemoteObjectId)) {
                    eventMap[deleted.RemoteObjectId] = new Tuple<AbstractFolderEvent, AbstractFolderEvent>(
                        areLocalEvents ? newEvent : null,
                        areLocalEvents ? null : newEvent);
                } else {
                    eventMap[deleted.RemoteObjectId] = new Tuple<AbstractFolderEvent, AbstractFolderEvent>(
                        areLocalEvents ? newEvent : eventMap[deleted.RemoteObjectId].Item1,
                        areLocalEvents ? eventMap[deleted.RemoteObjectId].Item2 : newEvent);
                }
            }
        }
    }
}