using System;

using CmisSync.Lib.Events;

namespace CmisSync.Lib.Sync
{
    public class FolderSynchronizer : ReportingSyncEventHandler
    {
        public static readonly int FOLDER_SYNCHRONIZER_PRIORITY = 0;
        public FolderSynchronizer (SyncEventQueue queue) : base (queue)
        {
        }

        public override int Priority {
            get {
                return FOLDER_SYNCHRONIZER_PRIORITY;
            }
        }

        public override bool Handle (ISyncEvent e)
        {
            if(e is FolderEvent) {
                return true;
            }
            return false;
        }
    }
}

