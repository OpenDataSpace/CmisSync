//-----------------------------------------------------------------------
// <copyright file="SHA1ManagedReuseTest.cs" company="GRAU DATA AG">
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
    using System.Linq;
    using System.Security.Cryptography;
    using System.IO;

    using CmisSync.Lib.HashAlgorithm;

    using NUnit.Framework;

    [TestFixture]
    public class SHA1ManagedReuseTest {
        [Test, Category("Fast"), Category("Hash")]
        public void SHA1ManagedReuse() {
            byte[] data = new byte[4096];

            using (SHA1Managed sha1 = new SHA1Managed())
            using (SHA1Reuse reuse = new SHA1Reuse()) {
                Assert.IsTrue(reuse.Compute(data).SequenceEqual(sha1.ComputeHash((data))));

                //HashAlgorithm hash0 = reuse.GetHashAlgorithm();
                //sha1.TransformBlock(data, 0, 4096, data, 0);
                //reuse.TransformBlock(data, 0, 4096, data, 0);
                //hash0.TransformBlock(data, 0, 4096, data, 0);

                //HashAlgorithm hash1 = reuse.GetHashAlgorithm();
                //sha1.TransformBlock(data, 0, 4096, data, 0);
                //reuse.TransformBlock(data, 0, 4096, data, 0);
                //hash0.TransformBlock(data, 0, 4096, data, 0);
                //hash1.TransformBlock(data, 0, 4096, data, 0);

                //HashAlgorithm hash2 = reuse.GetHashAlgorithm();
                //sha1.TransformBlock(data, 0, 4096, data, 0);
                //reuse.TransformBlock(data, 0, 4096, data, 0);
                //hash0.TransformBlock(data, 0, 4096, data, 0);
                //hash1.TransformBlock(data, 0, 4096, data, 0);
                //hash2.TransformBlock(data, 0, 4096, data, 0);

                //HashAlgorithm hash3 = reuse.GetHashAlgorithm();
                //sha1.TransformFinalBlock(data, 4096, 0);
                //reuse.TransformFinalBlock(data, 4096, 0);
                //hash0.TransformFinalBlock(data, 4096, 0);
                //hash1.TransformFinalBlock(data, 4096, 0);
                //hash2.TransformFinalBlock(data, 4096, 0);
                //hash3.TransformFinalBlock(data, 4096, 0);

                //byte[] result = sha1.Hash;
                //Assert.IsTrue(result.SequenceEqual(reuse.Hash));
                //Assert.IsTrue(result.SequenceEqual(hash0.Hash));
                //Assert.IsTrue(result.SequenceEqual(hash1.Hash));
                //Assert.IsTrue(result.SequenceEqual(hash2.Hash));
                //Assert.IsTrue(result.SequenceEqual(hash3.Hash));
            }
        }
    }
}
