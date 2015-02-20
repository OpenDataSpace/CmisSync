//-----------------------------------------------------------------------
// <copyright file="EventCategory.cs" company="GRAU DATA AG">
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

    /// <summary>
    /// Available event category for countable events.
    /// </summary>
    public enum EventCategory {
        /// <summary>
        /// Ignore this event and do not count this.
        /// </summary>
        NoCategory,

        /// <summary>
        /// A change between local fs and remote server is detected.
        /// </summary>
        DetectedChange,

        /// <summary>
        /// The full synchronization is requested.
        /// </summary>
        SyncRequested,

        /// <summary>
        /// The periodic synchronization is requested.
        /// </summary>
        PeriodicSync,

        /// <summary>
        /// Any connection exception occured.
        /// </summary>
        ConnectionException
    }
}