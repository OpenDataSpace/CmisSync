//-----------------------------------------------------------------------
// <copyright file="NonClosingHashStreamTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.StreamsTests {
    using System;
    using System.IO;
    using System.Security.Cryptography;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Streams;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class NonClosingHashStreamTest {
        [Test, Category("Fast"), Category("Streams")]
        public void ConstructorTest() {
            var mock = new Mock<Stream>();
            var hashAlg = new Mock<HashAlgorithm>();
            using (var stream = new NonClosingHashStream(mock.Object, hashAlg.Object, CryptoStreamMode.Read)) {
                Assert.AreEqual(CryptoStreamMode.Read, stream.CipherMode);
            }

            using (var stream = new NonClosingHashStream(mock.Object, hashAlg.Object, CryptoStreamMode.Write)) {
                Assert.AreEqual(CryptoStreamMode.Write, stream.CipherMode);
            }
        }

        [Test, Category("Fast"), Category("Streams")]
        public void ConstructorFailsOnHashAlgorithmIsNull() {
            Assert.Throws<ArgumentNullException>(() => { using (var stream = new NonClosingHashStream(new Mock<Stream>().Object, null, CryptoStreamMode.Write)); });
        }

        [Test, Category("Fast"), Category("Streams")]
        public void ConstructorFailsOnStreamIsNull() {
            Assert.Throws<ArgumentNullException>(() => { using (var stream = new NonClosingHashStream(null, new Mock<HashAlgorithm>().Object, CryptoStreamMode.Write)); });
        }

        [Test, Category("Fast"), Category("Streams")]
        public void ReadTest() {
            byte[] content = new byte[1024];
            using (var stream = new MemoryStream(content))
            using (var hashAlg = new SHA1Managed())
            using (var outputstream = new MemoryStream()) {
                using (var hashstream = new NonClosingHashStream(stream, hashAlg, CryptoStreamMode.Read)) {
                    hashstream.CopyTo(outputstream);
                }

                Assert.AreEqual(content, outputstream.ToArray());
                hashAlg.TransformFinalBlock(new byte[0], 0, 0);
                Assert.AreEqual(SHA1.Create().ComputeHash(content), hashAlg.Hash);
            }
        }

        [Test, Category("Fast"), Category("Streams")]
        public void WriteTest() {
            byte[] content = new byte[1024];
            using (var stream = new MemoryStream(content))
            using (var hashAlg = new SHA1Managed())
            using (var outputstream = new MemoryStream()) {
                using (var hashstream = new NonClosingHashStream(outputstream, hashAlg, CryptoStreamMode.Write)) {
                    stream.CopyTo(hashstream);
                }

                Assert.AreEqual(content, outputstream.ToArray());
                hashAlg.TransformFinalBlock(new byte[0], 0, 0);
                Assert.AreEqual(SHA1.Create().ComputeHash(content), hashAlg.Hash);
            }
        }
    }
}