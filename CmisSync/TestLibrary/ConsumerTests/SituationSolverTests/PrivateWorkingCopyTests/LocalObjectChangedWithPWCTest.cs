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
    using System.Collections.Generic;
    using System.Security.Cryptography;
    using System.IO;

    using CmisSync.Lib.Consumer.SituationSolver;
    using CmisSync.Lib.Consumer.SituationSolver.PWC;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;

    using DataSpace.Common.Transmissions;

    using DotCMIS.Client;
    using DotCMIS.Data;
    using DotCMIS.Exceptions;

    using Moq;

    using NUnit.Framework;

    using TestUtils;

    [TestFixture]
    public class LocalObjectChangedWithPWCTest : IsTestWithConfiguredLog4Net {
        private readonly string parentId = "parentId";
        private readonly string fileName = "file.bin";
        private readonly string objectIdOld = "objectIdOld";
        private readonly string objectIdPWC = "objectIdPWC";
        private readonly string objectIdNew = "objectIdNew";
        private readonly string changeTokenOld = "changeTokenOld";
        private readonly string changeTokenPWC = "changeTokenPWC";
        private readonly string changeTokenNew = "changeTokenNew";

        private Mock<ISession> session;
        private Mock<IMetaDataStorage> storage;
        private Mock<IFileTransmissionStorage> transmissionStorage;
        private Mock<ITransmissionFactory> manager;
        private Mock<ISolver> folderOrFileContentUnchangedAddedSolver;

        private string parentPath;
        private string localPath;
        private long chunkSize;

        private Mock<IFileInfo> localFile;
        private Mock<IDocument> remoteDocument;
        private Mock<IDocument> remoteDocumentPWC;
        private Mock<IMappedObject> mappedObject;

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
        public void ConstructorDoesNotTouchesSession() {
            this.SetUpMocks();
            this.session = new Mock<ISession>(MockBehavior.Strict);
            this.CreateSolver();
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
        public void SolveCallIsDeniedIfRemoteFileIsChanged([Values(123456)]long fileSize) {
            this.SetUpMocks();

            this.SetupFile();
            this.mappedObject.Object.LastChangeToken = this.changeTokenOld + ".change";

            var underTest = this.CreateSolver();
            Assert.Throws<ArgumentException>(() => underTest.Solve(this.localFile.Object, this.remoteDocument.Object, ContentChangeType.CHANGED));
        }

        [Test, Category("Fast"), Category("Solver")]
        public void SolverDeniesAnyRequestIfRemoteDocumentIsReadOnly() {
            this.SetUpMocks();
            this.SetupFile();
            this.remoteDocument.SetupReadOnly(true);
            var underTest = this.CreateSolver();
            underTest.Solve(this.localFile.Object, this.remoteDocument.Object, ContentChangeType.CHANGED);
            this.storage.VerifyThatNoObjectIsManipulated();
            this.manager.VerifyThatNoTransmissionIsCreated();
            this.transmissionStorage.VerifyThatNoObjectIsAddedChangedOrDeleted();
        }

        [Test, Category("Fast"), Category("Solver")]
        public void SolverUploadsFileContentByCreatingNewPWC([Values(1, 1024, 123456)]long fileSize) {
            this.SetUpMocks();
            this.SetupFile();
            this.manager.SetupCreateTransmissionOnce(TransmissionType.UploadModifiedFile, this.localPath);
            byte[] content = new byte[fileSize];
            var hash = SHA1.Create().ComputeHash(content);
            this.localFile.SetupStream(content);

            this.remoteDocument.SetupCheckout(this.remoteDocumentPWC, this.changeTokenNew, this.objectIdNew);

            var underTest = this.CreateSolver();
            underTest.Solve(this.localFile.Object, this.remoteDocument.Object, ContentChangeType.CHANGED);

            this.storage.VerifySavedMappedObject(MappedObjectType.File, this.objectIdNew, this.fileName, this.parentId, this.changeTokenNew, contentSize: fileSize, checksum: hash);
            this.manager.VerifyThatTransmissionWasCreatedOnce();
        }

        [Test, Category("Fast"), Category("Solver")]
        public void SolverUploadsFileContentByCreatingNewPWCButCheckInIsDenied() {
            long fileSize = 1024;
            this.SetUpMocks();
            this.SetupFile();
            this.manager.SetupCreateTransmissionOnce(TransmissionType.UploadModifiedFile, this.localPath);
            byte[] content = new byte[fileSize];
            this.localFile.SetupStream(content);

            this.remoteDocument.SetupCheckout(this.remoteDocumentPWC, this.changeTokenNew, this.objectIdNew);
            this.remoteDocumentPWC.Setup(d => d.CheckIn(It.IsAny<bool>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<IContentStream>(), It.IsAny<string>())).Throws<CmisPermissionDeniedException>();

            var underTest = this.CreateSolver();
            underTest.Solve(this.localFile.Object, this.remoteDocument.Object, ContentChangeType.CHANGED);

            this.storage.VerifyThatNoObjectIsManipulated();
            this.manager.VerifyThatTransmissionWasCreatedOnce();
        }

        [Test, Category("Fast"), Category("Solver"), Ignore("TODO")]
        public void SolverUploadsFileContentWithNewPWCAndGetsInterrupted() {
        }

        [Test, Category("Fast"), Category("Solver"), Ignore("TODO")]
        public void SolverContinesUploadFileContentWithStoredInformations() {
            Assert.Ignore("TODO");
        }

        [Test, Category("Fast"), Category("Solver"), Ignore("TODO")]
        public void SolverContinesUploadFileContentWithStoredInformationsAndGetsInterruptedAgain() {
            Assert.Ignore("TODO");
        }

        [Test, Category("Fast"), Category("Solver"), Ignore("TODO")]
        public void SolverUploadsFileContentByCreatingNewPwcIfPwcWasCanceled() {
            Assert.Ignore("TODO");
        }

        [Test, Category("Fast"), Category("Solver"), Ignore("TODO")]
        public void SolverUploadsFileContentByCreatingNewPwcIfObjectNotFoundOnServer() {
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

        private void SetUpMocks(bool serverCanModifyLastModificationDate = true) {
            this.session = new Mock<ISession>();
            this.session.SetupTypeSystem(serverCanModifyLastModificationDate: serverCanModifyLastModificationDate);
            this.storage = new Mock<IMetaDataStorage>();
            this.chunkSize = 4096;
            this.transmissionStorage = new Mock<IFileTransmissionStorage>();
            this.manager = new Mock<ITransmissionFactory>(MockBehavior.Strict);
            this.transmissionStorage.Setup(f => f.ChunkSize).Returns(this.chunkSize);
            this.folderOrFileContentUnchangedAddedSolver = new Mock<ISolver>(MockBehavior.Strict);
        }

        private void SetupFile() {
            this.parentPath = Path.GetTempPath();
            this.localPath = Path.Combine(this.parentPath, this.fileName);

            var parentDirInfo = Mock.Of<IDirectoryInfo>(d => d.FullName == this.parentPath && d.Name == Path.GetFileName(this.parentPath) && d.Exists == true);
            this.storage.AddLocalFolder(parentDirInfo, this.parentId);

            var file = Mock.Of<IFileInfo>(
                f =>
                f.FullName == this.localPath &&
                f.Name == this.fileName &&
                f.Exists == true &&
                f.IsExtendedAttributeAvailable() == true &&
                f.Directory == parentDirInfo);
            this.localFile = Mock.Get(file);

            var parents = new List<IFolder>();
            parents.Add(Mock.Of<IFolder>(f => f.Id == this.parentId));

            var doc = Mock.Of<IDocument>(
                d =>
                d.Name == this.fileName &&
                d.Id == this.objectIdOld &&
                d.Parents == parents &&
                d.ChangeToken == this.changeTokenOld);
            this.remoteDocument = Mock.Get(doc);
            this.session.AddRemoteObject(doc);

            var docPWC = Mock.Of<IDocument>(
                d =>
                d.Name == this.fileName &&
                d.Id == this.objectIdPWC &&
                d.ChangeToken == this.changeTokenPWC);
            this.remoteDocumentPWC = Mock.Get(docPWC);
            long length = 0;
            this.remoteDocumentPWC.Setup(d => d.AppendContentStream(It.IsAny<IContentStream>(), It.IsAny<bool>(), It.IsAny<bool>())).Callback<IContentStream, bool, bool>((stream, last, refresh) => {
                byte[] buffer = new byte[stream.Length.GetValueOrDefault()];
                length += stream.Stream.Read(buffer, 0, buffer.Length);
            });
            this.remoteDocumentPWC.Setup(d => d.ContentStreamLength).Returns(() => { return length; });
            this.session.AddRemoteObject(docPWC);

            this.remoteDocument.SetupCheckout(this.remoteDocumentPWC, this.changeTokenNew, this.objectIdNew);
            this.session.Setup(s => s.GetObject(It.Is<IObjectId>(o => o.Id == this.objectIdNew))).Returns<IObjectId>((id) => {
                Assert.AreEqual(id.Id, doc.Id);
                return doc;
            });

            this.mappedObject = this.storage.AddLocalFile(this.localFile.Object, this.objectIdOld);
            this.mappedObject.Object.LastChangeToken = this.changeTokenOld;
            this.mappedObject.Object.ParentId = this.parentId;
            this.mappedObject.Object.Guid = Guid.NewGuid();
        }
    }
}