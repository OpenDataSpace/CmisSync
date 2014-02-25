using System;
using System.Collections.Generic;
using System.IO;

using CmisSync.Lib.Events;
using CmisSync.Lib.Sync.Strategy;
using CmisSync.Lib.Storage;

using DotCMIS.Client;

using NUnit.Framework;

using Moq;

namespace TestLibrary.SyncStrategiesTests
{
    [TestFixture]
    public class CrawlerTest
    {
        private string localPath; 
        private IDirectoryInfo localFolder;
        private DirectoryInfo realLocalFolder;

        [SetUp]
        public void SetUp() {
            localPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            realLocalFolder = new DirectoryInfo(localPath);
            localFolder = new DirectoryInfoWrapper(realLocalFolder);
            localFolder.Create();
        }

        [TearDown]
        public void TearDown() {
            localFolder.Refresh();
            realLocalFolder.Delete();
        }

        [Test, Category("Fast")]
        public void ConstructorWithValidInputTest () {
            var queue = new Mock<ISyncEventQueue>().Object;
            var remoteFolder = new Mock<IFolder>().Object;
            var localFolder = new Mock<IDirectoryInfo>();
            var crawler = new Crawler(queue, remoteFolder, localFolder.Object);
            Assert.AreEqual(Crawler.CRAWLER_PRIORITY, crawler.Priority);
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorFailsOnNullQueueTest ()
        {
            var remoteFolder = new Mock<IFolder>().Object;
            var localFolder = new Mock<IDirectoryInfo>();

            new Crawler(null, remoteFolder, localFolder.Object);
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorFailsOnNullLocalFolderTest ()
        {
            var queue = new Mock<ISyncEventQueue>().Object;
            var remoteFolder = new Mock<IFolder>().Object;
            new Crawler(queue, remoteFolder, null);
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorFailsOnNullRemoteFolderTest ()
        {
            var queue = new Mock<ISyncEventQueue>().Object;
            new Crawler(queue, null, localFolder);
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

        [Ignore]
        [Test, Category("Fast")]
        public void CrawlingSpecificFolderTest() {
            Assert.Fail("TODO");
        }

        [Ignore]
        [Test, Category("Fast")]
        public void CrawlingBaseFolderTest() {
            Assert.Fail("TODO");
        }

        private Mock<IFolder> CreateFolder(List<string> fileNames, int folderCount) {
            var remoteFolder = new Mock<IFolder>();
            var remoteChildren = new Mock<IItemEnumerable<ICmisObject>>();
            var list = new List<ICmisObject>();
            foreach(var name in fileNames) {
                var doc = new Mock<IDocument>();
                doc.Setup(d => d.Name).Returns(name);
                list.Add(doc.Object);
            }
            remoteChildren.Setup(r => r.GetEnumerator()).Returns(list.GetEnumerator());
            remoteFolder.Setup(r => r.GetChildren()).Returns(remoteChildren.Object);
            return remoteFolder;
        }

        private Mock<IDirectoryInfo> CreateLocalFolder(string path, List<string> fileNames) {
            var localFolder = new Mock<IDirectoryInfo>();
            localFolder.Setup(f => f.FullName).Returns(path);
            var list = new List<IFileInfo>();
            foreach(var name in fileNames) {
                var file = new Mock<IFileInfo>();
                file.Setup(d => d.Name).Returns(name);
                list.Add(file.Object);
            }
            localFolder.Setup(f => f.GetFiles()).Returns(list.ToArray());
            return localFolder;

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

            var remoteFolder = CreateFolder(new List<string>(), 0);

            var localFolder = new Mock<IDirectoryInfo>();

            var crawler = new Crawler(queue.Object, remoteFolder.Object, localFolder.Object);
            var startEvent = new StartNextSyncEvent(true);
            Assert.True(crawler.Handle(startEvent));

            queue.Verify(q => q.AddEvent(It.IsAny<FullSyncCompletedEvent>()),Times.Once());
            //this and only this should be added
            queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()),Times.Once());

        }

        private bool VerifyRemoteFileEvent(FileEvent e, string folder, string file, MetaDataChangeType metaType, ContentChangeType changeType){
            VerifyFileEvent(e, folder, file);
            Assert.That(e.RemoteFile, Is.Not.Null);
            Assert.That(e.Remote, Is.EqualTo(metaType));
            Assert.That(e.RemoteContent, Is.EqualTo(changeType));
            Assert.That(e.Local, Is.EqualTo(MetaDataChangeType.NONE));
            Assert.That(e.LocalContent, Is.EqualTo(ContentChangeType.NONE));
            return true;
        }

        private bool VerifyLocalFileEventCreated(FileEvent e, string folder, string file) {
            VerifyFileEvent(e, folder, file);
            Assert.That(e.RemoteFile, Is.Null);
            Assert.That(e.Remote, Is.EqualTo(MetaDataChangeType.NONE));
            Assert.That(e.RemoteContent, Is.EqualTo(ContentChangeType.NONE));
            Assert.That(e.Local, Is.EqualTo(MetaDataChangeType.CREATED));
            Assert.That(e.LocalContent, Is.EqualTo(ContentChangeType.CREATED));
            return true;
        }
      

        private bool VerifyFileEvent(FileEvent e, string folder, string file){
            Assert.That(e.LocalFile.FullName, Is.EqualTo(Path.Combine(folder, file)));
            Assert.That(e.LocalParentDirectory.FullName, Is.EqualTo(folder));
                    
            return true;
        }

        [Test, Category("Fast")]
        public void CrawlingRemoteFolderWith1FileEmptyLocal () {
            string localPath = "/";
            string name = "file";
            var queue = new Mock<ISyncEventQueue>();

            var remoteFolder = CreateFolder(new List<string> {name}, 0);

            var localFolder = CreateLocalFolder(localPath, new List<string>());

            var crawler = GetCrawlerWithFakes(queue);
            var crawlEvent = new CrawlRequestEvent(localFolder.Object, remoteFolder.Object);
            Assert.True(crawler.Handle(crawlEvent));

            queue.Verify(q => q.AddEvent(
                        It.Is<FileEvent>(e => VerifyRemoteFileEvent(e, localPath, name, MetaDataChangeType.CREATED, ContentChangeType.NONE))
                        ), Times.Once());
        } 

        [Test, Category("Fast")]
        public void CrawlingRemoteFolderWith1FileRemoteAndLocal () {
            string localPath = "/";
            string name = "file";
            var queue = new Mock<ISyncEventQueue>();

            var remoteFolder = CreateFolder(new List<string> {name}, 0);

            var localFolder = CreateLocalFolder(localPath, new List<string> {name});

            var crawler = GetCrawlerWithFakes(queue);
            var crawlEvent = new CrawlRequestEvent(localFolder.Object, remoteFolder.Object);
            Assert.True(crawler.Handle(crawlEvent));

            queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Once());
            queue.Verify(q => q.AddEvent(
                        It.Is<FileEvent>(e => VerifyRemoteFileEvent(e, localPath, name, MetaDataChangeType.NONE, ContentChangeType.NONE))
                        ), Times.Once());
        } 

        [Test, Category("Fast")]
        public void CrawlingRemoteFolderWith1FileOnlyLocal () {
            string localPath = "/";
            string name = "file";
            var queue = new Mock<ISyncEventQueue>();

            var remoteFolder = CreateFolder(new List<string>() , 0);

            var localFolder = CreateLocalFolder(localPath, new List<string> {name});

            var crawler = GetCrawlerWithFakes(queue);
            var crawlEvent = new CrawlRequestEvent(localFolder.Object, remoteFolder.Object);
            Assert.True(crawler.Handle(crawlEvent));

            queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Once());
            queue.Verify(q => q.AddEvent(
                        It.Is<FileEvent>(e => VerifyLocalFileEventCreated(e, localPath, name))
                        ), Times.Once());
        } 
    }
}

