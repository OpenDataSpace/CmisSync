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

    public class QueuedCategorizedEventsCounter : IObservable<Tuple<string, int>>, IEventCounter {
        private List<IObserver<Tuple<string, int>>> categoryCounterObservers;
        private ConcurrentDictionary<string, int> categoryCounter;
        private bool disposed = false;
        public QueuedCategorizedEventsCounter() {
            this.categoryCounterObservers = new List<IObserver<Tuple<string, int>>>();
            this.categoryCounter = new ConcurrentDictionary<string, int>();
        }

        /// <summary>
        /// Subscribe observer for all countable events and their category.
        /// </summary>
        /// <param name="observer">Observer for categorized counter.</param>
        /// <returns>A reference to an interface that allows observers to stop receiving notifications before the provider has finished sending them.</returns>
        public virtual IDisposable Subscribe(IObserver<Tuple<string, int>> observer) {
            if (observer == null) {
                throw new ArgumentNullException("Given observer is null");
            }

            if (!this.categoryCounterObservers.Contains(observer)) {
                this.categoryCounterObservers.Add(observer);
            }

            return new Unsubscriber<Tuple<string, int>>(this.categoryCounterObservers, observer);
        }

        public void Decrease(ICountableEvent e) {
            var category = e.Category;
            var value = this.categoryCounter.AddOrUpdate(category, 0, delegate(string cat, int counter) {
                return counter - 1;
            });
            foreach (var observer in this.categoryCounterObservers) {
                observer.OnNext(new Tuple<string, int>(category, value));
            }
        }

        public void Increase(ICountableEvent e) {
            string category = e.Category;
            var value = this.categoryCounter.AddOrUpdate(category, 1, delegate(string cat, int counter) {
                return counter + 1;
            });
            foreach (var observer in this.categoryCounterObservers) {
                observer.OnNext(new Tuple<string, int>(category, value));
            }
        }

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