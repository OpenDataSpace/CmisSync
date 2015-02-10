//-----------------------------------------------------------------------
// <copyright file="LocalObjectAddedTest.cs" company="GRAU DATA AG">
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
    using System.Security.Cryptography;
    using System.IO;

    using CmisSync.Lib.Storage.FileSystem;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.FileTransmission;
    using CmisSync.Lib.Consumer.SituationSolver;

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

        private Mock<ISession> session;
        private Mock<IFileSystemInfoFactory> fsFactory;
        private Mock<IMetaDataStorage> storage;
        private Mock<IFileTransmissionStorage> transmissionStorage;
        private ActiveActivitiesManager transmissionManager;

        private string parentPath;
        private string localPath;
        private readonly long chunkSize = 8 * 1024;
        private readonly int chunkCount = 4;
        private readonly byte[] emptyHash = SHA1.Create().ComputeHash(new byte[0]);
        private byte[] fileContent;
        private byte[] fileHash;
        private Mock<IFileInfo> localFile;
        private Mock<IFileInfo> cacheFile;
        private Mock<IDocument> remoteDocument;

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
            this.fileContent = new byte[this.chunkCount * this.chunkSize];
            this.fileHash = SHA1.Create().ComputeHash(fileContent);

            var parentDir = Mock.Of<IDirectoryInfo>(d => d.FullName == this.parentPath && d.Name == Path.GetFileName(this.parentPath));
            this.localFile = Mock.Get(Mock.Of<IFileInfo>(
                f =>
                f.FullName == this.localPath &&
                f.Name == this.objectName &&
                f.Directory == parentDir));

            this.cacheFile = this.fsFactory.SetupDownloadCacheFile();
            this.cacheFile.SetupAllProperties();
            this.cacheFile.Setup(f => f.FullName).Returns(this.localPath + ".sync");
            this.cacheFile.Setup(f => f.Name).Returns(this.objectName + ".sync");
            this.cacheFile.Setup(f => f.Directory).Returns(parentDir);
            this.cacheFile.Setup(f => f.IsExtendedAttributeAvailable()).Returns(true);
            this.fsFactory.AddIFileInfo(this.cacheFile.Object);

            this.remoteDocument = new Mock<IDocument>();
        }

        private void SetupToAbortDownload(out long lengthWrite) {
            var stream = new Mock<MemoryStream>();
            stream.SetupAllProperties();
            stream.Setup(f => f.CanWrite).Returns(true);    //  required for System.Security.Cryptography.CryptoStream

            long length = 0;
            stream.Setup(f => f.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Callback((byte[] buffer, int offset, int count) => {
                if (length > 0) {
                    foreach (FileTransmissionEvent transmissionEvent in this.transmissionManager.ActiveTransmissions) {
                        transmissionEvent.ReportProgress(new TransmissionProgressEventArgs { Aborting = true });
                    }
                }
                length += count;
            });
            stream.Setup(f => f.Length).Returns(() => { return length; });

            this.cacheFile.Setup(f => f.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)).Returns(() => {
                this.cacheFile.Setup(f => f.Exists).Returns(true);
                return stream.Object;
            });

            Mock<IDocument> remoteObject = MockOfIDocumentUtil.CreateRemoteDocumentMock(null, this.objectId, this.objectName, this.parentId, this.fileContent.Length, this.fileContent, this.changeToken);
            remoteObject.Setup(f => f.LastModificationDate).Returns((DateTime?)this.creationDate);

            var solver = new RemoteObjectAdded(this.session.Object, this.storage.Object, this.transmissionStorage.Object, this.transmissionManager, this.fsFactory.Object);
            Assert.Throws<AbortException>(() => solver.Solve(this.localFile.Object, remoteObject.Object));

            this.cacheFile.Verify(f => f.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None), Times.Once());
            this.cacheFile.VerifySet(f => f.Uuid = It.Is<Guid?>(uuid => uuid != null && !uuid.Equals(Guid.Empty)), Times.Never());
            this.cacheFile.Verify(f => f.MoveTo(this.localPath), Times.Never());
            this.localFile.VerifySet(d => d.LastWriteTimeUtc = It.Is<DateTime>(date => date.Equals(this.creationDate)), Times.Never());
            this.transmissionStorage.Verify(f => f.GetObjectByRemoteObjectId(It.IsAny<string>()), Times.Once());
            this.transmissionStorage.Verify(f => f.SaveObject(It.IsAny<IFileTransmissionObject>()), Times.Once());
            this.transmissionStorage.Verify(f => f.RemoveObjectByRemoteObjectId(It.IsAny<string>()), Times.Never());
            this.storage.Verify(f => f.SaveMappedObject(It.IsAny<IMappedObject>()), Times.Never());

            lengthWrite = length;
        }

        [Test, Category("Fast"), Category("Solver")]
        public void RemoteFileAdded() {
            long length = 0;
            SetupToAbortDownload(out length);

            Mock<MemoryStream> stream = new Mock<MemoryStream>();
            stream.SetupAllProperties();
            stream.Setup(f => f.CanWrite).Returns(true);    //  required for System.Security.Cryptography.CryptoStream
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
                Array.Copy(this.fileContent, lengthRead, buffer, offset, countRead);
                lengthRead += countRead;
                stream.Object.Position = lengthRead;
                return countRead;
            });

            this.cacheFile.Setup(f => f.Open(It.IsAny<FileMode>(), It.IsAny<FileAccess>(), It.IsAny<FileShare>())).Returns(stream.Object);

            Mock<IDocument> remoteObject = MockOfIDocumentUtil.CreateRemoteDocumentMock(null, this.objectId, this.objectName, this.parentId, this.fileContent.Length, this.fileContent, this.changeToken);
            remoteObject.Setup(f => f.LastModificationDate).Returns((DateTime?)this.creationDate);

            var solver = new RemoteObjectAdded(this.session.Object, this.storage.Object, this.transmissionStorage.Object, this.transmissionManager, this.fsFactory.Object);
            solver.Solve(this.localFile.Object, remoteObject.Object);

            Assert.That(length, Is.EqualTo(this.fileContent.Length));
            stream.Verify(f => f.Seek(0, SeekOrigin.Begin), Times.Once());
            this.cacheFile.Verify(f => f.Open(It.IsAny<FileMode>(), It.IsAny<FileAccess>(), It.IsAny<FileShare>()), Times.Exactly(3));   //  first open in SetupToAbortThePreviousDownload, second open to validate checksum, third open to download
            this.cacheFile.VerifySet(f => f.Uuid = It.Is<Guid?>(uuid => uuid != null && !uuid.Equals(Guid.Empty)), Times.Once());
            this.cacheFile.Verify(f => f.MoveTo(this.localPath), Times.Once());
            this.localFile.VerifySet(d => d.LastWriteTimeUtc = It.Is<DateTime>(date => date.Equals(this.creationDate)), Times.Once());
            this.storage.VerifySavedMappedObject(MappedObjectType.File, this.objectId, this.objectName, this.parentId, this.changeToken, true, this.creationDate, this.creationDate, this.fileHash, this.fileContent.Length);
            this.transmissionStorage.Verify(f => f.GetObjectByRemoteObjectId(this.objectId), Times.Exactly(2));
            this.transmissionStorage.Verify(f => f.SaveObject(It.IsAny<IFileTransmissionObject>()), Times.Once());
            this.transmissionStorage.Verify(f => f.RemoveObjectByRemoteObjectId(this.objectId), Times.Once());
        }

        [Ignore("TODO")]
        [Test, Category("Fast"), Category("Solver")]
        public void RemoteFileChanged() {
            Assert.Fail("TODO");
        }

        [Test, Category("Fast"), Category("Solver")]
        public void RemoteFileAddedWhileChangeLocalBeforeContinue() {
            long lengthPrev = 0;
            SetupToAbortDownload(out lengthPrev);

            Mock<MemoryStream> stream = new Mock<MemoryStream>();
            stream.SetupAllProperties();
            stream.Setup(f => f.CanWrite).Returns(true);    //  required for System.Security.Cryptography.CryptoStream

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
                Array.Copy(this.fileContent, lengthRead, buffer, 0, countRead);
                //  change the first byte
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

            this.cacheFile.Setup(f => f.Delete()).Callback(() => {
                stream.Setup(f => f.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns(0);
                stream.Object.Position = 0;
                this.cacheFile.Setup(f => f.Exists).Returns(false);
            });
            this.cacheFile.Setup(f => f.Open(It.IsAny<FileMode>(), It.IsAny<FileAccess>(), It.IsAny<FileShare>())).Returns(stream.Object);

            Mock<IDocument> remoteObject = MockOfIDocumentUtil.CreateRemoteDocumentMock(null, this.objectId, this.objectName, this.parentId, this.fileContent.Length, this.fileContent, this.changeToken);
            remoteObject.Setup(f => f.LastModificationDate).Returns((DateTime?)this.creationDate);

            var solver = new RemoteObjectAdded(this.session.Object, this.storage.Object, this.transmissionStorage.Object, this.transmissionManager, this.fsFactory.Object);
            solver.Solve(this.localFile.Object, remoteObject.Object);

            Assert.That(length, Is.EqualTo(this.fileContent.Length));
            stream.Verify(f => f.Seek(0, SeekOrigin.Begin), Times.Never());
            this.cacheFile.Verify(f => f.Open(It.IsAny<FileMode>(), It.IsAny<FileAccess>(), It.IsAny<FileShare>()), Times.Exactly(3));   //  first open in SetupToAbortThePreviousDownload, second open to validate checksum, third open to download
            this.cacheFile.Verify(f => f.Delete(), Times.Once());
            this.cacheFile.VerifySet(f => f.Uuid = It.Is<Guid?>(uuid => uuid != null && !uuid.Equals(Guid.Empty)), Times.Once());
            this.cacheFile.Verify(f => f.MoveTo(this.localPath), Times.Once());
            this.localFile.VerifySet(d => d.LastWriteTimeUtc = It.Is<DateTime>(date => date.Equals(this.creationDate)), Times.Once());
            this.storage.VerifySavedMappedObject(MappedObjectType.File, this.objectId, this.objectName, this.parentId, this.changeToken, true, this.creationDate, this.creationDate, this.fileHash, this.fileContent.Length);
            this.transmissionStorage.Verify(f => f.GetObjectByRemoteObjectId(this.objectId), Times.Exactly(2));
            this.transmissionStorage.Verify(f => f.SaveObject(It.IsAny<IFileTransmissionObject>()), Times.Once());
            this.transmissionStorage.Verify(f => f.RemoveObjectByRemoteObjectId(this.objectId), Times.Once());
        }

        [Ignore("TODO")]
        [Test, Category("Fast"), Category("Solver")]
        public void RemoteFileChangedWhileChangeLocalBeforeContinue() {
            Assert.Fail("TODO");
        }

        [Test, Category("Fast"), Category("Solver")]
        public void RemoteFileAddedWhileChangeRemoteBeforeContinue() {
            long lengthPrev = 0;
            SetupToAbortDownload(out lengthPrev);

            Mock<MemoryStream> stream = new Mock<MemoryStream>();
            stream.SetupAllProperties();
            stream.Setup(f => f.CanWrite).Returns(true);    //  required for System.Security.Cryptography.CryptoStream
            long length = 0;
            stream.Setup(f => f.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Callback((byte[] buffer, int offset, int count) => length += count);
            stream.Setup(f => f.Length).Returns(() => { return length; });
            stream.Setup(f => f.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns((byte[] buffer, int offset, int count) => count);
            this.cacheFile.Setup(f => f.Delete()).Callback(() => {
                stream.Setup(f => f.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns(0);
                this.cacheFile.Setup(f => f.Exists).Returns(false);
            });
            this.cacheFile.Setup(f => f.Open(It.IsAny<FileMode>(), It.IsAny<FileAccess>(), It.IsAny<FileShare>())).Returns(stream.Object);

            string newLastChangeToken = this.changeToken + ".change";
            Mock<IDocument> remoteObject = MockOfIDocumentUtil.CreateRemoteDocumentMock(null, this.objectId, this.objectName, this.parentId, this.fileContent.Length, this.fileContent, newLastChangeToken);
            remoteObject.Setup(f => f.LastModificationDate).Returns((DateTime?)this.creationDate);

            var solver = new RemoteObjectAdded(this.session.Object, this.storage.Object, this.transmissionStorage.Object, this.transmissionManager, this.fsFactory.Object);
            solver.Solve(this.localFile.Object, remoteObject.Object);

            Assert.That(length, Is.EqualTo(this.fileContent.Length));
            stream.Verify(f => f.Seek(0, SeekOrigin.Begin), Times.Never());
            this.cacheFile.Verify(f => f.Delete(), Times.Once());
            this.cacheFile.Verify(f => f.Open(It.IsAny<FileMode>(), It.IsAny<FileAccess>(), It.IsAny<FileShare>()), Times.Exactly(2));   //  first open in SetupToAbortThePreviousDownload, second open to download
            this.cacheFile.VerifySet(f => f.Uuid = It.Is<Guid?>(uuid => uuid != null && !uuid.Equals(Guid.Empty)), Times.Once());
            this.cacheFile.Verify(f => f.MoveTo(this.localPath), Times.Once());
            this.localFile.VerifySet(d => d.LastWriteTimeUtc = It.Is<DateTime>(date => date.Equals(this.creationDate)), Times.Once());
            this.storage.VerifySavedMappedObject(MappedObjectType.File, this.objectId, this.objectName, this.parentId, newLastChangeToken, true, this.creationDate, this.creationDate, fileHash, this.fileContent.Length);
            this.transmissionStorage.Verify(f => f.GetObjectByRemoteObjectId(this.objectId), Times.Exactly(2));
            this.transmissionStorage.Verify(f => f.SaveObject(It.IsAny<IFileTransmissionObject>()), Times.Once());
            this.transmissionStorage.Verify(f => f.RemoveObjectByRemoteObjectId(this.objectId), Times.Once());
        }

        [Ignore("TODO")]
        [Test, Category("Fast"), Category("Solver")]
        public void RemoteFileChangedWhileChangeRemoteBeforeContinue() {
            Assert.Fail("TODO");
        }

        [Ignore("TODO")]
        [Test, Category("Fast"), Category("Solver")]
        public void RemoteFileAddedWhileChangeLocalAndRemoteBeforeContinue() {
            Assert.Fail("TODO");
        }

        [Ignore("TODO")]
        [Test, Category("Fast"), Category("Solver")]
        public void RemoteFileChangedWhileChangeLocalAndRemoteBeforeContinue() {
            Assert.Fail("TODO");
        }

        [Test, Category("Fast"), Category("Solver")]
        public void RemoteFileAddedWhileChangeLocalCacheBeforeContinue() {
            long lengthPrev = 0;
            SetupToAbortDownload(out lengthPrev);

            Mock<MemoryStream> stream = new Mock<MemoryStream>();
            stream.SetupAllProperties();
            stream.Setup(f => f.CanWrite).Returns(true);    //  required for System.Security.Cryptography.CryptoStream

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
                Array.Copy(this.fileContent, lengthRead, buffer, 0, countRead);
                //  change the first byte
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

            this.cacheFile.Setup(f => f.Delete()).Callback(() => {
                stream.Setup(f => f.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns(0);
                stream.Object.Position = 0;
                this.cacheFile.Setup(f => f.Exists).Returns(false);
            });
            this.cacheFile.Setup(f => f.Open(It.IsAny<FileMode>(), It.IsAny<FileAccess>(), It.IsAny<FileShare>())).Returns(stream.Object);

            Mock<IDocument> remoteObject = MockOfIDocumentUtil.CreateRemoteDocumentMock(null, this.objectId, this.objectName, this.parentId, this.fileContent.Length, this.fileContent, this.changeToken);
            remoteObject.Setup(f => f.LastModificationDate).Returns((DateTime?)this.creationDate);

            var solver = new RemoteObjectAdded(this.session.Object, this.storage.Object, this.transmissionStorage.Object, this.transmissionManager, this.fsFactory.Object);
            solver.Solve(this.localFile.Object, remoteObject.Object);

            Assert.That(length, Is.EqualTo(this.fileContent.Length));
            stream.Verify(f => f.Seek(0, SeekOrigin.Begin), Times.Never());
            this.cacheFile.Verify(f => f.Open(It.IsAny<FileMode>(), It.IsAny<FileAccess>(), It.IsAny<FileShare>()), Times.Exactly(3));   //  first open in SetupToAbortThePreviousDownload, second open to validate checksum, third open to download
            this.cacheFile.Verify(f => f.Delete(), Times.Once());
            this.cacheFile.VerifySet(f => f.Uuid = It.Is<Guid?>(uuid => uuid != null && !uuid.Equals(Guid.Empty)), Times.Once());
            this.cacheFile.Verify(f => f.MoveTo(this.localPath), Times.Once());
            this.localFile.VerifySet(d => d.LastWriteTimeUtc = It.Is<DateTime>(date => date.Equals(this.creationDate)), Times.Once());
            this.storage.VerifySavedMappedObject(MappedObjectType.File, this.objectId, this.objectName, this.parentId, this.changeToken, true, this.creationDate, this.creationDate, this.fileHash, this.fileContent.Length);
            this.transmissionStorage.Verify(f => f.GetObjectByRemoteObjectId(this.objectId), Times.Exactly(2));
            this.transmissionStorage.Verify(f => f.SaveObject(It.IsAny<IFileTransmissionObject>()), Times.Once());
            this.transmissionStorage.Verify(f => f.RemoveObjectByRemoteObjectId(this.objectId), Times.Once());
        }

        [Test, Category("Fast"), Category("Solver")]
        public void RemoteFileAddedWhileDeleteLocalCacheBeforeContinue() {
            long lengthPrev = 0;
            SetupToAbortDownload(out lengthPrev);

            this.cacheFile.Setup(f => f.Exists).Returns(false);

            Mock<MemoryStream> stream = new Mock<MemoryStream>();
            stream.SetupAllProperties();
            stream.Setup(f => f.CanWrite).Returns(true);    //  required for System.Security.Cryptography.CryptoStream

            long length = 0;
            stream.Setup(f => f.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Callback((byte[] buffer, int offset, int count) => length += count);

            this.cacheFile.Setup(f => f.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)).Returns(() => {
                this.cacheFile.Setup(f => f.Exists).Returns(true);
                return stream.Object;
            });

            Mock<IDocument> remoteObject = MockOfIDocumentUtil.CreateRemoteDocumentMock(null, this.objectId, this.objectName, this.parentId, this.fileContent.Length, this.fileContent, this.changeToken);
            remoteObject.Setup(f => f.LastModificationDate).Returns((DateTime?)this.creationDate);

            var solver = new RemoteObjectAdded(this.session.Object, this.storage.Object, this.transmissionStorage.Object, this.transmissionManager, this.fsFactory.Object);
            solver.Solve(this.localFile.Object, remoteObject.Object);

            Assert.That(length, Is.EqualTo(this.fileContent.Length));
            this.cacheFile.Verify(f => f.Open(It.IsAny<FileMode>(), It.IsAny<FileAccess>(), It.IsAny<FileShare>()), Times.Exactly(2));   //  first open in SetupToAbortThePreviousDownload, second open to download
            this.cacheFile.VerifySet(f => f.Uuid = It.Is<Guid?>(uuid => uuid != null && !uuid.Equals(Guid.Empty)), Times.Once());
            this.cacheFile.Verify(f => f.MoveTo(this.localPath), Times.Once());
            this.localFile.VerifySet(d => d.LastWriteTimeUtc = It.Is<DateTime>(date => date.Equals(this.creationDate)), Times.Once());
            this.storage.VerifySavedMappedObject(MappedObjectType.File, this.objectId, this.objectName, this.parentId, this.changeToken, true, this.creationDate, this.creationDate, this.fileHash, this.fileContent.Length);
            this.transmissionStorage.Verify(f => f.GetObjectByRemoteObjectId(this.objectId), Times.Exactly(2));
            this.transmissionStorage.Verify(f => f.SaveObject(It.IsAny<IFileTransmissionObject>()), Times.Once());
            this.transmissionStorage.Verify(f => f.RemoveObjectByRemoteObjectId(this.objectId), Times.Once());
        }

    }
}
