//-----------------------------------------------------------------------
// <copyright file="SyncEventQueue.cs" company="GRAU DATA AG">
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
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using CmisSync.Lib.Events;

    using log4net;

    /// <summary>
    /// Sync event queue.
    /// </summary>
    public class SyncEventQueue : ICountingQueue {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(SyncEventQueue));
        private BlockingCollection<ISyncEvent> queue = new BlockingCollection<ISyncEvent>();
        private Task consumer;
        private AutoResetEvent suspendHandle = new AutoResetEvent(false);
        private bool alreadyDisposed = false;
        private bool suspend = false;
        private List<IObserver<int>> fullCounterObservers;
        private int fullCounter = 0;
        private List<IObserver<Tuple<string, int>>> categoryCounterObservers;
        private ConcurrentDictionary<string, int> categoryCounter;
        private object subscriberLock = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Queueing.SyncEventQueue"/> class.
        /// </summary>
        /// <param name="manager">Manager holding the handler.</param>
        public SyncEventQueue(ISyncEventManager manager) {
            if (manager == null) {
                throw new ArgumentException("manager may not be null");
            }

            this.fullCounterObservers = new List<IObserver<int>>();
            this.categoryCounterObservers = new List<IObserver<Tuple<string, int>>>();
            this.categoryCounter = new ConcurrentDictionary<string, int>();
            this.EventManager = manager;
            this.consumer = new Task(() => this.Listen(this.queue, this.EventManager, this.suspendHandle));
            this.consumer.Start();
        }

        /// <summary>
        /// Gets the event manager.
        /// </summary>
        /// <value>The event manager.</value>
        public ISyncEventManager EventManager { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is stopped.
        /// </summary>
        /// <value><c>true</c> if this instance is stopped; otherwise, <c>false</c>.</value>
        public bool IsStopped {
            get {
                return this.consumer.IsCompleted;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is empty.
        /// </summary>
        /// <value><c>true</c> if this instance is empty; otherwise, <c>false</c>.</value>
        public bool IsEmpty {
            get {
                return this.queue.Count == 0;
            }
        }

        /// <summary>
        /// Adds the event to the queue.
        /// </summary>
        /// <param name="newEvent">New event.</param>
        /// <exception cref="InvalidOperationException">When Listener is already stopped</exception>
        public virtual void AddEvent(ISyncEvent newEvent) {
            if (this.alreadyDisposed) {
                Logger.Info(string.Format("Queue was already Disposed. Dropping Event: {0}", newEvent.ToString()));
                return;
            }

            if (this.IsStopped) {
                Logger.Info(string.Format("Queue was already Stopped. Dropping Event: {0}", newEvent.ToString()));
                return;
            }

            try {
                if (newEvent is ICountableEvent) {
                    string category = (newEvent as ICountableEvent).Category;
                    if (!string.IsNullOrEmpty(category)) {
                        int fullcounter = Interlocked.Increment(ref this.fullCounter);
                        lock (subscriberLock) {
                            foreach (var observer in this.fullCounterObservers) {
                                observer.OnNext(fullcounter);
                            }

                            var value = this.categoryCounter.AddOrUpdate(category, 1, delegate(string cat, int counter) {
                                return counter + 1;
                            });
                            foreach (var observer in this.categoryCounterObservers) {
                                observer.OnNext(new Tuple<string, int>(category, value));
                            }
                        }
                    }
                }

                this.queue.Add(newEvent);
                if(!(newEvent is IRemoveFromLoggingEvent)) {
                    Logger.Debug(string.Format("Added Event: {0}", newEvent.ToString()));
                }

            } catch(InvalidOperationException) {
                Logger.Info(string.Format("Queue was already Stopped. Dropping Event: {0}", newEvent.ToString()));
            }
        }

        /// <summary>
        /// Stops the listener.
        /// </summary>
        public void StopListener() {
            if (this.alreadyDisposed) {
                return;
            }

            this.queue.CompleteAdding();
        }

        /// <summary>
        /// Waits for stopped.
        /// </summary>
        public void WaitForStopped() {
            this.consumer.Wait();
        }

        /// <summary>
        /// Waits for stopped.
        /// </summary>
        /// <returns><c>true</c>, if stopped in period of timeout, <c>false</c> otherwise.</returns>
        /// <param name="timeout">Timeout time span.</param>
        public bool WaitForStopped(TimeSpan timeout) {
            return this.consumer.Wait(timeout);
        }

        /// <summary>
        /// Waits for stopped.
        /// </summary>
        /// <returns><c>true</c>, if stopped in period of timeout, <c>false</c> otherwise.</returns>
        /// <param name="milisecondsTimeout">Miliseconds timeout.</param>
        public bool WaitForStopped(int milisecondsTimeout) {
            return this.consumer.Wait(milisecondsTimeout);
        }

        /// <summary>
        /// Releases all resource used by the <see cref="CmisSync.Lib.Queueing.SyncEventQueue"/> object.
        /// </summary>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the
        /// <see cref="CmisSync.Lib.Queueing.SyncEventQueue"/>. The <see cref="Dispose"/> method leaves the
        /// <see cref="CmisSync.Lib.Queueing.SyncEventQueue"/> in an unusable state. After calling
        /// <see cref="Dispose"/>, you must release all references to the
        /// <see cref="CmisSync.Lib.Queueing.SyncEventQueue"/> so the garbage collector can reclaim the memory that the
        /// <see cref="CmisSync.Lib.Queueing.SyncEventQueue"/> was occupying.</remarks>
        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Suspend the queue consumer thread after finished the processing of the actual event.
        /// </summary>
        public void Suspend() {
            this.suspend = true;
        }

        /// <summary>
        /// Continue the queue consumer if it is suspended.
        /// </summary>
        public void Continue() {
            this.suspend = false;
            this.suspendHandle.Set();
        }

        /// <summary>
        /// Subscribe observer for all countable events.
        /// </summary>
        /// <param name="observer">Observer.</param>
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

        /// <summary>
        /// Subscribe observer for all countable events and their category.
        /// </summary>
        /// <param name="observer">Observer.</param>
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

        /// <summary>
        /// Dispose the this instance if isDisposing is true.
        /// </summary>
        /// <param name="isDisposing">If set to <c>true</c> is disposing.</param>
        protected virtual void Dispose(bool isDisposing) {
            if (this.alreadyDisposed) {
                return;
            }

            if (!this.IsStopped) {
                Logger.Warn("Disposing a not yet stopped SyncEventQueue");
            }

            if (isDisposing) {
                this.queue.Dispose();
                foreach (var observer in this.categoryCounterObservers) {
                    observer.OnCompleted();
                }

                foreach (var observer in this.fullCounterObservers) {
                    observer.OnCompleted();
                }

                this.categoryCounterObservers.Clear();
                this.fullCounterObservers.Clear();
            }

            this.alreadyDisposed = true;
        }

        private void Listen(BlockingCollection<ISyncEvent> queue, ISyncEventManager manager, WaitHandle waitHandle) {
            Logger.Debug("Starting to listen on SyncEventQueue");
            while (!queue.IsCompleted)
            {
                ISyncEvent syncEvent = null;

                // Blocks if number.Count == 0
                // IOE means that Take() was called on a completed collection.
                // Some other thread can call CompleteAdding after we pass the
                // IsCompleted check but before we call Take.
                // In this example, we can simply catch the exception since the
                // loop will break on the next iteration.
                try {
                    syncEvent = queue.Take();
                } catch (InvalidOperationException) {
                }

                if (syncEvent != null) {
                    try {
                        if (this.suspend) {
                            Logger.Debug("Suspending sync");
                            waitHandle.WaitOne();
                            Logger.Debug("Continue sync");
                        }

                        manager.Handle(syncEvent);
                        if (syncEvent is ICountableEvent) {
                            string category = (syncEvent as ICountableEvent).Category;
                            if (!string.IsNullOrEmpty(category)) {
                                int fullcounter = Interlocked.Decrement(ref this.fullCounter);
                                lock (subscriberLock) {
                                    foreach (var observer in this.fullCounterObservers) {
                                        observer.OnNext(fullcounter);
                                    }

                                    var value = this.categoryCounter.AddOrUpdate(category, 0, delegate(string cat, int counter) {
                                        return counter - 1;
                                    });
                                    foreach (var observer in this.categoryCounterObservers) {
                                        observer.OnNext(new Tuple<string, int>(category, value));
                                    }
                                }
                            }
                        }
                    } catch(Exception e) {
                        Logger.Error(string.Format("Exception in EventHandler on Event {0}: ", syncEvent.ToString()), e);
                    }
                }
            }

            Logger.Debug("Stopping to listen on SyncEventQueue");
        }

        private class Unsubscriber<T> : IDisposable
        {
            private List<IObserver<T>>_observers;
            private IObserver<T> _observer;

            public Unsubscriber(List<IObserver<T>> observers, IObserver<T> observer) {
                this._observers = observers;
                this._observer = observer;
            }

            public void Dispose() {
                if (_observer != null && _observers.Contains(_observer)) {
                    _observers.Remove(_observer);
                }
            }
        }
    }
}