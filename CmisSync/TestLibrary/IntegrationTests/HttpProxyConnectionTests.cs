
namespace TestLibrary.IntegrationTests
{
    using System;
    using System.Collections.Generic;

    using CmisSync.Lib;
    using CmisSync.Lib.Cmis;
    using CmisSync.Lib.Credentials;

    using DotCMIS;
    using DotCMIS.Client;

    using NUnit.Framework;

    [TestFixture, Timeout(60000)]
    public class HttpProxyConnectionTests
    {
        [SetUp]
        public void ResetToDefaultProxySettings()
        {
            Config.ProxySettings settings = new Config.ProxySettings();
            settings.Selection = Config.ProxySelection.SYSTEM;
            settings.LoginRequired = false;
            HttpProxyUtils.SetDefaultProxy(settings, true);
        }

        /// <summary>
        /// Gets the repositories trough proxy.
        /// </summary>
        /// <param name="cmisServerUrl">Cmis server URL.</param>
        /// <param name="cmisUser">Cmis user.</param>
        /// <param name="cmisPassword">Cmis password.</param>
        /// <param name="proxyUrl">Proxy URL.</param>
        /// <param name="proxyUser">Proxy user.</param>
        /// <param name="proxyPassword">Proxy password.</param>
        [Test, TestCaseSource(typeof(ITUtils), "ProxyServer"), Category("Slow"), Category("IT")]
        public void GetRepositoriesTroughProxy(
            string cmisServerUrl,
            string cmisUser,
            string cmisPassword,
            string proxyUrl,
            string proxyUser,
            string proxyPassword)
        {
            if (string.IsNullOrEmpty(proxyUrl)) {
                Assert.Ignore();
            }

            ServerCredentials credentials = new ServerCredentials
            {
                Address = new Uri(cmisServerUrl),
                UserName = cmisUser,
                Password = cmisPassword
            };

            Config.ProxySettings proxySettings = new Config.ProxySettings();
            proxySettings.Selection = string.IsNullOrEmpty(cmisServerUrl) ? Config.ProxySelection.NOPROXY : Config.ProxySelection.CUSTOM;
            proxySettings.Server = new Uri(proxyUrl);
            proxySettings.LoginRequired = !string.IsNullOrEmpty(proxyUser);
            if (proxySettings.LoginRequired)
            {
                proxySettings.Username = proxyUser;
                proxySettings.ObfuscatedPassword = Crypto.Obfuscate(proxyPassword);
            }

            HttpProxyUtils.SetDefaultProxy(proxySettings, true);

            Assert.That(CmisUtils.GetRepositories(credentials), Is.Not.Empty);
        }
    }
}