using System;
using System.IO;
using System.Security.Cryptography;

using CmisSync.Lib;
using CmisSync.Lib.Events;
using CmisSync.Lib.Streams;

using NUnit.Framework;

using Moq;

namespace TestLibrary.StreamTests
{
    [TestFixture]
    public class NonClosingHashStreamTest
    {
        [Test, Category("Fast"), Category("Streams")]
        public void ConstructorTest() {
            var mock = new Mock<Stream>();
            var hashAlg = new Mock<HashAlgorithm>();
            using (var stream = new NonClosingHashStream(mock.Object,hashAlg.Object,CryptoStreamMode.Read)) {
                Assert.AreEqual(CryptoStreamMode.Read, stream.CipherMode);
            }
            using (var stream = new NonClosingHashStream(mock.Object,hashAlg.Object,CryptoStreamMode.Write)) {
                Assert.AreEqual(CryptoStreamMode.Write, stream.CipherMode);
            }
        }

        [Test, Category("Fast"), Category("Streams")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorFailsOnHashAlgorithmIsNull()
        {
            using (var stream = new NonClosingHashStream(new Mock<Stream>().Object, null, CryptoStreamMode.Write));
        }

        [Test, Category("Fast"), Category("Streams")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorFailsOnStreamIsNull()
        {
            using (var stream = new NonClosingHashStream(null, new Mock<HashAlgorithm>().Object, CryptoStreamMode.Write));
        }

        [Test, Category("Fast"), Category("Streams")]
        public void ReadTest() {
            byte[] content = new byte[1024];
            using(var stream = new MemoryStream(content))
            using (var hashAlg = new SHA1Managed())
            using ( var outputstream = new MemoryStream())
            {
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
            using(var stream = new MemoryStream(content))
            using (var hashAlg = new SHA1Managed())
            using ( var outputstream = new MemoryStream())
            {
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

