
namespace TestLibrary.IntegrationTests.NetworkFailuresTests {
    using System;
    using System.Linq;

    using Moq;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    using Toxiproxy.Net;

    [TestFixture, Category("FailureIT")]
    public class ToxyProxySetup {
        [Test]
        public void SetupConnectionToProxy() {
            using (var conn = new Connection()) {
                var client = conn.Client();
                foreach (var proxyName in client.All().Keys) {
                    client.Delete(proxyName);
                }

                var proxy = client.Add(new Proxy() {
                    Enabled = true,
                    Name = "local_to_devel",
                    Upstream = "devel.dataspace.cc:80",
                    Listen = "127.0.0.1:8080"
                });
                var bandwidth = client.FindProxy(proxy.Name).UpStreams().BandwidthToxic;
                bandwidth.Enabled = true;
                bandwidth.Rate = 1000;
                bandwidth.Update();
                bandwidth = client.FindProxy(proxy.Name).DownStreams().BandwidthToxic;
                bandwidth.Enabled = true;
                bandwidth.Rate = 1000;
                bandwidth.Update();
            }
        }
    }
}