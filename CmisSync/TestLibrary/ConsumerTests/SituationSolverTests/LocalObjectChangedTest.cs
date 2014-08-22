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
        private Mock<ActiveActivitiesManager> manager;
        private Mock<IMetaDataStorage> storage;
        private Mock<ISession> session;
        private LocalObjectChanged underTest;
        private Mock<ISyncEventQueue> queue;

        [SetUp]
        public void SetUp() {
            this.manager = new Mock<ActiveActivitiesManager>() {
                CallBase = true
            };
            this.storage = new Mock<IMetaDataStorage>();
            this.session = new Mock<ISession>();
            this.queue = new Mock<ISyncEventQueue>();
            this.underTest = new LocalObjectChanged(this.session.Object, this.storage.Object, this.queue.Object, this.manager.Object, true);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void DefaultConstructorTest()
        {
            new LocalObjectChanged(this.session.Object, this.storage.Object, this.queue.Object, this.manager.Object);
        }

        [Test, Category("Fast"), Category("Solver")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorThrowsExceptionIfQueueIsNull() {
            new LocalObjectChanged(this.session.Object, this.storage.Object, null, this.manager.Object);
        }

        [Test, Category("Fast"), Category("Solver")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorThrowsExceptionIfTransmissionManagerIsNull() {
            new LocalObjectChanged(this.session.Object, this.storage.Object, this.queue.Object, null);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalFolderChanged()
        {
            var modificationDate = DateTime.UtcNow;
            var localDirectory = new Mock<IDirectoryInfo>();
            localDirectory.Setup(f => f.LastWriteTimeUtc).Returns(modificationDate.AddMinutes(1));
            localDirectory.Setup(f => f.Exists).Returns(true);

            var mappedObject = new MappedObject(
                "name",
                "remoteId",
                MappedObjectType.Folder,
                "parentId",
                "changeToken")
            {
                Guid = Guid.NewGuid(),
                LastRemoteWriteTimeUtc = modificationDate.AddMinutes(1)
            };
            this.storage.AddMappedFolder(mappedObject);

            this.underTest.Solve(localDirectory.Object, Mock.Of<IFolder>());

            this.storage.VerifySavedMappedObject(
                MappedObjectType.Folder,
                "remoteId",
                mappedObject.Name,
                mappedObject.ParentId,
                mappedObject.LastChangeToken,
                true,
                localDirectory.Object.LastWriteTimeUtc);
            this.queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Never());
            localDirectory.VerifyThatLocalFileObjectLastWriteTimeUtcIsNeverModified();
            this.manager.Verify(m => m.AddTransmission(It.IsAny<FileTransmissionEvent>()), Times.Never());
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalFolderChangedFetchByGuidInExtendedAttribute()
        {
            Guid guid = Guid.NewGuid();
            var modificationDate = DateTime.UtcNow;
            var localDirectory = new Mock<IDirectoryInfo>();
            localDirectory.Setup(f => f.LastWriteTimeUtc).Returns(modificationDate.AddMinutes(1));
            localDirectory.Setup(f => f.Exists).Returns(true);
            localDirectory.Setup(f => f.GetExtendedAttribute(MappedObject.ExtendedAttributeKey)).Returns(guid.ToString());

            var mappedObject = new MappedObject(
                "name",
                "remoteId",
                MappedObjectType.Folder,
                "parentId",
                "changeToken")
            {
                Guid = guid,
                LastRemoteWriteTimeUtc = modificationDate.AddMinutes(1)
            };
            this.storage.AddMappedFolder(mappedObject);

            this.underTest.Solve(localDirectory.Object, Mock.Of<IFolder>());

            this.storage.VerifySavedMappedObject(
                MappedObjectType.Folder,
                "remoteId",
                mappedObject.Name,
                mappedObject.ParentId,
                mappedObject.LastChangeToken,
                true,
                localDirectory.Object.LastWriteTimeUtc);
            this.queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Never());
            localDirectory.VerifyThatLocalFileObjectLastWriteTimeUtcIsNeverModified();
            this.manager.Verify(m => m.AddTransmission(It.IsAny<FileTransmissionEvent>()), Times.Never());
            this.storage.Verify(s => s.GetObjectByLocalPath(It.IsAny<IFileSystemInfo>()), Times.Never());
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalFileModificationDateNotWritableShallNotThrow()
        {
            var modificationDate = DateTime.UtcNow;
            var newModificationDate = modificationDate.AddHours(1);
            var newChangeToken = "newChangeToken";
            int fileLength = 20;
            byte[] content = new byte[fileLength];

            var localFile = new Mock<IFileInfo>();
            localFile.SetupProperty(f => f.LastWriteTimeUtc, modificationDate.AddMinutes(1));
            localFile.SetupSet(f => f.LastWriteTimeUtc = It.IsAny<DateTime>()).Throws(new IOException());
            localFile.Setup(f => f.Length).Returns(fileLength);
            localFile.Setup(f => f.FullName).Returns("path");
            localFile.Setup(f => f.Exists).Returns(true);
            using (var uploadedContent = new MemoryStream()) {
                localFile.Setup(
                    f =>
                    f.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete)).Returns(() => { return new MemoryStream(content); });

                var mappedObject = new MappedObject(
                    "name",
                    "remoteId",
                    MappedObjectType.File,
                    "parentId",
                    "changeToken",
                    fileLength)
                {
                    Guid = Guid.NewGuid(),
                    LastRemoteWriteTimeUtc = modificationDate.AddMinutes(1),
                    LastLocalWriteTimeUtc = modificationDate,
                    LastChecksum = new byte[20],
                    ChecksumAlgorithmName = "SHA-1"
                };
                this.storage.AddMappedFile(mappedObject, "path");
                var remoteFile = MockOfIDocumentUtil.CreateRemoteDocumentMock(null, "remoteId", "name", "parentId", fileLength, new byte[20]);
                remoteFile.Setup(r => r.SetContentStream(It.IsAny<IContentStream>(), true, true)).Callback<IContentStream, bool, bool>(
                    (s, o, r) =>
                    { s.Stream.CopyTo(uploadedContent);
                    remoteFile.Setup(f => f.LastModificationDate).Returns(newModificationDate);
                    remoteFile.Setup(f => f.ChangeToken).Returns(newChangeToken);
                });

                this.underTest.Solve(localFile.Object, remoteFile.Object);
            }

            localFile.VerifyThatLocalFileObjectLastWriteTimeUtcIsNeverModified();
            this.manager.Verify(m => m.AddTransmission(It.IsAny<FileTransmissionEvent>()), Times.Once());
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalFileModificationDateChanged()
        {
            string path = "path";
            var modificationDate = DateTime.UtcNow;
            int fileLength = 20;
            byte[] content = new byte[fileLength];
            byte[] expectedHash = SHA1Managed.Create().ComputeHash(content);
            var localFile = new Mock<IFileInfo>();
            localFile.SetupProperty(f => f.LastWriteTimeUtc, modificationDate.AddMinutes(1));
            localFile.Setup(f => f.Length).Returns(fileLength);
            localFile.Setup(f => f.FullName).Returns(path);
            localFile.Setup(f => f.Exists).Returns(true);

            using (var stream = new MemoryStream(content)) {
                localFile.Setup(
                    f =>
                    f.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete)).Returns(stream);

                var mappedObject = new MappedObject(
                    "name",
                    "remoteId",
                    MappedObjectType.File,
                    "parentId",
                    "changeToken",
                    fileLength)
                {
                    Guid = Guid.NewGuid(),
                    LastRemoteWriteTimeUtc = modificationDate.AddMinutes(1),
                    LastLocalWriteTimeUtc = modificationDate,
                    LastChecksum = expectedHash,
                    ChecksumAlgorithmName = "SHA-1"
                };

                this.storage.AddMappedFile(mappedObject, path);

                this.underTest.Solve(localFile.Object, Mock.Of<IDocument>());

                this.storage.VerifySavedMappedObject(
                    MappedObjectType.File,
                    "remoteId",
                    mappedObject.Name,
                    mappedObject.ParentId,
                    mappedObject.LastChangeToken,
                    true,
                    localFile.Object.LastWriteTimeUtc,
                    mappedObject.LastRemoteWriteTimeUtc,
                    expectedHash,
                    fileLength);
                this.queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Never());
                this.manager.Verify(m => m.AddTransmission(It.IsAny<FileTransmissionEvent>()), Times.Never());
            }

            localFile.VerifyThatLocalFileObjectLastWriteTimeUtcIsNeverModified();
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalFileContentChanged()
        {
            Guid uuid = Guid.NewGuid();
            var modificationDate = DateTime.UtcNow;
            var newModificationDate = modificationDate.AddHours(1);
            var newChangeToken = "newChangeToken";
            int fileLength = 20;
            byte[] content = new byte[fileLength];
            byte[] expectedHash = SHA1Managed.Create().ComputeHash(content);

            var localFile = new Mock<IFileInfo>();
            localFile.SetupProperty(f => f.LastWriteTimeUtc, modificationDate.AddMinutes(1));
            localFile.Setup(f => f.Length).Returns(fileLength);
            localFile.Setup(f => f.FullName).Returns("path");
            localFile.SetupGuid(uuid);
            localFile.Setup(f => f.Exists).Returns(true);
            using (var uploadedContent = new MemoryStream()) {
                localFile.Setup(
                    f =>
                    f.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete)).Returns(() => { return new MemoryStream(content); });

                var mappedObject = new MappedObject(
                    "name",
                    "remoteId",
                    MappedObjectType.File,
                    "parentId",
                    "changeToken",
                    fileLength)
                {
                    Guid = uuid,
                    LastRemoteWriteTimeUtc = modificationDate.AddMinutes(1),
                    LastLocalWriteTimeUtc = modificationDate,
                    LastChecksum = new byte[20],
                    ChecksumAlgorithmName = "SHA-1"
                };
                this.storage.AddMappedFile(mappedObject, "path");
                var remoteFile = MockOfIDocumentUtil.CreateRemoteDocumentMock(null, "remoteId", "name", "parentId", fileLength, new byte[20]);
                remoteFile.Setup(r => r.SetContentStream(It.IsAny<IContentStream>(), true, true)).Callback<IContentStream, bool, bool>(
                    (s, o, r) =>
                    { s.Stream.CopyTo(uploadedContent);
                    remoteFile.Setup(f => f.LastModificationDate).Returns(newModificationDate);
                    remoteFile.Setup(f => f.ChangeToken).Returns(newChangeToken);
                });

                this.underTest.Solve(localFile.Object, remoteFile.Object);

                this.storage.VerifySavedMappedObject(
                    MappedObjectType.File,
                    "remoteId",
                    mappedObject.Name,
                    mappedObject.ParentId,
                    newChangeToken,
                    true,
                    localFile.Object.LastWriteTimeUtc,
                    localFile.Object.LastWriteTimeUtc,
                    expectedHash,
                    fileLength);
                remoteFile.VerifySetContentStream();
                Assert.That(uploadedContent.ToArray(), Is.EqualTo(content));
                localFile.VerifyThatLocalFileObjectLastWriteTimeUtcIsNeverModified();
                this.manager.Verify(m => m.AddTransmission(It.IsAny<FileTransmissionEvent>()), Times.Once());
            }
        }

        [Test, Category("Fast"), Category("Solver")]
        public void PermissionDeniedTriggersNoOperation()
        {
            Guid uuid = Guid.NewGuid();
            var modificationDate = DateTime.UtcNow;
            int fileLength = 20;
            byte[] content = new byte[fileLength];

            var localFile = new Mock<IFileInfo>();
            localFile.SetupProperty(f => f.LastWriteTimeUtc, modificationDate.AddMinutes(1));
            localFile.Setup(f => f.Length).Returns(fileLength);
            localFile.Setup(f => f.FullName).Returns("path");
            localFile.SetupGuid(uuid);
            localFile.Setup(f => f.Exists).Returns(true);
            using (var uploadedContent = new MemoryStream()) {
                localFile.Setup(
                    f =>
                    f.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete)).Returns(() => { return new MemoryStream(content); });

                var mappedObject = new MappedObject(
                    "name",
                    "remoteId",
                    MappedObjectType.File,
                    "parentId",
                    "changeToken",
                    fileLength)
                {
                    Guid = uuid,
                    LastRemoteWriteTimeUtc = modificationDate.AddMinutes(1),
                    LastLocalWriteTimeUtc = modificationDate,
                    LastChecksum = new byte[20],
                    ChecksumAlgorithmName = "SHA-1"
                };
                this.storage.AddMappedFile(mappedObject, "path");
                var remoteFile = MockOfIDocumentUtil.CreateRemoteDocumentMock(null, "remoteId", "name", "parentId", fileLength, new byte[20]);

                remoteFile.Setup(r => r.SetContentStream(It.IsAny<IContentStream>(), true, true)).Throws(new CmisPermissionDeniedException());

                this.underTest.Solve(localFile.Object, remoteFile.Object);

                this.storage.Verify(s => s.SaveMappedObject(It.IsAny<IMappedObject>()), Times.Never());
                remoteFile.VerifySetContentStream();
            }
        }

        [Test, Category("Fast"), Category("Solver")]
        public void PermissionDeniedOnModificationDateSavesTheLocalDate()
        {
            Guid uuid = Guid.NewGuid();
            var modificationDate = DateTime.UtcNow;

            var localFolder = new Mock<IDirectoryInfo>();
            localFolder.SetupProperty(f => f.LastWriteTimeUtc, modificationDate.AddMinutes(1));
            localFolder.Setup(f => f.FullName).Returns("path");
            localFolder.SetupGuid(uuid);
            localFolder.Setup(f => f.Exists).Returns(true);

            var mappedObject = new MappedObject(
                "name",
                "remoteId",
                MappedObjectType.Folder,
                "parentId",
                "changeToken")
            {
                Guid = uuid,
                LastRemoteWriteTimeUtc = modificationDate.AddMinutes(1),
                LastLocalWriteTimeUtc = modificationDate,
            };
            this.storage.AddMappedFolder(mappedObject, "path");
            var remoteFolder = MockOfIFolderUtil.CreateRemoteFolderMock("remoteId", "name", "path", "parentId");

            remoteFolder.Setup(r => r.UpdateProperties(It.IsAny<IDictionary<string, object>>(), true)).Throws(new CmisPermissionDeniedException());

            this.underTest.Solve(localFolder.Object, remoteFolder.Object);

            this.storage.VerifySavedMappedObject(MappedObjectType.Folder, "remoteId", "name", "parentId", "changeToken", true, localFolder.Object.LastWriteTimeUtc, remoteFolder.Object.LastModificationDate);
            remoteFolder.Verify(r => r.UpdateProperties(It.IsAny<IDictionary<string, object>>(), true));
        }

        [Test, Category("Fast"), Category("Solver")]
        public void IgnoreChangesOnNonExistingLocalObject() {
            var localDirectory = new Mock<IDirectoryInfo>();
            localDirectory.Setup(f => f.Exists).Returns(false);

            Assert.Throws<ArgumentException>(() => this.underTest.Solve(localDirectory.Object, Mock.Of<IFolder>()));

            this.storage.Verify(s => s.SaveMappedObject(It.IsAny<IMappedObject>()), Times.Never());
            this.queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Never());
            localDirectory.VerifyThatLocalFileObjectLastWriteTimeUtcIsNeverModified();
            this.manager.Verify(m => m.AddTransmission(It.IsAny<FileTransmissionEvent>()), Times.Never());
        }
    }
}
