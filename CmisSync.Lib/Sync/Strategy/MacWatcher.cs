using System;
using System.IO;

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
                if (value) {
                    isStarted = FsStream.Start ();
                } else {
                    FsStream.Stop ();
                    isStarted = false;
                }
            }
        }

        private FSEventStream FsStream;
        private bool isStarted = false;

        public MacWatcher (string pathname, ISyncEventQueue queue) : base(queue)
        {
            NSApplication.Init ();

            if (String.IsNullOrEmpty(pathname))
                throw new ArgumentNullException ("The given fs stream must not be null");
            FsStream = new FSEventStream (new [] { pathname }, TimeSpan.FromSeconds (1), FSEventStreamCreateFlags.FileEvents);
            EnableEvents = false;

            FsStream.Events += OnFSEventStreamEvents;
            FsStream.ScheduleWithRunLoop (NSRunLoop.Current);
        }

        private void OnFSEventStreamEvents (object sender, FSEventStreamEventsArgs e)
        {
            foreach(MonoMac.CoreServices.FSEvent fsEvent in e.Events) {
                if ((fsEvent.Flags & FSEventStreamEventFlags.ItemCreated) != 0) {
                    if (File.Exists (fsEvent.Path)) {
                        Queue.AddEvent (new CmisSync.Lib.Events.FSEvent (WatcherChangeTypes.Created, fsEvent.Path));
                        return;
                    }
                }
                if ((fsEvent.Flags & FSEventStreamEventFlags.ItemRemoved) != 0) {
                    if (!File.Exists (fsEvent.Path)) {
                        Queue.AddEvent (new CmisSync.Lib.Events.FSEvent (WatcherChangeTypes.Deleted, fsEvent.Path));
                        return;
                    }
                }
                if ((fsEvent.Flags & FSEventStreamEventFlags.ItemModified) != 0) {
                    if (File.Exists (fsEvent.Path)) {
                        Queue.AddEvent (new CmisSync.Lib.Events.FSEvent (WatcherChangeTypes.Changed, fsEvent.Path));
                    }
                }
                if ((fsEvent.Flags & FSEventStreamEventFlags.ItemRenamed) != 0) {
                    //TODO rename optimization
                    if (File.Exists (fsEvent.Path)) {
                        Queue.AddEvent (new CmisSync.Lib.Events.FSEvent (WatcherChangeTypes.Created, fsEvent.Path));
                    } else {
                        Queue.AddEvent (new CmisSync.Lib.Events.FSEvent (WatcherChangeTypes.Deleted, fsEvent.Path));
                    }
                }
            }
        }
    }
}

