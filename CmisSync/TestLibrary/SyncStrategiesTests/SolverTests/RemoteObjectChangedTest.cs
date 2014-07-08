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

namespace TestLibrary.SyncStrategiesTests.SolverTests
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;

    using CmisSync.Lib;
    using CmisSync.Lib.Data;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Storage;
    using CmisSync.Lib.Sync.Solver;

    using DotCMIS.Client;
    using DotCMIS.Data;

    using Moq;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture]
    public class RemoteObjectChangedTest
    {
        private ActiveActivitiesManager manager;

        [SetUp]
        public void SetUp() {
            this.manager = new ActiveActivitiesManager();
        }

        [Test, Category("Fast"), Category("Solver")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorThrowsExceptionIfQueueIsNull()
        {
            new RemoteObjectChanged(null, this.manager);
        }

        [Test, Category("Fast"), Category("Solver")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorThrowsExceptionIfTransmissionManagerIsNull()
        {
            new RemoteObjectChanged(Mock.Of<ISyncEventQueue>(), null);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void ConstructorTakesQueueAndTransmissionManager()
        {
            new RemoteObjectChanged(Mock.Of<ISyncEventQueue>(), this.manager);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void RemoteFolderChanged()
        {
            DateTime creationDate = DateTime.UtcNow;
            string folderName = "a";
            string path = Path.Combine(Path.GetTempPath(), folderName);
            string id = "id";
            string parentId = "papa";
            string lastChangeToken = "token";
            string newChangeToken = "newToken";

            var storage = new Mock<IMetaDataStorage>();
            var queue = new Mock<ISyncEventQueue>();
            var dirInfo = new Mock<IDirectoryInfo>();
            dirInfo.Setup(d => d.FullName).Returns(path);
            dirInfo.Setup(d => d.Name).Returns(folderName);
            dirInfo.Setup(d => d.Parent).Returns(Mock.Of<IDirectoryInfo>());

            var mappedObject = new MappedObject(
                folderName,
                id,
                MappedObjectType.Folder,
                parentId,
                lastChangeToken)
            {
                Guid = Guid.NewGuid()
            };

            storage.AddMappedFolder(mappedObject);

            Mock<IFolder> remoteObject = MockOfIFolderUtil.CreateRemoteFolderMock(id, folderName, path, parentId, newChangeToken);
            remoteObject.Setup(f => f.LastModificationDate).Returns((DateTime?)creationDate);

            new RemoteObjectChanged(queue.Object, this.manager).Solve(Mock.Of<ISession>(), storage.Object, dirInfo.Object, remoteObject.Object);

            storage.VerifySavedMappedObject(MappedObjectType.Folder, id, folderName, parentId, newChangeToken);
            dirInfo.VerifySet(d => d.LastWriteTimeUtc = It.Is<DateTime>(date => date.Equals(creationDate)), Times.Once());
            queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Never());
        }

        [Test, Category("Fast"), Category("Solver")]
        public void RemoteDocumentChanged()
        {
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
            var queue = new Mock<ISyncEventQueue>();
            var storage = new Mock<IMetaDataStorage>();
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

            Mock<IFileSystemInfoFactory> fsFactory = new Mock<IFileSystemInfoFactory>();
            var cacheFile = fsFactory.AddFile(Path.Combine(Path.GetTempPath(), fileName + ".sync"), false);
            using (var oldContentStream = new MemoryStream(oldContent))
            using (var stream = new MemoryStream()) {
                cacheFile.Setup(c => c.Open(FileMode.Create, FileAccess.Write, FileShare.None)).Returns(stream);
                var backupFile = fsFactory.AddFile(Path.Combine(Path.GetTempPath(), fileName + ".bak.sync"), false);

                storage.AddMappedFile(mappedObject, path);

                Mock<IDocument> remoteObject = MockOfIDocumentUtil.CreateRemoteDocumentMock(null, id, fileName, parentId, newContentSize, newContent, newChangeToken);
                remoteObject.Setup(r => r.LastModificationDate).Returns(creationDate);

                Mock<IFileInfo> localFile = new Mock<IFileInfo>();
                localFile.SetupProperty(f => f.LastWriteTimeUtc, new DateTime(0));
                localFile.Setup(f => f.FullName).Returns(path);

                cacheFile.Setup(
                    c =>
                    c.Replace(localFile.Object, backupFile.Object, It.IsAny<bool>())).Returns(localFile.Object).Callback(
                    () =>
                    backupFile.Setup(
                    b =>
                    b.Open(FileMode.Open, FileAccess.Read, FileShare.None)).Returns(oldContentStream));

                new RemoteObjectChanged(queue.Object, this.manager, fsFactory.Object).Solve(Mock.Of<ISession>(), storage.Object, localFile.Object, remoteObject.Object);

                storage.VerifySavedMappedObject(MappedObjectType.File, id, fileName, parentId, newChangeToken, true, creationDate, expectedHash, newContent.Length);
                Assert.That(localFile.Object.LastWriteTimeUtc, Is.EqualTo(creationDate));
                queue.Verify(
                    q =>
                    q.AddEvent(It.Is<FileTransmissionEvent>(
                    e =>
                    e.Type == FileTransmissionType.DOWNLOAD_MODIFIED_FILE &&
                    e.CachePath == cacheFile.Object.FullName &&
                    e.Path == localFile.Object.FullName)),
                    Times.Once());
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
            var queue = new Mock<ISyncEventQueue>();
            var storage = new Mock<IMetaDataStorage>();
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

            Mock<IFileSystemInfoFactory> fsFactory = new Mock<IFileSystemInfoFactory>();
            fsFactory.Setup(f => f.CreateConflictFileInfo(It.IsAny<IFileInfo>())).Returns(Mock.Of<IFileInfo>(i => i.FullName == confictFilePath));
            var cacheFile = fsFactory.AddFile(Path.Combine(Path.GetTempPath(), fileName + ".sync"), false);
            using (var changedContentStream = new MemoryStream(changedContent))
            using (var stream = new MemoryStream()) {
                cacheFile.Setup(c => c.Open(FileMode.Create, FileAccess.Write, FileShare.None)).Returns(stream);
                var backupFile = fsFactory.AddFile(Path.Combine(Path.GetTempPath(), fileName + ".bak.sync"), false);

                storage.AddMappedFile(mappedObject, path);

                Mock<IDocument> remoteObject = MockOfIDocumentUtil.CreateRemoteDocumentMock(null, id, fileName, parentId, newContentSize, newContent, newChangeToken);
                remoteObject.Setup(r => r.LastModificationDate).Returns(creationDate);

                Mock<IFileInfo> localFile = new Mock<IFileInfo>();
                localFile.SetupProperty(f => f.LastWriteTimeUtc, new DateTime(0));
                localFile.Setup(f => f.FullName).Returns(path);

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

                new RemoteObjectChanged(queue.Object, this.manager, fsFactory.Object).Solve(Mock.Of<ISession>(), storage.Object, localFile.Object, remoteObject.Object);

                storage.VerifySavedMappedObject(MappedObjectType.File, id, fileName, parentId, newChangeToken, true, creationDate, expectedHash, newContent.Length);
                Assert.That(localFile.Object.LastWriteTimeUtc, Is.EqualTo(creationDate));
                queue.Verify(
                    q =>
                    q.AddEvent(It.Is<FileTransmissionEvent>(
                    e =>
                    e.Type == FileTransmissionType.DOWNLOAD_MODIFIED_FILE &&
                    e.CachePath == cacheFile.Object.FullName &&
                    e.Path == localFile.Object.FullName)),
                    Times.Once());
                cacheFile.Verify(c => c.Replace(localFile.Object, backupFile.Object, true), Times.Once());
                backupFile.Verify(b => b.MoveTo(confictFilePath), Times.Once());
                backupFile.Verify(b => b.Delete(), Times.Never());
                backupFile.Verify(b => b.SetExtendedAttribute(MappedObject.ExtendedAttributeKey, null), Times.Once());
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
