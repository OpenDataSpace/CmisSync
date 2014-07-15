//-----------------------------------------------------------------------
// <copyright file="MacWatcher.cs" company="GRAU DATA AG">
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
#if __COCOA__

using System;
using System.IO;
using System.Threading;

using MonoMac.Foundation;
using MonoMac.CoreServices;
using MonoMac.AppKit;

using log4net;

using CmisSync.Lib.Events;


namespace CmisSync.Lib.Sync.Strategy
{
    /// <summary>
    /// Implementation of a Mac OS specific file system watcher.
    /// </summary>
    public class MacWatcher : IWatcherProducer
    {

        private FSEventStream FsStream;
        private bool isStarted = false;
        private bool disposed = false;
        private bool StopRunLoop = false;
        private NSRunLoop RunLoop = null;
        private Thread RunLoopThread = null;
        private readonly ISyncEventQueue Queue;
        private MonoMac.CoreServices.FSEvent? LastRenameEvent;

        /// <summary>
        /// Enables the FSEvent report
        /// </summary>
        /// <value><c>true</c> if enable events; otherwise, <c>false</c>.</value>
        public bool EnableEvents {
            get {
                return isStarted;
            }
            set {
                if (value == isStarted) {
                    return;
                }
                if (value) {
                    isStarted = FsStream.Start();
                    if (isStarted) {
                        FsStream.FlushSync();
                        FsStream.Events += OnFSEventStreamEvents;
                    }
                } else {
                    FsStream.Events -= OnFSEventStreamEvents;
                    FsStream.FlushSync();
                    FsStream.Stop ();
                    isStarted = false;
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Sync.Strategy.MacWatcher"/> class.
        /// The default latency is set to 1 second.
        /// </summary>
        /// <param name="pathname">Path to be monitored.</param>
        /// <param name="queue">Queue to pass the new events to.</param>
        public MacWatcher(string pathname, ISyncEventQueue queue) : this(pathname, queue, TimeSpan.FromSeconds(1))
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Sync.Strategy.MacWatcher"/> class.
        /// </summary>
        /// <param name="pathname">Path to be monitored.</param>
        /// <param name="queue">Queue to pass the new events to.</param>
        /// <param name="latency">Maximum latency for file system events.</param>
        public MacWatcher(string pathname, ISyncEventQueue queue, TimeSpan latency)
        {
            if (string.IsNullOrEmpty(pathname)) {
                throw new ArgumentNullException("The given fs stream must not be null");
            }

            if (queue == null) {
                throw new ArgumentNullException("The given queue must not be null");
            }

            this.Queue = queue;
            this.RunLoopThread = new Thread(() =>
            {
                this.RunLoop = NSRunLoop.Current;
                while (!this.StopRunLoop) {
                    this.RunLoop.RunUntil(NSDate.FromTimeIntervalSinceNow(1));
                    this.CleanLastRenameEvent();
                }
            });

            this.RunLoopThread.Start();
            while (RunLoop == null) {
                Thread.Sleep(10);
            }

            this.FsStream = new FSEventStream(new [] { pathname }, latency, FSEventStreamCreateFlags.FileEvents);
            this.EnableEvents = false;
            this.FsStream.ScheduleWithRunLoop(this.RunLoop);
        }

        /// <summary>
        /// Dispose the FsStream.
        /// </summary>
        /// <param name="disposing">If set to <c>true</c> disposing.</param>
        protected void Dispose(bool disposing)
        {
            if (!disposed) {
                if (disposing) {
                    // Dispose of any managed resources of the derived class here.
                    EnableEvents = false;
                    FsStream.Invalidate();

                    StopRunLoop = true;
                    RunLoopThread.Join();
                    disposed = true;
                }
                // Dispose of any unmanaged resources of the derived class here.
            }
        }
        
        /// <summary>
        /// Releases all resource used by the <see cref="CmisSync.Lib.Sync.Strategy.WatcherConsumer"/> object.
        /// </summary>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="CmisSync.Lib.Sync.Strategy.Watcher"/>.
        /// The <see cref="Dispose"/> method leaves the <see cref="CmisSync.Lib.Sync.Strategy.Watcher"/> in an unusable
        /// state. After calling <see cref="Dispose"/>, you must release all references to the
        /// <see cref="CmisSync.Lib.Sync.Strategy.WatcherConsumer"/> so the garbage collector can reclaim the memory that the
        /// <see cref="CmisSync.Lib.Sync.Strategy.WatcherConsumer"/> was occupying.</remarks>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void OnFSEventStreamEvents (object sender, FSEventStreamEventsArgs e)
        {
            foreach (MonoMac.CoreServices.FSEvent fsEvent in e.Events) {
                bool isFile = (fsEvent.Flags & FSEventStreamEventFlags.ItemIsFile) != 0;
                if ((fsEvent.Flags & FSEventStreamEventFlags.ItemRemoved) != 0 && !FileOrDirectoryExists(fsEvent.Path, isFile)) {
                    this.Queue.AddEvent(new CmisSync.Lib.Events.FSEvent (WatcherChangeTypes.Deleted, fsEvent.Path, !isFile));
                    continue;
                }

                if ((fsEvent.Flags & FSEventStreamEventFlags.ItemCreated) != 0 && FileOrDirectoryExists(fsEvent.Path, isFile)) {
                    this.Queue.AddEvent(new CmisSync.Lib.Events.FSEvent(WatcherChangeTypes.Created, fsEvent.Path, !isFile));
                }

                if (((fsEvent.Flags & FSEventStreamEventFlags.ItemModified) != 0
                    || (fsEvent.Flags & FSEventStreamEventFlags.ItemInodeMetaMod) != 0)
                    && FileOrDirectoryExists(fsEvent.Path, isFile)) {
                    this.Queue.AddEvent(new CmisSync.Lib.Events.FSEvent(WatcherChangeTypes.Changed, fsEvent.Path, !isFile));
                }

                if ((fsEvent.Flags & FSEventStreamEventFlags.ItemRenamed) != 0) {
                    if (FileOrDirectoryExists(fsEvent.Path, isFile)) {
                        if (this.LastRenameEvent != null && this.LastRenameEvent.Value.Id == fsEvent.Id - 1) {
                            this.Queue.AddEvent(new CmisSync.Lib.Events.FSMovedEvent(this.LastRenameEvent.Value.Path, fsEvent.Path, !isFile));
                            this.LastRenameEvent = null;
                        } else {
                            this.Queue.AddEvent(new CmisSync.Lib.Events.FSEvent(WatcherChangeTypes.Created, fsEvent.Path, !isFile));
                        }
                    } else {
                        this.LastRenameEvent = fsEvent;
                        continue;
                    }
                }
                this.CleanLastRenameEvent();
            }
        }

        /// <summary>
        /// Cleans the last rename event. If no corresponding second rename event has been found yet, the rename has been a sign for a deletion.
        /// </summary>
        private void CleanLastRenameEvent() {
            if (this.LastRenameEvent != null)
            {
                bool isFile = (((MonoMac.CoreServices.FSEvent)this.LastRenameEvent).Flags & FSEventStreamEventFlags.ItemIsFile) != 0;
                this.Queue.AddEvent(new CmisSync.Lib.Events.FSEvent(WatcherChangeTypes.Deleted, this.LastRenameEvent.Value.Path, !isFile));
                this.LastRenameEvent = null;
            }
        }

        internal static bool FileOrDirectoryExists(string path, bool isFile)
        {
            return isFile ? File.Exists(path) : Directory.Exists(path);
        }
    }
}

#endif
