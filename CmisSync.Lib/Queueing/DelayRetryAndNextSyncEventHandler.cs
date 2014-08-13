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
        private bool syncHasBeenDelayed = false;
        private bool lastDelayedSyncWasFullSync = false;
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
            bool isEventDelayed = false;

            if(e is AbstractFolderEvent) {
                isEventDelayed = this.DelayEventIfRetryCountPositive(e as AbstractFolderEvent);
            }

            if(e is StartNextSyncEvent) {
                if(this.SyncHasToBeDelayed()) {
                    this.DelayNextSyncEvent(e as StartNextSyncEvent);
                    isEventDelayed = true;
                }
            }

            this.FireDelayedEventsIfQueueIsEmpty();

            return isEventDelayed;
        }

        private bool DelayEventIfRetryCountPositive(AbstractFolderEvent fileOrFolderEvent) {
            if(fileOrFolderEvent.RetryCount > 0) {
                this.retryEvents.Add(fileOrFolderEvent);
                return true;
            }

            return false;
        }

        private void DelayNextSyncEvent(StartNextSyncEvent startNextSyncEvent) {
            this.syncHasBeenDelayed = true;
            if(!this.lastDelayedSyncWasFullSync && startNextSyncEvent.FullSyncRequested == true) {
                this.lastDelayedSyncWasFullSync = true;
            }
        }

        private void FireDelayedEventsIfQueueIsEmpty() {
            if(this.Queue.IsEmpty && this.syncHasBeenDelayed) {
                if(this.lastDelayedSyncWasFullSync) {
                    this.retryEvents.Clear();
                }

                foreach(var storedRetryEvent in this.retryEvents) {
                    Queue.AddEvent(storedRetryEvent);
                }

                this.Queue.AddEvent(new StartNextSyncEvent(this.lastDelayedSyncWasFullSync));
                this.syncHasBeenDelayed = false;
            }
        }

        private bool SyncHasToBeDelayed() {
            // delay if queue not empty or if already stored events have to be fired.
            return !this.Queue.IsEmpty || this.syncHasBeenDelayed || this.retryEvents.Count > 0;
        }
    }
}
