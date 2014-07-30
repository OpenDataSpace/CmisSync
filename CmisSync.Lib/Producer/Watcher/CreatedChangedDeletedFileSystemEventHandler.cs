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

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;

    /// <summary>
    /// Created/Changed/Deleted file system events handler to report the events to the given queue.
    /// </summary>
    public class CreatedChangedDeletedFileSystemEventHandler : IDisposable
    {
        private ISyncEventQueue queue;
        private IMetaDataStorage storage;
        private IFileSystemInfoFactory fsFactory;
        private long threshold;
        /// <summary>
        /// The timer. Exposed to allow testability
        /// </summary>
        protected Timer timer;
        private object listLock = new object();
        private List<Tuple<FileSystemEventArgs, Guid, DateTime, bool>> events;
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
            this.events = new List<Tuple<FileSystemEventArgs, Guid, DateTime, bool>>();
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
            bool isDirectory = false;
            if (e.ChangeType == WatcherChangeTypes.Deleted) {
                var obj = this.storage.GetObjectByLocalPath(this.fsFactory.CreateFileInfo(e.FullPath));
                Guid guid = Guid.Empty;
                if (obj != null) {
                    isDirectory = obj.Type == MappedObjectType.Folder;
                    guid = obj.Guid;
                }

                this.AddEventToList(e, guid, isDirectory);
            } else {
                bool? check = this.fsFactory.IsDirectory(e.FullPath);
                if (check != null) {
                    isDirectory = (bool)check;
                    Guid fsGuid = Guid.Empty;
                    IFileSystemInfo fsInfo = isDirectory ? (IFileSystemInfo)this.fsFactory.CreateDirectoryInfo(e.FullPath) : (IFileSystemInfo)this.fsFactory.CreateFileInfo(e.FullPath);
                    string ea = fsInfo.GetExtendedAttribute(MappedObject.ExtendedAttributeKey);
                    if (ea != null) {
                        Guid.TryParse(ea, out fsGuid);
                    }

                    this.AddEventToList(e, fsGuid, isDirectory);
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.disposed) {
                if (disposing) {
                    this.timer.Dispose();
                }

                this.disposed = true;
            }
        }

        private void ResetTimerInterval() {
            double interval = this.threshold - (DateTime.UtcNow - this.events[0].Item3).Milliseconds;
            if (interval < 1) {
                interval = 1;
            }

            this.timer.Interval = interval;
        }

        private void AddEventToList(FileSystemEventArgs args, Guid guid, bool isDirectory) {
            lock (this.listLock) {
                this.timer.Stop();
                this.events.Add(new Tuple<FileSystemEventArgs, Guid, DateTime, bool>(args, guid, DateTime.UtcNow, isDirectory));
                this.ResetTimerInterval();
                this.timer.Start();
            }
        }

        private void PopEventsFromList() {
            lock (this.listLock) {
                this.timer.Stop();
                if (this.events.Count == 0) {
                    return;
                }

                while (this.events.Count > 0 && (DateTime.UtcNow - this.events[0].Item3).Milliseconds >= this.threshold) {
                    var entry = this.events[0];
                    this.events.RemoveAt(0);
                    if (entry.Item1.ChangeType == WatcherChangeTypes.Created) {
                        Guid fsGuid;
                        IFileSystemInfo fsInfo = entry.Item4 ? (IFileSystemInfo)this.fsFactory.CreateDirectoryInfo(entry.Item1.FullPath) : (IFileSystemInfo)this.fsFactory.CreateFileInfo(entry.Item1.FullPath);
                        try {
                            string fsUuid = fsInfo.GetExtendedAttribute(MappedObject.ExtendedAttributeKey);
                            if (Guid.TryParse(fsUuid, out fsGuid) && fsGuid != Guid.Empty) {
                                var correspondingDeletion = this.events.Find((Tuple<FileSystemEventArgs, Guid, DateTime, bool> obj) => obj.Item2 == fsGuid);
                                if (correspondingDeletion != null) {
                                    this.queue.AddEvent(new FSMovedEvent(correspondingDeletion.Item1.FullPath, entry.Item1.FullPath, entry.Item4));
                                    this.events.Remove(correspondingDeletion);
                                    continue;
                                }
                            }
                        } catch (ExtendedAttributeException) {
                        }
                    } else if (entry.Item1.ChangeType == WatcherChangeTypes.Deleted && entry.Item2 != Guid.Empty) {
                        var correspondingCreation = this.events.Find((Tuple<FileSystemEventArgs, Guid, DateTime, bool> obj) => obj.Item2 == entry.Item2 && obj.Item1.ChangeType == WatcherChangeTypes.Created);
                        if (correspondingCreation != null) {
                            this.queue.AddEvent(new FSMovedEvent(entry.Item1.FullPath, correspondingCreation.Item1.FullPath, entry.Item4));
                            this.events.Remove(correspondingCreation);
                            continue;
                        }
                    }

                    this.queue.AddEvent(new FSEvent(entry.Item1.ChangeType, entry.Item1.FullPath, entry.Item4));
                }

                if (this.events.Count > 0) {
                    this.ResetTimerInterval();
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
                        var correspondingDeletion = this.events.Find((Tuple<FileSystemEventArgs, Guid, DateTime, bool> obj) => obj.Item2 == fsGuid);
                        if (correspondingDeletion != null) {
                            this.queue.AddEvent(new FSMovedEvent(correspondingDeletion.Item1.FullPath, args.FullPath, isDirectory));
                            this.events.Remove(correspondingDeletion);
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