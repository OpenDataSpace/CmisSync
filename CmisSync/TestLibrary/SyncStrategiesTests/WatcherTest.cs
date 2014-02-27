using System;
using System.IO;

using CmisSync.Lib.Events;
using CmisSync.Lib.Storage;
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
        private DirectoryInfo localSubFolder;
        private Mock<ISyncEventQueue> queue;
        private AbstractFolderEvent returnedFileEvent;
        private AbstractFolderEvent returnedFolderEvent;


        [SetUp]
        public void SetUp() {
            localPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            localFolder = new DirectoryInfo(localPath);
            localFolder.Create();
            localSubFolder = new DirectoryInfo(Path.Combine(localFolder.FullName, Path.GetRandomFileName()));
            localSubFolder.Create();
            localFile = new FileInfo(Path.Combine(localFolder.FullName, Path.GetRandomFileName()));
            using(localFile.Create());
            queue = new Mock<ISyncEventQueue>();
            returnedFileEvent = null;
            returnedFolderEvent = null;
        }

        [TearDown]
        public void TearDown() {
            localFile.Refresh();
            if(localFile.Exists)
                localFile.Delete();
            localSubFolder.Refresh();
            if(localSubFolder.Exists)
                localSubFolder.Delete(true);
            localFolder.Refresh();
            if(localFolder.Exists)
                localFolder.Delete(true);
        }

        [Test, Category("Fast")]
        public void IgnoreWrongEventsTest() {
            queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Never());
            using (var watcher = new Watcher(queue.Object))
            {
                Assert.False(watcher.Handle(new Mock<ISyncEvent>().Object));
                Assert.False(watcher.Handle(new Mock<FileEvent>(new Mock<IFileInfo>().Object, null, null){CallBase = false}.Object));
            }
        }

        [Test, Category("Fast")]
        public void HandleFSFileAddedEventsTest () {
            queue.Setup(q => q.AddEvent(It.IsAny<AbstractFolderEvent>()))
                .Callback((ISyncEvent f) => returnedFileEvent = f as AbstractFolderEvent);
            using (var watcher = new Watcher(queue.Object))
            {
                var fileCreatedFSEvent = new FSEvent(WatcherChangeTypes.Created, localFile.FullName);
                Assert.True(watcher.Handle(fileCreatedFSEvent));
                Assert.AreEqual(MetaDataChangeType.CREATED, returnedFileEvent.Local);
                Assert.AreEqual(ContentChangeType.CREATED, (returnedFileEvent as FileEvent).LocalContent);
                Assert.AreEqual(localFile.FullName, (returnedFileEvent as FileEvent).LocalFile.FullName);
                Assert.IsNull((returnedFileEvent as FileEvent).RemoteFile);
                Assert.AreEqual(MetaDataChangeType.NONE, (returnedFileEvent as FileEvent).Remote);
                Assert.AreEqual(ContentChangeType.NONE, (returnedFileEvent as FileEvent).RemoteContent);
            }
        }

        [Test, Category("Fast")]
        public void HandleFSFileChangedEventsTest () {
            queue.Setup(q => q.AddEvent(It.IsAny<AbstractFolderEvent>()))
                .Callback((ISyncEvent f) => returnedFileEvent = f as AbstractFolderEvent);
            using (var watcher = new Watcher(queue.Object))
            {
                var fileChangedFSEvent = new FSEvent(WatcherChangeTypes.Changed, localFile.FullName);
                Assert.True(watcher.Handle(fileChangedFSEvent));
                Assert.AreEqual(MetaDataChangeType.NONE, returnedFileEvent.Local);
                Assert.AreEqual(ContentChangeType.CHANGED, (returnedFileEvent as FileEvent).LocalContent);
                Assert.AreEqual(localFile.FullName, (returnedFileEvent as FileEvent).LocalFile.FullName);
                Assert.IsNull((returnedFileEvent as FileEvent).RemoteFile);
                Assert.AreEqual(MetaDataChangeType.NONE, (returnedFileEvent as FileEvent).Remote);
                Assert.AreEqual(ContentChangeType.NONE, (returnedFileEvent as FileEvent).RemoteContent);
            }
        }

        [Test, Category("Fast")]
        public void HandleFSFileRemovedEventsTest () {
            AbstractFolderEvent returnedFileEvent = null;
            queue.Setup(q => q.AddEvent(It.IsAny<AbstractFolderEvent>()))
                .Callback((ISyncEvent f) => returnedFileEvent = f as AbstractFolderEvent);
            using (var watcher = new Watcher(queue.Object))
            {
                var fileRemovedFSEvent = new FSEvent(WatcherChangeTypes.Deleted, localFile.FullName);
                Assert.True(watcher.Handle(fileRemovedFSEvent));
                Assert.AreEqual(MetaDataChangeType.DELETED, returnedFileEvent.Local);
                Assert.AreEqual(ContentChangeType.DELETED, (returnedFileEvent as FileEvent).LocalContent);
                Assert.AreEqual(localFile.FullName, (returnedFileEvent as FileEvent).LocalFile.FullName);
                Assert.IsNull((returnedFileEvent as FileEvent).RemoteFile);
                Assert.AreEqual(MetaDataChangeType.NONE, (returnedFileEvent as FileEvent).Remote);
                Assert.AreEqual(ContentChangeType.NONE, (returnedFileEvent as FileEvent).RemoteContent);
            }
        }

        [Test, Category("Fast")]
        public void HandleFSFileRenamedEventsTest () {
            string oldpath = Path.Combine(localFolder.FullName, Path.GetRandomFileName());
            queue.Setup(q => q.AddEvent(It.IsAny<AbstractFolderEvent>()))
                .Callback((ISyncEvent f) => returnedFileEvent = f as AbstractFolderEvent);
            using (var watcher = new Watcher(queue.Object))
            {
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
        }

        [Test, Category("Fast")]
        public void HandleFSFolderAddedEventsTest () {
            queue.Setup(q => q.AddEvent(It.IsAny<AbstractFolderEvent>()))
                .Callback((ISyncEvent f) => returnedFolderEvent = f as AbstractFolderEvent);
            using (var watcher = new Watcher(queue.Object))
            {
                var folderCreatedFSEvent = new FSEvent(WatcherChangeTypes.Created, localFolder.FullName);
                Assert.True(watcher.Handle(folderCreatedFSEvent));
                Assert.AreEqual(MetaDataChangeType.CREATED, returnedFolderEvent.Local);
                Assert.AreEqual(localFolder.FullName, (returnedFolderEvent as FolderEvent).LocalFolder.FullName);
                Assert.IsNull((returnedFolderEvent as FolderEvent).RemoteFolder);
                Assert.AreEqual(MetaDataChangeType.NONE, (returnedFolderEvent as FolderEvent).Remote);
            }
        }

        [Test, Category("Fast")]
        public void HandleFSFolderChangedEventsTest () {
            queue.Setup(q => q.AddEvent(It.IsAny<AbstractFolderEvent>()))
                .Callback((ISyncEvent f) => returnedFolderEvent = f as AbstractFolderEvent);
            using (var watcher = new Watcher(queue.Object))
            {
                var folderChangedFSEvent = new FSEvent(WatcherChangeTypes.Changed, localFolder.FullName);
                Assert.True(watcher.Handle(folderChangedFSEvent));
                Assert.AreEqual(MetaDataChangeType.CHANGED, returnedFolderEvent.Local);
                Assert.AreEqual(localFolder.FullName, (returnedFolderEvent as FolderEvent).LocalFolder.FullName);
                Assert.IsNull((returnedFolderEvent as FolderEvent).RemoteFolder);
                Assert.AreEqual(MetaDataChangeType.NONE, (returnedFolderEvent as FolderEvent).Remote);
            }
        }

        [Test, Category("Fast")]
        public void HandleFSFolderRemovedEventsTest () {
            queue.Setup(q => q.AddEvent(It.IsAny<AbstractFolderEvent>()))
                .Callback((ISyncEvent f) => returnedFolderEvent = f as AbstractFolderEvent);
            using (var watcher = new Watcher(queue.Object))
            {
                var folderRemovedFSEvent = new FSEvent(WatcherChangeTypes.Deleted, localFolder.FullName);
                Assert.True(watcher.Handle(folderRemovedFSEvent));
                Assert.AreEqual(MetaDataChangeType.DELETED, returnedFolderEvent.Local);
                Assert.AreEqual(localFolder.FullName, (returnedFolderEvent as FolderEvent).LocalFolder.FullName);
                Assert.IsNull((returnedFolderEvent as FolderEvent).RemoteFolder);
                Assert.AreEqual(MetaDataChangeType.NONE, (returnedFolderEvent as FolderEvent).Remote);
            }
        }

        [Test, Category("Fast")]
        public void HandleFSFolderRenamedEventsTest () {
            string oldpath = Path.Combine(localFolder.FullName, Path.GetRandomFileName());
            queue.Setup(q => q.AddEvent(It.IsAny<AbstractFolderEvent>()))
                .Callback((ISyncEvent f) => returnedFolderEvent = f as AbstractFolderEvent);
            using (var watcher = new Watcher(queue.Object))
            {
                var folderRenamedFSEvent = new FSMovedEvent(oldpath, localFolder.FullName);
                Assert.True(watcher.Handle(folderRenamedFSEvent));
                Assert.AreEqual(MetaDataChangeType.MOVED, returnedFolderEvent.Local);
                Assert.AreEqual(localFolder.FullName, (returnedFolderEvent as FolderEvent).LocalFolder.FullName);
                Assert.AreEqual(oldpath, (returnedFolderEvent as FolderMovedEvent).OldLocalFolder.FullName);
                Assert.IsNull((returnedFolderEvent as FolderMovedEvent).RemoteFolder);
                Assert.IsNull((returnedFolderEvent as FolderMovedEvent).OldRemoteFolderPath);
                Assert.AreEqual(MetaDataChangeType.NONE, (returnedFolderEvent as FolderEvent).Remote);
            }
        }
    }
}

