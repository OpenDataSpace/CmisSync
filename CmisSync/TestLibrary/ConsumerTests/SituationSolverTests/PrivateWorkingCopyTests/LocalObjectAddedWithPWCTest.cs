﻿//-----------------------------------------------------------------------
// <copyright file="LocalObjectAddedWithPWCTest.cs" company="GRAU DATA AG">
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
    using System.IO;
    using System.Security.Cryptography;

    using CmisSync.Lib.Cmis;
    using CmisSync.Lib.Cmis.ConvenienceExtenders;
    using CmisSync.Lib.Consumer.SituationSolver;
    using CmisSync.Lib.Consumer.SituationSolver.PWC;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;

    using DataSpace.Common.Transmissions;

    using DotCMIS;
    using DotCMIS.Client;
    using DotCMIS.Data;
    using DotCMIS.Enums;
    using DotCMIS.Exceptions;

    using Moq;

    using NUnit.Framework;

    using TestUtils;

    [TestFixture]
    public class LocalObjectAddedWithPWCTest {
        private readonly string parentId = "parentId";
        private readonly string objectName = "objectName";
        private readonly string objectIdOld = "objectIdOld";
        private readonly string objectIdPWC = "objectIdPWC";
        private readonly string objectIdNew = "objectIdNew";
        private readonly string changeTokenOld = "changeTokenOld";
        private readonly string changeTokenPWC = "changeTokenPWC";
        private readonly string changeTokenNew = "changeTokenNew";

        private Mock<ISession> session;
        private Mock<IMetaDataStorage> storage;
        private Mock<IFileTransmissionStorage> transmissionStorage;
        private Mock<TransmissionManager> manager;
        private Mock<ISolver> folderOrEmptyFileAddedSolver;
        private ITransmissionFactory transmissionFactory;

        private string parentPath;
        private string localPath;
        private long chunkSize;

        private Mock<IFileInfo> localFile;
        private Mock<IDocument> remoteDocument;
        private Mock<IDocument> remoteDocumentPWC;

        [Test, Category("Fast"), Category("Solver")]
        public void Constructor() {
            this.SetUpMocks();
            this.CreateSolver();
        }

        [Test, Category("Fast"), Category("Solver")]
        public void ConstructorDoesNotTouchesTheSession() {
            this.SetUpMocks();
            this.session = new Mock<ISession>(MockBehavior.Strict);
            this.CreateSolver();
        }

        [Test, Category("Fast"), Category("Solver")]
        public void ConstructorFailsIfGivenSolverIsNull() {
            this.SetUpMocks();
            Assert.Throws<ArgumentNullException>(() => new LocalObjectAddedWithPWC(
                this.session.Object,
                this.storage.Object,
                this.transmissionStorage.Object,
                this.transmissionFactory,
                null));
        }

        [Test, Category("Fast"), Category("Solver")]
        public void NewDirectoriesCallsArePassedToTheGivenSolver() {
            this.SetUpMocks();
            var folderSolver = new Mock<ISolver>();
            var undertest = new LocalObjectAddedWithPWC(
                this.session.Object,
                this.storage.Object,
                this.transmissionStorage.Object,
                this.transmissionFactory,
                folderSolver.Object);
            var localFolder = new Mock<IDirectoryInfo>();

            undertest.Solve(localFolder.Object, null, ContentChangeType.CREATED, ContentChangeType.NONE);

            folderSolver.Verify(s => s.Solve(localFolder.Object, null, ContentChangeType.CREATED, ContentChangeType.NONE), Times.Once());
        }

        [Test, Category("Fast"), Category("Solver")]
        public void SolverFailsIfFileIsDeleted() {
            this.SetUpMocks();

            this.SetupFile();
            this.localFile.Setup(f => f.Exists).Returns(false);

            var undertest = this.CreateSolver();

            Assert.Throws<FileNotFoundException>(() => undertest.Solve(this.localFile.Object, null));
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalEmptyFileAddedIsPassedToGivenSolver() {
            this.SetUpMocks();
            this.SetupFile();
            this.localFile.SetupStream(new byte[0]);

            var undertest = this.CreateSolver();
            this.folderOrEmptyFileAddedSolver.Setup(s => s.Solve(this.localFile.Object, null, It.IsAny<ContentChangeType>(), It.IsAny<ContentChangeType>()));

            undertest.Solve(this.localFile.Object, null, ContentChangeType.CREATED, ContentChangeType.NONE);

            this.folderOrEmptyFileAddedSolver.Verify(s => s.Solve(this.localFile.Object, null, ContentChangeType.CREATED, ContentChangeType.NONE), Times.Once());
            this.localFile.VerifyThatLocalFileObjectLastWriteTimeUtcIsNeverModified();
            this.storage.VerifyThatNoObjectIsManipulated();
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalFileAddedWithFileSize([Values(1, 1024, 123456)]int fileSize) {
            this.SetUpMocks();

            this.SetupFile();
            byte[] content = new byte[fileSize];
            byte[] hash = SHA1.Create().ComputeHash(content);
            this.localFile.SetupStream(content);

            var undertest = this.CreateSolver();
            undertest.Solve(this.localFile.Object, null);

            this.localFile.VerifyThatLocalFileObjectLastWriteTimeUtcIsNeverModified();
            this.storage.VerifySavedMappedObject(MappedObjectType.File, this.objectIdNew, this.objectName, this.parentId, this.changeTokenNew, checksum: hash, contentSize: fileSize, times: Times.Once());
            this.remoteDocument.Verify(d => d.SetContentStream(It.IsAny<IContentStream>(), It.IsAny<bool>()), Times.Never());
            int counts = (int)((fileSize + this.chunkSize - 1) / this.chunkSize);
            this.remoteDocumentPWC.Verify(d => d.AppendContentStream(It.IsAny<IContentStream>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Exactly(counts));
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalFileAddedWithReadOnlyRemoteFolder([Values(1)]int fileSize) {
            this.SetUpMocks();

            this.SetupFile();

            string parentParentId = "parentParentId";
            var parentParentDirInfo = Mock.Of<IDirectoryInfo>(d => d.FullName == Path.GetDirectoryName(this.parentPath) && d.Exists == true);
            var parentDirInfo = Mock.Of<IDirectoryInfo>(d => d.FullName == this.parentPath && d.Exists == true && d.Parent == parentParentDirInfo);
            this.localFile.Setup(f => f.Directory).Returns(parentDirInfo);
            this.storage.Setup(s => s.GetObjectByLocalPath(It.Is<IDirectoryInfo>(d => d.FullName == this.parentPath))).Returns(() => { return null; });
            this.storage.Setup(s => s.GetObjectByLocalPath(It.Is<IDirectoryInfo>(d => d.FullName == parentParentDirInfo.FullName))).Returns(Mock.Of<IMappedObject>(o => o.RemoteObjectId == parentParentId));
            this.session.Setup(s => s.GetObject(parentParentId)).Returns(Mock.Of<IFolder>(o => o.AllowableActions.Actions == new HashSet<string>()));

            byte[] content = new byte[fileSize];
            byte[] hash = SHA1.Create().ComputeHash(content);
            this.localFile.SetupStream(content);

            var undertest = this.CreateSolver();
            undertest.Solve(this.localFile.Object, null);

            this.localFile.VerifyThatLocalFileObjectLastWriteTimeUtcIsNeverModified();
            this.storage.VerifySavedMappedObject(MappedObjectType.File, this.objectIdNew, this.objectName, this.parentId, this.changeTokenNew, checksum: hash, contentSize: fileSize, times: Times.Never());
            this.remoteDocument.Verify(d => d.SetContentStream(It.IsAny<IContentStream>(), It.IsAny<bool>()), Times.Never());
            this.remoteDocumentPWC.Verify(d => d.AppendContentStream(It.IsAny<IContentStream>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Never());
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalFileIsUsedByAnotherProcessOnOpenFile() {
            this.SetUpMocks();

            this.SetupFile();
            this.localFile.Setup(f => f.Length).Returns(10);
            this.localFile.SetupOpenThrows(new IOException("Already in use by another process"));

            var undertest = this.CreateSolver();
            Assert.Throws<IOException>(() => undertest.Solve(this.localFile.Object, null));
            this.storage.VerifyThatNoObjectIsManipulated();
            this.transmissionStorage.VerifyThatNoObjectIsAddedChangedOrDeleted();
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalFileAddedAndServerDeniesCheckIn() {
            int fileSize = 1024;
            this.SetUpMocks();

            this.SetupFile();
            this.remoteDocumentPWC.Setup(d => d.CheckIn(It.IsAny<bool>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<IContentStream>(), It.IsAny<string>())).Throws<CmisPermissionDeniedException>();
            byte[] content = new byte[fileSize];
            this.localFile.SetupStream(content);

            var undertest = this.CreateSolver();
            undertest.Solve(this.localFile.Object, null);

            this.localFile.VerifyThatLocalFileObjectLastWriteTimeUtcIsNeverModified();
            this.remoteDocument.Verify(d => d.SetContentStream(It.IsAny<IContentStream>(), It.IsAny<bool>()), Times.Never());
            this.storage.VerifyThatNoObjectIsManipulated();
        }

        private void SetupFile() {
            this.parentPath = Path.GetTempPath();
            this.localPath = Path.Combine(this.parentPath, this.objectName);

            var parentDirInfo = Mock.Of<IDirectoryInfo>(d => d.FullName == this.parentPath && d.Name == Path.GetFileName(this.parentPath) && d.Exists == true);
            this.storage.AddLocalFolder(parentDirInfo, this.parentId);

            var file = Mock.Of<IFileInfo>(
                f =>
                f.FullName == this.localPath &&
                f.Name == this.objectName &&
                f.Exists == true &&
                f.IsExtendedAttributeAvailable() == true &&
                f.Directory == parentDirInfo);
            this.localFile = Mock.Get(file);

            var parents = new List<IFolder>();
            parents.Add(Mock.Of<IFolder>(f => f.Id == this.parentId));

            var docId = Mock.Of<IObjectId>(
                o =>
                o.Id == this.objectIdOld);

            this.session.Setup(s => s.CreateDocument(
                It.Is<IDictionary<string, object>>(
                d => d.Contains(new KeyValuePair<string, object>(PropertyIds.Name, this.objectName)) && d.Contains(new KeyValuePair<string, object>(PropertyIds.ObjectTypeId, BaseTypeId.CmisDocument.GetCmisValue()))),
                It.Is<IObjectId>(p => p.Id == this.parentId),
                null,
                VersioningState.CheckedOut)).Returns(docId);

            var doc = Mock.Of<IDocument>(
                d =>
                d.Name == this.objectName &&
                d.Parents == parents &&
                d.Id == this.objectIdOld &&
                d.ChangeToken == this.changeTokenOld);
            this.remoteDocument = Mock.Get(doc);

            var docPWC = Mock.Of<IDocument>(
                d =>
                d.Name == this.objectName &&
                d.Id == this.objectIdPWC &&
                d.ChangeToken == this.changeTokenPWC);
            this.remoteDocumentPWC = Mock.Get(docPWC);

            this.remoteDocument.SetupCheckout(this.remoteDocumentPWC, this.changeTokenNew, this.objectIdNew);
            this.session.AddRemoteObject(this.remoteDocumentPWC.Object);
            this.remoteDocumentPWC.Setup(d => d.AppendContentStream(It.IsAny<IContentStream>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns<IContentStream, bool, bool>((cs, last, refresh) => {
                using (var temp = new MemoryStream()) {
                    cs.Stream.CopyTo(temp);
                }

                return remoteDocumentPWC.Object;
            });

            this.session.Setup(s => s.GetObject(It.Is<IObjectId>(o => o.Id == this.objectIdOld), It.IsAny<IOperationContext>())).Returns<IObjectId, IOperationContext>((id, context) => {
                Assert.AreEqual(id.Id, doc.Id);
                return doc;
            });
            this.session.Setup(s => s.GetObject(It.Is<IObjectId>(o => o.Id == this.objectIdOld))).Returns<IObjectId>((id) => {
                Assert.AreEqual(id.Id, doc.Id);
                return doc;
            });
            this.session.Setup(s => s.GetObject(It.Is<IObjectId>(o => o.Id == this.objectIdNew), It.IsAny<IOperationContext>())).Returns<IObjectId, IOperationContext>((id, context) => {
                Assert.AreEqual(id.Id, doc.Id);
                return doc;
            });
            this.session.Setup(s => s.GetObject(It.Is<IObjectId>(o => o.Id == this.objectIdNew))).Returns<IObjectId>((id) => {
                Assert.AreEqual(id.Id, doc.Id);
                return doc;
            });
        }

        private LocalObjectAddedWithPWC CreateSolver() {
            return new LocalObjectAddedWithPWC(
                this.session.Object,
                this.storage.Object,
                this.transmissionStorage.Object,
                this.transmissionFactory,
                this.folderOrEmptyFileAddedSolver.Object);
        }

        private void SetUpMocks(bool serverCanModifyLastModificationDate = true) {
            this.session = new Mock<ISession>();
            this.session.SetupTypeSystem(serverCanModifyLastModificationDate: serverCanModifyLastModificationDate);

            this.storage = new Mock<IMetaDataStorage>();

            this.chunkSize = 4096;
            this.transmissionStorage = new Mock<IFileTransmissionStorage>();
            this.transmissionStorage.Setup(f => f.ChunkSize).Returns(this.chunkSize);

            this.manager = new Mock<TransmissionManager>() { CallBase = true };
            this.transmissionFactory = this.manager.Object.CreateFactory();
            this.folderOrEmptyFileAddedSolver = new Mock<ISolver>(MockBehavior.Strict);
        }
    }
}