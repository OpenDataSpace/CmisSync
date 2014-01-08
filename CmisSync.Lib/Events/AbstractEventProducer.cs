using System;

namespace CmisSync.Lib.Events
{
    /// <summary>
    /// Abstract event producer.
    /// </summary>
    public abstract class AbstractEventProducer
    {
        /// <summary>
        /// Gets the queue where events can be added.
        /// </summary>
        /// <value>
        /// The queue.
        /// </value>
        protected SyncEventQueue Queue { get; private set; }
        public AbstractEventProducer (SyncEventQueue queue)
        {
            if(queue == null)
                throw new ArgumentNullException("The given event queue must no be null");
            Queue = queue;
        }
    }
}

