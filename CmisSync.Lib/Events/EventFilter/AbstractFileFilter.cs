using System;

namespace CmisSync.Lib.Events.Filter
{
    /// <summary>
    /// Abstract file filter. It takes an event queue make it possible to report any filtered event by requeueing an ignore Event to the queue
    /// </summary>
    public abstract class AbstractFileFilter : SyncEventHandler
    {
        /// <summary>
        /// The queue where the ignores should be reported to.
        /// </summary>
        protected readonly ISyncEventQueue Queue;

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Events.Filter.AbstractFileFilter"/> class.
        /// </summary>
        /// <param name='queue'>
        /// Queue where all filtered events should be reported to.
        /// </param>
        public AbstractFileFilter(ISyncEventQueue queue) {
            if( queue == null)
                throw new ArgumentNullException("The given queue must not be null, bacause the Filters are reporting their filtered events to this queue");
            this.Queue = queue;
        }
    }
}

