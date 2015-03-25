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
    using System.Security.Cryptography;
    using System.IO;

    using CmisSync.Lib.Cmis.ConvenienceExtenders;
    using CmisSync.Lib.Consumer.SituationSolver;
    using CmisSync.Lib.Consumer.SituationSolver.PWC;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;
    using DotCMIS.Data;

    using Moq;

    using NUnit.Framework;

    using TestUtils;

    [TestFixture]
    public class LocalObjectChangedRemoteObjectChangedWithPWCTest {
        private Mock<ISession> session;
        private Mock<IMetaDataStorage> storage;
        private Mock<IFileTransmissionStorage> transmissionStorage;
        private Mock<ActiveActivitiesManager> manager;
        private Mock<ISolver> fallbackSolver;

        private readonly string parentId = "parentId";
        private readonly string fileName = "file.bin";
        private readonly string objectIdOld = "objectIdOld";
        private readonly string objectIdPWC = "objectIdPWC";
        private readonly string objectIdNew = "objectIdNew";
        private readonly string changeTokenOld = "changeTokenOld";
        private readonly string changeTokenPWC = "changeTokenPWC";
        private readonly string changeTokenNew = "changeTokenNew";

        private Mock<IFileInfo> localFile;
        private Mock<IDocument> remoteDocument;
        private Mock<IDocument> remoteDocumentPWC;
        private Mock<IMappedObject> mappedObject;

        private string parentPath;
        private string localPath;
        private long chunkSize;

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

        [Test, Category("Fast"), Category("Solver")]
        public void FallbackIsNotUsedIfOnlyLocalContentHasBeenChanged() {
            this.SetUpMocks();

            this.SetupFile();
            Mock<IMappedObject> obj = Mock.Get(this.storage.Object.GetObjectByRemoteId(this.objectIdOld));
            this.mappedObject.Object.LastChecksum = SHA1.Create().ComputeHash(new byte[0]);
            this.mappedObject.Object.ChecksumAlgorithmName = "SHA-1";
            this.mappedObject.Object.LastContentSize = 0;

            long fileSize = this.chunkSize * 4;
            byte[] content = new byte[fileSize];
            var hash = SHA1.Create().ComputeHash(content);
            this.localFile.SetupStream(content);

            DateTime now = DateTime.UtcNow;
            obj.Object.LastRemoteWriteTimeUtc = now - TimeSpan.FromHours(2);
            this.remoteDocument.Setup(d => d.LastModificationDate).Returns(now - TimeSpan.FromHours(1));
            obj.Object.LastLocalWriteTimeUtc = now - TimeSpan.FromHours(2);
            this.localFile.Setup(f => f.LastWriteTimeUtc).Returns(now);

             var underTest = this.CreateSolver();
            this.fallbackSolver.Setup(s => s.Solve(this.localFile.Object, this.remoteDocument.Object, ContentChangeType.CHANGED, ContentChangeType.NONE));

            underTest.Solve(this.localFile.Object, this.remoteDocument.Object, ContentChangeType.CHANGED, ContentChangeType.NONE);

            this.fallbackSolver.Verify(s => s.Solve(this.localFile.Object, this.remoteDocument.Object, ContentChangeType.CHANGED, ContentChangeType.NONE), Times.Never());
            this.storage.VerifySavedMappedObject(MappedObjectType.File, this.objectIdNew, this.fileName, this.parentId, this.changeTokenNew, contentSize: fileSize, checksum: hash, lastLocalModification: now);
            this.remoteDocument.VerifyUpdateLastModificationDate(now, Times.Once(), true);
        }

        private LocalObjectChangedRemoteObjectChangedWithPWC CreateSolver() {
            return new LocalObjectChangedRemoteObjectChangedWithPWC(
                this.session.Object,
                this.storage.Object,
                this.transmissionStorage.Object,
                this.manager.Object,
                this.fallbackSolver.Object);
        }

        private void SetupFile() {
            this.parentPath = Path.GetTempPath();
            this.localPath = Path.Combine(this.parentPath, this.fileName);

            var file = Mock.Of<IFileInfo>(
                f =>
                f.FullName == this.localPath &&
                f.Name == this.fileName &&
                f.Exists == true &&
                f.IsExtendedAttributeAvailable() == true);
            this.localFile = Mock.Get(file);

            var doc = Mock.Of<IDocument>(
                d =>
                d.Name == this.fileName &&
                d.Id == this.objectIdOld &&
                d.ChangeToken == this.changeTokenOld);
            this.remoteDocument = Mock.Get(doc);
            this.session.AddRemoteObject(doc);

            var docPWC = Mock.Of<IDocument>(
                d =>
                d.Name == this.fileName &&
                d.Id == this.objectIdPWC &&
                d.ChangeToken == this.changeTokenPWC);
            this.remoteDocumentPWC = Mock.Get(docPWC);
            this.session.AddRemoteObject(docPWC);

            long length = 0;
            this.remoteDocumentPWC.Setup(d => d.AppendContentStream(It.IsAny<IContentStream>(), It.IsAny<bool>(), It.IsAny<bool>())).Callback<IContentStream, bool, bool>((stream, last, refresh) => {
                byte[] buffer = new byte[stream.Length.GetValueOrDefault()];
                length += stream.Stream.Read(buffer, 0, buffer.Length);
            });
            this.remoteDocumentPWC.Setup(d => d.ContentStreamLength).Returns(() => { return length; });

            this.remoteDocument.SetupCheckout(this.remoteDocumentPWC, this.changeTokenNew, this.objectIdNew);
            this.session.Setup(s => s.GetObject(It.Is<IObjectId>(o => o.Id == this.objectIdNew))).Returns<IObjectId>((id) => {
                Assert.AreEqual(id.Id, doc.Id);
                return doc;
            });

            this.mappedObject = new Mock<IMappedObject>();
            this.storage.Setup(s => s.GetObjectByRemoteId(It.Is<string>(id => id == this.objectIdOld))).Returns(this.mappedObject.Object);
            this.mappedObject.SetupAllProperties();
            this.mappedObject.Setup(o => o.Type).Returns(MappedObjectType.File);
            this.mappedObject.Object.RemoteObjectId = this.objectIdOld;
            this.mappedObject.Object.Name = this.fileName;
            this.mappedObject.Object.LastChangeToken = this.changeTokenOld;
            this.mappedObject.Object.Guid = Guid.NewGuid();
            this.mappedObject.Object.ParentId = this.parentId;
        }

        private void SetUpMocks(bool isPwcUpdateable = true, bool serverCanModifyLastModificationDate = true) {
            this.session = new Mock<ISession>();
            this.session.SetupTypeSystem(serverCanModifyLastModificationDate: serverCanModifyLastModificationDate);
            this.session.SetupPrivateWorkingCopyCapability(isPwcUpdateable: isPwcUpdateable);

            this.storage = new Mock<IMetaDataStorage>();

            this.chunkSize = 4096;
            this.transmissionStorage = new Mock<IFileTransmissionStorage>();
            this.transmissionStorage.Setup(f => f.ChunkSize).Returns(this.chunkSize);

            this.manager = new Mock<ActiveActivitiesManager>();

            this.fallbackSolver = new Mock<ISolver>(MockBehavior.Strict);
        }
    }
}