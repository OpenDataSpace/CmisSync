
namespace TestLibrary.ConsumerTests.SituationSolverTests.PrivateWorkingCopyTests {
    using System;

    using CmisSync.Lib.Consumer.SituationSolver;
    using CmisSync.Lib.Consumer.SituationSolver.PWC;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;

    using Moq;

    using NUnit.Framework;

    using TestUtils;

    [TestFixture]
    public class LocalObjectAddedWithPWCTest {
        private Mock<ISession> session;
        private Mock<IMetaDataStorage> storage;
        private Mock<IFileTransmissionStorage> transmissionStorage;
        private Mock<ActiveActivitiesManager> manager;
        [Test, Category("Fast")]
        public void ConstructorWithGivenFolderAddedSolverAndTransmissionManager() {
            this.SetUpMocks();
            new LocalObjectAddedWithPWC(
                this.session.Object,
                this.storage.Object,
                this.transmissionStorage.Object,
                this.manager.Object,
                Mock.Of<ISolver>());
        }

        [Test, Category("Fast")]
        public void ConstructorFailsIfSessionIsNotAbleToWorkWithPrivateWorkingCopies() {
            this.SetUpMocks(isPwcUpdateable: false);
            Assert.Throws<ArgumentException>(
                () =>
                new LocalObjectAddedWithPWC(
                this.session.Object,
                this.storage.Object,
                this.transmissionStorage.Object,
                this.manager.Object,
                Mock.Of<ISolver>()));
        }

        [Test, Category("Fast")]
        public void ConstructorFailsIfGivenSolverIsNull() {
            this.SetUpMocks();
            Assert.Throws<ArgumentNullException>(
                () =>
                new LocalObjectAddedWithPWC(
                this.session.Object,
                this.storage.Object,
                this.transmissionStorage.Object,
                this.manager.Object,
                null));
        }

        [Test, Category("Fast")]
        public void NewDirectoriesCallsArePassedToTheGivenSolver() {
            this.SetUpMocks();
            var folderSolver = new Mock<ISolver>();
            var undertest = new LocalObjectAddedWithPWC(
                this.session.Object,
                this.storage.Object,
                this.transmissionStorage.Object,
                this.manager.Object,
                folderSolver.Object);
            var localFolder = new Mock<IDirectoryInfo>();

            undertest.Solve(localFolder.Object, null, ContentChangeType.CREATED, ContentChangeType.NONE);

            folderSolver.Verify(s => s.Solve(localFolder.Object, null, ContentChangeType.CREATED, ContentChangeType.NONE), Times.Once());
        }

        private void SetUpMocks(bool isPwcUpdateable = true) {
            this.session = new Mock<ISession>();
            this.session.SetupTypeSystem();
            this.session.SetupPrivateWorkingCopyCapability(isPwcUpdateable: isPwcUpdateable);
            this.storage = new Mock<IMetaDataStorage>();
            this.transmissionStorage = new Mock<IFileTransmissionStorage>();
            this.manager = new Mock<ActiveActivitiesManager>();
        }
    }
}