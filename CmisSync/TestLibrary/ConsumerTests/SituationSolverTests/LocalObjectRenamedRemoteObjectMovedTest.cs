
namespace TestLibrary.ConsumerTests.SituationSolverTests
{
    using System;

    using CmisSync.Lib.Consumer.SituationSolver;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;

    using Moq;

    using NUnit.Framework;

    using TestUtils;

    [TestFixture]
    public class LocalObjectRenamedRemoteObjectMovedTest
    {
        private Mock<ISession> session;
        private Mock<IMetaDataStorage> storage;
        private Mock<ISyncEventQueue> queue;
        private ActiveActivitiesManager transmissionManager;
        private Mock<LocalObjectRenamedRemoteObjectRenamed> renameSolver;
        private Mock<LocalObjectChangedRemoteObjectChanged> changeSolver;

        [SetUp]
        public void SetUp()
        {
            this.session = new Mock<ISession>();
            this.session.SetupTypeSystem();
            this.storage = new Mock<IMetaDataStorage>();
            this.queue = new Mock<ISyncEventQueue>();
            this.transmissionManager = new ActiveActivitiesManager();
            this.changeSolver = new Mock<LocalObjectChangedRemoteObjectChanged>(this.session.Object, this.storage.Object, this.transmissionManager, Mock.Of<IFileSystemInfoFactory>());
            this.renameSolver = new Mock<LocalObjectRenamedRemoteObjectRenamed>(this.session.Object, this.storage.Object, this.queue.Object, this.changeSolver.Object);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void DefaultConstructor() {
            new LocalObjectRenamedRemoteObjectMoved(this.session.Object, Mock.Of<IMetaDataStorage>(), this.queue.Object, this.renameSolver.Object, this.changeSolver.Object);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void ConstructorFailsIfNoQueueIsPassed() {
            Assert.Throws<ArgumentNullException>(() => new LocalObjectRenamedRemoteObjectMoved(this.session.Object, Mock.Of<IMetaDataStorage>(), null, this.renameSolver.Object, this.changeSolver.Object));
        }

        [Test, Category("Fast"), Category("Solver")]
        public void ConstructorFailsIfNoRenameSolverIsPassed() {
            Assert.Throws<ArgumentNullException>(() => new LocalObjectRenamedRemoteObjectMoved(this.session.Object, Mock.Of<IMetaDataStorage>(), this.queue.Object, null, this.changeSolver.Object));
        }

        [Test, Category("Fast"), Category("Solver")]
        public void ConstructorFailsIfNoChangeSolverIsPassed() {
            Assert.Throws<ArgumentNullException>(() => new LocalObjectRenamedRemoteObjectMoved(this.session.Object, Mock.Of<IMetaDataStorage>(), this.queue.Object, this.renameSolver.Object, null));
        }
    }
}