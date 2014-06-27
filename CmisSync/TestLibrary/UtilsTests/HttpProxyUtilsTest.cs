//-----------------------------------------------------------------------
// <copyright file="HttpProxyUtilsTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.UtilsTests
{
    using System;
    using System.Net;

    using CmisSync.Lib;
    using CmisSync.Lib.Config;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class HttpProxyUtilsTest
    {
        private ProxySettings settings;

        [SetUp]
        public void SetUp()
        {
            this.settings = new ProxySettings();
        }

        [Ignore]
        [Test, Category("Fast")]
        public void TestDefaultProxyEquality()
        {
            Assert.AreEqual(WebRequest.GetSystemWebProxy(), WebRequest.GetSystemWebProxy());
        }

        [Ignore]
        [Test, Category("Fast")]
        public void SetDefaultProxyToAuto()
        {
            // Prepare with false settings
            this.settings.Selection = ProxySelection.CUSTOM;
            this.settings.LoginRequired = false;
            this.settings.Server = new Uri("http://example-false.com:8080/");
            HttpProxyUtils.SetDefaultProxy(this.settings);

            // Set correct ones
            this.settings.Selection = ProxySelection.SYSTEM;
            HttpProxyUtils.SetDefaultProxy(this.settings);
            Assert.AreEqual(WebRequest.DefaultWebProxy, WebRequest.GetSystemWebProxy());
        }

        [Ignore]
        [Test, Category("Fast")]
        public void SetDefaultProxyToAutoOnNullInput()
        {
            // Prepare with false settings
            this.settings.Selection = ProxySelection.CUSTOM;
            this.settings.LoginRequired = false;
            this.settings.Server = new Uri("http://example-false.com:8080/");
            HttpProxyUtils.SetDefaultProxy(this.settings);

            HttpProxyUtils.SetDefaultProxy(new ProxySettings { Selection = ProxySelection.SYSTEM });
            Assert.AreEqual(WebRequest.DefaultWebProxy, WebRequest.GetSystemWebProxy());
        }

        [Test, Category("Fast")]
        public void SetDefaultProxyToHTTPWithoutCredentials()
        {
            this.settings.Selection = ProxySelection.CUSTOM;
            this.settings.LoginRequired = false;
            this.settings.Server = new Uri("http://example.com:8080/");
            HttpProxyUtils.SetDefaultProxy(this.settings);
            Assert.NotNull(WebRequest.DefaultWebProxy);
            Assert.AreNotEqual(WebRequest.DefaultWebProxy, WebRequest.GetSystemWebProxy());
            Assert.IsNull(WebRequest.DefaultWebProxy.Credentials);
        }

        [Test, Category("Fast")]
        public void SetDefaultProxyToHTTPWithCredentials()
        {
            this.settings.Selection = ProxySelection.CUSTOM;
            this.settings.Server = new Uri("http://example.com:8080/");
            this.settings.LoginRequired = true;
            this.settings.Username = "testuser";
            this.settings.ObfuscatedPassword = new CmisSync.Lib.Credentials.Password("password").ObfuscatedPassword;
            HttpProxyUtils.SetDefaultProxy(this.settings);
            Assert.IsNotNull(WebRequest.DefaultWebProxy);
            Assert.IsNotNull(WebRequest.DefaultWebProxy.Credentials);
        }

        [Test, Category("Fast")]
        public void SetDefaultProxyToBeIgnored()
        {
            this.settings.Selection = ProxySelection.CUSTOM;
            this.settings.LoginRequired = false;
            this.settings.Server = new Uri("http://example.com:8080/");
            HttpProxyUtils.SetDefaultProxy(this.settings);
            this.settings.Selection = ProxySelection.NOPROXY;
            HttpProxyUtils.SetDefaultProxy(this.settings);
            Assert.IsNull(HttpWebRequest.DefaultWebProxy);
        }
    }
}