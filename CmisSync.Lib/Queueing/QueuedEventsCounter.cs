
namespace CmisSync.Lib.Queueing {
    using System;
    using System.Collections.Generic;
    using System.Threading;

    using CmisSync.Lib.Events;

    public class QueuedEventsCounter : IObservable<int>, IEventCounter{
        private List<IObserver<int>> fullCounterObservers;
        private int fullCounter = 0;
        private bool disposed = false;

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
                throw new ArgumentNullException("Given observer is null");
            }

            if (!this.fullCounterObservers.Contains(observer)) {
                this.fullCounterObservers.Add(observer);
            }

            return new Unsubscriber<int>(this.fullCounterObservers, observer);
        }

        public void Decrease(ICountableEvent e) {
            int fullcounter = Interlocked.Decrement(ref this.fullCounter);
            foreach (var observer in this.fullCounterObservers) {
                observer.OnNext(fullcounter);
            }
        }

        public void Increase(ICountableEvent e) {
            int fullcounter = Interlocked.Increment(ref this.fullCounter);
            foreach (var observer in this.fullCounterObservers) {
                observer.OnNext(fullcounter);
            }
        }

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