using System;
using System.Net;

using CmisSync.Lib.Cmis;
using CmisSync.Lib.Storage;

using NUnit.Framework;

using Moq;

namespace TestLibrary.AuthenticationProviderTests
{
    [TestFixture]
    public class PersistentStandardAuthenticationProviderTest
    {
        private ICookieStorage storage;
        private Uri url;
        private CookieCollection cookieCollection;

        [SetUp]
        public void SetUp()
        {
            url = new Uri("https://example.com/cmis/atom");
            cookieCollection = new CookieCollection();
            storage = new Mock<TemporaryCookieStorage>(){CallBase = true}.Object;
            storage.Cookies = cookieCollection;
        }

        [Test, Category("Fast")]
        public void ConstructorWithValidInput()
        {
            using(new PersistentStandardAuthenticationProvider(storage, url));
        }

        [Test, Category("Fast")]
        public void NtlmConstructorWithValidInput()
        {
            using(new PersistentNtlmAuthenticationProvider(storage, url));
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorFailsOnNullStorage()
        {
            using(new PersistentStandardAuthenticationProvider(null, url));
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NtlmConstructorFailsOnNullStorage()
        {
            using(new PersistentNtlmAuthenticationProvider(null, url));
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorFailsOnNullUrl()
        {
            using(new PersistentStandardAuthenticationProvider(storage, null));
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NtlmConstructorFailsOnNullUrl()
        {
            using(new PersistentNtlmAuthenticationProvider(storage, null));
        }

        [Test, Category("Fast")]
        public void SetCookieCollectionOnDispose()
        {
            using(new PersistentStandardAuthenticationProvider(storage, url));
        }

        [Test, Category("Fast")]
        public void SetCookieCollectionFilledWithCookiesOnDispose()
        {
            Cookie c = new Cookie("name","value", url.AbsolutePath, url.Host);
            using(var auth = new PersistentStandardAuthenticationProvider(storage, url)){
                auth.Cookies.Add(c);
            }
            Assert.AreEqual(c, storage.Cookies[0]);
            Assert.AreEqual(1, storage.Cookies.Count);
        }

/*        [Test, Category("Fast")]
        public void HandleResponseWithoutACookie()
        {
            var response = new Mock<HttpWebResponse>(MockBehavior.Loose).Object;
            using(var auth = new PersistentStandardAuthenticationProvider(storage, url))
            {
                auth.HandleResponse(response);
            }
            Assert.AreEqual(0, storage.Cookies.Count);
        }

        [Test, Category("Fast")]
        public void HandleResponseWithOneCookie()
        {
            Assert.Fail("TODO");
        }

        [Test, Category("Fast")]
        public void HandleResponseWithTwoCookies()
        {
            Assert.Fail("TODO");
        } */

        [Test, Category("Fast")]
        public void DoNotFailOnNonHTTPResponse()
        {
            using(var auth = new PersistentStandardAuthenticationProvider(storage, url))
            {
                auth.HandleResponse(new Mock<WebResponse>().Object);
            }
        }
    }
}

