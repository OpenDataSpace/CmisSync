//-----------------------------------------------------------------------
// <copyright file="LocalObjectChangedRemoteObjectRenamedTest.cs" company="GRAU DATA AG">
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
    public class LocalObjectChangedRemoteObjectRenamedTest
    {
        private ActiveActivitiesManager manager;
        private Mock<ISession> session;
        private Mock<IMetaDataStorage> storage;
        private Mock<IFileSystemInfoFactory> fsFactory;
        private Mock<LocalObjectChangedRemoteObjectChanged> changeSolver;
        private LocalObjectChangedRemoteObjectRenamed underTest;

        [Test, Category("Fast"), Category("Solver")]
        public void ConstructorThrowsExceptionIfSolverIsNull() {
            Mock<ISession> session = new Mock<ISession>();
            session.SetupTypeSystem();
            Assert.Throws<ArgumentNullException>(() => new LocalObjectChangedRemoteObjectRenamed(session.Object, Mock.Of<IMetaDataStorage>(), null));
        }

        [Test, Category("Fast"), Category("Solver")]
        public void ConstructorTakesChangeSolver() {
            this.SetUpMocks();
        }

        [Test, Category("Fast"), Category("Solver")]
        public void RenameFile()
        {
            this.SetUpMocks();
            long fileLength = 100;
            string oldPath = Path.Combine(Path.GetTempPath(), "oldname");
            string newPath = Path.Combine(Path.GetTempPath(), "newname");
            var mappedObject = new MappedObject("oldname", "remoteId", MappedObjectType.File, "parentId", "changeToken", fileLength) { Guid = Guid.NewGuid() };
            this.storage.AddMappedFolder(mappedObject);
            var remoteFolder = Mock.Of<IDocument>(
                f =>
                f.Id == "remoteId" &&
                f.Name == "newname" &&
                f.Parents[0].Id == "parentId");
            var dir = Mock.Of<IFileInfo>(
                d =>
                d.FullName == oldPath &&
                d.Directory.FullName == Path.GetTempPath());
            this.storage.Setup(s => s.Matcher.CreateLocalPath(remoteFolder)).Returns(newPath);
            this.underTest.Solve(dir, remoteFolder, ContentChangeType.NONE, ContentChangeType.NONE);
            Mock.Get(dir).Verify(d => d.MoveTo(newPath), Times.Once());
            this.changeSolver.Verify(s => s.Solve(dir, remoteFolder, ContentChangeType.NONE, ContentChangeType.NONE), Times.Once());
            this.storage.VerifySavedMappedObject(MappedObjectType.File, "remoteId", "newname", "parentId", "changeToken", contentSize: fileLength);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void PassContentChangesToChangeSolver()
        {
            this.SetUpMocks();
            long fileLength = 100;
            string oldPath = Path.Combine(Path.GetTempPath(), "oldname");
            string newPath = Path.Combine(Path.GetTempPath(), "newname");
            var mappedObject = new MappedObject("oldname", "remoteId", MappedObjectType.File, "parentId", "changeToken", fileLength) { Guid = Guid.NewGuid() };
            this.storage.AddMappedFolder(mappedObject);
            var remoteFolder = Mock.Of<IDocument>(
                f =>
                f.Id == "remoteId" &&
                f.Name == "newname" &&
                f.Parents[0].Id == "parentId");
            var dir = Mock.Of<IFileInfo>(
                d =>
                d.FullName == oldPath &&
                d.Directory.FullName == Path.GetTempPath());
            this.storage.Setup(s => s.Matcher.CreateLocalPath(remoteFolder)).Returns(newPath);
            this.underTest.Solve(dir, remoteFolder, ContentChangeType.CHANGED, ContentChangeType.CHANGED);
            this.changeSolver.Verify(s => s.Solve(dir, remoteFolder, ContentChangeType.CHANGED, ContentChangeType.CHANGED), Times.Once());
        }

        [Test, Category("Fast"), Category("Solver")]
        public void RenameFolder()
        {
            this.SetUpMocks();
            string oldPath = Path.Combine(Path.GetTempPath(), "oldname");
            string newPath = Path.Combine(Path.GetTempPath(), "newname");
            var mappedObject = new MappedObject("oldname", "remoteId", MappedObjectType.Folder, "parentId", "changeToken") { Guid = Guid.NewGuid() };
            this.storage.AddMappedFolder(mappedObject);
            var remoteFolder = Mock.Of<IFolder>(
                f =>
                f.Id == "remoteId" &&
                f.Name == "newname" &&
                f.ParentId == "parentId");
            var dir = Mock.Of<IDirectoryInfo>(
                d =>
                d.FullName == oldPath &&
                d.Parent.FullName == Path.GetTempPath());
            this.storage.Setup(s => s.Matcher.CreateLocalPath(remoteFolder)).Returns(newPath);
            this.underTest.Solve(dir, remoteFolder, ContentChangeType.NONE, ContentChangeType.NONE);
            Mock.Get(dir).Verify(d => d.MoveTo(newPath), Times.Once());
            this.changeSolver.Verify(s => s.Solve(dir, remoteFolder, ContentChangeType.NONE, ContentChangeType.NONE), Times.Once());
            this.storage.VerifySavedMappedObject(MappedObjectType.Folder, "remoteId", "newname", "parentId", "changeToken");
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
            this.underTest = new LocalObjectChangedRemoteObjectRenamed(this.session.Object, this.storage.Object, this.changeSolver.Object);
        }
    }
}