//-----------------------------------------------------------------------
// <copyright file="LocalObjectChangedTest.cs" company="GRAU DATA AG">
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
    using System.Collections.Generic;
    using System.IO;
    using System.Security.Cryptography;

    using CmisSync.Lib.Consumer.SituationSolver;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;
    using DotCMIS.Data;
    using DotCMIS.Exceptions;

    using Moq;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture]
    public class LocalObjectChangedTest
    {
        private readonly string objectName = "name";
        private readonly string remoteId = "remoteId";
        private readonly string oldChangeToken = "oldChangeToken";
        private readonly string newChangeToken = "newChangeToken";
        private readonly string parentId = "parentId";

        private string localPath;
        private string remotePath;
        private Mock<ActiveActivitiesManager> manager;
        private Mock<IMetaDataStorage> storage;
        private Mock<ISession> session;
        private LocalObjectChanged underTest;
        private Guid uuid;
        private DateTime modificationDate;

        [Test, Category("Fast"), Category("Solver")]
        public void DefaultConstructorTest() {
            var session = new Mock<ISession>();
            session.SetupTypeSystem();
            new LocalObjectChanged(session.Object, Mock.Of<IMetaDataStorage>(), null, new ActiveActivitiesManager());
        }

        [Test, Category("Fast"), Category("Solver")]
        public void ConstructorThrowsExceptionIfTransmissionManagerIsNull() {
            var session = new Mock<ISession>();
            session.SetupTypeSystem();
            Assert.Throws<ArgumentNullException>(() => new LocalObjectChanged(session.Object, Mock.Of<IMetaDataStorage>(), null, null));
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalFolderChanged() {
            this.SetUpMocks();
            var modificationDate = DateTime.UtcNow;
            var localDirectory = this.CreateLocalDirectory(modificationDate.AddMinutes(1));
            var mappedObject = this.CreateMappedFolder(modificationDate.AddMinutes(1));
            this.storage.AddMappedFolder(mappedObject);

            this.underTest.Solve(localDirectory.Object, Mock.Of<IFolder>(f => f.ChangeToken == this.oldChangeToken));

            this.VerifySavedFolder(this.oldChangeToken, localDirectory.Object.LastWriteTimeUtc);
            localDirectory.VerifyThatLocalFileObjectLastWriteTimeUtcIsNeverModified();
            this.manager.Verify(m => m.AddTransmission(It.IsAny<FileTransmissionEvent>()), Times.Never());
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalObjectChangeFailsIfObjectIsNotAvailableInStorage() {
            this.SetUpMocks();
            var modificationDate = DateTime.UtcNow;
            var localDirectory = this.CreateLocalDirectory(modificationDate.AddMinutes(1));

            Assert.Throws<ArgumentException>(() => this.underTest.Solve(localDirectory.Object, Mock.Of<IFolder>()));

            this.storage.Verify(s => s.SaveMappedObject(It.IsAny<IMappedObject>()), Times.Never());
            localDirectory.VerifyThatLocalFileObjectLastWriteTimeUtcIsNeverModified();
            this.manager.Verify(m => m.AddTransmission(It.IsAny<FileTransmissionEvent>()), Times.Never());
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalFolderChangedFetchByGuidInExtendedAttribute() {
            this.SetUpMocks();
            var localDirectory = this.CreateLocalDirectory(this.modificationDate.AddMinutes(1));
            var mappedObject = this.CreateMappedFolder(this.modificationDate.AddMinutes(1));
            this.storage.AddMappedFolder(mappedObject);

            this.underTest.Solve(localDirectory.Object, Mock.Of<IFolder>(f => f.ChangeToken == this.oldChangeToken));

            this.VerifySavedFolder(this.oldChangeToken, localDirectory.Object.LastWriteTimeUtc);
            localDirectory.VerifyThatLocalFileObjectLastWriteTimeUtcIsNeverModified();
            this.manager.Verify(m => m.AddTransmission(It.IsAny<FileTransmissionEvent>()), Times.Never());
            this.storage.Verify(s => s.GetObjectByLocalPath(It.IsAny<IFileSystemInfo>()), Times.Never());
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalFileModificationDateNotWritableShallNotThrow() {
            this.SetUpMocks();
            var newModificationDate = this.modificationDate.AddHours(1);
            int fileLength = 20;
            byte[] content = new byte[fileLength];
            var localFile = this.CreateLocalFile(fileLength, this.modificationDate.AddMinutes(1));
            using (var uploadedContent = new MemoryStream()) {
                localFile.Setup(
                    f =>
                    f.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete)).Returns(() => { return new MemoryStream(content); });
                var mappedObject = this.CreateMappedObject(true, this.modificationDate.AddMinutes(1), fileLength, new byte[20]);
                this.storage.AddMappedFile(mappedObject);
                var remoteFile = MockOfIDocumentUtil.CreateRemoteDocumentMock(null, this.remoteId, this.objectName, this.parentId, fileLength, new byte[fileLength], this.oldChangeToken);
                remoteFile.Setup(r => r.SetContentStream(It.IsAny<IContentStream>(), true, true)).Callback<IContentStream, bool, bool>(
                    (s, o, r) =>
                    { s.Stream.CopyTo(uploadedContent);
                    remoteFile.Setup(f => f.LastModificationDate).Returns(newModificationDate);
                    remoteFile.Setup(f => f.ChangeToken).Returns(this.newChangeToken);
                });

                this.underTest.Solve(localFile.Object, remoteFile.Object);
            }

            localFile.VerifyThatLocalFileObjectLastWriteTimeUtcIsNeverModified();
            this.manager.Verify(m => m.AddTransmission(It.IsAny<FileTransmissionEvent>()), Times.Once());
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalFileModificationDateChanged() {
            this.SetUpMocks();
            int fileLength = 20;
            byte[] content = new byte[fileLength];
            byte[] expectedHash = SHA1Managed.Create().ComputeHash(content);
            var localFile = this.CreateLocalFile(fileLength, this.modificationDate.AddMinutes(1));
            using (var stream = new MemoryStream(content)) {
                localFile.Setup(
                    f =>
                    f.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete)).Returns(stream);
                var mappedObject = this.CreateMappedObject(true, this.modificationDate.AddMinutes(1), fileLength, expectedHash);
                this.storage.AddMappedFile(mappedObject);

                this.underTest.Solve(localFile.Object, Mock.Of<IDocument>(d => d.ChangeToken == this.oldChangeToken));

                this.VerifySavedFile(this.oldChangeToken, localFile.Object.LastWriteTimeUtc, (DateTime)mappedObject.LastRemoteWriteTimeUtc, expectedHash, fileLength);
                this.manager.Verify(m => m.AddTransmission(It.IsAny<FileTransmissionEvent>()), Times.Never());
            }

            localFile.VerifyThatLocalFileObjectLastWriteTimeUtcIsNeverModified();
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalFileContentChanged() {
            this.SetUpMocks();
            var newModificationDateOnServerAfterUpload = this.modificationDate.AddHours(1);
            int fileLength = 20;
            byte[] content = new byte[fileLength];
            byte[] expectedHash = SHA1Managed.Create().ComputeHash(content);

            var localFile = this.CreateLocalFile(fileLength, this.modificationDate.AddMinutes(1));
            using (var uploadedContent = new MemoryStream()) {
                localFile.Setup(
                    f =>
                    f.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete)).Returns(() => { return new MemoryStream(content); });
                var mappedObject = this.CreateMappedFile(this.modificationDate.AddMinutes(1), fileLength, new byte[20]);

                this.storage.AddMappedFile(mappedObject);
                var remoteFile = MockOfIDocumentUtil.CreateRemoteDocumentMock(null, this.remoteId, this.objectName, this.parentId, fileLength, new byte[fileLength], this.oldChangeToken);
                remoteFile.Setup(r => r.SetContentStream(It.IsAny<IContentStream>(), true, true)).Callback<IContentStream, bool, bool>(
                    (s, o, r) =>
                    { s.Stream.CopyTo(uploadedContent);
                    remoteFile.Setup(f => f.LastModificationDate).Returns(newModificationDateOnServerAfterUpload);
                    remoteFile.Setup(f => f.ChangeToken).Returns(this.newChangeToken);
                });
                remoteFile.SetupUpdateModificationDate();

                this.underTest.Solve(localFile.Object, remoteFile.Object);

                remoteFile.VerifyUpdateLastModificationDate(localFile.Object.LastWriteTimeUtc);
                remoteFile.VerifySetContentStream();
                Assert.That(uploadedContent.ToArray(), Is.EqualTo(content));
                localFile.VerifyThatLocalFileObjectLastWriteTimeUtcIsNeverModified();
                this.manager.Verify(m => m.AddTransmission(It.IsAny<FileTransmissionEvent>()), Times.Once());
                this.VerifySavedFile(this.newChangeToken, localFile.Object.LastWriteTimeUtc, localFile.Object.LastWriteTimeUtc, expectedHash, fileLength);
            }
        }

        [Test, Category("Fast"), Category("Solver")]
        public void PermissionDeniedTriggersNoOperation() {
            this.SetUpMocks();
            int fileLength = 20;
            byte[] content = new byte[fileLength];

            var localFile = this.CreateLocalFile(fileLength, this.modificationDate.AddMinutes(1));
            using (var uploadedContent = new MemoryStream()) {
                localFile.Setup(
                    f =>
                    f.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete)).Returns(() => { return new MemoryStream(content); });
                var mappedObject = this.CreateMappedFile(this.modificationDate.AddMinutes(1), fileLength, new byte[20]);
                this.storage.AddMappedFile(mappedObject);
                var remoteFile = MockOfIDocumentUtil.CreateRemoteDocumentMock(null, this.remoteId, this.objectName, this.parentId, fileLength, new byte[20], this.oldChangeToken);
                remoteFile.Setup(r => r.SetContentStream(It.IsAny<IContentStream>(), true, true)).Throws(new CmisPermissionDeniedException());

                this.underTest.Solve(localFile.Object, remoteFile.Object);

                this.storage.Verify(s => s.SaveMappedObject(It.IsAny<IMappedObject>()), Times.Never());
                remoteFile.VerifySetContentStream();
            }
        }

        [Test, Category("Fast"), Category("Solver")]
        public void StorageExceptionTriggersNoOperation() {
            this.SetUpMocks();
            int fileLength = 20;
            byte[] content = new byte[fileLength];

            var localFile = this.CreateLocalFile(fileLength, this.modificationDate.AddMinutes(1));
            using (var uploadedContent = new MemoryStream()) {
                localFile.Setup(
                    f =>
                    f.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete)).Returns(() => { return new MemoryStream(content); });
                var mappedObject = this.CreateMappedFile(this.modificationDate.AddMinutes(1), fileLength, new byte[20]);
                this.storage.AddMappedFile(mappedObject);
                var remoteFile = MockOfIDocumentUtil.CreateRemoteDocumentMock(null, this.remoteId, this.objectName, this.parentId, fileLength, new byte[20], this.oldChangeToken);
                remoteFile.Setup(r => r.SetContentStream(It.IsAny<IContentStream>(), true, true)).Throws(new CmisStorageException());

                this.underTest.Solve(localFile.Object, remoteFile.Object);

                this.storage.Verify(s => s.SaveMappedObject(It.IsAny<IMappedObject>()), Times.Never());
                remoteFile.VerifySetContentStream();
            }
        }

        [Test, Category("Fast"), Category("Solver")]
        public void PermissionDeniedOnModificationDateSavesTheLocalDate() {
            this.SetUpMocks();
            var localFolder = this.CreateLocalDirectory(this.modificationDate.AddMinutes(1));
            var mappedObject = this.CreateMappedObject(false, this.modificationDate.AddMinutes(1));
            this.storage.AddMappedFolder(mappedObject);
            var remoteFolder = MockOfIFolderUtil.CreateRemoteFolderMock(this.remoteId, this.objectName, this.remotePath, this.parentId, this.oldChangeToken);

            remoteFolder.Setup(r => r.UpdateProperties(It.IsAny<IDictionary<string, object>>(), true)).Throws(new CmisPermissionDeniedException());

            this.underTest.Solve(localFolder.Object, remoteFolder.Object);
            this.VerifySavedFolder(this.oldChangeToken, localFolder.Object.LastWriteTimeUtc, remoteFolder.Object.LastModificationDate);
            remoteFolder.Verify(r => r.UpdateProperties(It.IsAny<IDictionary<string, object>>(), true));
        }

        [Test, Category("Fast"), Category("Solver")]
        public void IgnoreChangesOnNonExistingLocalObject() {
            this.SetUpMocks();
            var localDirectory = new Mock<IDirectoryInfo>();
            localDirectory.Setup(f => f.Exists).Returns(false);

            Assert.Throws<ArgumentException>(() => this.underTest.Solve(localDirectory.Object, Mock.Of<IFolder>()));

            this.storage.Verify(s => s.SaveMappedObject(It.IsAny<IMappedObject>()), Times.Never());
            localDirectory.VerifyThatLocalFileObjectLastWriteTimeUtcIsNeverModified();
            this.manager.Verify(m => m.AddTransmission(It.IsAny<FileTransmissionEvent>()), Times.Never());
        }

        [Test, Category("Fast"), Category("Solver")]
        public void FailsIfRemoteObjectHasBeenChangedSinceLastSync() {
            this.SetUpMocks();
            var mappedObject = this.CreateMappedFile();
            this.storage.AddMappedFile(mappedObject);
            var localFile = this.CreateLocalFile();
            var remoteFile = new Mock<IDocument>(MockBehavior.Strict);
            remoteFile.Setup(r => r.ChangeToken).Returns(this.newChangeToken);
            remoteFile.Setup(r => r.Id).Returns(this.remoteId);

            Assert.Throws<ArgumentException>(() => this.underTest.Solve(localFile.Object, remoteFile.Object));

            localFile.VerifyThatLocalFileObjectLastWriteTimeUtcIsNeverModified();
            this.storage.VerifyThatNoObjectIsManipulated();
        }

        private void SetUpMocks() {
            this.manager = new Mock<ActiveActivitiesManager>() {
                CallBase = true
            };
            this.storage = new Mock<IMetaDataStorage>();
            this.session = new Mock<ISession>();
            this.session.SetupTypeSystem();
            this.underTest = new LocalObjectChanged(this.session.Object, this.storage.Object, null, this.manager.Object);
            this.uuid = Guid.NewGuid();
            this.modificationDate = DateTime.UtcNow;
            this.localPath = Path.Combine("temp", this.objectName);
            this.remotePath = "/temp/" + this.objectName;
        }

        private IMappedObject CreateMappedFolder(DateTime? lastRemoteModification = null) {
            return this.CreateMappedObject(false, lastRemoteModification);
        }

        private IMappedObject CreateMappedFile(DateTime? lastRemoteModification = null, long contentSize = -1, byte[] contentHash = null) {
            return this.CreateMappedObject(true, lastRemoteModification, contentSize, contentHash);
        }

        private IMappedObject CreateMappedObject(bool isFile, DateTime? lastRemoteModification = null, long contentSize = -1, byte[] contentHash = null) {
            var mappedObject = new MappedObject(
                this.objectName,
                this.remoteId,
                isFile ? MappedObjectType.File : MappedObjectType.Folder,
                this.parentId,
                this.oldChangeToken)
            {
                Guid = this.uuid,
                LastRemoteWriteTimeUtc = lastRemoteModification ?? DateTime.UtcNow,
                ChecksumAlgorithmName = isFile ? "SHA-1" : null,
                LastContentSize = isFile ? contentSize > 0 ? contentSize : 0 : -1,
                LastChecksum = contentHash
            };
            return mappedObject;
        }

        private Mock<IDirectoryInfo> CreateLocalDirectory(DateTime modificationDate) {
            var localDirectory = new Mock<IDirectoryInfo>();
            localDirectory.Setup(f => f.LastWriteTimeUtc).Returns(modificationDate.AddMinutes(1));
            localDirectory.Setup(f => f.Exists).Returns(true);
            localDirectory.SetupGuid(this.uuid);
            return localDirectory;
        }

        private void VerifySavedFile(string changeToken, DateTime lastLocalModification, DateTime lastRemoteModification, byte[] hash, long fileLength) {
            this.storage.VerifySavedMappedObject(
                MappedObjectType.File,
                this.remoteId,
                this.objectName,
                this.parentId,
                changeToken,
                true,
                lastLocalModification,
                lastRemoteModification,
                hash,
                fileLength);
        }

        private void VerifySavedFolder(string changeToken, DateTime lastLocalModification, DateTime? lastRemoteModification = null) {
            this.storage.VerifySavedMappedObject(
                MappedObjectType.Folder,
                this.remoteId,
                this.objectName,
                this.parentId,
                changeToken,
                true,
                lastLocalModification,
                lastRemoteModification);
        }

        private Mock<IFileInfo> CreateLocalFile(long? fileLength = 20, DateTime? lastModification = null, bool exists = true) {
            var localFile = new Mock<IFileInfo>();
            localFile.SetupProperty(f => f.LastWriteTimeUtc, lastModification ?? DateTime.UtcNow);
            localFile.Setup(f => f.Length).Returns(fileLength ?? 20);
            localFile.Setup(f => f.FullName).Returns(this.localPath);
            localFile.SetupGuid(this.uuid);
            localFile.Setup(f => f.Exists).Returns(exists);
            return localFile;
        }
    }
}