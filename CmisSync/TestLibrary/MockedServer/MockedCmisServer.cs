//-----------------------------------------------------------------------
// <copyright file="MockedCmisServer.cs" company="GRAU DATA AG">
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

namespace TestLibrary.MockedServer
{
    using System;
    using System.Collections.Generic;

    public class MockedCmisServer
    {
        private static Dictionary<string, MockedCmisServer> servers = new Dictionary<string, MockedCmisServer>();

        private MockedCmisServer(string hostname) {
            this.Hostname = hostname;
        }

        public string Hostname { get; private set; }

        public static MockedCmisServer GetServer(string hostname) {
            lock (servers) {
                var server = servers[hostname];
                if (server == null) {
                    server = new MockedCmisServer(hostname);
                    servers.Add(hostname, server);
                }

                return server;
            }
        }

        public void Destroy() {
            lock (servers) {
                servers.Remove(this.Hostname);
            }
        }
    }
}