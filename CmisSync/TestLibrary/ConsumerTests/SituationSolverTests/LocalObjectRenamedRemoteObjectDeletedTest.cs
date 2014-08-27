using CmisSync.Lib.Storage.FileSystem;
using CmisSync.Lib.Events;
using CmisSync.Lib.Storage.Database.Entities;


namespace TestLibrary.ConsumerTests.SituationSolverTests
{
    using System;

    using CmisSync.Lib.Consumer.SituationSolver;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.Database;

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
        private LocalObjectRenamedRemoteObjectDeleted underTest;

        [SetUp]
        public void SetUp() {
            this.session = new Mock<ISession>();
            this.storage = new Mock<IMetaDataStorage>();
            this.queue = new Mock<ISyncEventQueue>();
            this.manager = new Mock<ActiveActivitiesManager> { CallBase = true };
            this.secondSolver = new Mock<ISolver>();
            this.underTest = new LocalObjectRenamedRemoteObjectDeleted(
                this.session.Object,
                this.storage.Object,
                this.queue.Object,
                this.manager.Object,
                true,
                this.secondSolver.Object);
        }

        [Test, Category("Fast")]
        public void ConstructorWorksWithDateModification() {
            new LocalObjectRenamedRemoteObjectDeleted(
                Mock.Of<ISession>(),
                Mock.Of<IMetaDataStorage>(),
                Mock.Of<ISyncEventQueue>(),
                Mock.Of<ActiveActivitiesManager>(),
                true);
        }

        [Test, Category("Fast")]
        public void ConstructorWorksWithoutDateModification() {
            new LocalObjectRenamedRemoteObjectDeleted(
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
            var folder = Mock.Of<IDirectoryInfo>(
                f =>
                f.Name == "newName");
            this.underTest.Solve(folder, null, ContentChangeType.NONE, ContentChangeType.NONE);
            this.storage.Verify(s => s.RemoveObject(mappedObject));
            this.secondSolver.Verify(s => s.Solve(folder, null, ContentChangeType.CREATED, ContentChangeType.NONE));
        }

        [Test, Category("Fast")]
        public void HandlesFileEventAndPassesArgumentsToSecondSolverAfterDbModification() {
            var mappedObject = new MappedObject("name", "remoteId", MappedObjectType.File, "parentId", "changeToken", 10) {
                Guid = Guid.NewGuid()
            };
            var file = Mock.Of<IFileInfo>();
            this.underTest.Solve(file, null, ContentChangeType.NONE, ContentChangeType.NONE);
            this.storage.Verify(s => s.RemoveObject(mappedObject));
            this.secondSolver.Verify(s => s.Solve(file, null, ContentChangeType.CREATED, ContentChangeType.NONE));
        }
    }
}