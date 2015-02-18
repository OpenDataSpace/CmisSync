

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