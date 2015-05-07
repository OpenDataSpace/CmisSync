//-----------------------------------------------------------------------
// <copyright file="SyncStatus.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Cmis {
    using System;

    /// <summary>
    /// Current status of the synchronization.
    /// </summary>
    public enum SyncStatus {
        /// <summary>
        /// Normal operation.
        /// </summary>
        Idle,

        /// <summary>
        /// Synchronization is suspended.
        /// </summary>
        Suspend,

        /// <summary>
        /// Connection is not established.
        /// </summary>
        Disconnected,

        /// <summary>
        /// Actually changes are synchronized.
        /// </summary>
        Synchronizing,

        /// <summary>
        /// Any sync conflict or warning happend
        /// </summary>
        Warning,

        /// <summary>
        /// The complete connection is deactivated.
        /// </summary>
        Deactivated
    }
}