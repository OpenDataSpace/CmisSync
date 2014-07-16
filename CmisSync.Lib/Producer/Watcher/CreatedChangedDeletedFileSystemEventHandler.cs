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

namespace CmisSync.Lib.Producer.Watcher
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Timers;

    using CmisSync.Lib.Data;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage;
    using CmisSync.Lib.Storage.Database;

    /// <summary>
    /// Created/Changed/Deleted file system events handler to report the events to the given queue.
    /// </summary>
    public class CreatedChangedDeletedFileSystemEventHandler : IDisposable
    {
        private ISyncEventQueue queue;
        private IMetaDataStorage storage;
        private IFileSystemInfoFactory fsFactory;
        private long threshold;
        private Timer timer;
        private object listLock = new object();
        private List<Tuple<FileSystemEventArgs, Guid, DateTime, bool>> deletions;
        private bool disposed = false;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="CmisSync.Lib.Sync.Strategy.CreatedChangedDeletedFileSystemEventHandler"/> class.
        /// </summary>
        /// <param name="queue">Sync event queue.</param>
        /// <param name="storage">Meta data storage.</param>
        /// <param name="fsFactory">File system info factory.</param>
        /// <param name="threshold">Delay after which a deleted event is passed to the queue.</param>
        public CreatedChangedDeletedFileSystemEventHandler(
            ISyncEventQueue queue,
            IMetaDataStorage storage,
            IFileSystemInfoFactory fsFactory = null,
            long threshold = 100)
        {
            if (queue == null) {
                throw new ArgumentNullException("Given queue is null");
            }

            if (storage == null) {
                throw new ArgumentNullException("Given storage is null");
            }

            this.queue = queue;
            this.storage = storage;
            this.threshold = threshold;
            this.fsFactory = fsFactory ?? new FileSystemInfoFactory();
            this.deletions = new List<Tuple<FileSystemEventArgs, Guid, DateTime, bool>>();
            this.timer = new Timer();
            this.timer.AutoReset = false;
            this.timer.Elapsed += (sender, e) => this.PopEventsFromList();
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
        public virtual void Handle(object source, FileSystemEventArgs e) {
            bool isDirectory;
            if (e.ChangeType == WatcherChangeTypes.Deleted) {
                var obj = this.storage.GetObjectByLocalPath(this.fsFactory.CreateFileInfo(e.FullPath));
                if (obj != null) {
                    isDirectory = obj.Type == CmisSync.Lib.Data.MappedObjectType.Folder;
                    if (obj.Guid != Guid.Empty) {
                        this.AddEventToList(e, obj.Guid, isDirectory);
                        return;
                    }
                } else {
                    // we can not know if it was a directory or not but is not relevant => skip this event
                    return;
                }
            } else {
                bool? check = this.fsFactory.IsDirectory(e.FullPath);
                if (check != null) {
                    isDirectory = (bool)check;
                    if (e.ChangeType == WatcherChangeTypes.Created) {
                        if (this.MergingAddedAndDeletedEvent(e, isDirectory)) {
                            return;
                        }
                    }
                } else {
                    return;
                }
            }

            this.queue.AddEvent(new FSEvent(e.ChangeType, e.FullPath, isDirectory));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if(!this.disposed) {
                if(disposing) {
                    this.timer.Dispose();
                }

                disposed = true;
            }
        }

        private void AddEventToList(FileSystemEventArgs args, Guid guid, bool isDirectory) {
            lock (this.listLock) {
                this.timer.Stop();
                this.deletions.Add(new Tuple<FileSystemEventArgs, Guid, DateTime, bool>(args, guid, DateTime.UtcNow, isDirectory));
                this.timer.Interval = this.threshold - (DateTime.UtcNow - this.deletions[0].Item3).Milliseconds;
                this.timer.Start();
            }
        }

        private void PopEventsFromList() {
            lock (this.listLock) {
                this.timer.Stop();
                if (this.deletions.Count == 0) {
                    return;
                }

                while (this.deletions.Count > 0 && (DateTime.UtcNow - this.deletions[0].Item3).Milliseconds >= this.threshold) {
                    var entry = this.deletions[0];
                    this.queue.AddEvent(new FSEvent(entry.Item1.ChangeType, entry.Item1.FullPath, entry.Item4));
                    this.deletions.RemoveAt(0);
                }

                if (this.deletions.Count > 0) {
                    this.timer.Interval = this.threshold - (DateTime.UtcNow - this.deletions[0].Item3).Milliseconds;
                    this.timer.Start();
                }
            }
        }

        private bool MergingAddedAndDeletedEvent(FileSystemEventArgs args, bool isDirectory) {
            lock (this.listLock) {
                Guid fsGuid;
                IFileSystemInfo fsInfo = isDirectory ? (IFileSystemInfo)this.fsFactory.CreateDirectoryInfo(args.FullPath) : (IFileSystemInfo)this.fsFactory.CreateFileInfo(args.FullPath);
                try {
                    string fsUuid = fsInfo.GetExtendedAttribute(MappedObject.ExtendedAttributeKey);
                    if (Guid.TryParse(fsUuid, out fsGuid)) {
                        var correspondingDeletion = this.deletions.Find((Tuple<FileSystemEventArgs, Guid, DateTime, bool> obj) => obj.Item2 == fsGuid);
                        if (correspondingDeletion != null) {
                            this.queue.AddEvent(new FSMovedEvent(correspondingDeletion.Item1.FullPath, args.FullPath, isDirectory));
                            this.deletions.Remove(correspondingDeletion);
                            return true;
                        }
                    }

                    return false;
                } catch(ExtendedAttributeException) {
                    return false;
                }
            }
        }
    }
}