//-----------------------------------------------------------------------
// <copyright file="PersistentStandardAuthenticationProviderTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.AuthenticationProviderTests
{
    using System;
    using System.Net;

    using CmisSync.Lib.Cmis;
    using CmisSync.Lib.Storage;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class PersistentStandardAuthenticationProviderTest
    {
        private Mock<ICookieStorage> storage;
        private Uri url;

        [SetUp]
        public void SetUp()
        {
            this.url = new Uri("https://example.com/cmis/atom");
            this.storage = new Mock<ICookieStorage>();
        }

        [Test, Category("Fast")]
        public void ConstructorWithValidInput()
        {
            using (new PersistentStandardAuthenticationProvider(this.storage.Object, this.url))
            {
            }
        }

        [Test, Category("Fast")]
        public void NtlmConstructorWithValidInput()
        {
            using (new PersistentNtlmAuthenticationProvider(this.storage.Object, this.url))
            {
            }
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorFailsOnNullStorage()
        {
            using (new PersistentStandardAuthenticationProvider(null, this.url))
            {
            }
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NtlmConstructorFailsOnNullStorage()
        {
            using (new PersistentNtlmAuthenticationProvider(null, this.url))
            {
            }
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorFailsOnNullUrl()
        {
            using (new PersistentStandardAuthenticationProvider(this.storage.Object, null))
            {
            }
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NtlmConstructorFailsOnNullUrl()
        {
            using (new PersistentNtlmAuthenticationProvider(this.storage.Object, null))
            {
            }
        }

        [Test, Category("Fast")]
        public void SetCookieCollectionOnDispose()
        {
            using (new PersistentStandardAuthenticationProvider(this.storage.Object, this.url))
            {
            }
        }

        [Test, Category("Fast")]
        public void SetCookieCollectionFilledWithCookiesOnDispose()
        {
            Cookie c = new Cookie("name", "value", this.url.AbsolutePath, this.url.Host);
            using (var auth = new PersistentStandardAuthenticationProvider(this.storage.Object, this.url))
            {
                auth.Cookies.Add(c);
            }

            this.storage.VerifySet(s => s.Cookies = It.Is<CookieCollection>(cc => cc.Count == 1 && c.Equals(cc[0])));   
        }

        [Test, Category("Fast")]
        public void DoNotFailOnNonHTTPResponse()
        {
            using (var auth = new PersistentStandardAuthenticationProvider(this.storage.Object, this.url))
            {
                auth.HandleResponse(new Mock<WebResponse>().Object);
            }
        }
    }
}