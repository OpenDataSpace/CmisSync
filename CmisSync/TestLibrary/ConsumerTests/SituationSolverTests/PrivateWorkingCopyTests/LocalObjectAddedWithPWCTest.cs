
namespace TestLibrary.ConsumerTests.SituationSolverTests.PrivateWorkingCopyTests {
    using System;

    using CmisSync.Lib.Consumer.SituationSolver;
    using CmisSync.Lib.Consumer.SituationSolver.PWC;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.Database;

    using DotCMIS.Client;

    using Moq;

    using NUnit.Framework;

    using TestUtils;

    [TestFixture]
    public class LocalObjectAddedWithPWCTest {
        private Mock<ISession> session;

        [Test, Category("Fast")]
        public void ConstructorWithGivenFolderAddedSolverAndTransmissionManager() {
            this.SetUpSession();
            new LocalObjectAddedWithPWC(this.session.Object, Mock.Of<IMetaDataStorage>(), Mock.Of<IFileTransmissionStorage>(), Mock.Of<ActiveActivitiesManager>(), Mock.Of<ISolver>());
        }

        [Test, Category("Fast")]
        public void ConstructorFailsIfSessionIsNotAbleToWorkWithPrivateWorkingCopies() {
            this.SetUpSession(isPwcUpdateable: false);
            Assert.Throws<ArgumentException>(() => new LocalObjectAddedWithPWC(this.session.Object, Mock.Of<IMetaDataStorage>(), Mock.Of<IFileTransmissionStorage>(), Mock.Of<ActiveActivitiesManager>(), Mock.Of<ISolver>()));
        }

        private void SetUpSession(bool isPwcUpdateable = true) {
            this.session = new Mock<ISession>();
            session.SetupTypeSystem();
            session.SetupPrivateWorkingCopyCapability(isPwcUpdateable: isPwcUpdateable);
        }
    }
}