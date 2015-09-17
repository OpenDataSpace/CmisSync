//-----------------------------------------------------------------------
// <copyright file="CancelCheckOutTest.cs" company="GRAU DATA AG">
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