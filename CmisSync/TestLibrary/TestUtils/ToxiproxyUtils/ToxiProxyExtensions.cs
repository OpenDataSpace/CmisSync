
namespace TestLibrary.TestUtils.ToxiproxyUtils {
    using System;

    using NUnit.Framework;

    using TestLibrary.IntegrationTests;

    using Toxiproxy.Net;

    public static class ToxiProxyExtensions {
        public static Connection EnsureThatToxiProxyIsAvailable(this IsToxiProxyTest test) {
            try {
                string proxyName = test.ToxiProxyServerName ?? "127.0.0.1";
                int proxyPort = test.ToxiProxyServerManagementPort ?? 8474;
                return new Connection(proxyName, proxyPort);
            } catch (Exception e) {
                Assert.Ignore(string.Format("Connection to ToxiProxy failed: {0}", e.Message));
                return null;
            }
        }

        public static void RemoveAllProxies(this Connection connection) {
            connection.Client().RemoveAllProxies();
        }

        public static void RemoveAllProxies(this Client client) {
            foreach (var proxyName in client.All().Keys) {
                client.Delete(proxyName);
            }
        }

        public static Proxy CreateAndAddProxy(this Client client, IsToxiProxyTest basedOn, int ListeningOnPort = 8080) {
            var url = basedOn.RemoteUrl.ToString();
            var remoteHostName = new UriBuilder(url).Host;
            var remoteHostPort = new UriBuilder(url).Port;
            return client.Add(new Proxy() {
                Enabled = true,
                Name = string.Format("local_to_cmis_{0}", Guid.NewGuid().ToString()),
                Upstream = string.Format("{0}:{1}", remoteHostName, remoteHostPort),
                Listen = string.Format("{0}:{1}", basedOn.ToxiProxyServerName, ListeningOnPort)
            });
        }
    }
}