//-----------------------------------------------------------------------
// <copyright file="GenericSyncEventHandler.cs" company="GRAU DATA AG">
//
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General private License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//
//   This program is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//   GNU General private License for more details.
//
//   You should have received a copy of the GNU General private License
//   along with this program. If not, see http://www.gnu.org/licenses/.
//
// </copyright>
//-----------------------------------------------------------------------

namespace CmisSync.Lib.Events
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Generic sync event delegate.
    /// Only Events are passed, which are the same type as the given generic type.
    /// </summary>
    /// <param name='e'>
    /// The event, which is the same type like TSyncEventType
    /// </param>
    /// <returns>
    /// <c>true</c> if the event has been handled, otherwise <c>false</c>
    /// </returns>
    public delegate bool GenericSyncEventDelegate<TSyncEventType>(ISyncEvent e);

    /// <summary>
    /// Generic sync event handler. Takes a delegate as handler which is called, if the event is a type of the generic event.
    /// </summary>
    public class GenericSyncEventHandler<TSyncEventType> : SyncEventHandler
    {
        private GenericSyncEventDelegate<TSyncEventType> handler;
        private int priority;

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Events.GenericSyncEventHandler`1"/> class.
        /// </summary>
        /// <param name='handler'>
        /// Handler which will be called on incomming event.
        /// </param>
        public GenericSyncEventHandler(GenericSyncEventDelegate<TSyncEventType> handler)
            : this(EventHandlerPriorities.GetPriority(typeof(GenericSyncEventHandler<>)), handler)
        {
        }

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
            this.handler = handler;
        }

        /// <summary>
        /// Cannot be changed after the constructor has been called.
        /// </summary>
        /// <value>
        /// The priority.
        /// </value>
        public override int Priority
        {
            get
            {
                return this.priority;
            }
        }

        /// <summary>
        /// Handles only the specified TSyncEventType events by passing it to the given delegate.
        /// </summary>
        /// <param name='e'>
        /// All events can be passed, but only fitting events will be handled by the delegate. Otherwise this returns false as default.
        /// </param>
        /// <returns>
        /// <c>false</c> if the event is not the same type like the given generic class, otherwise the result of the callback handler.
        /// </returns>
        public override bool Handle(ISyncEvent e)
        {
            return (e is TSyncEventType) ? this.handler(e) : false;
        }
    }
}
