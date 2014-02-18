using System;

using MonoMac.Foundation;
using MonoMac.CoreServices;

using log4net;

using CmisSync.Lib.Events;


namespace CmisSync.Lib.Sync.Strategy
{
    public class MacWatcher : Watcher
    {
        private FSEventStream FsStream;

        public MacWatcher (FSEventStream stream, ISyncEventQueue queue) : base(queue)
        {
            FsStream = stream;
        }
    }
}

