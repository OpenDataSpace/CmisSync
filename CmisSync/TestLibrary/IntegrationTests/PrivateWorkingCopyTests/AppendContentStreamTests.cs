
namespace TestLibrary.IntegrationTests.PrivateWorkingCopyTests {
    using System;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;

    using CmisSync.Lib.Cmis.ConvenienceExtenders;

    using DotCMIS.Client;

    using NUnit.Framework;

    using TestUtils;

    [TestFixture, Timeout(180000), TestName("PWC")]
    public class AppendContentStreamTests : BaseFullRepoTest {
        private readonly string fileName = "fileName.bin";
        private readonly string content = "content";

        [Test, Category("Slow"), MaxTime(180000)]
        public void CheckOutDocumentAppendContentAndCheckIn() {
            this.EnsureThatPrivateWorkingCopySupportIsAvailable();
            var doc = this.remoteRootDir.CreateDocument(this.fileName, (string)null);

            var newId = doc.CheckOut();
            doc = newId == null ? doc : this.session.GetObject(newId) as IDocument;
            doc = doc.AppendContent(content) ?? doc;
            var newObjectId = doc.CheckIn(true, null, null, string.Empty);
            var newDocument = this.session.GetObject(newObjectId) as IDocument;
            newDocument.Refresh();

            this.remoteRootDir.Refresh();
            Assert.That(this.remoteRootDir.GetChildren().First().Name, Is.EqualTo(this.fileName));
            Assert.That(newDocument.Name, Is.EqualTo(this.fileName));
            Assert.That(newDocument.ContentStreamLength, Is.EqualTo(this.content.Length));
            this.AssertThatHashesAreEqualIfExists(content, newDocument);
        }

        [Test, Category("Slow"), MaxTime(180000)]
        public void CheckOutDocumentWithContentAndAppendContent() {
            this.EnsureThatPrivateWorkingCopySupportIsAvailable();
            var doc = this.remoteRootDir.CreateDocument(this.fileName, this.content);
            this.AssertThatHashesAreEqualIfExists(content, doc);

            var newId = doc.CheckOut();
            doc = newId == null ? doc : this.session.GetObject(newId) as IDocument;
            doc = doc.AppendContent(content) ?? doc;
            this.AssertThatHashesAreEqualIfExists(this.content + this.content, doc);
        }

        [Test, Category("Slow"), MaxTime(180000)]
        public void CheckOutDocumentWithContentAndAppendContentAndCheckIn() {
            this.EnsureThatPrivateWorkingCopySupportIsAvailable();
            var doc = this.remoteRootDir.CreateDocument(this.fileName, this.content);
            this.AssertThatHashesAreEqualIfExists(content, doc);

            var newId = doc.CheckOut();
            doc = newId == null ? doc : this.session.GetObject(newId) as IDocument;
            doc = doc.AppendContent(content) ?? doc;

            var newObjectId = doc.CheckIn(true, null, null, string.Empty);
            doc = this.session.GetObject(newObjectId) as IDocument;
            doc.Refresh();

            this.AssertThatHashesAreEqualIfExists(this.content + this.content, doc);
        }

        [Test, Category("Slow"), MaxTime(180000)]
        public void CheckOutDocumentWithContentAndAppendContentAndCancelCheckout() {
            this.EnsureThatPrivateWorkingCopySupportIsAvailable();
            var doc = this.remoteRootDir.CreateDocument(this.fileName, this.content);
            this.AssertThatHashesAreEqualIfExists(content, doc);

            var newId = doc.CheckOut();
            doc = newId == null ? doc : this.session.GetObject(newId) as IDocument;
            doc = doc.AppendContent(content) ?? doc;
            doc.CancelCheckOut();

            doc = this.remoteRootDir.GetChildren().First() as IDocument;
            this.AssertThatHashesAreEqualIfExists(content, doc);
        }
    }
}