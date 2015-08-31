//-----------------------------------------------------------------------
// <copyright file="NetWatcher.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Producer.Watcher {
    using System;
    using System.IO;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.FileSystem;

    /// <summary>
    /// .Net file system watcher.
    /// </summary>
    public class NetWatcher : IWatcherProducer {
        private readonly ISyncEventQueue queue;

        /// <summary>
        /// The .Net file system watcher instance.
        /// </summary>
        private FileSystemWatcher fileSystemWatcher;

        /// <summary>
        /// Whether this object has been disposed or not.
        /// </summary>
        private bool disposed;

        private IMetaDataStorage storage;

        private IFileSystemInfoFactory fsFactory;

        private CreatedChangedDeletedFileSystemEventHandler createChangeDeleteHandler;

        private RenamedFileSystemEventHandler renamedHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="NetWatcher"/> class.
        /// Takes the given file system watcher and listens for events and passes them to the given queue
        /// </summary>
        /// <param name="watcher">File System Watcher.</param>
        /// <param name="queue">Queue for the occured events.</param>
        /// <param name="storage">Meta Data Storage to verify, if a deleted object is a file or folder.</param>
        /// <param name="fsFactory">File system info factory. If factory is null, the normal file system is used, otherwise the given factory.</param>
        public NetWatcher(
            FileSystemWatcher watcher,
            ISyncEventQueue queue,
            IMetaDataStorage storage,
            FileSystemInfoFactory fsFactory = null)
        {
            if (watcher == null) {
                throw new ArgumentNullException("watcher");
            }

            if (string.IsNullOrEmpty(watcher.Path)) {
                throw new ArgumentException("The given watcher must contain a path, where it is listening");
            }

            if (queue == null) {
                throw new ArgumentNullException("queue");
            }

            if (storage == null) {
                throw new ArgumentNullException("storage");
            }

            this.fsFactory = fsFactory ?? new FileSystemInfoFactory();

            this.queue = queue;
            this.storage = storage;

            this.fileSystemWatcher = watcher;
            this.fileSystemWatcher.IncludeSubdirectories = true;
            this.fileSystemWatcher.Filter = "*";
            this.fileSystemWatcher.InternalBufferSize = 4 * 1024 * 16;
            this.fileSystemWatcher.NotifyFilter = NotifyFilters.Size | NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite | NotifyFilters.Security;

            this.createChangeDeleteHandler = new CreatedChangedDeletedFileSystemEventHandler(this.queue, this.storage, this.fsFactory);
            this.renamedHandler = new RenamedFileSystemEventHandler(this.queue, this.fsFactory);

            this.fileSystemWatcher.Created += new FileSystemEventHandler(this.createChangeDeleteHandler.Handle);
            this.fileSystemWatcher.Deleted += new FileSystemEventHandler(this.createChangeDeleteHandler.Handle);
            this.fileSystemWatcher.Changed += new FileSystemEventHandler(this.createChangeDeleteHandler.Handle);
            this.fileSystemWatcher.Renamed += new RenamedEventHandler(this.renamedHandler.Handle);
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="CmisSync.Lib.Sync.Strategy.NetWatcher"/> enable events.
        /// </summary>
        /// <value><c>true</c> if enable events; otherwise, <c>false</c>.</value>
        public bool EnableEvents {
            get { return this.fileSystemWatcher.EnableRaisingEvents; }
            set { this.fileSystemWatcher.EnableRaisingEvents = value; }
        }

        /// <summary>
        /// Releases all resource used by the <see cref="CmisSync.Lib.Sync.Strategy.WatcherConsumer"/> object.
        /// </summary>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="CmisSync.Lib.Sync.Strategy.Watcher"/>.
        /// The <see cref="Dispose"/> method leaves the <see cref="CmisSync.Lib.Sync.Strategy.Watcher"/> in an unusable
        /// state. After calling <see cref="Dispose"/>, you must release all references to the
        /// <see cref="CmisSync.Lib.Sync.Strategy.WatcherConsumer"/> so the garbage collector can reclaim the memory that the
        /// <see cref="CmisSync.Lib.Sync.Strategy.WatcherConsumer"/> was occupying.</remarks>
        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose the .Net File System Watcher.
        /// </summary>
        /// <param name="disposing">If set to <c>true</c> disposing.</param>
        protected void Dispose(bool disposing) {
            if (!this.disposed) {
                if (disposing) {
                    this.EnableEvents = false;
                    this.fileSystemWatcher.Dispose();
                    this.createChangeDeleteHandler.Dispose();
                }

                this.disposed = true;
            }
        }
    }
}