//-----------------------------------------------------------------------
// <copyright file="BandwidthNotifyingStreamTest.cs" company="GRAU DATA AG">
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

    using CmisSync.Lib.Streams;

    using NUnit.Framework;

    [TestFixture]
    public class BandwidthNotifyingStreamTest {
        [Test, Category("Fast"), Category("Streams")]
        public void EnsureBandwidthIsReportedIfProgressIsShorterThanOneSecond() {
            byte[] inputContent = new byte[1024];
            bool isMoreThanZeroReported = false;
            using (var inputStream = new MemoryStream(inputContent))
            using (var outputStream = new MemoryStream())
            using (var underTest = new BandwidthNotifyingStream(inputStream)) {
                underTest.PropertyChanged += delegate(object sender, System.ComponentModel.PropertyChangedEventArgs args) {
                    if ((sender as BandwidthNotifyingStream).BitsPerSecond > 0) {
                        isMoreThanZeroReported = true;
                    }
                };
                underTest.CopyTo(outputStream);
                Assert.That(outputStream.Length == inputContent.Length);
            }

            Assert.That(isMoreThanZeroReported, Is.True);
        }
    }
}