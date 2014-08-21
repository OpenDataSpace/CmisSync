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
        private LocalEventGenerator localEventGenerator;
        private RemoteEventGenerator remoteEventGenerator;

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

            Dictionary<string, Tuple<AbstractFolderEvent, AbstractFolderEvent>> eventMap = new Dictionary<string, Tuple<AbstractFolderEvent, AbstractFolderEvent>>();
            createdEvents.creationEvents = this.remoteEventGenerator.CreateEvents(storedObjectsForRemote, remoteTree, eventMap);
            createdEvents.creationEvents.AddRange(this.localEventGenerator.CreateEvents(storedObjectsForLocal, localTree, eventMap));

            createdEvents.mergableEvents = eventMap;

            IMappedObject rootNode = storedTree.Item;
            storedObjectsForLocal.Remove(rootNode);
            storedObjectsForRemote.Remove(rootNode);

            createdEvents.removedLocalObjects = this.TransformToFileSystemInfoDict(storedObjectsForLocal);
            createdEvents.removedRemoteObjects = this.TransformToFileSystemInfoDict(storedObjectsForRemote);

            return createdEvents;
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
    }
}
