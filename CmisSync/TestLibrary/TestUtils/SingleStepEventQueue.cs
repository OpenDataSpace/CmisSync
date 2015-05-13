//-----------------------------------------------------------------------
// <copyright file="SingleStepEventQueue.cs" company="GRAU DATA AG">
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

namespace TestLibrary {
    using System;
    using System.Collections.Concurrent;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Queueing;

    /// <summary>
    /// This is a synchronous test-replacement for SyncEventQueue
    /// </summary>
    /// Do not use this in production code.
    /// It contains public fields that could do a lot of harm
    public class SingleStepEventQueue : ICountingQueue {
        private IEventCounter fullCounter;
        private IEventCounter categoryCounter;
        private SyncEventHandler dropAllFsEventsHandler;
        private bool isDroppingAllFsEvents = false;
        public ISyncEventManager Manager;
        public ConcurrentQueue<ISyncEvent> Queue = new ConcurrentQueue<ISyncEvent>();

        public SingleStepEventQueue(
            ISyncEventManager manager,
            IEventCounter fullCounter = null,
            IEventCounter categoryCounter = null)
        {
            this.Manager = manager;
            this.fullCounter = fullCounter ?? new QueuedEventsCounter();
            this.categoryCounter = categoryCounter ?? new QueuedCategorizedEventsCounter();
            this.dropAllFsEventsHandler = new GenericSyncEventHandler<IFSEvent>(
                int.MaxValue,
                delegate(ISyncEvent e) {
                return true;
            });
        }

        public ISyncEventManager EventManager {
            get { return this.Manager; }
        }

        public bool IsStopped {
            get {
                return this.Queue.Count == 0;
            }
        }

        public bool IsEmpty {
            get {
                return this.Queue.Count == 0;
            }
        }

        public bool SwallowExceptions { get; set; }

        public bool DropAllLocalFileSystemEvents {
            get {
                return this.isDroppingAllFsEvents;
            }

            set {
                if (value != this.isDroppingAllFsEvents) {
                    this.isDroppingAllFsEvents = value;
                    if (this.isDroppingAllFsEvents) {
                        this.Manager.AddEventHandler(this.dropAllFsEventsHandler);
                    } else {
                        this.Manager.RemoveEventHandler(this.dropAllFsEventsHandler);
                    }
                }
            }
        }

        public void AddEvent(ISyncEvent e) {
            if (e is ICountableEvent && (e as ICountableEvent).Category != EventCategory.NoCategory) {
                this.fullCounter.Increase(e as ICountableEvent);
                this.categoryCounter.Increase(e as ICountableEvent);
            }

            this.Queue.Enqueue(e);
        }

        public void Step() {
            ISyncEvent e;
            if (this.Queue.TryDequeue(out e)) {
                try {
                    this.Manager.Handle(e);
                } catch (System.IO.InvalidDataException) {
                    throw;
                } catch (Exception exp) {
                    if (!this.SwallowExceptions) {
                        throw;
                    } else {
                        Console.WriteLine(exp.ToString());
                    }
                }

                if (e is ICountableEvent && (e as ICountableEvent).Category != EventCategory.NoCategory) {
                    this.fullCounter.Decrease(e as ICountableEvent);
                    this.categoryCounter.Decrease(e as ICountableEvent);
                }
            }
        }

        public void Run() {
            while (!this.IsStopped) {
                this.Step();
            }
        }

        public void RunStartSyncEvent() {
            var startSyncEvent = new StartNextSyncEvent(false);
            this.AddEvent(startSyncEvent);
            this.Run();
        }

        public void Dispose() {
            if (this.categoryCounter != null) {
                this.categoryCounter.Dispose();
            }

            if (this.fullCounter != null) {
                this.fullCounter.Dispose();
            }
        }

        public bool WaitForStopped(int timeout) {
            return true;
        }

        public void StopListener() {
        }

        public void Suspend() {
        }

        public void Continue() {
        }

        public IDisposable Subscribe(IObserver<int> observer) {
            return null;
        }

        public IDisposable Subscribe(IObserver<Tuple<EventCategory, int>> observer) {
            return null;
        }

        public IObservable<int> FullCounter {
            get {
                return (IObservable<int>)this.fullCounter;
            }
        }

        public IObservable<Tuple<EventCategory, int>> CategoryCounter {
            get {
                return (IObservable<Tuple<EventCategory, int>>)this.categoryCounter;
            }
        }
    }
}