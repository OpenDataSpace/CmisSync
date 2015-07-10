//-----------------------------------------------------------------------
// <copyright file="LocalObjectChangedRemoteObjectMovedTest.cs" company="GRAU DATA AG">
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
    public class LocalObjectChangedRemoteObjectMovedTest {
        private TransmissionManager manager;
        private Mock<ISession> session;
        private Mock<IMetaDataStorage> storage;
        private Mock<IFileSystemInfoFactory> fsFactory;
        private Mock<LocalObjectChangedRemoteObjectChanged> changeSolver;
        private LocalObjectChangedRemoteObjectMoved underTest;

        [Test, Category("Fast"), Category("Solver")]
        public void ConstructorThrowsExceptionIfSolverIsNull() {
            Mock<ISession> session = new Mock<ISession>();
            session.SetupTypeSystem();
            Assert.Throws<ArgumentNullException>(() => new LocalObjectChangedRemoteObjectMoved(session.Object, Mock.Of<IMetaDataStorage>(), null));
        }

        [Test, Category("Fast"), Category("Solver")]
        public void ConstructorTakesChangeSolver() {
            this.SetUpMocks();
        }

        [Test, Category("Fast"), Category("Solver")]
        public void MoveLocalFolder(
            [Values(true, false)]bool childrenAreIgnored,
            [Values(true, false)]bool localWasReadOnly,
            [Values(true, false)]bool remoteWasReadOnly,
            [Values(true, false)]bool remoteIsReadOnly) {
            this.SetUpMocks();
            string oldPath = Path.Combine(Path.GetTempPath(), "old", "name");
            string newPath = Path.Combine(Path.GetTempPath(), "new", "name");
            var mappedObject = new MappedObject("name", "remoteId", MappedObjectType.Folder, "oldParentId", "changeToken") {
                Guid = Guid.NewGuid(),
                IsReadOnly = remoteWasReadOnly
            };
            this.storage.AddMappedFolder(mappedObject);
            var remoteFolder = MockOfIFolderUtil.CreateRemoteFolderMock("remoteId", "name", "path", "parentId", ignored: childrenAreIgnored, readOnly: remoteIsReadOnly);
            var dir = new Mock<IDirectoryInfo>();
            dir.Setup(d => d.FullName).Returns(oldPath);
            dir.SetupProperty(d => d.ReadOnly, localWasReadOnly);
            this.storage.Setup(s => s.Matcher.CreateLocalPath(remoteFolder.Object)).Returns(newPath);

            this.underTest.Solve(dir.Object, remoteFolder.Object, ContentChangeType.NONE, ContentChangeType.NONE);

            dir.Verify(d => d.MoveTo(newPath), Times.Once());
            dir.VerifySet(d => d.ReadOnly = remoteIsReadOnly, remoteIsReadOnly != localWasReadOnly ? Times.Once() : Times.Never());
            this.changeSolver.Verify(s => s.Solve(dir.Object, remoteFolder.Object, ContentChangeType.NONE, ContentChangeType.NONE), Times.Once());
            this.storage.VerifySavedMappedObject(MappedObjectType.Folder, "remoteId", "name", "parentId", "changeToken", ignored: childrenAreIgnored, readOnly: remoteIsReadOnly);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void MoveAndRenameLocalFolder(
            [Values(true, false)]bool childrenAreIgnored,
            [Values(true, false)]bool localWasReadOnly,
            [Values(true, false)]bool remoteWasReadOnly,
            [Values(true, false)]bool remoteIsReadOnly)
        {
            this.SetUpMocks();
            string oldPath = Path.Combine(Path.GetTempPath(), "old", "oldname");
            string newPath = Path.Combine(Path.GetTempPath(), "new", "newname");
            var mappedObject = new MappedObject("oldname", "remoteId", MappedObjectType.Folder, "oldParentId", "changeToken") {
                Guid = Guid.NewGuid(),
                IsReadOnly = remoteWasReadOnly
            };
            this.storage.AddMappedFolder(mappedObject);
            var remoteFolder = MockOfIFolderUtil.CreateRemoteFolderMock("remoteId", "newname", "path", "parentId", ignored: childrenAreIgnored, readOnly: remoteIsReadOnly);
            var dir = new Mock<IDirectoryInfo>();
            dir.Setup(d => d.FullName).Returns(oldPath);
            dir.SetupProperty(d => d.ReadOnly, localWasReadOnly);
            this.storage.Setup(s => s.Matcher.CreateLocalPath(remoteFolder.Object)).Returns(newPath);

            this.underTest.Solve(dir.Object, remoteFolder.Object, ContentChangeType.NONE, ContentChangeType.NONE);

            dir.Verify(d => d.MoveTo(newPath), Times.Once());
            dir.VerifySet(d => d.ReadOnly = remoteIsReadOnly, remoteIsReadOnly != localWasReadOnly ? Times.Once() : Times.Never());
            this.changeSolver.Verify(s => s.Solve(dir.Object, remoteFolder.Object, ContentChangeType.NONE, ContentChangeType.NONE), Times.Once());
            this.storage.VerifySavedMappedObject(MappedObjectType.Folder, "remoteId", "newname", "parentId", "changeToken", ignored: childrenAreIgnored, readOnly: remoteIsReadOnly);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void MoveFile(
            [Values(true, false)]bool localWasReadOnly,
            [Values(true, false)]bool remoteWasReadOnly,
            [Values(true, false)]bool remoteIsReadOnly)
        {
            this.SetUpMocks();
            long fileLength = 100;
            string oldPath = Path.Combine(Path.GetTempPath(), "old", "name");
            string newPath = Path.Combine(Path.GetTempPath(), "new", "name");
            var mappedObject = new MappedObject("name", "remoteId", MappedObjectType.File, "oldParentId", "changeToken", fileLength) { Guid = Guid.NewGuid() };
            this.storage.AddMappedFile(mappedObject);
            var remoteFile = new Mock<IDocument>().SetupName("name").SetupId("remoteId").SetupParent(Mock.Of<IFolder>(p => p.Id == "parentId"));
            remoteFile.SetupReadOnly(remoteIsReadOnly);
            var file = new Mock<IFileInfo>();
            file.Setup(f => f.FullName).Returns(oldPath);
            file.SetupProperty(f => f.ReadOnly, localWasReadOnly);
            this.storage.Setup(s => s.Matcher.CreateLocalPath(remoteFile.Object)).Returns(newPath);

            this.underTest.Solve(file.Object, remoteFile.Object, ContentChangeType.NONE, ContentChangeType.NONE);

            file.Verify(d => d.MoveTo(newPath), Times.Once());
            this.changeSolver.Verify(s => s.Solve(file.Object, remoteFile.Object, ContentChangeType.NONE, ContentChangeType.NONE), Times.Once());
            file.VerifySet(f => f.ReadOnly = remoteIsReadOnly, remoteIsReadOnly != localWasReadOnly ? Times.Once() : Times.Never());
            this.storage.VerifySavedMappedObject(MappedObjectType.File, "remoteId", "name", "parentId", "changeToken", contentSize: fileLength, readOnly: remoteIsReadOnly);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void MoveAndRenameFile(
            [Values(true, false)]bool localWasReadOnly,
            [Values(true, false)]bool remoteWasReadOnly,
            [Values(true, false)]bool remoteIsReadOnly)
        {
            this.SetUpMocks();
            long fileLength = 100;
            string oldPath = Path.Combine(Path.GetTempPath(), "old", "oldname");
            string newPath = Path.Combine(Path.GetTempPath(), "new", "newname");
            var mappedObject = new MappedObject("oldname", "remoteId", MappedObjectType.File, "oldParentId", "changeToken", fileLength) {
                Guid = Guid.NewGuid(),
                IsReadOnly = remoteWasReadOnly
            };
            this.storage.AddMappedFile(mappedObject);
            var remoteFile = new Mock<IDocument>().SetupName("newname").SetupId("remoteId").SetupParent(Mock.Of<IFolder>(f => f.Id == "parentId"));
            remoteFile.SetupReadOnly(remoteIsReadOnly);
            var file = new Mock<IFileInfo>().SetupReadOnly(localWasReadOnly);
            file.Setup(f => f.FullName).Returns(oldPath);

            this.storage.Setup(s => s.Matcher.CreateLocalPath(remoteFile.Object)).Returns(newPath);

            this.underTest.Solve(file.Object, remoteFile.Object, ContentChangeType.NONE, ContentChangeType.NONE);

            file.Verify(d => d.MoveTo(newPath), Times.Once());
            file.VerifySet(d => d.ReadOnly = remoteIsReadOnly, remoteIsReadOnly != localWasReadOnly ? Times.Once() : Times.Never());
            this.changeSolver.Verify(s => s.Solve(file.Object, remoteFile.Object, ContentChangeType.NONE, ContentChangeType.NONE), Times.Once());
            this.storage.VerifySavedMappedObject(MappedObjectType.File, "remoteId", "newname", "parentId", "changeToken", contentSize: fileLength, readOnly: remoteIsReadOnly);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void PassContentChanges() {
            this.SetUpMocks();
            long fileLength = 100;
            string oldPath = Path.Combine(Path.GetTempPath(), "old", "name");
            string newPath = Path.Combine(Path.GetTempPath(), "new", "name");
            var mappedObject = new MappedObject("name", "remoteId", MappedObjectType.File, "oldParentId", "changeToken", fileLength) { Guid = Guid.NewGuid() };
            this.storage.AddMappedFile(mappedObject);
            var remoteFile = new Mock<IDocument>().SetupId("remoteId").SetupName("name").SetupParent(Mock.Of<IFolder>(f => f.Id == "parentId"));
            var file = new Mock<IFileInfo>().SetupFullName(oldPath);
            this.storage.Setup(s => s.Matcher.CreateLocalPath(remoteFile.Object)).Returns(newPath);

            this.underTest.Solve(file.Object, remoteFile.Object, ContentChangeType.CHANGED, ContentChangeType.CHANGED);

            this.changeSolver.Verify(s => s.Solve(file.Object, remoteFile.Object, ContentChangeType.CHANGED, ContentChangeType.CHANGED), Times.Once());
            this.storage.VerifyThatNoObjectIsManipulated();
        }

        private void SetUpMocks() {
            this.manager = new TransmissionManager();
            this.session = new Mock<ISession>();
            this.session.SetupTypeSystem();
            this.storage = new Mock<IMetaDataStorage>();
            this.fsFactory = new Mock<IFileSystemInfoFactory>();
            this.changeSolver = new Mock<LocalObjectChangedRemoteObjectChanged>(
                this.session.Object,
                this.storage.Object,
                null,
                this.manager.CreateFactory(),
                this.fsFactory.Object);
            this.underTest = new LocalObjectChangedRemoteObjectMoved(this.session.Object, this.storage.Object, this.changeSolver.Object);
        }
    }
}