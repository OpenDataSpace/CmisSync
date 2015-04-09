//-----------------------------------------------------------------------
// <copyright file="LocalObjectRenamedTest.cs" company="GRAU DATA AG">
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

    using CmisSync.Lib.Consumer;
    using CmisSync.Lib.Consumer.SituationSolver;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;
    using DotCMIS.Exceptions;

    using Moq;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture]
    public class LocalObjectRenamedTest {
        private readonly string oldName = "oldName";
        private readonly string newName = "newName";
        private readonly string id = "id";
        private readonly string newChangeToken = "newChange";
        private readonly DateTime modificationDate = DateTime.UtcNow;
        private readonly DateTime newModificationDate = DateTime.UtcNow.AddMinutes(1);

        private Mock<IMetaDataStorage> storage;
        private Mock<ISession> session;
        private LocalObjectRenamed underTest;

        [SetUp]
        public void SetUp() {
            this.storage = new Mock<IMetaDataStorage>();
            this.session = new Mock<ISession>();
            this.session.SetupTypeSystem();
            this.underTest = new LocalObjectRenamed(this.session.Object, this.storage.Object);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void DefaultConstructorTest() {
            new LocalObjectRenamed(this.session.Object, this.storage.Object);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void PermissionDeniedLeadsToNoOperation() {
            var remoteFolder = new Mock<IFolder>();
            remoteFolder.Setup(f => f.Name).Returns(this.oldName);
            remoteFolder.Setup(f => f.Id).Returns(this.id);
            remoteFolder.Setup(f => f.Rename(this.newName, true)).Throws(new CmisPermissionDeniedException());
            var localFolder = new Mock<IDirectoryInfo>();
            localFolder.SetupProperty(f => f.LastWriteTimeUtc, this.modificationDate);
            localFolder.Setup(f => f.Name).Returns(this.newName);
            var mappedFolder = new Mock<IMappedObject>();
            mappedFolder.SetupAllProperties();
            mappedFolder.SetupProperty(f => f.Guid, Guid.NewGuid());
            mappedFolder.SetupProperty(f => f.Name, this.oldName);
            mappedFolder.SetupProperty(f => f.RemoteObjectId, this.id);
            mappedFolder.Setup(f => f.Type).Returns(MappedObjectType.Folder);

            this.storage.AddMappedFolder(mappedFolder.Object);

            this.underTest.Solve(localFolder.Object, remoteFolder.Object);

            this.storage.Verify(f => f.SaveMappedObject(It.IsAny<IMappedObject>()), Times.Never());
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalFolderRenamed([Values(true, false)]bool childrenAreIgnored) {
            var remoteFolder = MockOfIFolderUtil.CreateRemoteFolderMock(this.id, this.oldName, "path", null, changetoken: null, ignored: childrenAreIgnored);
            remoteFolder.Setup(f => f.Rename(this.newName, true)).Callback(
                () => {
                remoteFolder.Setup(f => f.Name).Returns(this.newName);
                remoteFolder.Setup(f => f.ChangeToken).Returns(this.newChangeToken);
                remoteFolder.Setup(f => f.LastModificationDate).Returns(this.newModificationDate);
            }).Returns(Mock.Of<IObjectId>(o => o.Id == this.id));
            var localFolder = new Mock<IDirectoryInfo>();
            localFolder.SetupProperty(f => f.LastWriteTimeUtc, this.modificationDate);
            localFolder.Setup(f => f.Name).Returns(this.newName);
            var mappedFolder = new Mock<IMappedObject>();
            mappedFolder.SetupAllProperties();
            mappedFolder.SetupProperty(f => f.Guid, Guid.NewGuid());
            mappedFolder.SetupProperty(f => f.Name, this.oldName);
            mappedFolder.SetupProperty(f => f.RemoteObjectId, this.id);
            mappedFolder.Setup(f => f.Type).Returns(MappedObjectType.Folder);
            mappedFolder.Setup(f => f.LastContentSize).Returns(-1);
            this.storage.AddMappedFolder(mappedFolder.Object);

            this.underTest.Solve(localFolder.Object, remoteFolder.Object);

            remoteFolder.Verify(f => f.Rename(It.Is<string>(s => s == this.newName), It.Is<bool>(b => b == true)), Times.Once());

            this.storage.VerifySavedMappedObject(MappedObjectType.Folder, this.id, this.newName, null, this.newChangeToken, true, this.modificationDate, this.newModificationDate, ignored: childrenAreIgnored);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalFileRenamed() {
            byte[] content = Encoding.UTF8.GetBytes("content");
            byte[] hash = SHA1.Create().ComputeHash(content);
            using (var contentStream = new MemoryStream(content)) {
                var remoteFile = new Mock<IDocument>();
                remoteFile.Setup(f => f.Name).Returns(this.oldName);
                remoteFile.Setup(f => f.Id).Returns(this.id);
                remoteFile.Setup(f => f.Rename(this.newName, true)).Callback(
                    () => {
                    remoteFile.Setup(f => f.Name).Returns(this.newName);
                    remoteFile.Setup(f => f.ChangeToken).Returns(this.newChangeToken);
                    remoteFile.Setup(f => f.LastModificationDate).Returns(this.modificationDate.AddMinutes(1));
                }).Returns(Mock.Of<IObjectId>(o => o.Id == this.id));
                var localFile = new Mock<IFileInfo>();
                localFile.SetupProperty(f => f.LastWriteTimeUtc, this.modificationDate);
                localFile.Setup(f => f.Exists).Returns(true);
                localFile.Setup(f => f.Name).Returns(this.newName);
                localFile.Setup(f => f.Length).Returns(content.Length);
                localFile.Setup(f => f.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete)).Returns(contentStream);
                var mappedFile = new Mock<IMappedObject>();
                mappedFile.SetupAllProperties();
                mappedFile.SetupProperty(f => f.Guid, Guid.NewGuid());
                mappedFile.SetupProperty(f => f.Name, this.oldName);
                mappedFile.SetupProperty(f => f.RemoteObjectId, this.id);
                mappedFile.Setup(f => f.Type).Returns(MappedObjectType.File);
                mappedFile.Setup(f => f.LastContentSize).Returns(content.Length);
                mappedFile.Setup(f => f.LastChecksum).Returns(hash);

                this.storage.AddMappedFile(mappedFile.Object);

                this.underTest.Solve(localFile.Object, remoteFile.Object);

                remoteFile.Verify(f => f.Rename(It.Is<string>(s => s == this.newName), It.Is<bool>(b => b == true)), Times.Once());

                this.storage.VerifySavedMappedObject(MappedObjectType.File, this.id, this.newName, null, this.newChangeToken, true, this.modificationDate, this.modificationDate.AddMinutes(1), contentSize: content.Length);
            }
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalFileRenamedAndContentLengthIsDifferentThrowsException() {
            byte[] content = Encoding.UTF8.GetBytes("content");
            byte[] hash = SHA1.Create().ComputeHash(content);
            DateTime oldModificationDate = DateTime.UtcNow.AddDays(1);
            using (var contentStream = new MemoryStream(content)) {
                var remoteFile = new Mock<IDocument>();
                remoteFile.Setup(f => f.Name).Returns(this.oldName);
                remoteFile.Setup(f => f.Id).Returns(this.id);
                remoteFile.Setup(f => f.Rename(this.newName, true)).Callback(
                    () => {
                    remoteFile.Setup(f => f.Name).Returns(this.newName);
                    remoteFile.Setup(f => f.ChangeToken).Returns(this.newChangeToken);
                    remoteFile.Setup(f => f.LastModificationDate).Returns(this.newModificationDate);
                }).Returns(Mock.Of<IObjectId>(o => o.Id == this.id));
                var localFile = new Mock<IFileInfo>();
                localFile.SetupProperty(f => f.LastWriteTimeUtc, this.modificationDate);
                localFile.Setup(f => f.Name).Returns(this.newName);
                localFile.Setup(f => f.Length).Returns(content.Length);
                localFile.Setup(f => f.Exists).Returns(true);
                localFile.Setup(f => f.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete)).Returns(contentStream);
                var mappedFile = new Mock<IMappedObject>();
                mappedFile.SetupAllProperties();
                mappedFile.SetupProperty(f => f.Guid, Guid.NewGuid());
                mappedFile.SetupProperty(f => f.Name, this.oldName);
                mappedFile.SetupProperty(f => f.RemoteObjectId, this.id);
                mappedFile.Setup(f => f.Type).Returns(MappedObjectType.File);
                mappedFile.Setup(f => f.LastContentSize).Returns(0);
                mappedFile.Setup(f => f.LastChecksum).Returns(hash);
                mappedFile.Setup(f => f.LastLocalWriteTimeUtc).Returns(oldModificationDate);

                this.storage.AddMappedFile(mappedFile.Object);

                Assert.Throws<ArgumentException>(() => this.underTest.Solve(localFile.Object, remoteFile.Object));

                remoteFile.Verify(f => f.Rename(It.Is<string>(s => s == this.newName), It.Is<bool>(b => b == true)), Times.Once());

                this.storage.VerifySavedMappedObject(MappedObjectType.File, this.id, this.newName, null, this.newChangeToken, true, oldModificationDate, contentSize: 0);
            }
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalFileRenamedAndContentIsDifferentThrowsException() {
            byte[] content = Encoding.UTF8.GetBytes("content");
            DateTime oldModificationDate = DateTime.UtcNow.AddDays(1);
            using (var contentStream = new MemoryStream(content)) {
                var remoteFile = new Mock<IDocument>();
                remoteFile.Setup(f => f.Name).Returns(this.oldName);
                remoteFile.Setup(f => f.Id).Returns(this.id);
                remoteFile.Setup(f => f.Rename(this.newName, true)).Callback(
                    () => {
                    remoteFile.Setup(f => f.Name).Returns(this.newName);
                    remoteFile.Setup(f => f.ChangeToken).Returns(this.newChangeToken);
                    remoteFile.Setup(f => f.LastModificationDate).Returns(this.modificationDate.AddMinutes(1));
                }).Returns(Mock.Of<IObjectId>(o => o.Id == this.id));
                var localFile = new Mock<IFileInfo>();
                localFile.SetupProperty(f => f.LastWriteTimeUtc, this.modificationDate);
                localFile.Setup(f => f.Name).Returns(this.newName);
                localFile.Setup(f => f.Length).Returns(content.Length);
                localFile.Setup(f => f.Exists).Returns(true);
                localFile.Setup(f => f.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete)).Returns(contentStream);
                var mappedFile = new Mock<IMappedObject>();
                mappedFile.SetupAllProperties();
                mappedFile.SetupProperty(f => f.Guid, Guid.NewGuid());
                mappedFile.SetupProperty(f => f.Name, this.oldName);
                mappedFile.SetupProperty(f => f.RemoteObjectId, this.id);
                mappedFile.Setup(f => f.Type).Returns(MappedObjectType.File);
                mappedFile.Setup(f => f.LastContentSize).Returns(content.Length);
                mappedFile.Setup(f => f.LastChecksum).Returns(new byte[20]);
                mappedFile.Setup(f => f.LastLocalWriteTimeUtc).Returns(oldModificationDate);

                this.storage.AddMappedFile(mappedFile.Object);

                Assert.Throws<ArgumentException>(() => this.underTest.Solve(localFile.Object, remoteFile.Object));

                remoteFile.Verify(f => f.Rename(It.Is<string>(s => s == this.newName), It.Is<bool>(b => b == true)), Times.Once());

                this.storage.VerifySavedMappedObject(MappedObjectType.File, this.id, this.newName, null, this.newChangeToken, true, oldModificationDate, contentSize: content.Length);
            }
        }

        [Test, Category("Fast"), Category("Solver")]
        public void ConflictOnUtf8CharacterLeadsToNoSavings() {
            var remoteFolder = new Mock<IFolder>();
            remoteFolder.Setup(f => f.Name).Returns(this.oldName);
            remoteFolder.Setup(f => f.Id).Returns(this.id);
            remoteFolder.Setup(f => f.Rename(@"ä".Normalize(NormalizationForm.FormD), true)).Throws<CmisNameConstraintViolationException>();
            var localFolder = new Mock<IDirectoryInfo>();
            localFolder.SetupProperty(f => f.LastWriteTimeUtc, this.modificationDate);
            localFolder.Setup(f => f.Name).Returns(@"ä".Normalize(NormalizationForm.FormD));
            var mappedFolder = new Mock<IMappedObject>();
            mappedFolder.SetupAllProperties();
            mappedFolder.SetupProperty(f => f.Guid, Guid.NewGuid());
            mappedFolder.SetupProperty(f => f.Name, this.oldName);
            mappedFolder.SetupProperty(f => f.RemoteObjectId, this.id);
            mappedFolder.Setup(f => f.Type).Returns(MappedObjectType.Folder);

            this.storage.AddMappedFolder(mappedFolder.Object);

            Assert.Throws<InteractionNeededException>(() => this.underTest.Solve(localFolder.Object, remoteFolder.Object));

            remoteFolder.Verify(f => f.Rename(It.Is<string>(s => s == @"ä".Normalize(NormalizationForm.FormD)), It.Is<bool>(b => b == true)), Times.Once());

            this.storage.VerifyThatNoObjectIsManipulated();
        }
    }
}