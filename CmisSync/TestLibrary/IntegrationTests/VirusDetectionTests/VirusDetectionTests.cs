
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
                Assert.Throws<CmisConstraintException>(() => doc.SetContentStream(contentStream, true, false));
            }
        }
    }
}