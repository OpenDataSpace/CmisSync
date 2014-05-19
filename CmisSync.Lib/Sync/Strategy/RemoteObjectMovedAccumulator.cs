//-----------------------------------------------------------------------
// <copyright file="RemoteObjectMovedAccumulator.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Sync.Strategy
{
    using System;

    using CmisSync.Lib.Data;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Storage;

    /// <summary>
    /// Remote object moved accumulator.
    /// Takes File/Folder Events and checks if the remote object has been moved.
    /// If the were moved, the privious local path will be added to the event.
    /// </summary>
    public class RemoteObjectMovedAccumulator : ReportingSyncEventHandler
    {
        private IFileSystemInfoFactory fsFactory;
        private IMetaDataStorage storage;

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Sync.Strategy.RemoteObjectMovedAccumulator"/> class.
        /// </summary>
        /// <param name="queue">Sync event queue.</param>
        /// <param name="storage">Meta data storage.</param>
        /// <param name="fsFactory">FileSystemInfo factory.</param>
        public RemoteObjectMovedAccumulator(ISyncEventQueue queue, IMetaDataStorage storage, IFileSystemInfoFactory fsFactory = null) : base(queue)
        {
            if (storage == null) {
                throw new ArgumentNullException("Given storage is null");
            }

            this.storage = storage;
            this.fsFactory = fsFactory == null ? new FileSystemInfoFactory() : fsFactory;
        }

        /// <summary>
        /// Handles File/FolderEvents.
        /// </summary>
        /// <param name="e">The event to handle.</param>
        /// <returns><c>false</c> on every event</returns>
        public override bool Handle(ISyncEvent e)
        {
            if(!CouldLocalObjectBeAccumulated(e as AbstractFolderEvent)) {
                return false;
            }

            var storedObject = this.GetStoredObject(e as AbstractFolderEvent);
            if(storedObject != null) {
                string parentId = this.GetParentId(e as AbstractFolderEvent);
                if (storedObject.ParentId != parentId) {
                    this.AccumulateEvent(e as AbstractFolderEvent, storedObject);
                }
            }

            return false;
        }

        private bool CouldLocalObjectBeAccumulated(AbstractFolderEvent e) {
            if (e == null) {
                return false;
            } else if(e.Remote == MetaDataChangeType.DELETED) {
                return false;
            } else if (e is FileEvent) {
                return (e as FileEvent).LocalFile == null;
            } else if (e is FolderEvent) {
                return (e as FolderEvent).LocalFolder == null;
            } else {
                return false;
            }
        }

        private IMappedObject GetStoredObject(AbstractFolderEvent e) {
            if (e is FolderEvent) {
                return this.storage.GetObjectByRemoteId((e as FolderEvent).RemoteFolder.Id);
            } else if(e is FileEvent) {
                return this.storage.GetObjectByRemoteId((e as FileEvent).RemoteFile.Id);
            }

            return null;
        }

        private string GetParentId(AbstractFolderEvent e) {
            if (e is FolderEvent) {
                return (e as FolderEvent).RemoteFolder.ParentId;
            } else if(e is FileEvent) {
                return (e as FileEvent).RemoteFile.Parents[0].Id;
            } else {
                throw new ArgumentException();
            }
        }

        void AccumulateEvent(AbstractFolderEvent abstractFolderEvent, IMappedObject storedObject)
        {
            if(abstractFolderEvent is FolderEvent) {
                (abstractFolderEvent as FolderEvent).LocalFolder = this.fsFactory.CreateDirectoryInfo(this.storage.GetLocalPath(storedObject));
            }

            if(abstractFolderEvent is FileEvent) {
                (abstractFolderEvent as FileEvent).LocalFile = this.fsFactory.CreateFileInfo(this.storage.GetLocalPath(storedObject));
            }
        }
    }
}