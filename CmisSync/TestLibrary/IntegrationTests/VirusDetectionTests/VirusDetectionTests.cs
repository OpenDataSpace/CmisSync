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
    using DotCMIS.Exceptions;

    using NUnit.Framework;

    using TestUtils;
    [TestFixture, Timeout(180000), TestName("VirusDetection")]
    public class VirusDetectionTests : BaseFullRepoTest {
        [Test, Category("Slow"), Ignore("not yet implemented on server")]
        public void SetVirusContentStream() {
            string fileName = "eicar.bin";
            var doc = this.remoteRootDir.CreateDocument(fileName, null);
            byte[] eicar = System.Text.ASCIIEncoding.ASCII.GetBytes("X5O!P%@AP[4\\PZX54(P^)7CC)7}$EICAR-STANDARD-ANTIVIRUS-TEST-FILE!$H+H*");
            using (var stream = new MemoryStream(eicar)) {
                var contentStream = new ContentStream();
                contentStream.FileName = fileName;
                contentStream.MimeType = MimeType.GetMIMEType(contentStream.FileName);
                contentStream.Stream = stream;
                var ex = Assert.Throws<CmisConstraintException>(() => doc.SetContentStream(contentStream, true, false));
                Assert.That(ex.IsVirusDetectionException(), Is.True);
            }
        }
    }
}