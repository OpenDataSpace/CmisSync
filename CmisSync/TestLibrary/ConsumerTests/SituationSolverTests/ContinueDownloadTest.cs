//-----------------------------------------------------------------------
// <copyright file="ContinueDownloadTest.cs" company="GRAU DATA AG">
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
    using System.Collections.Generic;
    using System.IO;
    using System.Security.Cryptography;

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
    public class ContinueDownloadTest : IsTestWithConfiguredLog4Net {
        private readonly DateTime creationDate = DateTime.UtcNow;
        private readonly string objectName = "objectName";
        private readonly string objectId = "objectId";
        private readonly string parentId = "parentId";
        private readonly string changeToken = "changeToken";
        private readonly long chunkSize = 8 * 1024;
        private readonly int chunkCount = 4;
        private readonly byte[] emptyHash = SHA1.Create().ComputeHash(new byte[0]);

        private Mock<ISession> session;
        private Mock<IFileSystemInfoFactory> fsFactory;
        private Mock<IMetaDataStorage> storage;
        private Mock<IFileTransmissionStorage> transmissionStorage;
        private ActiveActivitiesManager transmissionManager;

        private string parentPath;
        private string localPath;
        private byte[] fileContentOld;
        private byte[] fileHashOld;
        private byte[] fileContent;
        private byte[] fileHash;
        private Mock<IFileInfo> localFile;
        private long localFileLength;
        private Mock<IFileInfo> cacheFile;
        private Mock<IFileInfo> backupFile;

        [SetUp]
        public void SetUp() {
            this.session = new Mock<ISession>();
            this.session.SetupTypeSystem();
            this.fsFactory = new Mock<IFileSystemInfoFactory>(MockBehavior.Strict);
            this.storage = new Mock<IMetaDataStorage>();
            this.transmissionStorage = new Mock<IFileTransmissionStorage>();
            this.transmissionManager = new ActiveActivitiesManager();

            this.transmissionStorage.Setup(f => f.SaveObject(It.IsAny<IFileTransmissionObject>())).Callback<IFileTransmissionObject>((o) => {
                this.transmissionStorage.Setup(f => f.GetObjectByRemoteObjectId(It.IsAny<string>())).Returns(o);
            });
            this.transmissionStorage.Setup(f => f.RemoveObjectByRemoteObjectId(It.IsAny<string>())).Callback(() => {
                this.transmissionStorage.Setup(f => f.GetObjectByRemoteObjectId(It.IsAny<string>())).Returns((IFileTransmissionObject)null);
            });

            this.parentPath = Path.GetTempPath();
            this.localPath = Path.Combine(this.parentPath, this.objectName);
            this.fileContentOld = new byte[this.chunkCount * this.chunkSize];
            this.fileContentOld[0] = 0;
            this.fileHashOld = SHA1.Create().ComputeHash(this.fileContentOld);
            this.fileContent = new byte[this.chunkCount * this.chunkSize];
            this.fileContent[0] = 1;
            this.fileHash = SHA1.Create().ComputeHash(this.fileContent);

            var parentDir = Mock.Of<IDirectoryInfo>(d => d.FullName == this.parentPath && d.Name == Path.GetFileName(this.parentPath));
            this.localFile = Mock.Get(Mock.Of<IFileInfo>(
                f =>
                f.FullName == this.localPath &&
                f.Name == this.objectName &&
                f.Directory == parentDir));
            this.localFileLength = 0;

            this.cacheFile = this.fsFactory.SetupDownloadCacheFile();
            this.cacheFile.SetupAllProperties();
            this.cacheFile.Setup(f => f.FullName).Returns(this.localPath + ".sync");
            this.cacheFile.Setup(f => f.Name).Returns(this.objectName + ".sync");
            this.cacheFile.Setup(f => f.Directory).Returns(parentDir);
            this.cacheFile.Setup(f => f.IsExtendedAttributeAvailable()).Returns(true);
            this.fsFactory.AddIFileInfo(this.cacheFile.Object);

            this.backupFile = new Mock<IFileInfo>();
        }

        [Test, Category("Fast"), Category("Solver")]
        public void RemoteFileAdded() {
            var solver = new RemoteObjectAdded(this.session.Object, this.storage.Object, this.transmissionStorage.Object, this.transmissionManager, this.fsFactory.Object);

            this.RunSolverToAbortDownload(solver);

            this.RunSolverToContinueDownload(solver);
            this.cacheFile.VerifySet(f => f.Uuid = It.Is<Guid?>(uuid => uuid != null && !uuid.Equals(Guid.Empty)), Times.Once());
            this.cacheFile.Verify(f => f.MoveTo(this.localPath), Times.Once());
        }

        [Test, Category("Fast"), Category("Solver")]
        public void RemoteFileChanged() {
            this.SetupRemoteFileChanged();

            var solver = new RemoteObjectChanged(this.session.Object, this.storage.Object, this.transmissionStorage.Object, this.transmissionManager, this.fsFactory.Object);

            this.RunSolverToAbortDownload(solver, remoteContent: ContentChangeType.CHANGED);

            this.RunSolverToContinueDownload(solver, remoteContent: ContentChangeType.CHANGED);
            this.cacheFile.Verify(c => c.Replace(this.localFile.Object, this.backupFile.Object, true), Times.Once());
            this.backupFile.Verify(b => b.Delete(), Times.Once());
        }

        [Test, Category("Fast"), Category("Solver")]
        public void RemoteFileAddedWhileChangeLocalCacheBeforeContinue() {
            var solver = new RemoteObjectAdded(this.session.Object, this.storage.Object, this.transmissionStorage.Object, this.transmissionManager, this.fsFactory.Object);

            this.RunSolverToAbortDownload(solver);

            this.RunSolverToChangeLocalCacheBeforeContinue(solver);
            this.cacheFile.VerifySet(f => f.Uuid = It.Is<Guid?>(uuid => uuid != null && !uuid.Equals(Guid.Empty)), Times.Once());
            this.cacheFile.Verify(f => f.MoveTo(this.localPath), Times.Once());
        }

        [Test, Category("Fast"), Category("Solver")]
        public void RemoteFileChangedWhileChangeLocalCacheBeforeContinue() {
            this.SetupRemoteFileChanged();

            var solver = new RemoteObjectChanged(this.session.Object, this.storage.Object, this.transmissionStorage.Object, this.transmissionManager, this.fsFactory.Object);

            this.RunSolverToAbortDownload(solver, remoteContent: ContentChangeType.CHANGED);

            this.RunSolverToChangeLocalCacheBeforeContinue(solver, remoteContent: ContentChangeType.CHANGED);
            this.cacheFile.Verify(c => c.Replace(this.localFile.Object, this.backupFile.Object, true), Times.Once());
            this.backupFile.Verify(b => b.Delete(), Times.Once());
        }

        [Test, Category("Fast"), Category("Solver")]
        public void RemoteFileAddedWhileChangeRemoteBeforeContinue() {
            var solver = new RemoteObjectAdded(this.session.Object, this.storage.Object, this.transmissionStorage.Object, this.transmissionManager, this.fsFactory.Object);

            this.RunSolverToAbortDownload(solver);

            this.RunSolverToChangeRemoteBeforeContinue(solver);
            this.cacheFile.VerifySet(f => f.Uuid = It.Is<Guid?>(uuid => uuid != null && !uuid.Equals(Guid.Empty)), Times.Once());
            this.cacheFile.Verify(f => f.MoveTo(this.localPath), Times.Once());
        }

        [Test, Category("Fast"), Category("Solver")]
        public void RemoteFileChangedWhileChangeRemoteBeforeContinue() {
            this.SetupRemoteFileChanged();

            var solver = new RemoteObjectChanged(this.session.Object, this.storage.Object, this.transmissionStorage.Object, this.transmissionManager, this.fsFactory.Object);

            this.RunSolverToAbortDownload(solver, remoteContent: ContentChangeType.CHANGED);

            this.RunSolverToChangeRemoteBeforeContinue(solver, remoteContent: ContentChangeType.CHANGED);
            this.cacheFile.Verify(c => c.Replace(this.localFile.Object, this.backupFile.Object, true), Times.Once());
            this.backupFile.Verify(b => b.Delete(), Times.Once());
        }

        [Test, Category("Fast"), Category("Solver")]
        public void RemoteFileAddedWhileDeleteLocalCacheBeforeContinue() {
            var solver = new RemoteObjectAdded(this.session.Object, this.storage.Object, this.transmissionStorage.Object, this.transmissionManager, this.fsFactory.Object);

            this.RunSolverToAbortDownload(solver);
            this.RunSolverToDeleteLocalCacheBeforeContinue(solver);
            this.cacheFile.VerifySet(f => f.Uuid = It.Is<Guid?>(uuid => uuid != null && !uuid.Equals(Guid.Empty)), Times.Once());
            this.cacheFile.Verify(f => f.MoveTo(this.localPath), Times.Once());
        }

        [Test, Category("Fast"), Category("Solver")]
        public void RemoteFileChangedWhileDeleteLocalCacheBeforeContinue() {
            this.SetupRemoteFileChanged();

            var solver = new RemoteObjectChanged(this.session.Object, this.storage.Object, this.transmissionStorage.Object, this.transmissionManager, this.fsFactory.Object);

            this.RunSolverToAbortDownload(solver, remoteContent: ContentChangeType.CHANGED);

            this.RunSolverToDeleteLocalCacheBeforeContinue(solver, remoteContent: ContentChangeType.CHANGED);
            this.cacheFile.Verify(c => c.Replace(this.localFile.Object, this.backupFile.Object, true), Times.Once());
            this.backupFile.Verify(b => b.Delete(), Times.Once());
        }

        private void SetupRemoteFileChanged() {
            this.localFile.Setup(f => f.LastWriteTimeUtc).Returns(this.creationDate);
            this.backupFile = this.fsFactory.AddFile(Path.Combine(this.parentPath, this.objectName + ".bak.sync"), false);
            this.cacheFile.Setup(
                c =>
                c.Replace(this.localFile.Object, this.backupFile.Object, It.IsAny<bool>())).Returns(this.localFile.Object).Callback(
                () =>
                this.backupFile.Setup(
                b =>
                b.Open(FileMode.Open, FileAccess.Read, FileShare.None)).Returns(new MemoryStream(this.fileContentOld)));

            var mappedObject = new MappedObject(
                this.objectName,
                this.objectId,
                MappedObjectType.File,
                this.parentId,
                this.changeToken + ".old") {
                    Guid = Guid.NewGuid(),
                    LastContentSize = 0,
                    LastChecksum = this.fileHashOld,
                    ChecksumAlgorithmName = "SHA-1",
                    LastLocalWriteTimeUtc = this.creationDate,
                };
            this.storage.AddMappedFile(mappedObject);
        }

        private void RunSolverToAbortDownload(
            AbstractEnhancedSolver solver,
            ContentChangeType localContent = ContentChangeType.NONE,
            ContentChangeType remoteContent = ContentChangeType.NONE) {
            var stream = new Mock<MemoryStream>();
            stream.SetupAllProperties();
            stream.Setup(f => f.CanWrite).Returns(true); // required for System.Security.Cryptography.CryptoStream

            this.localFileLength = 0;
            stream.Setup(f => f.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Callback((byte[] buffer, int offset, int count) => {
                if (this.localFileLength > 0) {
                    foreach (FileTransmissionEvent transmissionEvent in this.transmissionManager.ActiveTransmissions) {
                        transmissionEvent.ReportProgress(new TransmissionProgressEventArgs { Aborting = true });
                    }
                }

                this.localFileLength += count;
            });
            stream.Setup(f => f.Length).Returns(() => { return this.localFileLength; });

            this.cacheFile.Setup(f => f.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)).Returns(() => {
                this.cacheFile.Setup(f => f.Exists).Returns(true);
                return stream.Object;
            });

            var remoteDocument = MockOfIDocumentUtil.CreateRemoteDocumentMock(null, this.objectId, this.objectName, this.parentId, this.fileContent.Length, this.fileContent, this.changeToken);
            remoteDocument.Setup(f => f.LastModificationDate).Returns((DateTime?)this.creationDate);

            Assert.Throws<AbortException>(() => solver.Solve(this.localFile.Object, remoteDocument.Object, localContent, remoteContent));

            this.cacheFile.Verify(f => f.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None), Times.Once());
            this.cacheFile.VerifySet(f => f.Uuid = It.Is<Guid?>(uuid => uuid != null && !uuid.Equals(Guid.Empty)), Times.Never());
            this.cacheFile.Verify(f => f.MoveTo(this.localPath), Times.Never());
            this.localFile.VerifySet(d => d.LastWriteTimeUtc = It.Is<DateTime>(date => date.Equals(this.creationDate)), Times.Never());
            this.transmissionStorage.Verify(f => f.GetObjectByRemoteObjectId(It.IsAny<string>()), Times.Once());
            this.transmissionStorage.Verify(f => f.SaveObject(It.IsAny<IFileTransmissionObject>()), Times.Once());
            this.transmissionStorage.Verify(f => f.RemoveObjectByRemoteObjectId(It.IsAny<string>()), Times.Never());
            this.storage.Verify(f => f.SaveMappedObject(It.IsAny<IMappedObject>()), Times.Never());
        }

        private void RunSolverToContinueDownload(
            AbstractEnhancedSolver solver,
            ContentChangeType localContent = ContentChangeType.NONE,
            ContentChangeType remoteContent = ContentChangeType.NONE) {
            Mock<MemoryStream> stream = new Mock<MemoryStream>();
            stream.SetupAllProperties();
            stream.Setup(f => f.CanWrite).Returns(true); // required for System.Security.Cryptography.CryptoStream
            stream.Setup(f => f.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Callback((byte[] buffer, int offset, int count) => this.localFileLength += count);
            stream.Setup(f => f.Length).Returns(() => { return this.localFileLength; });

            long lengthRead = 0;
            stream.Setup(f => f.Seek(It.IsAny<long>(), It.IsAny<SeekOrigin>())).Callback((long offset, SeekOrigin loc) => lengthRead = offset);
            stream.Setup(f => f.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns((byte[] buffer, int offset, int count) => {
                if (lengthRead >= this.localFileLength) {
                    return 0;
                }

                int countRead = count;
                if (countRead > (this.localFileLength - lengthRead)) {
                    countRead = (int)(this.localFileLength - lengthRead);
                }

                Array.Copy(this.fileContent, lengthRead, buffer, offset, countRead);
                lengthRead += countRead;
                stream.Object.Position = lengthRead;
                return countRead;
            });

            this.cacheFile.Setup(f => f.Open(It.IsAny<FileMode>(), It.IsAny<FileAccess>(), It.IsAny<FileShare>())).Returns(stream.Object);

            var remoteDocument = MockOfIDocumentUtil.CreateRemoteDocumentMock(null, this.objectId, this.objectName, this.parentId, this.fileContent.Length, this.fileContent, this.changeToken);
            remoteDocument.Setup(f => f.LastModificationDate).Returns((DateTime?)this.creationDate);

            solver.Solve(this.localFile.Object, remoteDocument.Object, localContent, remoteContent);

            Assert.That(this.localFileLength, Is.EqualTo(this.fileContent.Length));
            stream.Verify(f => f.Seek(0, SeekOrigin.Begin), Times.Once());
            this.cacheFile.Verify(f => f.Open(It.IsAny<FileMode>(), It.IsAny<FileAccess>(), It.IsAny<FileShare>()), Times.Exactly(3)); // first open in SetupToAbortThePreviousDownload, second open to validate checksum, third open to download
            this.localFile.VerifySet(d => d.LastWriteTimeUtc = It.Is<DateTime>(date => date.Equals(this.creationDate)), Times.Once());
            this.storage.VerifySavedMappedObject(MappedObjectType.File, this.objectId, this.objectName, this.parentId, this.changeToken, true, this.creationDate, this.creationDate, this.fileHash, this.fileContent.Length);
            this.transmissionStorage.Verify(f => f.GetObjectByRemoteObjectId(this.objectId), Times.Exactly(2));
            this.transmissionStorage.Verify(f => f.SaveObject(It.IsAny<IFileTransmissionObject>()), Times.AtLeastOnce());
            this.transmissionStorage.Verify(f => f.RemoveObjectByRemoteObjectId(this.objectId), Times.Once());
        }

        private void RunSolverToChangeLocalCacheBeforeContinue(
            AbstractEnhancedSolver solver,
            ContentChangeType localContent = ContentChangeType.NONE,
            ContentChangeType remoteContent = ContentChangeType.NONE) {
            this.SetupToChangeLocalCache();

            Mock<IDocument> remoteObject = MockOfIDocumentUtil.CreateRemoteDocumentMock(null, this.objectId, this.objectName, this.parentId, this.fileContent.Length, this.fileContent, this.changeToken);
            remoteObject.Setup(f => f.LastModificationDate).Returns((DateTime?)this.creationDate);

            solver.Solve(this.localFile.Object, remoteObject.Object, localContent, remoteContent);

            Assert.That(this.localFileLength, Is.EqualTo(this.fileContent.Length));
            this.cacheFile.Verify(f => f.Open(It.IsAny<FileMode>(), It.IsAny<FileAccess>(), It.IsAny<FileShare>()), Times.Exactly(3)); // first open in SetupToAbortThePreviousDownload, second open to validate checksum, third open to download
            this.cacheFile.Verify(f => f.Delete(), Times.Once());
            this.localFile.VerifySet(d => d.LastWriteTimeUtc = It.Is<DateTime>(date => date.Equals(this.creationDate)), Times.Once());
            this.storage.VerifySavedMappedObject(MappedObjectType.File, this.objectId, this.objectName, this.parentId, this.changeToken, true, this.creationDate, this.creationDate, this.fileHash, this.fileContent.Length);
            this.transmissionStorage.Verify(f => f.GetObjectByRemoteObjectId(this.objectId), Times.Exactly(2));
            this.transmissionStorage.Verify(f => f.SaveObject(It.IsAny<IFileTransmissionObject>()), Times.AtLeastOnce());
            this.transmissionStorage.Verify(f => f.RemoveObjectByRemoteObjectId(this.objectId), Times.Once());
        }

        private void RunSolverToChangeRemoteBeforeContinue(
            AbstractEnhancedSolver solver,
            ContentChangeType localContent = ContentChangeType.NONE,
            ContentChangeType remoteContent = ContentChangeType.NONE) {
            Mock<MemoryStream> stream = new Mock<MemoryStream>();
            stream.SetupAllProperties();
            stream.Setup(f => f.CanWrite).Returns(true); // required for System.Security.Cryptography.CryptoStream
            this.localFileLength = 0;
            stream.Setup(f => f.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Callback((byte[] buffer, int offset, int count) => this.localFileLength += count);
            stream.Setup(f => f.Length).Returns(() => { return this.localFileLength; });
            stream.Setup(f => f.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns((byte[] buffer, int offset, int count) => count);
            this.cacheFile.Setup(f => f.Delete()).Callback(() => {
                stream.Setup(f => f.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns(0);
                this.cacheFile.Setup(f => f.Exists).Returns(false);
            });
            this.cacheFile.Setup(f => f.Open(It.IsAny<FileMode>(), It.IsAny<FileAccess>(), It.IsAny<FileShare>())).Returns(() => {
                this.cacheFile.Setup(f => f.Exists).Returns(true);
                return stream.Object;
            });

            string newLastChangeToken = this.changeToken + ".change";
            Mock<IDocument> remoteObject = MockOfIDocumentUtil.CreateRemoteDocumentMock(null, this.objectId, this.objectName, this.parentId, this.fileContent.Length, this.fileContent, newLastChangeToken);
            remoteObject.Setup(f => f.LastModificationDate).Returns((DateTime?)this.creationDate);

            solver.Solve(this.localFile.Object, remoteObject.Object, localContent, remoteContent);

            Assert.That(this.localFileLength, Is.EqualTo(this.fileContent.Length));
            this.cacheFile.Verify(f => f.Delete(), Times.Once());
            this.cacheFile.Verify(f => f.Open(It.IsAny<FileMode>(), It.IsAny<FileAccess>(), It.IsAny<FileShare>()), Times.Exactly(2)); // first open in SetupToAbortThePreviousDownload, second open to download
            this.localFile.VerifySet(d => d.LastWriteTimeUtc = It.Is<DateTime>(date => date.Equals(this.creationDate)), Times.Once());
            this.storage.VerifySavedMappedObject(MappedObjectType.File, this.objectId, this.objectName, this.parentId, newLastChangeToken, true, this.creationDate, this.creationDate, this.fileHash, this.fileContent.Length);
            this.transmissionStorage.Verify(f => f.GetObjectByRemoteObjectId(this.objectId), Times.Exactly(2));
            this.transmissionStorage.Verify(f => f.SaveObject(It.IsAny<IFileTransmissionObject>()), Times.AtLeastOnce());
            this.transmissionStorage.Verify(f => f.RemoveObjectByRemoteObjectId(this.objectId), Times.Once());
        }

        private void RunSolverToDeleteLocalCacheBeforeContinue(
            AbstractEnhancedSolver solver,
            ContentChangeType localContent = ContentChangeType.NONE,
            ContentChangeType remoteContent = ContentChangeType.NONE) {
            this.cacheFile.Setup(f => f.Exists).Returns(false);

            Mock<MemoryStream> stream = new Mock<MemoryStream>();
            stream.SetupAllProperties();
            stream.Setup(f => f.CanWrite).Returns(true); // required for System.Security.Cryptography.CryptoStream

            this.localFileLength = 0;
            stream.Setup(f => f.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Callback((byte[] buffer, int offset, int count) => this.localFileLength += count);

            this.cacheFile.Setup(f => f.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)).Returns(() => {
                this.cacheFile.Setup(f => f.Exists).Returns(true);
                return stream.Object;
            });

            Mock<IDocument> remoteObject = MockOfIDocumentUtil.CreateRemoteDocumentMock(null, this.objectId, this.objectName, this.parentId, this.fileContent.Length, this.fileContent, this.changeToken);
            remoteObject.Setup(f => f.LastModificationDate).Returns((DateTime?)this.creationDate);

            solver.Solve(this.localFile.Object, remoteObject.Object, localContent, remoteContent);

            Assert.That(this.localFileLength, Is.EqualTo(this.fileContent.Length));
            this.cacheFile.Verify(f => f.Open(It.IsAny<FileMode>(), It.IsAny<FileAccess>(), It.IsAny<FileShare>()), Times.Exactly(2));   // first open in SetupToAbortThePreviousDownload, second open to download
            this.localFile.VerifySet(d => d.LastWriteTimeUtc = It.Is<DateTime>(date => date.Equals(this.creationDate)), Times.Once());
            this.storage.VerifySavedMappedObject(MappedObjectType.File, this.objectId, this.objectName, this.parentId, this.changeToken, true, this.creationDate, this.creationDate, this.fileHash, this.fileContent.Length);
            this.transmissionStorage.Verify(f => f.GetObjectByRemoteObjectId(this.objectId), Times.Exactly(2));
            this.transmissionStorage.Verify(f => f.SaveObject(It.IsAny<IFileTransmissionObject>()), Times.AtLeastOnce());
            this.transmissionStorage.Verify(f => f.RemoveObjectByRemoteObjectId(this.objectId), Times.Once());
        }

        private void SetupToChangeLocalCache() {
            Mock<MemoryStream> stream = new Mock<MemoryStream>();
            stream.SetupAllProperties();
            stream.Setup(f => f.CanWrite).Returns(true);    // required for System.Security.Cryptography.CryptoStream

            long lengthRead = 0;
            stream.Setup(f => f.Seek(It.IsAny<long>(), It.IsAny<SeekOrigin>())).Callback((long offset, SeekOrigin loc) => lengthRead = offset);
            stream.Setup(f => f.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns((byte[] buffer, int offset, int count) => {
                if (lengthRead >= this.localFileLength) {
                    return 0;
                }

                int countRead = count;
                if (countRead > (this.localFileLength - lengthRead)) {
                    countRead = (int)(this.localFileLength - lengthRead);
                }

                Array.Copy(this.fileContent, lengthRead, buffer, 0, countRead);

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

            this.localFileLength = 0;
            stream.Setup(f => f.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Callback((byte[] buffer, int offset, int count) => this.localFileLength += count);
            stream.Setup(f => f.Length).Returns(() => { return this.localFileLength; });

            this.cacheFile.Setup(f => f.Delete()).Callback(() => {
                stream.Setup(f => f.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns(0);
                stream.Object.Position = 0;
                this.cacheFile.Setup(f => f.Exists).Returns(false);
            });
            this.cacheFile.Setup(f => f.Open(It.IsAny<FileMode>(), It.IsAny<FileAccess>(), It.IsAny<FileShare>())).Returns(() => {
                this.cacheFile.Setup(f => f.Exists).Returns(true);
                return stream.Object;
            });
        }
    }
}