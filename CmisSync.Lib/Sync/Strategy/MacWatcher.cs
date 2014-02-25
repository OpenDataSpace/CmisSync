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
    public class MacWatcher : Watcher
    {
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

        public MacWatcher (string pathname, ISyncEventQueue queue, NSRunLoop loop) : base(queue)
        {
            if (String.IsNullOrEmpty (pathname) || loop == null)
                throw new ArgumentNullException ("The given fs stream must not be null");

            FsStream = new FSEventStream (new [] { pathname }, TimeSpan.FromSeconds (1), FSEventStreamCreateFlags.FileEvents);
            EnableEvents = false;
            FsStream.ScheduleWithRunLoop (loop);
        }

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
