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

namespace TestLibrary.ConsumerTests.SituationSolverTests
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Security.Cryptography;

    using CmisSync.Lib.Consumer;
    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.FileSystem;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Consumer.SituationSolver;
    using CmisSync.Lib.Producer.Watcher;

    using DotCMIS.Client;
    using DotCMIS.Data;
    using DotCMIS.Enums;

    using Moq;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture]
    public class LocalObjectAddedTest : IsTestWithConfiguredLog4Net
    {
        private Mock<ISession> session;
        private Mock<IMetaDataStorage> storage;
        private Mock<ISyncEventQueue> queue;

        [SetUp]
        public void SetUp()
        {
            this.session = new Mock<ISession>();
            this.storage = new Mock<IMetaDataStorage>();
            this.queue = new Mock<ISyncEventQueue>();
            this.session.SetupCreateOperationContext();
        }

        [Test, Category("Fast"), Category("Solver")]
        public void ConstructorWithGivenQueueAndActivityManager()
        {
            new LocalObjectAdded(Mock.Of<ISyncEventQueue>(), new ActiveActivitiesManager());
        }

        [Test, Category("Fast"), Category("Solver")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorThrowsExceptionIfQueueIsNull()
        {
            new LocalObjectAdded(null, new ActiveActivitiesManager());
        }

        [Test, Category("Fast"), Category("Solver")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorThrowsExceptionIfTransmissionManagerIsNull()
        {
            new LocalObjectAdded(Mock.Of<ISyncEventQueue>(), null);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void Local1ByteFileAddedWithoutExtAttr() {
            string fileName = "fileName";
            string fileId = "fileId";
            string parentId = "parentId";
            string lastChangeToken = "token";
            bool extendedAttributes = false;

            Mock<IFileInfo> fileInfo = new Mock<IFileInfo>();
            fileInfo.Setup(f => f.Length).Returns(1);
            var fileContent = new byte[1];
            var localFileStream = new MemoryStream(fileContent);
            byte[] hash = SHA1Managed.Create().ComputeHash(fileContent);

            fileInfo.Setup(f => f.Open(FileMode.Open, FileAccess.Read)).Returns(localFileStream);

            Mock<IDocument> document;
            this.RunSolveFile(fileName, fileId, parentId, lastChangeToken, extendedAttributes, fileInfo, out document);
            this.storage.VerifySavedMappedObject(MappedObjectType.File, fileId, fileName, parentId, lastChangeToken, Times.Exactly(2), extendedAttributes, null, hash, 1);
            this.session.Verify(
                s => s.CreateDocument(
                    It.Is<IDictionary<string, object>>(p => p.ContainsKey("cmis:name")),
                    It.Is<IObjectId>(o => o.Id == parentId),
                    It.Is<IContentStream>(st => st == null),
                    null,
                    null,
                    null,
                    null),
                Times.Once());
            fileInfo.Verify(d => d.SetExtendedAttribute(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
            document.Verify(d => d.SetContentStream(It.IsAny<IContentStream>(), true, true), Times.Once());
            this.queue.Verify(q => q.AddEvent(It.IsAny<FileTransmissionEvent>()), Times.Once());
        }

        [Test, Category("Fast"), Category("Solver")]
        public void Local1ByteFileAddedWithExtAttr() {
            string fileName = "fileName";
            string fileId = "fileId";
            string parentId = "parentId";
            string lastChangeToken = "token";
            bool extendedAttributes = true;

            Mock<IFileInfo> fileInfo = new Mock<IFileInfo>();
            fileInfo.Setup(f => f.Length).Returns(1);
            var fileContent = new byte[1];
            var localFileStream = new MemoryStream(fileContent);
            byte[] hash = SHA1Managed.Create().ComputeHash(fileContent);

            fileInfo.Setup(f => f.Open(FileMode.Open, FileAccess.Read)).Returns(localFileStream);

            Mock<IDocument> document;
            this.RunSolveFile(fileName, fileId, parentId, lastChangeToken, extendedAttributes, fileInfo, out document);
            this.storage.VerifySavedMappedObject(MappedObjectType.File, fileId, fileName, parentId, lastChangeToken, Times.Exactly(2), extendedAttributes, null, hash, 1);
            this.session.Verify(
                s => s.CreateDocument(
                It.Is<IDictionary<string, object>>(p => p.ContainsKey("cmis:name")),
                It.Is<IObjectId>(o => o.Id == parentId),
                It.Is<IContentStream>(st => st == null),
                null,
                null,
                null,
                null),
                Times.Once());
            fileInfo.Verify(d => d.SetExtendedAttribute(It.IsAny<string>(), It.IsAny<string>()), Times.Once());
            document.Verify(d => d.SetContentStream(It.IsAny<IContentStream>(), true, true), Times.Once());
            this.queue.Verify(q => q.AddEvent(It.IsAny<FileTransmissionEvent>()), Times.Once());
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalEmptyFileAddedWithoutExtAttr() {
            string fileName = "fileName";
            string fileId = "fileId";
            string parentId = "parentId";
            string lastChangeToken = "token";
            bool extendedAttributes = false;

            Mock<IFileInfo> fileInfo = new Mock<IFileInfo>();
            fileInfo.Setup(f => f.Length).Returns(0);


            Mock<IDocument> document;
            this.RunSolveFile(fileName, fileId, parentId, lastChangeToken, extendedAttributes, fileInfo, out document);
            this.storage.VerifySavedMappedObject(MappedObjectType.File, fileId, fileName, parentId, lastChangeToken, extendedAttributes, contentSize: 0);
            this.session.Verify(
                s => s.CreateDocument(
                    It.Is<IDictionary<string, object>>(p => p.ContainsKey("cmis:name")),
                    It.Is<IObjectId>(o => o.Id == parentId),
                    It.Is<IContentStream>(st => st == null),
                    null,
                    null,
                    null,
                    null),
                Times.Once());
            fileInfo.Verify(d => d.SetExtendedAttribute(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
            document.Verify(d => d.AppendContentStream(It.IsAny<IContentStream>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Never());
            this.queue.Verify(q => q.AddEvent(It.IsAny<FileTransmissionEvent>()), Times.Once());
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalEmptyFileAddedWithExtAttr() {
            string fileName = "fileName";
            string fileId = "fileId";
            string parentId = "parentId";
            string lastChangeToken = "token";
            bool extendedAttributes = true;

            Mock<IFileInfo> fileInfo = new Mock<IFileInfo>();
            fileInfo.Setup(f => f.Length).Returns(0);

            Mock<IDocument> document;
            this.RunSolveFile(fileName, fileId, parentId, lastChangeToken, extendedAttributes, fileInfo, out document);
            this.storage.VerifySavedMappedObject(MappedObjectType.File, fileId, fileName, parentId, lastChangeToken, extendedAttributes, contentSize: 0);
            this.session.Verify(
                s => s.CreateDocument(
                    It.Is<IDictionary<string, object>>(p => p.ContainsKey("cmis:name")),
                    It.Is<IObjectId>(o => o.Id == parentId),
                    It.Is<IContentStream>(st => st == null),
                    null,
                    null,
                    null,
                    null),
                Times.Once());
            fileInfo.Verify(d => d.SetExtendedAttribute(It.IsAny<string>(), It.IsAny<string>()), Times.Once());
            document.Verify(d => d.AppendContentStream(It.IsAny<IContentStream>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Never());
            this.queue.Verify(q => q.AddEvent(It.IsAny<FileTransmissionEvent>()), Times.Once());
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalFolderAddedWithoutExtAttr()
        {
            string folderName = "a";
            string id = "id";
            string parentId = "papa";
            string lastChangeToken = "token";
            bool extendedAttributes = false;

            var dirInfo = this.RunSolveFolder(folderName, id, parentId, lastChangeToken, extendedAttributes);

            this.storage.VerifySavedMappedObject(MappedObjectType.Folder, id, folderName, parentId, lastChangeToken, extendedAttributes);
            this.session.Verify(s => s.CreateFolder(It.Is<IDictionary<string, object>>(p => p.ContainsKey("cmis:name")), It.Is<IObjectId>(o => o.Id == parentId)), Times.Once());
            dirInfo.Verify(d => d.SetExtendedAttribute(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalFolderAddedWithExtendedAttributes()
        {
            string folderName = "a";

            string id = "id";
            string parentId = "papa";
            string lastChangeToken = "token";
            bool extendedAttributes = true;
            var dirInfo = this.RunSolveFolder(folderName, id, parentId, lastChangeToken, extendedAttributes);

            this.storage.VerifySavedMappedObject(MappedObjectType.Folder, id, folderName, parentId, lastChangeToken, extendedAttributes);
            this.session.Verify(s => s.CreateFolder(It.Is<IDictionary<string, object>>(p => p.ContainsKey("cmis:name")), It.Is<IObjectId>(o => o.Id == parentId)), Times.Once());
            dirInfo.Verify(d => d.SetExtendedAttribute(It.Is<string>(k => k == MappedObject.ExtendedAttributeKey), It.Is<string>(v => !v.Equals(Guid.Empty))), Times.Once());
        }

        [Test, Category("Fast"), Category("Solver")]
        [ExpectedException(typeof(RetryException))]
        public void LocalFileIsUsedByAnotherProcess() {
            string fileName = "fileName";
            string fileId = "fileId";
            string parentId = "parentId";
            string lastChangeToken = "token";
            bool extendedAttributes = true;
            Exception exception = new ExtendedAttributeException();

            Mock<IFileInfo> fileInfo = new Mock<IFileInfo>();
            fileInfo.Setup(f => f.Length).Returns(0);
            fileInfo.Setup(f => f.SetExtendedAttribute(It.IsAny<string>(), It.IsAny<string>())).Throws(exception);

            try {
                Mock<IDocument> document;
                this.RunSolveFile(fileName, fileId, parentId, lastChangeToken, extendedAttributes, fileInfo, out document);
            } catch (RetryException e) {
                Assert.That(e.InnerException, Is.EqualTo(exception));
                fileInfo.Verify(d => d.SetExtendedAttribute(It.IsAny<string>(), It.IsAny<string>()), Times.Once());
                this.storage.Verify(s => s.SaveMappedObject(It.IsAny<IMappedObject>()), Times.Never());
                this.queue.Verify(q => q.AddEvent(It.IsAny<FileTransmissionEvent>()), Times.Never());
                throw;
            }
        }

        [Test, Category("Fast"), Category("Solver")]
        public void DoNotWriteLastWriteTimeUtcIfNotNecessary()
        {
            string fileName = "fileName";
            string fileId = "fileId";
            string parentId = "parentId";
            string lastChangeToken = "token";
            bool extendedAttributes = true;

            Mock<IFileInfo> fileInfo = new Mock<IFileInfo>();
            fileInfo.Setup(f => f.Length).Returns(0);

            Mock<IDocument> document;
            this.RunSolveFile(fileName, fileId, parentId, lastChangeToken, extendedAttributes, fileInfo, out document, false);

            fileInfo.VerifySet(f => f.LastWriteTimeUtc = It.IsAny<DateTime>(), Times.Never());
        }

        [Test, Category("Fast"), Category("Solver")]
        public void DoNotDieIfNotAbleToWriteUtcTime()
        {
            string fileName = "fileName";
            string fileId = "fileId";
            string parentId = "parentId";
            string lastChangeToken = "token";
            bool extendedAttributes = true;

            Mock<IFileInfo> fileInfo = new Mock<IFileInfo>();
            fileInfo.Setup(f => f.Length).Returns(0);
            fileInfo.SetupSet(f => f.LastWriteTimeUtc = It.IsAny<DateTime>()).Throws(new IOException());

            Mock<IDocument> document;
            this.RunSolveFile(fileName, fileId, parentId, lastChangeToken, extendedAttributes, fileInfo, out document, true);

        }

        private IDirectoryInfo SetupParentFolder(string parentId)
        {
            var parentDirInfo = Mock.Of<IDirectoryInfo>(d =>
                                                  d.FullName == Path.GetTempPath() &&
                                                  d.Name == Path.GetFileName(Path.GetTempPath()));

            this.storage.Setup(s => s.GetObjectByLocalPath(It.Is<IDirectoryInfo>(d => d.FullName == Path.GetTempPath()))).Returns(
                             Mock.Of<IMappedObject>(o =>
                             o.RemoteObjectId == parentId));
            return parentDirInfo;
        }

        private void RunSolveFile(string fileName, string fileId, string parentId, string lastChangeToken, bool extendedAttributes, Mock<IFileInfo> fileInfo, out Mock<IDocument> documentMock, bool returnLastModificationDate = false)
        {
            var parentDirInfo = this.SetupParentFolder(parentId);

            var parents = new List<IFolder>();
            parents.Add(Mock.Of<IFolder>(f => f.Id == parentId));

            string path = Path.Combine(Path.GetTempPath(), fileName);
            var futureRemoteDoc = Mock.Of<IDocument>(f =>
                                                f.Name == fileName &&
                                                f.Id == fileId &&
                                                f.Parents == parents &&
                                                f.ChangeToken == lastChangeToken);
            var futureRemoteDocId = Mock.Of<IObjectId>(o =>
                                                    o.Id == fileId);

            this.session.Setup(s => s.CreateDocument(
                It.Is<IDictionary<string, object>>(p => (string)p["cmis:name"] == fileName),
                It.Is<IObjectId>(o => o.Id == parentId),
                null,
                null,
                null,
                null,
                null)).Returns(futureRemoteDocId);
            IOperationContext operationContext = It.Is<IOperationContext>(
                o =>
                o.Filter.Contains("cmis:name") &&
                o.Filter.Contains("cmis:objectId") &&
                o.Filter.Contains("cmis:parentId") &&
                o.Filter.Contains("cmis:lastModificationDate") &&
                o.Filter.Contains("cmis:changeToken") &&
                o.Filter.Contains("cmis:contentStreamLength") &&
                o.Filter.Contains("cmis:contentStreamFileName") &&
                o.CacheEnabled == true);
            this.session.Setup(s => s.GetObject(It.Is<IObjectId>(o => o == futureRemoteDocId), It.IsAny<IOperationContext>())).Returns(futureRemoteDoc);
            Mock.Get(futureRemoteDoc).Setup(
                doc =>
                doc.SetContentStream(It.IsAny<IContentStream>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .Callback<IContentStream, bool, bool>(
                    (s, o, r) =>
                    {
                    using(var temp = new MemoryStream())
                    {
                        s.Stream.CopyTo(temp);
                    }
                });
            if(returnLastModificationDate) {
                Mock.Get(futureRemoteDoc).Setup(doc => doc.LastModificationDate).Returns(new DateTime());
            }
            fileInfo.Setup(d => d.FullName).Returns(path);
            fileInfo.Setup(d => d.Name).Returns(fileName);
            fileInfo.Setup(d => d.Exists).Returns(true);
            fileInfo.Setup(d => d.IsExtendedAttributeAvailable()).Returns(extendedAttributes);

            fileInfo.Setup(d => d.Directory).Returns(parentDirInfo);
            var transmissionManager = new ActiveActivitiesManager();
            var solver = new LocalObjectAdded(this.queue.Object, transmissionManager);

            solver.Solve(this.session.Object, this.storage.Object, fileInfo.Object, null);
            documentMock = Mock.Get(futureRemoteDoc);
            Assert.That(transmissionManager.ActiveTransmissions, Is.Empty);
        }

        private Mock<IDirectoryInfo> RunSolveFolder(string folderName, string id, string parentId, string lastChangeToken, bool extendedAttributes)
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
            IOperationContext operationContext = It.Is<IOperationContext>(
                o =>
                o.Filter.Contains("cmis:name") &&
                o.Filter.Contains("cmis:objectId") &&
                o.Filter.Contains("cmis:parentId") &&
                o.Filter.Contains("cmis:changeToken") &&
                o.CacheEnabled == true);
            this.session.Setup(s => s.GetObject(It.Is<IObjectId>(o => o == futureRemoteFolderId), It.IsAny<IOperationContext>())).Returns(futureRemoteFolder);

            var dirInfo = new Mock<IDirectoryInfo>();
            dirInfo.Setup(d => d.FullName).Returns(path);
            dirInfo.Setup(d => d.Name).Returns(folderName);
            dirInfo.Setup(d => d.Exists).Returns(true);
            dirInfo.Setup(d => d.IsExtendedAttributeAvailable()).Returns(extendedAttributes);

            var parentDirInfo = this.SetupParentFolder(parentId);
            dirInfo.Setup(d => d.Parent).Returns(parentDirInfo);
            var transmissionManager = new ActiveActivitiesManager();
            var solver = new LocalObjectAdded(this.queue.Object, transmissionManager);

            solver.Solve(this.session.Object, this.storage.Object, dirInfo.Object, null);
            return dirInfo;
        }

        private void VerifyThatLocalFileObjectLastWriteTimeUtcIsNeverModified(Mock<IFileSystemInfo> fsInfo) {
            fsInfo.VerifySet(o => o.LastWriteTimeUtc = It.IsAny<DateTime>(), Times.Never());
        }
    }
}
