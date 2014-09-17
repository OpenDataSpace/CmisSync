//-----------------------------------------------------------------------
// <copyright file="CredentialsTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.ConfigTests
{
    using System;
    using System.IO;
    using System.Xml.Serialization;

    using CmisSync.Lib.Config;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class CryptoTest
    {
        [Test, Category("Fast")]
        public void EncryptAndDecryptStrings()
        {
            string[] test_pws = { string.Empty, "test", "Whatever", "Something to try" };
            foreach (string pass in test_pws)
            {
                string crypted = Crypto.Obfuscate(pass);
                Assert.AreEqual(Crypto.Deobfuscate(crypted), pass);
            }
        }

        [Test, Category("Fast")]
        public void EncryptedIsDifferentToPlaintext()
        {
            string plain = "Testtesttest";
            string encrypted = Crypto.Obfuscate(plain);
            Assert.IsFalse(encrypted.Contains(plain));
        }
    }

    [TestFixture]
    public class PasswordTest
    {
        [Test, Category("Fast")]
        public void DefaultConstructor()
        {
            var password = new Password();
            Assert.IsNull(password.ObfuscatedPassword);
            Assert.IsNull(password.ToString());
        }

        [Test, Category("Fast")]
        public void ConstructorTakingAPaintextPassword()
        {
            string passwd = "Test";
            var password = new Password(passwd);
            Assert.NotNull(password.ObfuscatedPassword);
            Assert.AreEqual(passwd, password.ToString());
        }

        [Test, Category("Fast")]
        public void ImplizitConstructorWithString()
        {
            string passwd = "Test";
            Password password = passwd;
            Assert.NotNull(password.ObfuscatedPassword);
            Assert.AreEqual(passwd, password.ToString());
        }

        [Test, Category("Fast")]
        public void SetAndGetObfuscatedPassword()
        {
            string passwd = "Test";
            string obfuscated = new Password(passwd).ObfuscatedPassword;
            Password password = new Password { ObfuscatedPassword = obfuscated };
            Assert.NotNull(password.ObfuscatedPassword);
            Assert.AreEqual(obfuscated, password.ObfuscatedPassword);
            Assert.AreEqual(passwd, password.ToString());
        }

        [Test, Category("Fast")]
        public void EnsureSerializationOnlyContainsObfuscatedPassword()
        {
            string plaintext = "secret";
            Password pw = new Password(plaintext);
            XmlSerializer xmlSerializer = new XmlSerializer(pw.GetType());
            StringWriter textWriter = new StringWriter();

            xmlSerializer.Serialize(textWriter, pw);

            Assert.IsTrue(textWriter.ToString().Contains(pw.ObfuscatedPassword));
            Assert.IsFalse(textWriter.ToString().Contains(plaintext));
        }
    }

    [TestFixture]
    public class UserCredentialsTest
    {
        [Test, Category("Fast")]
        public void DefaultConstructor()
        {
            var cred = new UserCredentials();
            Assert.IsNull(cred.UserName);
            Assert.IsNull(cred.Password);
        }

        [Test, Category("Fast")]
        public void SetUsername()
        {
            string user = "testuser";
            var cred = new UserCredentials {
                UserName = user
            };
            Assert.AreEqual(user, cred.UserName);
        }

        [Test, Category("Fast")]
        public void SetPasswordObject()
        {
            var password = new Password("secret");
            var cred = new UserCredentials {
                Password = password
            };
            Assert.AreEqual(password.ToString(), cred.Password.ToString());
        }

        [Test, Category("Fast")]
        public void SetPasswordViaPlainTextString()
        {
            var password = new Password("secret");
            var cred = new UserCredentials {
                Password = password
            };
            Assert.AreEqual(password.ToString(), cred.Password.ToString());
        }
    }

    [TestFixture]
    public class ServerCredentialsTest
    {
        [Test, Category("Fast")]
        public void DefaultConstructor()
        {
            var cred = new ServerCredentials();
            Assert.IsNull(cred.Address);
            Assert.IsNull(cred.UserName);
            Assert.IsNull(cred.Password);
        }

        [Test, Category("Fast")]
        public void SetServerAddress()
        {
            string url = "http://example.com/";
            var cred = new ServerCredentials {
                Address = new Uri(url)
            };
            Assert.AreEqual(url, cred.Address.ToString());
        }
    }

    [TestFixture]
    public class CmisRepoCredentialsTest
    {
        [Test, Category("Fast")]
        public void DefaultConstructor()
        {
            var cred = new CmisRepoCredentials();
            Assert.IsNull(cred.Address);
            Assert.IsNull(cred.UserName);
            Assert.IsNull(cred.Password);
            Assert.IsNull(cred.RepoId);
        }

        [Test, Category("Fast")]
        public void SetRepoId()
        {
            string repoId = "RepoId";
            var cred = new CmisRepoCredentials() {
                RepoId = repoId
            };
            Assert.AreEqual(repoId, cred.RepoId);
        }
    }
}