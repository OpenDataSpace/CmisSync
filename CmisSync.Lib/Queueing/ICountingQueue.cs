//-----------------------------------------------------------------------
// <copyright file="ICountingQueue.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Queueing {
    using System;
    using System.Threading;

    using CmisSync.Lib.Events;

    /// <summary>
    /// I counting queue counts every countable event by its category if its added and substracts it if the event is handled and removed from queue.
    /// This queue also notifies listener about changes on categories or all countable events.
    /// </summary>
    public interface ICountingQueue : IDisposableSyncEventQueue {
        /// <summary>
        /// Occurs when an exception is thrown on handling a given ISyncEvent from queue.
        /// </summary>
        event EventHandler<ThreadExceptionEventArgs> OnException;

        /// <summary>
        /// Gets the full counter.
        /// </summary>
        /// <value>The full counter.</value>
        IObservable<int> FullCounter { get; }

        /// <summary>
        /// Gets the category counter.
        /// </summary>
        /// <value>The category counter.</value>
        IObservable<Tuple<EventCategory, int>> CategoryCounter { get; }
    }
}