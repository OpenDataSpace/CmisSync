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
    using System.IO;
    using System.Security.Cryptography;

    using CmisSync.Lib.Cmis;
    using CmisSync.Lib.Consumer;
    using CmisSync.Lib.Consumer.SituationSolver;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Exceptions;
    using CmisSync.Lib.FileTransmission;
    using CmisSync.Lib.Producer.Watcher;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;
    using DotCMIS.Data;
    using DotCMIS.Enums;
    using DotCMIS.Exceptions;

    using Moq;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture, Category("Fast"), Category("Solver")]
    public class LocalObjectAddedTest : IsTestWithConfiguredLog4Net {
        private readonly string parentId = "parentId";
        private readonly string localObjectName = "localName";
        private readonly string remoteObjectId = "remoteId";
        private readonly string remotePWCObjectId = "remotePWCId";
        private readonly string lastChangeToken = "token";
        private readonly string lastPWCChangeToken = "tokenPWC";

        private Mock<ISession> session;
        private Mock<IMetaDataStorage> storage;
        private Mock<IFileTransmissionStorage> transmissionStorage;
        private ITransmissionFactory transmissionFactory;
        private TransmissionManager transmissionManager;
        private bool withExtendedAttributes;
        private byte[] emptyhash = SHA1.Create().ComputeHash(new byte[0]);

        [Test]
        public void ConstructorWithGivenQueueAndActivityManager() {
            this.SetUpMocks();
            new LocalObjectAdded(this.session.Object, this.storage.Object, this.transmissionStorage.Object, this.transmissionFactory);
        }

        [Test]
        public void ConstructorThrowsExceptionIfTransmissionManagerIsNull() {
            this.SetUpMocks();
            Assert.Throws<ArgumentNullException>(() => new LocalObjectAdded(this.session.Object, this.storage.Object, this.transmissionStorage.Object, null));
        }

        [Test]
        public void Local1ByteFileAdded(
            [Values(true, false)]bool withExtendedAttributes)
        {
            this.SetUpMocks(withExtendedAttributes);
            Mock<IFileInfo> fileInfo = new Mock<IFileInfo>();
            fileInfo.Setup(f => f.Length).Returns(1);
            var fileContent = new byte[1];
            using (var localFileStream = new MemoryStream(fileContent)) {
                byte[] hash = SHA1Managed.Create().ComputeHash(fileContent);

                fileInfo.Setup(f => f.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete)).Returns(localFileStream);

                Mock<IDocument> document;
                this.SetupSolveFile(this.localObjectName, this.remoteObjectId, this.parentId, this.lastChangeToken, this.withExtendedAttributes, fileInfo, out document);
                this.RunSolveFile(fileInfo);
                this.storage.VerifySavedMappedObject(MappedObjectType.File, this.remoteObjectId, this.localObjectName, this.parentId, this.lastChangeToken, Times.Exactly(2), this.withExtendedAttributes, null, null, hash, 1);
                this.VerifyCreateDocument();
                if (this.withExtendedAttributes) {
                    fileInfo.VerifySet(f => f.Uuid = It.Is<Guid?>(uuid => uuid != null), Times.Once());
                } else {
                    fileInfo.VerifySet(f => f.Uuid = It.IsAny<Guid?>(), Times.Never());
                }

                fileInfo.VerifyThatLocalFileObjectLastWriteTimeUtcIsNeverModified();
                document.Verify(d => d.SetContentStream(It.IsAny<IContentStream>(), true, true), Times.Once());
                document.VerifyUpdateLastModificationDate(fileInfo.Object.LastWriteTimeUtc, true);
            }
        }

        [Test, TestCase(true, true), TestCase(true, false), TestCase(false, false)]
        public void LocalEmptyFileAdded(bool withExtendedAttributes, bool withAlreadySetUuid) {
            this.SetUpMocks(withExtendedAttributes);
            Guid uuid = Guid.NewGuid();
            Mock<IFileInfo> fileInfo = new Mock<IFileInfo>();
            fileInfo.Setup(f => f.Length).Returns(0);
            if (withAlreadySetUuid) {
                fileInfo.SetupGuid(uuid);
            }

            Mock<IDocument> document;

            this.SetupSolveFile(this.localObjectName, this.remoteObjectId, this.parentId, this.lastChangeToken, this.withExtendedAttributes, fileInfo, out document);
            this.RunSolveFile(fileInfo);

            this.storage.VerifySavedMappedObject(MappedObjectType.File, this.remoteObjectId, this.localObjectName, this.parentId, this.lastChangeToken, this.withExtendedAttributes, checksum: this.emptyhash, contentSize: 0);
            if (withAlreadySetUuid) {
                this.storage.Verify(s => s.SaveMappedObject(It.Is<IMappedObject>(o => o.Guid == uuid)));
            }

            this.VerifyCreateDocument(isEmpty: true);
            if (this.withExtendedAttributes && !withAlreadySetUuid) {
                fileInfo.VerifySet(f => f.Uuid = It.Is<Guid?>(u => u != null), Times.Once());
            } else {
                fileInfo.VerifySet(f => f.Uuid = It.IsAny<Guid?>(), Times.Never());
            }

            fileInfo.VerifyThatLocalFileObjectLastWriteTimeUtcIsNeverModified();
            document.Verify(d => d.AppendContentStream(It.IsAny<IContentStream>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Never());
        }

        [Test]
        public void LocalFolderAdded([Values(true, false)]bool withExtendedAttributes) {
            this.SetUpMocks(withExtendedAttributes);

            Mock<IFolder> futureFolder;

            var dirInfo = this.RunSolveFolder(this.localObjectName, this.remoteObjectId, this.parentId, this.lastChangeToken, this.withExtendedAttributes, out futureFolder);

            this.storage.VerifySavedMappedObject(MappedObjectType.Folder, this.remoteObjectId, this.localObjectName, this.parentId, this.lastChangeToken, this.withExtendedAttributes);
            this.session.Verify(s => s.CreateFolder(It.Is<IDictionary<string, object>>(p => p.ContainsKey("cmis:name")), It.Is<IObjectId>(o => o.Id == this.parentId)), Times.Once());
            dirInfo.VerifyThatLocalFileObjectLastWriteTimeUtcIsNeverModified();
            if (this.withExtendedAttributes) {
                dirInfo.VerifySet(d => d.Uuid = It.Is<Guid?>(uuid => !uuid.Equals(Guid.Empty)), Times.Once());
            } else {
                dirInfo.VerifySet(d => d.Uuid = It.IsAny<Guid?>(), Times.Never());
            }
        }

        [Test]
        public void LocalFolderAddingFailsBecauseOfAConflict() {
            this.SetUpMocks();
            var solver = new LocalObjectAdded(this.session.Object, this.storage.Object, this.transmissionStorage.Object, this.transmissionFactory);
            var dirInfo = new Mock<IDirectoryInfo>();
            dirInfo.Setup(d => d.Exists).Returns(true);
            dirInfo.Setup(d => d.Name).Returns("dir");
            var parentDirInfo = this.SetupParentFolder(this.parentId);
            dirInfo.Setup(d => d.Parent).Returns(parentDirInfo);
            this.session.Setup(s => s.CreateFolder(It.IsAny<IDictionary<string, object>>(), It.IsAny<IObjectId>())).Throws(new CmisConstraintException("Conflict"));
            Assert.Throws<CmisConstraintException>(() => solver.Solve(dirInfo.Object, null));

            this.storage.VerifyThatNoObjectIsManipulated();
            this.session.Verify(s => s.CreateFolder(It.Is<IDictionary<string, object>>(p => p.ContainsKey("cmis:name")), It.Is<IObjectId>(o => o.Id == this.parentId)), Times.Once());
            dirInfo.VerifyThatLocalFileObjectLastWriteTimeUtcIsNeverModified();
            dirInfo.VerifySet(d => d.Uuid = It.IsAny<Guid?>(), Times.Never());
        }

        [Test]
        public void LocalFolderAddingFailsBecauseUtf8Character() {
            this.SetUpMocks();
            var solver = new LocalObjectAdded(this.session.Object, this.storage.Object, this.transmissionStorage.Object, this.transmissionFactory);
            var dirInfo = new Mock<IDirectoryInfo>();
            dirInfo.Setup(d => d.Exists).Returns(true);
            dirInfo.Setup(d => d.Name).Returns(@"ä".Normalize(System.Text.NormalizationForm.FormD));
            var parentDirInfo = this.SetupParentFolder(this.parentId);
            dirInfo.Setup(d => d.Parent).Returns(parentDirInfo);
            this.session.Setup(s => s.CreateFolder(It.IsAny<IDictionary<string, object>>(), It.IsAny<IObjectId>())).Throws(new CmisConstraintException("Conflict"));
            Assert.Throws<InteractionNeededException>(() => solver.Solve(dirInfo.Object, null));

            this.storage.VerifyThatNoObjectIsManipulated();
            this.session.Verify(s => s.CreateFolder(It.Is<IDictionary<string, object>>(p => p.ContainsKey("cmis:name")), It.Is<IObjectId>(o => o.Id == this.parentId)), Times.Once());
            dirInfo.VerifyThatLocalFileObjectLastWriteTimeUtcIsNeverModified();
            dirInfo.VerifySet(d => d.Uuid = It.IsAny<Guid?>(), Times.Never());
        }

        [Test]
        public void LocalFolderAddedWithAlreadyExistingGuid() {
            this.SetUpMocks(true);
            Mock<IFolder> futureFolder;
            Guid guid = Guid.NewGuid();
            this.storage.AddLocalFile("unimportant", "unimportant", guid);

            var dirInfo = this.RunSolveFolder(this.localObjectName, this.remoteObjectId, this.parentId, this.lastChangeToken, this.withExtendedAttributes, out futureFolder, guid);

            this.storage.VerifySavedMappedObject(MappedObjectType.Folder, this.remoteObjectId, this.localObjectName, this.parentId, this.lastChangeToken, this.withExtendedAttributes);
            this.session.Verify(s => s.CreateFolder(It.Is<IDictionary<string, object>>(p => p.ContainsKey("cmis:name")), It.Is<IObjectId>(o => o.Id == this.parentId)), Times.Once());
            dirInfo.VerifyThatLocalFileObjectLastWriteTimeUtcIsNeverModified();
            dirInfo.VerifySet(d => d.Uuid = It.Is<Guid?>(uuid => !uuid.Equals(guid)), Times.Once());
        }

        [Test]
        public void LocalFileIsUsedByAnotherProcess() {
            this.SetUpMocks(true);
            var exception = new ExtendedAttributeException();

            Mock<IFileInfo> fileInfo = new Mock<IFileInfo>();
            fileInfo.Setup(f => f.Length).Returns(0);
            fileInfo.SetupSet(f => f.Uuid = It.IsAny<Guid?>()).Throws(exception);
            Mock<IDocument> document;
            this.SetupSolveFile(this.localObjectName, this.remoteObjectId, this.parentId, this.lastChangeToken, this.withExtendedAttributes, fileInfo, out document);

            var e = Assert.Throws<RetryException>(() => this.RunSolveFile(fileInfo));

            Assert.That(e.InnerException, Is.EqualTo(exception));
            fileInfo.VerifySet(f => f.Uuid = It.Is<Guid?>(uuid => uuid != null), Times.Once());
            this.storage.Verify(s => s.SaveMappedObject(It.IsAny<IMappedObject>()), Times.Never());
            fileInfo.VerifyThatLocalFileObjectLastWriteTimeUtcIsNeverModified();
        }

        [Test]
        public void LocalFileIsUsedByAnotherProcessOnOpenFile() {
            this.SetUpMocks();
            Mock<IFileInfo> fileInfo = new Mock<IFileInfo>();
            fileInfo.Setup(f => f.Length).Returns(10);
            fileInfo.SetupOpenThrows(new IOException("Already in use by another process"));

            Mock<IDocument> document;
            Assert.Throws<IOException>(() => {
                this.SetupSolveFile(this.localObjectName, this.remoteObjectId, this.parentId, lastChangeToken, true, fileInfo, out document);
                this.RunSolveFile(fileInfo);
            });
            fileInfo.VerifySet(f => f.Uuid = It.Is<Guid?>(uuid => uuid != null), Times.Once());
            this.storage.VerifySavedMappedObject(MappedObjectType.File, this.remoteObjectId, this.localObjectName, this.parentId, this.lastChangeToken, true, checksum: this.emptyhash, contentSize: 0);
            this.VerifyCreateDocument(isEmpty: false);
            fileInfo.VerifyThatLocalFileObjectLastWriteTimeUtcIsNeverModified();
        }

        [Test]
        public void DoNotWriteLastWriteTimeUtcIfNotNecessary() {
            this.SetUpMocks(true);

            Mock<IFileInfo> fileInfo = new Mock<IFileInfo>();
            fileInfo.Setup(f => f.Length).Returns(0);

            Mock<IDocument> document;
            this.SetupSolveFile(this.localObjectName, this.remoteObjectId, this.parentId, this.lastChangeToken, this.withExtendedAttributes, fileInfo, out document, false);
            this.RunSolveFile(fileInfo);

            fileInfo.VerifyThatLocalFileObjectLastWriteTimeUtcIsNeverModified();
        }

        [Test]
        public void PermissionDeniedLeadsToNoOperation() {
            this.SetUpMocks(true);

            string path = Path.Combine(Path.GetTempPath(), this.localObjectName);
            this.session.Setup(s => s.CreateDocument(
                It.Is<IDictionary<string, object>>(p => (string)p["cmis:name"] == this.localObjectName),
                It.Is<IObjectId>(o => o.Id == this.parentId),
                It.IsAny<IContentStream>(),
                null,
                null,
                null,
                null)).Throws(new CmisPermissionDeniedException());

            Mock<IFileInfo> fileInfo = new Mock<IFileInfo>();
            fileInfo.Setup(f => f.Length).Returns(0);

            var parentDirInfo = this.SetupParentFolder(this.parentId);

            var parents = new List<IFolder>();
            parents.Add(Mock.Of<IFolder>(f => f.Id == this.parentId));
            fileInfo.Setup(d => d.FullName).Returns(path);
            fileInfo.Setup(d => d.Name).Returns(this.localObjectName);
            fileInfo.Setup(d => d.Exists).Returns(true);
            fileInfo.Setup(d => d.IsExtendedAttributeAvailable()).Returns(this.withExtendedAttributes);

            fileInfo.Setup(d => d.Directory).Returns(parentDirInfo);
            var solver = new LocalObjectAdded(this.session.Object, this.storage.Object, this.transmissionStorage.Object, this.transmissionFactory);
            solver.Solve(fileInfo.Object, null);
            this.storage.Verify(s => s.SaveMappedObject(It.IsAny<IMappedObject>()), Times.Never());
        }

        [Test]
        public void ParentFolderDoesNotExistsOnServerDueToMissingAllowedActions() {
            this.SetUpMocks(true);

            string path = Path.Combine(Path.GetTempPath(), this.localObjectName);
            this.session.Setup(s => s.CreateDocument(
                It.Is<IDictionary<string, object>>(p => (string)p["cmis:name"] == this.localObjectName),
                It.Is<IObjectId>(o => o.Id == this.parentId),
                It.IsAny<IContentStream>(),
                null,
                null,
                null,
                null)).Throws(new CmisPermissionDeniedException());

            Mock<IFileInfo> fileInfo = new Mock<IFileInfo>();
            fileInfo.Setup(f => f.Length).Returns(0);

            var parentDirInfo = new Mock<IDirectoryInfo>();
            var parentsParentDirInfo = new Mock<IDirectoryInfo>();
            Guid parentsParentGuid = Guid.NewGuid();
            string remoteParentsParentId = Guid.NewGuid().ToString();
            parentsParentDirInfo.Setup(d => d.Uuid).Returns(parentsParentGuid);
            parentDirInfo.Setup(d => d.Parent).Returns(parentsParentDirInfo.Object);
            parentDirInfo.Setup(d => d.Exists).Returns(true);
            parentsParentDirInfo.Setup(d => d.Exists).Returns(true);
            var mappedFolder = Mock.Of<IMappedObject>(o => o.Guid == parentsParentGuid && o.RemoteObjectId == remoteParentsParentId);
            var remoteParentsParentFolder = Mock.Of<IFolder>(f => f.Id == remoteParentsParentId);
            Mock.Get(remoteParentsParentFolder).SetupReadOnly();
            this.session.AddRemoteObject(remoteParentsParentFolder);
            this.storage.Setup(s => s.GetObjectByLocalPath(parentsParentDirInfo.Object)).Returns(mappedFolder);

            fileInfo.Setup(d => d.FullName).Returns(path);
            fileInfo.Setup(d => d.Name).Returns(this.localObjectName);
            fileInfo.Setup(d => d.Exists).Returns(true);
            fileInfo.Setup(d => d.IsExtendedAttributeAvailable()).Returns(this.withExtendedAttributes);

            fileInfo.Setup(d => d.Directory).Returns(parentDirInfo.Object);
            var solver = new LocalObjectAdded(this.session.Object, this.storage.Object, this.transmissionStorage.Object, this.transmissionFactory);
            solver.Solve(fileInfo.Object, null);
            this.storage.VerifyThatNoObjectIsManipulated();
        }

        [Test]
        public void ParentFolderDoesNotExistsOnServerDueToPastErrorThrowsException() {
            this.SetUpMocks(true);

            string path = Path.Combine(Path.GetTempPath(), this.localObjectName);
            this.session.Setup(s => s.CreateDocument(
                It.Is<IDictionary<string, object>>(p => (string)p["cmis:name"] == this.localObjectName),
                It.Is<IObjectId>(o => o.Id == this.parentId),
                It.IsAny<IContentStream>(),
                null,
                null,
                null,
                null)).Throws(new CmisPermissionDeniedException());

            Mock<IFileInfo> fileInfo = new Mock<IFileInfo>();
            fileInfo.Setup(f => f.Length).Returns(0);

            var parentDirInfo = new Mock<IDirectoryInfo>();

            fileInfo.Setup(d => d.FullName).Returns(path);
            fileInfo.Setup(d => d.Name).Returns(this.localObjectName);
            fileInfo.Setup(d => d.Exists).Returns(true);
            fileInfo.Setup(d => d.IsExtendedAttributeAvailable()).Returns(this.withExtendedAttributes);

            fileInfo.Setup(d => d.Directory).Returns(parentDirInfo.Object);
            var transmissionManager = new TransmissionManager();
            var solver = new LocalObjectAdded(this.session.Object, this.storage.Object, this.transmissionStorage.Object, this.transmissionFactory);
            Assert.Throws<ArgumentException>(() => solver.Solve(fileInfo.Object, null));
            this.storage.VerifyThatNoObjectIsManipulated();
        }

        [Test]
        public void StorageExceptionOnUploadLeadsToSavedEmptyState() {
            this.SetUpMocks();

            Mock<IFileInfo> fileInfo = new Mock<IFileInfo>();
            fileInfo.Setup(f => f.Length).Returns(1);
            var fileContent = new byte[1];
            var localFileStream = new MemoryStream(fileContent);

            fileInfo.Setup(f => f.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete)).Returns(localFileStream);

            Mock<IDocument> document;
            this.SetupSolveFile(this.localObjectName, this.remoteObjectId, this.parentId, this.lastChangeToken, this.withExtendedAttributes, fileInfo, out document, failsOnUploadContent: true);
            this.RunSolveFile(fileInfo);
            this.storage.VerifySavedMappedObject(MappedObjectType.File, this.remoteObjectId, this.localObjectName, this.parentId, this.lastChangeToken, this.withExtendedAttributes, checksum: this.emptyhash, contentSize: 0);
            this.VerifyCreateDocument();
            fileInfo.VerifySet(f => f.Uuid = It.Is<Guid?>(uuid => uuid != null), Times.Once());
            fileInfo.VerifyThatLocalFileObjectLastWriteTimeUtcIsNeverModified();
            document.Verify(d => d.SetContentStream(It.IsAny<IContentStream>(), true, true), Times.Once());
            document.Verify(d => d.UpdateProperties(It.IsAny<IDictionary<string, object>>()), Times.Never());
        }

        [Test]
        public void SolverFailsIfLocalFileOrFolderDoesNotExistsAnymore() {
            this.SetUpMocks();
            string path = Path.Combine(Path.GetTempPath(), this.localObjectName);

            var fileSystemInfo = new Mock<IFileSystemInfo>(MockBehavior.Strict);
            fileSystemInfo.Setup(f => f.Refresh());
            fileSystemInfo.Setup(f => f.Exists).Returns(false);
            fileSystemInfo.Setup(f => f.FullName).Returns(path);
            var solver = new LocalObjectAdded(this.session.Object, this.storage.Object, Mock.Of<IFileTransmissionStorage>(), this.transmissionFactory);

            Assert.Throws<FileNotFoundException>(() => solver.Solve(fileSystemInfo.Object, null));
        }

        private IDirectoryInfo SetupParentFolder(string parentId) {
            var parentDirInfo = Mock.Of<IDirectoryInfo>(
                d =>
                d.FullName == Path.GetTempPath() &&
                d.Name == Path.GetFileName(Path.GetTempPath()));

            this.storage.Setup(
                s =>
                s.GetObjectByLocalPath(It.Is<IDirectoryInfo>(d => d.FullName == Path.GetTempPath()))).Returns(
                Mock.Of<IMappedObject>(
                o =>
                o.RemoteObjectId == parentId));
            return parentDirInfo;
        }

        private void SetupSolveFile(
            string fileName,
            string fileId,
            string parentId,
            string lastChangeToken,
            bool extendedAttributes,
            Mock<IFileInfo> fileInfo,
            out Mock<IDocument> documentMock,
            bool returnLastModificationDate = false,
            bool failsOnUploadContent = false)
        {
            var parentDirInfo = this.SetupParentFolder(parentId);
            var parents = new List<IFolder>();
            parents.Add(Mock.Of<IFolder>(f => f.Id == parentId));

            string path = Path.Combine(Path.GetTempPath(), fileName);
            var futureRemoteDoc = Mock.Of<IDocument>(
                f =>
                f.Name == fileName &&
                f.Id == fileId &&
                f.Parents == parents &&
                f.ChangeToken == lastChangeToken);
            var futureRemoteDocId = Mock.Of<IObjectId>(
                o =>
                o.Id == fileId);

            this.session.Setup(s => s.CreateDocument(
                It.Is<IDictionary<string, object>>(p => (string)p["cmis:name"] == fileName),
                It.Is<IObjectId>(o => o.Id == parentId),
                It.Is<IContentStream>(stream => this.SetupFutureRemoteDocStream(Mock.Get(futureRemoteDoc), stream)),
                null,
                null,
                null,
                null)).Returns(futureRemoteDocId);
            this.session.Setup(s => s.GetObject(It.Is<IObjectId>(o => o == futureRemoteDocId), It.IsAny<IOperationContext>())).Returns(futureRemoteDoc);
            Mock.Get(futureRemoteDoc).Setup(
                doc =>
                doc.SetContentStream(It.IsAny<IContentStream>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .Callback<IContentStream, bool, bool>((s, o, r) => {
                    if (failsOnUploadContent) {
                        throw new CmisStorageException("StorageException");
                    } else {
                        using (var temp = new MemoryStream()) {
                            s.Stream.CopyTo(temp);
                        }
                    }
                });
            if (returnLastModificationDate) {
                Mock.Get(futureRemoteDoc).Setup(doc => doc.LastModificationDate).Returns(new DateTime());
            }

            fileInfo.Setup(d => d.FullName).Returns(path);
            fileInfo.Setup(d => d.Name).Returns(fileName);
            fileInfo.Setup(d => d.Exists).Returns(true);
            fileInfo.Setup(d => d.IsExtendedAttributeAvailable()).Returns(extendedAttributes);
            fileInfo.Setup(d => d.Directory).Returns(parentDirInfo);

            documentMock = Mock.Get(futureRemoteDoc);
        }

        private void RunSolveFile(Mock<IFileInfo> fileInfo) {
            var solver = new LocalObjectAdded(this.session.Object, this.storage.Object, this.transmissionStorage.Object, this.transmissionFactory);

            solver.Solve(fileInfo.Object, null);
            Assert.That(this.transmissionManager.ActiveTransmissions, Is.Empty);
        }

        private void RunSolveFileChanged(Mock<IFileInfo> fileInfo, Mock<IDocument> document) {
            var solver = new LocalObjectChanged(this.session.Object, this.storage.Object, this.transmissionStorage.Object, this.transmissionFactory);

            solver.Solve(fileInfo.Object, document.Object);
            Assert.That(this.transmissionManager.ActiveTransmissions, Is.Empty);
        }

        private bool SetupFutureRemoteDocStream(Mock<IDocument> doc, IContentStream stream) {
            if (stream == null) {
                return true;
            } else {
                doc.Setup(d => d.ContentStreamLength).Returns(stream.Length);
                doc.Setup(d => d.ContentStreamFileName).Returns(stream.FileName);
                doc.Setup(d => d.ContentStreamMimeType).Returns(stream.MimeType);
                return true;
            }
        }

        private Mock<IDirectoryInfo> RunSolveFolder(
            string folderName,
            string id,
            string parentId,
            string lastChangeToken,
            bool extendedAttributes,
            out Mock<IFolder> folderMock,
            Guid? existingGuid = null,
                TransmissionManager transmissionManager = null)
        {
            string path = Path.Combine(Path.GetTempPath(), folderName);
            var futureRemoteFolder = Mock.Of<IFolder>(
                f =>
                f.Name == folderName &&
                f.Id == id &&
                f.ParentId == parentId &&
                f.ChangeToken == lastChangeToken);
            var futureRemoteFolderId = Mock.Of<IObjectId>(
                o =>
                o.Id == id);

            this.session.Setup(s => s.CreateFolder(It.Is<IDictionary<string, object>>(p => (string)p["cmis:name"] == folderName), It.Is<IObjectId>(o => o.Id == parentId))).Returns(futureRemoteFolderId);
            this.session.Setup(s => s.GetObject(It.Is<IObjectId>(o => o == futureRemoteFolderId), It.IsAny<IOperationContext>())).Returns(futureRemoteFolder);

            var dirInfo = new Mock<IDirectoryInfo>();
            dirInfo.Setup(d => d.FullName).Returns(path);
            dirInfo.Setup(d => d.Name).Returns(folderName);
            dirInfo.Setup(d => d.Exists).Returns(true);
            dirInfo.Setup(d => d.IsExtendedAttributeAvailable()).Returns(extendedAttributes);
            if (existingGuid != null) {
                dirInfo.SetupGuid((Guid)existingGuid);
            }

            var parentDirInfo = this.SetupParentFolder(parentId);
            dirInfo.Setup(d => d.Parent).Returns(parentDirInfo);
            if (transmissionManager == null) {
                transmissionManager = new TransmissionManager();
            }

            var solver = new LocalObjectAdded(this.session.Object, this.storage.Object, this.transmissionStorage.Object, this.transmissionFactory);

            solver.Solve(dirInfo.Object, null);

            folderMock = Mock.Get(futureRemoteFolder);
            return dirInfo;
        }

        private bool VerifyEmptyStream(IContentStream stream) {
            Assert.That(stream, Is.Not.Null);
            Assert.That(stream.Length, Is.EqualTo(0));
            Assert.That(string.IsNullOrEmpty(stream.MimeType), Is.False);
            return true;
        }

        private void VerifyCreateDocument(bool isEmpty = false) {
            if (isEmpty) {
                this.session.Verify(
                    s =>
                    s.CreateDocument(
                    It.Is<IDictionary<string, object>>(p => (string)p["cmis:name"] == this.localObjectName),
                    It.Is<IObjectId>(o => o.Id == this.parentId),
                    It.Is<IContentStream>(st => this.VerifyEmptyStream(st)),
                    null,
                    null,
                    null,
                    null),
                    Times.Once());
            } else {
                this.session.Verify(
                    s =>
                    s.CreateDocument(
                    It.Is<IDictionary<string, object>>(p => (string)p["cmis:name"] == this.localObjectName),
                    It.Is<IObjectId>(o => o.Id == this.parentId),
                    It.Is<IContentStream>(st => st == null),
                    null,
                    null,
                    null,
                    null),
                    Times.Once());
            }
        }

        private void SetUpMocks(bool withExtendedAttributes = true) {
            this.withExtendedAttributes = withExtendedAttributes;
            this.session = new Mock<ISession>();
            this.session.SetupTypeSystem();
            this.storage = new Mock<IMetaDataStorage>();
            this.storage.Setup(f => f.SaveMappedObject(It.IsAny<IMappedObject>())).Callback<IMappedObject>((o) => {
                this.storage.Setup(f => f.GetObjectByLocalPath(It.IsAny<IFileSystemInfo>())).Returns(o);
            });
            this.transmissionStorage = new Mock<IFileTransmissionStorage>();
            this.transmissionStorage.Setup(f => f.SaveObject(It.IsAny<IFileTransmissionObject>())).Callback<IFileTransmissionObject>((o) => {
                this.transmissionStorage.Setup(f => f.GetObjectByRemoteObjectId(It.IsAny<string>())).Returns(o);
            });
            this.transmissionManager = new TransmissionManager();
            this.transmissionFactory = this.transmissionManager.CreateFactory();

            this.session.SetupCreateOperationContext();
        }
    }
}