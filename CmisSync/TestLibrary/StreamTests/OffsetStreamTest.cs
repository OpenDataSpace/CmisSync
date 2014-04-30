//-----------------------------------------------------------------------
// <copyright file="OffsetStreamTest.cs" company="GRAU DATA AG">
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
    using System.Security.Cryptography;

    using CmisSync.Lib;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Streams;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class OffsetStreamTest
    {
        private long offset;
        private long contentLength;
        private byte[] content;

        [SetUp]
        public void SetUp()
        {
            this.offset = 100;
            this.contentLength = 100;
            this.content = new byte[this.contentLength];
            using (RandomNumberGenerator random = RandomNumberGenerator.Create()) {
                random.GetBytes(this.content);
            }
        }

        [Test, Category("Fast"), Category("Streams")]
        public void ConstructorWithoutOffset()
        {
            using (var stream = new OffsetStream(new Mock<Stream>().Object))
            {
                Assert.AreEqual(0, stream.Offset);
            }
        }

        [Test, Category("Fast"), Category("Streams")]
        public void ConstructorWithOffset()
        {
            using (var stream = new OffsetStream(new Mock<Stream>().Object, 10))
            {
                Assert.AreEqual(10, stream.Offset);
            }
        }

        [Test, Category("Fast"), Category("Streams")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorFailsOnStreamIsNull()
        {
            using (new OffsetStream(null))
            {
            }
        }

        [Test, Category("Fast"), Category("Streams")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorFailsOnStreamIsNullAnOffsetIsGiven()
        {
            using (new OffsetStream(null, 10))
            {
            }
        }

        [Test, Category("Fast"), Category("Streams")]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ConstructorFailsOnNegativeOffset()
        {
            using (new OffsetStream(new Mock<Stream>().Object, -1))
            {
            }
        }

        [Test, Category("Fast"), Category("Streams")]
        public void LengthTest()
        {
            // static length test
            using (MemoryStream memstream = new MemoryStream(this.content))
            using (OffsetStream offsetstream = new OffsetStream(memstream, this.offset))
            {
                Assert.AreEqual(this.offset + this.content.Length, offsetstream.Length);
            }

            // dynamic length test
            using (MemoryStream memstream = new MemoryStream())
                using (OffsetStream offsetstream = new OffsetStream(memstream, this.offset))
            {
                Assert.AreEqual(0, memstream.Length);
                Assert.AreEqual(this.offset, offsetstream.Length);
                offsetstream.SetLength(200);
                Assert.AreEqual(200, offsetstream.Length);
                Assert.AreEqual(200 - this.offset, memstream.Length);
                try
                {
                    offsetstream.SetLength(50);
                    Assert.Fail();
                }
                catch (ArgumentOutOfRangeException)
                {
                }

                Assert.AreEqual(200, offsetstream.Length);
                Assert.AreEqual(200 - this.offset, memstream.Length);
            }
        }

        [Test, Category("Fast"), Category("Streams")]
        public void SeekTest()
        {
            using (MemoryStream memstream = new MemoryStream(this.content))
                using (OffsetStream offsetstream = new OffsetStream(memstream, this.offset))
            {
                Assert.True(offsetstream.CanSeek);
                Assert.AreEqual(memstream.CanSeek, offsetstream.CanSeek);
                Assert.AreEqual(memstream.Position + this.offset, offsetstream.Position);
                long pos = offsetstream.Seek(10, SeekOrigin.Begin);
                Assert.AreEqual(110, pos);
                Assert.AreEqual(10, memstream.Position);
                pos = offsetstream.Seek(0, SeekOrigin.End);
                Assert.AreEqual(offsetstream.Length, pos);
                Assert.AreEqual(memstream.Length, memstream.Position);
                pos = offsetstream.Seek(0, SeekOrigin.Current);
                Assert.AreEqual(offsetstream.Length, pos);
                Assert.AreEqual(memstream.Length, memstream.Position);
                offsetstream.Seek(10, SeekOrigin.Begin);
                pos = offsetstream.Seek(10, SeekOrigin.Current);
                Assert.AreEqual(this.offset + 20, pos);
                Assert.AreEqual(20, memstream.Position);

                // negative seek
                pos = offsetstream.Seek(-10, SeekOrigin.Current);
                Assert.AreEqual(this.offset + 10, pos);
                Assert.AreEqual(10, memstream.Position);
                pos = offsetstream.Seek(-10, SeekOrigin.Current);
                Assert.AreEqual(this.offset, pos);
                Assert.AreEqual(0, memstream.Position);

                // seek into illegal areas
                try
                {
                    pos = offsetstream.Seek(-10, SeekOrigin.Current);
                    Assert.Fail();
                }
                catch (IOException)
                {
                }

                Assert.AreEqual(this.offset, pos);
                Assert.AreEqual(0, memstream.Position);
            }

            // Using an unseekable stream should return CanSeek = false
            var mockstream = new Mock<Stream>();
            mockstream.SetupGet(s => s.CanSeek).Returns(false);
            using (OffsetStream offsetstream = new OffsetStream(mockstream.Object)) {
                Assert.False(offsetstream.CanSeek);
            }
        }

        [Test, Category("Fast"), Category("Streams")]
        public void ReadTest()
        {
            // Read block
            byte[] buffer = new byte[this.contentLength];
            using (MemoryStream memstream = new MemoryStream(this.content))
                using (OffsetStream offsetstream = new OffsetStream(memstream, this.offset))
            {
                Assert.AreEqual(0, memstream.Position);
                Assert.AreEqual(this.offset, offsetstream.Position);
                int len = offsetstream.Read(buffer, 0, buffer.Length);
                Assert.AreEqual(this.contentLength, len);
                Assert.AreEqual(this.contentLength + this.offset, offsetstream.Position);
                Assert.AreEqual(this.content, buffer);
                len = offsetstream.Read(buffer, 0, buffer.Length);
                Assert.AreEqual(0, len);
            }
        }
        
        [Test, Category("Fast"), Category("Streams")]
        public void WriteTest()
        {
            // Write one block
            using (MemoryStream memstream = new MemoryStream())
                using (OffsetStream offsetstream = new OffsetStream(memstream, this.offset))
            {
                Assert.AreEqual(0, memstream.Position);
                Assert.AreEqual(this.offset, offsetstream.Position);
                offsetstream.Write(this.content, 0, this.content.Length);
                Assert.AreEqual(this.content.Length + this.offset, offsetstream.Position);
                Assert.AreEqual(this.content, memstream.ToArray());
            }

            // Write single bytes
            using (MemoryStream memstream = new MemoryStream())
                using (OffsetStream offsetstream = new OffsetStream(memstream, this.offset))
            {
                Assert.AreEqual(0, memstream.Position);
                Assert.AreEqual(this.offset, offsetstream.Position);
                foreach (byte b in this.content) {
                    offsetstream.WriteByte(b);
                }

                Assert.AreEqual(this.content.Length + this.offset, offsetstream.Position);
                Assert.AreEqual(this.content, memstream.ToArray());
            }
        }
    }
}