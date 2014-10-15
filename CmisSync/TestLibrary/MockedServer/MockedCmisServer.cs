

namespace TestLibrary.MockedServer
{
    using System;
    using System.Collections.Generic;

    public class MockedCmisServer
    {
        private static Dictionary<string, MockedCmisServer> servers = new Dictionary<string, MockedCmisServer>();

        public static MockedCmisServer GetServer(string hostname) {
            lock(servers) {
                var server = servers[hostname];
                if (server == null) {
                    server = new MockedCmisServer(hostname);
                    servers.Add(hostname, server);
                }

                return server;
            }
        }

        private MockedCmisServer(string hostname) {
            this.Hostname = hostname;
        }

        public void Destroy() {
            lock(servers) {
                servers.Remove(this.Hostname);
            }
        }

        public string Hostname { get; private set; }
    }
}