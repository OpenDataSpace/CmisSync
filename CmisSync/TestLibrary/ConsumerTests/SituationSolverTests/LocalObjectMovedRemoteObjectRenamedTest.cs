//-----------------------------------------------------------------------
// <copyright file="LocalObjectMovedRemoteObjectRenamedTest.cs" company="GRAU DATA AG">
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
    public class LocalObjectMovedRemoteObjectRenamedTest
    {
        private readonly string oldName = "oldName";
        private readonly string newName = "newName";
        private readonly string remoteId = "remoteId";
        private readonly string oldParentId = "oldParentId";
        private readonly string changeToken = "changeToken";
        private readonly string newParentName = "newParent";
        private readonly string newParentId = "newParentId";
        private readonly string differentLocalName = "newLocalName";
        private readonly long fileLength = 100;

        private string newPath;
        private string newParentPath;
        private Guid localUuid = Guid.NewGuid();
        private Guid newParentUuid = Guid.NewGuid();
        private TransmissionManager manager;
        private Mock<ISession> session;
        private Mock<IMetaDataStorage> storage;
        private Mock<IFileSystemInfoFactory> fsFactory;
        private Mock<LocalObjectChangedRemoteObjectChanged> changeSolver;
        private Mock<LocalObjectRenamedRemoteObjectRenamed> renameSolver;
        private LocalObjectMovedRemoteObjectRenamed underTest;

        [Test, Category("Fast"), Category("Solver")]
        public void ConstructorThrowsExceptionIfRenameSolverIsNull() {
            this.SetUpMocks();
            Assert.Throws<ArgumentNullException>(() => new LocalObjectMovedRemoteObjectRenamed(this.session.Object, this.storage.Object, null, this.renameSolver.Object));
        }

        [Test, Category("Fast"), Category("Solver")]
        public void ConstructorThrowsExceptionIfChangeSolverIsNull() {
            this.SetUpMocks();
            Assert.Throws<ArgumentNullException>(() => new LocalObjectMovedRemoteObjectRenamed(this.session.Object, this.storage.Object, this.changeSolver.Object, null));
        }

        [Test, Category("Fast"), Category("Solver")]
        public void ConstructorTakesSolver() {
            this.SetUpMocks();
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalFileMoveRemoteRename() {
            this.SetUpMocks();
            var file = this.CreateLocalFileAndInitStorage();
            var doc = this.CreateRemoteDoc();

            this.underTest.Solve(file.Object, doc.Object, ContentChangeType.CHANGED, ContentChangeType.CHANGED);

            doc.Verify(d => d.Move(It.Is<IObjectId>(o => o.Id == this.oldParentId), It.Is<IObjectId>(o => o.Id == this.newParentId)), Times.Once());
            file.Verify(f => f.MoveTo(this.newPath), Times.Once());
            this.VerifySavedFile(this.newName);
            this.VerifyThatChangeSolverIsCalled(file.Object, doc.Object, ContentChangeType.CHANGED, ContentChangeType.CHANGED);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalFolderMoveRemoteRename([Values(true, false)]bool childrenAreIgnored) {
            this.SetUpMocks();
            var dir = this.CreateLocalDirAndInitStorage();
            var folder = this.CreateRemoteFolder(childrenAreIgnored);

            this.underTest.Solve(dir.Object, folder.Object, ContentChangeType.NONE, ContentChangeType.NONE);

            folder.Verify(d => d.Move(It.Is<IObjectId>(o => o.Id == this.oldParentId), It.Is<IObjectId>(o => o.Id == this.newParentId)), Times.Once());
            dir.Verify(f => f.MoveTo(this.newPath), Times.Once());
            this.VerifySavedFolder(this.newName, childrenAreIgnored);
            this.VerifyThatChangeSolverIsCalled(dir.Object, folder.Object);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalFileMoveAndRenameRemoteRename() {
            this.SetUpMocks();
            var file = this.CreateLocalFileAndInitStorage(this.differentLocalName);
            var doc = this.CreateRemoteDoc();

            this.underTest.Solve(file.Object, doc.Object, ContentChangeType.CHANGED, ContentChangeType.CHANGED);

            doc.Verify(d => d.Move(It.Is<IObjectId>(o => o.Id == this.oldParentId), It.Is<IObjectId>(o => o.Id == this.newParentId)), Times.Once());
            file.Verify(f => f.MoveTo(this.newPath), Times.Never());
            this.VerifySavedFile(this.oldName);
            this.VerifyThatRenameSolverIsCalled(file.Object, doc.Object, ContentChangeType.CHANGED, ContentChangeType.CHANGED);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalFileMoveAndRenameRemoteRenameToSameName() {
            this.SetUpMocks();
            var file = this.CreateLocalFileAndInitStorage(this.newName);
            var doc = this.CreateRemoteDoc();

            this.underTest.Solve(file.Object, doc.Object, ContentChangeType.CHANGED, ContentChangeType.CHANGED);

            doc.Verify(d => d.Move(It.Is<IObjectId>(o => o.Id == this.oldParentId), It.Is<IObjectId>(o => o.Id == this.newParentId)), Times.Once());
            file.Verify(f => f.MoveTo(this.newPath), Times.Never());
            this.VerifySavedFile(this.newName);
            this.VerifyThatChangeSolverIsCalled(file.Object, doc.Object, ContentChangeType.CHANGED, ContentChangeType.CHANGED);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalFolderMoveAndRenameRemoteRename([Values(true, false)]bool childrenAreIgnored) {
            this.SetUpMocks();
            var dir = this.CreateLocalDirAndInitStorage(this.differentLocalName);
            var folder = this.CreateRemoteFolder(childrenAreIgnored);

            this.underTest.Solve(dir.Object, folder.Object, ContentChangeType.NONE, ContentChangeType.NONE);

            folder.Verify(d => d.Move(It.Is<IObjectId>(o => o.Id == this.oldParentId), It.Is<IObjectId>(o => o.Id == this.newParentId)), Times.Once());
            dir.Verify(f => f.MoveTo(It.IsAny<string>()), Times.Never);
            this.VerifySavedFolder(this.oldName, childrenAreIgnored);
            this.VerifyThatRenameSolverIsCalled(dir.Object, folder.Object);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalFolderMoveAndRenameRemoteRenameToSameNewName([Values(true, false)]bool childrenAreIgnored) {
            this.SetUpMocks();
            var dir = this.CreateLocalDirAndInitStorage(this.newName);
            var folder = this.CreateRemoteFolder(childrenAreIgnored);

            this.underTest.Solve(dir.Object, folder.Object, ContentChangeType.NONE, ContentChangeType.NONE);

            folder.Verify(d => d.Move(It.Is<IObjectId>(o => o.Id == this.oldParentId), It.Is<IObjectId>(o => o.Id == this.newParentId)), Times.Once());
            dir.Verify(f => f.MoveTo(It.IsAny<string>()), Times.Never);
            this.VerifySavedFolder(this.newName, childrenAreIgnored);
            this.VerifyThatChangeSolverIsCalled(dir.Object, folder.Object);
        }

        private void SetUpMocks() {
            this.newParentPath = Path.Combine(Path.GetTempPath(), this.newParentName);
            this.newPath = Path.Combine(this.newParentPath, this.newName);
            this.manager = new TransmissionManager();
            this.session = new Mock<ISession>();
            this.session.SetupTypeSystem();
            this.storage = new Mock<IMetaDataStorage>();
            var newParentObj = new MappedObject(
                this.newParentName,
                this.newParentId,
                MappedObjectType.Folder,
                null,
                this.changeToken)
            {
                Guid = this.newParentUuid
            };
            this.storage.AddMappedFolder(newParentObj);
            this.fsFactory = new Mock<IFileSystemInfoFactory>();
            this.changeSolver = new Mock<LocalObjectChangedRemoteObjectChanged>(
                this.session.Object,
                this.storage.Object,
                null,
                this.manager,
                this.fsFactory.Object);
            this.renameSolver = new Mock<LocalObjectRenamedRemoteObjectRenamed>(
                this.session.Object,
                this.storage.Object,
                this.changeSolver.Object);
            this.session.AddRemoteObjects(Mock.Of<IFolder>(o => o.Id == this.oldParentId), Mock.Of<IFolder>(o => o.Id == this.newParentId));
            this.underTest = new LocalObjectMovedRemoteObjectRenamed(this.session.Object, this.storage.Object, this.changeSolver.Object, this.renameSolver.Object);
        }

        private Mock<IFileInfo> CreateLocalFileAndInitStorage(string newName = null) {
            var file = Mock.Of<IFileInfo>(
                f =>
                f.Uuid == this.localUuid &&
                f.Name == (newName ?? this.oldName) &&
                f.Directory.Uuid == this.newParentUuid &&
                f.Directory.FullName == this.newParentPath);
            var mock = Mock.Get(file);
            mock.Setup(
                f =>
                f.MoveTo(this.newPath))
                .Callback(
                    () =>
                    {
                    mock.SetupGet(f => f.Name).Returns(this.newName);
                    mock.SetupGet(f => f.FullName).Returns(this.newPath);
                });
            var obj = new MappedObject(this.oldName, this.remoteId, MappedObjectType.File, this.oldParentId, this.changeToken, this.fileLength) { Guid = this.localUuid };
            this.storage.AddMappedFile(obj);
            return mock;
        }

        private Mock<IDirectoryInfo> CreateLocalDirAndInitStorage(string newName = null) {
            var dir = Mock.Of<IDirectoryInfo>(
                d =>
                d.Uuid == this.localUuid &&
                d.Name == (newName ?? this.oldName) &&
                d.Parent.Uuid == this.newParentUuid &&
                d.Parent.FullName == this.newParentPath);
            var mock = Mock.Get(dir);
            mock.Setup(
                f =>
                f.MoveTo(this.newPath))
                .Callback(
                    () =>
                    {
                    mock.SetupGet(f => f.Name).Returns(this.newName);
                    mock.SetupGet(f => f.FullName).Returns(this.newPath);
                });
            var obj = new MappedObject(this.oldName, this.remoteId, MappedObjectType.Folder, this.oldParentId, this.changeToken) { Guid = this.localUuid };
            this.storage.AddMappedFile(obj);
            return mock;
        }

        private Mock<IFolder> CreateRemoteFolder(bool ignored) {
            var folder = MockOfIFolderUtil.CreateRemoteFolderMock(this.remoteId, this.newName, "path", this.oldParentId, ignored: ignored);
            folder.Setup(
                d =>
                d.Move(It.Is<IObjectId>(o => o.Id == this.oldParentId), It.Is<IObjectId>(o => o.Id == this.newParentId)))
                .Returns(folder.Object);
            return folder;
        }

        private Mock<IDocument> CreateRemoteDoc() {
            var doc = Mock.Of<IDocument>(
                d =>
                d.Id == this.remoteId &&
                d.Name == this.newName);
            var mock = Mock.Get(doc);
            mock.Setup(
                d =>
                d.Move(It.Is<IObjectId>(o => o.Id == this.oldParentId), It.Is<IObjectId>(o => o.Id == this.newParentId)))
                .Returns(doc);
            return mock;
        }

        private void VerifySavedFolder(string name, bool ignored) {
            this.storage.VerifySavedMappedObject(MappedObjectType.Folder, this.remoteId, name, this.newParentId, this.changeToken, ignored: ignored);
        }

        private void VerifySavedFile(string name) {
            this.storage.VerifySavedMappedObject(MappedObjectType.File, this.remoteId, name, this.newParentId, this.changeToken, contentSize: this.fileLength);
        }

        private void VerifyThatChangeSolverIsCalled(
            IFileSystemInfo localObject,
            ICmisObject remoteObject,
            ContentChangeType localChange = ContentChangeType.NONE,
            ContentChangeType remoteChange = ContentChangeType.NONE) {
            this.changeSolver.Verify(s => s.Solve(localObject, remoteObject, localChange, remoteChange), Times.Once());
            this.renameSolver.Verify(s => s.Solve(It.IsAny<IFileSystemInfo>(), It.IsAny<IObjectId>(), It.IsAny<ContentChangeType>(), It.IsAny<ContentChangeType>()), Times.Never());
        }

        private void VerifyThatRenameSolverIsCalled(
            IFileSystemInfo localObject,
            ICmisObject remoteObject,
            ContentChangeType localChange = ContentChangeType.NONE,
            ContentChangeType remoteChange = ContentChangeType.NONE) {
            this.changeSolver.Verify(s => s.Solve(It.IsAny<IFileSystemInfo>(), It.IsAny<IObjectId>(), It.IsAny<ContentChangeType>(), It.IsAny<ContentChangeType>()), Times.Never());
            this.renameSolver.Verify(s => s.Solve(localObject, remoteObject, localChange, remoteChange), Times.Once());
        }
    }
}