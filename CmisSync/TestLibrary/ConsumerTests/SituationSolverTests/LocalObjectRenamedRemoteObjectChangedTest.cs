//-----------------------------------------------------------------------
// <copyright file="LocalObjectRenamedRemoteObjectChangedTest.cs" company="GRAU DATA AG">
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
    public class LocalObjectRenamedRemoteObjectChangedTest
    {
        private ActiveActivitiesManager manager;
        private Mock<ISession> session;
        private Mock<IMetaDataStorage> storage;
        private Mock<IFileSystemInfoFactory> fsFactory;
        private Mock<LocalObjectChangedRemoteObjectChanged> changeSolver;
        private LocalObjectRenamedRemoteObjectChanged underTest;

        [Test, Category("Fast"), Category("Solver")]
        public void ConstructorThrowsExceptionIfSolverIsNull() {
            Mock<ISession> session = new Mock<ISession>();
            session.SetupTypeSystem();
            Assert.Throws<ArgumentNullException>(() => new LocalObjectRenamedRemoteObjectChanged(session.Object, Mock.Of<IMetaDataStorage>(), null));
        }

        [Test, Category("Fast"), Category("Solver")]
        public void ConstructorTakesChangeSolver() {
            this.SetUpMocks();
        }

        [Test, Category("Fast"), Category("Solver")]
        public void RenameFile() {
            this.SetUpMocks();
            long fileLength = 100;
            var file = Mock.Of<IFileInfo>(
                f =>
                f.Name == "newName");
            var doc = Mock.Of<IDocument>(
                d =>
                d.Id == "remoteId");
            var obj = new MappedObject("oldName", "remoteId", MappedObjectType.File, "parentId", "changeToken", fileLength) { Guid = Guid.NewGuid() };
            this.storage.AddMappedFile(obj);
            this.underTest.Solve(file, doc, ContentChangeType.CHANGED, ContentChangeType.CHANGED);
            Mock.Get(doc).Verify(d => d.Rename("newName", true), Times.Once());
            this.storage.VerifySavedMappedObject(MappedObjectType.File, "remoteId", "newName", "parentId", "changeToken", contentSize: fileLength);
            this.changeSolver.Verify(s => s.Solve(file, doc, ContentChangeType.CHANGED, ContentChangeType.CHANGED));
        }

        [Test, Category("Fast"), Category("Solver")]
        public void RenameFolder() {
            this.SetUpMocks();
            var dir = Mock.Of<IDirectoryInfo>(
                f =>
                f.Name == "newName");
            var folder = Mock.Of<IFolder>(
                d =>
                d.Id == "remoteId");
            var obj = new MappedObject("oldName", "remoteId", MappedObjectType.Folder, "parentId", "changeToken") { Guid = Guid.NewGuid() };
            this.storage.AddMappedFolder(obj);
            this.underTest.Solve(dir, folder, ContentChangeType.NONE, ContentChangeType.NONE);
            Mock.Get(folder).Verify(f => f.Rename("newName", true), Times.Once());
            this.storage.VerifySavedMappedObject(MappedObjectType.Folder, "remoteId", "newName", "parentId", "changeToken");
            this.changeSolver.Verify(s => s.Solve(dir, folder, ContentChangeType.NONE, ContentChangeType.NONE));

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
            this.underTest = new LocalObjectRenamedRemoteObjectChanged(this.session.Object, this.storage.Object, this.changeSolver.Object);
        }
    }
}