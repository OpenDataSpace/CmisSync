//-----------------------------------------------------------------------
// <copyright file="SyncScheduler.cs" company="GRAU DATA AG">
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
    using System.Timers;

    /// <summary>
    /// Sync scheduler. Inserts every pollInterval a new StartNextSyncEvent into the Queue
    /// </summary>
    public class SyncScheduler : SyncEventHandler, IDisposable
    {
        /// <summary>
        /// The default Queue Event Handler PRIORITY.
        /// </summary>
        private double interval;
        private ISyncEventQueue queue;
        private Timer timer;

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Events.SyncScheduler"/> class.
        /// Starts adding events automatically after successful creation.
        /// </summary>
        /// <param name="queue">Sync event queue.</param>
        /// <param name="pollInterval">Poll interval.</param>
        public SyncScheduler(ISyncEventQueue queue, double pollInterval = 5000)
        {
            if (queue == null) {
                throw new ArgumentNullException("Given queue must not be null");
            }

            if (pollInterval <= 0) {
                throw new ArgumentException("pollinterval must be greater than zero");
            }

            this.interval = pollInterval;
            this.queue = queue;
            this.timer = new Timer(this.interval);
            this.timer.Elapsed += delegate(object sender, ElapsedEventArgs e) {
                this.queue.AddEvent(new StartNextSyncEvent());
            };
        }

        /// <summary>
        /// Starts adding events into the Queue, if it has been stopped before.
        /// </summary>
        public void Start() {
            this.timer.Start();
        }

        /// <summary>
        /// Stops adding event into the Queue
        /// </summary>
        public void Stop() {
            this.timer.Stop();
        }

        /// <summary>
        /// Handles Config changes if the poll interval has been changed.
        /// Resets also the timer if a full sync event has been recognized.
        /// </summary>
        /// <param name="e">Sync event.</param>
        /// <returns><c>false</c> on every event.</returns>
        public override bool Handle(ISyncEvent e)
        {
            RepoConfigChangedEvent config = e as RepoConfigChangedEvent;
            if (config != null) {
                double newInterval = config.RepoInfo.PollInterval;
                if (newInterval > 0 && this.interval != newInterval) {
                    this.interval = newInterval;
                    this.Stop();
                    this.timer.Interval = this.interval;
                    this.Start();
                }

                return false;
            }

            StartNextSyncEvent start = e as StartNextSyncEvent;
            if(start != null && start.FullSyncRequested) {
                this.Stop();
                this.Start();
            }

            return false;
        }

        /// <summary>
        /// Releases all resource used by the <see cref="CmisSync.Lib.Events.SyncScheduler"/> object.
        /// </summary>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="CmisSync.Lib.Events.SyncScheduler"/>.
        /// The <see cref="Dispose"/> method leaves the <see cref="CmisSync.Lib.Events.SyncScheduler"/> in an unusable
        /// state. After calling <see cref="Dispose"/>, you must release all references to the
        /// <see cref="CmisSync.Lib.Events.SyncScheduler"/> so the garbage collector can reclaim the memory that the
        /// <see cref="CmisSync.Lib.Events.SyncScheduler"/> was occupying.</remarks>
        public void Dispose() {
            this.timer.Stop();
            this.timer.Dispose();
        }
    }
}