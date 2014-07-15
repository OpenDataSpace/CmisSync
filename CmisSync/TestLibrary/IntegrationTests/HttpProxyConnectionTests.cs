//-----------------------------------------------------------------------
// <copyright file="HttpProxyConnectionTests.cs" company="GRAU DATA AG">
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
namespace TestLibrary.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.Net;

    using CmisSync.Lib;
    using CmisSync.Lib.Cmis;
    using CmisSync.Lib.Cmis.UiUtils;
    using CmisSync.Lib.Config;

    using DotCMIS;
    using DotCMIS.Client;

    using NUnit.Framework;

    [TestFixture, Timeout(60000)]
    public class HttpProxyConnectionTests
    {
        [SetUp, TearDown]
        public void ResetToDefaultProxySettings()
        {
            ProxySettings settings = new ProxySettings();
            settings.Selection = ProxySelection.SYSTEM;
            settings.LoginRequired = false;
            HttpProxyUtils.SetDefaultProxy(settings, true);
        }

        [TestFixtureSetUp]
        public void DisableSSLVerification()
        {
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        }

        [TestFixtureTearDown]
        public void EnableSSLVerification()
        {
            ServicePointManager.ServerCertificateValidationCallback = null;
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

            ProxySettings proxySettings = new ProxySettings();
            proxySettings.Selection = string.IsNullOrEmpty(cmisServerUrl) ? ProxySelection.NOPROXY : ProxySelection.CUSTOM;
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