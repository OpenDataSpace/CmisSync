//-----------------------------------------------------------------------
// <copyright file="StartNextSyncEvent.cs" company="GRAU DATA AG">
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
    using System.Collections.Generic;

    /// <summary>
    /// This event should be used by scheduler to periodically start sync processes.
    /// If any inconsitancy is detected, it could also be used by the algorithm itself to force a full sync on the next sync execution.
    /// </summary>
    public class StartNextSyncEvent : ISyncEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Events.StartNextSyncEvent"/> class.
        /// </summary>
        /// <param name="fullSyncRequested">If set to <c>true</c> a full sync is requested.</param>
        public StartNextSyncEvent(bool fullSyncRequested = false)
        {
            this.FullSyncRequested = fullSyncRequested;
        }

        /// <summary>
        /// Gets or sets the last content change token on server.
        /// </summary>
        /// <value>The last token on server.</value>
        public string LastTokenOnServer { get; set; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="CmisSync.Lib.Events.StartNextSyncEvent"/> should force a full sync.
        /// </summary>
        /// <value>
        /// <c>true</c> if full sync requested; otherwise, <c>false</c>.
        /// </value>
        public bool FullSyncRequested { get; private set; }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="CmisSync.Lib.Events.StartNextSyncEvent"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents the current <see cref="CmisSync.Lib.Events.StartNextSyncEvent"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format("[StartNextSyncEvent: FullSyncRequested={0}]", this.FullSyncRequested);
        }
    }
}