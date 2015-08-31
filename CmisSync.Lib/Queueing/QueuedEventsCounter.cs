//-----------------------------------------------------------------------
// <copyright file="QueuedEventsCounter.cs" company="GRAU DATA AG">
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
    using System.Collections.Generic;
    using System.Threading;

    using CmisSync.Lib.Events;

    /// <summary>
    /// Queued events counter.
    /// </summary>
    public class QueuedEventsCounter : IObservable<int>, IEventCounter {
        private List<IObserver<int>> fullCounterObservers;
        private int fullCounter;
        private bool disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Queueing.QueuedEventsCounter"/> class.
        /// </summary>
        public QueuedEventsCounter() {
            this.fullCounterObservers = new List<IObserver<int>>();
        }

        /// <summary>
        /// Subscribe observer for all countable events.
        /// </summary>
        /// <param name="observer">Observer for all countable events.</param>
        /// <returns>A reference to an interface that allows observers to stop receiving notifications before the provider has finished sending them.</returns>
        public virtual IDisposable Subscribe(IObserver<int> observer) {
            if (observer == null) {
                throw new ArgumentNullException("observer");
            }

            if (!this.fullCounterObservers.Contains(observer)) {
                this.fullCounterObservers.Add(observer);
            }

            return new Unsubscriber<int>(this.fullCounterObservers, observer);
        }

        /// <summary>
        /// Decrease the counter if event fits.
        /// </summary>
        /// <param name="e">Countable event.</param>
        public void Decrease(ICountableEvent e) {
            int fullcounter = Interlocked.Decrement(ref this.fullCounter);
            foreach (var observer in this.fullCounterObservers) {
                observer.OnNext(fullcounter);
            }
        }

        /// <summary>
        /// Increase the counter if event fits.
        /// </summary>
        /// <param name="e">Countable event.</param>
        public void Increase(ICountableEvent e) {
            int fullcounter = Interlocked.Increment(ref this.fullCounter);
            foreach (var observer in this.fullCounterObservers) {
                observer.OnNext(fullcounter);
            }
        }

        /// <summary>
        /// Releases all resource used by the <see cref="CmisSync.Lib.Queueing.QueuedEventsCounter"/> object.
        /// </summary>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the
        /// <see cref="CmisSync.Lib.Queueing.QueuedEventsCounter"/>. The <see cref="Dispose"/> method leaves the
        /// <see cref="CmisSync.Lib.Queueing.QueuedEventsCounter"/> in an unusable state. After calling
        /// <see cref="Dispose"/>, you must release all references to the
        /// <see cref="CmisSync.Lib.Queueing.QueuedEventsCounter"/> so the garbage collector can reclaim the memory that
        /// the <see cref="CmisSync.Lib.Queueing.QueuedEventsCounter"/> was occupying.</remarks>
        public void Dispose() {
            if (this.disposed) {
                return;
            }

            foreach (var observer in this.fullCounterObservers) {
                observer.OnCompleted();
            }

            this.fullCounterObservers.Clear();
            this.disposed = true;
        }
    }
}