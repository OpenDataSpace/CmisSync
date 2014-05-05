using System;

namespace CmisSync.Lib.Events.Filter
{
    /// <summary>
    /// Abstract file filter. It takes an event queue make it possible to report any filtered event by requeueing an ignore Event to the queue
    /// </summary>
    public abstract class AbstractFileFilter : SyncEventHandler
    {
        private static readonly int DEFAULT_FILTER_PRIORITY = 9999;
        /// <summary>
        /// The queue where the ignores should be reported to.
        /// </summary>
        protected readonly ISyncEventQueue Queue;
        /// <summary>
        /// Default filter priority is set to 9999. May not be changed during runtime.
        /// </summary>
        /// <value>
        /// The priority of the filter.
        /// </value>
        public override int Priority { get { return DEFAULT_FILTER_PRIORITY; } }

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

