//-----------------------------------------------------------------------
// <copyright file="RemoteObjectAddedTest.cs" company="GRAU DATA AG">
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
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;

    using CmisSync.Lib.Consumer.SituationSolver;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;

    using Moq;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture]
    public class RemoteObjectAddedTest
    {
        private readonly DateTime creationDate = DateTime.UtcNow;
        private readonly string id = "id";
        private readonly string objectName = "a";
        private readonly string parentId = "parentId";
        private readonly string lastChangeToken = "token";

        private string path;
        private ActiveActivitiesManager manager;
        private Mock<ISyncEventQueue> queue;
        private Mock<ISession> session;
        private Mock<IMetaDataStorage> storage;
        private RemoteObjectAdded underTest;
        private Mock<IFileSystemInfoFactory> fsFactory;

        [SetUp]
        public void SetUp()
        {
            this.path = Path.Combine(Path.GetTempPath(), this.objectName);
            this.manager = new ActiveActivitiesManager();
            this.session = new Mock<ISession>();
            this.storage = new Mock<IMetaDataStorage>();
            this.queue = new Mock<ISyncEventQueue>();
            this.fsFactory = new Mock<IFileSystemInfoFactory>(MockBehavior.Strict);
            this.underTest = new RemoteObjectAdded(this.session.Object, this.storage.Object, this.queue.Object, this.manager, this.fsFactory.Object);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void ConstructorTakesQueue()
        {
            new RemoteObjectAdded(this.session.Object, this.storage.Object, Mock.Of<ISyncEventQueue>(), this.manager);
        }

        [Test, Category("Fast"), Category("Solver")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorThrowsExceptionIfQueueIsNull() {
            new RemoteObjectAdded(this.session.Object, this.storage.Object, null, this.manager);
        }

        [Test, Category("Fast"), Category("Solver")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorThrowsExceptionIfTransmissionManagerIsNull() {
            new RemoteObjectAdded(this.session.Object, this.storage.Object, Mock.Of<ISyncEventQueue>(), null);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void RemoteFolderAdded()
        {
            var dirInfo = new Mock<IDirectoryInfo>();
            dirInfo.SetupAllProperties();
            dirInfo.Setup(d => d.FullName).Returns(this.path);
            dirInfo.Setup(d => d.Name).Returns(this.objectName);
            dirInfo.Setup(d => d.Parent).Returns(Mock.Of<IDirectoryInfo>());
            dirInfo.Setup(d => d.IsExtendedAttributeAvailable()).Returns(false);

            Mock<IFolder> remoteObject = MockOfIFolderUtil.CreateRemoteFolderMock(this.id, this.objectName, this.path, this.parentId, this.lastChangeToken);
            remoteObject.Setup(f => f.LastModificationDate).Returns((DateTime?)this.creationDate);

            this.underTest.Solve(dirInfo.Object, remoteObject.Object);

            dirInfo.Verify(d => d.Create(), Times.Once());
            this.storage.VerifySavedMappedObject(MappedObjectType.Folder, this.id, this.objectName, this.parentId, this.lastChangeToken, false, this.creationDate);
            dirInfo.VerifySet(d => d.LastWriteTimeUtc = It.Is<DateTime>(date => date.Equals(this.creationDate)), Times.Once());
            dirInfo.Verify(d => d.SetExtendedAttribute(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [Test, Category("Fast"), Category("Solver")]
        public void RemoteFolderAddedAndExtendedAttributesAreAvailable()
        {
            var dirInfo = new Mock<IDirectoryInfo>();
            dirInfo.SetupAllProperties();
            dirInfo.Setup(d => d.FullName).Returns(this.path);
            dirInfo.Setup(d => d.Name).Returns(this.objectName);
            dirInfo.Setup(d => d.Parent).Returns(Mock.Of<IDirectoryInfo>());
            dirInfo.Setup(d => d.IsExtendedAttributeAvailable()).Returns(true);

            Mock<IFolder> remoteObject = MockOfIFolderUtil.CreateRemoteFolderMock(this.id, this.objectName, this.path, this.parentId, this.lastChangeToken);
            remoteObject.Setup(f => f.LastModificationDate).Returns((DateTime?)this.creationDate);

            this.underTest.Solve(dirInfo.Object, remoteObject.Object);
            dirInfo.Verify(d => d.Create(), Times.Once());
            this.storage.VerifySavedMappedObject(MappedObjectType.Folder, this.id, this.objectName, this.parentId, this.lastChangeToken, true, this.creationDate);
            dirInfo.VerifySet(d => d.LastWriteTimeUtc = It.Is<DateTime>(date => date.Equals(this.creationDate)), Times.Once());
            dirInfo.Verify(d => d.SetExtendedAttribute(It.Is<string>(k => k.Equals(MappedObject.ExtendedAttributeKey)), It.IsAny<string>()), Times.Once());
        }

        [Test, Category("Fast"), Category("Solver")]
        public void RemoteFileAddedAndExtendedAttributesAreAvailable()
        {
            var cacheFileInfo = new Mock<IFileInfo>();
            var fileInfo = new Mock<IFileInfo>();
            var parentDir = Mock.Of<IDirectoryInfo>(d => d.FullName == Path.GetTempPath());
            fileInfo.SetupAllProperties();
            fileInfo.Setup(f => f.FullName).Returns(this.path);
            fileInfo.Setup(f => f.Name).Returns(this.objectName);
            fileInfo.Setup(f => f.Directory).Returns(parentDir);
            byte[] content = Encoding.UTF8.GetBytes("content");
            byte[] expectedHash = SHA1Managed.Create().ComputeHash(content);
            cacheFileInfo.SetupAllProperties();
            cacheFileInfo.Setup(f => f.FullName).Returns(this.path + ".sync");
            cacheFileInfo.Setup(f => f.Name).Returns(this.objectName + ".sync");
            cacheFileInfo.Setup(f => f.Directory).Returns(parentDir);
            cacheFileInfo.Setup(f => f.IsExtendedAttributeAvailable()).Returns(true);
            using (var stream = new MemoryStream()) {
                cacheFileInfo.Setup(f => f.Open(FileMode.Create, FileAccess.Write, FileShare.Read)).Returns(stream);
                this.fsFactory.AddIFileInfo(cacheFileInfo.Object);

                Mock<IDocument> remoteObject = MockOfIDocumentUtil.CreateRemoteDocumentMock(null, this.id, this.objectName, this.parentId, content.Length, content, this.lastChangeToken);
                remoteObject.Setup(f => f.LastModificationDate).Returns((DateTime?)this.creationDate);

                this.underTest.Solve(fileInfo.Object, remoteObject.Object);

                cacheFileInfo.Verify(f => f.Open(FileMode.Create, FileAccess.Write, FileShare.Read), Times.Once());
                cacheFileInfo.Verify(f => f.SetExtendedAttribute(It.Is<string>(s => s.Equals(MappedObject.ExtendedAttributeKey)), It.IsAny<string>()), Times.Once());
                cacheFileInfo.Verify(f => f.MoveTo(this.path), Times.Once());
                fileInfo.VerifySet(d => d.LastWriteTimeUtc = It.Is<DateTime>(date => date.Equals(this.creationDate)), Times.Once());
                this.queue.Verify(q => q.AddEvent(It.Is<FileTransmissionEvent>(e => e.Type == FileTransmissionType.DOWNLOAD_NEW_FILE)), Times.Once());
                this.storage.VerifySavedMappedObject(MappedObjectType.File, this.id, this.objectName, this.parentId, this.lastChangeToken, true, this.creationDate, expectedHash, content.Length);
            }
        }

        [Test, Category("Fast"), Category("Solver")]
        public void RemoteFileAddedAndLocalFileIsCreatedWhileDownloadIsInProgress()
        {
            string uuid = Guid.NewGuid().ToString();
            string conflictPath = this.path + ".conflict";
            var conflictFileInfo = Mock.Of<IFileInfo>(i => i.FullName == conflictPath);
            var cacheFileInfo = new Mock<IFileInfo>();
            var fileInfo = new Mock<IFileInfo>();
            var parentDir = Mock.Of<IDirectoryInfo>(d => d.FullName == Path.GetTempPath());
            fileInfo.SetupAllProperties();
            fileInfo.Setup(f => f.FullName).Returns(this.path);
            fileInfo.Setup(f => f.Name).Returns(this.objectName);
            fileInfo.Setup(f => f.Directory).Returns(parentDir);
            fileInfo.Setup(f => f.GetExtendedAttribute(MappedObject.ExtendedAttributeKey)).Returns(uuid);
            byte[] content = Encoding.UTF8.GetBytes("content");
            byte[] expectedHash = SHA1Managed.Create().ComputeHash(content);
            cacheFileInfo.SetupAllProperties();
            cacheFileInfo.Setup(f => f.FullName).Returns(this.path + ".sync");
            cacheFileInfo.Setup(f => f.Name).Returns(this.objectName + ".sync");
            cacheFileInfo.Setup(f => f.Directory).Returns(parentDir);
            cacheFileInfo.Setup(f => f.IsExtendedAttributeAvailable()).Returns(true);
            cacheFileInfo.Setup(f => f.Replace(fileInfo.Object, conflictFileInfo, true)).Returns(fileInfo.Object);
            using (var stream = new MemoryStream()) {
                cacheFileInfo.Setup(f => f.Open(FileMode.Create, FileAccess.Write, FileShare.Read)).Returns(stream).Callback(() => fileInfo.Setup(f => f.Exists).Returns(true));
                cacheFileInfo.Setup(f => f.MoveTo(this.path)).Throws(new IOException());

                this.fsFactory.AddIFileInfo(cacheFileInfo.Object);
                this.fsFactory.Setup(f => f.CreateConflictFileInfo(fileInfo.Object)).Returns(conflictFileInfo);
                Mock<IDocument> remoteObject = MockOfIDocumentUtil.CreateRemoteDocumentMock(null, this.id, this.objectName, this.parentId, content.Length, content, this.lastChangeToken);
                remoteObject.Setup(f => f.LastModificationDate).Returns((DateTime?)this.creationDate);

                this.underTest.Solve(fileInfo.Object, remoteObject.Object);

                cacheFileInfo.Verify(f => f.Open(FileMode.Create, FileAccess.Write, FileShare.Read), Times.Once());
                cacheFileInfo.Verify(f => f.SetExtendedAttribute(It.Is<string>(s => s.Equals(MappedObject.ExtendedAttributeKey)), It.IsAny<string>()), Times.Once());
                cacheFileInfo.Verify(f => f.MoveTo(this.path), Times.Once());
                cacheFileInfo.Verify(f => f.Replace(fileInfo.Object, conflictFileInfo, true), Times.Once());
                fileInfo.VerifySet(d => d.LastWriteTimeUtc = It.Is<DateTime>(date => date.Equals(this.creationDate)), Times.Once());
                this.queue.Verify(q => q.AddEvent(It.Is<FileTransmissionEvent>(e => e.Type == FileTransmissionType.DOWNLOAD_NEW_FILE)), Times.Once());
                this.storage.VerifySavedMappedObject(MappedObjectType.File, this.id, this.objectName, this.parentId, this.lastChangeToken, true, this.creationDate, expectedHash, content.Length);
                Mock.Get(conflictFileInfo).Verify(c => c.SetExtendedAttribute(MappedObject.ExtendedAttributeKey, null), Times.Once());
            }
        }
    }
}
