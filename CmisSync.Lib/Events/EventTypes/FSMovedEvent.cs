using System;
using System.IO;

namespace CmisSync.Lib.Events
{
    public class FSMovedEvent : FSEvent
    {
        public virtual string OldPath { get; private set; }

        public FSMovedEvent (string oldPath, string newPath) : base(WatcherChangeTypes.Renamed, newPath)
        {
            OldPath = oldPath;
        }
    }
}

