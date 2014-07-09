//-----------------------------------------------------------------------
// <copyright file="CreatedChangedDeletedFileSystemEventHandler.cs" company="GRAU DATA AG">
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
    using System.IO;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Storage;

    /// <summary>
    /// Created/Changed/Deleted file system events handler to report the events to the given queue.
    /// </summary>
    public class CreatedChangedDeletedFileSystemEventHandler
    {
        private ISyncEventQueue queue;
        private IMetaDataStorage storage;
        private IFileSystemInfoFactory fsFactory;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="CmisSync.Lib.Sync.Strategy.CreatedChangedDeletedFileSystemEventHandler"/> class.
        /// </summary>
        /// <param name="queue">Sync event queue.</param>
        /// <param name="storage">Meta data storage.</param>
        /// <param name="fsFactory">File system info factory.</param>
        public CreatedChangedDeletedFileSystemEventHandler(ISyncEventQueue queue, IMetaDataStorage storage, IFileSystemInfoFactory fsFactory = null)
        {
            if (queue == null) {
                throw new ArgumentNullException("Given queue is null");
            }

            if (storage == null) {
                throw new ArgumentNullException("Given storage is null");
            }

            this.queue = queue;
            this.storage = storage;
            this.fsFactory = fsFactory ?? new FileSystemInfoFactory();
        }

        /// <summary>
        /// Raises the created/changed/deleted event as FSEvent.
        /// </summary>
        /// <param name='source'>
        /// Source file system watcher.
        /// </param>
        /// <param name='e'>
        /// Reported changes.
        /// </param>
        public void Handle(object source, FileSystemEventArgs e) {
            bool isDirectory;
            if (e.ChangeType == WatcherChangeTypes.Deleted) {
                var obj = this.storage.GetObjectByLocalPath(this.fsFactory.CreateFileInfo(e.FullPath));
                if (obj != null) {
                    isDirectory = obj.Type == CmisSync.Lib.Data.MappedObjectType.Folder;
                } else {
                    // we can not know if it was a directory or not but is not relevant => skip this event
                    return;
                }
            } else {
                bool? check = this.fsFactory.IsDirectory(e.FullPath);
                if (check != null) {
                    isDirectory = (bool)check;
                } else {
                    return;
                }
            }

            this.queue.AddEvent(new FSEvent(e.ChangeType, e.FullPath, isDirectory));
        }
    }
}