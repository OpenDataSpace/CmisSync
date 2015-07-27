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

    using CmisSync.Lib.Consumer;
    using CmisSync.Lib.Consumer.SituationSolver;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Exceptions;
    using CmisSync.Lib.FileTransmission;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;
    using DotCMIS.Exceptions;

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
        public void ConstructorSetsPropertiesCorrectly([Values(true, false)]bool withGivenTransmissionStorage) {
            var session = new Mock<ISession>();
            session.SetupTypeSystem();
            var storage = Mock.Of<IMetaDataStorage>();
            var transmissionStorage = withGivenTransmissionStorage ? Mock.Of<IFileTransmissionStorage>() : null;
            var underTest = new SolverClass(session.Object, storage, transmissionStorage);

            Assert.That(underTest.GetSession(), Is.EqualTo(session.Object));
            Assert.That(underTest.GetStorage(), Is.EqualTo(storage));
            Assert.That(underTest.GetTransmissionStorage(), Is.EqualTo(transmissionStorage));
        }

        [Test, Category("Fast"), Category("Solver")]
        public void ConstructorSetsServerPropertyCorrectly([Values(true, false)]bool serverCanModifyLastModificationDate) {
            var session = new Mock<ISession>();
            session.SetupTypeSystem(serverCanModifyLastModificationDate);
            var underTest = new SolverClass(session.Object, Mock.Of<IMetaDataStorage>());

            Assert.That(underTest.GetModification(), Is.EqualTo(serverCanModifyLastModificationDate));
        }

        [Test, Category("Fast"), Category("Solver")]
        public void EnsureLegalCharactersThrowsExceptionIfFilenameContainsUtf8Character() {
            var session = new Mock<ISession>();
            session.SetupTypeSystem();
            var underTest = new SolverClass(session.Object, Mock.Of<IMetaDataStorage>());
            var exception = new CmisConstraintException();
            var fileInfo = Mock.Of<IFileSystemInfo>(f => f.Name == @"ä" && f.FullName == @"ä");

            Assert.Throws<InteractionNeededException>(() => underTest.CallEnsureThatLocalFileNameContainsLegalCharacters(fileInfo, exception));
        }

        [Test, Category("Fast"), Category("Solver")]
        public void EnsureLegalCharactersIfFilenameIsValid() {
            var session = new Mock<ISession>();
            session.SetupTypeSystem();
            var underTest = new SolverClass(session.Object, Mock.Of<IMetaDataStorage>());
            var exception = new CmisConstraintException();
            var fileInfo = Mock.Of<IFileSystemInfo>(f => f.Name == "foo");

            underTest.CallEnsureThatLocalFileNameContainsLegalCharacters(fileInfo, exception);
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
                IMetaDataStorage storage,
                IFileTransmissionStorage transmissionStorage = null) : base(session, storage, transmissionStorage) {
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

            public IFileTransmissionStorage GetTransmissionStorage() {
                return this.TransmissionStorage;
            }

            public byte[] Upload(IFileInfo localFile, IDocument doc, ITransmissionFactory transmissionManager) {
                var transmission = transmissionManager.CreateTransmission(TransmissionType.UPLOAD_MODIFIED_FILE, localFile.FullName);
                return this.UploadFile(localFile, doc, transmission);
            }

            public override void Solve(
                IFileSystemInfo localFileSystemInfo,
                IObjectId remoteId,
                ContentChangeType localContent,
                ContentChangeType remoteContent)
            {
                throw new NotImplementedException();
            }

            public void CallEnsureThatLocalFileNameContainsLegalCharacters(IFileSystemInfo fileInfo, CmisConstraintException e) {
                this.EnsureThatLocalFileNameContainsLegalCharacters(fileInfo, e);
            }
        }
    }
}