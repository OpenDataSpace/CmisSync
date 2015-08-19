//-----------------------------------------------------------------------
// <copyright file="IsFullTestWithToxyProxy.cs" company="GRAU DATA AG">
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
﻿
namespace TestLibrary.IntegrationTests.NetworkFailuresTests {
    using System;

    using NUnit.Framework;

    using Toxiproxy.Net;

    using TestLibrary.TestUtils.ToxiproxyUtils;

    [TestFixture, Category("Slow"), Category("ToxiProxy")]
    public abstract class IsFullTestWithToxyProxy : BaseFullRepoTest {
        public string ToxiProxyServerName { get; private set; }
        public int? ToxiProxyServerManagementPort { get; private set; }
        public int ToxiProxyListeningPort = 8080;
        public string RemoteUrl { get; private set; }
        public Proxy Proxy { get; private set; }
        private Connection connection;
        public ToxiproxyAuthenticationProviderWrapper AuthProviderWrapper { get; set; }
        protected bool RetryOnInitialConnection {
            get {
                return this.repo.UseRetryingConnectionScheduler;
            } set {
                this.repo.UseRetryingConnectionScheduler = value;
            }
        }

        [SetUp]
        public void InitProxy() {
            this.RemoteUrl = this.repoInfo.Address.ToString();
            this.connection = this.EnsureThatToxiProxyIsAvailable();
            this.RemoveAllProxies();
            this.Proxy = this.CreateAndAddProxy();
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

        private Connection EnsureThatToxiProxyIsAvailable() {
            try {
                string proxyName = this.ToxiProxyServerName ?? "127.0.0.1";
                int proxyPort = this.ToxiProxyServerManagementPort ?? 8474;
                var connection = new Connection(proxyName, proxyPort);
                connection.Client().All();
                return connection;
            } catch (Exception e) {
                Assert.Ignore(string.Format("Connection to ToxiProxy failed: {0}", e.Message));
                return null;
            }
        }

        private void RemoveAllProxies() {
            var client = this.connection.Client();
            foreach (var proxyName in client.All().Keys) {
                client.Delete(proxyName);
            }
        }

        private Proxy CreateAndAddProxy() {
            var client = this.connection.Client();
            var url = this.RemoteUrl.ToString();
            var remoteHostName = new UriBuilder(url).Host;
            var remoteHostPort = new UriBuilder(url).Port;
            return client.Add(new Proxy() {
                Enabled = true,
                Name = string.Format("local_to_cmis_{0}", Guid.NewGuid().ToString()),
                Upstream = string.Format("{0}:{1}", remoteHostName, remoteHostPort),
                Listen = string.Format("{0}:{1}", this.ToxiProxyServerName, this.ToxiProxyListeningPort)
            });
        }

        public void SwitchProxyState() {
            this.Proxy.Enabled = !this.Proxy.Enabled;
            this.Proxy.Update();
        }
    }
}