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

namespace TestLibrary.StreamsTests {
    using System;
    using System.IO;

    using CmisSync.Lib;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Streams;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class BandwidthLimitedStreamTest {
        private readonly int length = 1024 * 10;
        private byte[] buffer;
        private long limit = 1024;

        [SetUp]
        public void Setup() {
            this.buffer = new byte[this.length];
            this.limit = 1024;
        }

        [Test, Category("Fast"), Category("Streams")]
        public void ConstructorThrowsExceptionIfLimitIsZero() {
            Assert.Throws<ArgumentException>(() => { using (new BandwidthLimitedStream(Mock.Of<Stream>(), 0)); });
        }

        [Test, Category("Fast"), Category("Streams")]
        public void ConstructorThrowsExceptionIfLimitIsNegative() {
            Assert.Throws<ArgumentException>(() => { using (new BandwidthLimitedStream(Mock.Of<Stream>(), -1)); });
        }

        [Test, Category("Fast"), Category("Streams")]
        public void ConstructorThrowsExceptionIfStreamIsNull() {
            Assert.Throws<ArgumentNullException>(() => { using (new BandwidthLimitedStream(null, 1)); });
        }

        [Test, Category("Fast"), Category("Streams")]
        public void ConstructorThrowsExceptionIfBothParametersAreInvalid() {
            Assert.Throws(Is.InstanceOf<ArgumentException>(), () => { using (new BandwidthLimitedStream(null, -10)); });
        }

        [Test, Category("Fast"), Category("Streams")]
        public void ConstructorWithLimits() {
            using (var wrappedStream = Mock.Of<Stream>())
            using (var underTest = new BandwidthLimitedStream(wrappedStream, this.limit)) {
                Assert.That(underTest.ReadLimit, Is.EqualTo(this.limit));
                Assert.That(underTest.WriteLimit, Is.EqualTo(this.limit));
            }
        }

        [Test, Category("Fast"), Category("Streams")]
        public void ConstructorWithoutLimits() {
            using (var wrappedStream = Mock.Of<Stream>())
            using (var underTest = new BandwidthLimitedStream(wrappedStream)) {
                Assert.That(underTest.ReadLimit, Is.Null);
                Assert.That(underTest.WriteLimit, Is.Null);
            }
        }

        [Test, Category("Slow"), Category("Streams"), Timeout(10200)]
        public void LimitReadOrWriteStream([Values(true, false)]bool read) {
            using (var sourceOrTargetStream  = new MemoryStream(new byte[this.length]))
            using (var wrappedStream = new MemoryStream(this.buffer))
            using (var monitorStream = new BandwidthNotifyingStream(wrappedStream))
            using (var underTest = new BandwidthLimitedStream(monitorStream, limit: this.limit)) {
                monitorStream.PropertyChanged += (object sender, System.ComponentModel.PropertyChangedEventArgs e) => {
                    if (e.PropertyName == Utils.NameOf(() => monitorStream.BitsPerSecond)) {
                        Assert.That(monitorStream.BitsPerSecond, Is.Null.Or.LessThanOrEqualTo(this.limit));
                    }
                };

                if (read) {
                    underTest.CopyTo(sourceOrTargetStream);
                } else {
                    sourceOrTargetStream.CopyTo(underTest);
                }
            }
        }

        [Test, Category("Fast"), Category("Streams")]
        public void SetAndGetReadAndWriteLimits(
            [Values(true, false)]bool limitRead,
            [Values(true, false)]bool limitWrite)
        {
            using (var wrappedStream = Mock.Of<Stream>())
            using (var underTest = new BandwidthLimitedStream(wrappedStream)) {
                underTest.ReadLimit = limitRead ? this.limit : (long?)null;
                underTest.WriteLimit = limitWrite ? this.limit : (long?)null;
                Assert.That(underTest.ReadLimit, Is.EqualTo(limitRead ? this.limit : (long?)null));
                Assert.That(underTest.WriteLimit, Is.EqualTo(limitWrite ? this.limit : (long?)null));
            }
        }

        [Test, Category("Fast"), Category("Streams")]
        public void DisableLimits(
            [Values(true, false)]bool limitRead,
            [Values(true, false)]bool limitWrite)
        {
            using (var wrappedStream = Mock.Of<Stream>())
            using (var underTest = new BandwidthLimitedStream(wrappedStream)) {
                underTest.ReadLimit = limitRead ? this.limit : (long?)null;
                underTest.WriteLimit = limitWrite ? this.limit : (long?)null;
                underTest.DisableReadLimit();
                underTest.DisableWriteLimit();
                Assert.That(underTest.ReadLimit, Is.Null);
                Assert.That(underTest.WriteLimit, Is.Null);
            }
        }

        [Test, Category("Fast"), Category("Streams")]
        public void ChangingLimitsNotifiesListener() {
            using (var wrappedStream = Mock.Of<Stream>())
            using (var underTest = new BandwidthLimitedStream(wrappedStream)) {
                int readLimitNotified = 0;
                int writeLimitNotified = 0;
                long? expectedReadLimit = this.limit;
                long? expectedWriteLimit = this.limit;
                underTest.PropertyChanged += (sender, e) => {
                    Assert.That(sender, Is.EqualTo(underTest));
                    if (e.PropertyName == Utils.NameOf((BandwidthLimitedStream s) => s.ReadLimit)) {
                        readLimitNotified++;
                        Assert.That((sender as BandwidthLimitedStream).ReadLimit, Is.EqualTo(expectedReadLimit));
                    } else if (e.PropertyName == Utils.NameOf((BandwidthLimitedStream s) => s.WriteLimit)) {
                        writeLimitNotified++;
                        Assert.That((sender as BandwidthLimitedStream).WriteLimit, Is.EqualTo(expectedWriteLimit));
                    }
                };
                underTest.WriteLimit = expectedWriteLimit;
                underTest.ReadLimit = expectedReadLimit;

                Assert.That(readLimitNotified, Is.EqualTo(1));
                Assert.That(writeLimitNotified, Is.EqualTo(1));

                expectedReadLimit = null;
                expectedWriteLimit = null;

                underTest.DisableLimits();

                Assert.That(readLimitNotified, Is.EqualTo(2));
                Assert.That(writeLimitNotified, Is.EqualTo(2));
            }
        }
    }
}