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
    public class OffsetStreamTest
    {

        private long offset;
        private long contentLength;
        private byte[] content;

        [SetUp]
        public void SetUp(){
            offset = 100;
            contentLength = 100;
            content = new byte[contentLength];
            using(RandomNumberGenerator random = RandomNumberGenerator.Create()){
                random.GetBytes(content);
            }
        }

        [Test, Category("Fast"), Category("Streams")]
        public void ConstructorWithoutOffset(){
            using (var stream = new OffsetStream(new Mock<Stream>().Object)){
                Assert.AreEqual(0, stream.Offset);
            }
        }

        [Test, Category("Fast"), Category("Streams")]
        public void ConstructorWithOffset()
        {
            using (var stream = new OffsetStream(new Mock<Stream>().Object, 10)){
                Assert.AreEqual(10, stream.Offset);
            }
        }

        [Test, Category("Fast"), Category("Streams")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorFailsOnStreamIsNull()
        {
            using(new OffsetStream(null));
        }

        [Test, Category("Fast"), Category("Streams")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorFailsOnStreamIsNullAnOffsetIsGiven()
        {
            using(new OffsetStream(null, 10));
        }

        [Test, Category("Fast"), Category("Streams")]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ConstructorFailsOnNegativeOffset()
        {
            using(new OffsetStream(new Mock<Stream>().Object, -1));
        }

        [Test, Category("Fast"), Category("Streams")]
        public void LengthTest() {
            //static length test
            using(MemoryStream memstream = new MemoryStream(content))
                using(OffsetStream offsetstream = new OffsetStream(memstream, offset))
            {
                Assert.AreEqual(offset + content.Length, offsetstream.Length);
            }
            // dynamic length test
            using(MemoryStream memstream = new MemoryStream())
                using(OffsetStream offsetstream = new OffsetStream(memstream, offset))
            {
                Assert.AreEqual(0, memstream.Length);
                Assert.AreEqual(offset, offsetstream.Length);
                offsetstream.SetLength(200);
                Assert.AreEqual(200, offsetstream.Length);
                Assert.AreEqual(200-offset, memstream.Length);
                try{
                    offsetstream.SetLength(50);
                    Assert.Fail ();
                }catch(ArgumentOutOfRangeException){}
                Assert.AreEqual(200, offsetstream.Length);
                Assert.AreEqual(200-offset, memstream.Length);
            }
        }

        [Test, Category("Fast"), Category("Streams")]
        public void SeekTest() {
            using(MemoryStream memstream = new MemoryStream(content))
                using(OffsetStream offsetstream = new OffsetStream(memstream, offset))
            {
                Assert.True(offsetstream.CanSeek);
                Assert.AreEqual(memstream.CanSeek, offsetstream.CanSeek);
                Assert.AreEqual(memstream.Position + offset, offsetstream.Position);
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
                Assert.AreEqual(offset + 20, pos);
                Assert.AreEqual(20, memstream.Position);
                //negative seek
                pos = offsetstream.Seek(-10, SeekOrigin.Current);
                Assert.AreEqual(offset + 10, pos);
                Assert.AreEqual(10, memstream.Position);
                pos = offsetstream.Seek(-10, SeekOrigin.Current);
                Assert.AreEqual(offset, pos);
                Assert.AreEqual(0, memstream.Position);
                //seek into illegal areas
                try{
                    pos = offsetstream.Seek(-10, SeekOrigin.Current);
                    Assert.Fail();
                }catch(IOException){}
                Assert.AreEqual(offset, pos);
                Assert.AreEqual(0, memstream.Position);
            }

            // Using an unseekable stream should return CanSeek = false
            var mockstream = new Mock<Stream>();
            mockstream.SetupGet( s => s.CanSeek).Returns(false);
            using(OffsetStream offsetstream = new OffsetStream(mockstream.Object)) {
                Assert.False(offsetstream.CanSeek);
            }
        }

        [Test, Category("Fast"), Category("Streams")]
        public void ReadTest() {
            // Read block
            byte[] buffer = new byte[contentLength];
            using (MemoryStream memstream = new MemoryStream(content))
                using (OffsetStream offsetstream = new OffsetStream(memstream, offset))
            {
                Assert.AreEqual(0, memstream.Position);
                Assert.AreEqual(offset, offsetstream.Position);
                int len = offsetstream.Read(buffer, 0, buffer.Length);
                Assert.AreEqual(contentLength, len);
                Assert.AreEqual(contentLength+offset, offsetstream.Position);
                Assert.AreEqual(content, buffer);
                len = offsetstream.Read(buffer, 0, buffer.Length);
                Assert.AreEqual(0, len);
            }
        }

        
        [Test, Category("Fast"), Category("Streams")]
        public void WriteTest() {
            // Write one block
            using (MemoryStream memstream = new MemoryStream())
                using (OffsetStream offsetstream = new OffsetStream(memstream, offset))
            {
                Assert.AreEqual(0, memstream.Position);
                Assert.AreEqual(offset, offsetstream.Position);
                offsetstream.Write(content, 0, content.Length);
                Assert.AreEqual(content.Length + offset, offsetstream.Position);
                Assert.AreEqual(content, memstream.ToArray());
            }
            // Write single bytes
            using (MemoryStream memstream = new MemoryStream())
                using (OffsetStream offsetstream = new OffsetStream(memstream, offset))
            {
                Assert.AreEqual(0, memstream.Position);
                Assert.AreEqual(offset, offsetstream.Position);
                foreach(byte b in content)
                    offsetstream.WriteByte(b);
                Assert.AreEqual(content.Length + offset, offsetstream.Position);
                Assert.AreEqual(content, memstream.ToArray());
            }
        }
    }
}

