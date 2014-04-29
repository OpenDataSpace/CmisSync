//-----------------------------------------------------------------------
// <copyright file="CrawlerTest.cs" company="GRAU DATA AG">
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

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Storage;
    using CmisSync.Lib.Sync.Strategy;

    using DotCMIS.Client;

    using Moq;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture]
    public class CrawlerTest
    {
        private string localPath;
        private IDirectoryInfo localFolder;
        private DirectoryInfo realLocalFolder;

        [SetUp]
        public void SetUp() {
            this.localPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            this.realLocalFolder = new DirectoryInfo(this.localPath);
            this.localFolder = new DirectoryInfoWrapper(this.realLocalFolder);
            this.localFolder.Create();
        }

        [TearDown]
        public void TearDown() {
            this.localFolder.Refresh();
            this.realLocalFolder.Delete();
        }

        [Test, Category("Fast")]
        public void ConstructorWithValidInputTest() {
            var queue = new Mock<ISyncEventQueue>().Object;
            var remoteFolder = new Mock<IFolder>().Object;
            var localFolder = new Mock<IDirectoryInfo>();
            var factory = new Mock<IFileSystemInfoFactory>();
            new Crawler(queue, remoteFolder, localFolder.Object, factory.Object);
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorFailsOnNullQueueTest()
        {
            var remoteFolder = new Mock<IFolder>().Object;
            var localFolder = new Mock<IDirectoryInfo>();

            new Crawler(null, remoteFolder, localFolder.Object);
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorFailsOnNullLocalFolderTest()
        {
            var queue = new Mock<ISyncEventQueue>().Object;
            var remoteFolder = new Mock<IFolder>().Object;
            new Crawler(queue, remoteFolder, null);
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorFailsOnNullRemoteFolderTest()
        {
            var queue = new Mock<ISyncEventQueue>().Object;
            new Crawler(queue, null, this.localFolder);
        }

        [Test, Category("Fast")]
        public void IgnoreWrongEventsTest() {
            var queue = new Mock<ISyncEventQueue>().Object;
            var remoteFolder = new Mock<IFolder>().Object;
            var localFolder = new Mock<IDirectoryInfo>();
            var wrongEvent = new Mock<ISyncEvent>().Object;
            var crawler = new Crawler(queue, remoteFolder, localFolder.Object);
            Assert.False(crawler.Handle(wrongEvent));
        }

        private Crawler GetCrawlerWithFakes(Mock<ISyncEventQueue> queue) {
            //these are fakes that throw if they are used
            var localFolder = new Mock<IDirectoryInfo>(MockBehavior.Strict);
            var remoteFolder = new Mock<IFolder>(MockBehavior.Strict);
            return new Crawler(queue.Object, remoteFolder.Object, localFolder.Object);
        }

        [Test, Category("Fast")]
        public void CrawlingFullSyncEmptyFoldersTest() {
            var queue = new Mock<ISyncEventQueue>();

            var remoteFolder = MockSessionUtil.CreateCmisFolder();

            var localFolder = new Mock<IDirectoryInfo>();

            var crawler = new Crawler(queue.Object, remoteFolder.Object, localFolder.Object);
            var startEvent = new StartNextSyncEvent(true);
            Assert.True(crawler.Handle(startEvent));

            queue.Verify(q => q.AddEvent(It.IsAny<FullSyncCompletedEvent>()), Times.Once());
            // this and only this should be added
            queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Once());
        }

        private bool VerifyRemoteFileEvent(FileEvent e, string folder, string file, MetaDataChangeType metaType, ContentChangeType changeType) {
            this.VerifyFileEvent(e, folder, file);
            Assert.That(e.RemoteFile, Is.Not.Null);
            Assert.That(e.Remote, Is.EqualTo(metaType));
            Assert.That(e.RemoteContent, Is.EqualTo(changeType));
            Assert.That(e.Local, Is.EqualTo(MetaDataChangeType.NONE));
            Assert.That(e.LocalContent, Is.EqualTo(ContentChangeType.NONE));
            return true;
        }

        private bool VerifyLocalFileEventCreated(FileEvent e, string folder, string file) {
            this.VerifyFileEvent(e, folder, file);
            Assert.That(e.RemoteFile, Is.Null);
            Assert.That(e.Remote, Is.EqualTo(MetaDataChangeType.NONE));
            Assert.That(e.RemoteContent, Is.EqualTo(ContentChangeType.NONE));
            Assert.That(e.Local, Is.EqualTo(MetaDataChangeType.CREATED));
            Assert.That(e.LocalContent, Is.EqualTo(ContentChangeType.CREATED));
            return true;
        }

        private bool VerifyFileEvent(FileEvent e, string folder, string file) {
            Assert.That(e.LocalFile.FullName, Is.EqualTo(Path.Combine(folder, file)));
            Assert.That(e.LocalParentDirectory.FullName, Is.EqualTo(folder));
                    
            return true;
        }

        private bool VerifyRemoteFolderEvent(FolderEvent e, string name)
        {
            Assert.That(e.RemoteFolder, Is.Not.Null);
            Assert.That(e.RemoteFolder.Name, Is.EqualTo(name));
            Assert.That(e.Remote, Is.EqualTo(MetaDataChangeType.CREATED));
            return true;
        }

        [Test, Category("Fast")]
        public void CrawlingRemoteFolderWith1FileEmptyLocal() {
            string localPath = "/";
            string name = "file";
            var queue = new Mock<ISyncEventQueue>();

            var remoteFolder = MockSessionUtil.CreateCmisFolder(fileNames: new List<string> { name });

            var localFolder = MockOfIFileSystemInfoFactoryUtil.CreateLocalFolder(localPath);

            var crawler = this.GetCrawlerWithFakes(queue);
            var crawlEvent = new CrawlRequestEvent(localFolder.Object, remoteFolder.Object);
            Assert.True(crawler.Handle(crawlEvent));

            queue.Verify(q => q.AddEvent(
                It.Is<FileEvent>(e => this.VerifyRemoteFileEvent(e, localPath, name, MetaDataChangeType.CREATED, ContentChangeType.NONE))
                ), Times.Once());
        } 
        
        [Test, Category("Fast")]
        public void CrawlingRemoteFolderWith1FileWithContentEmptyLocal() {
            string localPath = "/";
            string name = "file";
            var queue = new Mock<ISyncEventQueue>();

            var remoteFolder = MockSessionUtil.CreateCmisFolder(fileNames: new List<string> { name }, contentStream: true);

            var localFolder = MockOfIFileSystemInfoFactoryUtil.CreateLocalFolder(localPath);

            var crawler = this.GetCrawlerWithFakes(queue);
            var crawlEvent = new CrawlRequestEvent(localFolder.Object, remoteFolder.Object);
            Assert.True(crawler.Handle(crawlEvent));

            queue.Verify(q => q.AddEvent(
                It.Is<FileEvent>(e => this.VerifyRemoteFileEvent(e, localPath, name, MetaDataChangeType.CREATED, ContentChangeType.CREATED))
                ), Times.Once());
        }

        [Test, Category("Fast")]
        public void CrawlingRemoteFolderWith1ChildFolder() {
            string localPath = "/";
            string name = "folder";
            var queue = new Mock<ISyncEventQueue>();

            var remoteFolder = MockSessionUtil.CreateCmisFolder(folderNames: new List<string> { name });

            var localFolder = MockOfIFileSystemInfoFactoryUtil.CreateLocalFolder(localPath);

            var crawler = this.GetCrawlerWithFakes(queue);
            var crawlEvent = new CrawlRequestEvent(localFolder.Object, remoteFolder.Object);
            Assert.That(crawler.Handle(crawlEvent), Is.True);

            queue.Verify(q => q.AddEvent(
                It.Is<FolderEvent>(e => this.VerifyRemoteFolderEvent(e, name))
                ), Times.Once());
            queue.Verify(q => q.AddEvent(
                It.Is<CrawlRequestEvent>(e => e.RemoteFolder.Name == name && e.LocalFolder.Name == name)
                ), Times.Once());
        }

        [Test, Category("Fast")]
        public void CrawlingFolderWith1FileRemoteAndLocal() {
            string localPath = "/";
            string name = "file";
            var queue = new Mock<ISyncEventQueue>();

            var remoteFolder = MockSessionUtil.CreateCmisFolder(fileNames: new List<string> { name });

            var localFolder = MockOfIFileSystemInfoFactoryUtil.CreateLocalFolder(localPath, fileNames: new List<string> { name });

            var crawler = this.GetCrawlerWithFakes(queue);
            var crawlEvent = new CrawlRequestEvent(localFolder.Object, remoteFolder.Object);
            Assert.True(crawler.Handle(crawlEvent));

            queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Once());
            queue.Verify(q => q.AddEvent(
                It.Is<FileEvent>(e => this.VerifyRemoteFileEvent(e, localPath, name, MetaDataChangeType.NONE, ContentChangeType.NONE))
                        ), Times.Once());
        }

        [Test, Category("Fast")]
        public void CrawlingFolderWith1FileOnlyLocal() {
            string localPath = "/";
            string name = "file";
            var queue = new Mock<ISyncEventQueue>();

            var remoteFolder = MockSessionUtil.CreateCmisFolder();

            var localFolder = MockOfIFileSystemInfoFactoryUtil.CreateLocalFolder(localPath, fileNames: new List<string> { name });

            var crawler = this.GetCrawlerWithFakes(queue);
            var crawlEvent = new CrawlRequestEvent(localFolder.Object, remoteFolder.Object);
            Assert.True(crawler.Handle(crawlEvent));

            queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Once());
            queue.Verify(q => q.AddEvent(
                It.Is<FileEvent>(e => this.VerifyLocalFileEventCreated(e, localPath, name))
                ), Times.Once());
        }

        private bool VerifyLocalFolderEvent(FolderEvent e, string folder, string file) {
            Assert.That(e.LocalFolder.FullName, Is.EqualTo(Path.Combine(folder, file)));
            Assert.That(e.RemoteFolder, Is.Null);
            Assert.That(e.Remote, Is.EqualTo(MetaDataChangeType.NONE));
            Assert.That(e.Local, Is.EqualTo(MetaDataChangeType.CREATED));
            return true;
        }

        [Test, Category("Fast")]
        public void CrawlingFolderWith1LocalFolder() {
            string localPath = "/";
            string name = "folder";
            var queue = new Mock<ISyncEventQueue>();

            var remoteFolder = MockSessionUtil.CreateCmisFolder();

            var localFolder = MockOfIFileSystemInfoFactoryUtil.CreateLocalFolder(localPath, folderNames: new List<string> { name });

            var crawler = this.GetCrawlerWithFakes(queue);
            var crawlEvent = new CrawlRequestEvent(localFolder.Object, remoteFolder.Object);
            Assert.True(crawler.Handle(crawlEvent));

            queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Exactly(2));
            queue.Verify(q => q.AddEvent(
                It.Is<FolderEvent>(e => this.VerifyLocalFolderEvent(e, localPath, name))
                ), Times.Once());
            queue.Verify(q => q.AddEvent(
                It.Is<CrawlRequestEvent>(e => e.LocalFolder.Name == name && e.RemoteFolder == null)
                ), Times.Once());
        }

        private bool VerifyRemoteFolderEventCreation(FolderEvent e, string name) {
            Assert.That(e.LocalFolder, Is.Null);
            Assert.That(e.RemoteFolder.Name, Is.EqualTo(name));
            Assert.That(e.Remote, Is.EqualTo(MetaDataChangeType.CREATED));
            Assert.That(e.Local, Is.EqualTo(MetaDataChangeType.NONE));
            return true;
        }

        [Test, Category("Fast")]
        public void CrawlingFolderWith1RemoteFolder() {
            string localPath = "/";
            string name = "file";
            var queue = new Mock<ISyncEventQueue>();

            var remoteFolder = MockSessionUtil.CreateCmisFolder(folderNames: new List<string> { name });

            var localFolder = MockOfIFileSystemInfoFactoryUtil.CreateLocalFolder(localPath);

            var crawler = this.GetCrawlerWithFakes(queue);
            var crawlEvent = new CrawlRequestEvent(localFolder.Object, remoteFolder.Object);
            Assert.True(crawler.Handle(crawlEvent));

            queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Exactly(2));
            queue.Verify(q => q.AddEvent(
                It.Is<FolderEvent>(e => this.VerifyRemoteFolderEventCreation(e, name))
                ), Times.Once());
            queue.Verify(q => q.AddEvent(
                It.Is<CrawlRequestEvent>(e => e.LocalFolder.Name == name && e.RemoteFolder.Name == name)
                ), Times.Once());
        }

        private bool VerifyRemoteFolderEventUpdate(FolderEvent e, string folder, string name) {
            Assert.That(e.LocalFolder.FullName, Is.EqualTo(Path.Combine(folder, name)));
            Assert.That(e.RemoteFolder.Name, Is.EqualTo(name));
            Assert.That(e.Remote, Is.EqualTo(MetaDataChangeType.NONE));
            Assert.That(e.Local, Is.EqualTo(MetaDataChangeType.NONE));
            return true;
        }

        private bool VerifyCrawlRequest(CrawlRequestEvent e, string folder, string name) {
            Assert.That(e.LocalFolder.FullName, Is.EqualTo(Path.Combine(folder, name)));
            Assert.That(e.RemoteFolder.Name, Is.EqualTo(name));
            return true;
        }

        [Test, Category("Fast")]
        public void CrawlingFolderWith1RemoteoAndLocalFolder() {
            string localPath = "/";
            string name = "file";
            var queue = new Mock<ISyncEventQueue>();

            var remoteFolder = MockSessionUtil.CreateCmisFolder(folderNames: new List<string> { name });

            var localFolder = MockOfIFileSystemInfoFactoryUtil.CreateLocalFolder(localPath, folderNames: new List<string> { name });

            var crawler = GetCrawlerWithFakes(queue);
            var crawlEvent = new CrawlRequestEvent(localFolder.Object, remoteFolder.Object);
            Assert.True(crawler.Handle(crawlEvent));

            queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Exactly(2));
            queue.Verify(q => q.AddEvent(
                It.Is<FolderEvent>(e => this.VerifyRemoteFolderEventUpdate(e, localPath, name))
                ), Times.Once());
            queue.Verify(q => q.AddEvent(
                It.Is<CrawlRequestEvent>(e => this.VerifyCrawlRequest(e, localPath, name))
                ), Times.Once());
        }

        [Test, Category("Fast")]
        public void ParallelEventsTest() {
            string localPath = "/";
            var queue = new Mock<ISyncEventQueue>();

            var localFiles = new List<string> { "1", "3" };
            var remoteFiles = new List<string> { "1", "2" };
            var localFolders = new List<string> { "a", "c" };
            var remoteFolders = new List<string> { "a", "b" };

            var remoteFolder = MockSessionUtil.CreateCmisFolder(fileNames: remoteFiles, folderNames: remoteFolders);

            var localFolder = MockOfIFileSystemInfoFactoryUtil.CreateLocalFolder(localPath, fileNames: localFiles, folderNames: localFolders);

            var crawler = this.GetCrawlerWithFakes(queue);
            var crawlEvent = new CrawlRequestEvent(localFolder.Object, remoteFolder.Object);
            Assert.True(crawler.Handle(crawlEvent));

            queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Exactly(9));
            queue.Verify(q => q.AddEvent(It.IsAny<FileEvent>()), Times.Exactly(3));
            queue.Verify(q => q.AddEvent(It.IsAny<FolderEvent>()), Times.Exactly(3));
            queue.Verify(q => q.AddEvent(It.IsAny<CrawlRequestEvent>()), Times.Exactly(3));
        }
    }
}
