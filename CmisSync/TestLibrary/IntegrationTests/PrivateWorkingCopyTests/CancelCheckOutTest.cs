
namespace TestLibrary.IntegrationTests.PrivateWorkingCopyTests {
    using System;

    using CmisSync.Lib.Cmis.ConvenienceExtenders;

    using DotCMIS.Exceptions;

    using NUnit.Framework;

    using TestUtils;

    [TestFixture, TestName("CancelCheckOut"), Ignore("https://mantis.dataspace.cc/view.php?id=4708")]
    public class CancelCheckOutTest : BaseFullRepoTest {
        [Test]
        public void CreateCheckedOutDocInFolderAndCancelCheckOut() {
            this.EnsureThatPrivateWorkingCopySupportIsAvailable();
            var doc = this.remoteRootDir.CreateDocument("test.bin", "content", true);
            var docs = this.remoteRootDir.GetCheckedOutDocs();
            Assert.That(docs.TotalNumItems, Is.EqualTo(1));
            Assert.That(docs.HasMoreItems, Is.False);
            foreach (var returnedDoc in docs) {
                Assert.That(returnedDoc.Id, Is.EqualTo(doc.Id));
                Assert.That(returnedDoc.Name, Is.EqualTo(doc.Name));
            }

            doc.CancelCheckOut();
            docs = this.remoteRootDir.GetCheckedOutDocs();
            Assert.That(docs.TotalNumItems, Is.EqualTo(0));
            Assert.That(docs.HasMoreItems, Is.False);
        }

        [Test]
        public void CreateCheckedOutDocInFolderAndCancelAllCheckedOutDocuments() {
            this.EnsureThatPrivateWorkingCopySupportIsAvailable();
            this.remoteRootDir.CreateDocument("test.bin", "content", true);
            var docs = this.remoteRootDir.GetCheckedOutDocs();
            foreach (var doc in docs) {
                doc.CancelCheckOut();
            }

            docs = this.remoteRootDir.GetCheckedOutDocs();
            Assert.That(docs.TotalNumItems, Is.EqualTo(0));
            Assert.That(docs.HasMoreItems, Is.False);
        }

        [Test]
        public void CreateCheckedOutDocumentInFolderAndSubfolderIsNotListingIt() {
            this.EnsureThatPrivateWorkingCopySupportIsAvailable();
            var folder = this.remoteRootDir.CreateFolder("folder");
            this.remoteRootDir.CreateDocument("test.bin", "content", true);
            folder.CreateDocument("test2.bin", "content2", true);
            var docs = folder.GetCheckedOutDocs();
            Assert.That(docs.TotalNumItems, Is.EqualTo(1));
            Assert.That(docs.HasMoreItems, Is.False);
        }

        [Test]
        public void CreateCheckedOutDocumentInSubFolderWhichGetsDeleted() {
            this.EnsureThatPrivateWorkingCopySupportIsAvailable();
            var folder = this.remoteRootDir.CreateFolder("folder");
            var doc = folder.CreateDocument("test.bin", "content", true);
            folder.DeleteTree(true, null, true);
            Assert.Throws<CmisRuntimeException>(() => doc.CheckIn(true, null, null, "should fail"));
            var docs = this.remoteRootDir.GetCheckedOutDocs();
            Assert.That(docs.TotalNumItems, Is.EqualTo(0));
            Assert.That(docs.HasMoreItems, Is.False);
        }
    }
}