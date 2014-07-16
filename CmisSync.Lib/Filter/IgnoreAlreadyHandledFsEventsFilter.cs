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

namespace CmisSync.Lib.Filter
{
    using System;
    using System.IO;

    using CmisSync.Lib.Data;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Storage;

    /// <summary>
    /// Already added objects fs event filter.
    /// </summary>
    public class IgnoreAlreadyHandledFsEventsFilter : SyncEventHandler
    {
        private IMetaDataStorage storage;
        private IFileSystemInfoFactory fsFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Filter.IgnoreAlreadyHandledFsEventsFilter"/> class.
        /// </summary>
        /// <param name="storage">Storage instance.</param>
        /// <param name="fsFactory">Fs factory.</param>
        public IgnoreAlreadyHandledFsEventsFilter(IMetaDataStorage storage, IFileSystemInfoFactory fsFactory = null)
        {
            if (storage == null) {
                throw new ArgumentNullException("Given storage is null");
            }

            this.storage = storage;
            this.fsFactory = fsFactory ?? new FileSystemInfoFactory();
        }

        /// <summary>
        /// Filter priority
        /// </summary>
        /// <value>The default filter priority.</value>
        public override int Priority {
            get {
                return EventHandlerPriorities.GetPriority(typeof(IgnoreAlreadyHandledFsEventsFilter));
            }
        }

        /// <summary>
        /// Filters FSEvents if they signalize an add and are handled already.
        /// This occurs if the syncer creates a local object on file system.
        /// </summary>
        /// <param name="e">Sync event</param>
        /// <returns><c>true</c> if the storage contains an entry for this object</returns>
        public override bool Handle(ISyncEvent e)
        {
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
                            string guid = path.GetExtendedAttribute(MappedObject.ExtendedAttributeKey);
                            Guid fsGuid;
                            if (Guid.TryParse(guid, out fsGuid)) {
                                return fsGuid == obj.Guid;
                            } else {
                                return false;
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
                    } else if(path.Exists) {
                        try {
                            Guid uuid;
                            string ea = path.GetExtendedAttribute(MappedObject.ExtendedAttributeKey);
                            if (ea != null && Guid.TryParse(ea, out uuid)) {
                                return uuid == o.Guid;
                            }
                        } catch (IOException) {
                        }
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