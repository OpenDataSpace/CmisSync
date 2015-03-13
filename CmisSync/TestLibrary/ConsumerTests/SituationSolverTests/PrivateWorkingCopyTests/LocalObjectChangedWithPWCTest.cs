
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
    public class LocalObjectChangedWithPWCTest : IsTestWithConfiguredLog4Net {
        private Mock<ISession> session;
        private Mock<IMetaDataStorage> storage;
        private Mock<IFileTransmissionStorage> transmissionStorage;
        private Mock<ActiveActivitiesManager> manager;
        private Mock<ISolver> folderOrFileContentUnchangedAddedSolver;

        [Test, Category("Fast"), Category("Solver")]
        public void Constructor() {
            this.SetUpMocks();
            new LocalObjectChangedWithPWC(
                this.session.Object,
                this.storage.Object,
                this.transmissionStorage.Object,
                this.manager.Object,
                Mock.Of<ISolver>());
        }

        [Test, Category("Fast"), Category("Solver")]
        public void ConstructorFailsIfGivenSolverIsNull() {
            this.SetUpMocks();
            Assert.Throws<ArgumentNullException>(
                () =>
                new LocalObjectChangedWithPWC(
                this.session.Object,
                this.storage.Object,
                this.transmissionStorage.Object,
                this.manager.Object,
                null));
        }

        [Test, Category("Fast"), Category("Solver")]
        public void ConstructorFailsIfSessionIsNotAbleToWorkWithPrivateWorkingCopies() {
            this.SetUpMocks(isPwcUpdateable: false);
            Assert.Throws<ArgumentException>(
                () =>
                new LocalObjectChangedWithPWC(
                this.session.Object,
                this.storage.Object,
                this.transmissionStorage.Object,
                this.manager.Object,
                Mock.Of<ISolver>()));
        }

        [Test, Category("Fast"), Category("Solver")]
        public void SolveCallIsPassedToGivenSolverIfNoMatchingSituationIsFound() {
            this.SetUpMocks();
            var underTest = this.CreateSolver();
            this.folderOrFileContentUnchangedAddedSolver.Setup(
                s =>
                s.Solve(It.IsAny<IFileSystemInfo>(), It.IsAny<IObjectId>(), It.IsAny<ContentChangeType>(), It.IsAny<ContentChangeType>()));
            var folder = Mock.Of<IDirectoryInfo>();
            var remoteId = Mock.Of<IFolder>();

            underTest.Solve(folder, remoteId, localContent: ContentChangeType.NONE, remoteContent: ContentChangeType.NONE);

            this.folderOrFileContentUnchangedAddedSolver.Verify(
                s => s.Solve(folder, remoteId, ContentChangeType.NONE, ContentChangeType.NONE), Times.Once());
        }

        private LocalObjectChangedWithPWC CreateSolver() {
            return new LocalObjectChangedWithPWC(
                this.session.Object,
                this.storage.Object,
                this.transmissionStorage.Object,
                this.manager.Object,
                this.folderOrFileContentUnchangedAddedSolver.Object);
        }

        private void SetUpMocks(bool isPwcUpdateable = true, bool serverCanModifyLastModificationDate = true) {
            this.session = new Mock<ISession>();
            this.session.SetupTypeSystem(serverCanModifyLastModificationDate: serverCanModifyLastModificationDate);
            this.session.SetupPrivateWorkingCopyCapability(isPwcUpdateable: isPwcUpdateable);
            this.storage = new Mock<IMetaDataStorage>();
            this.transmissionStorage = new Mock<IFileTransmissionStorage>();
            this.manager = new Mock<ActiveActivitiesManager>();
            this.folderOrFileContentUnchangedAddedSolver = new Mock<ISolver>(MockBehavior.Strict);
        }
    }
}