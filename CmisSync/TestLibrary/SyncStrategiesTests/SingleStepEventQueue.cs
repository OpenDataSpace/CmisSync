using System.Collections.Generic;

using CmisSync.Lib.Events;

namespace TestLibrary
{   
    /// <summary>
    /// This is a synchronous test-replacement for SyncEventQueue
    /// </summary>
    /// Do not use this in production code. 
    /// It contains public fields that could do a lot of harm 
    public class SingleStepEventQueue : ISyncEventQueue {
        public SyncEventManager manager; 
        public Queue<ISyncEvent> queue = new Queue<ISyncEvent>();

        public SingleStepEventQueue(SyncEventManager manager) {
            this.manager = manager;
        }

        public void AddEvent(ISyncEvent e) {
            queue.Enqueue(e);
        }

        public bool IsStopped {
            get {
                return queue.Count == 0; 
            }
        }

        public void Step() {
            ISyncEvent e = queue.Dequeue();
            manager.Handle(e);
        }

        public void Run() {
            while(!IsStopped) {
                Step();
            }
        }
    }
}
