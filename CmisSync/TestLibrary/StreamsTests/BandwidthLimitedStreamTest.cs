//-----------------------------------------------------------------------
// <copyright file="BandwidthLimitedStreamTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.StreamsTests
{
    using System;
    using System.IO;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Streams;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class BandwidthLimitedStreamTest
    {
        private int length;
        private byte[] buffer;

        [SetUp]
        public void Setup()
        {
            this.length = 1024 * 1024;
            this.buffer = new byte[this.length];
        }

        [Test, Category("Fast"), Category("Streams")]
        [ExpectedException(typeof(ArgumentException))]
        public void ConstructorThrowsExceptionIfLimitIsZero()
        {
            using (BandwidthLimitedStream limited = new BandwidthLimitedStream(Mock.Of<Stream>(), 0))
            {
            }
        }

        [Test, Category("Fast"), Category("Streams")]
        [ExpectedException(typeof(ArgumentException))]
        public void ConstructorThrowsExceptionIfLimitIsNegative()
        {
            using (BandwidthLimitedStream limited = new BandwidthLimitedStream(Mock.Of<Stream>(), -1))
            {
            }
        }

        [Test, Category("Fast"), Category("Streams")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorThrowsExceptionIfStreamIsNull()
        {
            using (BandwidthLimitedStream limited = new BandwidthLimitedStream(null, 1))
            {
            }
        }

        [Test, Category("Fast"), Category("Streams")]
        [ExpectedException(typeof(Exception))]
        public void ConstructorThrowsExceptionIfBothParametersAreInvalid()
        {
            try
            {
                using (BandwidthLimitedStream limited = new BandwidthLimitedStream(null, -10))
                {
                }
            }
            catch (ArgumentNullException)
            {
                throw new Exception();
            }
            catch (ArgumentException)
            {
                throw new Exception();
            }
        }

        [Test, Category("Fast"), Category("Streams")]
        public void ConstructorTest()
        {
            long limit = 0;

            limit = this.length;
            using (Stream memory = new MemoryStream(this.buffer))
            using (BandwidthLimitedStream limited = new BandwidthLimitedStream(memory, limit)) {
                Assert.AreEqual(limit, limited.ReadLimit);
                Assert.AreEqual(limit, limited.WriteLimit);
            }

            using (Stream memory = new MemoryStream(this.buffer))
            using (BandwidthLimitedStream limited = new BandwidthLimitedStream(memory)) {
                Assert.Less(limited.ReadLimit, 0);
                Assert.Less(limited.WriteLimit, 0);
            }
        }

        [Ignore]
        [Test, Category("Slow"), Category("Streams")]
        public void ReadTest()
        {
            byte[] buf = new byte[this.length];
            int counter = 0;
            using (MemoryStream memstream = new MemoryStream(this.buffer))
            using (BandwidthLimitedStream stream = new BandwidthLimitedStream(memstream, limit: this.length)) {
                DateTime start = DateTime.Now;
                while (counter < this.length) {
                    counter += stream.Read(buf, 0, this.length - counter);
                }

                TimeSpan duration = DateTime.Now - start;
                Assert.GreaterOrEqual(duration.TotalMilliseconds, 1000);
                Assert.AreEqual(1, duration.TotalSeconds);
            }

            counter = 0;
            using (MemoryStream memstream = new MemoryStream(this.buffer))
            using (BandwidthLimitedStream stream = new BandwidthLimitedStream(memstream)) {
                DateTime start = DateTime.Now;
                while (counter < this.length) {
                    counter += stream.Read(buf, 0, this.length - counter);
                }

                TimeSpan duration = DateTime.Now - start;
                Assert.GreaterOrEqual(duration.TotalMilliseconds, 1000);
                Assert.AreEqual(1, duration.TotalSeconds);
            }
        }

        [Ignore]
        [Test, Category("Slow"), Category("Streams")]
        public void WriteTest()
        {
            byte[] buf = new byte[this.length];
            using (MemoryStream memstream = new MemoryStream(this.buffer))
            using (BandwidthLimitedStream stream = new BandwidthLimitedStream(memstream, limit: this.length)) {
                DateTime start = DateTime.Now;
                stream.Write(buf, 0, this.length);
                TimeSpan duration = DateTime.Now - start;
                Assert.GreaterOrEqual(duration.TotalMilliseconds, 1000);
                Assert.AreEqual(1, duration.TotalSeconds);
            }

            using (MemoryStream memstream = new MemoryStream(this.buffer))
            using (BandwidthLimitedStream stream = new BandwidthLimitedStream(memstream)) {
                DateTime start = DateTime.Now;
                stream.Write(buf, 0, this.length);
                TimeSpan duration = DateTime.Now - start;
                Assert.GreaterOrEqual(duration.TotalMilliseconds, 1000);
                Assert.AreEqual(1, duration.TotalSeconds);
            }
        }

        [Test, Category("Fast"), Category("Streams")]
        public void ConfigureLimitsTest()
        {
            long limit = this.length;
            using (Stream memory = new MemoryStream(this.buffer))
            using (BandwidthLimitedStream limited = new BandwidthLimitedStream(memory)) {
                Assert.Less(limited.ReadLimit, 0);
                Assert.Less(limited.WriteLimit, 0);
                limited.ReadLimit = limit;
                Assert.AreEqual(limit, limited.ReadLimit);
                Assert.Less(limited.WriteLimit, 0);
                limited.WriteLimit = limit;
                Assert.AreEqual(limit, limited.ReadLimit);
                Assert.AreEqual(limit, limited.WriteLimit);
                limited.DisableLimits();
                Assert.Less(limited.ReadLimit, 0);
                Assert.Less(limited.WriteLimit, 0);
                limited.ReadLimit = limit;
                limited.WriteLimit = limit;
                Assert.AreEqual(limit, limited.ReadLimit);
                Assert.AreEqual(limit, limited.WriteLimit);
                limited.DisableReadLimit();
                Assert.Less(limited.ReadLimit, 0);
                Assert.AreEqual(limit, limited.WriteLimit);
                limited.DisableWriteLimit();
                Assert.Less(limited.ReadLimit, 0);
                Assert.Less(limited.WriteLimit, 0);
            }
        }
    }
}