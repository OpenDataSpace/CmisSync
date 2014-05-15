//-----------------------------------------------------------------------
// <copyright file="Watcher.cs" company="GRAU DATA AG">
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
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Diagnostics;
    using System.IO;
    using System.Text;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Storage;

    using DotCMIS;
    using DotCMIS.Client;

    using log4net;

    /// <summary>
    /// Watcher sync.
    /// </summary>
    public class Watcher : ReportingSyncEventHandler, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Watcher));

        private bool alreadyDisposed = false;

        private IFileSystemInfoFactory fsFactory = new FileSystemInfoFactory();

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Sync.Strategy.Watcher"/> class.
        /// </summary>
        /// <param name='queue'>
        /// Queue where the FSEvents and also the FileEvents and FolderEvents are reported.
        /// </param>
        public Watcher(ISyncEventQueue queue) : base(queue)
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="CmisSync.Lib.Sync.Strategy.Watcher"/> enables the FSEvent report
        /// </summary>
        /// <value>
        /// <c>true</c> if enable events; otherwise, <c>false</c>.
        /// </value>
        public virtual bool EnableEvents { get; set; }

        // This is a workaround to enable mocking Watcher with callbase
        // TODO: Remove this when Watcher is seperated

        /// <summary>
        /// Gets the priority.
        /// </summary>
        /// <value>
        /// The priority.
        /// </value>
        public override int Priority {
            get {
                return EventHandlerPriorities.GetPriority(typeof(CmisSync.Lib.Sync.Strategy.Watcher));
            }
        }

        /// <summary>
        /// Handle the specified Event if it is FSEvent.
        /// </summary>
        /// <param name='e'>
        /// The Event.
        /// </param>
        /// <returns>
        /// True if handled.
        /// </returns>
        public override bool Handle(ISyncEvent e)
        {
            var fsevent = e as IFSEvent;
            if (fsevent == null) {
                return false;
            }

            Logger.Debug("Handling FSEvent: " + e);

            if (fsevent.IsDirectory()) {
                this.HandleFolderEvents(fsevent);
            } else {
                this.HandleFileEvents(fsevent);
            }

            return true;
        }

        #region IDisposable implementation

        /// <summary>
        /// Releases all resource used by the <see cref="CmisSync.Lib.Sync.Strategy.Watcher"/> object.
        /// </summary>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="CmisSync.Lib.Sync.Strategy.Watcher"/>.
        /// The <see cref="Dispose"/> method leaves the <see cref="CmisSync.Lib.Sync.Strategy.Watcher"/> in an unusable
        /// state. After calling <see cref="Dispose"/>, you must release all references to the
        /// <see cref="CmisSync.Lib.Sync.Strategy.Watcher"/> so the garbage collector can reclaim the memory that the
        /// <see cref="CmisSync.Lib.Sync.Strategy.Watcher"/> was occupying.</remarks>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose the specified disposing.
        /// </summary>
        /// <param name="disposing">If set to <c>true</c> disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.alreadyDisposed)
            {
                // Indicate that the instance has been disposed.
                this.alreadyDisposed = true;
            }
        }

        #endregion

        private void HandleFolderEvents(IFSEvent e)
        {
            var movedEvent = e as FSMovedEvent;
            FolderEvent folderEvent;
            if (movedEvent != null) {
                folderEvent = new FolderMovedEvent(
                    this.fsFactory.CreateDirectoryInfo(movedEvent.OldPath),
                    this.fsFactory.CreateDirectoryInfo(movedEvent.Path),
                    null,
                    null)
                { Local = MetaDataChangeType.MOVED };
            } else {
                folderEvent = new FolderEvent(this.fsFactory.CreateDirectoryInfo(e.Path), null, this);
                switch (e.Type) {
                case WatcherChangeTypes.Created:
                    folderEvent.Local = MetaDataChangeType.CREATED;
                    break;
                case WatcherChangeTypes.Changed:
                    folderEvent.Local = MetaDataChangeType.CHANGED;
                    break;
                case WatcherChangeTypes.Deleted:
                    folderEvent.Local = MetaDataChangeType.DELETED;
                    break;
                default:
                    // This should never ever happen
                    return;
                }
            }

            Queue.AddEvent(folderEvent);
        }

        /// <summary>
        /// Handles the FSEvents of files and creates FileEvents.
        /// </summary>
        /// <param name='e'>
        /// The FSEvent.
        /// </param>
        private void HandleFileEvents(IFSEvent e)
        {
            var movedEvent = e as FSMovedEvent;
            if (movedEvent != null) {
                var oldfile = this.fsFactory.CreateFileInfo(movedEvent.OldPath);
                var newfile = this.fsFactory.CreateFileInfo(movedEvent.Path);
                var newEvent = new FileMovedEvent(
                    oldfile,
                    newfile,
                    null,
                    newfile.Directory,
                    null,
                    null);
                Queue.AddEvent(newEvent);
            } else {
                var file = this.fsFactory.CreateFileInfo(e.Path);
                var newEvent = new FileEvent(file, file.Directory, null);
                switch (e.Type) {
                case WatcherChangeTypes.Created:
                    newEvent.Local = MetaDataChangeType.CREATED;
                    newEvent.LocalContent = ContentChangeType.CREATED;
                    break;
                case WatcherChangeTypes.Changed:
                    newEvent.LocalContent = ContentChangeType.CHANGED;
                    break;
                case WatcherChangeTypes.Deleted:
                    newEvent.Local = MetaDataChangeType.DELETED;
                    newEvent.LocalContent = ContentChangeType.DELETED;
                    break;
                }

                Queue.AddEvent(newEvent);
            }
        }
    }
}
