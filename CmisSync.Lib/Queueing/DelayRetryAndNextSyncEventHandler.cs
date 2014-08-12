//-----------------------------------------------------------------------
// <copyright file="DelayRetryAndNextSyncEventHandler.cs" company="GRAU DATA AG">
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
namespace CmisSync.Lib.Queueing
{
    using System.Collections.Generic;

    using CmisSync.Lib.Events;

    /// <summary>
    /// Delay StartNextSyncEvents and AbstractFolderEvents with RetryCounter > 0.
    /// Events are only retried if no full sync is requested as they are obsolete then.
    /// </summary>
    public class DelayRetryAndNextSyncEventHandler : ReportingSyncEventHandler
    {
        private bool triggerSyncWhenQueueEmpty = false;
        private bool triggerFullSync = false;
        private List<AbstractFolderEvent> retryEvents = new List<AbstractFolderEvent>();

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Queueing.DelayRetryAndNextSyncEventHandler"/> class.
        /// </summary>
        /// <param name='queue'>
        /// The SyncEventQueue.
        /// </param>
        public DelayRetryAndNextSyncEventHandler(ISyncEventQueue queue) : base(queue)
        {
        }

        /// <summary>
        /// Handle the specified e if AbstractFolderEvent or StartNextSyncEvent.
        /// </summary>
        /// <param name='e'>
        /// The ISyncEvent.
        /// </param>
        /// <returns>
        /// True if handled
        /// </returns>
        public override bool Handle(ISyncEvent e) {
            bool hasBeenHandled = false;

            var startNextSyncEvent = e as StartNextSyncEvent;
            if(startNextSyncEvent != null) {
                this.triggerSyncWhenQueueEmpty = true;
                this.triggerFullSync = startNextSyncEvent.FullSyncRequested;
                hasBeenHandled = true;
            }

            var fileOrFolderEvent = e as AbstractFolderEvent;
            if(fileOrFolderEvent != null && fileOrFolderEvent.RetryCount > 0) {
                this.retryEvents.Add(fileOrFolderEvent);
                hasBeenHandled = true;
            }

            if(this.Queue.IsEmpty && this.triggerSyncWhenQueueEmpty) {
                if(this.triggerFullSync) {
                    this.retryEvents.Clear();
                }

                foreach(var storedRetryEvent in this.retryEvents) {
                    Queue.AddEvent(storedRetryEvent);
                }

                this.Queue.AddEvent(new StartNextSyncEvent(this.triggerFullSync));
                this.triggerSyncWhenQueueEmpty = false;
            }

            return hasBeenHandled;
        }
    }
}
