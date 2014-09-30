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
        private string newPath;
        private string newParentPath;
        private ActiveActivitiesManager manager;
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
            long fileLength = 100;
            Guid fileUuid = Guid.NewGuid();
            Guid newParentUuid = Guid.NewGuid();
            this.newPath = Path.Combine(this.newParentPath, this.newName);
            var file = Mock.Of<IFileInfo>(
                f =>
                f.Uuid == fileUuid &&
                f.Name == this.oldName &&
                f.Directory.Uuid == newParentUuid &&
                f.Directory.FullName == this.newParentPath);
            var doc = Mock.Of<IDocument>(
                d =>
                d.Id == this.remoteId &&
                d.Name == this.newName);
            this.session.AddRemoteObjects(Mock.Of<IFolder>(o => o.Id == this.oldParentId), Mock.Of<IFolder>(o => o.Id == this.newParentId));
            Mock.Get(doc).Setup(d => d.Move(It.Is<IObjectId>(o => o.Id == this.oldParentId), It.Is<IObjectId>(o => o.Id == this.newParentId))).Returns(doc);
            Mock.Get(file).Setup(f => f.MoveTo(this.newPath)).Callback(() => { Mock.Get(file).SetupGet(f => f.Name).Returns(this.newName); Mock.Get(file).SetupGet(f => f.FullName).Returns(this.newPath);});
            var obj = new MappedObject(this.oldName, this.remoteId, MappedObjectType.File, this.oldParentId, this.changeToken, fileLength) { Guid = fileUuid };
            this.storage.AddMappedFile(obj);
            var newParentObj = new MappedObject(this.newParentName, this.newParentId, MappedObjectType.Folder, null, this.changeToken) { Guid = newParentUuid };
            this.storage.AddMappedFolder(newParentObj);

            this.underTest.Solve(file, doc, ContentChangeType.CHANGED, ContentChangeType.CHANGED);

            this.changeSolver.Verify(s => s.Solve(file, doc, ContentChangeType.CHANGED, ContentChangeType.CHANGED), Times.Once());
            this.renameSolver.Verify(s => s.Solve(It.IsAny<IFileSystemInfo>(), It.IsAny<IObjectId>(), It.IsAny<ContentChangeType>(), It.IsAny<ContentChangeType>()), Times.Never());
            Mock.Get(doc).Verify(d => d.Move(It.Is<IObjectId>(o => o.Id == this.oldParentId), It.Is<IObjectId>(o => o.Id == this.newParentId)), Times.Once());
            Mock.Get(file).Verify(f => f.MoveTo(this.newPath), Times.Once());
            this.storage.VerifySavedMappedObject(MappedObjectType.File, this.remoteId, this.newName, this.newParentId, this.changeToken, contentSize: fileLength);
        }

        [Test, Category("Fast"), Category("Solver"), Ignore("Not Implemented")]
        public void LocalFolderMoveRemoteRename() {
            Assert.Fail("TODO");
        }

        [Test, Category("Fast"), Category("Solver"), Ignore("Not Implemented")]
        public void LocalFileMoveAndRenameRemoteRename() {
            Assert.Fail("TODO");
        }

        [Test, Category("Fast"), Category("Solver"), Ignore("Not Implemented")]
        public void LocalFolderMoveAndRenameRemoteRename() {
            Assert.Fail("TODO");
        }

        private void SetUpMocks() {
            this.newParentPath = Path.Combine(Path.GetTempPath(), this.newParentName);
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
            this.renameSolver = new Mock<LocalObjectRenamedRemoteObjectRenamed>(
                this.session.Object,
                this.storage.Object,
                this.changeSolver.Object);
            this.underTest = new LocalObjectMovedRemoteObjectRenamed(this.session.Object, this.storage.Object, this.changeSolver.Object, this.renameSolver.Object);
        }
    }
}