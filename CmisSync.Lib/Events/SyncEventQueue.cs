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

namespace CmisSync.Lib.Events
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;

    using log4net;

    /// <summary>
    /// Sync event queue.
    /// </summary>
    public class SyncEventQueue : IDisposableSyncEventQueue {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(SyncEventQueue));
        private BlockingCollection<ISyncEvent> queue = new BlockingCollection<ISyncEvent>();
        private Task consumer;
        private AutoResetEvent suspendHandle = new AutoResetEvent(false);
        private bool alreadyDisposed = false;
        private bool suspend = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Events.SyncEventQueue"/> class.
        /// </summary>
        /// <param name="manager">Manager holding the handler.</param>
        public SyncEventQueue(ISyncEventManager manager) {
            if(manager == null) {
                throw new ArgumentException("manager may not be null");
            }

            this.EventManager = manager;
            this.consumer = new Task(() => Listen(this.queue, this.EventManager, this.suspendHandle));
            this.consumer.Start();
        }

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
        /// Adds the event to the queue.
        /// </summary>
        /// <param name="newEvent">New event.</param>
        /// <exception cref="InvalidOperationException">When Listener is already stopped</exception>
        public virtual void AddEvent(ISyncEvent newEvent) {
            if (this.alreadyDisposed) {
                throw new ObjectDisposedException("SyncEventQueue", "Called AddEvent on Disposed object");
            }

            Logger.Debug(string.Format("Adding Event: {0}", newEvent.ToString()));
            this.queue.Add(newEvent);
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

        protected virtual void Dispose(bool isDisposing) {
            if (this.alreadyDisposed) {
                return;
            }

            if (!this.IsStopped) {
                Logger.Warn("Disposing a not yet stopped SyncEventQueue");
            }

            if(isDisposing) {
                this.queue.Dispose();
            }

            this.alreadyDisposed = true;
        }

        private void Listen(BlockingCollection<ISyncEvent> queue, ISyncEventManager manager, WaitHandle waitHandle) {
            Logger.Debug("Starting to listen on SyncEventQueue");
            while (!queue.IsCompleted)
            {
                if (this.suspend)
                {
                    waitHandle.WaitOne();
                }

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
                        manager.Handle(syncEvent);
                    } catch(Exception e) {
                        Logger.Error("Exception in EventHandler");
                        Logger.Error("Event was: " + syncEvent);
                        Logger.Error(e);
                        Logger.Error(e.StackTrace);
                    }
                }
            }

            Logger.Debug("Stopping to listen on SyncEventQueue");
        }
    }
}
