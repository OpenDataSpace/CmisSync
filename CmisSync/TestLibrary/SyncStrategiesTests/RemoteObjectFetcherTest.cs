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

namespace TestLibrary.SyncStrategiesTests
{
    using System;
    using System.IO;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Storage;
    using CmisSync.Lib.Sync.Strategy;

    using DotCMIS.Client;
    using DotCMIS.Exceptions;

    using Moq;

    using NUnit.Framework;

    using TestLibrary.TestUtils;
    using TestLibrary.UtilsTests;

    [TestFixture]
    public class RemoteObjectFetcherTest 
    {
        private static readonly string Path = "/path";
        private static readonly string Id = "myId";

        [Test, Category("Fast")]
        public void ConstructorTest() {
            var session = new Mock<ISession>();
            var storage = new Mock<IMetaDataStorage>();
            new RemoteObjectFetcher(session.Object, storage.Object);
        }

        [Test, Category("Fast")]
        public void FileEventWithoutObjectId() {
            var session = new Mock<ISession>();
            session.SetupSessionDefaultValues();
            IDocument remote = MockOfIDocumentUtil.CreateRemoteDocumentMock(null, Id, "name", null).Object;
            session.Setup(s => s.GetObject(Id, It.IsAny<IOperationContext>())).Returns(remote);

            var storage = new Mock<IMetaDataStorage>();
            storage.AddLocalFile(Path, Id);

            var fileEvent = new FileEvent(new FileInfoWrapper(new FileInfo(Path)));
            var fetcher = new RemoteObjectFetcher(session.Object, storage.Object);

            Assert.That(fetcher.Handle(fileEvent), Is.False);
            Assert.That(fileEvent.RemoteFile, Is.Not.Null);
        }

        [Test, Category("Fast")]
        public void FileEventForRemovedFile() {
            var session = new Mock<ISession>();
            session.SetupSessionDefaultValues();
            session.Setup(s => s.GetObject(Id, It.IsAny<IOperationContext>())).Throws(new CmisObjectNotFoundException());

            var storage = new Mock<IMetaDataStorage>();
            storage.AddLocalFile(Path, Id);

            var fileEvent = new FileEvent(new FileInfoWrapper(new FileInfo(Path)));

            var fetcher = new RemoteObjectFetcher(session.Object, storage.Object);
            Assert.That(fetcher.Handle(fileEvent), Is.False);
            Assert.That(fileEvent.RemoteFile, Is.Null);
        }

        [Test, Category("Fast")]
        public void FileEventWithIDocument() {
            var session = new Mock<ISession>();
            var storage = new Mock<IMetaDataStorage>();
            var fetcher = new RemoteObjectFetcher(session.Object, storage.Object);
            var fileEvent = new FileEvent(new Mock<IFileInfo>().Object, new Mock<IDocument>().Object); 
            fetcher.Handle(fileEvent);
            session.Verify(s => s.GetObject(It.IsAny<string>(), It.IsAny<IOperationContext>()), Times.Never());
        }

        [Test, Category("Fast")]
        public void FolderEventWithIFolder() {
            var session = new Mock<ISession>();
            var storage = new Mock<IMetaDataStorage>();
            var fetcher = new RemoteObjectFetcher(session.Object, storage.Object);
            var fileEvent = new FolderEvent(new Mock<IDirectoryInfo>().Object, new Mock<IFolder>().Object); 
            fetcher.Handle(fileEvent);
            session.Verify(s => s.GetObject(It.IsAny<string>(), It.IsAny<IOperationContext>()), Times.Never());
        }

        [Test, Category("Fast")]
        public void FolderEventWithoutObjectId() {
            var session = new Mock<ISession>();
            session.SetupSessionDefaultValues();
            IFolder remote = MockOfIFolderUtil.CreateRemoteFolderMock(Id, "name", "/name").Object;
            session.Setup(s => s.GetObject(Id, It.IsAny<IOperationContext>())).Returns(remote);

            var storage = new Mock<IMetaDataStorage>();
            storage.AddLocalFolder(Path, Id);

            var folderEvent = new FolderEvent(new DirectoryInfoWrapper(new DirectoryInfo(Path)));
            var fetcher = new RemoteObjectFetcher(session.Object, storage.Object);

            Assert.That(fetcher.Handle(folderEvent), Is.False);
            Assert.That(folderEvent.RemoteFolder, Is.Not.Null);
        }

        [Test, Category("Fast")]
        public void FolderEventForRemovedFolder() {
            var session = new Mock<ISession>();
            session.SetupSessionDefaultValues();
            session.Setup(s => s.GetObject(Id, It.IsAny<IOperationContext>())).Throws(new CmisObjectNotFoundException());

            var storage = new Mock<IMetaDataStorage>();
            storage.AddLocalFolder(Path, Id);

            var folderEvent = new FolderEvent(new DirectoryInfoWrapper(new DirectoryInfo(Path)));

            var fetcher = new RemoteObjectFetcher(session.Object, storage.Object);
            Assert.That(fetcher.Handle(folderEvent), Is.False);
            Assert.That(folderEvent.RemoteFolder, Is.Null);
        }

        [Test, Category("Fast")]
        public void CrawlRequestedEventWithNewRemoteFolder() {
            var session = new Mock<ISession>();
            session.SetupSessionDefaultValues();
            IFolder remote = MockOfIFolderUtil.CreateRemoteFolderMock(Id, "name", "/name").Object;
            session.Setup(s => s.GetObject(Id, It.IsAny<IOperationContext>())).Returns(remote);

            var storage = new Mock<IMetaDataStorage>();
            storage.AddLocalFolder(Path, Id);

            var crawlEvent = new CrawlRequestEvent(localFolder: new DirectoryInfoWrapper(new DirectoryInfo(Path)), remoteFolder: null);

            var fetcher = new RemoteObjectFetcher(session.Object, storage.Object);
            Assert.That(fetcher.Handle(crawlEvent), Is.False);
            Assert.That(crawlEvent.RemoteFolder, Is.EqualTo(remote));
        }

        [Test, Category("Fast")]
        public void OperationContextDoesNotUsesTheSessionCache() {
            var session = new Mock<ISession>();
            session.SetupSessionDefaultValues();
            IFolder remote = MockOfIFolderUtil.CreateRemoteFolderMock(Id, "name", "/name").Object;
            session.Setup(s => s.GetObject(Id, It.IsAny<IOperationContext>())).Returns(remote);

            var storage = new Mock<IMetaDataStorage>();
            storage.AddLocalFolder(Path, Id);

            var folderEvent = new FolderEvent(new DirectoryInfoWrapper(new DirectoryInfo(Path)));
            var fetcher = new RemoteObjectFetcher(session.Object, storage.Object);

            Assert.That(fetcher.Handle(folderEvent), Is.False);
            OperationContextFactoryTest.VerifyThatCachingIsDisabled(session);
        }
    }
}
