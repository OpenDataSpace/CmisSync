using System;

namespace CmisSync.Lib.Events
{
    public class FullSyncCompletedEvent : EncapsuledEvent
    {
        public FullSyncCompletedEvent (StartNextSyncEvent startEvent) : base (startEvent)
        { }

        /// <summary>
        /// Completed sync requested event
        /// </summary>
        public StartNextSyncEvent StartEvent { get { return this.Event as StartNextSyncEvent; } }
    }
}

