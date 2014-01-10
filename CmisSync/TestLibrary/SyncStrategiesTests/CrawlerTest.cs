using System;
using System.IO;

using CmisSync.Lib.Events;
using CmisSync.Lib.Sync.Strategy;

using DotCMIS.Client;

using NUnit.Framework;

using Moq;

namespace TestLibrary.SyncStrategiesTests
{
    [TestFixture]
    public class CrawlerTest
    {
        private string localPath; 
        private DirectoryInfo localFolder;

        [SetUp]
        public void SetUp() {
            localPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            localFolder = new DirectoryInfo(localPath);
            localFolder.Create();
        }

        [TearDown]
        public void TearDown() {
            localFolder.Refresh();
            localFolder.Delete();
        }

        [Test, Category("Fast")]
        public void ConstructorTest () {
            var queuemanager = new Mock<SyncEventManager>().Object;
            var queue = new Mock<SyncEventQueue>(queuemanager).Object;
            var remoteFolder = new Mock<IFolder>().Object;
            var localFolder = new DirectoryInfo("test");
            var crawler = new Crawler(queue, remoteFolder, localFolder);
            Assert.AreEqual(Crawler.CRAWLER_PRIORITY, crawler.Priority);
            try {
                new Crawler(null, remoteFolder, localFolder);
                Assert.Fail ();
            }catch(ArgumentNullException){}
            try {
                new Crawler(queue, null, localFolder);
                Assert.Fail ();
            }catch(ArgumentNullException){}
            try {
                new Crawler(queue, remoteFolder, null);
                Assert.Fail ();
            }catch(ArgumentNullException){}
        }

        [Test, Category("Fast")]
        public void IgnoreWrongEventsTest() {
            var queuemanager = new Mock<SyncEventManager>().Object;
            var queue = new Mock<SyncEventQueue>(queuemanager).Object;
            var remoteFolder = new Mock<IFolder>().Object;
            var localFolder = new DirectoryInfo("test");
            var wrongEvent = new Mock<ISyncEvent>().Object;
            var crawler = new Crawler(queue, remoteFolder, localFolder);
            Assert.False(crawler.Handle(wrongEvent));
        }

        [Ignore]
        [Test, Category("Fast")]
        public void CrawlingSpecificFolderTest() {
            var manager = new SyncEventManager();
            using (var queue = new SyncEventQueue(manager)) {
     //           manager.AddEventHandler(new Crawler());
            }
        }

        [Ignore]
        [Test, Category("Fast")]
        public void CrawlingBaseFolderTest() {

        }

        [Ignore]
        [Test, Category("Fast")]
        public void CrawlingEmptyFoldersTest() {
            var queuemanager = new Mock<SyncEventManager>().Object;
            var queue = new Mock<SyncEventQueue>(queuemanager).Object;
            var remoteFolder = new Mock<IFolder>();
            var remoteChildren = new Mock<IItemEnumerable<ICmisObject>>();
//            remoteChildren.Setup(remoteChildren => remoteChildren.)
//            remoteFolder.Setup(folder => folder.GetChildren()).Returns()
        }
    }
}

