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
using System;

using CmisSync.Lib;

using Moq;

using NUnit.Framework;
using System.Net;

namespace TestLibrary.UtilsTests
{
    [TestFixture]
    public class HttpProxyUtilsTest
    {
        private Config.ProxySettings Settings;
        [SetUp]
        public void SetUp()
        {
            Settings = new Config.ProxySettings();
        }

        [Ignore]
        [Test, Category("Fast")]
        public void TestDefaultProxyEquality()
        {
            Assert.AreEqual( WebRequest.GetSystemWebProxy() , WebRequest.GetSystemWebProxy() );
        }

        [Ignore]
        [Test, Category("Fast")]
        public void SetDefaultProxyToAuto()
        {
            // Prepare with false settings
            Settings.Selection = Config.ProxySelection.CUSTOM;
            Settings.LoginRequired = false;
            Settings.Server = new Uri("http://example-false.com:8080/");
            HttpProxyUtils.SetDefaultProxy(Settings);

            //Set correct ones
            Settings.Selection = Config.ProxySelection.SYSTEM;
            HttpProxyUtils.SetDefaultProxy(Settings);
            Assert.AreEqual(WebRequest.DefaultWebProxy, WebRequest.GetSystemWebProxy() );
        }

        [Ignore]
        [Test, Category("Fast")]
        public void SetDefaultProxyToAutoOnNullInput()
        {
            // Prepare with false settings
            Settings.Selection = Config.ProxySelection.CUSTOM;
            Settings.LoginRequired = false;
            Settings.Server = new Uri("http://example-false.com:8080/");
            HttpProxyUtils.SetDefaultProxy(Settings);

            HttpProxyUtils.SetDefaultProxy(new Config.ProxySettings{Selection = Config.ProxySelection.SYSTEM});
            Assert.AreEqual(WebRequest.DefaultWebProxy, WebRequest.GetSystemWebProxy() );
        }

        [Test, Category("Fast")]
        public void SetDefaultProxyToHTTPWithoutCredentials()
        {
            Settings.Selection = Config.ProxySelection.CUSTOM;
            Settings.LoginRequired = false;
            Settings.Server = new Uri("http://example.com:8080/");
            HttpProxyUtils.SetDefaultProxy(Settings);
            Assert.NotNull(WebRequest.DefaultWebProxy);
            Assert.AreNotEqual(WebRequest.DefaultWebProxy, WebRequest.GetSystemWebProxy());
            Assert.IsNull(WebRequest.DefaultWebProxy.Credentials);
        }

        [Test, Category("Fast")]
        public void SetDefaultProxyToHTTPWithCredentials()
        {
            Settings.Selection = Config.ProxySelection.CUSTOM;
            Settings.Server = new Uri("http://example.com:8080/");
            Settings.LoginRequired = true;
            Settings.Username = "testuser";
            Settings.ObfuscatedPassword = "password";
            HttpProxyUtils.SetDefaultProxy(Settings);
            Assert.IsNotNull(WebRequest.DefaultWebProxy);
            Assert.IsNotNull(WebRequest.DefaultWebProxy.Credentials);
        }

        [Test, Category("Fast")]
        public void SetDefaultProxyToBeIgnored()
        {
            Settings.Selection = Config.ProxySelection.CUSTOM;
            Settings.LoginRequired = false;
            Settings.Server = new Uri("http://example.com:8080/");
            HttpProxyUtils.SetDefaultProxy(Settings);
            Settings.Selection = Config.ProxySelection.NOPROXY;
            HttpProxyUtils.SetDefaultProxy(Settings);
            Assert.IsNull (HttpWebRequest.DefaultWebProxy);
        }
    }
}

