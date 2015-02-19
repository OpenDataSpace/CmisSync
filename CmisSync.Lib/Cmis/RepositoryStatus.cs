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
    public class RepositoryStatus {
        public RepositoryStatus() {
        }
        private bool connected;
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

        public bool Paused { get; set; }
        public bool Warning { get; set; }
        public bool SyncRequested { get; set; }
        public int KnownChanges { get; set; }

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