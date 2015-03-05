//-----------------------------------------------------------------------
// <copyright file="AbstractEnhancedSolverTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.ConsumerTests.SituationSolverTests {
    using System;

    using CmisSync.Lib.Consumer.SituationSolver;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;

    using Moq;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture]
    public class AbstractEnhancedSolverTest {
        [Test, Category("Fast"), Category("Solver")]
        public void ConstructorThrowsExceptionIfSessionIsNull() {
            Assert.Throws<ArgumentNullException>(() => new SolverClass(null, Mock.Of<IMetaDataStorage>()));
        }

        [Test, Category("Fast"), Category("Solver")]
        public void ConstructorThrowsExceptionIfStorageIsNull() {
            Assert.Throws<ArgumentNullException>(() => new SolverClass(Mock.Of<ISession>(), null));
        }

        [Test, Category("Fast"), Category("Solver")]
        public void ConstructorSetsPropertiesCorrectly() {
            var session = new Mock<ISession>();
            session.SetupTypeSystem();
            var storage = Mock.Of<IMetaDataStorage>();

            var underTest = new SolverClass(session.Object, storage);

            Assert.That(underTest.GetSession(), Is.EqualTo(session.Object));
            Assert.That(underTest.GetStorage(), Is.EqualTo(storage));
        }

        [Test, Category("Fast"), Category("Solver")]
        public void ConstructorSetsServerPropertyCorrectly() {
            var session = new Mock<ISession>();
            session.SetupTypeSystem(true);

            var underTest = new SolverClass(session.Object, Mock.Of<IMetaDataStorage>());

            Assert.That(underTest.GetModification(), Is.True);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void ConstructorSetsModificationPossibilityToFalse() {
            var session = new Mock<ISession>();
            session.SetupTypeSystem(false);
            var underTest = new SolverClass(session.Object, Mock.Of<IMetaDataStorage>());
            Assert.That(underTest.GetModification(), Is.False);
        }

        [Test, Category("Fast"), Category("Solver"), Ignore("TODO")]
        public void UploadFileClosesTransmissionOnIOException() {
            var session = new Mock<ISession>();
            session.SetupTypeSystem();

            var underTest = new SolverClass(session.Object, Mock.Of<IMetaDataStorage>());

            underTest.Upload(null, null, null);
            Assert.Fail("TODO");
        }

        [Test, Category("Fast"), Category("Solver"), Ignore("TODO")]
        public void DownloadChangesClosesTransmissionOnIOExceptionOnOpenCacheFile() {
            Assert.Fail("TODO");
        }

        private class SolverClass : AbstractEnhancedSolver {
            public SolverClass(
                ISession session,
                IMetaDataStorage storage) : base(session, storage) {
            }

            public ISession GetSession() {
                return this.Session;
            }

            public IMetaDataStorage GetStorage() {
                return this.Storage;
            }

            public bool GetModification() {
                return this.ServerCanModifyDateTimes;
            }

            public byte[] Upload(IFileInfo localFile, IDocument doc, ActiveActivitiesManager transmissionManager) {
                FileTransmissionEvent transmissionEvent = new FileTransmissionEvent(FileTransmissionType.UPLOAD_MODIFIED_FILE, localFile.FullName);
                transmissionManager.AddTransmission(transmissionEvent);
                return this.UploadFile(localFile, ref doc, transmissionEvent);
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