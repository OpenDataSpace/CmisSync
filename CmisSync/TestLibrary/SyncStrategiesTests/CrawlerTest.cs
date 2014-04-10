using System;
using System.Collections.Generic;
using System.IO;

using CmisSync.Lib.Events;
using CmisSync.Lib.Sync.Strategy;
using CmisSync.Lib.Storage;

using DotCMIS.Client;

using NUnit.Framework;

using Moq;

using TestLibrary.TestUtils;

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
            var factory = new Mock<IFileSystemInfoFactory>();
            var crawler = new Crawler(queue, remoteFolder, localFolder.Object, factory.Object);
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

        private Mock<IDirectoryInfo> CreateLocalFolder(string path, List<string> fileNames = null, List<string> folderNames = null) {
            var localFolder = new Mock<IDirectoryInfo>();
            localFolder.Setup(f => f.FullName).Returns(path);
            var fileList = new List<IFileInfo>();
            if(fileNames != null){
                foreach(var name in fileNames) {
                    var file = new Mock<IFileInfo>();
                    file.Setup(d => d.Name).Returns(name);
                    fileList.Add(file.Object);
                }
            }
            localFolder.Setup(f => f.GetFiles()).Returns(fileList.ToArray());
            var folderList = new List<IDirectoryInfo>();
            if(folderNames != null){
                foreach(var name in folderNames) {
                    var folder = new Mock<IDirectoryInfo>();
                    folder.Setup(d => d.Name).Returns(name);
                    folderList.Add(folder.Object);
                }
            }
            localFolder.Setup(f => f.GetDirectories()).Returns(folderList.ToArray());
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

            var remoteFolder = MockSessionUtil.CreateCmisFolder();

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

            var remoteFolder = MockSessionUtil.CreateCmisFolder(fileNames : new List<string> {name});

            var localFolder = CreateLocalFolder(localPath);

            var crawler = GetCrawlerWithFakes(queue);
            var crawlEvent = new CrawlRequestEvent(localFolder.Object, remoteFolder.Object);
            Assert.True(crawler.Handle(crawlEvent));

            queue.Verify(q => q.AddEvent(
                        It.Is<FileEvent>(e => VerifyRemoteFileEvent(e, localPath, name, MetaDataChangeType.CREATED, ContentChangeType.NONE))
                        ), Times.Once());
        } 
        
        [Test, Category("Fast")]
        public void CrawlingRemoteFolderWith1FileWithContentEmptyLocal () {
            string localPath = "/";
            string name = "file";
            var queue = new Mock<ISyncEventQueue>();

            var remoteFolder = MockSessionUtil.CreateCmisFolder(fileNames : new List<string> {name}, contentStream : true);

            var localFolder = CreateLocalFolder(localPath);

            var crawler = GetCrawlerWithFakes(queue);
            var crawlEvent = new CrawlRequestEvent(localFolder.Object, remoteFolder.Object);
            Assert.True(crawler.Handle(crawlEvent));

            queue.Verify(q => q.AddEvent(
                        It.Is<FileEvent>(e => VerifyRemoteFileEvent(e, localPath, name, MetaDataChangeType.CREATED, ContentChangeType.CREATED))
                        ), Times.Once());
        } 

        [Test, Category("Fast")]
        public void CrawlingFolderWith1FileRemoteAndLocal () {
            string localPath = "/";
            string name = "file";
            var queue = new Mock<ISyncEventQueue>();

            var remoteFolder = MockSessionUtil.CreateCmisFolder(fileNames : new List<string> {name});

            var localFolder = CreateLocalFolder(localPath, fileNames : new List<string> {name});

            var crawler = GetCrawlerWithFakes(queue);
            var crawlEvent = new CrawlRequestEvent(localFolder.Object, remoteFolder.Object);
            Assert.True(crawler.Handle(crawlEvent));

            queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Once());
            queue.Verify(q => q.AddEvent(
                        It.Is<FileEvent>(e => VerifyRemoteFileEvent(e, localPath, name, MetaDataChangeType.NONE, ContentChangeType.NONE))
                        ), Times.Once());
        } 

        [Test, Category("Fast")]
        public void CrawlingFolderWith1FileOnlyLocal () {
            string localPath = "/";
            string name = "file";
            var queue = new Mock<ISyncEventQueue>();

            var remoteFolder = MockSessionUtil.CreateCmisFolder();

            var localFolder = CreateLocalFolder(localPath, fileNames : new List<string> {name});

            var crawler = GetCrawlerWithFakes(queue);
            var crawlEvent = new CrawlRequestEvent(localFolder.Object, remoteFolder.Object);
            Assert.True(crawler.Handle(crawlEvent));

            queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Once());
            queue.Verify(q => q.AddEvent(
                        It.Is<FileEvent>(e => VerifyLocalFileEventCreated(e, localPath, name))
                        ), Times.Once());
        } 

        private bool VerifyLocalFolderEvent(FolderEvent e, string folder, string file, IFolder remoteFolder) {
            Assert.That(e.LocalFolder.FullName, Is.EqualTo(Path.Combine(folder, file)));
            Assert.That(e.RemoteFolder, Is.EqualTo(remoteFolder));
            Assert.That(e.Remote, Is.EqualTo(MetaDataChangeType.NONE));
            Assert.That(e.Local, Is.EqualTo(MetaDataChangeType.CREATED));
            return true;
        }

        [Test, Category("Fast")]
        public void CrawlingFolderWith1LocalFolder () {
            string localPath = "/";
            string name = "file";
            var queue = new Mock<ISyncEventQueue>();

            var remoteFolder = MockSessionUtil.CreateCmisFolder();

            var localFolder = CreateLocalFolder(localPath, folderNames : new List<string> {name});

            var crawler = GetCrawlerWithFakes(queue);
            var crawlEvent = new CrawlRequestEvent(localFolder.Object, remoteFolder.Object);
            Assert.True(crawler.Handle(crawlEvent));

            queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Once());
            queue.Verify(q => q.AddEvent(
                        It.Is<FolderEvent>(e => VerifyLocalFolderEvent(e, localPath, name, remoteFolder.Object))
                        ), Times.Once());
        } 

        private bool VerifyRemoteFolderEventCreation(FolderEvent e, string folder, string name) {
            Assert.That(e.LocalFolder.FullName, Is.EqualTo(folder));
            Assert.That(e.RemoteFolder.Name, Is.EqualTo(name));
            Assert.That(e.Remote, Is.EqualTo(MetaDataChangeType.CREATED));
            Assert.That(e.Local, Is.EqualTo(MetaDataChangeType.NONE));           
            Assert.That(e.Recursive, Is.EqualTo(true));
            return true;
        }

        [Test, Category("Fast")]
        public void CrawlingFolderWith1RemoteFolder () {
            string localPath = "/";
            string name = "file";
            var queue = new Mock<ISyncEventQueue>();

            var remoteFolder = MockSessionUtil.CreateCmisFolder(folderNames : new List<string> {name});

            var localFolder = CreateLocalFolder(localPath);

            var crawler = GetCrawlerWithFakes(queue);
            var crawlEvent = new CrawlRequestEvent(localFolder.Object, remoteFolder.Object);
            Assert.True(crawler.Handle(crawlEvent));

            queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Once());
            queue.Verify(q => q.AddEvent(
                        It.Is<FolderEvent>(e => VerifyRemoteFolderEventCreation(e, localPath, name))
                        ), Times.Once());
        } 

        private bool VerifyRemoteFolderEventUpdate(FolderEvent e, string folder, string name) {
            Assert.That(e.LocalFolder.FullName, Is.EqualTo(Path.Combine(folder, name)));
            Assert.That(e.RemoteFolder.Name, Is.EqualTo(name));
            Assert.That(e.Remote, Is.EqualTo(MetaDataChangeType.NONE));
            Assert.That(e.Local, Is.EqualTo(MetaDataChangeType.NONE));           
            Assert.That(e.Recursive, Is.EqualTo(false));
            return true;
        }

        private bool VerifyCrawlRequest(CrawlRequestEvent e, string folder, string name) {
            Assert.That(e.LocalFolder.FullName, Is.EqualTo(Path.Combine(folder, name)));
            Assert.That(e.RemoteFolder.Name, Is.EqualTo(name));
            return true;
        }

        [Test, Category("Fast")]
        public void CrawlingFolderWith1RemoteoAndLocalFolder () {
            string localPath = "/";
            string name = "file";
            var queue = new Mock<ISyncEventQueue>();

            var remoteFolder = MockSessionUtil.CreateCmisFolder(folderNames : new List<string> {name});

            var localFolder = CreateLocalFolder(localPath, folderNames : new List<string> {name});

            var crawler = GetCrawlerWithFakes(queue);
            var crawlEvent = new CrawlRequestEvent(localFolder.Object, remoteFolder.Object);
            Assert.True(crawler.Handle(crawlEvent));

            queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Exactly(2));
            queue.Verify(q => q.AddEvent(
                        It.Is<FolderEvent>(e => VerifyRemoteFolderEventUpdate(e, localPath, name))
                        ), Times.Once());
            queue.Verify(q => q.AddEvent(
                        It.Is<CrawlRequestEvent>(e => VerifyCrawlRequest(e, localPath, name))
                        ), Times.Once());
        } 


        [Test, Category("Fast")]
        public void ParallelEventsTest () {
            string localPath = "/";
            var queue = new Mock<ISyncEventQueue>();

            var localFiles = new List<string> {"1","3"};
            var remoteFiles = new List<string> {"1","2"};
            var localFolders = new List<string> {"a","c"};
            var remoteFolders = new List<string> {"a","b"};

            var remoteFolder = MockSessionUtil.CreateCmisFolder(fileNames : remoteFiles, folderNames : remoteFolders);

            var localFolder = CreateLocalFolder(localPath, fileNames : localFiles, folderNames : localFolders);

            var crawler = GetCrawlerWithFakes(queue);
            var crawlEvent = new CrawlRequestEvent(localFolder.Object, remoteFolder.Object);
            Assert.True(crawler.Handle(crawlEvent));

            queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Exactly(7));
            queue.Verify(q => q.AddEvent(It.IsAny<FileEvent>()), Times.Exactly(3));
            queue.Verify(q => q.AddEvent(It.IsAny<FolderEvent>()), Times.Exactly(3));
            queue.Verify(q => q.AddEvent(It.IsAny<CrawlRequestEvent>()), Times.Exactly(1));
        }
    }
}

