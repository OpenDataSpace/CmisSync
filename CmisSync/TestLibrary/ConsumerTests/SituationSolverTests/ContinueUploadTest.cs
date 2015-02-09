//-----------------------------------------------------------------------
// <copyright file="LocalObjectAddedTest.cs" company="GRAU DATA AG">
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
    using System.Collections.Generic;
    using System.Security.Cryptography;
    using System.IO;

    using CmisSync.Lib.Storage.FileSystem;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.FileTransmission;
    using CmisSync.Lib.Consumer.SituationSolver;

    using DotCMIS.Client;
    using DotCMIS.Data;

    using Moq;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture]
    public class ContinueUploadTest : IsTestWithConfiguredLog4Net {
        private readonly string parentId = "parentId";
        private readonly string objectName = "objectName";
        private readonly string objectOldId = "objectId";
        private readonly string objectPWCId = "objectPWCId";
        private readonly string objectNewId = "objectId";   //  for OpenDataSpace CMIS gateway, remote object ID will not change after checkin
        private readonly string changeTokenOld = "changeTokenOld";
        private readonly string changeTokenPWC = "changeTokenPWC";
        private readonly string changeTokenNew = "changeTokenNew";

        private Mock<ISession> session;
        private Mock<IMetaDataStorage> storage;
        private Mock<IFileTransmissionStorage> transmissionStorage;

        private ActiveActivitiesManager transmissionManager;

        private readonly byte[] emptyHash = SHA1.Create().ComputeHash(new byte[0]);
        private readonly long chunkSize = 8 * 1024;
        private readonly int chunkCount = 4;
        private Mock<IFileInfo> localFile;
        private Mock<IDocument> remoteDocument;
        private Mock<IDocument> remotePWCDocument;

        [SetUp]
        public void Setup() {
            this.session = new Mock<ISession>();
            this.session.SetupTypeSystem();
            //this.session.SetupCreateOperationContext();

            this.storage = new Mock<IMetaDataStorage>();
            this.storage.Setup(f => f.SaveMappedObject(It.IsAny<IMappedObject>())).Callback<IMappedObject>((o) => {
                this.storage.Setup(f => f.GetObjectByLocalPath(It.IsAny<IFileSystemInfo>())).Returns(o);
            });

            this.transmissionStorage = new Mock<IFileTransmissionStorage>();
            this.transmissionStorage.Setup(f => f.SaveObject(It.IsAny<IFileTransmissionObject>())).Callback<IFileTransmissionObject>((o) => {
                this.transmissionStorage.Setup(f => f.GetObjectByRemoteObjectId(o.RemoteObjectId)).Returns(o);
            });

            this.session.Setup(f => f.RepositoryInfo.Capabilities.IsPwcUpdatableSupported).Returns(true);
            this.transmissionStorage.Setup(f => f.ChunkSize).Returns(this.chunkSize);

            this.transmissionManager = new ActiveActivitiesManager();
        }

        private void SetupFile() {
            this.localFile = new Mock<IFileInfo>();
            this.remoteDocument = new Mock<IDocument>();
            this.remotePWCDocument = new Mock<IDocument>();

            var parentDirInfo = Mock.Of<IDirectoryInfo>(d => d.FullName == Path.GetTempPath() && d.Name == Path.GetFileName(Path.GetTempPath()));
            this.storage.Setup(f => f.GetObjectByLocalPath(It.Is<IDirectoryInfo>(d => d.FullName == Path.GetTempPath()))).Returns(Mock.Of<IMappedObject>(o => o.RemoteObjectId == parentId));

            var parents = new List<IFolder>();
            parents.Add(Mock.Of<IFolder>(f => f.Id == this.parentId));

            string path = Path.Combine(Path.GetTempPath(), this.objectName);

            var file = Mock.Of<IFileInfo>(
                f =>
                f.FullName == path &&
                f.Name == this.objectName &&
                f.Exists == true &&
                f.IsExtendedAttributeAvailable() == true &&
                f.Directory == parentDirInfo);
            this.localFile = Mock.Get(file);

            var docId = Mock.Of<IObjectId>(
                o =>
                o.Id == this.objectOldId);

            var doc = Mock.Of<IDocument>(
                d =>
                d.Name == this.objectName &&
                d.Id == this.objectOldId &&
                d.Parents == parents &&
                d.ChangeToken == this.changeTokenOld);
            this.remoteDocument = Mock.Get(doc);

            this.session.Setup(s => s.CreateDocument(
                It.IsAny<IDictionary<string, object>>(),
                It.IsAny<IObjectId>(),
                It.IsAny<IContentStream>(),
                null,
                null,
                null,
                null)).Returns(docId);
            this.remoteDocument.Setup(
                d =>
                d.SetContentStream(It.IsAny<IContentStream>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .Callback<IContentStream, bool, bool>((s, o, r) => {
                    using (var temp = new MemoryStream()) {
                        s.Stream.CopyTo(temp);
                    }
                });
            this.remoteDocument.Setup(d => d.LastModificationDate).Returns(new DateTime());
            this.session.Setup(s => s.GetObject(It.Is<IObjectId>(o => o == docId), It.IsAny<IOperationContext>())).Returns(doc);

            var docPWC = Mock.Of<IDocument>(
                d =>
                d.Name == this.objectName &&
                d.Id == this.objectPWCId &&
                d.ChangeToken == this.changeTokenPWC);
            this.remotePWCDocument = Mock.Get(docPWC);
            this.remotePWCDocument.SetupAllProperties();
            long length = 0;
            this.remotePWCDocument.Setup(d => d.AppendContentStream(It.IsAny<IContentStream>(), It.IsAny<bool>(), It.IsAny<bool>())).Callback<IContentStream, bool, bool>((stream, last, refresh) => {
                byte[] buffer = new byte[stream.Length.GetValueOrDefault()];
                length += stream.Stream.Read(buffer, 0, buffer.Length);
            });
            this.remotePWCDocument.Setup(d => d.ContentStreamLength).Returns(() => { return length; });
            this.session.Setup(s=>s.GetObject(this.objectPWCId)).Returns(this.remotePWCDocument.Object);

            this.remoteDocument.SetupCheckout(this.remotePWCDocument, this.changeTokenNew);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalFileAdded() {
            this.SetupFile();

            long fileLength = this.chunkCount * this.chunkSize;
            var fileContent = new byte[fileLength];
            byte[] hash = SHA1Managed.Create().ComputeHash(fileContent);

            var stream = new Mock<MemoryStream>();
            stream.SetupAllProperties();
            stream.Setup(s => s.CanRead).Returns(true);
            stream.Setup(s => s.Length).Returns(fileLength);
            long readLength = 0;
            stream.Setup(s => s.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns((byte[] buffer, int offset, int count) => {
                if (readLength > 0) {
                    foreach (FileTransmissionEvent transmissionEvent in this.transmissionManager.ActiveTransmissions) {
                        transmissionEvent.ReportProgress(new TransmissionProgressEventArgs { Aborting = true });
                    }
                }
                if (readLength + count > fileLength) {
                    count = (int)(fileLength - readLength);
                }
                Array.Copy(fileContent, readLength, buffer, offset, count);
                readLength += count;
                return count;
            });
            stream.Setup(s => s.Position).Returns(() => { return readLength; });

            this.localFile.Setup(f => f.Length).Returns(fileLength);
            this.localFile.Setup(f => f.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete)).Returns(stream.Object);

            var solverAdded = new LocalObjectAdded(this.session.Object, this.storage.Object, this.transmissionStorage.Object, this.transmissionManager);
            Assert.Throws<AbortException>(() => solverAdded.Solve(this.localFile.Object, null));
            Assert.That(this.transmissionManager.ActiveTransmissions, Is.Empty);

            readLength = 0;
            stream.Setup(s => s.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns((byte[] buffer, int offset, int count) => {
                if (readLength + count > fileLength) {
                    count = (int)(fileLength - readLength);
                }
                Array.Copy(fileContent, readLength, buffer, offset, count);
                readLength += count;
                return count;
            });

            var solverChanged = new LocalObjectChanged(this.session.Object, this.storage.Object, this.transmissionStorage.Object, this.transmissionManager);

            solverChanged.Solve(this.localFile.Object, this.remoteDocument.Object);
            Assert.That(transmissionManager.ActiveTransmissions, Is.Empty);

            this.storage.VerifySavedMappedObject(MappedObjectType.File, this.objectNewId, this.objectName, this.parentId, this.changeTokenNew, Times.Exactly(2), true, null, null, hash, fileLength);
            this.session.Verify(
                s =>
                s.CreateDocument(
                It.Is<IDictionary<string, object>>(p => (string)p["cmis:name"] == this.objectName),
                It.Is<IObjectId>(o => o.Id == this.parentId),
                It.Is<IContentStream>(st => st == null),
                null,
                null,
                null,
                null),
                Times.Once());
            this.localFile.VerifySet(f => f.Uuid = It.Is<Guid?>(uuid => uuid != null), Times.Once());
            this.localFile.VerifyThatLocalFileObjectLastWriteTimeUtcIsNeverModified();
            this.remotePWCDocument.Verify(d => d.AppendContentStream(It.IsAny<IContentStream>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Exactly(this.chunkCount + 1));  //  plus 1 for one AppendContentStream is aborted
            this.remoteDocument.VerifyUpdateLastModificationDate(this.localFile.Object.LastWriteTimeUtc, true);

        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalFileChanged() {
            Assert.Fail("TODO");
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalFileAddedWhileChangeLocalBeforeContinue() {
            Assert.Fail("TODO");
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalFileChangedWhileChangeLocalBeforeContinue() {
            Assert.Fail("TODO");
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalFileAddedWhileChangeRemoteBeforeContinue() {
            Assert.Fail("TODO");
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalFileChangedWhileChangeRemoteBeforeContinue() {
            Assert.Fail("TODO");
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalFileAddedWhileChangeLocalAndRemoteBeforeContinue() {
            Assert.Fail("TODO");
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalFileChangedWhileChangeLocalAndRemoteBeforeContinue() {
            Assert.Fail("TODO");
        }
    }
}
