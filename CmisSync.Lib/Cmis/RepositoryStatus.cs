//-----------------------------------------------------------------------
// <copyright file="RepositoryStatus.cs" company="GRAU DATA AG">
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
    /// Repository status.
    /// </summary>
    public class RepositoryStatus {
        private bool connected;

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Cmis.RepositoryStatus"/> class.
        /// </summary>
        public RepositoryStatus() {
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="CmisSync.Lib.Cmis.RepositoryStatus"/> is connected.
        /// </summary>
        /// <value><c>true</c> if connected; otherwise, <c>false</c>.</value>
        public bool Connected {
            get {
                return this.connected;
            }

            set {
                if (value) {
                    this.Warning = false;
                }

                this.connected = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="CmisSync.Lib.Cmis.RepositoryStatus"/> is paused.
        /// </summary>
        /// <value><c>true</c> if paused; otherwise, <c>false</c>.</value>
        public bool Paused { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="CmisSync.Lib.Cmis.RepositoryStatus"/> is in warning state.
        /// </summary>
        /// <value><c>true</c> if warning; otherwise, <c>false</c>.</value>
        public bool Warning { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="CmisSync.Lib.Cmis.RepositoryStatus"/> is in sync requested state.
        /// </summary>
        /// <value><c>true</c> if sync requested; otherwise, <c>false</c>.</value>
        public bool SyncRequested { get; set; }

        /// <summary>
        /// Gets or sets the known changes.
        /// </summary>
        /// <value>The known changes.</value>
        public int KnownChanges { get; set; }

        /// <summary>
        /// Gets the status resulting on the other properties.
        /// </summary>
        /// <value>The status.</value>
        public SyncStatus Status {
            get {
                if (this.Paused) {
                    return SyncStatus.Suspend;
                }

                if (this.Warning) {
                    return SyncStatus.Warning;
                }

                if (!this.Connected) {
                    return SyncStatus.Disconnected;
                }

                if (this.SyncRequested || this.KnownChanges > 0) {
                    return SyncStatus.Synchronizing;
                } else {
                    return SyncStatus.Idle;
                }
            }
        }
    }
}