
namespace TestLibrary.IntegrationTests.LinkTests {
    using System;
    using System.Collections.Generic;

    using CmisSync.Lib.Cmis.ConvenienceExtenders;

    using DotCMIS.Client;
    using DotCMIS.Exceptions;

    using NUnit.Framework;

    [TestFixture, Timeout(180000), TestName("ListLinks")]
    public class ListLinksTests : BaseLinkTest {
        [Test]
        public void ListLinks() {
            var doc = this.remoteRootDir.CreateDocument("testfile.bin", "test content");
            var folder = this.remoteRootDir.CreateFolder("uploadTarget");
            var downloadLink = this.session.CreateDownloadLink(objectIds: doc.Id);
            var uploadLink = this.session.CreateUploadLink(targetFolder: folder);

            var result = new List<IQueryResult>(this.session.GetAllLinks());

            Assert.That(result.Count, Is.GreaterThanOrEqualTo(2));
        }

        [SetUp]
        public void EnsureThatListingLinksIsSupported() {
            try {
                new List<IQueryResult>(this.session.GetAllLinks());
            } catch (CmisNotSupportedException) {
                Assert.Ignore("Server does not support to query links");
            }
        }
    }
}