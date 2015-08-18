
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
        public void ConnectToRepoAndSimulateConnectionProblems() {
            this.InitializeAndRunRepo(swallowExceptions: false);

            this.AuthProviderWrapper.OnAuthenticate += (object obj) => {
                this.SwitchProxyState();
            };

            for (int i = 0; i < 10; i++ ) {
                this.AddStartNextSyncEvent(forceCrawl: i % 2 == 0);
                this.repo.Run();
            }
        }
    }
}