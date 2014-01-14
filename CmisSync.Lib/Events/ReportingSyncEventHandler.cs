using System;

using log4net;

namespace CmisSync.Lib.Events
{

    public abstract class ReportingSyncEventHandler : SyncEventHandler
    {

        protected readonly ISyncEventQueue Queue;
        public ReportingSyncEventHandler(ISyncEventQueue queue) : base() {
            if( queue == null )
                throw new ArgumentNullException("Given SyncEventQueue was null");
            this.Queue = queue;
        }

    }
}

