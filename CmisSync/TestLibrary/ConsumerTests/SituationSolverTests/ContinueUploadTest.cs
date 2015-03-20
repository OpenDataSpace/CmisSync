//-----------------------------------------------------------------------
// <copyright file="ContinueUploadTest.cs" company="GRAU DATA AG">
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
    using CmisSync.Lib.Consumer.SituationSolver.PWC;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.FileTransmission;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS;
    using DotCMIS.Enums;
    using DotCMIS.Client;
    using DotCMIS.Data;

    using Moq;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture]
    public class ContinueUploadTest : IsTestWithConfiguredLog4Net {
        private readonly string parentId = "parentId";
        private readonly string objectName = "objectName";
        private readonly string objectOldId = "objectId";
        private readonly string objectPWCId = "objectPWCId";
        private readonly string objectNewId = "objectId"; // for OpenDataSpace CMIS gateway, remote object ID will not change after checkin
        private readonly string changeTokenOld = "changeTokenOld";
        private readonly string changeTokenPWC = "changeTokenPWC";
        private readonly string changeTokenNew = "changeTokenNew";
        private readonly byte[] emptyHash = SHA1.Create().ComputeHash(new byte[0]);
        private readonly long chunkSize = 8 * 1024;
        private readonly int chunkCount = 4;

        private Mock<ISession> session;
        private Mock<IMetaDataStorage> storage;
        private Mock<IFileTransmissionStorage> transmissionStorage;

        private TransmissionManager transmissionManager;

        private string parentPath;
        private string localPath;
        private byte[] fileContent;
        private byte[] fileHash;
        private long fileLength;
        private byte[] fileContentChanged;
        private byte[] fileHashChanged;
        private Mock<IFileInfo> localFile;
        private Mock<IDocument> remoteDocument;
        private Mock<IDocument> remotePWCDocument;

        [SetUp]
        public void Setup() {
            this.session = new Mock<ISession>();
            this.session.SetupTypeSystem();

            // this.session.SetupCreateOperationContext();
            this.storage = new Mock<IMetaDataStorage>();
            this.storage.Setup(f => f.SaveMappedObject(It.IsAny<IMappedObject>())).Callback<IMappedObject>((o) => {
                this.storage.Setup(f => f.GetObjectByLocalPath(It.IsAny<IFileSystemInfo>())).Returns(o);
            });

            this.transmissionStorage = new Mock<IFileTransmissionStorage>();
            this.transmissionStorage.Setup(f => f.SaveObject(It.IsAny<IFileTransmissionObject>())).Callback<IFileTransmissionObject>((o) => {
                this.transmissionStorage.Setup(f => f.GetObjectByRemoteObjectId(o.RemoteObjectId)).Returns(o);
                this.transmissionStorage.Setup(f => f.GetObjectByLocalPath(o.LocalPath)).Returns(o);
            });

            this.session.Setup(f => f.RepositoryInfo.Capabilities.IsPwcUpdatableSupported).Returns(true);
            this.transmissionStorage.Setup(f => f.ChunkSize).Returns(this.chunkSize);

            this.transmissionManager = new TransmissionManager();
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalFileAdded() {
            this.SetupFile();

            var solverAdded = new LocalObjectAddedWithPWC(this.session.Object, this.storage.Object, this.transmissionStorage.Object, this.transmissionManager, new LocalObjectAdded(this.session.Object, this.storage.Object, this.transmissionStorage.Object, this.transmissionManager));
            this.RunSolverToAbortUpload(solverAdded);
            this.RunSolverToContinueUpload(solverAdded);

            this.storage.VerifySavedMappedObject(MappedObjectType.File, this.objectNewId, this.objectName, this.parentId, this.changeTokenNew, Times.Once(), true, null, null, this.fileHash, this.fileLength);

            this.transmissionStorage.Verify(s => s.SaveObject(It.IsAny<IFileTransmissionObject>()), Times.Exactly(this.chunkCount));
            this.transmissionStorage.Verify(s => s.RemoveObjectByRemoteObjectId(It.IsAny<string>()), Times.Once());
            this.session.Verify(
                s =>
                s.CreateDocument(
                It.Is<IDictionary<string, object>>(p => (string)p["cmis:name"] == this.objectName),
                It.Is<IObjectId>(o => o.Id == this.parentId),
                null,
                VersioningState.CheckedOut),
                Times.Once());
            this.remoteDocument.Verify(d => d.CheckOut(), Times.Once());
            this.localFile.VerifySet(f => f.Uuid = It.Is<Guid?>(uuid => uuid != null), Times.Once());
            this.localFile.VerifyThatLocalFileObjectLastWriteTimeUtcIsNeverModified();
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalFileChanged() {
            this.SetupFile();
            this.SetupForLocalFileChanged();

            var solverChanged = new LocalObjectChangedWithPWC(this.session.Object, this.storage.Object, this.transmissionStorage.Object, this.transmissionManager, new LocalObjectAdded(this.session.Object, this.storage.Object, this.transmissionStorage.Object, this.transmissionManager));

            this.RunSolverToAbortUpload(solverChanged);

            this.RunSolverToContinueUpload(solverChanged);

            this.storage.VerifySavedMappedObject(MappedObjectType.File, this.objectNewId, this.objectName, this.parentId, this.changeTokenNew, Times.Once(), true, null, null, this.fileHash, this.fileLength);
            this.transmissionStorage.Verify(s => s.SaveObject(It.IsAny<IFileTransmissionObject>()), Times.Exactly(this.chunkCount));
            this.transmissionStorage.Verify(s => s.RemoveObjectByRemoteObjectId(It.IsAny<string>()), Times.Once());
            this.session.Verify(
                s =>
                s.CreateDocument(
                It.Is<IDictionary<string, object>>(p => (string)p["cmis:name"] == this.objectName),
                It.Is<IObjectId>(o => o.Id == this.parentId),
                null,
                VersioningState.CheckedOut),
                Times.Never());
            this.localFile.VerifySet(f => f.Uuid = It.Is<Guid?>(uuid => uuid != null), Times.Never());
            this.localFile.VerifyThatLocalFileObjectLastWriteTimeUtcIsNeverModified();
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalFileAddedWhileChangeLocalBeforeContinue() {
            this.SetupFile();

            var solverAdded = new LocalObjectAddedWithPWC(this.session.Object, this.storage.Object, this.transmissionStorage.Object, this.transmissionManager, new LocalObjectAdded(this.session.Object, this.storage.Object, this.transmissionStorage.Object, this.transmissionManager));
            this.RunSolverToAbortUpload(solverAdded);
            this.RunSolverToChangeLocalBeforeContinue(solverAdded);
            this.storage.VerifySavedMappedObject(MappedObjectType.File, this.objectNewId, this.objectName, this.parentId, this.changeTokenNew, Times.Once(), true, null, null, this.fileHashChanged, this.fileLength);

            this.transmissionStorage.Verify(s => s.SaveObject(It.IsAny<IFileTransmissionObject>()), Times.AtLeast(this.chunkCount + 1));    //  plus 1 to save state for abort
            this.session.Verify(
                s =>
                s.CreateDocument(
                It.Is<IDictionary<string, object>>(p => (string)p["cmis:name"] == this.objectName),
                It.Is<IObjectId>(o => o.Id == this.parentId),
                null,
                VersioningState.CheckedOut),
                Times.Once());
            this.localFile.VerifySet(f => f.Uuid = It.Is<Guid?>(uuid => uuid != null), Times.Once());
            this.localFile.VerifyThatLocalFileObjectLastWriteTimeUtcIsNeverModified();
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalFileChangedWhileChangeLocalBeforeContinue() {
            this.SetupFile();
            this.SetupForLocalFileChanged();

            var solverChanged = new LocalObjectChangedWithPWC(this.session.Object, this.storage.Object, this.transmissionStorage.Object, this.transmissionManager, new LocalObjectAdded(this.session.Object, this.storage.Object, this.transmissionStorage.Object, this.transmissionManager));

            this.RunSolverToAbortUpload(solverChanged);

            this.RunSolverToChangeLocalBeforeContinue(solverChanged);

            this.transmissionStorage.Verify(s => s.SaveObject(It.IsAny<IFileTransmissionObject>()), Times.AtLeast(this.chunkCount + 1));    //  plus 1 to save state for abort
            this.storage.VerifySavedMappedObject(MappedObjectType.File, this.objectNewId, this.objectName, this.parentId, this.changeTokenNew, Times.Once(), true, null, null, this.fileHashChanged, this.fileLength);
            this.session.Verify(
                s =>
                s.CreateDocument(
                It.Is<IDictionary<string, object>>(p => (string)p["cmis:name"] == this.objectName),
                It.Is<IObjectId>(o => o.Id == this.parentId),
                null,
                VersioningState.CheckedOut),
                Times.Never());
            this.localFile.VerifySet(f => f.Uuid = It.Is<Guid?>(uuid => uuid != null), Times.Never());
            this.localFile.VerifyThatLocalFileObjectLastWriteTimeUtcIsNeverModified();
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalFileChangedWhileChangeRemoteBeforeContinue() {
            this.SetupFile();
            this.SetupForLocalFileChanged();

            var solverChanged = new LocalObjectChangedWithPWC(this.session.Object, this.storage.Object, this.transmissionStorage.Object, this.transmissionManager, new LocalObjectAdded(this.session.Object, this.storage.Object, this.transmissionStorage.Object, this.transmissionManager));

            this.RunSolverToAbortUpload(solverChanged);

            this.RunSolverToChangeRemoteBeforeContinue(solverChanged);

            // no save, for changing the remote file will force a crawl sync
            this.storage.VerifySavedMappedObject(MappedObjectType.File, this.objectOldId, this.objectName, this.parentId, this.changeTokenNew, Times.Never(), true, null, null, this.fileHash, this.fileLength);

            this.session.Verify(
                s =>
                s.CreateDocument(
                It.Is<IDictionary<string, object>>(p => (string)p["cmis:name"] == this.objectName),
                It.Is<IObjectId>(o => o.Id == this.parentId),
                null,
                VersioningState.CheckedOut),
                Times.Never());
            this.localFile.VerifySet(f => f.Uuid = It.Is<Guid?>(uuid => uuid != null), Times.Never());
            this.localFile.VerifyThatLocalFileObjectLastWriteTimeUtcIsNeverModified();
        }

        private void SetupForLocalFileChanged() {
            var mappedObject = new MappedObject(
                this.objectName,
                this.objectOldId,
                MappedObjectType.File,
                this.parentId,
                this.changeTokenOld) {
                    Guid = Guid.NewGuid(),
                    ChecksumAlgorithmName = "SHA-1",
                    LastContentSize = 0,
                    LastChecksum = this.emptyHash
                };
            this.storage.AddMappedFile(mappedObject, this.localPath);
        }

        private void RunSolverToAbortUpload(
            AbstractEnhancedSolverWithPWC solver,
            ContentChangeType localContent = ContentChangeType.NONE,
            ContentChangeType remoteContent = ContentChangeType.NONE) {
            var stream = new Mock<MemoryStream>();
            stream.SetupAllProperties();
            stream.Setup(s => s.CanRead).Returns(true);
            stream.Setup(s => s.Length).Returns(this.fileLength);
            long readLength = 0;
            stream.Setup(s => s.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns((byte[] buffer, int offset, int count) => {
                if (readLength > 0) {
                    foreach (Transmission transmission in this.transmissionManager.ActiveTransmissions) {
                        transmission.Abort();
                    }
                }

                if (readLength + count > this.fileLength) {
                    count = (int)(this.fileLength - readLength);
                }

                Array.Copy(this.fileContent, readLength, buffer, offset, count);
                readLength += count;
                return count;
            });
            stream.Setup(s => s.Position).Returns(() => { return readLength; });

            this.localFile.Setup(f => f.Length).Returns(this.fileLength);
            this.localFile.Setup(f => f.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete)).Returns(stream.Object);

            Assert.Throws<AbortException>(() => solver.Solve(this.localFile.Object, this.remoteDocument.Object, localContent, remoteContent));
            Assert.That(this.transmissionManager.ActiveTransmissions, Is.Empty);
        }

        private void RunSolverToContinueUpload(
            AbstractEnhancedSolverWithPWC solver,
            ContentChangeType localContent = ContentChangeType.NONE,
            ContentChangeType remoteContent = ContentChangeType.NONE) {
            long readLength = 0;
            var stream = new Mock<MemoryStream>();
            stream.SetupAllProperties();
            stream.Setup(s => s.CanRead).Returns(true);
            stream.Setup(s => s.Length).Returns(this.fileLength);
            stream.Setup(s => s.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns((byte[] buffer, int offset, int count) => {
                if (readLength + count > this.fileLength) {
                    count = (int)(this.fileLength - readLength);
                }

                Array.Copy(this.fileContent, readLength, buffer, offset, count);
                readLength += count;
                return count;
            });
            stream.Setup(f => f.Seek(It.IsAny<long>(), It.IsAny<SeekOrigin>())).Callback((long offset, SeekOrigin loc) => readLength = offset);
            stream.Setup(s => s.Position).Returns(() => { return readLength; });

            this.localFile.Setup(f => f.Length).Returns(this.fileLength);
            this.localFile.Setup(f => f.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete)).Returns(stream.Object);

            solver.Solve(this.localFile.Object, this.remoteDocument.Object, localContent, remoteContent);
            Assert.That(this.transmissionManager.ActiveTransmissions, Is.Empty);

            this.remotePWCDocument.Verify(d => d.AppendContentStream(It.IsAny<IContentStream>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Exactly(this.chunkCount + 1)); // plus 1 for one AppendContentStream is aborted
            this.remotePWCDocument.Verify(d => d.CheckIn(It.IsAny<bool>(), It.Is<IDictionary<string, object>>(p => p.ContainsKey(PropertyIds.LastModificationDate)), null, It.IsAny<string>()), Times.Once());
        }

        private void RunSolverToChangeLocalBeforeContinue(
            AbstractEnhancedSolverWithPWC solver,
            ContentChangeType localContent = ContentChangeType.NONE,
            ContentChangeType remoteContent = ContentChangeType.NONE) {
            this.SetupToChangeLocal();

            solver.Solve(this.localFile.Object, this.remoteDocument.Object, localContent, remoteContent);
            Assert.That(this.transmissionManager.ActiveTransmissions, Is.Empty);

            this.remotePWCDocument.Verify(d => d.DeleteContentStream(), Times.Exactly(2));
            this.remotePWCDocument.Verify(d => d.AppendContentStream(It.IsAny<IContentStream>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Exactly(this.chunkCount + 3)); // plus 1 for one AppendContentStream is aborted, plus 2 for the last upload having two AppendContentStream
            this.remotePWCDocument.Verify(d => d.CheckIn(It.IsAny<bool>(), It.Is<IDictionary<string, object>>(p => p.ContainsKey(PropertyIds.LastModificationDate)), null, It.IsAny<string>()), Times.Once());
        }

        private void RunSolverToChangeRemoteBeforeContinue(
            AbstractEnhancedSolverWithPWC solver,
            ContentChangeType localContent = ContentChangeType.NONE,
            ContentChangeType remoteContent = ContentChangeType.NONE) {
            this.remoteDocument.Setup(d => d.ChangeToken).Returns(this.changeTokenOld + ".change");

            Assert.Throws<ArgumentException>(() => solver.Solve(this.localFile.Object, this.remoteDocument.Object, localContent, remoteContent));
            Assert.That(this.transmissionManager.ActiveTransmissions, Is.Empty);

            this.remotePWCDocument.Verify(d => d.DeleteContentStream(), Times.Exactly(1));
            this.remotePWCDocument.Verify(d => d.AppendContentStream(It.IsAny<IContentStream>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Exactly(3)); // 1 for one AppendContentStream is aborted, and 2 for the last upload having two AppendContentStream
            this.remotePWCDocument.Verify(d => d.CheckIn(It.IsAny<bool>(), It.IsAny<IDictionary<string, object>>(), null, It.IsAny<string>()), Times.Never());
        }

        private void SetupToChangeLocal() {
            long readLength = 0;
            var stream = new Mock<MemoryStream>();
            stream.SetupAllProperties();
            stream.Setup(s => s.CanRead).Returns(true);
            stream.Setup(s => s.Length).Returns(this.fileLength);
            stream.Setup(s => s.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns((byte[] buffer, int offset, int count) => {
                if (readLength + count > this.fileLength) {
                    count = (int)(this.fileLength - readLength);
                }

                Array.Copy(this.fileContentChanged, readLength, buffer, offset, count);
                readLength += count;
                return count;
            });
            stream.Setup(f => f.Seek(It.IsAny<long>(), It.IsAny<SeekOrigin>())).Callback((long offset, SeekOrigin loc) => readLength = offset);
            stream.Setup(s => s.Position).Returns(() => { return readLength; });

            this.localFile.Setup(f => f.Length).Returns(this.fileLength);
            this.localFile.Setup(f => f.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete)).Returns(stream.Object);
        }

        private void SetupFile() {
            this.parentPath = Path.GetTempPath();
            this.localPath = Path.Combine(this.parentPath, this.objectName);

            var parentDirInfo = Mock.Of<IDirectoryInfo>(d => d.FullName == this.parentPath && d.Name == Path.GetFileName(this.parentPath));
            this.storage.AddLocalFolder(parentDirInfo, this.parentId);

            var parents = new List<IFolder>();
            parents.Add(Mock.Of<IFolder>(f => f.Id == this.parentId));

            var file = Mock.Of<IFileInfo>(
                f =>
                f.FullName == this.localPath &&
                f.Name == this.objectName &&
                f.Exists == true &&
                f.IsExtendedAttributeAvailable() == true &&
                f.Directory == parentDirInfo);
            this.localFile = Mock.Get(file);

            var docId = Mock.Of<IObjectId>(
                o =>
                o.Id == this.objectOldId);

            var doc = Mock.Of<IDocument>(
                d =>
                d.Name == this.objectName &&
                d.Id == this.objectOldId &&
                d.Parents == parents &&
                d.ChangeToken == this.changeTokenOld);
            this.remoteDocument = Mock.Get(doc);

            this.session.Setup(s => s.CreateDocument(
                It.IsAny<IDictionary<string, object>>(),
                It.IsAny<IObjectId>(),
                null,
                VersioningState.CheckedOut)).Returns(docId);
            this.remoteDocument.Setup(
                d =>
                d.SetContentStream(It.IsAny<IContentStream>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .Callback<IContentStream, bool, bool>((s, o, r) => {
                    using (var temp = new MemoryStream()) {
                        s.Stream.CopyTo(temp);
                    }
                });
            this.remoteDocument.Setup(d => d.LastModificationDate).Returns(new DateTime());
            this.session.AddRemoteObject(this.remoteDocument.Object);

            var docPWC = Mock.Of<IDocument>(
                d =>
                d.Name == this.objectName &&
                d.Id == this.objectPWCId &&
                d.ChangeToken == this.changeTokenPWC);
            this.remotePWCDocument = Mock.Get(docPWC);
            long length = 0;
            this.remotePWCDocument.Setup(d => d.AppendContentStream(It.IsAny<IContentStream>(), It.IsAny<bool>(), It.IsAny<bool>())).Callback<IContentStream, bool, bool>((stream, last, refresh) => {
                byte[] buffer = new byte[stream.Length.GetValueOrDefault()];
                length += stream.Stream.Read(buffer, 0, buffer.Length);
            });
            this.remotePWCDocument.Setup(d => d.ContentStreamLength).Returns(() => { return length; });
            this.remotePWCDocument.Setup(d => d.DeleteContentStream()).Callback(() => { length = 0; });
            this.session.AddRemoteObject(this.remotePWCDocument.Object);

            this.remoteDocument.SetupCheckout(this.remotePWCDocument, this.changeTokenNew);

            this.fileLength = this.chunkCount * this.chunkSize;
            this.fileContent = new byte[this.fileLength];
            this.fileHash = SHA1Managed.Create().ComputeHash(this.fileContent);
            this.fileContentChanged = new byte[this.fileLength];
            Array.Copy(this.fileContent, this.fileContentChanged, this.fileLength);

            // change the first byte
            if (this.fileContentChanged[0] == (byte)0) {
                this.fileContentChanged[0] = (byte)1;
            } else {
                this.fileContentChanged[0] = (byte)0;
            }

            this.fileHashChanged = SHA1Managed.Create().ComputeHash(this.fileContentChanged);
        }
    }
}