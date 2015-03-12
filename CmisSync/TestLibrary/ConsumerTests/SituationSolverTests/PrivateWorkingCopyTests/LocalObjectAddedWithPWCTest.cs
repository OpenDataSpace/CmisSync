//-----------------------------------------------------------------------
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

    using CmisSync.Lib.Cmis.ConvenienceExtenders;
    using CmisSync.Lib.Consumer.SituationSolver;
    using CmisSync.Lib.Consumer.SituationSolver.PWC;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS;
    using DotCMIS.Client;
    using DotCMIS.Data;

    using Moq;

    using NUnit.Framework;

    using TestUtils;

    [TestFixture]
    public class LocalObjectAddedWithPWCTest {
        private readonly string parentId = "parentId";
        private readonly string objectName = "objectName";
        private readonly string objectId = "objectId";
        private readonly string changeTokenOld = "changeTokenOld";
        private readonly string changeTokenNew = "changeTokenNew";
        private readonly byte[] emptyHash = SHA1.Create().ComputeHash(new byte[0]);

        private Mock<ISession> session;
        private Mock<IMetaDataStorage> storage;
        private Mock<IFileTransmissionStorage> transmissionStorage;
        private Mock<ActiveActivitiesManager> manager;
        private Mock<ISolver> folderAddedSolver;

        private string parentPath;
        private string localPath;
        private byte[] fileContent;
        private byte[] fileHash;
        private long fileLength;

        private Mock<IFileInfo> localFile;
        private Mock<IDocument> remoteDocument;

        [Test, Category("Fast"), Category("Solver")]
        public void Constructor() {
            this.SetUpMocks();
            new LocalObjectAddedWithPWC(
                this.session.Object,
                this.storage.Object,
                this.transmissionStorage.Object,
                this.manager.Object,
                Mock.Of<ISolver>());
        }

        [Test, Category("Fast"), Category("Solver")]
        public void ConstructorFailsIfSessionIsNotAbleToWorkWithPrivateWorkingCopies() {
            this.SetUpMocks(isPwcUpdateable: false);
            Assert.Throws<ArgumentException>(
                () =>
                new LocalObjectAddedWithPWC(
                this.session.Object,
                this.storage.Object,
                this.transmissionStorage.Object,
                this.manager.Object,
                Mock.Of<ISolver>()));
        }

        [Test, Category("Fast")]
        public void ConstructorFailsIfGivenSolverIsNull() {
            this.SetUpMocks();
            Assert.Throws<ArgumentNullException>(
                () =>
                new LocalObjectAddedWithPWC(
                this.session.Object,
                this.storage.Object,
                this.transmissionStorage.Object,
                this.manager.Object,
                null));
        }

        [Test, Category("Fast")]
        public void NewDirectoriesCallsArePassedToTheGivenSolver() {
            this.SetUpMocks();
            var folderSolver = new Mock<ISolver>();
            var undertest = new LocalObjectAddedWithPWC(
                this.session.Object,
                this.storage.Object,
                this.transmissionStorage.Object,
                this.manager.Object,
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

        [Test, Category("Fast"), Category("Solver"), TestCase(true, true), TestCase(false, false)]
        public void LocalEmptyFileAdded(bool withAlreadySetUuid, bool canModifyDateTimes) {
            this.SetUpMocks(true, canModifyDateTimes);

            this.SetupFile();
            this.localFile.SetupStream(new byte[0]);
            if (withAlreadySetUuid) {
                this.localFile.SetupGuid(Guid.NewGuid());
            }

            var undertest = this.CreateSolver();
            undertest.Solve(this.localFile.Object, null);

            this.session.Verify(
                s => s.CreateDocument(
                    It.Is<IDictionary<string, object>>(p => p.ContainsKey(PropertyIds.CreationDate) == canModifyDateTimes && p.ContainsKey(PropertyIds.LastModificationDate) == canModifyDateTimes),
                    It.IsAny<IObjectId>(),
                    null,
                    null,
                    null,
                    null,
                    null),
                Times.Once());
            if (withAlreadySetUuid) {
                this.localFile.VerifySet(f => f.Uuid = It.IsAny<Guid?>(), Times.Never());
            } else {
                this.localFile.VerifySet(f => f.Uuid = It.IsAny<Guid>(), Times.Once());
            }

            this.localFile.VerifyThatLocalFileObjectLastWriteTimeUtcIsNeverModified();
            this.storage.VerifySavedMappedObject(MappedObjectType.File, this.objectId, this.objectName, this.parentId, this.changeTokenOld, checksum: this.emptyHash, contentSize: 0, times: Times.Once());
            this.remoteDocument.Verify(d => d.SetContentStream(It.IsAny<IContentStream>(), It.IsAny<bool>()), Times.Never());
            this.remoteDocument.Verify(d => d.AppendContentStream(It.IsAny<IContentStream>(), It.IsAny<bool>()), Times.Never());
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalFileAdded([Values(1, 1024, 123456)]int fileSize) {
            this.SetUpMocks();

            this.SetupFile();
            byte[] content = new byte[fileSize];
            byte[] hash = SHA1.Create().ComputeHash(content);
            this.localFile.SetupStream(content);

            var undertest = this.CreateSolver();
            undertest.Solve(this.localFile.Object, null);

            this.localFile.VerifyThatLocalFileObjectLastWriteTimeUtcIsNeverModified();
            this.storage.VerifySavedMappedObject(MappedObjectType.File, this.objectId, this.objectName, this.parentId, this.changeTokenNew, checksum: hash, contentSize: fileSize, times: Times.Once());
            this.remoteDocument.Verify(d => d.SetContentStream(It.IsAny<IContentStream>(), It.IsAny<bool>()), Times.Never());
            this.remoteDocument.Verify(d => d.AppendContentStream(It.IsAny<IContentStream>(), It.IsAny<bool>()), Times.Once());
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalFileIsUsedByAnotherProcessOnOpenFile() {
            this.SetUpMocks();

            this.SetupFile();
            //this.localFile.Setup(f => f.Length).Returns(0);
            this.localFile.Setup(f => f.Open(It.IsAny<FileMode>())).Throws(new IOException("Alread in use by another process"));
            this.localFile.Setup(f => f.Open(It.IsAny<FileMode>(), It.IsAny<FileAccess>())).Throws(new IOException("Alread in use by another process"));
            this.localFile.Setup(f => f.Open(It.IsAny<FileMode>(), It.IsAny<FileAccess>(), It.IsAny<FileShare>())).Throws(new IOException("Alread in use by another process"));

            var undertest = this.CreateSolver();
            Assert.Throws<IOException>(() => undertest.Solve(this.localFile.Object, null));
        }

        private void SetupFile() {
            this.parentPath = Path.GetTempPath();
            this.localPath = Path.Combine(this.parentPath, this.objectName);

            var parentDirInfo = Mock.Of<IDirectoryInfo>(d => d.FullName == this.parentPath && d.Name == Path.GetFileName(this.parentPath));
            this.storage.Setup(f => f.GetObjectByLocalPath(It.Is<IDirectoryInfo>(d => d.FullName == this.parentPath))).Returns(Mock.Of<IMappedObject>(o => o.RemoteObjectId == this.parentId));

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

            var doc = Mock.Of<IDocument>(
                d =>
                d.Name == this.objectName &&
                d.Id == this.objectId &&
                d.Parents == parents &&
                d.ChangeToken == this.changeTokenOld);
            this.remoteDocument = Mock.Get(doc);

            var docId = Mock.Of<IObjectId>(
                o =>
                o.Id == this.objectId);

            this.session.Setup(s => s.CreateDocument(
                It.IsAny<IDictionary<string, object>>(),
                It.IsAny<IObjectId>(),
                It.IsAny<IContentStream>(),
                null,
                null,
                null,
                null)).Returns(docId);

            //this.remoteDocument.Setup(d => d.LastModificationDate).Returns(new DateTime());
            //this.session.Setup(s => s.GetObject(It.Is<IObjectId>(o => o.Id == docId.Id), It.IsAny<IOperationContext>())).Returns<IObjectId, IOperationContext>((id, context) => {
            //    return doc;
            //});
            //this.session.Setup(s => s.GetObject(It.Is<IObjectId>(o => o.Id == docId.Id))).Returns<IObjectId>((id) => {
            //    return doc;
            //});
        }

        private LocalObjectAddedWithPWC CreateSolver() {
            return new LocalObjectAddedWithPWC(
                this.session.Object,
                this.storage.Object,
                this.transmissionStorage.Object,
                this.manager.Object,
                this.folderAddedSolver.Object);
        }

        private void SetUpMocks(bool isPwcUpdateable = true, bool isServerCanModifyDateTimes = true) {
            this.session = new Mock<ISession>();
            this.session.SetupTypeSystem(isServerCanModifyDateTimes);
            this.session.SetupPrivateWorkingCopyCapability(isPwcUpdateable: isPwcUpdateable);
            this.storage = new Mock<IMetaDataStorage>();
            this.transmissionStorage = new Mock<IFileTransmissionStorage>();
            this.manager = new Mock<ActiveActivitiesManager>();
            this.folderAddedSolver = new Mock<ISolver>(MockBehavior.Strict);
        }
    }
}