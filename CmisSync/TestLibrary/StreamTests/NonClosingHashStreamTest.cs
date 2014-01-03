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
        [Test, Category("Fast")]
        public void ConstructorTest() {
            var mock = new Mock<Stream>();
            var hashAlg = new Mock<HashAlgorithm>();
            using (var stream = new NonClosingHashStream(mock.Object,hashAlg.Object,CryptoStreamMode.Read)) {
                Assert.AreEqual(CryptoStreamMode.Read, stream.CipherMode);
            }
            using (var stream = new NonClosingHashStream(mock.Object,hashAlg.Object,CryptoStreamMode.Write)) {
                Assert.AreEqual(CryptoStreamMode.Write, stream.CipherMode);
            }
            try{
                using (var stream = new NonClosingHashStream(mock.Object, null, CryptoStreamMode.Write));
                Assert.Fail();
            }catch(ArgumentNullException){}
            try{
                using (var stream = new NonClosingHashStream(null, hashAlg.Object, CryptoStreamMode.Write));
                Assert.Fail();
            }catch(ArgumentNullException){}
        }

        [Test, Category("Fast")]
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

        [Test, Category("Fast")]
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

