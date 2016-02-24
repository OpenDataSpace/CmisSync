//-----------------------------------------------------------------------
// <copyright file="VirusDetectionTests.cs" company="GRAU DATA AG">
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
namespace TestLibrary.IntegrationTests.VirusDetectionTests {
    using System;
    using System.IO;

    using CmisSync.Lib.Cmis;
    using CmisSync.Lib.Cmis.ConvenienceExtenders;

    using DotCMIS.Client;
    using DotCMIS.Data;
    using DotCMIS.Data.Impl;
    using DotCMIS.Enums;
    using DotCMIS.Exceptions;

    using NUnit.Framework;

    using TestUtils;

    [TestFixture, TestName("VirusDetection"), Category("Slow"), Category("VirusDetection"), Timeout(180000)]
    public class VirusDetectionTests : BaseFullRepoTest {
        private readonly string fileName = "eicar.bin";

        [Ignore("https://mantis.dataspace.cc/view.php?id=4671")]
        [Test]
        public void UploadVirusByCreateDocument(
            [Values(VersioningState.Major, VersioningState.Minor, VersioningState.None, null)]VersioningState? versioningState)
        {
            var ex = Assert.Throws<CmisConstraintException>(() => {
                this.remoteRootDir.CreateDocument(this.fileName, this.Eicar);
            });
            Assert.That(ex.IsVirusDetectionException(), Is.True);
        }

        [Ignore("https://mantis.dataspace.cc/view.php?id=4671")]
        [Test]
        public void UploadVirusBySetContentStream(
            [Values("harmlessContent", null)]string initalContent)
        {
            var doc = this.remoteRootDir.CreateDocument(this.fileName, initalContent);
            using (var stream = new MemoryStream(this.Eicar)) {
                var contentStream = this.CreateStream(stream);
                var ex = Assert.Throws<CmisConstraintException>(() => doc.SetContentStream(contentStream, true, false));
                Assert.That(ex.IsVirusDetectionException(), Is.True);
            }
        }

        [Test]
        public void UploadVirusBySettingContentViaAppendContentStream(
            [Values("harmlessContent", null)]string initialContent)
        {
            var doc = this.remoteRootDir.CreateDocument(this.fileName, initialContent);
            doc.DeleteContentStream(true);
            using (var stream = new MemoryStream(this.Eicar)) {
                var contentStream = this.CreateStream(stream);
                var ex = Assert.Throws<CmisConstraintException>(() => doc.AppendContentStream(contentStream, true));
                Assert.That(ex.IsVirusDetectionException(), Is.True);
            }
        }

        [Test]
        public void UploadVirusByAppendingContentInPieces(
            [Values(true, false)]bool alwaysLastChunk)
        {
            var doc = this.remoteRootDir.CreateDocument(this.fileName, (string)null);
            var ex = Assert.Throws<CmisConstraintException>(() => {
                for (int i = 0; i < this.Eicar.Length; i++) {
                    bool lastChunk = alwaysLastChunk || i == (this.Eicar.Length - 1);
                    using (var stream = new MemoryStream(new byte[1] {this.Eicar[i]})) {
                        doc = doc.AppendContentStream(this.CreateStream(stream), lastChunk) ?? doc;
                    }
                }
            });
            Assert.That(ex.IsVirusDetectionException(), Is.True);
        }

        [Test]
        public void UploadVirusByPWCSetContentStream([Values(true, false)]bool major) {
            this.EnsureThatPrivateWorkingCopySupportIsAvailable();
            var doc = this.remoteRootDir.CreateDocument(this.fileName, (byte[])null, true);
            using (var stream = new MemoryStream(this.Eicar)) {
                var contentStream = this.CreateStream(stream);
                var ex = Assert.Throws<CmisConstraintException>(() => {
                    doc.SetContentStream(contentStream, true, false);
                    doc.CheckIn(major, null, null, string.Empty);
                });
                Assert.That(ex.IsVirusDetectionException(), Is.True);
            }
        }

        [Test]
        public void UploadVirusByPWCAppendContentStream([Values(true, false)]bool major) {
            this.EnsureThatPrivateWorkingCopySupportIsAvailable();
            var doc = this.remoteRootDir.CreateDocument(this.fileName, (byte[])null, true);
            using (var stream = new MemoryStream(this.Eicar)) {
                var contentStream = this.CreateStream(stream);
                var ex = Assert.Throws<CmisConstraintException>(() => {
                    doc.AppendContentStream(contentStream, true, false);
                    doc.CheckIn(major, null, null, string.Empty);
                });
                Assert.That(ex.IsVirusDetectionException(), Is.True);
            }
        }

        private ContentStream CreateStream(Stream stream) {
            var contentStream = new ContentStream();
            contentStream.FileName = this.fileName;
            contentStream.MimeType = MimeType.GetMIMEType(this.fileName);
            contentStream.Stream = stream;
            return contentStream;
        }

        private byte[] Eicar {
            get {
                return System.Text.ASCIIEncoding.ASCII.GetBytes(@"X5O!P%@AP[4\PZX54(P^)7CC)7}$EICAR-STANDARD-ANTIVIRUS-TEST-FILE!$H+H*");
            }
        }
    }
}