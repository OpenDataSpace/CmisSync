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
    public class LocalObjectChangedRemoteObjectMovedTest
    {
        private ActiveActivitiesManager manager;
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
        public void MoveLocalFolder() {
            this.SetUpMocks();
            string oldPath = Path.Combine(Path.GetTempPath(), "old", "name");
            string newPath = Path.Combine(Path.GetTempPath(), "new", "name");
            var mappedObject = new MappedObject("name", "remoteId", MappedObjectType.Folder, "oldParentId", "changeToken") { Guid = Guid.NewGuid() };
            this.storage.AddMappedFolder(mappedObject);
            var remoteFolder = Mock.Of<IFolder>(
                f =>
                f.Id == "remoteId" &&
                f.Name == "name" &&
                f.ParentId == "parentId");
            var dir = Mock.Of<IDirectoryInfo>(
                d =>
                d.FullName == oldPath);
            this.storage.Setup(s => s.Matcher.CreateLocalPath(remoteFolder)).Returns(newPath);
            this.underTest.Solve(dir, remoteFolder, ContentChangeType.NONE, ContentChangeType.NONE);
            Mock.Get(dir).Verify(d => d.MoveTo(newPath), Times.Once());
            this.changeSolver.Verify(s => s.Solve(dir, remoteFolder, ContentChangeType.NONE, ContentChangeType.NONE), Times.Once());
            this.storage.VerifySavedMappedObject(MappedObjectType.Folder, "remoteId", "name", "parentId", "changeToken");
        }

        [Test, Category("Fast"), Category("Solver")]
        public void MoveAndRenameLocalFolder() {
            this.SetUpMocks();
            string oldPath = Path.Combine(Path.GetTempPath(), "old", "oldname");
            string newPath = Path.Combine(Path.GetTempPath(), "new", "newname");
            var mappedObject = new MappedObject("oldname", "remoteId", MappedObjectType.Folder, "oldParentId", "changeToken") { Guid = Guid.NewGuid() };
            this.storage.AddMappedFolder(mappedObject);
            var remoteFolder = Mock.Of<IFolder>(
                f =>
                f.Id == "remoteId" &&
                f.Name == "newname" &&
                f.ParentId == "parentId");
            var dir = Mock.Of<IDirectoryInfo>(
                d =>
                d.FullName == oldPath);
            this.storage.Setup(s => s.Matcher.CreateLocalPath(remoteFolder)).Returns(newPath);
            this.underTest.Solve(dir, remoteFolder, ContentChangeType.NONE, ContentChangeType.NONE);
            Mock.Get(dir).Verify(d => d.MoveTo(newPath), Times.Once());
            this.changeSolver.Verify(s => s.Solve(dir, remoteFolder, ContentChangeType.NONE, ContentChangeType.NONE), Times.Once());
            this.storage.VerifySavedMappedObject(MappedObjectType.Folder, "remoteId", "newname", "parentId", "changeToken");
        }

        [Test, Category("Fast"), Category("Solver")]
        public void MoveFile() {
            this.SetUpMocks();
            long fileLength = 100;
            string oldPath = Path.Combine(Path.GetTempPath(), "old", "name");
            string newPath = Path.Combine(Path.GetTempPath(), "new", "name");
            var mappedObject = new MappedObject("name", "remoteId", MappedObjectType.File, "oldParentId", "changeToken", fileLength) { Guid = Guid.NewGuid() };
            this.storage.AddMappedFile(mappedObject);
            var remoteFile = Mock.Of<IDocument>(
                f =>
                f.Id == "remoteId" &&
                f.Name == "name" &&
                f.Parents[0].Id == "parentId");
            var file = Mock.Of<IFileInfo>(
                f =>
                f.FullName == oldPath);
            this.storage.Setup(s => s.Matcher.CreateLocalPath(remoteFile)).Returns(newPath);
            this.underTest.Solve(file, remoteFile, ContentChangeType.NONE, ContentChangeType.NONE);
            Mock.Get(file).Verify(d => d.MoveTo(newPath), Times.Once());
            this.changeSolver.Verify(s => s.Solve(file, remoteFile, ContentChangeType.NONE, ContentChangeType.NONE), Times.Once());
            this.storage.VerifySavedMappedObject(MappedObjectType.File, "remoteId", "name", "parentId", "changeToken", contentSize: fileLength);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void MoveAndRenameFile() {
            this.SetUpMocks();
            long fileLength = 100;
            string oldPath = Path.Combine(Path.GetTempPath(), "old", "oldname");
            string newPath = Path.Combine(Path.GetTempPath(), "new", "newname");
            var mappedObject = new MappedObject("oldname", "remoteId", MappedObjectType.File, "oldParentId", "changeToken", fileLength) { Guid = Guid.NewGuid() };
            this.storage.AddMappedFile(mappedObject);
            var remoteFile = Mock.Of<IDocument>(
                f =>
                f.Id == "remoteId" &&
                f.Name == "newname" &&
                f.Parents[0].Id == "parentId");
            var file = Mock.Of<IFileInfo>(
                f =>
                f.FullName == oldPath);
            this.storage.Setup(s => s.Matcher.CreateLocalPath(remoteFile)).Returns(newPath);
            this.underTest.Solve(file, remoteFile, ContentChangeType.NONE, ContentChangeType.NONE);
            Mock.Get(file).Verify(d => d.MoveTo(newPath), Times.Once());
            this.changeSolver.Verify(s => s.Solve(file, remoteFile, ContentChangeType.NONE, ContentChangeType.NONE), Times.Once());
            this.storage.VerifySavedMappedObject(MappedObjectType.File, "remoteId", "newname", "parentId", "changeToken", contentSize: fileLength);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void PassContentChanges() {
            this.SetUpMocks();
            long fileLength = 100;
            string oldPath = Path.Combine(Path.GetTempPath(), "old", "name");
            string newPath = Path.Combine(Path.GetTempPath(), "new", "name");
            var mappedObject = new MappedObject("name", "remoteId", MappedObjectType.File, "oldParentId", "changeToken", fileLength) { Guid = Guid.NewGuid() };
            this.storage.AddMappedFile(mappedObject);
            var remoteFile = Mock.Of<IDocument>(
                f =>
                f.Id == "remoteId" &&
                f.Name == "name" &&
                f.Parents[0].Id == "parentId");
            var file = Mock.Of<IFileInfo>(
                f =>
                f.FullName == oldPath);
            this.storage.Setup(s => s.Matcher.CreateLocalPath(remoteFile)).Returns(newPath);
            this.underTest.Solve(file, remoteFile, ContentChangeType.CHANGED, ContentChangeType.CHANGED);
            this.changeSolver.Verify(s => s.Solve(file, remoteFile, ContentChangeType.CHANGED, ContentChangeType.CHANGED), Times.Once());
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
                null,
                this.manager,
                this.fsFactory.Object);
            this.underTest = new LocalObjectChangedRemoteObjectMoved(this.session.Object, this.storage.Object, this.changeSolver.Object);
        }
    }
}

