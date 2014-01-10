using System;
using System.IO;

using CmisSync.Lib.Events;
using CmisSync.Lib.Sync.Strategy;

using NUnit.Framework;

using Moq;

namespace TestLibrary.SyncStrategiesTests
{
    [TestFixture]
    public class WatcherTest
    {

        private string localPath; 
        private DirectoryInfo localFolder;
        private FileInfo localFile;

        [SetUp]
        public void SetUp() {
            localPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            localFolder = new DirectoryInfo(localPath);
            localFolder.Create();
            localFile = new FileInfo(Path.Combine(localFolder.FullName, Path.GetRandomFileName()));
            localFile.Create();
        }

        [TearDown]
        public void TearDown() {
            localFile.Refresh();
            localFile.Delete();
            localFolder.Refresh();
            localFolder.Delete();
        }

        [Test, Category("Fast")]
        public void ConstructorTest() {
            var fswatcher = new Mock<FileSystemWatcher>(localFolder.FullName){CallBase = true}.Object;
            var manager = new Mock<SyncEventManager>().Object;
            var queue = new Mock<SyncEventQueue>(manager).Object;
            new Watcher(fswatcher, queue);
            try{
                new Watcher(null, queue);
                Assert.Fail();
            }catch (ArgumentNullException) {}
            try {
                new Watcher(fswatcher, null);
                Assert.Fail ();
            }catch(ArgumentNullException) {}
            try{
                fswatcher = new Mock<FileSystemWatcher>() {CallBase = true}.Object;
                new Watcher(fswatcher, queue);
                Assert.Fail();
            }catch (ArgumentException) {}
        }

        [Test, Category("Fast")]
        public void IgnoreWrongEventsTest() {
            var fswatcher = new Mock<FileSystemWatcher>(localFolder.FullName){CallBase = true}.Object;
            var manager = new Mock<SyncEventManager>().Object;
            var queue = new Mock<SyncEventQueue>(manager);
            queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Never());
            var watcher = new Watcher(fswatcher, queue.Object);
            Assert.False(watcher.Handle(new Mock<ISyncEvent>().Object));
            Assert.False(watcher.Handle(new Mock<FileEvent>(new FileInfo("test"), null, null){CallBase = false}.Object));
        }

        [Test, Category("Fast")]
        public void HandleFSFileAddedEventsTest () {
            var fswatcher = new Mock<FileSystemWatcher>(localFolder.FullName){CallBase = true}.Object;
            var manager = new Mock<SyncEventManager>().Object;
            var queue = new Mock<SyncEventQueue>(manager);
            AbstractFolderEvent returnedFileEvent = null;
            queue.Setup(q => q.AddEvent(It.IsAny<AbstractFolderEvent>()))
                .Callback((ISyncEvent f) => returnedFileEvent = f as AbstractFolderEvent);
            var watcher = new Watcher(fswatcher, queue.Object);
            var fileCreatedFSEvent = new FSEvent(WatcherChangeTypes.Created, localFile.FullName);
            Assert.True(watcher.Handle(fileCreatedFSEvent));
            Assert.AreEqual(MetaDataChangeType.CREATED, returnedFileEvent.Local);
            Assert.AreEqual(ContentChangeType.CREATED, (returnedFileEvent as FileEvent).LocalContent);
            Assert.AreEqual(localFile.FullName, (returnedFileEvent as FileEvent).LocalFile.FullName);
            Assert.IsNull((returnedFileEvent as FileEvent).RemoteFile);
            Assert.AreEqual(MetaDataChangeType.NONE, (returnedFileEvent as FileEvent).Remote);
            Assert.AreEqual(ContentChangeType.NONE, (returnedFileEvent as FileEvent).RemoteContent);
        }

        [Test, Category("Fast")]
        public void HandleFSFileChangedEventsTest () {
            var fswatcher = new Mock<FileSystemWatcher>(localFolder.FullName){CallBase = true}.Object;
            var manager = new Mock<SyncEventManager>().Object;
            var queue = new Mock<SyncEventQueue>(manager);
            AbstractFolderEvent returnedFileEvent = null;
            queue.Setup(q => q.AddEvent(It.IsAny<AbstractFolderEvent>()))
                .Callback((ISyncEvent f) => returnedFileEvent = f as AbstractFolderEvent);
            var watcher = new Watcher(fswatcher, queue.Object);
            var fileChangedFSEvent = new FSEvent(WatcherChangeTypes.Changed, localFile.FullName);
            Assert.True(watcher.Handle(fileChangedFSEvent));
            Assert.AreEqual(MetaDataChangeType.NONE, returnedFileEvent.Local);
            Assert.AreEqual(ContentChangeType.CHANGED, (returnedFileEvent as FileEvent).LocalContent);
            Assert.AreEqual(localFile.FullName, (returnedFileEvent as FileEvent).LocalFile.FullName);
            Assert.IsNull((returnedFileEvent as FileEvent).RemoteFile);
            Assert.AreEqual(MetaDataChangeType.NONE, (returnedFileEvent as FileEvent).Remote);
            Assert.AreEqual(ContentChangeType.NONE, (returnedFileEvent as FileEvent).RemoteContent);
        }

        [Test, Category("Fast")]
        public void HandleFSFileRemovedEventsTest () {
            var fswatcher = new Mock<FileSystemWatcher>(localFolder.FullName){CallBase = true}.Object;
            var manager = new Mock<SyncEventManager>().Object;
            var queue = new Mock<SyncEventQueue>(manager);
            AbstractFolderEvent returnedFileEvent = null;
            queue.Setup(q => q.AddEvent(It.IsAny<AbstractFolderEvent>()))
                .Callback((ISyncEvent f) => returnedFileEvent = f as AbstractFolderEvent);
            var watcher = new Watcher(fswatcher, queue.Object);
            var fileRemovedFSEvent = new FSEvent(WatcherChangeTypes.Deleted, localFile.FullName);
            Assert.True(watcher.Handle(fileRemovedFSEvent));
            Assert.AreEqual(MetaDataChangeType.DELETED, returnedFileEvent.Local);
            Assert.AreEqual(ContentChangeType.DELETED, (returnedFileEvent as FileEvent).LocalContent);
            Assert.AreEqual(localFile.FullName, (returnedFileEvent as FileEvent).LocalFile.FullName);
            Assert.IsNull((returnedFileEvent as FileEvent).RemoteFile);
            Assert.AreEqual(MetaDataChangeType.NONE, (returnedFileEvent as FileEvent).Remote);
            Assert.AreEqual(ContentChangeType.NONE, (returnedFileEvent as FileEvent).RemoteContent);
        }

        [Test, Category("Fast")]
        public void HandleFSFileRenamedEventsTest () {
            string oldpath = Path.Combine(localFolder.FullName, Path.GetRandomFileName());
            var fswatcher = new Mock<FileSystemWatcher>(localFolder.FullName){CallBase = true}.Object;
            var manager = new Mock<SyncEventManager>().Object;
            var queue = new Mock<SyncEventQueue>(manager);
            AbstractFolderEvent returnedFileEvent = null;
            queue.Setup(q => q.AddEvent(It.IsAny<AbstractFolderEvent>()))
                .Callback((ISyncEvent f) => returnedFileEvent = f as AbstractFolderEvent);
            var watcher = new Watcher(fswatcher, queue.Object);
            var fileRenamedFSEvent = new FSMovedEvent(oldpath, localFile.FullName);
            Assert.True(watcher.Handle(fileRenamedFSEvent));
            Assert.AreEqual(MetaDataChangeType.MOVED, returnedFileEvent.Local);
            Assert.AreEqual(ContentChangeType.NONE, (returnedFileEvent as FileMovedEvent).LocalContent);
            Assert.AreEqual(localFile.FullName, (returnedFileEvent as FileMovedEvent).LocalFile.FullName);
            Assert.AreEqual(oldpath, (returnedFileEvent as FileMovedEvent).OldLocalFile.FullName);
            Assert.IsNull((returnedFileEvent as FileEvent).RemoteFile);
            Assert.AreEqual(MetaDataChangeType.NONE, (returnedFileEvent as FileMovedEvent).Remote);
            Assert.AreEqual(ContentChangeType.NONE, (returnedFileEvent as FileMovedEvent).RemoteContent);
        }

        [Test, Category("Fast")]
        public void HandleFSFolderAddedEventsTest () {
            var fswatcher = new Mock<FileSystemWatcher>(localFolder.FullName){CallBase = true}.Object;
            var manager = new Mock<SyncEventManager>().Object;
            var queue = new Mock<SyncEventQueue>(manager);
            AbstractFolderEvent returnedFileEvent = null;
            queue.Setup(q => q.AddEvent(It.IsAny<AbstractFolderEvent>()))
                .Callback((ISyncEvent f) => returnedFileEvent = f as AbstractFolderEvent);
            var watcher = new Watcher(fswatcher, queue.Object);
            var folderCreatedFSEvent = new FSEvent(WatcherChangeTypes.Created, localFolder.FullName);
            Assert.True(watcher.Handle(folderCreatedFSEvent));
            Assert.AreEqual(MetaDataChangeType.CREATED, returnedFileEvent.Local);
            Assert.AreEqual(localFolder.FullName, (returnedFileEvent as FolderEvent).LocalFolder.FullName);
            Assert.IsNull((returnedFileEvent as FolderEvent).RemoteFolder);
            Assert.AreEqual(MetaDataChangeType.NONE, (returnedFileEvent as FolderEvent).Remote);
        }

        [Test, Category("Fast")]
        public void HandleFSFolderChangedEventsTest () {
            var fswatcher = new Mock<FileSystemWatcher>(localFolder.FullName){CallBase = true}.Object;
            var manager = new Mock<SyncEventManager>().Object;
            var queue = new Mock<SyncEventQueue>(manager);
            AbstractFolderEvent returnedFileEvent = null;
            queue.Setup(q => q.AddEvent(It.IsAny<AbstractFolderEvent>()))
                .Callback((ISyncEvent f) => returnedFileEvent = f as AbstractFolderEvent);
            var watcher = new Watcher(fswatcher, queue.Object);
            var folderChangedFSEvent = new FSEvent(WatcherChangeTypes.Changed, localFolder.FullName);
            Assert.True(watcher.Handle(folderChangedFSEvent));
            Assert.AreEqual(MetaDataChangeType.CHANGED, returnedFileEvent.Local);
            Assert.AreEqual(localFolder.FullName, (returnedFileEvent as FolderEvent).LocalFolder.FullName);
            Assert.IsNull((returnedFileEvent as FolderEvent).RemoteFolder);
            Assert.AreEqual(MetaDataChangeType.NONE, (returnedFileEvent as FolderEvent).Remote);
        }

        [Test, Category("Fast")]
        public void HandleFSFolderRemovedEventsTest () {
            var fswatcher = new Mock<FileSystemWatcher>(localFolder.FullName){CallBase = true}.Object;
            var manager = new Mock<SyncEventManager>().Object;
            var queue = new Mock<SyncEventQueue>(manager);
            AbstractFolderEvent returnedFileEvent = null;
            queue.Setup(q => q.AddEvent(It.IsAny<AbstractFolderEvent>()))
                .Callback((ISyncEvent f) => returnedFileEvent = f as AbstractFolderEvent);
            var watcher = new Watcher(fswatcher, queue.Object);
            var folderRemovedFSEvent = new FSEvent(WatcherChangeTypes.Deleted, localFolder.FullName);
            Assert.True(watcher.Handle(folderRemovedFSEvent));
            Assert.AreEqual(MetaDataChangeType.DELETED, returnedFileEvent.Local);
            Assert.AreEqual(localFolder.FullName, (returnedFileEvent as FolderEvent).LocalFolder.FullName);
            Assert.IsNull((returnedFileEvent as FolderEvent).RemoteFolder);
            Assert.AreEqual(MetaDataChangeType.NONE, (returnedFileEvent as FolderEvent).Remote);
        }

        [Test, Category("Fast")]
        public void HandleFSFolderRenamedEventsTest () {
            string oldpath = Path.Combine(localFolder.FullName, Path.GetRandomFileName());
            var fswatcher = new Mock<FileSystemWatcher>(localFolder.FullName){CallBase = true}.Object;
            var manager = new Mock<SyncEventManager>().Object;
            var queue = new Mock<SyncEventQueue>(manager);
            AbstractFolderEvent returnedFileEvent = null;
            queue.Setup(q => q.AddEvent(It.IsAny<AbstractFolderEvent>()))
                .Callback((ISyncEvent f) => returnedFileEvent = f as AbstractFolderEvent);
            var watcher = new Watcher(fswatcher, queue.Object);
            var folderRenamedFSEvent = new FSMovedEvent(oldpath, localFolder.FullName);
            Assert.True(watcher.Handle(folderRenamedFSEvent));
            Assert.AreEqual(MetaDataChangeType.MOVED, returnedFileEvent.Local);
            Assert.AreEqual(localFolder.FullName, (returnedFileEvent as FolderEvent).LocalFolder.FullName);
            Assert.AreEqual(oldpath, (returnedFileEvent as FolderMovedEvent).OldLocalFolder.FullName);
            Assert.IsNull((returnedFileEvent as FolderMovedEvent).RemoteFolder);
            Assert.IsNull((returnedFileEvent as FolderMovedEvent).OldRemoteFolderPath);
            Assert.AreEqual(MetaDataChangeType.NONE, (returnedFileEvent as FolderEvent).Remote);
        }


        [Ignore]
        [Test, Category("Fast")]
        public void ReportFSFileAddedEventTest () {
            var fswatcher = new Mock<FileSystemWatcher>(localFolder.FullName){CallBase = false};
            var manager = new Mock<SyncEventManager>().Object;
            var queue = new Mock<SyncEventQueue>(manager);
            FSEvent returnedFSEvent = null;
            queue.Setup(q => q.AddEvent(It.IsAny<FSEvent>()))
                .Callback((ISyncEvent file) => returnedFSEvent = file as FSEvent);
            var watcher = new Watcher(fswatcher.Object, queue.Object);
            fswatcher.Raise(m => m.Created += null, null, new FileSystemEventArgs(WatcherChangeTypes.Created,localFolder.FullName, localFile.Name));
            Assert.AreEqual(localFile.FullName, returnedFSEvent.Path);
            Assert.AreEqual(WatcherChangeTypes.Created, returnedFSEvent.Type);
        }

        [Ignore]
        [Test, Category("Fast")]
        public void ReportFSFileChangedEventTest () {
            var fswatcher = new Mock<FileSystemWatcher>(localFolder.FullName){CallBase = true}.Object;
            var manager = new Mock<SyncEventManager>().Object;
            var queue = new Mock<SyncEventQueue>(manager);
            FSEvent returnedFSEvent = null;
            queue.Setup(q => q.AddEvent(It.IsAny<AbstractFolderEvent>()))
                .Callback((ISyncEvent file) => returnedFSEvent = file as FSEvent);
            var watcher = new Watcher(fswatcher, queue.Object);
            
            Assert.Fail ();
        }

        [Ignore]
        [Test, Category("Fast")]
        public void ReportFSFileRenamedEventTest () {
            var fswatcher = new Mock<FileSystemWatcher>(localFolder.FullName){CallBase = true}.Object;
            var manager = new Mock<SyncEventManager>().Object;
            var queue = new Mock<SyncEventQueue>(manager);
            FSEvent returnedFSEvent = null;
            queue.Setup(q => q.AddEvent(It.IsAny<AbstractFolderEvent>()))
                .Callback((ISyncEvent file) => returnedFSEvent = file as FSEvent);
            var watcher = new Watcher(fswatcher, queue.Object);
            
            Assert.Fail ();
        }

        [Ignore]
        [Test, Category("Fast")]
        public void ReportFSFileRemovedEventTest () {
            var fswatcher = new Mock<FileSystemWatcher>(localFolder.FullName){CallBase = true}.Object;
            var manager = new Mock<SyncEventManager>().Object;
            var queue = new Mock<SyncEventQueue>(manager);
            FSEvent returnedFSEvent = null;
            queue.Setup(q => q.AddEvent(It.IsAny<AbstractFolderEvent>()))
                .Callback((ISyncEvent file) => returnedFSEvent = file as FSEvent);
            var watcher = new Watcher(fswatcher, queue.Object);
            
            Assert.Fail ();
        }

        [Ignore]
        [Test, Category("Fast")]
        public void ReportFSFolderAddedEventTest () {
            var fswatcher = new Mock<FileSystemWatcher>(localFolder.FullName){CallBase = true}.Object;
            var manager = new Mock<SyncEventManager>().Object;
            var queue = new Mock<SyncEventQueue>(manager);
            FSEvent returnedFSEvent = null;
            queue.Setup(q => q.AddEvent(It.IsAny<AbstractFolderEvent>()))
                .Callback((ISyncEvent file) => returnedFSEvent = file as FSEvent);
            var watcher = new Watcher(fswatcher, queue.Object);
            
            Assert.Fail ();
        }

        [Ignore]
        [Test, Category("Fast")]
        public void ReportFSFolderChangedEventTest () {
            var fswatcher = new Mock<FileSystemWatcher>(localFolder.FullName){CallBase = true}.Object;
            var manager = new Mock<SyncEventManager>().Object;
            var queue = new Mock<SyncEventQueue>(manager);
            FSEvent returnedFSEvent = null;
            queue.Setup(q => q.AddEvent(It.IsAny<AbstractFolderEvent>()))
                .Callback((ISyncEvent file) => returnedFSEvent = file as FSEvent);
            var watcher = new Watcher(fswatcher, queue.Object);
            
            Assert.Fail ();
        }

        [Ignore]
        [Test, Category("Fast")]
        public void ReportFSFolderRemovedEventTest () {
            var fswatcher = new Mock<FileSystemWatcher>(localFolder.FullName){CallBase = true}.Object;
            var manager = new Mock<SyncEventManager>().Object;
            var queue = new Mock<SyncEventQueue>(manager);
            FSEvent returnedFSEvent = null;
            queue.Setup(q => q.AddEvent(It.IsAny<AbstractFolderEvent>()))
                .Callback((ISyncEvent file) => returnedFSEvent = file as FSEvent);
            var watcher = new Watcher(fswatcher, queue.Object);
            
            Assert.Fail ();
        }

        [Ignore]
        [Test, Category("Fast")]
        public void ReportFSFolderRenamedEventTest () {
            var fswatcher = new Mock<FileSystemWatcher>(localFolder.FullName){CallBase = true}.Object;
            var manager = new Mock<SyncEventManager>().Object;
            var queue = new Mock<SyncEventQueue>(manager);
            FSEvent returnedFSEvent = null;
            queue.Setup(q => q.AddEvent(It.IsAny<AbstractFolderEvent>()))
                .Callback((ISyncEvent file) => returnedFSEvent = file as FSEvent);
            var watcher = new Watcher(fswatcher, queue.Object);
            
            Assert.Fail ();
        }
    }
}

