//-----------------------------------------------------------------------
// <copyright file="RequestIgnoredEvent.cs" company="GRAU DATA AG">
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

    using CmisSync.Lib.Queueing;

    /// <summary>
    /// Request ignored event.
    /// </summary>
    public class RequestIgnoredEvent : ISyncEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Events.RequestIgnoredEvent"/> class.
        /// </summary>
        /// <param name="ignoredEvent">Ignored event.</param>
        /// <param name="reason">Reason why it has been ignored.</param>
        /// <param name="source">The source which ignored the event.</param>
        public RequestIgnoredEvent(ISyncEvent ignoredEvent, string reason = null, SyncEventHandler source = null)
        {
            if (ignoredEvent == null) {
                throw new ArgumentNullException("The ignored event cannot be null");
            }

            if (reason == null && source == null) {
                throw new ArgumentNullException("There must be a reason or source given for the ignored event");
            }

            this.IgnoredEvent = ignoredEvent;
            this.Reason = (reason != null) ? reason : "Event has been ignored by: " + source.ToString();
        }

        /// <summary>
        /// Gets the ignored event.
        /// </summary>
        /// <value>The ignored event.</value>
        public ISyncEvent IgnoredEvent { get; private set; }

        /// <summary>
        /// Gets the reason why the event has been ignored.
        /// </summary>
        /// <value>The reason.</value>
        public string Reason { get; private set; }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="CmisSync.Lib.Events.RequestIgnoredEvent"/>.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents the current <see cref="CmisSync.Lib.Events.RequestIgnoredEvent"/>.</returns>
        public override string ToString()
        {
            return string.Format("[RequestIgnoredEvent: IgnoredEvent={0} Reason={1}]", this.IgnoredEvent, this.Reason);
        }
    }
}