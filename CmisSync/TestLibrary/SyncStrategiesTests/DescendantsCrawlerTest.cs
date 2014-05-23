//-----------------------------------------------------------------------
// <copyright file="DescendantsCrawlerTest.cs" company="GRAU DATA AG">
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
    using System.Collections.Generic;
    using System.IO;

    using CmisSync.Lib.Data;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Storage;
    using CmisSync.Lib.Sync.Strategy;

    using DotCMIS.Client;

    using Moq;

    using NUnit.Framework;

    using TestUtils;

    [TestFixture]
    public class DescendantsCrawlerTest
    {
        private readonly string remoteRootId = "rootId";
        private readonly string remoteRootPath = "/";
        private Mock<ISyncEventQueue> queue;
        private Mock<IMetaDataStorage> storage;
        private Mock<IFolder> remoteFolder;
        private Mock<IDirectoryInfo> localFolder;
        private Mock<IFileSystemInfoFactory> fsFactory;
        private string localRootPath;
        private MappedObject mappedRootObject;

        [SetUp]
        public void CreateMockObjects()
        {
            this.localRootPath = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());
            this.queue = new Mock<ISyncEventQueue>();
            this.storage = new Mock<IMetaDataStorage>();
            this.remoteFolder = new Mock<IFolder>();
            this.remoteFolder.SetupDescendants();
            this.localFolder = new Mock<IDirectoryInfo>();
            this.localFolder.Setup(f => f.FullName).Returns(this.localRootPath);
            this.localFolder.Setup(f => f.Exists).Returns(true);
            this.fsFactory = new Mock<IFileSystemInfoFactory>();
            this.fsFactory.AddIDirectoryInfo(this.localFolder.Object);
            this.mappedRootObject = new MappedObject(
                this.remoteRootPath,
                this.remoteRootId,
                MappedObjectType.Folder,
                null,
                "changeToken");
            this.storage.AddMappedFolder(this.mappedRootObject, this.localRootPath, this.remoteRootPath);
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorThrowsExceptionIfLocalFolderIsNull()
        {
            new DescendantsCrawler(Mock.Of<ISyncEventQueue>(), Mock.Of<IFolder>(), null, Mock.Of<IMetaDataStorage>());
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorThrowsExceptionIfRemoteFolderIsNull()
        {
            new DescendantsCrawler(Mock.Of<ISyncEventQueue>(), null, Mock.Of<IDirectoryInfo>(), Mock.Of<IMetaDataStorage>());
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorThrowsExceptionIfQueueIsNull()
        {
            new DescendantsCrawler(null, Mock.Of<IFolder>(), Mock.Of<IDirectoryInfo>(), Mock.Of<IMetaDataStorage>());
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorThrowsExceptionIfStorageIsNull()
        {
            new DescendantsCrawler(Mock.Of<ISyncEventQueue>(), Mock.Of<IFolder>(), Mock.Of<IDirectoryInfo>(), null);
        }

        [Test, Category("Fast")]
        public void ConstructorWorksWithoutFsInfoFactory()
        {
            new DescendantsCrawler(Mock.Of<ISyncEventQueue>(), Mock.Of<IFolder>(), Mock.Of<IDirectoryInfo>(), Mock.Of<IMetaDataStorage>());
        }

        [Test, Category("Fast")]
        public void ConstructorTakesFsInfoFactory()
        {
            this.CreateCrawler();
        }

        [Test, Category("Fast")]
        public void PriorityIsNormal()
        {
            var crawler = this.CreateCrawler();
            Assert.That(crawler.Priority == EventHandlerPriorities.NORMAL);
        }

        [Test, Category("Fast")]
        public void IgnoresNonFittingEvents()
        {
            var crawler = this.CreateCrawler();
            Assert.That(crawler.Handle(Mock.Of<ISyncEvent>()), Is.False);
            this.queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Never());
        }

        [Test, Category("Fast")]
        public void HandlesStartNextSyncEventAndReportsOnQueueIfDone()
        {
            var crawler = this.CreateCrawler();
            var startEvent = new StartNextSyncEvent();
            Assert.That(crawler.Handle(startEvent), Is.True);
            this.queue.Verify(q => q.AddEvent(It.Is<FullSyncCompletedEvent>(e => e.StartEvent.Equals(startEvent))), Times.Once());
        }

        [Test, Category("Fast")]
        public void RecognizesOneNewRemoteFolder()
        {
            var newRemoteFolder = Mock.Of<IFolder>();
            this.remoteFolder.SetupDescendants(newRemoteFolder);
            var crawler = this.CreateCrawler();

            Assert.That(crawler.Handle(new StartNextSyncEvent()), Is.True);
            this.queue.Verify(q => q.AddEvent(It.Is<FolderEvent>(e => e.RemoteFolder.Equals(newRemoteFolder))), Times.Once());
        }

        [Test, Category("Fast")]
        public void RegognizesNewRemoteFolderHierarchie()
        {
            var newRemoteSubFolder = Mock.Of<IFolder>();
            var newRemoteFolder = new Mock<IFolder>();
            this.remoteFolder.SetupDescendants(this.remoteFolder.Object);
            var crawler = this.CreateCrawler();

            Assert.That(crawler.Handle(new StartNextSyncEvent()), Is.True);
            this.queue.Verify(q => q.AddEvent(It.Is<FolderEvent>(e => e.RemoteFolder.Equals(newRemoteFolder.Object))), Times.Once());
            this.queue.Verify(q => q.AddEvent(It.Is<FolderEvent>(e => e.RemoteFolder.Equals(newRemoteSubFolder))), Times.Once());
        }

        [Test, Category("Fast")]
        public void RecognizesOneNewLocalFolder()
        {
            var newFolderMock = this.fsFactory.AddDirectory(Path.Combine(this.localRootPath, "newFolder"));
            var crawler = this.CreateCrawler();

            Assert.That(crawler.Handle(new StartNextSyncEvent()), Is.True);
            this.queue.Verify(q => q.AddEvent(It.Is<FolderEvent>(e => e.LocalFolder.Equals(newFolderMock.Object))), Times.Once());
        }

        private DescendantsCrawler CreateCrawler()
        {
            return new DescendantsCrawler(
                this.queue.Object,
                this.remoteFolder.Object,
                this.localFolder.Object,
                this.storage.Object,
                this.fsFactory.Object);
        }
    }
}