//-----------------------------------------------------------------------
// <copyright file="AlreadyAddedObjectsFsEventFilter.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Events.Filter
{
    using System;
    using System.IO;

    using CmisSync.Lib.Storage;

    public class AlreadyAddedObjectsFsEventFilter : SyncEventHandler
    {
        private IMetaDataStorage storage;
        private IFileSystemInfoFactory fsFactory;

        public AlreadyAddedObjectsFsEventFilter(IMetaDataStorage storage, IFileSystemInfoFactory fsFactory = null)
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
                return EventHandlerPriorities.GetPriority(typeof(AlreadyAddedObjectsFsEventFilter));
            }
        }

        public override bool Handle(ISyncEvent e)
        {
            if(e is FSEvent) {
                var fsEvent = e as FSEvent;
                IFileSystemInfo path = fsEvent.IsDirectory() ? (IFileSystemInfo)this.fsFactory.CreateDirectoryInfo(fsEvent.Path) : (IFileSystemInfo)this.fsFactory.CreateFileInfo(fsEvent.Path);
                if (fsEvent.Type == WatcherChangeTypes.Created && this.storage.GetObjectByLocalPath(path) != null) {
                    return true;
                }
                Console.WriteLine(storage.ToString());
            }

            return false;
        }
    }
}