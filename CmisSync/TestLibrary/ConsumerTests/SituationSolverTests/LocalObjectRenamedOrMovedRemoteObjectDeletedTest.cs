//-----------------------------------------------------------------------
// <copyright file="LocalObjectRenamedOrMovedRemoteObjectDeletedTest.cs" company="GRAU DATA AG">
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
    using CmisSync.Lib.FileTransmission;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;

    using Moq;

    using NUnit.Framework;

    using TestUtils;

    [TestFixture]
    public class LocalObjectRenamedOrMovedRemoteObjectDeletedTest {
        private Mock<IMetaDataStorage> storage;
        private Mock<IFileTransmissionStorage> transmissionStorage;
        private Mock<ISession> session;
        private Mock<ISolver> secondSolver;
        private Mock<ITransmissionFactory> transmissionFactory;
        private LocalObjectRenamedOrMovedRemoteObjectDeleted underTest;

        [SetUp]
        public void SetUp() {
            this.session = new Mock<ISession>();
            this.session.SetupTypeSystem();
            this.storage = new Mock<IMetaDataStorage>();
            this.transmissionStorage = new Mock<IFileTransmissionStorage>();
            this.transmissionFactory = new Mock<ITransmissionFactory>();
            this.secondSolver = new Mock<ISolver>();
            this.underTest = new LocalObjectRenamedOrMovedRemoteObjectDeleted(
                this.session.Object,
                this.storage.Object,
                this.transmissionStorage.Object,
                this.transmissionFactory.Object,
                this.secondSolver.Object);
        }

        [Test, Category("Fast")]
        public void ConstructorWorksWithDateModification() {
            new LocalObjectRenamedOrMovedRemoteObjectDeleted(
                this.session.Object,
                Mock.Of<IMetaDataStorage>(),
                Mock.Of<IFileTransmissionStorage>(),
                Mock.Of<ITransmissionFactory>());
        }

        [Test, Category("Fast")]
        public void ConstructorWorksWithoutDateModification() {
            new LocalObjectRenamedOrMovedRemoteObjectDeleted(
                this.session.Object,
                Mock.Of<IMetaDataStorage>(),
                Mock.Of<IFileTransmissionStorage>(),
                Mock.Of<ITransmissionFactory>());
        }

        [Test, Category("Fast")]
        public void HandlesFolderEventAndPassesArgumentsToSecondSolverAfterDbModification() {
            var mappedObject = new MappedObject("oldName", "remoteId", MappedObjectType.Folder, "parentId", "changeToken") {
                Guid = Guid.NewGuid()
            };
            this.storage.AddMappedFolder(mappedObject);
            var folder = Mock.Of<IDirectoryInfo>(
                f =>
                f.Name == "newName" &&
                f.Uuid == mappedObject.Guid &&
                f.FullName == "<path to folder>" &&
                f.GetFiles() == new IFileInfo[0] &&
                f.GetDirectories() == new IDirectoryInfo[0]);
            this.underTest.Solve(folder, null, ContentChangeType.NONE, ContentChangeType.NONE);
            this.storage.Verify(s => s.RemoveObject(mappedObject));
            this.secondSolver.Verify(s => s.Solve(folder, null, ContentChangeType.NONE, ContentChangeType.NONE));
        }

        [Test, Category("Fast")]
        public void HandlesFolderEventContainingSubElementsAndPassesArgumentsToSecondSolverAfterDbModificationAndThrowsExceptionToForceCrawlSync() {
            var mappedObject = new MappedObject("oldName", "remoteId", MappedObjectType.Folder, "parentId", "changeToken") {
                Guid = Guid.NewGuid()
            };
            this.storage.AddMappedFolder(mappedObject);
            var folder = Mock.Of<IDirectoryInfo>(
                f =>
                f.Name == "newName" &&
                f.Uuid == mappedObject.Guid &&
                f.FullName == "<path to folder>" &&
                f.GetFiles() == new IFileInfo[1] &&
                f.GetDirectories() == new IDirectoryInfo[0]);
            Assert.Throws<IOException>(() => this.underTest.Solve(folder, null, ContentChangeType.NONE, ContentChangeType.NONE));
            this.storage.Verify(s => s.RemoveObject(mappedObject));
            this.secondSolver.Verify(s => s.Solve(folder, null, ContentChangeType.NONE, ContentChangeType.NONE));
        }

        [Test, Category("Fast")]
        public void HandlesFileEventAndPassesArgumentsToSecondSolverAfterDbModification() {
            var mappedObject = new MappedObject("oldName", "remoteId", MappedObjectType.File, "parentId", "changeToken", 10) {
                Guid = Guid.NewGuid()
            };
            this.storage.AddMappedFile(mappedObject);
            var file = Mock.Of<IFileInfo>(
                f =>
                f.Name == "newName" &&
                f.Uuid == mappedObject.Guid);
            this.underTest.Solve(file, null, ContentChangeType.NONE, ContentChangeType.NONE);
            this.storage.Verify(s => s.RemoveObject(mappedObject));
            this.secondSolver.Verify(s => s.Solve(file, null, ContentChangeType.CREATED, ContentChangeType.NONE));
        }
    }
}