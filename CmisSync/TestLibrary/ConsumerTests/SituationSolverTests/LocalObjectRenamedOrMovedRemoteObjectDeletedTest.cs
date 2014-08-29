
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
    public class LocalObjectRenamedRemoteObjectDeletedTest
    {
        private Mock<IMetaDataStorage> storage;
        private Mock<ISession> session;
        private Mock<ISolver> secondSolver;
        private Mock<ActiveActivitiesManager> manager;
        private Mock<ISyncEventQueue> queue;
        private LocalObjectRenamedOrMovedRemoteObjectDeleted underTest;

        [SetUp]
        public void SetUp() {
            this.session = new Mock<ISession>();
            this.storage = new Mock<IMetaDataStorage>();
            this.queue = new Mock<ISyncEventQueue>();
            this.manager = new Mock<ActiveActivitiesManager> { CallBase = true };
            this.secondSolver = new Mock<ISolver>();
            this.underTest = new LocalObjectRenamedOrMovedRemoteObjectDeleted(
                this.session.Object,
                this.storage.Object,
                this.queue.Object,
                this.manager.Object,
                true,
                this.secondSolver.Object);
        }

        [Test, Category("Fast")]
        public void ConstructorWorksWithDateModification() {
            new LocalObjectRenamedOrMovedRemoteObjectDeleted(
                Mock.Of<ISession>(),
                Mock.Of<IMetaDataStorage>(),
                Mock.Of<ISyncEventQueue>(),
                Mock.Of<ActiveActivitiesManager>(),
                true);
        }

        [Test, Category("Fast")]
        public void ConstructorWorksWithoutDateModification() {
            new LocalObjectRenamedOrMovedRemoteObjectDeleted(
                Mock.Of<ISession>(),
                Mock.Of<IMetaDataStorage>(),
                Mock.Of<ISyncEventQueue>(),
                Mock.Of<ActiveActivitiesManager>(),
                false);
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