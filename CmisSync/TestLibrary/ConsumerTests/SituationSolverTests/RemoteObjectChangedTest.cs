//-----------------------------------------------------------------------
// <copyright file="RemoteObjectChangedTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.ConsumerTests.SituationSolverTests {
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;

    using CmisSync.Lib.Cmis;
    using CmisSync.Lib.Consumer.SituationSolver;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;

    using DataSpace.Common.Transmissions;

    using DotCMIS.Client;
    using DotCMIS.Data;

    using Moq;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture]
    public class RemoteObjectChangedTest {
        private TransmissionManager manager;
        private Mock<ISession> session;
        private Mock<IMetaDataStorage> storage;
        private Mock<IFileSystemInfoFactory> fsFactory;
        private RemoteObjectChanged underTest;
        private ITransmissionFactory transmissionFactory;

        [SetUp]
        public void SetUp() {
            this.manager = new TransmissionManager();
            this.transmissionFactory = this.manager.CreateFactory();
            this.session = new Mock<ISession>();
            this.session.SetupTypeSystem();
            this.storage = new Mock<IMetaDataStorage>();
            this.fsFactory = new Mock<IFileSystemInfoFactory>();
            this.underTest = new RemoteObjectChanged(this.session.Object, this.storage.Object, null, this.transmissionFactory, this.fsFactory.Object);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void ConstructorThrowsExceptionIfTransmissionManagerIsNull() {
            Assert.Throws<ArgumentNullException>(() => new RemoteObjectChanged(this.session.Object, this.storage.Object, null, null));
        }

        [Test, Category("Fast"), Category("Solver")]
        public void ConstructorTakesQueueAndTransmissionManager() {
            new RemoteObjectChanged(this.session.Object, this.storage.Object, null, this.transmissionFactory);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void RemoteFolderChangedAndModificationDateCouldNotBeSet(
            [Values(true, false)]bool childrenAreIgnored)
        {
            DateTime creationDate = DateTime.UtcNow;
            string folderName = "a";
            string path = Path.Combine(Path.GetTempPath(), folderName);
            string id = "id";
            string parentId = "papa";
            string lastChangeToken = "token";
            string newChangeToken = "newToken";
            DateTime modification = DateTime.UtcNow;

            var dirInfo = new Mock<IDirectoryInfo>();
            dirInfo.Setup(d => d.FullName).Returns(path);
            dirInfo.Setup(d => d.Name).Returns(folderName);
            dirInfo.Setup(d => d.Parent).Returns(Mock.Of<IDirectoryInfo>());
            dirInfo.SetupGet(d => d.LastWriteTimeUtc).Returns(modification);
            dirInfo.SetupSet(d => d.LastWriteTimeUtc = It.IsAny<DateTime>()).Throws(new IOException("Another process is using this folder"));

            var mappedObject = new MappedObject(
                folderName,
                id,
                MappedObjectType.Folder,
                parentId,
                lastChangeToken)
            {
                Guid = Guid.NewGuid()
            };

            this.storage.AddMappedFolder(mappedObject);

            Mock<IFolder> remoteObject = MockOfIFolderUtil.CreateRemoteFolderMock(id, folderName, path, parentId, newChangeToken, childrenAreIgnored);
            remoteObject.Setup(f => f.LastModificationDate).Returns((DateTime?)creationDate);

            this.underTest.Solve(dirInfo.Object, remoteObject.Object);

            this.storage.VerifySavedMappedObject(MappedObjectType.Folder, id, folderName, parentId, newChangeToken, lastLocalModification: modification, ignored: childrenAreIgnored);
            dirInfo.VerifySet(d => d.LastWriteTimeUtc = It.Is<DateTime>(date => date.Equals(creationDate)), Times.Once());
        }

        [Test, Category("Fast"), Category("Solver")]
        public void RemoteFolderChanged(
            [Values(true, false)]bool childrenAreIgnored,
            [Values(true, false)]bool remoteFolderWasReadOnly,
            [Values(true, false)]bool remoteFolderIsReadOnly)
        {
            DateTime creationDate = DateTime.UtcNow;
            string folderName = "a";
            string path = Path.Combine(Path.GetTempPath(), folderName);
            string id = "id";
            string parentId = "papa";
            string lastChangeToken = "token";
            string newChangeToken = "newToken";

            var dirInfo = new Mock<IDirectoryInfo>();
            dirInfo.Setup(d => d.FullName).Returns(path);
            dirInfo.Setup(d => d.Name).Returns(folderName);
            dirInfo.Setup(d => d.Parent).Returns(Mock.Of<IDirectoryInfo>());
            dirInfo.SetupProperty(d => d.ReadOnly, remoteFolderWasReadOnly);

            var mappedObject = new MappedObject(
                folderName,
                id,
                MappedObjectType.Folder,
                parentId,
                lastChangeToken)
            {
                Guid = Guid.NewGuid(),
                IsReadOnly = remoteFolderWasReadOnly
            };

            this.storage.AddMappedFolder(mappedObject);

            Mock<IFolder> remoteObject = MockOfIFolderUtil.CreateRemoteFolderMock(id, folderName, path, parentId, newChangeToken, childrenAreIgnored);
            remoteObject.Setup(f => f.LastModificationDate).Returns((DateTime?)creationDate);
            remoteObject.SetupReadOnly(remoteFolderIsReadOnly);

            this.underTest.Solve(dirInfo.Object, remoteObject.Object);

            this.storage.VerifySavedMappedObject(MappedObjectType.Folder, id, folderName, parentId, newChangeToken, ignored: childrenAreIgnored, readOnly: remoteFolderIsReadOnly);
            dirInfo.VerifySet(d => d.LastWriteTimeUtc = It.Is<DateTime>(date => date.Equals(creationDate)), Times.Once());
            dirInfo.VerifyThatReadOnlyPropertyIsSet(to: remoteFolderIsReadOnly, iff: remoteFolderIsReadOnly != remoteFolderWasReadOnly);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void RemoteDocumentsMetaDataChanged(
            [Values(true, false)]bool remoteDocumentWasReadOnly,
            [Values(true, false)]bool remoteDocumentIsReadOnly)
        {
            byte[] hash = new byte[20];
            DateTime modificationDate = DateTime.UtcNow;
            string fileName = "a";
            string path = Path.Combine(Path.GetTempPath(), fileName);
            string id = "id";
            string parentId = "papa";
            string lastChangeToken = "token";
            string newChangeToken = "newToken";

            var fileInfo = new Mock<IFileInfo>();
            fileInfo.Setup(d => d.FullName).Returns(path);
            fileInfo.Setup(d => d.Name).Returns(fileName);
            fileInfo.Setup(d => d.Directory).Returns(Mock.Of<IDirectoryInfo>());
            fileInfo.SetupProperty(d => d.ReadOnly, remoteDocumentWasReadOnly);

            var mappedObject = new MappedObject(
                fileName,
                id,
                MappedObjectType.File,
                parentId,
                lastChangeToken)
            {
                Guid = Guid.NewGuid(),
                LastContentSize = 0,
                LastChecksum = hash,
                ChecksumAlgorithmName = "SHA-1",
                IsReadOnly = remoteDocumentWasReadOnly
            };

            this.storage.AddMappedFile(mappedObject);

            Mock<IDocument> remoteObject = MockOfIDocumentUtil.CreateRemoteDocumentMock(null, id, fileName, parentId, changeToken: newChangeToken);
            remoteObject.Setup(f => f.LastModificationDate).Returns((DateTime?)modificationDate);
            remoteObject.SetupReadOnly(remoteDocumentIsReadOnly);

            this.underTest.Solve(fileInfo.Object, remoteObject.Object, ContentChangeType.NONE, ContentChangeType.NONE);

            this.storage.VerifySavedMappedObject(MappedObjectType.File, id, fileName, parentId, newChangeToken, contentSize: 0, checksum: hash, readOnly: remoteDocumentIsReadOnly);
            fileInfo.VerifySet(d => d.LastWriteTimeUtc = It.Is<DateTime>(date => date.Equals(modificationDate)), Times.Once());
            fileInfo.VerifyThatReadOnlyPropertyIsSet(to: remoteDocumentIsReadOnly, iff: remoteDocumentIsReadOnly != remoteDocumentWasReadOnly);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void RemoteDocumentChanged() {
            DateTime creationDate = DateTime.UtcNow;
            string fileName = "a";
            string path = Path.Combine(Path.GetTempPath(), fileName);
            string id = "id";
            string parentId = "papa";
            string lastChangeToken = "token";
            string newChangeToken = "newToken";
            byte[] newContent = Encoding.UTF8.GetBytes("content");
            byte[] oldContent = Encoding.UTF8.GetBytes("older content");
            long oldContentSize = oldContent.Length;
            long newContentSize = newContent.Length;
            byte[] expectedHash = SHA1Managed.Create().ComputeHash(newContent);
            byte[] oldHash = SHA1Managed.Create().ComputeHash(oldContent);
            var mappedObject = new MappedObject(
                fileName,
                id,
                MappedObjectType.File,
                parentId,
                lastChangeToken,
                oldContentSize)
            {
                Guid = Guid.NewGuid(),
                LastLocalWriteTimeUtc = new DateTime(0),
                LastRemoteWriteTimeUtc = new DateTime(0),
                LastChecksum = oldHash
            };

            using (var oldContentStream = new MemoryStream(oldContent))
            using (var stream = new MemoryStream()) {
                var backupFile = this.fsFactory.AddFile(Path.Combine(Path.GetTempPath(), fileName + ".bak.sync"), false);

                this.storage.AddMappedFile(mappedObject, path);

                Mock<IDocument> remoteObject = MockOfIDocumentUtil.CreateRemoteDocumentMock(null, id, fileName, parentId, newContentSize, newContent, newChangeToken);
                remoteObject.Setup(r => r.LastModificationDate).Returns(creationDate);

                Mock<IFileInfo> localFile = new Mock<IFileInfo>();
                localFile.SetupProperty(f => f.LastWriteTimeUtc, new DateTime(0));
                localFile.Setup(f => f.FullName).Returns(path);
                var cacheFile = this.fsFactory.SetupDownloadCacheFile(localFile.Object);
                cacheFile.Setup(c => c.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)).Returns(stream);
                cacheFile.Setup(
                    c =>
                    c.Replace(localFile.Object, backupFile.Object, It.IsAny<bool>())).Returns(localFile.Object).Callback(
                    () =>
                    backupFile.Setup(
                    b =>
                    b.Open(FileMode.Open, FileAccess.Read, FileShare.None)).Returns(oldContentStream));

                this.underTest.Solve(localFile.Object, remoteObject.Object, remoteContent: ContentChangeType.CHANGED);

                this.storage.VerifySavedMappedObject(MappedObjectType.File, id, fileName, parentId, newChangeToken, true, creationDate, creationDate, expectedHash, newContent.Length);
                Assert.That(localFile.Object.LastWriteTimeUtc, Is.EqualTo(creationDate));
                cacheFile.Verify(c => c.Replace(localFile.Object, backupFile.Object, true), Times.Once());
                backupFile.Verify(b => b.Delete(), Times.Once());
            }
        }

        [Test, Category("Fast")]
        public void RemoteDocumentChangedAndLocalFileGotChangedWhileDownloadingCreatesAConfictFile() {
            DateTime creationDate = DateTime.UtcNow;
            string fileName = "a";
            string path = Path.Combine(Path.GetTempPath(), fileName);
            string id = "id";
            string parentId = "papa";
            string lastChangeToken = "token";
            string newChangeToken = "newToken";
            string confictFilePath = Path.Combine(Path.GetTempPath(), fileName + ".conflict");
            byte[] newContent = Encoding.UTF8.GetBytes("content");
            byte[] oldContent = Encoding.UTF8.GetBytes("older content");
            byte[] changedContent = Encoding.UTF8.GetBytes("change content");
            long oldContentSize = oldContent.Length;
            long newContentSize = newContent.Length;
            byte[] expectedHash = SHA1Managed.Create().ComputeHash(newContent);
            byte[] oldHash = SHA1Managed.Create().ComputeHash(oldContent);
            var mappedObject = new MappedObject(
                fileName,
                id,
                MappedObjectType.File,
                parentId,
                lastChangeToken,
                oldContentSize)
            {
                Guid = Guid.NewGuid(),
                LastLocalWriteTimeUtc = new DateTime(0),
                LastRemoteWriteTimeUtc = new DateTime(0),
                LastChecksum = oldHash
            };

            this.fsFactory.Setup(f => f.CreateConflictFileInfo(It.IsAny<IFileInfo>())).Returns(Mock.Of<IFileInfo>(i => i.FullName == confictFilePath));
            using (var changedContentStream = new MemoryStream(changedContent))
            using (var stream = new MemoryStream()) {
                var backupFile = this.fsFactory.AddFile(Path.Combine(Path.GetTempPath(), fileName + ".bak.sync"), false);

                this.storage.AddMappedFile(mappedObject, path);

                Mock<IDocument> remoteObject = MockOfIDocumentUtil.CreateRemoteDocumentMock(null, id, fileName, parentId, newContentSize, newContent, newChangeToken);
                remoteObject.Setup(r => r.LastModificationDate).Returns(creationDate);

                Mock<IFileInfo> localFile = new Mock<IFileInfo>();
                localFile.SetupProperty(f => f.LastWriteTimeUtc, new DateTime(0));
                localFile.Setup(f => f.FullName).Returns(path);
                var cacheFile = this.fsFactory.SetupDownloadCacheFile(localFile.Object);
                cacheFile.Setup(c => c.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)).Returns(stream);

                this.SetupContentWithCallBack(remoteObject, newContent, fileName).Callback(
                    () =>
                    {
                    localFile.Setup(f => f.LastWriteTimeUtc).Returns(creationDate);
                });

                cacheFile.Setup(
                    c =>
                    c.Replace(localFile.Object, backupFile.Object, It.IsAny<bool>())).Returns(localFile.Object).Callback(
                    () =>
                    backupFile.Setup(
                    b =>
                    b.Open(FileMode.Open, FileAccess.Read, FileShare.None)).Returns(changedContentStream));

                this.underTest.Solve(localFile.Object, remoteObject.Object, remoteContent: ContentChangeType.CHANGED);

                this.storage.VerifySavedMappedObject(MappedObjectType.File, id, fileName, parentId, newChangeToken, true, creationDate, creationDate, expectedHash, newContent.Length);
                Assert.That(localFile.Object.LastWriteTimeUtc, Is.EqualTo(creationDate));
                cacheFile.Verify(c => c.Replace(localFile.Object, backupFile.Object, true), Times.Once());
                backupFile.Verify(b => b.MoveTo(confictFilePath), Times.Once());
                backupFile.Verify(b => b.Delete(), Times.Never());
                backupFile.VerifySet(b => b.Uuid = null, Times.Once());
            }
        }

        [Test, Category("Fast"), Category("Solver")]
        public void RemoteDocumentChangedAndResetUuidFailsOnLocalModificationDate() {
            DateTime creationDate = DateTime.UtcNow;
            string fileName = "a";
            string path = Path.Combine(Path.GetTempPath(), fileName);
            string id = "id";
            string parentId = "papa";
            string lastChangeToken = "token";
            string newChangeToken = "newToken";
            byte[] newContent = Encoding.UTF8.GetBytes("content");
            byte[] oldContent = Encoding.UTF8.GetBytes("older content");
            long oldContentSize = oldContent.Length;
            long newContentSize = newContent.Length;
            byte[] expectedHash = SHA1Managed.Create().ComputeHash(newContent);
            byte[] oldHash = SHA1Managed.Create().ComputeHash(oldContent);
            var mappedObject = new MappedObject(
                fileName,
                id,
                MappedObjectType.File,
                parentId,
                lastChangeToken,
                oldContentSize)
            {
                Guid = Guid.NewGuid(),
                LastLocalWriteTimeUtc = new DateTime(0),
                LastRemoteWriteTimeUtc = new DateTime(0),
                LastChecksum = oldHash
            };

            using (var oldContentStream = new MemoryStream(oldContent))
                using (var stream = new MemoryStream()) {
                var backupFile = this.fsFactory.AddFile(Path.Combine(Path.GetTempPath(), fileName + ".bak.sync"), false);

                this.storage.AddMappedFile(mappedObject, path);

                Mock<IDocument> remoteObject = MockOfIDocumentUtil.CreateRemoteDocumentMock(null, id, fileName, parentId, newContentSize, newContent, newChangeToken);
                remoteObject.Setup(r => r.LastModificationDate).Returns(creationDate);

                Mock<IFileInfo> localFile = new Mock<IFileInfo>();
                localFile.SetupProperty(f => f.LastWriteTimeUtc, new DateTime(0));
                localFile.Setup(f => f.FullName).Returns(path);
                var cacheFile = this.fsFactory.SetupDownloadCacheFile(localFile.Object);
                cacheFile.Setup(c => c.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)).Returns(stream);
                cacheFile.Setup(
                    c =>
                    c.Replace(localFile.Object, backupFile.Object, It.IsAny<bool>())).Returns(localFile.Object).Callback(
                    () =>
                    backupFile.Setup(
                    b =>
                    b.Open(FileMode.Open, FileAccess.Read, FileShare.None)).Returns(oldContentStream));
                backupFile.SetupSet(b => b.Uuid = It.IsAny<Guid?>()).Throws<RestoreModificationDateException>();

                this.underTest.Solve(localFile.Object, remoteObject.Object, remoteContent: ContentChangeType.CHANGED);

                this.storage.VerifySavedMappedObject(MappedObjectType.File, id, fileName, parentId, newChangeToken, true, creationDate, creationDate, expectedHash, newContent.Length);
                Assert.That(localFile.Object.LastWriteTimeUtc, Is.EqualTo(creationDate));
                cacheFile.Verify(c => c.Replace(localFile.Object, backupFile.Object, true), Times.Once());
                backupFile.Verify(b => b.Delete(), Times.Once());
            }
        }

        [Test, Category("Fast"), Category("Solver")]
        public void RemoteDocumentChangedAndSetUuidFailsOnLocalModificationDate() {
            DateTime creationDate = DateTime.UtcNow;
            string fileName = "a";
            string path = Path.Combine(Path.GetTempPath(), fileName);
            string id = "id";
            string parentId = "papa";
            string lastChangeToken = "token";
            string newChangeToken = "newToken";
            byte[] newContent = Encoding.UTF8.GetBytes("content");
            byte[] oldContent = Encoding.UTF8.GetBytes("older content");
            long oldContentSize = oldContent.Length;
            long newContentSize = newContent.Length;
            byte[] expectedHash = SHA1Managed.Create().ComputeHash(newContent);
            byte[] oldHash = SHA1Managed.Create().ComputeHash(oldContent);
            var mappedObject = new MappedObject(
                fileName,
                id,
                MappedObjectType.File,
                parentId,
                lastChangeToken,
                oldContentSize)
            {
                Guid = Guid.NewGuid(),
                LastLocalWriteTimeUtc = new DateTime(0),
                LastRemoteWriteTimeUtc = new DateTime(0),
                LastChecksum = oldHash
            };

            using (var oldContentStream = new MemoryStream(oldContent))
                using (var stream = new MemoryStream()) {
                var backupFile = this.fsFactory.AddFile(Path.Combine(Path.GetTempPath(), fileName + ".bak.sync"), false);

                this.storage.AddMappedFile(mappedObject, path);

                Mock<IDocument> remoteObject = MockOfIDocumentUtil.CreateRemoteDocumentMock(null, id, fileName, parentId, newContentSize, newContent, newChangeToken);
                remoteObject.Setup(r => r.LastModificationDate).Returns(creationDate);

                Mock<IFileInfo> localFile = new Mock<IFileInfo>();
                localFile.SetupProperty(f => f.LastWriteTimeUtc, new DateTime(0));
                localFile.Setup(f => f.FullName).Returns(path);
                var cacheFile = this.fsFactory.SetupDownloadCacheFile(localFile.Object);
                cacheFile.Setup(c => c.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)).Returns(stream);
                cacheFile.Setup(
                    c =>
                    c.Replace(localFile.Object, backupFile.Object, It.IsAny<bool>())).Returns(localFile.Object).Callback(
                    () =>
                    backupFile.Setup(
                    b =>
                    b.Open(FileMode.Open, FileAccess.Read, FileShare.None)).Returns(oldContentStream));
                localFile.SetupSet(l => l.Uuid = It.IsAny<Guid?>()).Throws<RestoreModificationDateException>();

                this.underTest.Solve(localFile.Object, remoteObject.Object, remoteContent: ContentChangeType.CHANGED);

                this.storage.VerifySavedMappedObject(MappedObjectType.File, id, fileName, parentId, newChangeToken, true, creationDate, creationDate, expectedHash, newContent.Length);
                Assert.That(localFile.Object.LastWriteTimeUtc, Is.EqualTo(creationDate));
                cacheFile.Verify(c => c.Replace(localFile.Object, backupFile.Object, true), Times.Once());
                backupFile.Verify(b => b.Delete(), Times.Once());
            }
        }

        private Moq.Language.Flow.IReturnsResult<IDocument> SetupContentWithCallBack(Mock<IDocument> doc, byte[] content, string fileName, string mimeType = "application/octet-stream") {
            var stream = Mock.Of<IContentStream>(
                s =>
                s.Length == content.Length &&
                s.MimeType == mimeType &&
                s.FileName == fileName &&
                s.Stream == new MemoryStream(content));
            return doc.Setup(d => d.GetContentStream()).Returns(stream);
        }
    }
}