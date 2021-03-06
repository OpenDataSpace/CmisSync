//-----------------------------------------------------------------------
// <copyright file="IgnoreAlreadyHandledFsEventsFilter.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Filter {
    using System;
    using System.IO;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;

    /// <summary>
    /// Already added objects fs event filter.
    /// </summary>
    public class IgnoreAlreadyHandledFsEventsFilter : SyncEventHandler {
        private IMetaDataStorage storage;
        private IFileSystemInfoFactory fsFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Filter.IgnoreAlreadyHandledFsEventsFilter"/> class.
        /// </summary>
        /// <param name="storage">Storage instance.</param>
        /// <param name="fsFactory">Fs factory.</param>
        public IgnoreAlreadyHandledFsEventsFilter(IMetaDataStorage storage, IFileSystemInfoFactory fsFactory = null) {
            if (storage == null) {
                throw new ArgumentNullException("storage");
            }

            this.storage = storage;
            this.fsFactory = fsFactory ?? new FileSystemInfoFactory();
        }

        /// <summary>
        /// Filters FSEvents if they signalize an add and are handled already.
        /// This occurs if the syncer creates a local object on file system.
        /// </summary>
        /// <param name="e">Sync event</param>
        /// <returns><c>true</c> if the storage contains an entry for this object</returns>
        public override bool Handle(ISyncEvent e) {
            if (e is IFSEvent) {
                var fsEvent = e as IFSEvent;
                IFileSystemInfo path = fsEvent.IsDirectory ? (IFileSystemInfo)this.fsFactory.CreateDirectoryInfo(fsEvent.LocalPath) : (IFileSystemInfo)this.fsFactory.CreateFileInfo(fsEvent.LocalPath);
                switch (fsEvent.Type) {
                case WatcherChangeTypes.Created:
                    return this.storage.GetObjectByLocalPath(path) != null;
                case WatcherChangeTypes.Renamed:
                    var obj = this.storage.GetObjectByLocalPath(path);
                    if (obj != null) {
                        if (obj.Guid != Guid.Empty) {
                            try {
                                Guid? guid = path.Uuid;
                                return guid == null ? false : guid == obj.Guid;
                            } catch (FileNotFoundException) {
                                return true;
                            } catch (DirectoryNotFoundException) {
                                return true;
                            }
                        } else {
                            return false;
                        }
                    } else {
                        return false;
                    }

                case WatcherChangeTypes.Deleted:
                    IMappedObject o = this.storage.GetObjectByLocalPath(path);
                    if (o == null) {
                        return true;
                    } else if (path.Exists) {
                        try {
                            Guid? uuid = path.Uuid;
                            if (uuid != null) {
                                return (Guid)uuid == o.Guid;
                            }
                        } catch (IOException) {
                        }
                    }

                    return false;
                case WatcherChangeTypes.Changed:
                    try {
                        var guid = path.Uuid;
                        if (guid != null && guid.GetValueOrDefault() != Guid.Empty) {
                            IMappedObject mappedObject = this.storage.GetObjectByGuid(guid.GetValueOrDefault());
                            if (mappedObject.LastLocalWriteTimeUtc == path.LastWriteTimeUtc &&
                                path.Name == mappedObject.Name &&
                                path.ReadOnly == mappedObject.IsReadOnly) {
                                if (path is IFileInfo) {
                                    var fileInfo = path as IFileInfo;
                                    if (mappedObject.Type == MappedObjectType.File &&
                                        fileInfo.Length == mappedObject.LastContentSize) {
                                        var storedParent = this.storage.GetObjectByGuid((Guid)fileInfo.Directory.Uuid);
                                        if (storedParent.RemoteObjectId == mappedObject.ParentId) {
                                            return true;
                                        }
                                    }
                                } else if (path is IDirectoryInfo) {
                                    var dirInfo = path as IDirectoryInfo;
                                    if (mappedObject.Type == MappedObjectType.Folder) {
                                        var storedParent = this.storage.GetObjectByGuid((Guid)dirInfo.Parent.Uuid);
                                        if (storedParent.RemoteObjectId == mappedObject.ParentId) {
                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                    } catch (Exception) {
                    }

                    return false;
                default:
                    return false;
                }
            }

            return false;
        }
    }
}