using System;
using System.IO;

using CmisSync.Lib.Storage;
using CmisSync.Lib.Sync.Strategy;
using CmisSync.Lib.Events;
using CmisSync.Lib.Sync.Solver;
using CmisSync.Lib.Data;

using DotCMIS.Client;
using DotCMIS.Client.Impl;

using NUnit.Framework;

using Moq;

using TestLibrary.TestUtils;

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
            var localDetection = new Mock<ISituationDetection<AbstractFolderEvent>>();
            var remoteDetection = new Mock<ISituationDetection<AbstractFolderEvent>>();
            var mechanism = new SyncMechanism(localDetection.Object, remoteDetection.Object, Queue.Object, Session.Object, Storage.Object);
            Assert.AreEqual(localDetection.Object, mechanism.LocalSituation);
            Assert.AreEqual(remoteDetection.Object, mechanism.RemoteSituation);
            Assert.AreEqual(Math.Pow(Enum.GetNames(typeof(SituationType)).Length,2), mechanism.Solver.Length);
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorFailsWithLocalDetectionNull()
        {
            var remoteDetection = new Mock<ISituationDetection<AbstractFolderEvent>>();
            new SyncMechanism(null, remoteDetection.Object, Queue.Object, Session.Object, Storage.Object);
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorFailsWithRemoteDetectionNull()
        {
            var localDetection = new Mock<ISituationDetection<AbstractFolderEvent>>();
            new SyncMechanism(localDetection.Object, null, Queue.Object, Session.Object, Storage.Object);
        }

        [Test, Category("Fast")]
        public void ConstructorForTestWorksWithValidInput()
        {
            var localDetection = new Mock<ISituationDetection<AbstractFolderEvent>>();
            var remoteDetection = new Mock<ISituationDetection<AbstractFolderEvent>>();
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
            var localDetection = new Mock<ISituationDetection<AbstractFolderEvent>>();
            var remoteDetection = new Mock<ISituationDetection<AbstractFolderEvent>>();
            int numberOfSolver = Enum.GetNames(typeof(SituationType)).Length;
            IObjectId remoteId = new ObjectId("RemoteId");
            string path = "path";
            string parentPath = ".";
            ISolver[,] solver = new ISolver[numberOfSolver,numberOfSolver];
            var noChangeSolver = new Mock<ISolver>();
            noChangeSolver.Setup(s => s.Solve(
                It.IsAny<ISession>(),
                It.IsAny<IMetaDataStorage>(),
                It.IsAny<IFileSystemInfo>(),
                It.Is<IObjectId>(id => id == remoteId)));
            localDetection.Setup(d => d.Analyse(
                It.Is<IMetaDataStorage>(db => db == Storage.Object),
                It.IsAny<AbstractFolderEvent>())).Returns(SituationType.NOCHANGE);
            remoteDetection.Setup(d => d.Analyse(
                It.Is<IMetaDataStorage>(db => db == Storage.Object),
                It.IsAny<AbstractFolderEvent>())).Returns(SituationType.NOCHANGE);
            solver [(int)SituationType.NOCHANGE, (int)SituationType.NOCHANGE] = noChangeSolver.Object;
            var mechanism = new SyncMechanism(
                localDetection.Object,
                remoteDetection.Object,
                Queue.Object,
                Session.Object,
                Storage.Object,
                solver);
            var remoteDoc = new Mock<IDocument>();
            remoteDoc.Setup(doc => doc.Id).Returns(remoteId.Id);
            var noChangeEvent = new Mock<FileEvent>(new FileInfoWrapper(new FileInfo(path)), new DirectoryInfoWrapper(new DirectoryInfo(parentPath)), remoteDoc.Object){CallBase=true}.Object;
            Assert.True(mechanism.Handle(noChangeEvent));
            noChangeSolver.Verify(s => s.Solve(
                It.Is<ISession>(se => se == Session.Object),
                It.IsAny<IMetaDataStorage>(),
                It.IsAny<IFileSystemInfo>(),
                It.Is<IObjectId>(id => id.Id == remoteId.Id)), Times.Once());
        }

        [Test, Category("Fast")]
        public void IgnoreNonFileOrFolderEvents()
        {
            var localDetection = new Mock<ISituationDetection<AbstractFolderEvent>>();
            var remoteDetection = new Mock<ISituationDetection<AbstractFolderEvent>>();
            var mechanism = new SyncMechanism(localDetection.Object, remoteDetection.Object, Queue.Object, Session.Object, Storage.Object);
            var invalidEvent = new Mock<ISyncEvent>().Object;
            Assert.IsFalse(mechanism.Handle(invalidEvent));
            localDetection.Verify(d => d.Analyse(It.IsAny<IMetaDataStorage>(), It.IsAny<AbstractFolderEvent>()), Times.Never());
            remoteDetection.Verify(d => d.Analyse(It.IsAny<IMetaDataStorage>(), It.IsAny<AbstractFolderEvent>()), Times.Never());
        }

        // Not yet implemented correctly, remove ignore to test if all situations do have got a default solver
        [Ignore]
        [Test, Category("Fast")]
        public void DefaultSolverCreatedForEverySituation()
        {
            var localDetection = new Mock<ISituationDetection<AbstractFolderEvent>>();
            var remoteDetection = new Mock<ISituationDetection<AbstractFolderEvent>>();
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
            var localDetection = new Mock<ISituationDetection<AbstractFolderEvent>>();
            var remoteDetection = new Mock<ISituationDetection<AbstractFolderEvent>>();
            localDetection.Setup(d => d.Analyse(It.IsAny<IMetaDataStorage>(), It.IsAny<AbstractFolderEvent>())).Returns(SituationType.NOCHANGE);
            remoteDetection.Setup(d => d.Analyse(It.IsAny<IMetaDataStorage>(), It.IsAny<AbstractFolderEvent>())).ReturnsInOrder(SituationType.CHANGED, SituationType.REMOVED);
            var failingSolver = new Mock<ISolver>();
            failingSolver.Setup(s => s.Solve(
                It.Is<ISession>((se) => se == Session.Object),
                It.IsAny<IMetaDataStorage>(),
                It.IsAny<IFileSystemInfo>(),
                It.IsAny<IObjectId>()))
                .Throws(new DotCMIS.Exceptions.CmisObjectNotFoundException());
            var successfulSolver = new Mock<ISolver>();
            successfulSolver.Setup( s => s.Solve(
                It.Is<ISession>((se) => se == Session.Object),
                It.IsAny<IMetaDataStorage>(),
                It.IsAny<IFileSystemInfo>(),
                It.IsAny<IObjectId>()));

            var mechanism = new SyncMechanism(localDetection.Object, remoteDetection.Object, Queue.Object, Session.Object, Storage.Object);
            var remoteDocument = new Mock<IDocument>();
            var remoteEvent = new Mock<FileEvent>(null, null, remoteDocument.Object).Object;
            mechanism.Solver[(int) SituationType.NOCHANGE, (int) SituationType.CHANGED] = failingSolver.Object;
            mechanism.Solver[(int) SituationType.NOCHANGE, (int) SituationType.REMOVED] = successfulSolver.Object;

            Assert.IsTrue(mechanism.Handle(remoteEvent));

            localDetection.Verify(d => d.Analyse(It.IsAny<IMetaDataStorage>(), It.IsAny<AbstractFolderEvent>()), Times.Exactly(2));
            remoteDetection.Verify(d => d.Analyse(It.IsAny<IMetaDataStorage>(), It.IsAny<AbstractFolderEvent>()), Times.Exactly(2));
        }

        [Test, Category("Fast"), Category("IT")]
        public void LocalFolderMoveAndRemoteFolderRenameSituation()
        {
            string remoteId = Guid.NewGuid().ToString();
            string oldFolderName = "oldName";
            string newRemoteName = "newName";
            string oldLocalPath = Path.Combine(Path.GetTempPath(), oldFolderName);
            string newLocalPath = Path.Combine(Path.GetTempPath(), "new", oldFolderName);
            string oldRemotePath = "/" + oldFolderName;
            string newRemotePath = "/" + newRemoteName;
            string oldLastChangeToken = Guid.NewGuid().ToString();
            string newLastChangeToken = Guid.NewGuid().ToString();
            DateTime? oldWriteTime = DateTime.UtcNow;
            DateTime? newWriteTime = ((DateTime) oldWriteTime).AddMilliseconds(500);

            var oldLocalFolder = Mock.Of<IDirectoryInfo>(d =>
                                                         d.Name == oldFolderName &&
                                                         d.FullName == oldLocalPath);
            var newLocalFolder = Mock.Of<IDirectoryInfo>(d =>
                                                         d.Name == oldFolderName &&
                                                         d.FullName == newLocalPath);
            var oldLocalParent = Mock.Of<IMappedFolder>( p =>
                                                       p.Name == "/" &&
                                                       p.Parent == (IMappedFolder) null &&
                                                       p.LastRemoteWriteTimeUtc == oldWriteTime);
            Storage.AddMappedFolder(Mock.Of<IMappedFolder>( f =>
                                                           f.RemoteObjectId == remoteId &&
                                                           f.Name == oldFolderName &&
                                                           f.Parent == oldLocalParent &&
                                                           f.LastChangeToken == oldLastChangeToken &&
                                                           f.GetLocalPath() == oldLocalPath &&
                                                           f.GetRemotePath() == oldRemotePath &&
                                                           f.LastRemoteWriteTimeUtc == oldWriteTime));
            Session.AddRemoteObject(Mock.Of<IFolder>(f =>
                                                     f.Id == remoteId &&
                                                     f.Name == newRemoteName &&
                                                     f.Path == newRemotePath &&
                                                     f.ChangeToken == newLastChangeToken &&
                                                     f.LastModificationDate == ((DateTime) newWriteTime).AddMilliseconds(500)));
            var localFS = new Mock<IFileSystemInfoFactory>();

            var localDetection = new LocalSituationDetection(localFS.Object);
            var remoteDetection = new RemoteSituationDetection(Session.Object);
            var folderEvent = new FolderMovedEvent(oldLocalFolder, newLocalFolder, null, null);
            var localMoveRemoteRenameSolver = new Mock<ISolver>();
            var mechanism = new SyncMechanism(localDetection, remoteDetection, Queue.Object, Session.Object, Storage.Object);
            mechanism.Solver[(int) SituationType.MOVED, (int) SituationType.RENAMED] = localMoveRemoteRenameSolver.Object;

            Assert.IsTrue(mechanism.Handle(folderEvent));

            localMoveRemoteRenameSolver.Verify(s => s.Solve(It.Is<ISession>( session => session == Session.Object), It.Is<IMetaDataStorage>(storage => storage == Storage.Object), It.IsAny<IFileSystemInfo>(), It.IsAny<IObjectId>()), Times.Once());
        }

        [Test, Category("Fast"), Category("IT")]
        public void RemoteFolderAddedSituation()
        {
            var remoteFolder = Mock.Of<IFolder>(f =>
                                                f.Id == "remoteId" &&
                                                f.Name == "name");
            var localFS = new Mock<IFileSystemInfoFactory>();
            var remoteFolderAddedSolver = new Mock<ISolver>();
            var localDetection = new LocalSituationDetection(localFS.Object);
            var remoteDetection = new RemoteSituationDetection(Session.Object);
            var folderEvent = new FolderEvent(remoteFolder: remoteFolder) { Remote = MetaDataChangeType.CREATED, Local = MetaDataChangeType.NONE };

            var mechanism = new SyncMechanism(localDetection, remoteDetection, Queue.Object, Session.Object, Storage.Object);
            mechanism.Solver[(int) SituationType.NOCHANGE, (int) SituationType.ADDED] = remoteFolderAddedSolver.Object;

            Assert.IsTrue(mechanism.Handle(folderEvent));

            remoteFolderAddedSolver.Verify(s => s.Solve(It.Is<ISession>( session => session == Session.Object), It.Is<IMetaDataStorage>(storage => storage == Storage.Object), It.IsAny<IFileSystemInfo>(), It.IsAny<IObjectId>()), Times.Once());
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

