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

