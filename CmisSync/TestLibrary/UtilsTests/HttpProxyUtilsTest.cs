using System;

using CmisSync.Lib;
using CmisSync.Lib.Config;

using Moq;

using NUnit.Framework;
using System.Net;

namespace TestLibrary.UtilsTests
{
    [TestFixture]
    public class HttpProxyUtilsTest
    {
        private ProxySettings Settings;
        [SetUp]
        public void SetUp()
        {
            Settings = new ProxySettings();
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
            Settings.Selection = ProxySelection.CUSTOM;
            Settings.LoginRequired = false;
            Settings.Server = new Uri("http://example-false.com:8080/");
            HttpProxyUtils.SetDefaultProxy(Settings);

            //Set correct ones
            Settings.Selection = ProxySelection.SYSTEM;
            HttpProxyUtils.SetDefaultProxy(Settings);
            Assert.AreEqual(WebRequest.DefaultWebProxy, WebRequest.GetSystemWebProxy() );
        }

        [Ignore]
        [Test, Category("Fast")]
        public void SetDefaultProxyToAutoOnNullInput()
        {
            // Prepare with false settings
            Settings.Selection = ProxySelection.CUSTOM;
            Settings.LoginRequired = false;
            Settings.Server = new Uri("http://example-false.com:8080/");
            HttpProxyUtils.SetDefaultProxy(Settings);

            HttpProxyUtils.SetDefaultProxy(new ProxySettings{Selection = ProxySelection.SYSTEM});
            Assert.AreEqual(WebRequest.DefaultWebProxy, WebRequest.GetSystemWebProxy() );
        }

        [Test, Category("Fast")]
        public void SetDefaultProxyToHTTPWithoutCredentials()
        {
            Settings.Selection = ProxySelection.CUSTOM;
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
            Settings.Selection = ProxySelection.CUSTOM;
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
            Settings.Selection = ProxySelection.CUSTOM;
            Settings.LoginRequired = false;
            Settings.Server = new Uri("http://example.com:8080/");
            HttpProxyUtils.SetDefaultProxy(Settings);
            Settings.Selection = ProxySelection.NOPROXY;
            HttpProxyUtils.SetDefaultProxy(Settings);
            Assert.IsNull (HttpWebRequest.DefaultWebProxy);
        }
    }
}

