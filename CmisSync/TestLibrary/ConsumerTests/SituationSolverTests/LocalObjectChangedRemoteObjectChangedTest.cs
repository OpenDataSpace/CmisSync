//-----------------------------------------------------------------------
// <copyright file="LocalObjectChangedRemoteObjectChangedTest.cs" company="GRAU DATA AG">
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
    public class LocalObjectChangedRemoteObjectChangedTest {
        private readonly string remoteId = "remoteId";
        private readonly string parentId = "parentId";
        private readonly string oldChangeToken = "oldChangeToken";
        private readonly string newChangeToken = "newChangeToken";
        private Mock<ISession> session;
        private Mock<IMetaDataStorage> storage;
        private TransmissionManager manager;
        private ITransmissionFactory transmissionFactory;
        private LocalObjectChangedRemoteObjectChanged underTest;

        [Test, Category("Fast"), Category("Solver")]
        public void ConstructorTakesSessionAndStorageAndDateSyncEnabled([Values(true, false)]bool serverCanModifyLastModificationDate) {
            var session = new Mock<ISession>();
            session.SetupTypeSystem(serverCanModifyLastModificationDate: serverCanModifyLastModificationDate);
            new LocalObjectChangedRemoteObjectChanged(session.Object, Mock.Of<IMetaDataStorage>(), null, Mock.Of<ITransmissionFactory>());
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalAndRemoteFolderAreChanged([Values(true, false)]bool childrenAreIgnored) {
            this.InitMocks();
            string folderName = "folderName";
            DateTime lastLocalModification = DateTime.UtcNow.AddDays(1);
            DateTime lastRemoteModification = DateTime.UtcNow.AddHours(1);
            var localFolder = Mock.Of<IDirectoryInfo>(f => f.LastWriteTimeUtc == lastLocalModification);
            var remoteFolder = MockOfIFolderUtil.CreateRemoteFolderMock(this.remoteId, folderName, "path", this.parentId, this.newChangeToken, childrenAreIgnored);
            remoteFolder.Setup(f => f.LastModificationDate).Returns(lastRemoteModification);
            var mappedFolder = Mock.Of<IMappedObject>(
                o =>
                o.Name == folderName &&
                o.RemoteObjectId == this.remoteId &&
                o.LastChangeToken == this.oldChangeToken &&
                o.LastLocalWriteTimeUtc == DateTime.UtcNow &&
                o.LastRemoteWriteTimeUtc == DateTime.UtcNow &&
                o.ParentId == this.parentId &&
                o.Type == MappedObjectType.Folder &&
                o.Guid == Guid.NewGuid() &&
                o.LastContentSize == -1);
            this.storage.AddMappedFolder(mappedFolder);

            this.underTest.Solve(localFolder, remoteFolder.Object, ContentChangeType.NONE, ContentChangeType.NONE);

            this.storage.VerifySavedMappedObject(MappedObjectType.Folder, this.remoteId, folderName, this.parentId, this.newChangeToken, lastLocalModification: lastLocalModification, lastRemoteModification: lastRemoteModification, ignored: childrenAreIgnored);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalAndRemoteFileDatesAreChangedAndLocalDateIsNewerUpdatesRemoteDate() {
            this.InitMocks();
            string fileName = "fileName";
            DateTime lastLocalModification = DateTime.UtcNow.AddDays(1);
            DateTime lastRemoteModification = DateTime.UtcNow.AddHours(1);
            var localFile = new Mock<IFileInfo>();
            byte[] contentHash;
            using (var stream = this.SetUpFileWithContent(localFile, "content", out contentHash, lastLocalModification)) {
                long length = stream.Length;
                var remoteFile = this.CreateRemoteDocument(lastRemoteModification, length, contentHash);
                var mappedObject = new MappedObject(fileName, this.remoteId, MappedObjectType.File, this.parentId, this.oldChangeToken, length) {
                    Guid = Guid.NewGuid(),
                    LastChecksum = contentHash,
                    LastLocalWriteTimeUtc = DateTime.UtcNow,
                    LastRemoteWriteTimeUtc = DateTime.UtcNow,
                    ChecksumAlgorithmName = "SHA-1"
                };
                this.storage.AddMappedFile(mappedObject);

                this.underTest.Solve(localFile.Object, remoteFile.Object, ContentChangeType.NONE, ContentChangeType.NONE);

                remoteFile.VerifyUpdateLastModificationDate(lastLocalModification, Times.Once(), true);
                localFile.VerifyThatLocalFileObjectLastWriteTimeUtcIsNeverModified();
                this.storage.VerifySavedMappedObject(MappedObjectType.File, this.remoteId, fileName, this.parentId, this.newChangeToken, lastLocalModification: lastLocalModification, lastRemoteModification: lastRemoteModification, checksum: contentHash, contentSize: length);
            }
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalAndRemoteFileDatesAreChangedAndRemoteDateIsNewerUpdatesLocalDate() {
            this.InitMocks();
            string fileName = "fileName";
            DateTime lastLocalModification = DateTime.UtcNow.AddHours(1);
            DateTime lastRemoteModification = DateTime.UtcNow.AddDays(1);
            var localFile = new Mock<IFileInfo>();
            byte[] contentHash;
            using (var stream = this.SetUpFileWithContent(localFile, "content", out contentHash, lastLocalModification)) {
                long length = stream.Length;
                var remoteFile = this.CreateRemoteDocument(lastRemoteModification, length, contentHash);
                var mappedObject = new MappedObject(fileName, this.remoteId, MappedObjectType.File, this.parentId, this.oldChangeToken, length) {
                    Guid = Guid.NewGuid(),
                    LastChecksum = contentHash,
                    LastLocalWriteTimeUtc = DateTime.UtcNow,
                    LastRemoteWriteTimeUtc = DateTime.UtcNow,
                    ChecksumAlgorithmName = "SHA-1"
                };
                this.storage.AddMappedFile(mappedObject);

                this.underTest.Solve(localFile.Object, remoteFile.Object, ContentChangeType.NONE, ContentChangeType.NONE);

                remoteFile.VerifyUpdateLastModificationDate(lastLocalModification, Times.Never(), true);
                localFile.VerifySet(f => f.LastWriteTimeUtc = lastRemoteModification);
                this.storage.VerifySavedMappedObject(MappedObjectType.File, this.remoteId, fileName, this.parentId, this.newChangeToken, lastLocalModification: lastLocalModification, lastRemoteModification: lastRemoteModification, checksum: contentHash, contentSize: length);
            }
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalAndRemoteContentIsChangedToTheSameContentAndRemoteDateIsNewerUpdatesLocalDate() {
            this.InitMocks();
            string fileName = "fileName";
            DateTime lastLocalModification = DateTime.UtcNow.AddHours(1);
            DateTime lastRemoteModification = DateTime.UtcNow.AddDays(1);
            var localFile = new Mock<IFileInfo>();
            byte[] contentHash;
            using (var stream = this.SetUpFileWithContent(localFile, "newcontent", out contentHash, lastLocalModification)) {
                long length = stream.Length;
                var remoteFile = this.CreateRemoteDocument(lastRemoteModification, length, contentHash);
                var mappedObject = new MappedObject(fileName, this.remoteId, MappedObjectType.File, this.parentId, this.oldChangeToken, length) {
                    Guid = Guid.NewGuid(),
                    LastChecksum = new byte[20],
                    LastLocalWriteTimeUtc = DateTime.UtcNow,
                    LastRemoteWriteTimeUtc = DateTime.UtcNow,
                    ChecksumAlgorithmName = "SHA-1"
                };
                this.storage.AddMappedFile(mappedObject);

                this.underTest.Solve(localFile.Object, remoteFile.Object, ContentChangeType.NONE, ContentChangeType.CHANGED);

                remoteFile.VerifyUpdateLastModificationDate(lastLocalModification, Times.Never(), true);
                localFile.VerifySet(f => f.LastWriteTimeUtc = lastRemoteModification);
                this.storage.VerifySavedMappedObject(MappedObjectType.File, this.remoteId, fileName, this.parentId, this.newChangeToken, lastLocalModification: lastLocalModification, lastRemoteModification: lastRemoteModification, checksum: contentHash, contentSize: length);
            }
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalAndRemoteContentIsChangedToTheSameContentAndLocalDateIsNewerUpdatesRemoteDate() {
            this.InitMocks();
            string fileName = "fileName";
            DateTime lastLocalModification = DateTime.UtcNow.AddDays(1);
            DateTime lastRemoteModification = DateTime.UtcNow.AddHours(1);
            var localFile = new Mock<IFileInfo>();
            byte[] contentHash;
            using (var stream = this.SetUpFileWithContent(localFile, "newcontent", out contentHash, lastLocalModification)) {
                long length = stream.Length;
                var remoteFile = this.CreateRemoteDocument(lastRemoteModification, length, contentHash);
                var mappedObject = new MappedObject(fileName, this.remoteId, MappedObjectType.File, this.parentId, this.oldChangeToken, length) {
                    Guid = Guid.NewGuid(),
                    LastChecksum = new byte[20],
                    LastLocalWriteTimeUtc = DateTime.UtcNow,
                    LastRemoteWriteTimeUtc = DateTime.UtcNow,
                    ChecksumAlgorithmName = "SHA-1"
                };
                this.storage.AddMappedFile(mappedObject);

                this.underTest.Solve(localFile.Object, remoteFile.Object, ContentChangeType.NONE, ContentChangeType.CHANGED);

                remoteFile.VerifyUpdateLastModificationDate(lastLocalModification, Times.Once(), true);
                localFile.VerifyThatLocalFileObjectLastWriteTimeUtcIsNeverModified();
                this.storage.VerifySavedMappedObject(MappedObjectType.File, this.remoteId, fileName, this.parentId, this.newChangeToken, lastLocalModification: lastLocalModification, lastRemoteModification: lastRemoteModification, checksum: contentHash, contentSize: length);
            }
        }

        private void InitMocks(bool dateSyncEnabled = true) {
            this.session = new Mock<ISession>();
            this.session.SetupTypeSystem();
            this.storage = new Mock<IMetaDataStorage>();
            this.manager = new TransmissionManager();
            this.transmissionFactory = this.manager.CreateFactory();
            this.underTest = new LocalObjectChangedRemoteObjectChanged(this.session.Object, this.storage.Object, null, this.transmissionFactory);
        }

        private Mock<IDocument> CreateRemoteDocument(DateTime lastRemoteModification, long contentLength, byte[] contentHash) {
            var remoteFile = new Mock<IDocument>();
            remoteFile.SetupGet(d => d.LastModificationDate).Returns(lastRemoteModification);
            remoteFile.SetupGet(d => d.Id).Returns(this.remoteId);
            remoteFile.SetupContentStreamHash(contentHash);
            remoteFile.SetupGet(d => d.ChangeToken).Returns(this.newChangeToken);
            return remoteFile;
        }

        private Stream SetUpFileWithContent(Mock<IFileInfo> file, string content, out byte[] hash, DateTime lastModification) {
            byte[] bytes = Encoding.UTF8.GetBytes(content);
            hash = SHA1.Create().ComputeHash(bytes);
            var stream = new MemoryStream(bytes);
            file.Setup(f => f.Length).Returns(bytes.Length);
            file.Setup(f => f.Exists).Returns(true);
            file.Setup(f => f.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete)).Returns(stream);
            file.Setup(f => f.LastWriteTimeUtc).Returns(lastModification);
            return stream;
        }
    }
}