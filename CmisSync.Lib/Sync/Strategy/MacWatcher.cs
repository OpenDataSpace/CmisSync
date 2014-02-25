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
    public class MacWatcher : Watcher
    {
        /// <summary>
        /// Enables the FSEvent report
        /// </summary>
        /// <value><c>true</c> if enable events; otherwise, <c>false</c>.</value>
        public override bool EnableEvents {
            get {
                return isStarted;
            }
            set {
                if (value == isStarted) {
                    return;
                }
                if (value) {
                    isStarted = FsStream.Start ();
                    if (isStarted) {
                        FsStream.FlushSync ();
                        FsStream.Events += OnFSEventStreamEvents;
                    }
                } else {
                    FsStream.Events -= OnFSEventStreamEvents;
                    FsStream.FlushSync ();
                    FsStream.Stop ();
                    isStarted = false;
                }
            }
        }

        private FSEventStream FsStream;
        private bool isStarted = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Sync.Strategy.MacWatcher"/> class.
        /// The default latency is set to 1 second.
        /// </summary>
        /// <param name="pathname">Pathname.</param>
        /// <param name="queue">Queue.</param>
        /// <param name="loop">Loop.</param>
        public MacWatcher (string pathname, ISyncEventQueue queue, NSRunLoop loop) : this(pathname, queue, loop, TimeSpan.FromSeconds(1))
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Sync.Strategy.MacWatcher"/> class.
        /// </summary>
        /// <param name="pathname">Pathname.</param>
        /// <param name="queue">Queue.</param>
        /// <param name="loop">Loop.</param>
        /// <param name="latency">Latency.</param>
        public MacWatcher (string pathname, ISyncEventQueue queue, NSRunLoop loop, TimeSpan latency) : base(queue)
        {
            if (String.IsNullOrEmpty (pathname) || loop == null)
                throw new ArgumentNullException ("The given fs stream must not be null");
            FsStream = new FSEventStream (new [] { pathname }, latency, FSEventStreamCreateFlags.FileEvents);
            EnableEvents = false;
            FsStream.ScheduleWithRunLoop (loop);
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="CmisSync.Lib.Sync.Strategy.MacWatcher"/> is reclaimed by garbage collection.
        /// </summary>
        ~MacWatcher()
        {
            EnableEvents = false;
            FsStream.Invalidate ();
        }

        private void OnFSEventStreamEvents (object sender, FSEventStreamEventsArgs e)
        {
            foreach(MonoMac.CoreServices.FSEvent fsEvent in e.Events) {
                if ((fsEvent.Flags & FSEventStreamEventFlags.ItemRemoved) != 0) {
                    if ((fsEvent.Flags & FSEventStreamEventFlags.ItemIsFile) != 0 && !File.Exists (fsEvent.Path)) {
                        Queue.AddEvent (new CmisSync.Lib.Events.FSEvent (WatcherChangeTypes.Deleted, fsEvent.Path));
                        return;
                    }
                    if ((fsEvent.Flags & FSEventStreamEventFlags.ItemIsDir) != 0 && !Directory.Exists (fsEvent.Path)) {
                        Queue.AddEvent (new CmisSync.Lib.Events.FSEvent (WatcherChangeTypes.Deleted, fsEvent.Path));
                        return;
                    }
                }
                if ((fsEvent.Flags & FSEventStreamEventFlags.ItemCreated) != 0) {
                    if ((fsEvent.Flags & FSEventStreamEventFlags.ItemIsFile) != 0 && File.Exists (fsEvent.Path)) {
                        Queue.AddEvent (new CmisSync.Lib.Events.FSEvent (WatcherChangeTypes.Created, fsEvent.Path));
                    }
                    if ((fsEvent.Flags & FSEventStreamEventFlags.ItemIsDir) != 0 && Directory.Exists (fsEvent.Path)) {
                        Queue.AddEvent (new CmisSync.Lib.Events.FSEvent (WatcherChangeTypes.Created, fsEvent.Path));
                    }
                }
                if ((fsEvent.Flags & FSEventStreamEventFlags.ItemModified) != 0 || (fsEvent.Flags & FSEventStreamEventFlags.ItemInodeMetaMod) != 0) {
                    if ((fsEvent.Flags & FSEventStreamEventFlags.ItemIsFile) != 0 && File.Exists (fsEvent.Path)) {
                        Queue.AddEvent (new CmisSync.Lib.Events.FSEvent (WatcherChangeTypes.Changed, fsEvent.Path));
                    }
                    if ((fsEvent.Flags & FSEventStreamEventFlags.ItemIsDir) != 0 && Directory.Exists (fsEvent.Path)) {
                        Queue.AddEvent (new CmisSync.Lib.Events.FSEvent (WatcherChangeTypes.Changed, fsEvent.Path));
                    }
                }
                if ((fsEvent.Flags & FSEventStreamEventFlags.ItemRenamed) != 0) {
                    //TODO rename optimization
                    if ((fsEvent.Flags & FSEventStreamEventFlags.ItemIsFile) != 0) {
                        if (File.Exists (fsEvent.Path)) {
                            Queue.AddEvent (new CmisSync.Lib.Events.FSEvent (WatcherChangeTypes.Created, fsEvent.Path));
                        } else {
                            Queue.AddEvent (new CmisSync.Lib.Events.FSEvent (WatcherChangeTypes.Deleted, fsEvent.Path));
                        }
                    }
                    if ((fsEvent.Flags & FSEventStreamEventFlags.ItemIsDir) != 0) {
                        if (Directory.Exists (fsEvent.Path)) {
                            Queue.AddEvent (new CmisSync.Lib.Events.FSEvent (WatcherChangeTypes.Created, fsEvent.Path));
                        } else {
                            Queue.AddEvent (new CmisSync.Lib.Events.FSEvent (WatcherChangeTypes.Deleted, fsEvent.Path));
                        }
                    }
                }
            }
        }
    }
}

#endif
