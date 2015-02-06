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
        private void Setup() {
            this.session = new Mock<ISession>();
            //this.session.SetupTypeSystem();
            //this.session.SetupCreateOperationContext();

            this.storage = new Mock<IMetaDataStorage>();
            this.storage.Setup(f => f.SaveMappedObject(It.IsAny<IMappedObject>())).Callback<IMappedObject>((o) => {
                //this.storage.Setup(f => f.GetObjectByLocalPath(It.IsAny<IFileSystemInfo>())).Returns(o);
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
            parents.Add(Mock.Of<IFolder>(f => f.Id == parentId));

            string path = Path.Combine(Path.GetTempPath(), this.objectName);

            var doc = Mock.Of<IDocument>(
                f =>
                f.Name == this.objectName &&
                f.Id == this.objectOldId &&
                f.Parents == parents &&
                f.ChangeToken == this.changeTokenOld);
            var docId = Mock.Of<IObjectId>(
                o =>
                o.Id == this.objectOldId);

            this.session.Setup(s => s.CreateDocument(
                It.IsAny<IDictionary<string, object>>(),
                It.IsAny<IObjectId>(),
                It.IsAny<IContentStream>(),
                null,
                null,
                null,
                null)).Returns(docId);
            this.session.Setup(s => s.GetObject(It.Is<IObjectId>(o => o == docId), It.IsAny<IOperationContext>())).Returns(doc);
            Mock.Get(doc).Setup(
                d =>
                d.SetContentStream(It.IsAny<IContentStream>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .Callback<IContentStream, bool, bool>((s, o, r) => {
                    using (var temp = new MemoryStream()) {
                        s.Stream.CopyTo(temp);
                    }
                });
            Mock.Get(doc).Setup(d => d.LastModificationDate).Returns(new DateTime());

            this.localFile.Setup(d => d.FullName).Returns(path);
            this.localFile.Setup(d => d.Name).Returns(this.objectName);
            this.localFile.Setup(d => d.Exists).Returns(true);
            this.localFile.Setup(d => d.IsExtendedAttributeAvailable()).Returns(true);
            this.localFile.Setup(d => d.Directory).Returns(parentDirInfo);

            this.remoteDocument = Mock.Get(doc);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalFileAdded() {
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

            Mock<IFileInfo> fileInfo = new Mock<IFileInfo>();
            fileInfo.Setup(f => f.Length).Returns(fileLength);
            fileInfo.Setup(f => f.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete)).Returns(stream.Object);

            Assert.Fail("TODO");
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
