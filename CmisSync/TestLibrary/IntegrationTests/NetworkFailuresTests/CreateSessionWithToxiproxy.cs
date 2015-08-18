
namespace TestLibrary.IntegrationTests.NetworkFailuresTests {
    using System;
    using System.Net;

    using NUnit.Framework;

    using TestUtils;
    using TestUtils.ToxiproxyUtils;

    using Toxiproxy.Net;

    [TestFixture, Category("Slow")]
    public class CreateSessionWithToxiproxy : IsFullTestWithToxyProxy {
        [Test]
        public void Connect() {
            this.InitializeAndRunRepo(swallowExceptions: true);

            int counter = 0;
            this.AuthProviderWrapper.OnAuthenticate += (object obj) => {
                counter++;
                if (counter >= 3 && this.Proxy.Enabled) {
                    this.Proxy.Enabled = false;
                    this.Proxy.Update();
                } else if (counter > 3 + 5) {
                    this.Proxy.Enabled = true;
                    this.Proxy.Update();
                    counter = 0;
                }
            };

            for (int i = 0; i < 10; i++ ) {
                this.AddStartNextSyncEvent();
                this.repo.Run();
            }
        }
    }
}