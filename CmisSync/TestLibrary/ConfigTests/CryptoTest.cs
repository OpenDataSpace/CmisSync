//-----------------------------------------------------------------------
// <copyright file="CryptoTest.cs" company="GRAU DATA AG">
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
    [TestFixture, Category("Fast")]
    public class CryptoTest {
        [Test]
        public void EncryptAndDecryptStrings() {
            string[] test_pws = { string.Empty, "test", "Whatever", "Something to try" };
            foreach (string pass in test_pws) {
                string crypted = Crypto.Obfuscate(pass);
                Assert.AreEqual(Crypto.Deobfuscate(crypted), pass);
            }
        }

        [Test]
        public void EncryptedIsDifferentToPlaintext() {
            string plain = "Testtesttest";
            string encrypted = Crypto.Obfuscate(plain);
            Assert.IsFalse(encrypted.Contains(plain));
        }
    }
}