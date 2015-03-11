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

    using CmisSync.Lib.Consumer.SituationSolver;
    using CmisSync.Lib.Consumer.SituationSolver.PWC;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.FileSystem;
    using CmisSync.Lib.Storage.Database.Entities;

    using DotCMIS.Client;
    using DotCMIS.Data;

    using Moq;

    using NUnit.Framework;

    using TestUtils;

    [TestFixture]
    public class LocalObjectAddedWithPWCTest {
        private Mock<ISession> session;
        private Mock<IMetaDataStorage> storage;
        private Mock<IFileTransmissionStorage> transmissionStorage;
        private Mock<ActiveActivitiesManager> manager;

        private readonly string parentId = "parentId";
        private readonly string objectName = "objectName";
        private readonly string objectId = "objectId";
        private readonly string changeToken = "changeToken";

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
                this.manager.Object);
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
                this.manager.Object));
        }

        [Test, Category("Fast"), Category("Solver")]
        public void SolverFailsIfDirectory() {
            this.SetUpMocks();
            var undertest = new LocalObjectAddedWithPWC(
                this.session.Object,
                this.storage.Object,
                this.transmissionStorage.Object,
                this.manager.Object);
            var localFolder = new Mock<IDirectoryInfo>();

            Assert.Throws<NotSupportedException>(() => undertest.Solve(localFolder.Object, null, ContentChangeType.CREATED, ContentChangeType.NONE));
        }

        [Test, Category("Fast"), Category("Solver")]
        public void SolverFailsIfFileIsDeleted() {
            this.SetUpMocks();
            var undertest = new LocalObjectAddedWithPWC(
                this.session.Object,
                this.storage.Object,
                this.transmissionStorage.Object,
                this.manager.Object);
            var localFile = Mock.Of<IFileInfo>(f => f.Exists == false);

            Assert.Throws<FileNotFoundException>(() => undertest.Solve(localFile, null, ContentChangeType.CREATED, ContentChangeType.NONE));
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalEmptyFileAdded() {
            Assert.Fail("TODO");
        }

        [Test, Category("Fast"), Category("Solver")]
        public void Local1ByteFileAdded() {
            Assert.Fail("TODO");
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalFileIsUsedByAnotherProcessOnOpenFile() {
            this.SetUpMocks();

            this.SetupFile();
            this.localFile.Setup(f => f.Length).Returns(0);
            this.localFile.Setup(f => f.Open(It.IsAny<FileMode>())).Throws(new IOException("Alread in use by another process"));
            this.localFile.Setup(f => f.Open(It.IsAny<FileMode>(), It.IsAny<FileAccess>())).Throws(new IOException("Alread in use by another process"));
            this.localFile.Setup(f => f.Open(It.IsAny<FileMode>(), It.IsAny<FileAccess>(), It.IsAny<FileShare>())).Throws(new IOException("Alread in use by another process"));

            Assert.Fail("TODO");
        }

        private void SetupFile() {
            this.localFile = new Mock<IFileInfo>();

            this.parentPath = Path.GetTempPath();
            this.localPath = Path.Combine(this.parentPath, this.objectName);

            var parentDirInfo = Mock.Of<IDirectoryInfo>(d => d.FullName == this.parentPath && d.Name == Path.GetFileName(this.parentPath));
            this.storage.Setup(f => f.GetObjectByLocalPath(It.Is<IDirectoryInfo>(d => d.FullName == this.parentPath))).Returns(Mock.Of<IMappedObject>(o => o.RemoteObjectId == this.parentId));

            var parents = new List<IFolder>();
            parents.Add(Mock.Of<IFolder>(f => f.Id == parentId));

            var file = Mock.Of<IFileInfo>(
                f =>
                f.FullName == this.localPath &&
                f.Name == this.objectName &&
                f.Exists == true &&
                f.IsExtendedAttributeAvailable() == true &&
                f.Directory == parentDirInfo);
            this.localFile = Mock.Get(file);

            var docId = Mock.Of<IObjectId>(
                o =>
                o.Id == this.objectId);

            var doc = Mock.Of<IDocument>(
                d =>
                d.Name == this.objectName &&
                d.Id == this.objectId &&
                d.Parents == parents &&
                d.ChangeToken == this.changeToken);
            this.remoteDocument = Mock.Get(doc);

            this.session.Setup(s => s.CreateDocument(
                It.IsAny<IDictionary<string, object>>(),
                It.IsAny<IObjectId>(),
                It.IsAny<IContentStream>(),
                null,
                null,
                null,
                null)).Returns(docId);
            this.remoteDocument.Setup(d => d.LastModificationDate).Returns(new DateTime());
            this.session.Setup(s => s.GetObject(It.Is<IObjectId>(o => o.Id == docId.Id), It.IsAny<IOperationContext>())).Returns<IObjectId, IOperationContext>((id, context) => {
                return doc;
            });
            this.session.Setup(s => s.GetObject(It.Is<IObjectId>(o => o.Id == docId.Id))).Returns<IObjectId>((id) => {
                return doc;
            });
        }

        private void SetUpMocks(bool isPwcUpdateable = true) {
            this.session = new Mock<ISession>();
            this.session.SetupTypeSystem();
            this.session.SetupPrivateWorkingCopyCapability(isPwcUpdateable: isPwcUpdateable);
            this.storage = new Mock<IMetaDataStorage>();
            this.transmissionStorage = new Mock<IFileTransmissionStorage>();
            this.manager = new Mock<ActiveActivitiesManager>();
        }
    }
}