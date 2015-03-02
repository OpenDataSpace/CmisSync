//-----------------------------------------------------------------------
// <copyright file="UserCredentialsTest.cs" company="GRAU DATA AG">
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
namespace TestLibrary.ConfigTests {
    using System;

    using CmisSync.Lib.Config;

    using NUnit.Framework;

    [TestFixture]
    public class UserCredentialsTest {
        [Test, Category("Fast")]
        public void DefaultConstructor() {
            var cred = new UserCredentials();
            Assert.IsNull(cred.UserName);
            Assert.IsNull(cred.Password);
        }

        [Test, Category("Fast")]
        public void SetUsername() {
            string user = "testuser";
            var cred = new UserCredentials {
                UserName = user
            };
            Assert.AreEqual(user, cred.UserName);
        }

        [Test, Category("Fast")]
        public void SetPasswordObject() {
            var password = new Password("secret");
            var cred = new UserCredentials {
                Password = password
            };
            Assert.AreEqual(password.ToString(), cred.Password.ToString());
        }

        [Test, Category("Fast")]
        public void SetPasswordViaPlainTextString() {
            var password = new Password("secret");
            var cred = new UserCredentials {
                Password = password
            };
            Assert.AreEqual(password.ToString(), cred.Password.ToString());
        }
    }
}