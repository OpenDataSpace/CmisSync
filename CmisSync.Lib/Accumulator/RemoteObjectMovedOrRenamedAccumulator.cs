//-----------------------------------------------------------------------
// <copyright file="RemoteObjectMovedOrRenamedAccumulator.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Accumulator {
    using System;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;

    using log4net;

    /// <summary>
    /// Remote object moved or renamed accumulator.
    /// Takes File/Folder Events and checks if the remote object has been moved or renamed.
    /// If the file/folder was moved, the privious local path will be added to the event.
    /// </summary>
    public class RemoteObjectMovedOrRenamedAccumulator : ReportingSyncEventHandler {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(RemoteObjectMovedOrRenamedAccumulator));

        private IFileSystemInfoFactory fsFactory;
        private IMetaDataStorage storage;

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteObjectMovedOrRenamedAccumulator"/> class.
        /// </summary>
        /// <param name="queue">Sync event queue.</param>
        /// <param name="storage">Meta data storage.</param>
        /// <param name="fsFactory">FileSystemInfo factory.</param>
        public RemoteObjectMovedOrRenamedAccumulator(ISyncEventQueue queue, IMetaDataStorage storage, IFileSystemInfoFactory fsFactory = null) : base(queue) {
            if (storage == null) {
                throw new ArgumentNullException("storage");
            }

            this.storage = storage;
            this.fsFactory = fsFactory == null ? new FileSystemInfoFactory() : fsFactory;
        }

        /// <summary>
        /// Handles File/FolderEvents.
        /// </summary>
        /// <param name="e">The event to handle.</param>
        /// <returns><c>false</c> on every event</returns>
        public override bool Handle(ISyncEvent e) {
            var ev = e as AbstractFolderEvent;
            if (!this.CouldLocalObjectBeAccumulated(ev)) {
                return false;
            }

            Logger.Debug("Handling event: " + ev);

            var storedObject = this.GetStoredObject(ev);
            Logger.Debug(storedObject);

            Logger.Debug(this.GetParentId(ev));
            if (storedObject != null) {
                if (storedObject.ParentId != this.GetParentId(ev)) {
                    this.AccumulateEvent(ev, storedObject);
                } else if(storedObject.Name != this.GetRemoteObjectName(ev)) {
                    this.AccumulateEvent(ev, storedObject);
                }
            }

            return false;
        }

        private bool CouldLocalObjectBeAccumulated(AbstractFolderEvent e) {
            if (e == null) {
                return false;
            } else if (e.Remote == MetaDataChangeType.DELETED) {
                return false;
            } else {
                var fileEvent = e as FileEvent;
                if (fileEvent != null) {
                    return fileEvent.LocalFile == null;
                } else {
                    var folderEvent = e as FolderEvent;
                    if (folderEvent != null) {
                        return folderEvent.LocalFolder == null;
                    } else {
                        return false;
                    }
                }
            }
        }

        private IMappedObject GetStoredObject(AbstractFolderEvent e) {
            var folderEvent = e as FolderEvent;
            if (folderEvent != null) {
                return this.storage.GetObjectByRemoteId(folderEvent.RemoteFolder.Id);
            } else {
                var fileEvent = e as FileEvent;
                if (fileEvent != null) {
                    return this.storage.GetObjectByRemoteId(fileEvent.RemoteFile.Id);
                }
            }

            return null;
        }

        private string GetParentId(AbstractFolderEvent e) {
            var folderEvent = e as FolderEvent;
            if (folderEvent != null) {
                return folderEvent.RemoteFolder.ParentId;
            } else {
                var fileEvent = e as FileEvent;
                if (fileEvent != null) {
                    return fileEvent.RemoteFile.Parents[0].Id;
                } else {
                    throw new ArgumentException("e");
                }
            }
        }

        private string GetRemoteObjectName(AbstractFolderEvent e) {
            var folderEvent = e as FolderEvent;
            if (folderEvent != null) {
                return folderEvent.RemoteFolder.Name;
            } else {
                var fileEvent = e as FileEvent;
                if (fileEvent != null) {
                    return fileEvent.RemoteFile.Name;
                } else {
                    throw new ArgumentException("given event e is not a FileEvent or a FolderEvent");
                }
            }
        }

        private void AccumulateEvent(AbstractFolderEvent abstractFolderEvent, IMappedObject storedObject) {
            Logger.Debug("Accumulating: " + this.storage.GetLocalPath(storedObject));
            var folderEvent = abstractFolderEvent as FolderEvent;
            if (folderEvent != null) {
                folderEvent.LocalFolder = this.fsFactory.CreateDirectoryInfo(this.storage.GetLocalPath(storedObject));
            } else {
                var fileEvent = abstractFolderEvent as FileEvent;
                if (fileEvent != null) {
                    fileEvent.LocalFile = this.fsFactory.CreateFileInfo(this.storage.GetLocalPath(storedObject));
                }
            }
        }
    }
}