//-----------------------------------------------------------------------
// <copyright file="RemoteObjectAddedTest.cs" company="GRAU DATA AG">
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

    using CmisSync.Lib.Consumer.SituationSolver;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.FileTransmission;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;

    using Moq;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture]
    public class RemoteObjectAddedTest {
        private readonly DateTime creationDate = DateTime.UtcNow;
        private readonly string id = "id";
        private readonly string objectName = "a";
        private readonly string parentId = "parentId";
        private readonly string lastChangeToken = "token";

        private string path;
        private ActiveActivitiesManager manager;
        private Mock<ISession> session;
        private Mock<IMetaDataStorage> storage;
        private Mock<IFileTransmissionStorage> transmissionStorage;
        private RemoteObjectAdded underTest;
        private Mock<IFileSystemInfoFactory> fsFactory;

        [SetUp]
        public void SetUp() {
            this.path = Path.Combine(Path.GetTempPath(), this.objectName);
            this.manager = new ActiveActivitiesManager();
            this.session = new Mock<ISession>();
            this.session.SetupTypeSystem();
            this.storage = new Mock<IMetaDataStorage>();
            this.transmissionStorage = new Mock<IFileTransmissionStorage>();
            this.fsFactory = new Mock<IFileSystemInfoFactory>(MockBehavior.Strict);
            this.underTest = new RemoteObjectAdded(this.session.Object, this.storage.Object, this.transmissionStorage.Object, this.manager, this.fsFactory.Object);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void ConstructorTakesQueue() {
            new RemoteObjectAdded(this.session.Object, this.storage.Object, null, this.manager);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void ConstructorThrowsExceptionIfTransmissionManagerIsNull() {
            Assert.Throws<ArgumentNullException>(() => new RemoteObjectAdded(this.session.Object, this.storage.Object, null, null));
        }

        [Test, Category("Fast"), Category("Solver")]
        public void RemoteFolderAdded([Values(true, false)]bool childrenAreIgnored) {
            var dirInfo = new Mock<IDirectoryInfo>();
            dirInfo.SetupAllProperties();
            dirInfo.Setup(d => d.FullName).Returns(this.path);
            dirInfo.Setup(d => d.Name).Returns(this.objectName);
            dirInfo.Setup(d => d.Parent).Returns(Mock.Of<IDirectoryInfo>());
            dirInfo.Setup(d => d.IsExtendedAttributeAvailable()).Returns(false);

            Mock<IFolder> remoteObject = MockOfIFolderUtil.CreateRemoteFolderMock(this.id, this.objectName, this.path, this.parentId, this.lastChangeToken, ignored: childrenAreIgnored);
            remoteObject.Setup(f => f.LastModificationDate).Returns((DateTime?)this.creationDate);

            this.underTest.Solve(dirInfo.Object, remoteObject.Object);

            dirInfo.Verify(d => d.Create(), Times.Once());
            this.storage.VerifySavedMappedObject(MappedObjectType.Folder, this.id, this.objectName, this.parentId, this.lastChangeToken, false, this.creationDate, ignored: childrenAreIgnored);
            dirInfo.VerifySet(d => d.LastWriteTimeUtc = It.Is<DateTime>(date => date.Equals(this.creationDate)), Times.Once());
            dirInfo.VerifySet(d => d.Uuid = It.IsAny<Guid?>(), Times.Never());
        }

        [Test, Category("Fast"), Category("Solver")]
        public void RemoteFolderAddedAndExtendedAttributesAreAvailable([Values(true, false)]bool childrenAreIgnored) {
            var dirInfo = new Mock<IDirectoryInfo>();
            dirInfo.SetupAllProperties();
            dirInfo.Setup(d => d.FullName).Returns(this.path);
            dirInfo.Setup(d => d.Name).Returns(this.objectName);
            dirInfo.Setup(d => d.Parent).Returns(Mock.Of<IDirectoryInfo>());
            dirInfo.Setup(d => d.IsExtendedAttributeAvailable()).Returns(true);

            Mock<IFolder> remoteObject = MockOfIFolderUtil.CreateRemoteFolderMock(this.id, this.objectName, this.path, this.parentId, this.lastChangeToken, ignored: childrenAreIgnored);
            remoteObject.Setup(f => f.LastModificationDate).Returns((DateTime?)this.creationDate);

            this.underTest.Solve(dirInfo.Object, remoteObject.Object);
            dirInfo.Verify(d => d.Create(), Times.Once());
            this.storage.VerifySavedMappedObject(MappedObjectType.Folder, this.id, this.objectName, this.parentId, this.lastChangeToken, true, this.creationDate, ignored: childrenAreIgnored);
            dirInfo.VerifySet(d => d.LastWriteTimeUtc = It.Is<DateTime>(date => date.Equals(this.creationDate)), Times.Once());
            dirInfo.VerifySet(d => d.Uuid = It.Is<Guid?>(uuid => uuid != null && !uuid.Equals(Guid.Empty)), Times.Once());
        }

        [Test, Category("Fast"), Category("Solver")]
        public void RemoteFileAddedAndExtendedAttributesAreAvailable() {
            var fileInfo = new Mock<IFileInfo>();
            var cacheFileInfo = this.fsFactory.SetupDownloadCacheFile();
            var parentDir = Mock.Of<IDirectoryInfo>(d => d.FullName == Path.GetTempPath());
            fileInfo.SetupAllProperties();
            fileInfo.Setup(f => f.FullName).Returns(this.path);
            fileInfo.Setup(f => f.Name).Returns(this.objectName);
            fileInfo.Setup(f => f.Directory).Returns(parentDir);
            byte[] content = Encoding.UTF8.GetBytes("content");
            byte[] expectedHash = SHA1Managed.Create().ComputeHash(content);
            cacheFileInfo.SetupAllProperties();
            cacheFileInfo.Setup(f => f.FullName).Returns(this.path + ".sync");
            cacheFileInfo.Setup(f => f.Name).Returns(this.objectName + ".sync");
            cacheFileInfo.Setup(f => f.Directory).Returns(parentDir);
            cacheFileInfo.Setup(f => f.IsExtendedAttributeAvailable()).Returns(true);
            using (var stream = new MemoryStream()) {
                cacheFileInfo.Setup(f => f.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)).Returns(() => {
                    cacheFileInfo.Setup(f => f.Exists).Returns(true);
                    return stream;
                });
                this.fsFactory.AddIFileInfo(cacheFileInfo.Object);

                Mock<IDocument> remoteObject = MockOfIDocumentUtil.CreateRemoteDocumentMock(null, this.id, this.objectName, this.parentId, content.Length, content, this.lastChangeToken);
                remoteObject.Setup(f => f.LastModificationDate).Returns((DateTime?)this.creationDate);

                this.underTest.Solve(fileInfo.Object, remoteObject.Object);

                cacheFileInfo.Verify(f => f.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None), Times.Once());
                cacheFileInfo.VerifySet(f => f.Uuid = It.Is<Guid?>(uuid => uuid != null && !uuid.Equals(Guid.Empty)), Times.Once());
                cacheFileInfo.Verify(f => f.MoveTo(this.path), Times.Once());
                fileInfo.VerifySet(d => d.LastWriteTimeUtc = It.Is<DateTime>(date => date.Equals(this.creationDate)), Times.Once());
                this.storage.VerifySavedMappedObject(MappedObjectType.File, this.id, this.objectName, this.parentId, this.lastChangeToken, true, this.creationDate, this.creationDate, expectedHash, content.Length);
            }
        }

        [Test, Category("Fast"), Category("Solver")]
        public void RemoteFileAddedAndExceptionOnModificationDateIsThrown() {
            var fileInfo = new Mock<IFileInfo>();
            var cacheFileInfo = this.fsFactory.SetupDownloadCacheFile();
            var parentDir = Mock.Of<IDirectoryInfo>(d => d.FullName == Path.GetTempPath());
            fileInfo.SetupAllProperties();
            fileInfo.Setup(f => f.FullName).Returns(this.path);
            fileInfo.Setup(f => f.Name).Returns(this.objectName);
            fileInfo.Setup(f => f.Directory).Returns(parentDir);
            fileInfo.SetupSet(f => f.LastWriteTimeUtc = It.IsAny<DateTime>()).Throws<IOException>();
            DateTime modification = DateTime.UtcNow;
            fileInfo.SetupGet(f => f.LastWriteTimeUtc).Returns(modification);
            byte[] content = Encoding.UTF8.GetBytes("content");
            byte[] expectedHash = SHA1Managed.Create().ComputeHash(content);
            cacheFileInfo.SetupAllProperties();
            cacheFileInfo.Setup(f => f.FullName).Returns(this.path + ".sync");
            cacheFileInfo.Setup(f => f.Name).Returns(this.objectName + ".sync");
            cacheFileInfo.Setup(f => f.Directory).Returns(parentDir);
            cacheFileInfo.Setup(f => f.IsExtendedAttributeAvailable()).Returns(true);
            using (var stream = new MemoryStream()) {
                cacheFileInfo.Setup(f => f.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)).Returns(() => {
                    cacheFileInfo.Setup(f => f.Exists).Returns(true);
                    return stream;
                });
                this.fsFactory.AddIFileInfo(cacheFileInfo.Object);

                Mock<IDocument> remoteObject = MockOfIDocumentUtil.CreateRemoteDocumentMock(null, this.id, this.objectName, this.parentId, content.Length, content, this.lastChangeToken);
                remoteObject.Setup(f => f.LastModificationDate).Returns((DateTime?)this.creationDate);

                this.underTest.Solve(fileInfo.Object, remoteObject.Object);

                cacheFileInfo.Verify(f => f.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None), Times.Once());
                cacheFileInfo.VerifySet(f => f.Uuid = It.Is<Guid?>(uuid => uuid != null && !uuid.Equals(Guid.Empty)), Times.Once());
                cacheFileInfo.Verify(f => f.MoveTo(this.path), Times.Once());
                fileInfo.VerifySet(d => d.LastWriteTimeUtc = It.Is<DateTime>(date => date.Equals(this.creationDate)), Times.Once());
                this.storage.VerifySavedMappedObject(MappedObjectType.File, this.id, this.objectName, this.parentId, this.lastChangeToken, true, modification, this.creationDate, expectedHash, content.Length);
            }
        }

        [Test, Category("Fast"), Category("Solver")]
        public void RemoteFileAddedAndLocalFileIsCreatedWhileDownloadIsInProgress() {
            string conflictPath = this.path + ".conflict";
            var conflictFileInfo = Mock.Of<IFileInfo>(i => i.FullName == conflictPath);
            var fileInfo = new Mock<IFileInfo>();
            var cacheFileInfo = this.fsFactory.SetupDownloadCacheFile();
            var parentDir = Mock.Of<IDirectoryInfo>(d => d.FullName == Path.GetTempPath());
            fileInfo.SetupAllProperties();
            fileInfo.Setup(f => f.FullName).Returns(this.path);
            fileInfo.Setup(f => f.Name).Returns(this.objectName);
            fileInfo.Setup(f => f.Directory).Returns(parentDir);
            fileInfo.Setup(f => f.Uuid).Returns(Guid.NewGuid());
            byte[] content = Encoding.UTF8.GetBytes("content");
            byte[] expectedHash = SHA1Managed.Create().ComputeHash(content);
            cacheFileInfo.SetupAllProperties();
            cacheFileInfo.Setup(f => f.FullName).Returns(this.path + ".sync");
            cacheFileInfo.Setup(f => f.Name).Returns(this.objectName + ".sync");
            cacheFileInfo.Setup(f => f.Directory).Returns(parentDir);
            cacheFileInfo.Setup(f => f.IsExtendedAttributeAvailable()).Returns(true);
            cacheFileInfo.Setup(f => f.Replace(fileInfo.Object, conflictFileInfo, true)).Returns(fileInfo.Object);
            using (var stream = new MemoryStream()) {
                cacheFileInfo.Setup(f => f.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)).Returns(() => {
                    cacheFileInfo.Setup(f => f.Exists).Returns(true);
                    return stream;
                });
                cacheFileInfo.Setup(f => f.MoveTo(this.path)).Callback(() => fileInfo.Setup(file => file.Refresh()).Callback(() => fileInfo.Setup(newFile => newFile.Exists).Returns(true))).Throws(new IOException());
                fileInfo.SetupStream(Encoding.UTF8.GetBytes("other content"));
                this.fsFactory.AddIFileInfo(cacheFileInfo.Object);
                this.fsFactory.Setup(f => f.CreateConflictFileInfo(fileInfo.Object)).Returns(conflictFileInfo);
                Mock<IDocument> remoteObject = MockOfIDocumentUtil.CreateRemoteDocumentMock(null, this.id, this.objectName, this.parentId, content.Length, content, this.lastChangeToken);
                remoteObject.Setup(f => f.LastModificationDate).Returns((DateTime?)this.creationDate);

                this.underTest.Solve(fileInfo.Object, remoteObject.Object);

                cacheFileInfo.Verify(f => f.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None), Times.Once());
                cacheFileInfo.VerifySet(f => f.Uuid = It.Is<Guid?>(uuid => uuid != null && !uuid.Equals(Guid.Empty)), Times.Once());
                cacheFileInfo.Verify(f => f.MoveTo(this.path), Times.Once());
                cacheFileInfo.Verify(f => f.Replace(fileInfo.Object, conflictFileInfo, true), Times.Once());
                fileInfo.VerifySet(d => d.LastWriteTimeUtc = It.Is<DateTime>(date => date.Equals(this.creationDate)), Times.Once());
                this.storage.VerifySavedMappedObject(MappedObjectType.File, this.id, this.objectName, this.parentId, this.lastChangeToken, true, this.creationDate, this.creationDate, expectedHash, content.Length);
                Mock.Get(conflictFileInfo).VerifySet(c => c.Uuid = null, Times.Once());
            }
        }

        [Test, Category("Fast"), Category("Solver")]
        public void ExceptionIsThrownIfAnAlreadySyncedFileShouldBeSyncedAgain() {
            Guid uuid = Guid.NewGuid();
            var file = Mock.Of<IFileInfo>(
                f =>
                f.Exists == true &&
                f.Uuid == uuid &&
                f.FullName == "path");
            this.storage.AddLocalFile(file.FullName, this.id, uuid);
            Assert.Throws<ArgumentException>(() => this.underTest.Solve(file, Mock.Of<IDocument>(d => d.Id == this.id)));
            this.storage.VerifyThatNoObjectIsManipulated();
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalAndRemoteFilesAreSavedAsEqualIfTheContentIsEqual() {
            var fileInfo = new Mock<IFileInfo>();
            var parentDir = Mock.Of<IDirectoryInfo>(d => d.FullName == Path.GetTempPath());
            fileInfo.SetupAllProperties();
            fileInfo.Setup(f => f.FullName).Returns(this.path);
            fileInfo.Setup(f => f.Name).Returns(this.objectName);
            fileInfo.Setup(f => f.Directory).Returns(parentDir);
            fileInfo.Setup(f => f.Exists).Returns(true);
            byte[] content = Encoding.UTF8.GetBytes("content");
            fileInfo.SetupStream(content);
            byte[] expectedHash = SHA1Managed.Create().ComputeHash(content);
            Mock<IDocument> remoteObject = MockOfIDocumentUtil.CreateRemoteDocumentMock(null, this.id, this.objectName, this.parentId, content.Length, content, this.lastChangeToken);
            remoteObject.SetupContentStreamHash(expectedHash);
            remoteObject.Setup(f => f.LastModificationDate).Returns((DateTime?)this.creationDate);

            this.underTest.Solve(fileInfo.Object, remoteObject.Object);

            fileInfo.VerifySet(f => f.Uuid = It.Is<Guid?>(uuid => uuid != null && !uuid.Equals(Guid.Empty)), Times.Once());
            this.storage.VerifySavedMappedObject(MappedObjectType.File, this.id, this.objectName, this.parentId, this.lastChangeToken, true, this.creationDate, this.creationDate, expectedHash, content.Length);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalAndRemoteFilesAreSavedAsEqualIfTheContentIsEqualAndNoContentStreamHashIsAvailable() {
            var fileInfo = new Mock<IFileInfo>();
            var cacheFileInfo = this.fsFactory.SetupDownloadCacheFile();
            var parentDir = Mock.Of<IDirectoryInfo>(d => d.FullName == Path.GetTempPath());
            fileInfo.SetupAllProperties();
            fileInfo.Setup(f => f.FullName).Returns(this.path);
            fileInfo.Setup(f => f.Name).Returns(this.objectName);
            fileInfo.Setup(f => f.Directory).Returns(parentDir);
            fileInfo.Setup(f => f.Exists).Returns(true);
            byte[] content = Encoding.UTF8.GetBytes("content");
            fileInfo.SetupStream(content);
            byte[] expectedHash = SHA1Managed.Create().ComputeHash(content);
            cacheFileInfo.SetupAllProperties();
            cacheFileInfo.Setup(f => f.FullName).Returns(this.path + ".sync");
            cacheFileInfo.Setup(f => f.Name).Returns(this.objectName + ".sync");
            cacheFileInfo.Setup(f => f.Directory).Returns(parentDir);
            cacheFileInfo.Setup(f => f.IsExtendedAttributeAvailable()).Returns(true);
            cacheFileInfo.Setup(f => f.MoveTo(this.path)).Throws<IOException>();
            cacheFileInfo.Setup(f => f.Delete()).Callback(() => cacheFileInfo.Setup(f => f.Exists).Returns(false));
            using (var stream = new MemoryStream()) {
                cacheFileInfo.Setup(f => f.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)).Returns(() => {
                    cacheFileInfo.Setup(f => f.Exists).Returns(true);
                    return stream;
                });
                this.fsFactory.AddIFileInfo(cacheFileInfo.Object);

                Mock<IDocument> remoteObject = MockOfIDocumentUtil.CreateRemoteDocumentMock(null, this.id, this.objectName, this.parentId, content.Length, content, this.lastChangeToken);
                remoteObject.Setup(f => f.LastModificationDate).Returns((DateTime?)this.creationDate);

                this.underTest.Solve(fileInfo.Object, remoteObject.Object);

                cacheFileInfo.Verify(f => f.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None), Times.Once());
                fileInfo.VerifySet(f => f.Uuid = It.Is<Guid?>(uuid => uuid != null && !uuid.Equals(Guid.Empty)), Times.Once());
                cacheFileInfo.Verify(f => f.MoveTo(this.path), Times.Once());
                cacheFileInfo.Verify(f => f.Delete(), Times.Once());
                fileInfo.VerifySet(d => d.LastWriteTimeUtc = It.Is<DateTime>(date => date.Equals(this.creationDate)), Times.Once());
                this.storage.VerifySavedMappedObject(MappedObjectType.File, this.id, this.objectName, this.parentId, this.lastChangeToken, true, this.creationDate, this.creationDate, expectedHash, content.Length);
            }
        }

        [Test, Category("Fast"), Category("Solver")]
        public void AbortThePreviousDownloadAndContinueTheNextDownload() {
            var fileInfo = new Mock<IFileInfo>();
            var cacheFileInfo = this.fsFactory.SetupDownloadCacheFile();
            byte[] content = new byte[1024 * 1024];
            long length = 0;
            this.SetupToAbortThePreviousDownload(fileInfo, cacheFileInfo, content, out length);

            Mock<MemoryStream> stream = new Mock<MemoryStream>();
            stream.SetupAllProperties();
            stream.Setup(f => f.CanWrite).Returns(true);    // required for System.Security.Cryptography.CryptoStream
            stream.Setup(f => f.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Callback((byte[] buffer, int offset, int count) => length += count);
            stream.Setup(f => f.Length).Returns(() => { return length; });

            long lengthRead = 0;
            stream.Setup(f => f.Seek(It.IsAny<long>(), It.IsAny<SeekOrigin>())).Callback((long offset, SeekOrigin loc) => lengthRead = offset);
            stream.Setup(f => f.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns((byte[] buffer, int offset, int count) => {
                if (lengthRead >= length) {
                    return 0;
                }

                int countRead = count;
                if (countRead > (length - lengthRead)) {
                    countRead = (int)(length - lengthRead);
                }

                Array.Copy(content, lengthRead, buffer, offset, countRead);
                lengthRead += countRead;
                stream.Object.Position = lengthRead;
                return countRead;
            });

            cacheFileInfo.Setup(f => f.Open(It.IsAny<FileMode>(), It.IsAny<FileAccess>(), It.IsAny<FileShare>())).Returns(stream.Object);

            Mock<IDocument> remoteObject = MockOfIDocumentUtil.CreateRemoteDocumentMock(null, this.id, this.objectName, this.parentId, content.Length, content, this.lastChangeToken);
            remoteObject.Setup(f => f.LastModificationDate).Returns((DateTime?)this.creationDate);

            this.underTest.Solve(fileInfo.Object, remoteObject.Object);

            Assert.That(length, Is.EqualTo(content.Length));
            stream.Verify(f => f.Seek(0, SeekOrigin.Begin), Times.Once());
            cacheFileInfo.Verify(f => f.Open(It.IsAny<FileMode>(), It.IsAny<FileAccess>(), It.IsAny<FileShare>()), Times.Exactly(3));   // first open in SetupToAbortThePreviousDownload, second open to validate checksum, third open to download
            cacheFileInfo.VerifySet(f => f.Uuid = It.Is<Guid?>(uuid => uuid != null && !uuid.Equals(Guid.Empty)), Times.Once());
            cacheFileInfo.Verify(f => f.MoveTo(this.path), Times.Once());
            fileInfo.VerifySet(d => d.LastWriteTimeUtc = It.Is<DateTime>(date => date.Equals(this.creationDate)), Times.Once());
            this.storage.VerifySavedMappedObject(MappedObjectType.File, this.id, this.objectName, this.parentId, this.lastChangeToken, true, this.creationDate, this.creationDate, SHA1Managed.Create().ComputeHash(content), content.Length);
            this.transmissionStorage.Verify(f => f.GetObjectByRemoteObjectId(this.id), Times.Exactly(2));
            this.transmissionStorage.Verify(f => f.SaveObject(It.IsAny<IFileTransmissionObject>()), Times.AtLeastOnce());
            this.transmissionStorage.Verify(f => f.RemoveObjectByRemoteObjectId(this.id), Times.Once());
        }

        [Test, Category("Fast"), Category("Solver")]
        public void AbortThePreviousDownloadAndChangeTheRemoteFileAndContinueTheNextDownload() {
            var fileInfo = new Mock<IFileInfo>();
            var cacheFileInfo = this.fsFactory.SetupDownloadCacheFile();
            byte[] content = new byte[1024 * 1024];
            long lengthPrev = 0;
            this.SetupToAbortThePreviousDownload(fileInfo, cacheFileInfo, content, out lengthPrev);

            Mock<MemoryStream> stream = new Mock<MemoryStream>();
            stream.SetupAllProperties();
            stream.Setup(f => f.CanWrite).Returns(true);    // required for System.Security.Cryptography.CryptoStream
            long length = 0;
            stream.Setup(f => f.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Callback((byte[] buffer, int offset, int count) => length += count);
            stream.Setup(f => f.Length).Returns(() => { return length; });
            stream.Setup(f => f.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns((byte[] buffer, int offset, int count) => count);
            cacheFileInfo.Setup(f => f.Delete()).Callback(() => {
                stream.Setup(f => f.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns(0);
                cacheFileInfo.Setup(f => f.Exists).Returns(false);
            });
            cacheFileInfo.Setup(f => f.Open(It.IsAny<FileMode>(), It.IsAny<FileAccess>(), It.IsAny<FileShare>())).Returns(() => {
                cacheFileInfo.Setup(f => f.Exists).Returns(true);
                return stream.Object;
            });

            string newLastChangeToken = this.lastChangeToken + ".change";
            Mock<IDocument> remoteObject = MockOfIDocumentUtil.CreateRemoteDocumentMock(null, this.id, this.objectName, this.parentId, content.Length, content, newLastChangeToken);
            remoteObject.Setup(f => f.LastModificationDate).Returns((DateTime?)this.creationDate);

            this.underTest.Solve(fileInfo.Object, remoteObject.Object);

            Assert.That(length, Is.EqualTo(content.Length));
            stream.Verify(f => f.Seek(0, SeekOrigin.Begin), Times.Never());
            cacheFileInfo.Verify(f => f.Delete(), Times.Once());
            cacheFileInfo.Verify(f => f.Open(It.IsAny<FileMode>(), It.IsAny<FileAccess>(), It.IsAny<FileShare>()), Times.Exactly(2));   // first open in SetupToAbortThePreviousDownload, second open to download
            cacheFileInfo.VerifySet(f => f.Uuid = It.Is<Guid?>(uuid => uuid != null && !uuid.Equals(Guid.Empty)), Times.Once());
            cacheFileInfo.Verify(f => f.MoveTo(this.path), Times.Once());
            fileInfo.VerifySet(d => d.LastWriteTimeUtc = It.Is<DateTime>(date => date.Equals(this.creationDate)), Times.Once());
            this.storage.VerifySavedMappedObject(MappedObjectType.File, this.id, this.objectName, this.parentId, newLastChangeToken, true, this.creationDate, this.creationDate, SHA1Managed.Create().ComputeHash(content), content.Length);
            this.transmissionStorage.Verify(f => f.GetObjectByRemoteObjectId(this.id), Times.Exactly(2));
            this.transmissionStorage.Verify(f => f.SaveObject(It.IsAny<IFileTransmissionObject>()), Times.AtLeastOnce());
            this.transmissionStorage.Verify(f => f.RemoveObjectByRemoteObjectId(this.id), Times.Once());
        }

        [Test, Category("Fast"), Category("Solver")]
        public void AbortThePreviousDownloadAndChangeTheLocalCacheFileAndContinueTheNextDownload() {
            var fileInfo = new Mock<IFileInfo>();
            var cacheFileInfo = this.fsFactory.SetupDownloadCacheFile();
            byte[] content = new byte[1024 * 1024];
            long lengthPrev = 0;
            this.SetupToAbortThePreviousDownload(fileInfo, cacheFileInfo, content, out lengthPrev);

            Mock<MemoryStream> stream = new Mock<MemoryStream>();
            stream.SetupAllProperties();
            stream.Setup(f => f.CanWrite).Returns(true);    // required for System.Security.Cryptography.CryptoStream

            long lengthRead = 0;
            stream.Setup(f => f.Seek(It.IsAny<long>(), It.IsAny<SeekOrigin>())).Callback((long offset, SeekOrigin loc) => lengthRead = offset);
            stream.Setup(f => f.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns((byte[] buffer, int offset, int count) => {
                if (lengthRead >= lengthPrev) {
                    return 0;
                }

                int countRead = count;
                if (countRead > (lengthPrev - lengthRead)) {
                    countRead = (int)(lengthPrev - lengthRead);
                }

                Array.Copy(content, lengthRead, buffer, 0, countRead);

                // change the first byte
                if (buffer[0] == (byte)0) {
                    buffer[0] = (byte)1;
                } else {
                    buffer[0] = (byte)0;
                }

                lengthRead += countRead;
                stream.Object.Position = lengthRead;
                return countRead;
            });

            long length = 0;
            stream.Setup(f => f.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Callback((byte[] buffer, int offset, int count) => length += count);
            stream.Setup(f => f.Length).Returns(() => { return length; });

            cacheFileInfo.Setup(f => f.Delete()).Callback(() => {
                stream.Setup(f => f.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns(0);
                stream.Object.Position = 0;
                cacheFileInfo.Setup(f => f.Exists).Returns(false);
            });
            cacheFileInfo.Setup(f => f.Open(It.IsAny<FileMode>(), It.IsAny<FileAccess>(), It.IsAny<FileShare>())).Returns(() => {
                cacheFileInfo.Setup(f => f.Exists).Returns(true);
                return stream.Object;
            });

            Mock<IDocument> remoteObject = MockOfIDocumentUtil.CreateRemoteDocumentMock(null, this.id, this.objectName, this.parentId, content.Length, content, this.lastChangeToken);
            remoteObject.Setup(f => f.LastModificationDate).Returns((DateTime?)this.creationDate);

            this.underTest.Solve(fileInfo.Object, remoteObject.Object);

            Assert.That(length, Is.EqualTo(content.Length));
            stream.Verify(f => f.Seek(0, SeekOrigin.Begin), Times.Never());
            cacheFileInfo.Verify(f => f.Open(It.IsAny<FileMode>(), It.IsAny<FileAccess>(), It.IsAny<FileShare>()), Times.Exactly(3));   // first open in SetupToAbortThePreviousDownload, second open to validate checksum, third open to download
            cacheFileInfo.Verify(f => f.Delete(), Times.Once());
            cacheFileInfo.VerifySet(f => f.Uuid = It.Is<Guid?>(uuid => uuid != null && !uuid.Equals(Guid.Empty)), Times.Once());
            cacheFileInfo.Verify(f => f.MoveTo(this.path), Times.Once());
            fileInfo.VerifySet(d => d.LastWriteTimeUtc = It.Is<DateTime>(date => date.Equals(this.creationDate)), Times.Once());
            this.storage.VerifySavedMappedObject(MappedObjectType.File, this.id, this.objectName, this.parentId, this.lastChangeToken, true, this.creationDate, this.creationDate, SHA1Managed.Create().ComputeHash(content), content.Length);
            this.transmissionStorage.Verify(f => f.GetObjectByRemoteObjectId(this.id), Times.Exactly(2));
            this.transmissionStorage.Verify(f => f.SaveObject(It.IsAny<IFileTransmissionObject>()), Times.AtLeastOnce());
            this.transmissionStorage.Verify(f => f.RemoveObjectByRemoteObjectId(this.id), Times.Once());
        }

        [Test, Category("Fast"), Category("Solver")]
        public void AbortThePreviousDownloadAndDeleteTheLocalCacheFileAndContinueTheNextDownload() {
            var fileInfo = new Mock<IFileInfo>();
            var cacheFileInfo = this.fsFactory.SetupDownloadCacheFile();
            byte[] content = new byte[1024 * 1024];
            long lengthPrev = 0;
            this.SetupToAbortThePreviousDownload(fileInfo, cacheFileInfo, content, out lengthPrev);

            cacheFileInfo.Setup(f => f.Exists).Returns(false);

            Mock<MemoryStream> stream = new Mock<MemoryStream>();
            stream.SetupAllProperties();
            stream.Setup(f => f.CanWrite).Returns(true);    // required for System.Security.Cryptography.CryptoStream

            long length = 0;
            stream.Setup(f => f.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Callback((byte[] buffer, int offset, int count) => length += count);

            cacheFileInfo.Setup(f => f.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)).Returns(() => {
                cacheFileInfo.Setup(f => f.Exists).Returns(true);
                return stream.Object;
            });

            Mock<IDocument> remoteObject = MockOfIDocumentUtil.CreateRemoteDocumentMock(null, this.id, this.objectName, this.parentId, content.Length, content, this.lastChangeToken);
            remoteObject.Setup(f => f.LastModificationDate).Returns((DateTime?)this.creationDate);

            this.underTest.Solve(fileInfo.Object, remoteObject.Object);

            Assert.That(length, Is.EqualTo(content.Length));
            cacheFileInfo.Verify(f => f.Open(It.IsAny<FileMode>(), It.IsAny<FileAccess>(), It.IsAny<FileShare>()), Times.Exactly(2));   // first open in SetupToAbortThePreviousDownload, second open to download
            cacheFileInfo.VerifySet(f => f.Uuid = It.Is<Guid?>(uuid => uuid != null && !uuid.Equals(Guid.Empty)), Times.Once());
            cacheFileInfo.Verify(f => f.MoveTo(this.path), Times.Once());
            fileInfo.VerifySet(d => d.LastWriteTimeUtc = It.Is<DateTime>(date => date.Equals(this.creationDate)), Times.Once());
            this.storage.VerifySavedMappedObject(MappedObjectType.File, this.id, this.objectName, this.parentId, this.lastChangeToken, true, this.creationDate, this.creationDate, SHA1Managed.Create().ComputeHash(content), content.Length);
            this.transmissionStorage.Verify(f => f.GetObjectByRemoteObjectId(this.id), Times.Exactly(2));
            this.transmissionStorage.Verify(f => f.SaveObject(It.IsAny<IFileTransmissionObject>()), Times.AtLeastOnce());
            this.transmissionStorage.Verify(f => f.RemoveObjectByRemoteObjectId(this.id), Times.Once());
        }

        private void SetupToAbortThePreviousDownload(Mock<IFileInfo> fileInfo, Mock<IFileInfo> cacheFileInfo, byte[] content, out long lengthWrite) {
            var parentDir = Mock.Of<IDirectoryInfo>(d => d.FullName == Path.GetTempPath());

            fileInfo.SetupAllProperties();
            fileInfo.Setup(f => f.FullName).Returns(this.path);
            fileInfo.Setup(f => f.Name).Returns(this.objectName);
            fileInfo.Setup(f => f.Directory).Returns(parentDir);

            cacheFileInfo.SetupAllProperties();
            cacheFileInfo.Setup(f => f.FullName).Returns(this.path + ".sync");
            cacheFileInfo.Setup(f => f.Name).Returns(this.objectName + ".sync");
            cacheFileInfo.Setup(f => f.Directory).Returns(parentDir);
            cacheFileInfo.Setup(f => f.IsExtendedAttributeAvailable()).Returns(true);
            this.fsFactory.AddIFileInfo(cacheFileInfo.Object);

            this.transmissionStorage.Setup(f => f.SaveObject(It.IsAny<IFileTransmissionObject>())).Callback<IFileTransmissionObject>((o) => {
                this.transmissionStorage.Setup(f => f.GetObjectByRemoteObjectId(It.IsAny<string>())).Returns(o);
            });
            this.transmissionStorage.Setup(f => f.RemoveObjectByRemoteObjectId(It.IsAny<string>())).Callback(() => {
                this.transmissionStorage.Setup(f => f.GetObjectByRemoteObjectId(It.IsAny<string>())).Returns((IFileTransmissionObject)null);
            });

            var stream = new Mock<MemoryStream>();
            stream.SetupAllProperties();
            stream.Setup(f => f.CanWrite).Returns(true);    // required for System.Security.Cryptography.CryptoStream

            long length = 0;
            stream.Setup(f => f.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Callback((byte[] buffer, int offset, int count) => {
                if (length > 0) {
                    foreach (FileTransmissionEvent transmissionEvent in this.manager.ActiveTransmissions) {
                        transmissionEvent.ReportProgress(new TransmissionProgressEventArgs { Aborting = true });
                    }
                }

                length += count;
            });

            cacheFileInfo.Setup(f => f.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)).Returns(() => {
                cacheFileInfo.Setup(f => f.Exists).Returns(true);
                return stream.Object;
            });

            Mock<IDocument> remoteObject = MockOfIDocumentUtil.CreateRemoteDocumentMock(null, this.id, this.objectName, this.parentId, content.Length, content, this.lastChangeToken);
            remoteObject.Setup(f => f.LastModificationDate).Returns((DateTime?)this.creationDate);

            Assert.Throws<AbortException>(() => this.underTest.Solve(fileInfo.Object, remoteObject.Object));

            cacheFileInfo.Verify(f => f.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None), Times.Once());
            cacheFileInfo.VerifySet(f => f.Uuid = It.Is<Guid?>(uuid => uuid != null && !uuid.Equals(Guid.Empty)), Times.Never());
            cacheFileInfo.Verify(f => f.MoveTo(this.path), Times.Never());
            fileInfo.VerifySet(d => d.LastWriteTimeUtc = It.Is<DateTime>(date => date.Equals(this.creationDate)), Times.Never());
            this.transmissionStorage.Verify(f => f.GetObjectByRemoteObjectId(It.IsAny<string>()), Times.Once());
            this.transmissionStorage.Verify(f => f.SaveObject(It.IsAny<IFileTransmissionObject>()), Times.AtLeastOnce());
            this.transmissionStorage.Verify(f => f.RemoveObjectByRemoteObjectId(It.IsAny<string>()), Times.Never());
            this.storage.Verify(f => f.SaveMappedObject(It.IsAny<IMappedObject>()), Times.Never());

            lengthWrite = length;
        }
    }
}