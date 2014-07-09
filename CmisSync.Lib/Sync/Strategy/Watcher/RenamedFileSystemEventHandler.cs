//-----------------------------------------------------------------------
// <copyright file="RenamedFileSystemEventHandler.cs" company="GRAU DATA AG">
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
    /// Renamed file system event handler.
    /// </summary>
    public class RenamedFileSystemEventHandler
    {
        private ISyncEventQueue queue;
        private IFileSystemInfoFactory fsFactory;
        private string path;

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Sync.Strategy.RenamedFileSystemEventHandler"/> class.
        /// </summary>
        /// <param name="queue">Sync event queue to report the events to.</param>
        /// <param name="rootpath">Root path to the watched file system directory.</param>
        /// <param name="fsFactory">File system factory.</param>
        public RenamedFileSystemEventHandler(ISyncEventQueue queue, string rootpath, IFileSystemInfoFactory fsFactory = null)
        {
            if (queue == null) {
                throw new ArgumentNullException("Given queue is null");
            }

            if (rootpath == null) {
                throw new ArgumentNullException("Given root path is null");
            }

            this.queue = queue;
            this.path = rootpath;
            this.fsFactory = fsFactory ?? new FileSystemInfoFactory();
        }

        public void Handle(object source, RenamedEventArgs e) {
            string oldname = e.OldFullPath;
            string newname = e.FullPath;
            bool? isDirectory = this.fsFactory.IsDirectory(e.FullPath);

            if (isDirectory == null) {
                this.queue.AddEvent(new StartNextSyncEvent(true));
                return;
            }

            if (oldname.StartsWith(this.path) && newname.StartsWith(this.path)) {
                this.queue.AddEvent(new FSMovedEvent(oldname, newname, (bool)isDirectory));
            } else if (oldname.StartsWith(this.path)) {
                this.queue.AddEvent(new FSEvent(WatcherChangeTypes.Deleted, oldname, (bool)isDirectory));
            } else if (newname.StartsWith(this.path)) {
                this.queue.AddEvent(new FSEvent(WatcherChangeTypes.Created, newname, (bool)isDirectory));
            }
        }
    }
}