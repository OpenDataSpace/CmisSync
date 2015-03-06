//-----------------------------------------------------------------------
// <copyright file="CmisConnectionExceptionEvent.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Events {
    using System;

    using DotCMIS.Exceptions;

    /// <summary>
    /// Cmis connection exception occured while handling event in SyncEventQueue and this event is added back to Queue to inform about this problem.
    /// </summary>
    public class CmisConnectionExceptionEvent : ExceptionEvent, ICountableEvent {
        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Events.CmisConnectionExceptionEvent"/> class.
        /// The OccuredAt property will be set automatically to the actual time when the constructor is called.
        /// </summary>
        /// <param name="connectionException">Connection exception.</param>
        public CmisConnectionExceptionEvent(CmisConnectionException connectionException) : base(connectionException) {
            this.OccuredAt = DateTime.Now;
        }

        /// <summary>
        /// Gets the time when the exception occured.
        /// </summary>
        /// <value>The occured at this timestamp.</value>
        public DateTime OccuredAt { get; private set; }

        /// <summary>
        /// Gets the category of the event. This can be used to differ between multiple event types.
        /// The returned value should never ever change its value after requesting it the first time.
        /// </summary>
        /// <value>The event category.</value>
        public EventCategory Category {
            get {
                return EventCategory.ConnectionException;
            }
        }
    }
}