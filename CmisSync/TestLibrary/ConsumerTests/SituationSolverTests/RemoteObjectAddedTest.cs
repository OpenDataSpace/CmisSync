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

namespace TestLibrary.ConsumerTests.SituationSolverTests
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;

    using CmisSync.Lib.FileTransmission;
    using CmisSync.Lib.Consumer.SituationSolver;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;

    using Moq;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture]
    public class RemoteObjectAddedTest
    {
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
        public void SetUp()
        {
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
        public void ConstructorTakesQueue()
        {
            new RemoteObjectAdded(this.session.Object, this.storage.Object, null, this.manager);
        }

        [Test, Category("Fast"), Category("Solver")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorThrowsExceptionIfTransmissionManagerIsNull() {
            new RemoteObjectAdded(this.session.Object, this.storage.Object, null, null);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void RemoteFolderAdded()
        {
            var dirInfo = new Mock<IDirectoryInfo>();
            dirInfo.SetupAllProperties();
            dirInfo.Setup(d => d.FullName).Returns(this.path);
            dirInfo.Setup(d => d.Name).Returns(this.objectName);
            dirInfo.Setup(d => d.Parent).Returns(Mock.Of<IDirectoryInfo>());
            dirInfo.Setup(d => d.IsExtendedAttributeAvailable()).Returns(false);

            Mock<IFolder> remoteObject = MockOfIFolderUtil.CreateRemoteFolderMock(this.id, this.objectName, this.path, this.parentId, this.lastChangeToken);
            remoteObject.Setup(f => f.LastModificationDate).Returns((DateTime?)this.creationDate);

            this.underTest.Solve(dirInfo.Object, remoteObject.Object);

            dirInfo.Verify(d => d.Create(), Times.Once());
            this.storage.VerifySavedMappedObject(MappedObjectType.Folder, this.id, this.objectName, this.parentId, this.lastChangeToken, false, this.creationDate);
            dirInfo.VerifySet(d => d.LastWriteTimeUtc = It.Is<DateTime>(date => date.Equals(this.creationDate)), Times.Once());
            dirInfo.VerifySet(d => d.Uuid = It.IsAny<Guid?>(), Times.Never());
        }

        [Test, Category("Fast"), Category("Solver")]
        public void RemoteFolderAddedAndExtendedAttributesAreAvailable()
        {
            var dirInfo = new Mock<IDirectoryInfo>();
            dirInfo.SetupAllProperties();
            dirInfo.Setup(d => d.FullName).Returns(this.path);
            dirInfo.Setup(d => d.Name).Returns(this.objectName);
            dirInfo.Setup(d => d.Parent).Returns(Mock.Of<IDirectoryInfo>());
            dirInfo.Setup(d => d.IsExtendedAttributeAvailable()).Returns(true);

            Mock<IFolder> remoteObject = MockOfIFolderUtil.CreateRemoteFolderMock(this.id, this.objectName, this.path, this.parentId, this.lastChangeToken);
            remoteObject.Setup(f => f.LastModificationDate).Returns((DateTime?)this.creationDate);

            this.underTest.Solve(dirInfo.Object, remoteObject.Object);
            dirInfo.Verify(d => d.Create(), Times.Once());
            this.storage.VerifySavedMappedObject(MappedObjectType.Folder, this.id, this.objectName, this.parentId, this.lastChangeToken, true, this.creationDate);
            dirInfo.VerifySet(d => d.LastWriteTimeUtc = It.Is<DateTime>(date => date.Equals(this.creationDate)), Times.Once());
            dirInfo.VerifySet(d => d.Uuid = It.Is<Guid?>(uuid => uuid != null && !uuid.Equals(Guid.Empty)), Times.Once());
        }

        [Test, Category("Fast"), Category("Solver")]
        public void RemoteFileAddedAndExtendedAttributesAreAvailable()
        {
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
                cacheFileInfo.Setup(f => f.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)).Returns(stream);
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
        public void RemoteFileAddedAndExceptionOnModificationDateIsThrown()
        {
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
                cacheFileInfo.Setup(f => f.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)).Returns(stream);
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
        public void RemoteFileAddedAndLocalFileIsCreatedWhileDownloadIsInProgress()
        {
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
                cacheFileInfo.Setup(f => f.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)).Returns(stream).Callback(() => fileInfo.Setup(f => f.Exists).Returns(true));
                cacheFileInfo.Setup(f => f.MoveTo(this.path)).Throws(new IOException());
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
            using (var stream = new MemoryStream()) {
                cacheFileInfo.Setup(f => f.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)).Returns(stream);
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
        public void AbortThePreviousDownloadAndContinueTheNextDownload()
        {
            var parentDir = Mock.Of<IDirectoryInfo>(d => d.FullName == Path.GetTempPath());

            var fileInfo = new Mock<IFileInfo>();
            fileInfo.SetupAllProperties();
            fileInfo.Setup(f => f.FullName).Returns(this.path);
            fileInfo.Setup(f => f.Name).Returns(this.objectName);
            fileInfo.Setup(f => f.Directory).Returns(parentDir);

            var cacheFileInfo = this.fsFactory.SetupDownloadCacheFile();
            cacheFileInfo.SetupAllProperties();
            cacheFileInfo.Setup(f => f.FullName).Returns(this.path + ".sync");
            cacheFileInfo.Setup(f => f.Name).Returns(this.objectName + ".sync");
            cacheFileInfo.Setup(f => f.Directory).Returns(parentDir);
            cacheFileInfo.Setup(f => f.IsExtendedAttributeAvailable()).Returns(true);
            cacheFileInfo.Setup(f => f.Exists).Returns(true);
            this.fsFactory.AddIFileInfo(cacheFileInfo.Object);

            var streamPrev = new Mock<MemoryStream>();
            streamPrev.SetupAllProperties();
            streamPrev.Setup(f => f.CanWrite).Returns(true);

            const int contentLength = 1024 * 1024;
            byte[] content = new byte[contentLength];
            byte[] expectedHash = SHA1Managed.Create().ComputeHash(content);

            long length = 0;

            bool first = true;
            streamPrev.Setup(f => f.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Callback((byte[] buffer, int offset, int count) => {
                if (first) {
                    IFileTransmissionObject transmissionObject = null;
                    this.transmissionStorage.Setup(f => f.SaveObject(It.IsAny<IFileTransmissionObject>())).Callback<IFileTransmissionObject>((o) => {
                        transmissionObject = o;
                    });
                    this.transmissionStorage.Setup(f => f.GetObjectByRemoteObjectId(It.IsAny<string>())).Returns(transmissionObject);
                    foreach (FileTransmissionEvent transmissionEvent in this.manager.ActiveTransmissions) {
                        transmissionEvent.ReportProgress(new TransmissionProgressEventArgs { Aborting = true });
                    }
                }
                first = false;
                length += count;
            });

            cacheFileInfo.Setup(f => f.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)).Returns(streamPrev.Object);
            Mock<IDocument> remoteObject = MockOfIDocumentUtil.CreateRemoteDocumentMock(null, this.id, this.objectName, this.parentId, content.Length, content, this.lastChangeToken);
            remoteObject.Setup(f => f.LastModificationDate).Returns((DateTime?)this.creationDate);
            Assert.Throws<AbortException>(() => this.underTest.Solve(fileInfo.Object, remoteObject.Object));

            cacheFileInfo.Verify(f => f.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None), Times.Once());
            cacheFileInfo.VerifySet(f => f.Uuid = It.Is<Guid?>(uuid => uuid != null && !uuid.Equals(Guid.Empty)), Times.Never());
            cacheFileInfo.Verify(f => f.MoveTo(this.path), Times.Never());
            fileInfo.VerifySet(d => d.LastWriteTimeUtc = It.Is<DateTime>(date => date.Equals(this.creationDate)), Times.Never());
            this.storage.VerifySavedMappedObject(MappedObjectType.File, this.id, this.objectName, this.parentId, this.lastChangeToken, Times.Never(), true, this.creationDate, this.creationDate, expectedHash, content.Length);

            var streamNext = new Mock<MemoryStream>();
            streamNext.SetupAllProperties();
            streamNext.Setup(f => f.CanRead).Returns(true);
            streamNext.Setup(f => f.CanWrite).Returns(true);

            streamNext.Setup(f => f.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Callback((byte[] buffer, int offset, int count) => length += count);
            long lengthRead = 0;
            streamNext.Setup(f => f.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns((byte[] buffer, int offset, int count) => {
                if (lengthRead >= length) {
                    return 0;
                }
                int countRead = count;
                if (countRead > (length - lengthRead)) {
                    countRead = (int)(length - lengthRead);
                }
                Array.Copy(content, lengthRead, buffer, 0, countRead);
                lengthRead += countRead;
                streamNext.Object.Position = lengthRead;
                return countRead;
            });

            cacheFileInfo.Setup(f => f.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)).Returns(streamNext.Object);
            remoteObject = MockOfIDocumentUtil.CreateRemoteDocumentMock(null, this.id, this.objectName, this.parentId, content.Length, content, this.lastChangeToken);
            remoteObject.Setup(f => f.LastModificationDate).Returns((DateTime?)this.creationDate);
            this.underTest.Solve(fileInfo.Object, remoteObject.Object);

            cacheFileInfo.Verify(f => f.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None), Times.Exactly(2));
            cacheFileInfo.VerifySet(f => f.Uuid = It.Is<Guid?>(uuid => uuid != null && !uuid.Equals(Guid.Empty)), Times.Once());
            cacheFileInfo.Verify(f => f.MoveTo(this.path), Times.Once());
            fileInfo.VerifySet(d => d.LastWriteTimeUtc = It.Is<DateTime>(date => date.Equals(this.creationDate)), Times.Once());
            this.storage.VerifySavedMappedObject(MappedObjectType.File, this.id, this.objectName, this.parentId, this.lastChangeToken, true, this.creationDate, this.creationDate, expectedHash, content.Length);
            Assert.That(length, Is.EqualTo(contentLength));
        }


    }
}
