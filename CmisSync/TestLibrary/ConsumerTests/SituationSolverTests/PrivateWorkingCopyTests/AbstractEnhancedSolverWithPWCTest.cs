using CmisSync.Lib.Storage.FileSystem;
using CmisSync.Lib.Events;


namespace TestLibrary.ConsumerTests.SituationSolverTests.PrivateWorkingCopyTests {
    using System;

    using CmisSync.Lib.Consumer.SituationSolver.PWC;
    using CmisSync.Lib.Storage.Database;

    using DotCMIS.Client;

    using Moq;

    using NUnit.Framework;

    using TestUtils;

    [TestFixture]
    public class AbstractEnhancedSolverWithPWCTest {
        [Test, Category("Fast")]
        public void ConstructorThrowsExceptionIfTransmissionStorageIsNull() {
            var session = new Mock<ISession>().SetupTypeSystem().SetupPrivateWorkingCopyCapability().Object;
            Assert.Throws<ArgumentNullException>(() => new SolverClass(session, Mock.Of<IMetaDataStorage>(), null));
        }

        [Test, Category("Fast")]
        public void ConstructorThrowsExceptionIfSessionDoesNotSupportPwc() {
            var session = new Mock<ISession>().SetupTypeSystem().SetupPrivateWorkingCopyCapability(false).Object;
            Assert.Throws<ArgumentException>(() => new SolverClass(session, Mock.Of<IMetaDataStorage>(), Mock.Of<IFileTransmissionStorage>()));
        }

        private class SolverClass : AbstractEnhancedSolverWithPWC {
            public SolverClass(
                ISession session,
                IMetaDataStorage storage,
                IFileTransmissionStorage transmissionStorage) : base(session, storage, transmissionStorage) {
            }

            public IFileTransmissionStorage GetTransmissionStorage() {
                return this.TransmissionStorage;
            }

            public override void Solve(
                IFileSystemInfo localFileSystemInfo,
                IObjectId remoteId,
                ContentChangeType localContent,
                ContentChangeType remoteContent)
            {
                throw new NotImplementedException();
            }
        }
    }
}