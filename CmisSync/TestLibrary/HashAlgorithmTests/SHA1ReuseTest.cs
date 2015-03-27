//-----------------------------------------------------------------------
// <copyright file="SHA1ReuseTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.HashAlgorithmTests {
    using System;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;

    using CmisSync.Lib.HashAlgorithm;

    using NUnit.Framework;

    [TestFixture]
    public class SHA1ReuseTest {
        [Test, Category("Fast"), Category("Hash")]
        public void ComputeEmpty() {
            byte[] data = new byte[0];

            using (SHA1Managed sha1 = new SHA1Managed())
            using (SHA1Reuse reuse = new SHA1Reuse()) {
                Assert.IsTrue(reuse.ComputeHash(data).SequenceEqual(sha1.ComputeHash(data)));
            }

            using (SHA1Managed sha1 = new SHA1Managed())
            using (SHA1Reuse reuse = new SHA1Reuse()) {
                Assert.IsTrue(reuse.ComputeHash(data).SequenceEqual(sha1.ComputeHash(data)));
            }
        }

        [Test, Category("Fast"), Category("Hash")]
        public void ComputeArray([Values(0, 1, 2, 10, 1024, 325245)]long length) {
            byte[] data = new byte[length];

            using (SHA1Managed sha1 = new SHA1Managed())
            using (SHA1Reuse reuse = new SHA1Reuse()) {
                Assert.IsTrue(reuse.ComputeHash(data).SequenceEqual(sha1.ComputeHash(data)));
            }

            using (SHA1Managed sha1 = new SHA1Managed())
            using (SHA1Reuse reuse = new SHA1Reuse()) {
                Assert.IsTrue(reuse.ComputeHash(data).SequenceEqual(sha1.ComputeHash(data)));
            }
        }

        [Test, Category("Fast"), Category("Hash")]
        public void Compute1024Bytes() {
            byte[] data = new byte[1024];

            using (SHA1Managed sha1 = new SHA1Managed())
            using (SHA1Reuse reuse = new SHA1Reuse()) {
                Assert.IsTrue(reuse.ComputeHash(data).SequenceEqual(sha1.ComputeHash(data)));
            }

            using (SHA1Managed sha1 = new SHA1Managed())
            using (SHA1Reuse reuse = new SHA1Reuse()) {
                Assert.IsTrue(reuse.ComputeHash(data).SequenceEqual(sha1.ComputeHash(data)));
            }
        }

        [Test, Category("Fast"), Category("Hash")]
        public void ComputeBlocksByEmptyBlockSize() {
            int dataLength = 0;
            byte[] data = new byte[dataLength];

            using (SHA1Managed sha1 = new SHA1Managed())
            using (SHA1Reuse reuse = new SHA1Reuse()) {
                for (int i = 0; i < 10; ++i) {
                    sha1.TransformBlock(data, 0, dataLength, data, 0);
                    reuse.TransformBlock(data, 0, dataLength, data, 0);
                }

                sha1.TransformFinalBlock(data, dataLength, 0);
                reuse.TransformFinalBlock(data, dataLength, 0);
                Assert.IsTrue(sha1.Hash.SequenceEqual(reuse.Hash));
            }
        }

        [Test, Category("Fast"), Category("Hash")]
        public void ComputeBlocksWithBlockSize([Values(1, 1024, 324734)]int dataLength) {
            byte[] data = new byte[dataLength];

            using (SHA1Managed sha1 = new SHA1Managed())
            using (SHA1Reuse reuse = new SHA1Reuse()) {
                for (int i = 0; i < 10; ++i) {
                    sha1.TransformBlock(data, 0, dataLength, data, 0);
                    reuse.TransformBlock(data, 0, dataLength, data, 0);
                }

                sha1.TransformFinalBlock(data, dataLength, 0);
                reuse.TransformFinalBlock(data, dataLength, 0);
                Assert.IsTrue(sha1.Hash.SequenceEqual(reuse.Hash));
            }
        }

        [Test, Category("Fast"), Category("Hash")]
        public void ComputeBlocksViaReuse() {
            int dataLength = 23;
            byte[] data = new byte[dataLength];

            using (SHA1Managed sha1 = new SHA1Managed())
            using (SHA1Reuse reuse = new SHA1Reuse()) {
                SHA1Reuse hash0 = (SHA1Reuse)reuse.Clone();
                sha1.TransformBlock(data, 0, dataLength, data, 0);
                reuse.TransformBlock(data, 0, dataLength, data, 0);
                hash0.TransformBlock(data, 0, dataLength, data, 0);

                SHA1Reuse hash1 = (SHA1Reuse)reuse.Clone();
                sha1.TransformBlock(data, 0, dataLength, data, 0);
                reuse.TransformBlock(data, 0, dataLength, data, 0);
                hash0.TransformBlock(data, 0, dataLength, data, 0);
                hash1.TransformBlock(data, 0, dataLength, data, 0);

                SHA1Reuse hash2 = (SHA1Reuse)reuse.Clone();
                sha1.TransformBlock(data, 0, dataLength, data, 0);
                reuse.TransformBlock(data, 0, dataLength, data, 0);
                hash0.TransformBlock(data, 0, dataLength, data, 0);
                hash1.TransformBlock(data, 0, dataLength, data, 0);
                hash2.TransformBlock(data, 0, dataLength, data, 0);

                SHA1Reuse hash3 = (SHA1Reuse)reuse.Clone();
                sha1.TransformFinalBlock(data, dataLength, 0);
                reuse.TransformFinalBlock(data, dataLength, 0);
                hash0.TransformFinalBlock(data, dataLength, 0);
                hash1.TransformFinalBlock(data, dataLength, 0);
                hash2.TransformFinalBlock(data, dataLength, 0);
                hash3.TransformFinalBlock(data, dataLength, 0);

                Assert.IsTrue(sha1.Hash.SequenceEqual(reuse.Hash));
                Assert.IsTrue(sha1.Hash.SequenceEqual(hash0.Hash));
                Assert.IsTrue(sha1.Hash.SequenceEqual(hash1.Hash));
                Assert.IsTrue(sha1.Hash.SequenceEqual(hash2.Hash));
                Assert.IsTrue(sha1.Hash.SequenceEqual(hash3.Hash));
            }
        }

        [Test, Category("Fast"), Category("Hash")]
        public void ComputeBlocksViaClone() {
            int dataLength = 23;
            byte[] data = new byte[dataLength];

            using (SHA1Managed sha1 = new SHA1Managed())
            using (SHA1Managed sha1_1 = new SHA1Managed())
            using (SHA1Reuse reuse = new SHA1Reuse())
            using (SHA1Reuse hash0 = (SHA1Reuse)reuse.Clone()) {
                for (int i = 0; i < 100; i++) {
                    sha1.TransformBlock(data, 0, dataLength, data, 0);
                    sha1_1.TransformBlock(data, 0, dataLength, data, 0);
                    reuse.TransformBlock(data, 0, dataLength, data, 0);
                    hash0.TransformBlock(data, 0, dataLength, data, 0);
                }

                using(var hash1 = hash0.Clone() as HashAlgorithm)
                using(var hash2 = hash0.Clone() as HashAlgorithm)
                using(var hash3 = hash0.Clone() as HashAlgorithm) {
                    sha1.TransformFinalBlock(data, dataLength, 0);
                    reuse.TransformFinalBlock(data, dataLength, 0);
                    hash0.TransformFinalBlock(data, dataLength, 0);
                    Assert.That(sha1.Hash, Is.EqualTo(reuse.Hash));
                    Assert.That(sha1.Hash, Is.EqualTo(hash0.Hash));

                    for (int i = 0; i < 100; i++) {
                        sha1_1.TransformBlock(data, 0, dataLength, data, 0);
                        hash1.TransformBlock(data, 0, dataLength, data, 0);
                        hash2.TransformBlock(data, 0, dataLength, data, 0);
                        hash3.TransformBlock(data, 0, dataLength, data, 0);
                    }

                    hash1.TransformFinalBlock(data, dataLength, 0);
                    hash2.TransformFinalBlock(data, dataLength, 0);
                    hash3.TransformFinalBlock(data, dataLength, 0);
                    sha1_1.TransformFinalBlock(data, dataLength, 0);
                    Assert.That(sha1_1.Hash, Is.EqualTo(hash1.Hash));
                    Assert.That(sha1_1.Hash, Is.EqualTo(hash2.Hash));
                    Assert.That(sha1_1.Hash, Is.EqualTo(hash3.Hash));
                }
           }
        }
    }
}