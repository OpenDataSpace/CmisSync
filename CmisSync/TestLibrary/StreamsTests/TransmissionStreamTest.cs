//-----------------------------------------------------------------------
// <copyright file="TransmissionStreamTest.cs" company="GRAU DATA AG">
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
﻿
namespace TestLibrary.StreamsTests {
    using System;
    using System.IO;

    using CmisSync.Lib.FileTransmission;
    using CmisSync.Lib.Streams;

    using NUnit.Framework;

    using Moq;

    [TestFixture]
    public class TransmissionStreamTest {
        [Test, Category("Fast"), Category("Streams")]
        public void ConstructorFailsIfStreamIsNull() {
            Assert.Throws<ArgumentNullException>(() => {using (new TransmissionStream(null, new Transmission(TransmissionType.DOWNLOAD_MODIFIED_FILE, "path")));});
        }

        [Test, Category("Fast"), Category("Streams")]
        public void ConstructorFailsIfTransmissionIsNull() {
            Assert.Throws<ArgumentNullException>(() => {
                using (var stream = new MemoryStream())
                using (new TransmissionStream(stream, null));
            });
        }

        [Test, Category("Fast"), Category("Streams")]
        public void ConstructorTakesTransmissionAndStream() {
            var transmission = new Transmission(TransmissionType.DOWNLOAD_MODIFIED_FILE, "path");
            using (var stream = new MemoryStream())
            using (var underTest = new TransmissionStream(stream, transmission)) {
            }
        }

        [Test, Category("Fast"), Category("Streams")]
        public void AbortReadIfAbortIsCalled() {
            var transmission = new Transmission(TransmissionType.DOWNLOAD_MODIFIED_FILE, "path");
            byte[] content = new byte[1024];
            using (var outputStream = new MemoryStream())
            using (var stream = new Mock<MemoryStream>() { CallBase = true }.Object)
            using (var underTest = new TransmissionStream(stream, transmission)) {
                transmission.Abort();
                Assert.Throws<AbortException>(() => underTest.CopyTo(outputStream));
                Mock.Get(stream).Verify(s => s.ReadByte(), Times.Never());
                Mock.Get(stream).Verify(s => s.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never());
            }
        }

        [Test, Category("Fast"), Category("Streams")]
        public void AbortWriteIfAbortIsCalled() {
            var transmission = new Transmission(TransmissionType.DOWNLOAD_MODIFIED_FILE, "path");
            using (var inputStream = new MemoryStream(new byte[1024 * 1024 * 10]))
            using (var stream = new Mock<MemoryStream>() { CallBase = true }.Object)
            using (var underTest = new TransmissionStream(stream, transmission)) {
                transmission.Abort();
                Assert.Throws<AbortException>(() => inputStream.CopyTo(underTest));
                Mock.Get(stream).Verify(s => s.WriteByte(It.IsAny<byte>()), Times.Never());
                Mock.Get(stream).Verify(s => s.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never());
            }
        }
    }
}