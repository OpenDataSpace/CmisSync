using NUnit.Framework.Constraints;


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
            this.session.CreateDownloadLink(objectIds: doc.Id);
            this.session.CreateUploadLink(targetFolder: folder);

            var results = new List<IQueryResult>(this.session.GetAllLinks());

            Assert.That(results.Count, Is.GreaterThanOrEqualTo(2));
            VerifyThatResultsAreValid(on: results);
        }

        [Test]
        public void ListLinksByType([Values(LinkType.DownloadLink, LinkType.UploadLink)]LinkType type) {
            this.EnsureThatListingLinksIsSupported(type);
            var doc = this.remoteRootDir.CreateDocument("testfile.bin", "test content");
            var folder = this.remoteRootDir.CreateFolder("uploadTarget");
            int linkCount = new List<IQueryResult>(this.session.GetAllLinks(ofType: type)).Count;
            int expectedLinkCount = linkCount + 1;
            this.session.CreateDownloadLink(objectIds: doc.Id);
            this.session.CreateUploadLink(targetFolder: folder);

            var results = new List<IQueryResult>(this.session.GetAllLinks(ofType: type));

            Assert.That(results.Count, Is.EqualTo(expectedLinkCount));
            VerifyThatResultsAreValid(on: results, andLinkType: Is.EqualTo(type));
        }

        private static void VerifyThatResultsAreValid(IList<IQueryResult> on, IResolveConstraint andLinkType = null) {
            andLinkType = andLinkType ?? Is.EqualTo(LinkType.UploadLink).Or.EqualTo(LinkType.DownloadLink);
            foreach (var link in on) {
                Assert.That(link.GetId(), Is.Not.Null.Or.Empty);
                Assert.That(link.GetLinkType(), andLinkType);
                Assert.That(link.GetUrl().AbsoluteUri, Is.Not.Null.Or.Empty);
            }
        }

        [SetUp]
        public void EnsureThatListingLinksIsSupported(LinkType? of = null) {
            try {
                new List<IQueryResult>(this.session.GetAllLinks(of));
            } catch (CmisNotSupportedException) {
                Assert.Ignore("Server does not support to query links");
            }
        }
    }
}