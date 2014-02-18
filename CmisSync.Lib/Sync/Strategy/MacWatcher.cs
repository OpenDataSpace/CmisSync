using System;
using System.IO;

using MonoMac.Foundation;
using MonoMac.CoreServices;

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

        public MacWatcher (FSEventStream stream, ISyncEventQueue queue) : base(queue)
        {
            FsStream = stream;
            EnableEvents = false;

            stream.Events += OnFSEventStreamEvents;
            stream.ScheduleWithRunLoop (NSRunLoop.Current);
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

