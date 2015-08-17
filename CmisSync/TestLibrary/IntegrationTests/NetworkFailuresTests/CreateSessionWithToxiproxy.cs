
namespace TestLibrary.IntegrationTests.NetworkFailuresTests {
    using System;
    using System.Net;

    using NUnit.Framework;

    using TestUtils;
    using TestUtils.ToxiproxyUtils;

    using Toxiproxy.Net;

    [TestFixture, Category("Slow")]
    public class CreateSessionWithToxiproxy : BaseFullRepoTest, IsToxiProxyTest {
        public string ToxiProxyServerName { get; private set; }
        public int? ToxiProxyServerManagementPort { get; private set; }
        public int ToxiProxyListeningPort = 8080;
        public string RemoteUrl { get; private set; }
        public Proxy Proxy { get; private set; }
        private Connection connection;
        public ToxiproxyAuthenticationProviderWrapper AuthProviderWrapper { get; set; }

        [SetUp]
        public void InitProxy() {
            this.RemoteUrl = this.repoInfo.Address.ToString();
            this.connection = this.EnsureThatToxiProxyIsAvailable();
            var client = connection.Client();
            client.RemoveAllProxies();
            this.Proxy = client.CreateAndAddProxy(basedOn: this);
            this.AuthProviderWrapper = new ToxiproxyAuthenticationProviderWrapper(this.session.Binding.GetAuthenticationProvider());
            this.repo.AuthProvider = this.AuthProviderWrapper;
            this.repo.SessionFactory = new ToxiSessionFactory(this.SessionFactory) {
                Host = this.ToxiProxyServerName ?? "127.0.0.1",
                Port = this.ToxiProxyListeningPort
            };
        }

        [TearDown]
        public void ShutDownProxyConnection() {
            this.Proxy = null;
            if (this.connection != null) {
                this.connection.Dispose();
                this.connection = null;
            }
        }

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