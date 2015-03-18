//-----------------------------------------------------------------------
// <copyright file="LocalObjectChangedRemoteObjectChangedWithPWCTest.cs" company="GRAU DATA AG">
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

    using CmisSync.Lib.Cmis.ConvenienceExtenders;
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
    public class LocalObjectChangedRemoteObjectChangedWithPWCTest {
        private long chunkSize;
        private Mock<ISession> session;
        private Mock<IMetaDataStorage> storage;
        private Mock<IFileTransmissionStorage> transmissionStorage;
        private Mock<TransmissionManager> manager;
        private Mock<ISolver> fallbackSolver;

        [Test, Category("Fast"), Category("Solver")]
        public void Constructor() {
            this.SetUpMocks();
            this.CreateSolver();
        }

        [Test, Category("Fast"), Category("Solver")]
        public void ConstructorFailsIfSessionIsNotAbleToWorkWithPrivateWorkingCopies() {
            this.SetUpMocks(isPwcUpdateable: false);
            Assert.Throws<ArgumentException>(
                () =>
                new LocalObjectChangedRemoteObjectChangedWithPWC(
                this.session.Object,
                this.storage.Object,
                this.transmissionStorage.Object,
                this.manager.Object,
                Mock.Of<ISolver>()));
        }

        [Test, Category("Fast"), Category("Solver")]
        public void ConstructorFailsIfGivenSolverIsNull() {
            this.SetUpMocks();
            Assert.Throws<ArgumentNullException>(
                () =>
                new LocalObjectChangedRemoteObjectChangedWithPWC(
                this.session.Object,
                this.storage.Object,
                this.transmissionStorage.Object,
                this.manager.Object,
                null));
        }

        [Test, Category("Fast"), Category("Solver")]
        public void FallbackIsCalledForDirectories() {
            this.SetUpMocks();
            var underTest = this.CreateSolver();
            var dir = new Mock<IDirectoryInfo>(MockBehavior.Strict).Object;
            var remoteDir = new Mock<IObjectId>(MockBehavior.Strict).Object;
            this.fallbackSolver.Setup(s => s.Solve(dir, remoteDir, ContentChangeType.NONE, ContentChangeType.NONE));

            underTest.Solve(dir, remoteDir, ContentChangeType.NONE, ContentChangeType.NONE);

            this.fallbackSolver.Verify(s => s.Solve(dir, remoteDir, ContentChangeType.NONE, ContentChangeType.NONE), Times.Once());
        }

        [Test, Category("Fast"), Category("Solver"), Ignore("TODO")]
        public void FallbackIsNotUsedIfOnlyLocalContentHasBeenChanged() {
            this.SetUpMocks();
            var underTest = this.CreateSolver();
            var file = new Mock<IFileInfo>(MockBehavior.Strict).Object;
            var remoteDoc = new Mock<IDocument>(MockBehavior.Strict).Object;

            underTest.Solve(file, remoteDoc, ContentChangeType.CHANGED, ContentChangeType.NONE);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void FallbackIsCalledIfRemoteContentHasBeenChanged(
            [Values(ContentChangeType.NONE, ContentChangeType.APPENDED, ContentChangeType.CHANGED, ContentChangeType.CREATED, ContentChangeType.DELETED)]ContentChangeType localChange,
            [Values(ContentChangeType.APPENDED, ContentChangeType.CHANGED, ContentChangeType.CREATED, ContentChangeType.DELETED)]ContentChangeType remoteChange) {
            this.SetUpMocks();
            var underTest = this.CreateSolver();
            var file = new Mock<IFileInfo>(MockBehavior.Strict).Object;
            var remoteDoc = new Mock<IDocument>(MockBehavior.Strict).Object;
            this.fallbackSolver.Setup(s => s.Solve(file, remoteDoc, localChange, remoteChange));

            underTest.Solve(file, remoteDoc, localChange, remoteChange);

            this.fallbackSolver.Verify(s => s.Solve(file, remoteDoc, localChange, remoteChange), Times.Once());
        }

        private LocalObjectChangedRemoteObjectChangedWithPWC CreateSolver() {
            return new LocalObjectChangedRemoteObjectChangedWithPWC(
                this.session.Object,
                this.storage.Object,
                this.transmissionStorage.Object,
                this.manager.Object,
                this.fallbackSolver.Object);
        }

        private void SetUpMocks(bool isPwcUpdateable = true, bool serverCanModifyLastModificationDate = true) {
            this.session = new Mock<ISession>();
            this.session.SetupTypeSystem(serverCanModifyLastModificationDate: serverCanModifyLastModificationDate);
            this.session.SetupPrivateWorkingCopyCapability(isPwcUpdateable: isPwcUpdateable);

            this.storage = new Mock<IMetaDataStorage>();

            this.chunkSize = 4096;
            this.transmissionStorage = new Mock<IFileTransmissionStorage>();
            this.transmissionStorage.Setup(f => f.ChunkSize).Returns(this.chunkSize);

            this.manager = new Mock<TransmissionManager>();

            this.fallbackSolver = new Mock<ISolver>(MockBehavior.Strict);
        }
    }
}