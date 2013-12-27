using System;

namespace CmisSync.Lib.Events.Filter
{
    public abstract class AbstractFileFilter : SyncEventHandler
    {
        private static readonly int DEFAULT_FILTER_PRIORITY = 9999;
        protected SyncEventQueue Queue;
        public override int Priority { get { return DEFAULT_FILTER_PRIORITY; } }
        public AbstractFileFilter(SyncEventQueue queue) {
            if( queue == null)
                throw new ArgumentNullException("The given queue must not be null, bacause the Filters are reporting their filtered events to this queue");
            this.Queue = queue;
        }
    }
}

