//-----------------------------------------------------------------------
// <copyright file="ConnectionStatus.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Status {
    using System;

    public class ConnectionStatus {
        private bool connected = false;
        public bool IsConnected {
            get {
                return this.connected;
            }

            set {
                if (value != this.connected) {
                    this.connected = value;
                    if (this.connected) {
                        this.ConnectedSince = DateTime.Now;
                        this.DisconnectedSince = null;
                    } else {
                        this.ConnectedSince = null;
                        this.DisconnectedSince = DateTime.Now;
                    }
                }
            }
        }

        public DateTime? ConnectedSince { get; private set; }

        public DateTime? DisconnectedSince { get; private set; }
    }
}