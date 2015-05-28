//-----------------------------------------------------------------------
// <copyright file="PasswordTest.cs" company="GRAU DATA AG">
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
    using System.IO;
    using System.Xml.Serialization;

    using CmisSync.Lib.Config;

    using NUnit.Framework;

    [TestFixture]
    public class PasswordTest {
        [Test, Category("Fast")]
        public void DefaultConstructor() {
            var password = new Password();
            Assert.IsNull(password.ObfuscatedPassword);
            Assert.IsNull(password.ToString());
        }

        [Test, Category("Fast")]
        public void ConstructorTakingAPaintextPassword() {
            string passwd = "Test";
            var password = new Password(passwd);
            Assert.NotNull(password.ObfuscatedPassword);
            Assert.AreEqual(passwd, password.ToString());
        }

        [Test, Category("Fast")]
        public void ImplizitConstructorWithString() {
            string passwd = "Test";
            Password password = passwd;
            Assert.NotNull(password.ObfuscatedPassword);
            Assert.AreEqual(passwd, password.ToString());
        }

        [Test, Category("Fast")]
        public void SetAndGetObfuscatedPassword() {
            string passwd = "Test";
            string obfuscated = new Password(passwd).ObfuscatedPassword;
            Password password = new Password { ObfuscatedPassword = obfuscated };
            Assert.NotNull(password.ObfuscatedPassword);
            Assert.AreEqual(obfuscated, password.ObfuscatedPassword);
            Assert.AreEqual(passwd, password.ToString());
        }

        [Test, Category("Fast")]
        public void EnsureSerializationOnlyContainsObfuscatedPassword() {
            string plaintext = "secret";
            Password pw = new Password(plaintext);
            XmlSerializer xmlSerializer = new XmlSerializer(pw.GetType());
            StringWriter textWriter = new StringWriter();

            xmlSerializer.Serialize(textWriter, pw);

            Assert.IsTrue(textWriter.ToString().Contains(pw.ObfuscatedPassword));
            Assert.IsFalse(textWriter.ToString().Contains(plaintext));
        }

        [Test, Category("Fast")]
        public void OnePasswordEqualsAnotherInstanceWithTheSameStoredPassword() {
            var underTest = new Password("secret");
            Assert.That(underTest, Is.EqualTo(new Password("secret")));
        }

        [Test, Category("Fast")]
        public void EqualsOnNullReturnsFalse() {
            Assert.That(new Password("secret").Equals(null), Is.False);
        }
    }
}