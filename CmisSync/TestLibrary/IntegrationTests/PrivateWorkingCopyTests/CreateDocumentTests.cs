//-----------------------------------------------------------------------
// <copyright file="CreateDocumentTests.cs" company="GRAU DATA AG">
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
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    using CmisSync.Lib.Cmis.ConvenienceExtenders;

    using DotCMIS;
    using DotCMIS.Client;
    using DotCMIS.Enums;
    using DotCMIS.Exceptions;

    using NUnit.Framework;

    using TestUtils;

    [TestFixture, Category("Slow"), TestName("PWC"), Timeout(180000)]
    public class CreateDocumentTests : BaseFullRepoTest {
        private readonly string fileName = "fileName.bin";
        private readonly string content = "content";

        [Test]
        public void CreateCheckedOutDocument([Values(true, false)]bool withPropertiesOnCheckIn, [Values(true, false)]bool deleteExistingContentStream) {
            this.EnsureThatPrivateWorkingCopySupportIsAvailable();

            var doc = this.remoteRootDir.CreateDocument(this.fileName, (string)null, checkedOut: true);
            this.remoteRootDir.Refresh();
            if (deleteExistingContentStream) {
                doc.DeleteContentStream();
            }

            doc.SetContent(this.content);
            Dictionary<string, object> properties = null;
            if (withPropertiesOnCheckIn) {
                properties = new Dictionary<string, object>();
                properties.Add(PropertyIds.Name, this.fileName);
                properties.Add(PropertyIds.ObjectTypeId, BaseTypeId.CmisDocument.GetCmisValue());
            }

            var newObjectId = doc.CheckIn(true, properties, null, string.Empty);
            var newDocument = this.session.GetObject(newObjectId) as IDocument;

            this.remoteRootDir.Refresh();
            Assert.That(this.remoteRootDir.GetChildren().First().Name, Is.EqualTo(this.fileName));
            Assert.That(newDocument.Name, Is.EqualTo(this.fileName));
            Assert.That(newDocument.ContentStreamLength, Is.EqualTo(this.content.Length));
            AssertThatContentHashIsEqualToExceptedIfSupported(newDocument, content);
        }

        [Test]
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

        [Test]
        public void CreateCheckedOutDocumentAndDoNotCheckIn() {
            this.EnsureThatPrivateWorkingCopySupportIsAvailable();
            this.remoteRootDir.CreateDocument(this.fileName, (string)null, checkedOut: true);
            this.remoteRootDir.Refresh();
            Assert.That(this.remoteRootDir.GetChildren().TotalNumItems, Is.EqualTo(0));
        }

        [Test]
        public void CreateCheckedOutDocumentMustFailIfDocumentAlreadyExists() {
            this.EnsureThatPrivateWorkingCopySupportIsAvailable();
            string fileName = "file.bin";
            this.remoteRootDir.CreateDocument(fileName, "content");
            Assert.Throws<CmisNameConstraintViolationException>(() => this.remoteRootDir.CreateDocument(fileName, "other content", true));
        }

        [Test]
        public void CreateDocumentViaPwcCheckInWithLastModificationDate() {
            this.EnsureThatPrivateWorkingCopySupportIsAvailable();
            string fileName = "file.bin";
            DateTime past = DateTime.UtcNow - TimeSpan.FromDays(1);
            var doc = this.remoteRootDir.CreateDocument(fileName, "content", checkedOut: true);
            var properties = new Dictionary<string, object>();
            properties.Add(PropertyIds.LastModificationDate, past);
            doc = this.session.GetObject(doc.CheckIn(true, properties, null, string.Empty)) as IDocument;
            doc.Refresh();
            Assert.That(doc.LastModificationDate, Is.EqualTo(past).Within(1).Seconds);
        }

        [Test]
        public void CreateTooDocumentsInTwoFoldersWithTheSameName() {
            this.EnsureThatPrivateWorkingCopySupportIsAvailable();
            string fileName = "file.bin";
            var folder = this.remoteRootDir.CreateFolder("folder1");
            var doc1 = this.remoteRootDir.CreateDocument(fileName, (string)null, true);
            var doc2 = folder.CreateDocument(fileName, (string)null, true);
            doc1.CheckIn(true, null, null, string.Empty);
            doc2.CheckIn(true, null, null, string.Empty);
            this.remoteRootDir.Refresh();
            Assert.That(this.remoteRootDir.GetChildren().Count(), Is.EqualTo(2));
        }

        [Test]
        public void CreateTooDocumentsWithTheSameSubName() {
            this.EnsureThatPrivateWorkingCopySupportIsAvailable();
            string fileName1 = "gpio.h";
            string fileName2 = "io.h";
            var doc1 = this.remoteRootDir.CreateDocument(fileName1, "content");
            var doc2 = this.remoteRootDir.CreateDocument(fileName2, "content");
            this.remoteRootDir.Refresh();
            doc1.Refresh();
            doc2.Refresh();
        }
    }
}