
namespace TestLibrary.IntegrationTests.NetworkFailuresTests {
    using System;

    using NUnit.Framework;

    using TestUtils;
    using TestUtils.ToxiproxyUtils;

    using Toxiproxy.Net;

    [TestFixture, Category("Slow")]
    public class CreateSessionWithToxiproxy : BaseFullRepoTest {
        private readonly string ProxyHostName = "127.0.0.1";
        private readonly int ProxyPort = 8080;
        public CreateSessionWithToxiproxy() {
            this.SessionFactory = new ToxiSessionFactory(DotCMIS.Client.Impl.SessionFactory.NewInstance()) {
                Host = this.ProxyHostName,
                Port = this.ProxyPort
            };
        }

        [Test]
        public void Connect() {
            var remoteHostName = new UriBuilder(this.repoInfo.Address.ToString()).Host;
            var remoteHostPort = new UriBuilder(this.repoInfo.Address.ToString()).Port;
            using (var conn = new Connection(ProxyHostName, resetAllToxicsAndProxiesOnClose: true)) {
                var client = conn.Client();
                foreach (var proxyName in client.All().Keys) {
                    client.Delete(proxyName);
                }

                client.Add(new Proxy() {
                    Enabled = true,
                    Name = string.Format("local_to_cmis_{0}", Guid.NewGuid().ToString()),
                    Upstream = string.Format("{0}:{1}", remoteHostName, remoteHostPort),
                    Listen = string.Format("{0}:{1}", ProxyHostName, ProxyPort)
                });
            }
        }
    }
}