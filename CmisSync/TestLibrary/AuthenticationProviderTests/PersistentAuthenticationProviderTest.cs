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
        private Mock<ICookieStorage> storage;
        private Uri url;

        [SetUp]
        public void SetUp()
        {
            url = new Uri("https://example.com/cmis/atom");
            storage = new Mock<ICookieStorage>();
        }

        [Test, Category("Fast")]
        public void ConstructorWithValidInput()
        {
            using(new PersistentStandardAuthenticationProvider(storage.Object, url));
        }

        [Test, Category("Fast")]
        public void NtlmConstructorWithValidInput()
        {
            using(new PersistentNtlmAuthenticationProvider(storage.Object, url));
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
            using(new PersistentStandardAuthenticationProvider(storage.Object, null));
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NtlmConstructorFailsOnNullUrl()
        {
            using(new PersistentNtlmAuthenticationProvider(storage.Object, null));
        }

        [Test, Category("Fast")]
        public void SetCookieCollectionOnDispose()
        {
            using(new PersistentStandardAuthenticationProvider(storage.Object, url));
        }

        [Test, Category("Fast")]
        public void SetCookieCollectionFilledWithCookiesOnDispose()
        {
            Cookie c = new Cookie("name","value", url.AbsolutePath, url.Host);
            using(var auth = new PersistentStandardAuthenticationProvider(storage.Object, url)){
                auth.Cookies.Add(c);
            }
            storage.VerifySet(s => s.Cookies = It.Is<CookieCollection>(cc => cc.Count == 1 && c.Equals(cc[0])));   
        }

        [Test, Category("Fast")]
        public void DoNotFailOnNonHTTPResponse()
        {
            using(var auth = new PersistentStandardAuthenticationProvider(storage.Object, url))
            {
                auth.HandleResponse(new Mock<WebResponse>().Object);
            }
        }
    }
}

