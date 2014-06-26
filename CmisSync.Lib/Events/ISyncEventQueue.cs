//-----------------------------------------------------------------------
// <copyright file="ISyncEventQueue.cs" company="GRAU DATA AG">
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
    using System.Collections.Concurrent;
    using System.Threading.Tasks;

    using log4net;

    /// <summary>
    /// Interface for all implementations of SyncEventQueues.
    /// This interface is the "usage interface" which does not contain lifecyle related Methods.
    /// The other interface which in fact contains them is <see cref="IDisposableEventQueue"/>
    /// </summary>
    public interface ISyncEventQueue {
        /// <summary>
        /// Gets a value indicating whether this instance is stopped.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is stopped; otherwise, <c>false</c>.
        /// </value>
        bool IsStopped { get; } 

        /// <summary>
        /// Gets the event manager.
        /// </summary>
        /// <value>
        /// The event manager.
        /// </value>
        ISyncEventManager EventManager { get; }

        /// <summary>
        /// Adds the event.
        /// </summary>
        /// <param name='newEvent'>
        /// New event.
        /// </param>
        /// <exception cref="InvalidOperationException">When Listener is already stopped</exception>
        void AddEvent(ISyncEvent newEvent);

        /// <summary>
        /// Suspend the queue consumer thread after finished the processing of the actual event.
        /// </summary>
        void Suspend();

        /// <summary>
        /// Continue the queue consumer if it is suspended.
        /// </summary>
        void Continue();
    }
}
