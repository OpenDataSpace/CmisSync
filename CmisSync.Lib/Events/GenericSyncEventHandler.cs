using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CmisSync.Lib.Events
{
    /// <summary>
    /// Generic sync event delegate.
    /// Only Events are passed, which are the same type as the given generic type.
    /// </summary>
    public delegate bool GenericSyncEventDelegate<TSyncEventType>(ISyncEvent e);
    /// <summary>
    /// Generic sync event handler. Takes a delegate as handler which is called, if the event is a type of the generic event.
    /// </summary>
    public class GenericSyncEventHandler<TSyncEventType> : SyncEventHandler
    {
        private int priority;
        /// <summary>
        /// Cannot be changed after the constructor has been called.
        /// </summary>
        /// <value>
        /// The priority.
        /// </value>
        public override int Priority { get { return priority; } }

        private GenericSyncEventDelegate<TSyncEventType> Handler;

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Events.GenericSyncEventHandler"/> class.
        /// </summary>
        /// <param name='priority'>
        /// The priority for the queue.
        /// </param>
        /// <param name='handler'>
        /// Delegate which should be called if any Event of the given TSyncEventType is passed from the queue.
        /// </param>
        public GenericSyncEventHandler(int priority, GenericSyncEventDelegate<TSyncEventType> handler)
        {
            this.priority = priority;
            Handler = handler;
        }

        /// <summary>
        /// Handles only the specified TSyncEventType events by passing it to the given delegate.
        /// </summary>
        /// <param name='e'>
        /// All events can be passed, but only fitting events will be handled by the delegate. Otherwise this returns false as default.
        /// </param>
        public override bool Handle(ISyncEvent e)
        {
            if (e is TSyncEventType)
                return Handler(e);
            else 
                return false;
        }
    }
}
