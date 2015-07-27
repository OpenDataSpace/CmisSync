//-----------------------------------------------------------------------
// <copyright file="RemoteObjectFetcherTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.AccumulatorTests {
    using System;
    using System.IO;

    using CmisSync.Lib.Accumulator;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Producer.Watcher;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;
    using DotCMIS.Exceptions;

    using Moq;

    using NUnit.Framework;

    using TestLibrary.TestUtils;
    using TestLibrary.UtilsTests;

    [TestFixture, Category("Fast")]
    public class RemoteObjectFetcherTest {
        private static readonly string Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "path");
        private static readonly string Id = "myId";
        private static readonly Guid Uuid = Guid.NewGuid();

        [Test]
        public void ConstructorTest() {
            var session = new Mock<ISession>();
            var storage = new Mock<IMetaDataStorage>();
            new RemoteObjectFetcher(session.Object, storage.Object);
        }

        [Test]
        public void DoNotGetExtendedAttributeIfFileDoesNotExist() {
            var session = new Mock<ISession>();
            session.SetupSessionDefaultValues();
            IDocument remote = MockOfIDocumentUtil.CreateRemoteDocumentMock(null, Id, "name", (string)null).Object;
            session.Setup(s => s.GetObject(Id, It.IsAny<IOperationContext>())).Returns(remote);

            var storage = new Mock<IMetaDataStorage>();
            storage.AddLocalFile(Path, Id, Uuid);

            var fileInfoMock = new Mock<IFileInfo>();
            fileInfoMock.Setup(f => f.Exists).Returns(false);

            var fileEvent = new FileEvent(fileInfoMock.Object);
            var fetcher = new RemoteObjectFetcher(session.Object, storage.Object);
            fetcher.Handle(fileEvent);

            fileInfoMock.Verify(f => f.GetExtendedAttribute(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void DoNotFetchIfExtendedAttributeIsMissing() {
            var session = new Mock<ISession>();
            session.SetupSessionDefaultValues();
            IDocument remote = MockOfIDocumentUtil.CreateRemoteDocumentMock(null, Id, "name", (string)null).Object;
            session.Setup(s => s.GetObject(Id, It.IsAny<IOperationContext>())).Returns(remote);

            var storage = new Mock<IMetaDataStorage>();
            storage.AddLocalFile(Path, Id, Uuid);

            var fileEvent = new FileEvent(Mock.Of<IFileInfo>());
            var fetcher = new RemoteObjectFetcher(session.Object, storage.Object);
            fetcher.Handle(fileEvent);

            session.Verify(s => s.GetObject(It.IsAny<string>(), It.IsAny<IOperationContext>()), Times.Never());
        }

        [Test]
        public void DoNotFetchIfDatabaseEntryIsMissing() {
            var session = new Mock<ISession>();
            session.SetupSessionDefaultValues();
            IDocument remote = MockOfIDocumentUtil.CreateRemoteDocumentMock(null, Id, "name", (string)null).Object;
            session.Setup(s => s.GetObject(Id, It.IsAny<IOperationContext>())).Returns(remote);

            var storage = new Mock<IMetaDataStorage>();

            var fileInfoMock = new Mock<IFileInfo>();
            fileInfoMock.Setup(f => f.GetExtendedAttribute(It.IsAny<string>())).Returns(Uuid.ToString());
            var fileEvent = new FileEvent(fileInfoMock.Object);
            var fetcher = new RemoteObjectFetcher(session.Object, storage.Object);
            fetcher.Handle(fileEvent);

            session.Verify(s => s.GetObject(It.IsAny<string>(), It.IsAny<IOperationContext>()), Times.Never());
        }

        [Test]
        public void FileDeletedEventWithoutObjectId() {
            var session = new Mock<ISession>();
            session.SetupSessionDefaultValues();
            IDocument remote = MockOfIDocumentUtil.CreateRemoteDocumentMock(null, Id, "name", (string)null).Object;
            session.Setup(s => s.GetObject(Id, It.IsAny<IOperationContext>())).Returns(remote);

            var storage = new Mock<IMetaDataStorage>();
            storage.AddLocalFile(Path, Id, Uuid);

            var fileInfoMock = new Mock<IFileInfo>();
            fileInfoMock.Setup(f => f.FullName).Returns(Path);
            var fileEvent = new FileEvent(fileInfoMock.Object) { Local = MetaDataChangeType.DELETED };
            var fetcher = new RemoteObjectFetcher(session.Object, storage.Object);

            Assert.That(fetcher.Handle(fileEvent), Is.False);
            Assert.That(fileEvent.RemoteFile, Is.Not.Null);
        }

        [Test]
        public void FileEventWithoutObjectId() {
            var session = new Mock<ISession>();
            session.SetupSessionDefaultValues();
            IDocument remote = MockOfIDocumentUtil.CreateRemoteDocumentMock(null, Id, "name", (string)null).Object;
            session.Setup(s => s.GetObject(Id, It.IsAny<IOperationContext>())).Returns(remote);

            var storage = new Mock<IMetaDataStorage>();
            storage.AddLocalFile(Path, Id, Uuid);

            var fileInfoMock = new Mock<IFileInfo>();
            fileInfoMock.Setup(f => f.Uuid).Returns(Uuid);
            fileInfoMock.Setup(f => f.Exists).Returns(true);
            var fileEvent = new FileEvent(fileInfoMock.Object);
            var fetcher = new RemoteObjectFetcher(session.Object, storage.Object);

            Assert.That(fetcher.Handle(fileEvent), Is.False);
            Assert.That(fileEvent.RemoteFile, Is.Not.Null);
        }

        [Test]
        public void FileEventForRemovedFile() {
            var session = new Mock<ISession>();
            session.SetupSessionDefaultValues();
            session.Setup(s => s.GetObject(Id, It.IsAny<IOperationContext>())).Throws(new CmisObjectNotFoundException());

            var storage = new Mock<IMetaDataStorage>();
            storage.AddLocalFile(Path, Id);

            var fileInfoMock = new Mock<IFileInfo>();
            fileInfoMock.Setup(f => f.GetExtendedAttribute(It.IsAny<string>())).Returns(Uuid.ToString());
            var fileEvent = new FileEvent(fileInfoMock.Object);

            var fetcher = new RemoteObjectFetcher(session.Object, storage.Object);
            Assert.That(fetcher.Handle(fileEvent), Is.False);
            Assert.That(fileEvent.RemoteFile, Is.Null);
        }

        [Test]
        public void FileEventWithIDocument() {
            var session = new Mock<ISession>();
            var storage = new Mock<IMetaDataStorage>();
            var fetcher = new RemoteObjectFetcher(session.Object, storage.Object);
            var fileEvent = new FileEvent(new Mock<IFileInfo>().Object, new Mock<IDocument>().Object);
            fetcher.Handle(fileEvent);
            session.Verify(s => s.GetObject(It.IsAny<string>(), It.IsAny<IOperationContext>()), Times.Never());
        }

        [Test]
        public void FileMovedEventWithoutLocalPath() {
            var session = new Mock<ISession>();
            var fetcher = new RemoteObjectFetcher(session.Object, Mock.Of<IMetaDataStorage>());
            var fileMovedEvent = new FileMovedEvent(null, null, Path, Mock.Of<IDocument>());
            Assert.That(fetcher.Handle(fileMovedEvent), Is.False);
            session.Verify(s => s.GetObject(It.IsAny<string>(), It.IsAny<IOperationContext>()), Times.Never());
        }

        [Test]
        public void FolderEventWithIFolder() {
            var session = new Mock<ISession>();
            var fetcher = new RemoteObjectFetcher(session.Object, Mock.Of<IMetaDataStorage>());
            var folderEvent = new FolderEvent(new Mock<IDirectoryInfo>().Object, new Mock<IFolder>().Object);
            fetcher.Handle(folderEvent);
            session.Verify(s => s.GetObject(It.IsAny<string>(), It.IsAny<IOperationContext>()), Times.Never());
        }

        [Test]
        public void FolderEventWithoutObjectId() {
            var session = new Mock<ISession>();
            session.SetupSessionDefaultValues();
            IFolder remote = MockOfIFolderUtil.CreateRemoteFolderMock(Id, "name", "/name").Object;
            session.Setup(s => s.GetObject(Id, It.IsAny<IOperationContext>())).Returns(remote);

            var storage = new Mock<IMetaDataStorage>();
            storage.AddLocalFolder(Path, Id, Uuid);

            var dirMock = new Mock<IDirectoryInfo>();
            dirMock.Setup(d => d.Exists).Returns(true);
            dirMock.Setup(d => d.Uuid).Returns(Uuid);

            var folderEvent = new FolderEvent(dirMock.Object);
            var fetcher = new RemoteObjectFetcher(session.Object, storage.Object);

            Assert.That(fetcher.Handle(folderEvent), Is.False);
            Assert.That(folderEvent.RemoteFolder, Is.Not.Null);
        }

        [Test]
        public void FolderEventWithoutObjectIdAndExtendedAttributeExceptionOnUuidRequest() {
            var session = new Mock<ISession>();
            session.SetupSessionDefaultValues();
            IFolder remote = MockOfIFolderUtil.CreateRemoteFolderMock(Id, "name", "/name").Object;
            session.Setup(s => s.GetObject(Id, It.IsAny<IOperationContext>())).Returns(remote);

            var storage = new Mock<IMetaDataStorage>();
            storage.AddLocalFolder(Path, Id, Uuid);

            var dirMock = new Mock<IDirectoryInfo>();
            dirMock.Setup(d => d.Exists).Returns(true);
            dirMock.Setup(d => d.Uuid).Throws<ExtendedAttributeException>();

            var folderEvent = new FolderEvent(dirMock.Object);
            var fetcher = new RemoteObjectFetcher(session.Object, storage.Object);

            Assert.That(fetcher.Handle(folderEvent), Is.False);
            Assert.That(folderEvent.RemoteFolder, Is.Null);
        }

        [Test]
        public void FolderEventForRemovedFolder() {
            var session = new Mock<ISession>();
            session.SetupSessionDefaultValues();
            session.Setup(s => s.GetObject(Id, It.IsAny<IOperationContext>())).Throws(new CmisObjectNotFoundException());

            var storage = new Mock<IMetaDataStorage>();
            storage.AddLocalFolder(Path, Id);

            var dirMock = new Mock<IDirectoryInfo>();
            dirMock.Setup(d => d.GetExtendedAttribute(MappedObject.ExtendedAttributeKey)).Returns(Uuid.ToString());

            var folderEvent = new FolderEvent(dirMock.Object);

            var fetcher = new RemoteObjectFetcher(session.Object, storage.Object);
            Assert.That(fetcher.Handle(folderEvent), Is.False);
            Assert.That(folderEvent.RemoteFolder, Is.Null);
        }

        [Test]
        public void CrawlRequestedEventWithNewRemoteFolder() {
            var session = new Mock<ISession>();
            session.SetupSessionDefaultValues();
            IFolder remote = MockOfIFolderUtil.CreateRemoteFolderMock(Id, "name", "/name").Object;
            session.Setup(s => s.GetObject(Id, It.IsAny<IOperationContext>())).Returns(remote);

            var storage = new Mock<IMetaDataStorage>();
            storage.AddLocalFolder(Path, Id, Uuid);

            var dirMock = new Mock<IDirectoryInfo>();
            dirMock.Setup(d => d.Exists).Returns(true);
            dirMock.Setup(d => d.Uuid).Returns(Uuid);
            var crawlEvent = new CrawlRequestEvent(localFolder: dirMock.Object, remoteFolder: null);

            var fetcher = new RemoteObjectFetcher(session.Object, storage.Object);
            Assert.That(fetcher.Handle(crawlEvent), Is.False);
            Assert.That(crawlEvent.RemoteFolder, Is.EqualTo(remote));
        }

        [Test]
        public void OperationContextDoesNotUsesTheSessionCache() {
            var session = new Mock<ISession>();
            session.SetupSessionDefaultValues();
            IFolder remote = MockOfIFolderUtil.CreateRemoteFolderMock(Id, "name", "/name").Object;
            session.Setup(s => s.GetObject(Id, It.IsAny<IOperationContext>())).Returns(remote);

            var storage = new Mock<IMetaDataStorage>();
            storage.AddLocalFolder(Path, Id, Uuid);

            var dirMock = new Mock<IDirectoryInfo>();
            dirMock.Setup(d => d.Exists).Returns(true);
            dirMock.Setup(d => d.GetExtendedAttribute(MappedObject.ExtendedAttributeKey)).Returns(Uuid.ToString());
            var folderEvent = new FolderEvent(dirMock.Object);
            var fetcher = new RemoteObjectFetcher(session.Object, storage.Object);

            Assert.That(fetcher.Handle(folderEvent), Is.False);
            session.VerifyThatCachingIsDisabled();
        }
    }
}