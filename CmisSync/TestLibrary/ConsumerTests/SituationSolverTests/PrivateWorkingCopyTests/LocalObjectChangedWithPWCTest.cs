//-----------------------------------------------------------------------
// <copyright file="LocalObjectChangedWithPWCTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.ConsumerTests.SituationSolverTests.PrivateWorkingCopyTests {
    using System;

    using CmisSync.Lib.Consumer.SituationSolver;
    using CmisSync.Lib.Consumer.SituationSolver.PWC;
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
    public class LocalObjectChangedWithPWCTest : IsTestWithConfiguredLog4Net {
        private readonly string fileName = "file.bin";

        private Mock<ISession> session;
        private Mock<IMetaDataStorage> storage;
        private Mock<IFileTransmissionStorage> transmissionStorage;
        private Mock<ActiveActivitiesManager> manager;
        private Mock<ISolver> folderOrFileContentUnchangedAddedSolver;

        [Test, Category("Fast"), Category("Solver")]
        public void Constructor() {
            this.SetUpMocks();
            this.CreateSolver();
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
            var folder = new Mock<IDirectoryInfo>(MockBehavior.Strict).Object;
            var remoteId = new Mock<IFolder>(MockBehavior.Strict).Object;

            underTest.Solve(folder, remoteId, localContent: ContentChangeType.NONE, remoteContent: ContentChangeType.NONE);

            this.folderOrFileContentUnchangedAddedSolver.Verify(
                s => s.Solve(folder, remoteId, ContentChangeType.NONE, ContentChangeType.NONE), Times.Once());
        }

        [Test, Category("Fast"), Category("Solver")]
        public void SolverUploadsFileContentByCreatingNewPWC() {
            Assert.Ignore("TODO");
            this.SetUpMocks();
            var underTest = this.CreateSolver();
            var uuid = Guid.NewGuid();
            DateTime lastWriteTime = DateTime.UtcNow;
            var file = Mock.Of<IFileInfo>(
                f =>
                f.Name == this.fileName &&
                f.Exists == true &&
                f.LastWriteTimeUtc == lastWriteTime &&
                f.Uuid == uuid);
            var remoteDoc = new Mock<IDocument>();
            var remotePwcDoc = new Mock<IDocument>();
            remoteDoc.SetupCheckout(remotePwcDoc, Guid.NewGuid().ToString());
            var mappedFile = this.storage.AddLocalFile(file, remoteDoc.Object.Id);
            underTest.Solve(file, Mock.Of<IDocument>(), ContentChangeType.CHANGED);

            // this.storage.VerifySavedMappedObject(MappedObjectType.File, newMasterDocumentId, this.fileName, this.parentId, newMasterDocumentChangeToken, contentSize: file.Object.Length, checksum: fileCheckSum);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void SolverUploadsFileContentWithNewPWCAndGetsInterrupted() {
            this.SetUpMocks();
            var underTest = this.CreateSolver();
            Assert.Ignore("TODO");
        }

        [Test, Category("Fast"), Category("Solver")]
        public void SolverContinesUploadFileContentWithStoredInformations() {
            this.SetUpMocks();
            var underTest = this.CreateSolver();
            Assert.Ignore("TODO");
        }

        [Test, Category("Fast"), Category("Solver")]
        public void SolverContinesUploadFileContentWithStoredInformationsAndGetsInterruptedAgain() {
            this.SetUpMocks();
            var underTest = this.CreateSolver();
            Assert.Ignore("TODO");
        }

        [Test, Category("Fast"), Category("Solver")]
        public void SolverUploadsFileContentByCreatingNewPwcIfPwcWasCanceled() {
            this.SetUpMocks();
            var underTest = this.CreateSolver();
            Assert.Ignore("TODO");
        }

        [Test, Category("Fast"), Category("Solver")]
        public void SolverUploadsFileContentByCreatingNewPwcIfObjectNotFoundOnServer() {
            this.SetUpMocks();
            var underTest = this.CreateSolver();
            Assert.Ignore("TODO");
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