//-----------------------------------------------------------------------
// <copyright file="QueuedCategorizedEventsCounter.cs" company="GRAU DATA AG">
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
    using System.Collections.Concurrent;

    using CmisSync.Lib.Events;

    /// <summary>
    /// Queued categorized events counter.
    /// </summary>
    public class QueuedCategorizedEventsCounter : IObservable<Tuple<EventCategory, int>>, IEventCounter {
        private List<IObserver<Tuple<EventCategory, int>>> categoryCounterObservers;
        private ConcurrentDictionary<EventCategory, int> categoryCounter;
        private bool disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Queueing.QueuedCategorizedEventsCounter"/> class.
        /// </summary>
        public QueuedCategorizedEventsCounter() {
            this.categoryCounterObservers = new List<IObserver<Tuple<EventCategory, int>>>();
            this.categoryCounter = new ConcurrentDictionary<EventCategory, int>();
        }

        /// <summary>
        /// Subscribe observer for all countable events and their category.
        /// </summary>
        /// <param name="observer">Observer for categorized counter.</param>
        /// <returns>A reference to an interface that allows observers to stop receiving notifications before the provider has finished sending them.</returns>
        public virtual IDisposable Subscribe(IObserver<Tuple<EventCategory, int>> observer) {
            if (observer == null) {
                throw new ArgumentNullException("observer");
            }

            if (!this.categoryCounterObservers.Contains(observer)) {
                this.categoryCounterObservers.Add(observer);
            }

            return new Unsubscriber<Tuple<EventCategory, int>>(this.categoryCounterObservers, observer);
        }

        /// <summary>
        /// Decrease the counter if event fits.
        /// </summary>
        /// <param name="e">Countable event.</param>
        public void Decrease(ICountableEvent e) {
            if (this.disposed) {
                throw new ObjectDisposedException(this.GetType().Name);
            }

            if (e == null) {
                throw new ArgumentNullException("e");
            }

            var category = e.Category;
            var value = this.categoryCounter.AddOrUpdate(category, 0, delegate(EventCategory cat, int counter) {
                return counter - 1;
            });
            foreach (var observer in this.categoryCounterObservers) {
                observer.OnNext(new Tuple<EventCategory, int>(category, value));
            }
        }

        /// <summary>
        /// Increase the counter if event fits.
        /// </summary>
        /// <param name="e">Countable event.</param>
        public void Increase(ICountableEvent e) {
            if (this.disposed) {
                throw new ObjectDisposedException(this.GetType().Name);
            }

            if (e == null) {
                throw new ArgumentNullException("e");
            }

            var category = e.Category;
            var value = this.categoryCounter.AddOrUpdate(category, 1, delegate(EventCategory cat, int counter) {
                return counter + 1;
            });
            foreach (var observer in this.categoryCounterObservers) {
                observer.OnNext(new Tuple<EventCategory, int>(category, value));
            }
        }

        /// <summary>
        /// Releases all resource used by the <see cref="CmisSync.Lib.Queueing.QueuedCategorizedEventsCounter"/> object.
        /// </summary>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the
        /// <see cref="CmisSync.Lib.Queueing.QueuedCategorizedEventsCounter"/>. The <see cref="Dispose"/> method leaves
        /// the <see cref="CmisSync.Lib.Queueing.QueuedCategorizedEventsCounter"/> in an unusable state. After calling
        /// <see cref="Dispose"/>, you must release all references to the
        /// <see cref="CmisSync.Lib.Queueing.QueuedCategorizedEventsCounter"/> so the garbage collector can reclaim the
        /// memory that the <see cref="CmisSync.Lib.Queueing.QueuedCategorizedEventsCounter"/> was occupying.</remarks>
        public void Dispose() {
            if (this.disposed) {
                return;
            }

            foreach (var observer in this.categoryCounterObservers) {
                observer.OnCompleted();
            }

            this.categoryCounterObservers.Clear();
            this.disposed = true;
        }
    }
}