using System;

using CmisSync.Lib.Cmis;

using DotCMIS.Binding;

using NUnit.Framework;

using Moq;
using System.Net;

namespace TestLibrary.AuthenticationProviderTests
{
    [TestFixture]
    public class KerberosAuthenticationProviderTest
    {
        private HttpWebRequest Request;
        private IBindingSession Session;

        [SetUp]
        public void SetUp()
        {
            Request = (HttpWebRequest)WebRequest.Create(new Uri("https://example.com/"));
            Session = new Mock<IBindingSession>().Object;
        }

        [Test, Category("Fast")]
        public void ConstructorTest(){
            var auth = new NtlmAuthenticationProvider(){Session = Session};
            Assert.AreEqual(Session, auth.Session);
        }

        [Test, Category("Fast")]
        public void AddCredentialsToWebRequest(){
            var auth = new NtlmAuthenticationProvider(){Session = Session};
            Assert.IsNull(Request.Credentials);
            auth.Authenticate(Request);
            Assert.IsNotNull(Request.Credentials);
            Assert.AreEqual(CredentialCache.DefaultCredentials, Request.Credentials);
        }

        [Test, Category("Fast")]
        public void EnableCookies()
        {
            var auth = new NtlmAuthenticationProvider(){Session = Session};
            Assert.IsNull(Request.CookieContainer);
            auth.Authenticate(Request);
            Assert.IsNotNull(Request.CookieContainer);
        }
    }
}

