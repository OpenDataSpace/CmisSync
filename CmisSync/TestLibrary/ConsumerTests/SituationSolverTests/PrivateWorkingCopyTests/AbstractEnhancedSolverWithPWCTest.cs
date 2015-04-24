//-----------------------------------------------------------------------
// <copyright file="AbstractEnhancedSolverWithPWCTest.cs" company="GRAU DATA AG">
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
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Security.Cryptography;

    using CmisSync.Lib.Consumer.SituationSolver.PWC;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.FileTransmission;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;
    using DotCMIS.Data;

    using Moq;

    using NUnit.Framework;

    using TestUtils;

    [TestFixture]
    public class AbstractEnhancedSolverWithPWCTest {
        private Mock<ISession> session;
        private Mock<IMetaDataStorage> storage;
        private Mock<IFileTransmissionStorage> transmissionStorage;
        private Transmission transmission;

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

        [Test, Category("Fast"), Ignore("TODO")]
        public void UploadFileContentByPassingCheckedOutDocument([Values(0, 1024, 1024 * 1024, 10 * 1024 * 1024)]int fileLength, [Values(1024)]long chunkSize) {
            byte[] content = new byte[fileLength];
            byte[] expectedHash = SHA1Managed.Create().ComputeHash(content);
            var underTest = this.InitializeMocksAndCreateSolver(chunkSize: chunkSize);
            var localFile = new Mock<IFileInfo>();
            localFile.SetupStream(content);
            var checkedOutDoc = new Mock<IDocument>();
            var checkedOutId = Guid.NewGuid().ToString();
            checkedOutDoc.Setup(d => d.Name).Returns("testfile.bin");
            checkedOutDoc.Setup(d => d.Id).Returns(checkedOutId);
            checkedOutDoc.Setup(d => d.VersionSeriesCheckedOutId).Returns(checkedOutId);
            checkedOutDoc.Setup(d => d.DeleteContentStream()).Callback(() => {
                checkedOutDoc.Setup(d => d.ContentStreamId).Returns((string)null);
                checkedOutDoc.Setup(d => d.ContentStreamLength).Returns(0);
                checkedOutDoc.Setup(d => d.GetContentStream()).Returns((IContentStream)null);
            });
            var mockedDoc = new Mock<IDocument>();
            var mockedDocId = Guid.NewGuid().ToString();
            mockedDoc.Setup(d => d.Id).Returns(mockedDocId);
            var newChangeToken = "new change token";
            mockedDoc.SetupCheckout(checkedOutDoc, newChangeToken);
            this.session.AddRemoteObjects(mockedDoc.Object, checkedOutDoc.Object);
            var doc = this.session.Object.GetObject(mockedDoc.Object.CheckOut()) as IDocument;
            var hash = underTest.CallUploadFileWithPWC(localFile.Object, ref doc, this.transmission, null);

            Assert.That(hash, Is.EqualTo(expectedHash));
            checkedOutDoc.Verify(d => d.CheckIn(true, It.IsAny<IDictionary<string, object>>(), It.IsAny<IContentStream>(), It.IsAny<string>()), Times.Once);
        }

        private SolverClass InitializeMocksAndCreateSolver(TransmissionType type = TransmissionType.UPLOAD_NEW_FILE, long chunkSize = 1024) {
            this.session = new Mock<ISession>().SetupTypeSystem().SetupPrivateWorkingCopyCapability();
            this.storage = new Mock<IMetaDataStorage>();
            this.transmissionStorage = new Mock<IFileTransmissionStorage>();
            this.transmissionStorage.Setup(s => s.ChunkSize).Returns(chunkSize);
            this.transmission = new Transmission(type, Path.GetTempPath());
            return new SolverClass(this.session.Object, this.storage.Object, this.transmissionStorage.Object);
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

            public byte[] CallUploadFileWithPWC(IFileInfo localFile, ref IDocument doc, Transmission transmission, IMappedObject mappedObject = null) {
                return this.UploadFileWithPWC(localFile, ref doc, transmission, mappedObject);
            }
        }
    }
}