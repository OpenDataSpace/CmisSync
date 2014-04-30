//-----------------------------------------------------------------------
// <copyright file="KerberosAuthenticationProviderTest.cs" company="GRAU DATA AG">
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

    using DotCMIS.Binding;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class KerberosAuthenticationProviderTest
    {
        private HttpWebRequest request;
        private IBindingSession session;

        [SetUp]
        public void SetUp()
        {
            this.request = (HttpWebRequest)WebRequest.Create(new Uri("https://example.com/"));
            this.session = new Mock<IBindingSession>().Object;
        }

        [Test, Category("Fast")]
        public void ConstructorTest()
        {
            var auth = new NtlmAuthenticationProvider { Session = this.session };
            Assert.AreEqual(this.session, auth.Session);
        }

        [Test, Category("Fast")]
        public void AddCredentialsToWebRequest()
        {
            var auth = new NtlmAuthenticationProvider { Session = this.session };
            Assert.IsNull(this.request.Credentials);
            auth.Authenticate(this.request);
            Assert.IsNotNull(this.request.Credentials);
            Assert.AreEqual(CredentialCache.DefaultCredentials, this.request.Credentials);
        }

        [Test, Category("Fast")]
        public void EnableCookies()
        {
            var auth = new NtlmAuthenticationProvider { Session = this.session };
            Assert.IsNull(this.request.CookieContainer);
            auth.Authenticate(this.request);
            Assert.IsNotNull(this.request.CookieContainer);
        }
    }
}