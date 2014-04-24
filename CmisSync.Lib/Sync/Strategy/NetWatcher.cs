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
using System;
using System.IO;

using CmisSync.Lib.Events;

namespace CmisSync.Lib.Sync.Strategy
{
    public class NetWatcher : Watcher
    {
        public override bool EnableEvents { get { return FsWatcher.EnableRaisingEvents; } set { FsWatcher.EnableRaisingEvents = value; } }

        private FileSystemWatcher FsWatcher;

        public NetWatcher (FileSystemWatcher watcher, ISyncEventQueue queue) : base(queue)
        {
            if (watcher == null)
                throw new ArgumentNullException ("The given fs watcher must not be null");
            if (String.IsNullOrEmpty (watcher.Path))
                throw new ArgumentException ("The given watcher must contain a path, where it is listening");
            FsWatcher = watcher;
            FsWatcher.IncludeSubdirectories = true;
            FsWatcher.Filter = "*";
            FsWatcher.InternalBufferSize = 4 * 1024 * 16;
            FsWatcher.NotifyFilter = NotifyFilters.Size | NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite | NotifyFilters.Security;
            //FsWatcher.Error += new ErrorEventHandler (OnError);
            FsWatcher.Created += new FileSystemEventHandler (OnCreatedChangedDeleted);
            FsWatcher.Deleted += new FileSystemEventHandler (OnCreatedChangedDeleted);
            FsWatcher.Changed += new FileSystemEventHandler (OnCreatedChangedDeleted);
            FsWatcher.Renamed += new RenamedEventHandler (OnRenamed);
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
        private void OnCreatedChangedDeleted (object source, FileSystemEventArgs e)
        {
            Queue.AddEvent (new FSEvent (e.ChangeType, e.FullPath));
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
        private void OnRenamed (object source, RenamedEventArgs e)
        {
            string oldname = e.OldFullPath;
            string newname = e.FullPath;
            if (oldname.StartsWith (FsWatcher.Path) && newname.StartsWith (FsWatcher.Path)) {
                Queue.AddEvent (new FSMovedEvent (oldname, newname));
            } else if (oldname.StartsWith (FsWatcher.Path)) {
                Queue.AddEvent (new FSEvent (WatcherChangeTypes.Deleted, oldname));
            } else if (newname.StartsWith (FsWatcher.Path)) {
                Queue.AddEvent (new FSEvent (WatcherChangeTypes.Created, newname));
            }
        }

        /// <summary>
        /// Whether this object has been disposed or not.
        /// </summary>
        private bool disposed = false;


        /// <summary>
        /// Dispose of the watcher.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    this.EnableEvents = false;
                    this.FsWatcher.Dispose();
                }
                disposed = true;
            }
            base.Dispose(disposing);
        }
    }
}

