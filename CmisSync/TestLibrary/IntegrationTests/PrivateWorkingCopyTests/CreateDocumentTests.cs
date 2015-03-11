
namespace TestLibrary.IntegrationTests.PrivateWorkingCopyTests {
    using System;
    using System.Linq;

    using CmisSync.Lib.Cmis.ConvenienceExtenders;

    using DotCMIS.Client;

    using NUnit.Framework;

    using TestUtils;

    [TestFixture, Timeout(180000), TestName("PWC")]
    public class CreateDocumentTests : BaseFullRepoTest {
        private readonly string fileName = "fileName.bin";
        private readonly string content = "content";

        [Test, Category("Slow"), MaxTime(180000)]
        public void CreateCheckedOutDocument() {
            this.EnsureThatPrivateWorkingCopySupportIsAvailable();

            var doc = this.remoteRootDir.CreateDocument(this.fileName, (string)null, checkedOut: true);
            this.remoteRootDir.Refresh();
            doc.SetContent(content);
            var newObjectId = doc.CheckIn(true, null, null, string.Empty);
            var newDocument = this.session.GetObject(newObjectId) as IDocument;

            this.remoteRootDir.Refresh();
            Assert.That(this.remoteRootDir.GetChildren().First().Name, Is.EqualTo(fileName));
            Assert.That(newDocument.Name, Is.EqualTo(fileName));
            Assert.That(newDocument.ContentStreamLength, Is.EqualTo(content.Length));
        }

        [Test, Category("Slow"), MaxTime(180000)]
        public void CreateCheckedOutDocumentAndCancelCheckout([Values(true, false)]bool settingContent) {
            this.EnsureThatPrivateWorkingCopySupportIsAvailable();

            var doc = this.remoteRootDir.CreateDocument(this.fileName, (string)null, checkedOut: true);
            if (settingContent) {
                doc.SetContent(this.content);
            }

            doc.CancelCheckOut();

            this.remoteRootDir.Refresh();
            Assert.That(this.remoteRootDir.GetChildren().TotalNumItems, Is.EqualTo(0));
        }

        [Test, Category("Slow"), MaxTime(180000)]
        public void CreateCheckedOutDocumentAndDoNotCheckIn() {
            this.EnsureThatPrivateWorkingCopySupportIsAvailable();
            this.remoteRootDir.CreateDocument(this.fileName, (string)null, checkedOut: true);
            this.remoteRootDir.Refresh();
            Assert.That(this.remoteRootDir.GetChildren().TotalNumItems, Is.EqualTo(0));
        }

        private void EnsureThatPrivateWorkingCopySupportIsAvailable() {
            if (!this.session.ArePrivateWorkingCopySupported()) {
                Assert.Ignore("This session does not support updates on private working copies");
            }
        }
    }
}