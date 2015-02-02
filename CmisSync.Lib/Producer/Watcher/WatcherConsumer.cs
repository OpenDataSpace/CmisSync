//-----------------------------------------------------------------------
// <copyright file="WatcherConsumer.cs" company="GRAU DATA AG">
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
    using System.Diagnostics;
    using System.IO;
    using System.Text;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS;
    using DotCMIS.Client;

    using log4net;

    /// <summary>
    /// Watcher sync.
    /// </summary>
    public class WatcherConsumer : ReportingSyncEventHandler
    {
        private IFileSystemInfoFactory fsFactory = new FileSystemInfoFactory();

        /// <summary>
        /// Initializes a new instance of the <see cref="WatcherConsumer"/> class.
        /// </summary>
        /// <param name='queue'>
        /// Queue where the FSEvents and also the FileEvents and FolderEvents are reported.
        /// </param>
        public WatcherConsumer(ISyncEventQueue queue) : base(queue)
        {
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

            if (fsevent.IsDirectory) {
                this.HandleFolderEvents(fsevent);
            } else {
                this.HandleFileEvents(fsevent);
            }

            return true;
        }

        private void HandleFolderEvents(IFSEvent e)
        {
            var movedEvent = e as IFSMovedEvent;
            FolderEvent folderEvent;
            if (movedEvent != null) {
                folderEvent = new FolderMovedEvent(
                    this.fsFactory.CreateDirectoryInfo(movedEvent.OldPath),
                    this.fsFactory.CreateDirectoryInfo(movedEvent.LocalPath),
                    null,
                    null,
                    this)
                { Local = MetaDataChangeType.MOVED };
            } else {
                folderEvent = new FolderEvent(this.fsFactory.CreateDirectoryInfo(e.LocalPath), null, this);
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
            var movedEvent = e as IFSMovedEvent;
            if (movedEvent != null) {
                var oldfile = this.fsFactory.CreateFileInfo(movedEvent.OldPath);
                var newfile = this.fsFactory.CreateFileInfo(movedEvent.LocalPath);
                var newEvent = new FileMovedEvent(
                    oldfile,
                    newfile,
                    null,
                    null);
                Queue.AddEvent(newEvent);
            } else {
                var file = this.fsFactory.CreateFileInfo(e.LocalPath);
                var newEvent = new FileEvent(file, null);
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
