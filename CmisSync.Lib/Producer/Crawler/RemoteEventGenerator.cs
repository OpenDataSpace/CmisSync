//-----------------------------------------------------------------------
// <copyright file="RemoteEventGenerator.cs" company="GRAU DATA AG">
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
namespace CmisSync.Lib
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CmisSync.Lib.Cmis.ConvenienceExtenders;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.Database.Entities;

    using DotCMIS.Client;
    public class RemoteEventGenerator
    {
        private IMetaDataStorage storage;

        public RemoteEventGenerator(IMetaDataStorage storage)
        {
            if (storage == null) {
                throw new ArgumentNullException("storage");
            }

            this.storage = storage;
        }

        /// <summary>
        /// Creates the local events and returns the creationEvents, the other Events are stored in the eventMap, handled objects are removed from storedObjects.
        /// </summary>
        /// <returns>
        /// The remote events.
        /// </returns>
        /// <param name='storedObjects'>
        /// Stored objects.
        /// </param>
        /// <param name='remoteTree'>
        /// Remote tree.
        /// </param>
        /// <param name='eventMap'>
        /// Event map.
        /// </param>
        public List<AbstractFolderEvent> CreateEvents(
            List<IMappedObject> storedObjects,
            IObjectTree<IFileableCmisObject> remoteTree,
            Dictionary<string, Tuple<AbstractFolderEvent, AbstractFolderEvent>> eventMap,
            ISet<IMappedObject> handledStoredObjects)
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

                createdEvents.AddRange(this.CreateEvents(storedObjects, child, eventMap, handledStoredObjects));
                if (storedMappedChild != null) {
                    handledStoredObjects.Add(storedMappedChild);
                }
            }

            return createdEvents;
        }

        private AbstractFolderEvent CreateRemoteEventBasedOnStorage(IFileableCmisObject cmisObject, IMappedObject storedParent, IMappedObject storedMappedChild)
        {
            AbstractFolderEvent newEvent = null;
            if (storedParent != null && storedMappedChild.ParentId == storedParent.RemoteObjectId) {
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
    }
}