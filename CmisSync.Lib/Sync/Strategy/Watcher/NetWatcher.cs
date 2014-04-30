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

namespace CmisSync.Lib.Sync.Strategy
{
    using System;
    using System.IO;

    using CmisSync.Lib.Events;

    /// <summary>
    /// .Net file system watcher.
    /// </summary>
    public class NetWatcher : Watcher
    {
        /// <summary>
        /// The .Net file system watcher instance.
        /// </summary>
        private FileSystemWatcher fileSystemWatcher;

        /// <summary>
        /// Whether this object has been disposed or not.
        /// </summary>
        private bool disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Sync.Strategy.NetWatcher"/> class.
        /// Takes the given file system watcher and listens for events and passes them to the given queue
        /// </summary>
        /// <param name="watcher">File System Watcher.</param>
        /// <param name="queue">Queue for the occured events.</param>
        public NetWatcher(FileSystemWatcher watcher, ISyncEventQueue queue) : base(queue)
        {
            if (watcher == null) {
                throw new ArgumentNullException("The given fs watcher must not be null");
            }

            if (string.IsNullOrEmpty(watcher.Path)) {
                throw new ArgumentException("The given watcher must contain a path, where it is listening");
            }

            this.fileSystemWatcher = watcher;
            this.fileSystemWatcher.IncludeSubdirectories = true;
            this.fileSystemWatcher.Filter = "*";
            this.fileSystemWatcher.InternalBufferSize = 4 * 1024 * 16;
            this.fileSystemWatcher.NotifyFilter = NotifyFilters.Size | NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite | NotifyFilters.Security;
            this.fileSystemWatcher.Created += new FileSystemEventHandler(this.OnCreatedChangedDeleted);
            this.fileSystemWatcher.Deleted += new FileSystemEventHandler(this.OnCreatedChangedDeleted);
            this.fileSystemWatcher.Changed += new FileSystemEventHandler(this.OnCreatedChangedDeleted);
            this.fileSystemWatcher.Renamed += new RenamedEventHandler(this.OnRenamed);
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="CmisSync.Lib.Sync.Strategy.NetWatcher"/> enable events.
        /// </summary>
        /// <value><c>true</c> if enable events; otherwise, <c>false</c>.</value>
        public override bool EnableEvents {
            get { return this.fileSystemWatcher.EnableRaisingEvents; }
            set { this.fileSystemWatcher.EnableRaisingEvents = value; }
        }

        /// <summary>
        /// Dispose the .Net File System Watcher.
        /// </summary>
        /// <param name="disposing">If set to <c>true</c> disposing.</param>
        protected override void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    this.EnableEvents = false;
                    this.fileSystemWatcher.Dispose();
                }

                this.disposed = true;
            }

            base.Dispose(disposing);
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
        private void OnCreatedChangedDeleted(object source, FileSystemEventArgs e)
        {
            Queue.AddEvent(new FSEvent(e.ChangeType, e.FullPath));
        }

        /// <summary>
        /// Raises the renamed event as FSMovedEvent.
        /// </summary>
        /// <param name='source'>
        /// Source file system watcher.
        /// </param>
        /// <param name='e'>
        /// Reported renaming.
        /// </param>
        private void OnRenamed(object source, RenamedEventArgs e)
        {
            string oldname = e.OldFullPath;
            string newname = e.FullPath;
            if (oldname.StartsWith(this.fileSystemWatcher.Path) && newname.StartsWith(this.fileSystemWatcher.Path))
            {
                Queue.AddEvent(new FSMovedEvent(oldname, newname));
            } else if (oldname.StartsWith(this.fileSystemWatcher.Path)) {
                Queue.AddEvent(new FSEvent(WatcherChangeTypes.Deleted, oldname));
            } else if (newname.StartsWith(this.fileSystemWatcher.Path)) {
                Queue.AddEvent(new FSEvent(WatcherChangeTypes.Created, newname));
            }
        }
    }
}