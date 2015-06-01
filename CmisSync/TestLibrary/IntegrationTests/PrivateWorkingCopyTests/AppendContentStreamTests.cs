//-----------------------------------------------------------------------
// <copyright file="AppendContentStreamTests.cs" company="GRAU DATA AG">
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
            var oldId = doc.Id;
            var newId = doc.CheckOut();
            var oldDoc = this.session.GetObject(oldId) as IDocument;
            oldDoc.Refresh();
            Assert.That(oldDoc.VersionSeriesCheckedOutId, Is.Not.Null);
            doc = newId == null ? doc : this.session.GetObject(newId) as IDocument;
            doc = doc.AppendContent(content) ?? doc;
            Assert.That(doc.Id, Is.EqualTo(oldDoc.VersionSeriesCheckedOutId));
            var newObjectId = doc.CheckIn(true, null, null, string.Empty);
            var newDocument = this.session.GetObject(newObjectId) as IDocument;
            newDocument.Refresh();

            this.remoteRootDir.Refresh();
            Assert.That(this.remoteRootDir.GetChildren().First().Name, Is.EqualTo(this.fileName));
            Assert.That(newDocument.Name, Is.EqualTo(this.fileName));
            Assert.That(newDocument.ContentStreamLength, Is.EqualTo(this.content.Length));
            Assert.That(newDocument.VersionSeriesCheckedOutId, Is.Null);
            this.AssertThatContentHashIsEqualToExceptedIfSupported(newDocument, this.content);
        }

        [Test, Category("Slow"), MaxTime(180000)]
        public void CheckOutDocumentWithContentAndAppendContent() {
            this.EnsureThatPrivateWorkingCopySupportIsAvailable();
            var doc = this.remoteRootDir.CreateDocument(this.fileName, this.content);
            doc.AssertThatIfContentHashExistsItIsEqualTo(this.content);

            var newId = doc.CheckOut();
            doc = newId == null ? doc : this.session.GetObject(newId) as IDocument;
            doc = doc.AppendContent(content) ?? doc;
            this.AssertThatContentHashIsEqualToExceptedIfSupported(doc, this.content + this.content);
        }

        [Test, Category("Slow"), MaxTime(180000)]
        public void CheckOutDocumentWithContentAndAppendContentAndCheckIn() {
            this.EnsureThatPrivateWorkingCopySupportIsAvailable();
            var emptyDocHash = IDocumentAssertUtils.ComputeSha1Hash(new byte[0]);
            var initialDocHash = IDocumentAssertUtils.ComputeSha1Hash(this.content);
            var doc = this.remoteRootDir.CreateDocument(this.fileName, this.content);
            doc.AssertThatIfContentHashExistsItIsEqualToHash(initialDocHash);

            var newId = doc.CheckOut();
            doc = newId == null ? doc : this.session.GetObject(newId) as IDocument;
            doc = doc.AppendContent(content) ?? doc;

            var newObjectId = doc.CheckIn(true, null, null, string.Empty);
            doc = this.session.GetObject(newObjectId) as IDocument;
            doc.Refresh();

            Assert.That(doc.ContentStreamHash(), Is.Not.EqualTo(emptyDocHash), "Hash is equal to empty document hash, but shouldn't");
            Assert.That(doc.ContentStreamHash(), Is.Not.EqualTo(initialDocHash), "Hash is equal to initial document hash, but shouldn't");

            this.AssertThatContentHashIsEqualToExceptedIfSupported(doc, this.content + this.content);
        }

        [Test, Category("Slow"), MaxTime(180000)]
        public void CheckOutDocumentWithContentAndAppendContentAndCancelCheckout() {
            this.EnsureThatPrivateWorkingCopySupportIsAvailable();
            var doc = this.remoteRootDir.CreateDocument(this.fileName, this.content);
            doc.AssertThatIfContentHashExistsItIsEqualTo(this.content);

            var newId = doc.CheckOut();
            doc = newId == null ? doc : this.session.GetObject(newId) as IDocument;
            doc = doc.AppendContent(content) ?? doc;
            doc.CancelCheckOut();

            doc = this.remoteRootDir.GetChildren().First() as IDocument;
            this.AssertThatContentHashIsEqualToExceptedIfSupported(doc, this.content);
        }

        [Test, Category("Slow"), MaxTime(180000)]
        public void AppendingAfterPWCIsCanceledMustFail() {
            this.EnsureThatPrivateWorkingCopySupportIsAvailable();
            var doc = this.remoteRootDir.CreateDocument(this.fileName, (string)null);
            var oldId = doc.Id;
            var newId = doc.CheckOut();
            var oldDoc = this.session.GetObject(oldId) as IDocument;
            oldDoc.Refresh();
            doc = newId == null ? doc : this.session.GetObject(newId) as IDocument;
            doc = doc.AppendContent(content, lastChunk: false) ?? doc;
            doc.CancelCheckOut();
            Assert.Catch<DotCMIS.Exceptions.CmisBaseException>(() => doc.AppendContent(content));
            oldDoc.Refresh();
            Assert.That(oldDoc.ContentStreamLength, Is.EqualTo(0));
            Assert.That(oldDoc.VersionSeriesCheckedOutId, Is.Null);
        }
    }
}