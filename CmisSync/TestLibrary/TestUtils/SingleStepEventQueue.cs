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

namespace TestLibrary
{
    using System.Collections.Concurrent;

    using CmisSync.Lib.Events;

    /// <summary>
    /// This is a synchronous test-replacement for SyncEventQueue
    /// </summary>
    /// Do not use this in production code.
    /// It contains public fields that could do a lot of harm
    public class SingleStepEventQueue : IDisposableSyncEventQueue
    {
        public ISyncEventManager Manager;
        public ConcurrentQueue<ISyncEvent> Queue = new ConcurrentQueue<ISyncEvent>();

        public SingleStepEventQueue(ISyncEventManager manager) {
            this.Manager = manager;
        }

        public ISyncEventManager EventManager {
            get { return this.Manager; }
        }

        public bool IsStopped {
            get {
                return this.Queue.Count == 0;
            }
        }

        public void AddEvent(ISyncEvent e) {
            this.Queue.Enqueue(e);
        }

        public void Step() {

            ISyncEvent e;
            if(this.Queue.TryDequeue(out e)){
                this.Manager.Handle(e);
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
        }

        public bool WaitForStopped(int timeout) {
            return true;
        }

        public void StopListener() {
        }
    }
}
