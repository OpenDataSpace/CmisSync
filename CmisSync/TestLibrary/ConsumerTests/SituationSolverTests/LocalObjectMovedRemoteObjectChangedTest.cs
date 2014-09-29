//-----------------------------------------------------------------------
// <copyright file="LocalObjectMovedRemoteObjectChangedTest.cs" company="GRAU DATA AG">
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

    using CmisSync.Lib.Consumer.SituationSolver;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;

    using Moq;

    using NUnit.Framework;

    using TestUtils;

    [TestFixture]
    public class LocalObjectMovedRemoteObjectChangedTest
    {
        private readonly string newParentsName = "newParent";
        private readonly string newParentsId = "newParentId";
        private readonly string oldParentsName = "oldParent";
        private readonly string oldParentsId = "oldParentId";
        private readonly string remoteObjectId = "remoteId";
        private readonly string changeToken = "changeToken";

        private ActiveActivitiesManager manager;
        private Mock<ISession> session;
        private Mock<IMetaDataStorage> storage;
        private Mock<IFileSystemInfoFactory> fsFactory;
        private Mock<LocalObjectChangedRemoteObjectChanged> changeSolver;
        private Mock<LocalObjectRenamedRemoteObjectChanged> renameSolver;
        private LocalObjectMovedRemoteObjectChanged underTest;
        private Guid parentUuid;

        [Test, Category("Fast"), Category("Solver")]
        public void ConstructorThrowsExceptionIfRenameSolverIsNull() {
            this.SetUpMocks();
            Assert.Throws<ArgumentNullException>(() => new LocalObjectMovedRemoteObjectChanged(this.session.Object, this.storage.Object, null, this.changeSolver.Object));
        }

        [Test, Category("Fast"), Category("Solver")]
        public void ConstructorThrowsExceptionIfChangeSolverIsNull() {
            this.SetUpMocks();
            Assert.Throws<ArgumentNullException>(() => new LocalObjectMovedRemoteObjectChanged(this.session.Object, this.storage.Object, this.renameSolver.Object, null));
        }

        [Test, Category("Fast"), Category("Solver")]
        public void ConstructorTakesSolver() {
            this.SetUpMocks();
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalFileMoved() {
            this.SetUpMocks();
            string fileName = "name";
            long fileLength = 100;
            var file = Mock.Of<IFileInfo>(
                f =>
                f.FullName == Path.Combine(Path.GetTempPath(), this.newParentsName, fileName) &&
                f.Name == fileName &&
                f.Directory.Uuid == this.parentUuid);
            var doc = Mock.Of<IDocument>(
                d =>
                d.Id == this.remoteObjectId &&
                d.Parents[0].Id == this.oldParentsId);
            Mock.Get(doc).Setup(d => d.Move(It.IsAny<IObjectId>(), It.IsAny<IObjectId>())).Returns(doc);
            var obj = new MappedObject(fileName, this.remoteObjectId, MappedObjectType.File, this.oldParentsId, this.changeToken, fileLength) { Guid = Guid.NewGuid() };
            this.storage.AddMappedFile(obj);
            this.underTest.Solve(file, doc, ContentChangeType.CHANGED, ContentChangeType.CHANGED);
            this.storage.VerifySavedMappedObject(MappedObjectType.File, this.remoteObjectId, fileName, this.newParentsId, this.changeToken, contentSize: fileLength);
            Mock.Get(doc).Verify(d => d.Move(It.Is<IObjectId>(o => o.Id == this.oldParentsId), It.Is<IObjectId>(o => o.Id == this.newParentsId)));
            this.changeSolver.Verify(s => s.Solve(file, doc, ContentChangeType.CHANGED, ContentChangeType.CHANGED), Times.Once());
            this.renameSolver.Verify(s => s.Solve(It.IsAny<IFileSystemInfo>(), It.IsAny<IObjectId>(), It.IsAny<ContentChangeType>(), It.IsAny<ContentChangeType>()), Times.Never);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalFileMovedAndRenamed() {
            this.SetUpMocks();
            string oldFileName = "oldName";
            string newFileName = "newName";
            long fileLength = 100;
            var file = Mock.Of<IFileInfo>(
                f =>
                f.FullName == Path.Combine(Path.GetTempPath(), this.newParentsName, newFileName) &&
                f.Name == newFileName &&
                f.Directory.Uuid == this.parentUuid);
            var doc = Mock.Of<IDocument>(
                d =>
                d.Id == this.remoteObjectId &&
                d.Parents[0].Id == this.oldParentsId);
            Mock.Get(doc).Setup(d => d.Move(It.IsAny<IObjectId>(), It.IsAny<IObjectId>())).Returns(doc);
            var obj = new MappedObject(oldFileName, this.remoteObjectId, MappedObjectType.File, this.oldParentsId, this.changeToken, fileLength) { Guid = Guid.NewGuid() };
            this.storage.AddMappedFile(obj);
            this.underTest.Solve(file, doc, ContentChangeType.CHANGED, ContentChangeType.CHANGED);
            this.storage.VerifySavedMappedObject(MappedObjectType.File, this.remoteObjectId, oldFileName, this.newParentsId, this.changeToken, contentSize: fileLength);
            Mock.Get(doc).Verify(d => d.Move(It.Is<IObjectId>(o => o.Id == this.oldParentsId), It.Is<IObjectId>(o => o.Id == this.newParentsId)));
            this.renameSolver.Verify(s => s.Solve(file, doc, ContentChangeType.CHANGED, ContentChangeType.CHANGED), Times.Once());
            this.changeSolver.Verify(s => s.Solve(It.IsAny<IFileSystemInfo>(), It.IsAny<IObjectId>(), It.IsAny<ContentChangeType>(), It.IsAny<ContentChangeType>()), Times.Never);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalFolderMoved() {
            this.SetUpMocks();
            string folderName = "name";
            var dir = Mock.Of<IDirectoryInfo>(
                d =>
                d.FullName == Path.Combine(Path.GetTempPath(), this.newParentsName, folderName) &&
                d.Name == folderName &&
                d.Parent.Uuid == this.parentUuid);
            var folder = Mock.Of<IFolder>(
                f =>
                f.Id == this.remoteObjectId &&
                f.ParentId == this.oldParentsId);
            Mock.Get(folder).Setup(d => d.Move(It.IsAny<IObjectId>(), It.IsAny<IObjectId>())).Returns(folder);
            var obj = new MappedObject(folderName, this.remoteObjectId, MappedObjectType.Folder, this.oldParentsId, this.changeToken) { Guid = Guid.NewGuid() };
            this.storage.AddMappedFolder(obj);
            this.underTest.Solve(dir, folder, ContentChangeType.NONE, ContentChangeType.NONE);
            this.storage.VerifySavedMappedObject(MappedObjectType.Folder, this.remoteObjectId, folderName, this.newParentsId, this.changeToken);
            Mock.Get(folder).Verify(d => d.Move(It.Is<IObjectId>(o => o.Id == this.oldParentsId), It.Is<IObjectId>(o => o.Id == this.newParentsId)));
            this.changeSolver.Verify(s => s.Solve(dir, folder, ContentChangeType.NONE, ContentChangeType.NONE), Times.Once());
            this.renameSolver.Verify(s => s.Solve(It.IsAny<IFileSystemInfo>(), It.IsAny<IObjectId>(), It.IsAny<ContentChangeType>(), It.IsAny<ContentChangeType>()), Times.Never);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalFolderMovedAndRenamed() {
            this.SetUpMocks();
            string oldDirName = "oldName";
            string newDirName = "newName";
            var dir = Mock.Of<IDirectoryInfo>(
                d =>
                d.FullName == Path.Combine(Path.GetTempPath(), this.newParentsName, newDirName) &&
                d.Name == newDirName &&
                d.Parent.Uuid == this.parentUuid);
            var folder = Mock.Of<IFolder>(
                f =>
                f.Id == this.remoteObjectId &&
                f.ParentId == this.oldParentsId);
            Mock.Get(folder).Setup(d => d.Move(It.IsAny<IObjectId>(), It.IsAny<IObjectId>())).Returns(folder);
            var obj = new MappedObject(oldDirName, this.remoteObjectId, MappedObjectType.Folder, this.oldParentsId, this.changeToken) { Guid = Guid.NewGuid() };
            this.storage.AddMappedFolder(obj);
            this.underTest.Solve(dir, folder, ContentChangeType.NONE, ContentChangeType.NONE);
            this.storage.VerifySavedMappedObject(MappedObjectType.Folder, this.remoteObjectId, oldDirName, this.newParentsId, this.changeToken);
            Mock.Get(folder).Verify(d => d.Move(It.Is<IObjectId>(o => o.Id == this.oldParentsId), It.Is<IObjectId>(o => o.Id == this.newParentsId)));
            this.renameSolver.Verify(s => s.Solve(dir, folder, ContentChangeType.NONE, ContentChangeType.NONE), Times.Once());
            this.changeSolver.Verify(s => s.Solve(It.IsAny<IFileSystemInfo>(), It.IsAny<IObjectId>(), It.IsAny<ContentChangeType>(), It.IsAny<ContentChangeType>()), Times.Never);
        }

        private void SetUpMocks() {
            this.manager = new ActiveActivitiesManager();
            this.session = new Mock<ISession>();
            this.session.SetupTypeSystem();
            this.storage = new Mock<IMetaDataStorage>();
            this.fsFactory = new Mock<IFileSystemInfoFactory>();
            this.changeSolver = new Mock<LocalObjectChangedRemoteObjectChanged>(
                this.session.Object,
                this.storage.Object,
                this.manager,
                this.fsFactory.Object);
            this.renameSolver = new Mock<LocalObjectRenamedRemoteObjectChanged>(
                this.session.Object,
                this.storage.Object,
                this.changeSolver.Object);
            this.underTest = new LocalObjectMovedRemoteObjectChanged(this.session.Object, this.storage.Object, this.renameSolver.Object, this.changeSolver.Object);
            var srcRemoteParent = Mock.Of<ICmisObject>(
                o =>
                o.Name == this.oldParentsName &&
                o.Id == this.oldParentsId);
            var targetRemoteParent = Mock.Of<ICmisObject>(
                o =>
                o.Name == this.newParentsName &&
                o.Id == this.newParentsId);
            this.session.AddRemoteObjects(srcRemoteParent, targetRemoteParent);
            this.parentUuid = Guid.NewGuid();
            var mappedParent = new MappedObject(this.newParentsName, this.newParentsId, MappedObjectType.Folder, "grandParentId", this.changeToken) { Guid = this.parentUuid };
            this.storage.AddMappedFolder(mappedParent);
        }
    }
}