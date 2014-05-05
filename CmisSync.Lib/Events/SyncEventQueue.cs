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
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

using log4net;

namespace CmisSync.Lib.Events
{
    public class SyncEventQueue : IDisposableSyncEventQueue {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(SyncEventQueue));

        private BlockingCollection<ISyncEvent> queue = new BlockingCollection<ISyncEvent>();

        private SyncEventManager manager;

        private Task consumer;

        private bool alreadyDisposed = false;
        
        private static void Listen(BlockingCollection<ISyncEvent> queue, SyncEventManager manager){
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
                try
                {
                    syncEvent = queue.Take();
                }
                catch (InvalidOperationException) { }

                if (syncEvent != null)
                {
                    try{
                        manager.Handle(syncEvent);
                    }catch(Exception e) {
                        Logger.Error("Exception in EventHandler");
                        Logger.Error(e);
                        Logger.Error(e.StackTrace);
                    }
                }
            }
            Logger.Debug("Stopping to listen on SyncEventQueue");
        }

        /// <exception cref="InvalidOperationException">When Listener is already stopped</exception>
        public virtual void AddEvent(ISyncEvent newEvent) {
            if(alreadyDisposed) {
                throw new ObjectDisposedException("SyncEventQueue", "Called AddEvent on Disposed object");
            }
            this.queue.Add(newEvent);
        }

        public SyncEventQueue(SyncEventManager manager) {
            if(manager == null) {
                throw new ArgumentException("manager may not be null");
            }
            this.manager = manager;
            this.consumer = new Task(() => Listen(this.queue, this.manager));
            this.consumer.Start();
        }

        public void StopListener() {
            if(alreadyDisposed) {
                return;
            }
            this.queue.CompleteAdding();
        }
        
        public bool IsStopped {
            get {
                return this.consumer.IsCompleted; 
            }
        }

        public void WaitForStopped() {
            this.consumer.Wait();
        }

        public bool WaitForStopped(TimeSpan timeout) {
            return this.consumer.Wait(timeout);
        }

        public bool WaitForStopped(int milisecondsTimeout) {
            return this.consumer.Wait(milisecondsTimeout);
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposing) {
            if(alreadyDisposed) {
                return;
            }
            if(!IsStopped){
                Logger.Warn("Disposing a not yet stopped SyncEventQueue");
            }
            if(isDisposing) {
                this.queue.Dispose();
            }
            this.alreadyDisposed = true;
        }
    }
}
