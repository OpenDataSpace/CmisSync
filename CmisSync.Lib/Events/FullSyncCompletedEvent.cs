//-----------------------------------------------------------------------
// <copyright file="FullSyncCompletedEvent.cs" company="GRAU DATA AG">
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

    /// <summary>
    /// Full sync completed event.
    /// </summary>
    public class FullSyncCompletedEvent : EncapsuledEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Events.FullSyncCompletedEvent"/> class.
        /// </summary>
        /// <param name="startEvent">Start event, which is completed.</param>
        public FullSyncCompletedEvent(StartNextSyncEvent startEvent) : base(startEvent)
        {
        }

        /// <summary>
        /// Gets the start event, which has been completed
        /// </summary>
        public StartNextSyncEvent StartEvent {
            get { return this.Event as StartNextSyncEvent; }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="CmisSync.Lib.Events.FullSyncCompletedEvent"/>.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents the current <see cref="CmisSync.Lib.Events.FullSyncCompletedEvent"/>.</returns>
        public override string ToString() {
            return "FullSyncCompletedEvent: " + base.ToString();
        }
    }
}