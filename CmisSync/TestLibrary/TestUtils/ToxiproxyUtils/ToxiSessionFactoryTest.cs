using DotCMIS;
using System.Collections.Generic;
using DotCMIS.Binding;
using DotCMIS.Client.Impl.Cache;


namespace TestLibrary.TestUtils.ToxiproxyUtils {
    using System;

    using DotCMIS.Client;

    using Moq;

    using NUnit.Framework;

    [TestFixture, Category("Fast")]
    public class ToxiSessionFactoryTest {
        [Test]
        public void HostInUrlIsReplaced(
            [Values(SessionParameter.AtomPubUrl, SessionParameter.BrowserUrl)]string urlKey,
            [Values("http", "https")]string protocol)
        {
            string newHost = "localhost";
            int newPort = (new Random().Next() + 1024) % 64000;
            string origUrl = string.Format("{0}://demo.dataspace.cc/cmis/", protocol);
            var parameters = new Dictionary<string, string>();
            parameters.Add(urlKey, origUrl);
            var orig = new Mock<ISessionFactory>();
            orig.Setup(f => f.CreateSession(It.IsAny<IDictionary<string, string>>()))
                .Callback<IDictionary<string, string>>((p) => this.ValidateUrl(
                    uri: new UriBuilder(p[urlKey]),
                    expectedHost: newHost,
                    expectedPort: newPort,
                    expectedProtocol: protocol,
                    expectedPath: new UriBuilder(origUrl).Path));
            orig.Setup(f => f.CreateSession(It.IsAny<IDictionary<string, string>>(), null, null, null))
                .Callback<IDictionary<string, string>, IObjectFactory, IAuthenticationProvider, ICache>((p, o, a, c) => this.ValidateUrl(
                    uri: new UriBuilder(p[urlKey]),
                    expectedHost: newHost,
                    expectedPort: newPort,
                    expectedProtocol: protocol,
                    expectedPath: new UriBuilder(origUrl).Path));
            orig.Setup(f => f.GetRepositories(It.IsAny<IDictionary<string, string>>()))
                .Callback<IDictionary<string, string>>((p) => this.ValidateUrl(
                    uri: new UriBuilder(p[urlKey]),
                    expectedHost: newHost,
                    expectedPort: newPort,
                    expectedProtocol: protocol,
                    expectedPath: new UriBuilder(origUrl).Path));
            var underTest = new ToxiSessionFactory(orig.Object) {
                Host = newHost,
                Port = newPort
            };

            underTest.CreateSession(parameters);
            underTest.CreateSession(parameters, null, null, null);
            underTest.GetRepositories(parameters);

            orig.Verify(f => f.CreateSession(It.IsAny<IDictionary<string, string>>()), Times.Once());
            orig.Verify(f => f.CreateSession(It.IsAny<IDictionary<string, string>>(), null, null, null), Times.Once());
            orig.Verify(f => f.GetRepositories(It.IsAny<IDictionary<string, string>>()), Times.Once());

            Assert.That(parameters[urlKey], Is.EqualTo(origUrl));
        }

        private void ValidateUrl(
            UriBuilder uri,
            string expectedHost,
            int expectedPort,
            string expectedProtocol,
            string expectedPath)
        {
            Assert.That(uri.Host, Is.EqualTo(expectedHost));
            Assert.That(uri.Port, Is.EqualTo(expectedPort));
            Assert.That(uri.Scheme, Is.EqualTo(expectedProtocol));
            Assert.That(uri.Path, Is.EqualTo(expectedPath));
        }
    }
}