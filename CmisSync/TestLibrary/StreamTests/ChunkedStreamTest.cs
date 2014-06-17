//-----------------------------------------------------------------------
// <copyright file="ChunkedStreamTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.StreamTests
{
    using System;
    using System.IO;

    using CmisSync.Lib;
    using CmisSync.Lib.Cmis;
    using CmisSync.Lib.Config;

    using NUnit.Framework;

    [TestFixture]
    public class ChunkedStreamTest
    {
        private readonly int chunkSize = 1024;

        [Test, Category("Fast"), Category("Streams")]
        public void TestWrite()
        {
            using (MemoryStream file = new MemoryStream())
            using (ChunkedStream chunked = new ChunkedStream(file, this.chunkSize))
            {
                byte[] buffer = new byte[2 * this.chunkSize];
                this.FillArray<byte>(buffer, (byte)'a');

                Assert.AreEqual(0, chunked.ChunkPosition);
                Assert.AreEqual(0, chunked.Position);
                Assert.AreEqual(0, chunked.Length);

                chunked.Write(buffer, 0, 1);
                Assert.AreEqual(1, file.Position);
                Assert.AreEqual(1, chunked.Position);
                Assert.AreEqual(1, chunked.Length);

                System.ArgumentOutOfRangeException e = Assert.Catch<System.ArgumentOutOfRangeException>(() => chunked.Write(buffer, 0, this.chunkSize));
                Assert.AreEqual("count", e.ParamName);
                Assert.AreEqual(this.chunkSize, e.ActualValue);
                Assert.AreEqual(1, file.Position);
                Assert.AreEqual(1, chunked.Position);
                Assert.AreEqual(1, chunked.Length);

                chunked.Write(buffer, 1, this.chunkSize - 1);
                Assert.AreEqual(this.chunkSize, file.Position);
                Assert.AreEqual(this.chunkSize, chunked.Position);
                Assert.AreEqual(this.chunkSize, chunked.Length);

                e = Assert.Catch<System.ArgumentOutOfRangeException>(() => chunked.Write(buffer, 0, 1));
                Assert.AreEqual("count", e.ParamName);
                Assert.AreEqual(1, e.ActualValue);
                Assert.AreEqual(this.chunkSize, file.Position);
                Assert.AreEqual(this.chunkSize, chunked.Position);
                Assert.AreEqual(this.chunkSize, chunked.Length);

                chunked.ChunkPosition = this.chunkSize;
                Assert.AreEqual(this.chunkSize, chunked.ChunkPosition);
                Assert.AreEqual(this.chunkSize, file.Position);
                Assert.AreEqual(0, chunked.Position);
                Assert.AreEqual(0, chunked.Length);

                chunked.Write(buffer, 0, this.chunkSize);
                Assert.AreEqual(2 * this.chunkSize, file.Position);
                Assert.AreEqual(this.chunkSize, chunked.Position);
                Assert.AreEqual(this.chunkSize, chunked.Length);

                e = Assert.Catch<System.ArgumentOutOfRangeException>(() => chunked.Write(buffer, 0, 1));
                Assert.AreEqual("count", e.ParamName);
                Assert.AreEqual(1, e.ActualValue);
                Assert.AreEqual(2 * this.chunkSize, file.Position);
                Assert.AreEqual(this.chunkSize, chunked.Position);
                Assert.AreEqual(this.chunkSize, chunked.Length);

                chunked.ChunkPosition = 4 * this.chunkSize;
                Assert.AreEqual(4 * this.chunkSize, chunked.ChunkPosition);
                Assert.AreEqual(4 * this.chunkSize, file.Position);
                Assert.AreEqual(0, chunked.Position);
                Assert.AreEqual(0, chunked.Length);

                chunked.Write(buffer, 1, this.chunkSize - 1);
                Assert.AreEqual((5 * this.chunkSize) - 1, file.Position);
                Assert.AreEqual(this.chunkSize - 1, chunked.Position);
                Assert.AreEqual(this.chunkSize - 1, chunked.Length);

                e = Assert.Catch<System.ArgumentOutOfRangeException>(() => chunked.Write(buffer, 0, this.chunkSize));
                Assert.AreEqual("count", e.ParamName);
                Assert.AreEqual(this.chunkSize, e.ActualValue);
                Assert.AreEqual((5 * this.chunkSize) - 1, file.Position);
                Assert.AreEqual(this.chunkSize - 1, chunked.Position);
                Assert.AreEqual(this.chunkSize - 1, chunked.Length);

                chunked.Write(buffer, 0, 1);
                Assert.AreEqual(5 * this.chunkSize, file.Position);
                Assert.AreEqual(this.chunkSize, chunked.Position);
                Assert.AreEqual(this.chunkSize, chunked.Length);

                e = Assert.Catch<System.ArgumentOutOfRangeException>(() => chunked.Write(buffer, 0, 1));
                Assert.AreEqual("count", e.ParamName);
                Assert.AreEqual(1, e.ActualValue);
                Assert.AreEqual(5 * this.chunkSize, file.Position);
                Assert.AreEqual(this.chunkSize, chunked.Position);
                Assert.AreEqual(this.chunkSize, chunked.Length);
            }
        }

        [Test, Category("Fast"), Category("Streams")]
        public void TestRead()
        {
            byte[] content = null;
            using (MemoryStream file = new MemoryStream())
            {
                byte[] buffer = new byte[this.chunkSize];

                this.FillArray<byte>(buffer, (byte)'1');
                file.Write(buffer, 0, this.chunkSize);

                this.FillArray<byte>(buffer, (byte)'2');
                file.Write(buffer, 0, this.chunkSize);

                this.FillArray<byte>(buffer, (byte)'3');
                file.Write(buffer, 0, 3);
                content = file.ToArray();
            }

            using (Stream file = new MemoryStream(content))
            using (ChunkedStream chunked = new ChunkedStream(file, this.chunkSize))
            {
                byte[] buffer = new byte[this.chunkSize];
                byte[] result = new byte[this.chunkSize];

                Assert.AreEqual(0, chunked.ChunkPosition);
                Assert.AreEqual(0, chunked.Position);
                Assert.AreEqual(this.chunkSize, chunked.Length);

                this.FillArray<byte>(buffer, (byte)'1');

                Assert.AreEqual(1, chunked.Read(result, 0, 1));
                Assert.IsTrue(this.EqualArray(buffer, result, 1));
                Assert.AreEqual(0, chunked.ChunkPosition);
                Assert.AreEqual(1, chunked.Position);
                Assert.AreEqual(this.chunkSize, chunked.Length);

                Assert.AreEqual(this.chunkSize - 1, chunked.Read(result, 1, this.chunkSize));
                Assert.IsTrue(this.EqualArray(buffer, result, this.chunkSize));
                Assert.AreEqual(0, chunked.ChunkPosition);
                Assert.AreEqual(this.chunkSize, chunked.Position);
                Assert.AreEqual(this.chunkSize, chunked.Length);

                Assert.AreEqual(0, chunked.Read(result, 0, this.chunkSize));
                Assert.AreEqual(0, chunked.ChunkPosition);
                Assert.AreEqual(this.chunkSize, chunked.Position);
                Assert.AreEqual(this.chunkSize, chunked.Length);

                chunked.ChunkPosition = 2 * this.chunkSize;
                Assert.AreEqual(2 * this.chunkSize, chunked.ChunkPosition);
                Assert.AreEqual(0, chunked.Position);
                Assert.AreEqual(3, chunked.Length);

                this.FillArray<byte>(buffer, (byte)'3');

                Assert.AreEqual(3, chunked.Read(result, 0, this.chunkSize));
                Assert.IsTrue(this.EqualArray(buffer, result, 3));
                Assert.AreEqual(2 * this.chunkSize, chunked.ChunkPosition);
                Assert.AreEqual(3, chunked.Position);
                Assert.AreEqual(3, chunked.Length);

                chunked.ChunkPosition = this.chunkSize;
                Assert.AreEqual(this.chunkSize, chunked.ChunkPosition);
                Assert.AreEqual(0, chunked.Position);
                Assert.AreEqual(this.chunkSize, chunked.Length);

                this.FillArray<byte>(buffer, (byte)'2');

                for (int i = 0; i < this.chunkSize; ++i)
                {
                    Assert.AreEqual(1, chunked.Read(result, i, 1));
                }

                Assert.IsTrue(this.EqualArray(buffer, result, this.chunkSize));
                Assert.AreEqual(this.chunkSize, chunked.ChunkPosition);
                Assert.AreEqual(this.chunkSize, chunked.Position);
                Assert.AreEqual(this.chunkSize, chunked.Length);
            }
        }

        private void FillArray<T>(T[] array, T value)
        {
            for (int i = 0; i < array.Length; ++i)
            {
                array[i] = value;
            }
        }

        private bool EqualArray<T>(T[] array1, T[] array2, int size)
        {
            for (int i = 0; i < size && i < array1.Length && i < array2.Length; ++i)
            {
                if (!array1[i].Equals(array2[i]))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
