//-----------------------------------------------------------------------
// <copyright file="ServerCredentialsTest.cs" company="GRAU DATA AG">
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
﻿
namespace TestLibrary {
    using System;

    using CmisSync.Lib.Config;

    using NUnit.Framework;

    [TestFixture, Category("Fast")]
    public class ServerCredentialsTest {
        [Test]
        public void DefaultConstructor() {
            var cred = new ServerCredentials();
            Assert.IsNull(cred.Address);
            Assert.IsNull(cred.UserName);
            Assert.IsNull(cred.Password);
        }

        [Test]
        public void SetServerAddress() {
            string url = "http://example.com/";
            var cred = new ServerCredentials {
                Address = new Uri(url)
            };
            Assert.AreEqual(url, cred.Address.ToString());
        }
    }
}