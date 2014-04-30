//-----------------------------------------------------------------------
// <copyright file="SyncMechanismTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.SyncStrategiesTests
{
    using System;
    using System.IO;

    using CmisSync.Lib.Data;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Storage;
    using CmisSync.Lib.Sync.Solver;
    using CmisSync.Lib.Sync.Strategy;

    using DotCMIS.Client;
    using DotCMIS.Client.Impl;

    using Moq;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture]
    public class SyncMechanismTest
    {
        private Mock<ISession> session;
        private Mock<ISyncEventQueue> queue;
        private Mock<IMetaDataStorage> storage;

        [SetUp]
        public void SetUp()
        {
            this.session = new Mock<ISession>();
            this.queue = new Mock<ISyncEventQueue>();
            this.storage = new Mock<IMetaDataStorage>();
        }

        [Test, Category("Fast")]
        public void ConstructorWorksWithValidInput()
        {
            var localDetection = new Mock<ISituationDetection<AbstractFolderEvent>>();
            var remoteDetection = new Mock<ISituationDetection<AbstractFolderEvent>>();
            var mechanism = new SyncMechanism(localDetection.Object, remoteDetection.Object, this.queue.Object, this.session.Object, this.storage.Object);
            Assert.AreEqual(localDetection.Object, mechanism.LocalSituation);
            Assert.AreEqual(remoteDetection.Object, mechanism.RemoteSituation);
            Assert.AreEqual(Math.Pow(Enum.GetNames(typeof(SituationType)).Length, 2), mechanism.Solver.Length);
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorFailsWithLocalDetectionNull()
        {
            var remoteDetection = new Mock<ISituationDetection<AbstractFolderEvent>>();
            new SyncMechanism(null, remoteDetection.Object, this.queue.Object, this.session.Object, this.storage.Object);
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorFailsWithRemoteDetectionNull()
        {
            var localDetection = new Mock<ISituationDetection<AbstractFolderEvent>>();
            new SyncMechanism(localDetection.Object, null, this.queue.Object, this.session.Object, this.storage.Object);
        }

        [Test, Category("Fast")]
        public void ConstructorForTestWorksWithValidInput()
        {
            var localDetection = new Mock<ISituationDetection<AbstractFolderEvent>>();
            var remoteDetection = new Mock<ISituationDetection<AbstractFolderEvent>>();
            int numberOfSolver = Enum.GetNames(typeof(SituationType)).Length;
            ISolver[,] solver = new ISolver[numberOfSolver, numberOfSolver];
            var mechanism = new SyncMechanism(localDetection.Object, remoteDetection.Object, this.queue.Object, this.session.Object, this.storage.Object, solver);
            Assert.AreEqual(localDetection.Object, mechanism.LocalSituation);
            Assert.AreEqual(remoteDetection.Object, mechanism.RemoteSituation);
            Assert.AreEqual(Math.Pow(Enum.GetNames(typeof(SituationType)).Length, 2), mechanism.Solver.Length);
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
            ISolver[,] solver = new ISolver[numberOfSolver, numberOfSolver];
            var noChangeSolver = new Mock<ISolver>();
            noChangeSolver.Setup(s => s.Solve(
                It.IsAny<ISession>(),
                It.IsAny<IMetaDataStorage>(),
                It.IsAny<IFileSystemInfo>(),
                It.Is<IObjectId>(id => id == remoteId)));
            localDetection.Setup(d => d.Analyse(
                It.Is<IMetaDataStorage>(db => db == this.storage.Object),
                It.IsAny<AbstractFolderEvent>())).Returns(SituationType.NOCHANGE);
            remoteDetection.Setup(d => d.Analyse(
                It.Is<IMetaDataStorage>(db => db == this.storage.Object),
                It.IsAny<AbstractFolderEvent>())).Returns(SituationType.NOCHANGE);
            solver[(int)SituationType.NOCHANGE, (int)SituationType.NOCHANGE] = noChangeSolver.Object;
            var mechanism = new SyncMechanism(
                localDetection.Object,
                remoteDetection.Object,
                this.queue.Object,
                this.session.Object,
                this.storage.Object,
                solver);
            var remoteDoc = new Mock<IDocument>();
            remoteDoc.Setup(doc => doc.Id).Returns(remoteId.Id);
            var noChangeEvent = new Mock<FileEvent>(new FileInfoWrapper(new FileInfo(path)), new DirectoryInfoWrapper(new DirectoryInfo(parentPath)), remoteDoc.Object) { CallBase = true }.Object;
            Assert.True(mechanism.Handle(noChangeEvent));
            noChangeSolver.Verify(
                s => s.Solve(
                It.Is<ISession>(se => se == this.session.Object),
                It.IsAny<IMetaDataStorage>(),
                It.IsAny<IFileSystemInfo>(),
                It.Is<IObjectId>(id => id.Id == remoteId.Id)),
                Times.Once());
        }

        [Test, Category("Fast")]
        public void IgnoreNonFileOrFolderEvents()
        {
            var localDetection = new Mock<ISituationDetection<AbstractFolderEvent>>();
            var remoteDetection = new Mock<ISituationDetection<AbstractFolderEvent>>();
            var mechanism = new SyncMechanism(localDetection.Object, remoteDetection.Object, this.queue.Object, this.session.Object, this.storage.Object);
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
            var mechanism = new SyncMechanism(localDetection.Object, remoteDetection.Object, this.queue.Object, this.session.Object, this.storage.Object);
            int solverCount = 0;
            foreach (ISolver s in mechanism.Solver)
            {
                if (s != null) {
                    solverCount++;
                } 
            }

            Assert.AreEqual((int)Math.Pow(Enum.GetNames(typeof(SituationType)).Length, 2), solverCount);
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
            failingSolver.Setup(
                s => s.Solve(
                It.Is<ISession>((se) => se == this.session.Object),
                It.IsAny<IMetaDataStorage>(),
                It.IsAny<IFileSystemInfo>(),
                It.IsAny<IObjectId>()))
                .Throws(new DotCMIS.Exceptions.CmisObjectNotFoundException());
            var successfulSolver = new Mock<ISolver>();
            successfulSolver.Setup(
                s => s.Solve(
                It.Is<ISession>((se) => se == this.session.Object),
                It.IsAny<IMetaDataStorage>(),
                It.IsAny<IFileSystemInfo>(),
                It.IsAny<IObjectId>()));

            var mechanism = new SyncMechanism(localDetection.Object, remoteDetection.Object, this.queue.Object, this.session.Object, this.storage.Object);
            var remoteDocument = new Mock<IDocument>();
            var remoteEvent = new Mock<FileEvent>(new Mock<IFileInfo>().Object, null, remoteDocument.Object).Object;
            mechanism.Solver[(int)SituationType.NOCHANGE, (int)SituationType.CHANGED] = failingSolver.Object;
            mechanism.Solver[(int)SituationType.NOCHANGE, (int)SituationType.REMOVED] = successfulSolver.Object;

            Assert.IsTrue(mechanism.Handle(remoteEvent));

            localDetection.Verify(d => d.Analyse(It.IsAny<IMetaDataStorage>(), It.IsAny<AbstractFolderEvent>()), Times.Exactly(2));
            remoteDetection.Verify(d => d.Analyse(It.IsAny<IMetaDataStorage>(), It.IsAny<AbstractFolderEvent>()), Times.Exactly(2));
        }

        // Ignore until the local situation detection implements local move
        [Ignore]
        [Test, Category("Fast"), Category("IT")]
        public void LocalFolderMoveAndRemoteFolderRenameSituation()
        {
            string remoteId = Guid.NewGuid().ToString();
            string oldFolderName = "oldName";
            string newRemoteName = "newName";
            string oldLocalPath = Path.Combine(Path.GetTempPath(), oldFolderName);
            string newLocalPath = Path.Combine(Path.GetTempPath(), "new", oldFolderName);
            string newRemotePath = "/" + newRemoteName;
            string oldLastChangeToken = Guid.NewGuid().ToString();
            string newLastChangeToken = Guid.NewGuid().ToString();
            DateTime? oldWriteTime = DateTime.UtcNow;
            DateTime? newWriteTime = ((DateTime)oldWriteTime).AddMilliseconds(500);

            var oldLocalFolder = Mock.Of<IDirectoryInfo>(d =>
                                                         d.Name == oldFolderName &&
                                                         d.FullName == oldLocalPath);
            var newLocalFolder = Mock.Of<IDirectoryInfo>(d =>
                                                         d.Name == oldFolderName &&
                                                         d.FullName == newLocalPath);
            var oldLocalParent = Mock.Of<IMappedObject>(p =>
                                                        p.Name == "/" &&
                                                        p.ParentId == null &&
                                                        p.LastRemoteWriteTimeUtc == oldWriteTime);
            this.storage.AddMappedFolder(Mock.Of<IMappedObject>(f =>
                                                                f.RemoteObjectId == remoteId &&
                                                                f.Name == oldFolderName &&
                                                                f.ParentId == oldLocalParent.RemoteObjectId &&
                                                                f.LastChangeToken == oldLastChangeToken &&
                                                                f.LastRemoteWriteTimeUtc == oldWriteTime));
            this.session.AddRemoteObject(Mock.Of<IFolder>(f =>
                                                          f.Id == remoteId &&
                                                          f.Name == newRemoteName &&
                                                          f.Path == newRemotePath &&
                                                          f.ChangeToken == newLastChangeToken &&
                                                          f.LastModificationDate == ((DateTime)newWriteTime).AddMilliseconds(500)));

            var localDetection = new LocalSituationDetection();
            var remoteDetection = new RemoteSituationDetection();
            var folderEvent = new FolderMovedEvent(oldLocalFolder, newLocalFolder, null, null);
            var localMoveRemoteRenameSolver = new Mock<ISolver>();
            var mechanism = new SyncMechanism(localDetection, remoteDetection, this.queue.Object, this.session.Object, this.storage.Object);
            mechanism.Solver[(int)SituationType.MOVED, (int)SituationType.RENAMED] = localMoveRemoteRenameSolver.Object;

            Assert.IsTrue(mechanism.Handle(folderEvent));

            localMoveRemoteRenameSolver.Verify(
                s => s.Solve(
                It.Is<ISession>(session => session == this.session.Object),
                It.Is<IMetaDataStorage>(storage => storage == this.storage.Object),
                It.IsAny<IFileSystemInfo>(),
                It.IsAny<IObjectId>()),
                Times.Once());
        }

        [Test, Category("Fast"), Category("IT")]
        public void RemoteFolderAddedSituation()
        {
            var remoteFolder = Mock.Of<IFolder>(f =>
                                                f.Id == "remoteId" &&
                                                f.Name == "name");
            var remoteFolderAddedSolver = new Mock<ISolver>();
            var localDetection = new LocalSituationDetection();
            var remoteDetection = new RemoteSituationDetection();
            var folderEvent = new FolderEvent(remoteFolder: remoteFolder, localFolder: new Mock<IDirectoryInfo>().Object)
            {
                Remote = MetaDataChangeType.CREATED,
                Local = MetaDataChangeType.NONE
            };

            var mechanism = new SyncMechanism(localDetection, remoteDetection, this.queue.Object, this.session.Object, this.storage.Object);
            mechanism.Solver[(int)SituationType.NOCHANGE, (int)SituationType.ADDED] = remoteFolderAddedSolver.Object;

            Assert.IsTrue(mechanism.Handle(folderEvent));

            remoteFolderAddedSolver.Verify(
                s => s.Solve(
                It.Is<ISession>(session => session == this.session.Object),
                It.Is<IMetaDataStorage>(storage => storage == this.storage.Object),
                It.IsAny<IFileSystemInfo>(),
                It.IsAny<IObjectId>()),
                Times.Once());
        }

        [Test, Category("Fast"), Category("IT")]
        public void LocalFolderAddedSituation()
        {
            var localFolder = Mock.Of<IDirectoryInfo>();
            var localFolderAddedSolver = new Mock<ISolver>();
            var localDetection = new LocalSituationDetection();
            var remoteDetection = new RemoteSituationDetection();
            var folderEvent = new FolderEvent(localFolder: localFolder) { Local = MetaDataChangeType.CREATED, Remote = MetaDataChangeType.NONE };

            var mechanism = new SyncMechanism(localDetection, remoteDetection, this.queue.Object, this.session.Object, this.storage.Object);
            mechanism.Solver[(int)SituationType.ADDED, (int)SituationType.NOCHANGE] = localFolderAddedSolver.Object;

            Assert.IsTrue(mechanism.Handle(folderEvent));

            localFolderAddedSolver.Verify(
                s => s.Solve(
                It.Is<ISession>(session => session == this.session.Object),
                It.Is<IMetaDataStorage>(storage => storage == this.storage.Object),
                It.IsAny<IFileSystemInfo>(),
                It.IsAny<IObjectId>()),
                Times.Once());
        }

        [Ignore]
        [Test, Category("Fast")]
        public void HandleErrorOnSolver()
        {
            // If a solver fails to solve the situation, the situation should be rescanned and if it not changed an Exception Event should be published on Queue
            Assert.Fail("TODO");
        }
    }
}