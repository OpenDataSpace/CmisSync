using System;
using System.IO;

using CmisSync.Lib.Storage;
using CmisSync.Lib.Sync.Strategy;
using CmisSync.Lib.Events;
using CmisSync.Lib.Sync.Solver;

using DotCMIS.Client;

using NUnit.Framework;

using Moq;

namespace TestLibrary.SyncStrategiesTests
{
    [TestFixture]
    public class SyncMechanismTest
    {

        private Mock<ISession> Session;
        private Mock<ISyncEventQueue> Queue;
        private Mock<IMetaDataStorage> Storage;

        [SetUp]
        public void SetUp()
        {
            Session = new Mock<ISession>();
            Queue = new Mock<ISyncEventQueue>();
            Storage = new Mock<IMetaDataStorage>();
        }


        [Test, Category("Fast")]
        public void ConstructorWorksWithValidInput()
        {
            var localDetection = new Mock<ISituationDetection<FileSystemInfo>>();
            var remoteDetection = new Mock<ISituationDetection<string>>();
            var mechanism = new SyncMechanism(localDetection.Object, remoteDetection.Object, Queue.Object, Session.Object, Storage.Object);
            Assert.AreEqual(localDetection.Object, mechanism.LocalSituation);
            Assert.AreEqual(remoteDetection.Object, mechanism.RemoteSituation);
            Assert.AreEqual(Math.Pow(Enum.GetNames(typeof(SituationType)).Length,2), mechanism.Solver.Length);
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorFailsWithLocalDetectionNull()
        {
            var remoteDetection = new Mock<ISituationDetection<string>>();
            new SyncMechanism(null, remoteDetection.Object, Queue.Object, Session.Object, Storage.Object);
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorFailsWithRemoteDetectionNull()
        {
            var localDetection = new Mock<ISituationDetection<FileSystemInfo>>();
            new SyncMechanism(localDetection.Object, null, Queue.Object, Session.Object, Storage.Object);
        }

        [Test, Category("Fast")]
        public void ConstructorForTestWorksWithValidInput()
        {
            var localDetection = new Mock<ISituationDetection<FileSystemInfo>>();
            var remoteDetection = new Mock<ISituationDetection<string>>();
            int numberOfSolver = Enum.GetNames(typeof(SituationType)).Length;
            ISolver[,] solver = new ISolver[numberOfSolver,numberOfSolver];
            var mechanism = new SyncMechanism(localDetection.Object, remoteDetection.Object, Queue.Object, Session.Object, Storage.Object, solver);
            Assert.AreEqual(localDetection.Object, mechanism.LocalSituation);
            Assert.AreEqual(remoteDetection.Object, mechanism.RemoteSituation);
            Assert.AreEqual(Math.Pow(Enum.GetNames(typeof(SituationType)).Length,2), mechanism.Solver.Length);
            Assert.AreEqual(solver, mechanism.Solver);
        }

        [Test, Category("Fast")]
        public void ChooseCorrectSolverForNoChange()
        {
            var localDetection = new Mock<ISituationDetection<FileSystemInfo>>();
            var remoteDetection = new Mock<ISituationDetection<string>>();
            int numberOfSolver = Enum.GetNames(typeof(SituationType)).Length;
            string remoteId = "RemoteId";
            string path = "path";
            string parentPath = ".";
            ISolver[,] solver = new ISolver[numberOfSolver,numberOfSolver];
            var noChangeSolver = new Mock<ISolver>();
            noChangeSolver.Setup(s => s.Solve(
                It.IsAny<ISession>(),
                It.IsAny<IMetaDataStorage>(),
                It.IsAny<FileSystemInfo>(),
                It.Is<string>(id => id == remoteId)));
            localDetection.Setup(d => d.Analyse(
                It.Is<IMetaDataStorage>(db => db == Storage.Object),
                It.IsAny<FileSystemInfo>())).Returns(SituationType.NOCHANGE);
            remoteDetection.Setup(d => d.Analyse(
                It.Is<IMetaDataStorage>(db => db == Storage.Object),
                It.IsAny<string>())).Returns(SituationType.NOCHANGE);
            solver [(int)SituationType.NOCHANGE, (int)SituationType.NOCHANGE] = noChangeSolver.Object;
            var mechanism = new SyncMechanism(
                localDetection.Object,
                remoteDetection.Object,
                Queue.Object,
                Session.Object,
                Storage.Object,
                solver);
            var remoteDoc = new Mock<IDocument>();
            remoteDoc.Setup(doc => doc.Id).Returns(remoteId);
            var noChangeEvent = new Mock<FileEvent>(new FileInfo(path), new DirectoryInfo(parentPath), remoteDoc.Object){CallBase=true}.Object;
            Assert.True(mechanism.Handle(noChangeEvent));
            noChangeSolver.Verify(s => s.Solve(
                It.Is<ISession>(se => se == Session.Object),
                It.IsAny<IMetaDataStorage>(),
                It.IsAny<FileSystemInfo>(),
                It.Is<string>(id => id == remoteId)), Times.Once());
        }

        [Test, Category("Fast")]
        public void IgnoreNonFileOrFolderEvents()
        {
            var localDetection = new Mock<ISituationDetection<FileSystemInfo>>();
            var remoteDetection = new Mock<ISituationDetection<string>>();
            var mechanism = new SyncMechanism(localDetection.Object, remoteDetection.Object, Queue.Object, Session.Object, Storage.Object);
            var invalidEvent = new Mock<ISyncEvent>().Object;
            Assert.IsFalse(mechanism.Handle(invalidEvent));
            localDetection.Verify(d => d.Analyse(It.IsAny<IMetaDataStorage>(), It.IsAny<FileSystemInfo>()), Times.Never());
            remoteDetection.Verify(d => d.Analyse(It.IsAny<IMetaDataStorage>(), It.IsAny<string>()), Times.Never());
        }

        // Not yet implemented correctly, remove ignore to test if all situations do have got a default solver
        [Ignore]
        [Test, Category("Fast")]
        public void DefaultSolverCreatedForEverySituation()
        {
            var localDetection = new Mock<ISituationDetection<FileSystemInfo>>();
            var remoteDetection = new Mock<ISituationDetection<string>>();
            var mechanism = new SyncMechanism(localDetection.Object, remoteDetection.Object, Queue.Object, Session.Object, Storage.Object);
            int solverCount = 0;
            foreach (ISolver s in mechanism.Solver)
            {
                if(s != null)
                    solverCount ++;
            }
            Assert.AreEqual((int)Math.Pow(Enum.GetNames(typeof(SituationType)).Length,2), solverCount);
        }

        // If a solver fails to solve the situation, the situation should be rescanned and if it changed, the new situation should be handled
        [Test, Category("Fast")]
        public void HandleErrorsOnSolverBecauseOfAChangedSituation()
        {
            var localDetection = new Mock<ISituationDetection<FileSystemInfo>>();
            var remoteDetection = new Mock<ISituationDetection<string>>();
            localDetection.Setup(d => d.Analyse(It.IsAny<IMetaDataStorage>(), It.IsAny<FileSystemInfo>())).Returns(SituationType.NOCHANGE);
            remoteDetection.Setup(d => d.Analyse(It.IsAny<IMetaDataStorage>(), It.IsAny<string>())).ReturnsInOrder(SituationType.CHANGED, SituationType.REMOVED);
            var failingSolver = new Mock<ISolver>();
            failingSolver.Setup(s => s.Solve(
                It.Is<ISession>((se) => se == Session.Object),
                It.IsAny<IMetaDataStorage>(),
                It.IsAny<FileSystemInfo>(),
                It.IsAny<string>()))
                .Throws(new DotCMIS.Exceptions.CmisObjectNotFoundException());
            var successfulSolver = new Mock<ISolver>();
            successfulSolver.Setup( s => s.Solve(
                It.Is<ISession>((se) => se == Session.Object),
                It.IsAny<IMetaDataStorage>(),
                It.IsAny<FileSystemInfo>(),
                It.IsAny<string>()));

            var mechanism = new SyncMechanism(localDetection.Object, remoteDetection.Object, Queue.Object, Session.Object, Storage.Object);
            var remoteDocument = new Mock<IDocument>();
            var remoteEvent = new Mock<FileEvent>(null, null, remoteDocument.Object).Object;
            mechanism.Solver[(int) SituationType.NOCHANGE, (int) SituationType.CHANGED] = failingSolver.Object;
            mechanism.Solver[(int) SituationType.NOCHANGE, (int) SituationType.REMOVED] = successfulSolver.Object;

            Assert.IsTrue(mechanism.Handle(remoteEvent));

            localDetection.Verify(d => d.Analyse(It.IsAny<IMetaDataStorage>(), It.IsAny<FileSystemInfo>()), Times.Exactly(2));
            remoteDetection.Verify(d => d.Analyse(It.IsAny<IMetaDataStorage>(), It.IsAny<string>()), Times.Exactly(2));
        }

        [Ignore]
        [Test, Category("Fast")]
        public void HandleErrorOnSolver()
        {
            // If a solver fails to solve the situation, the situation should be rescanned and if it not changed an Exception Event should be published on Queue
            Assert.Fail("TODO");
        }

        [Ignore]
        [Test, Category("Fast")]
        public void HandleIncompleteEventInformations()
        {
            // If a FileEvent doesn't contain all local and remote informations, the MetaDataStorage should be used to determine the missing informations
            Assert.Fail ("TODO");
        }
    }
}

