using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Data.Common;

using DotCMIS;
using DotCMIS.Client;

using CmisSync.Lib.Events;
using CmisSync.Lib.Storage;

using log4net;

namespace CmisSync.Lib.Sync.Strategy
{
    /// <summary>
    /// Watcher sync.
    /// </summary>
    public class Watcher : ReportingSyncEventHandler, IDisposable
    {

        // This is a workaround to enable mocking Watcher with callbase
        // TODO: Remove this when Watcher is seperated
        public override int Priority {
            get {
                return EventHandlerPriorities.GetPriority(typeof(CmisSync.Lib.Sync.Strategy.Watcher));
            }
        }
        /// <summary>
        /// The disposed.
        /// </summary>
        private bool _disposed;

        private IFileSystemInfoFactory fsFactory = new FileSystemInfoFactory();
        /// <summary>
        /// Enables the FSEvent report
        /// </summary>
        public virtual bool EnableEvents { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Sync.Strategy.Watcher"/> class.
        /// </summary>
        /// <param name='queue'>
        /// Queue where the FSEvents and also the FileEvents and FolderEvents are reported.
        /// </param>
        public Watcher (ISyncEventQueue queue) : base(queue)
        {
            _disposed = false;
        }

        /// <summary>
        /// Handles FSEvents.
        /// </summary>
        /// <param name='e'>
        /// All not yet filtered events. Only FSEvents are handled.
        /// </param>
        public override bool Handle (ISyncEvent e)
        {
            var fsevent = e as FSEvent;
            if (fsevent == null)
                return false;

            if (fsevent.IsDirectory ()) {
                handleFolderEvents (fsevent);
            } else {
                handleFileEvents (fsevent);
            }
            return true;
        }

        /// <summary>
        /// Handles the FSEvent of folder and creates FolderEvents.
        /// </summary>
        /// <param name='e'>
        /// FSEvent.
        /// </param>
        private void handleFolderEvents (FSEvent e)
        {
            var movedEvent = e as FSMovedEvent;
            FolderEvent folderEvent;
            if (movedEvent != null) {
                folderEvent = new FolderMovedEvent (
                    fsFactory.CreateDirectoryInfo (movedEvent.OldPath),
                    fsFactory.CreateDirectoryInfo (movedEvent.Path),
                    null, null) {Local = MetaDataChangeType.MOVED};
            } else {
                folderEvent = new FolderEvent (fsFactory.CreateDirectoryInfo (e.Path), null);
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
            Queue.AddEvent (folderEvent);
        }

        /// <summary>
        /// Handles the FSEvents of files and creates FileEvents.
        /// </summary>
        /// <param name='e'>
        /// FSEvent.
        /// </param>
        private void handleFileEvents (FSEvent e)
        {
            var movedEvent = e as FSMovedEvent;
            if (movedEvent != null) {
                var oldfile = fsFactory.CreateFileInfo(movedEvent.OldPath);
                var newfile = fsFactory.CreateFileInfo(movedEvent.Path);
                var newEvent = new FileMovedEvent (
                    oldfile,
                    newfile,
                    null, newfile.Directory,
                    null, null);
                Queue.AddEvent (newEvent);
            } else {
                var file = fsFactory.CreateFileInfo(e.Path);
                var newEvent = new FileEvent (file, file.Directory, null);
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
                Queue.AddEvent (newEvent);
            }

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
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose the specified disposing.
        /// </summary>
        /// <param name="disposing">If set to <c>true</c> disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                // Indicate that the instance has been disposed.
                _disposed = true;
            }
        }

        #endregion
    }
}
