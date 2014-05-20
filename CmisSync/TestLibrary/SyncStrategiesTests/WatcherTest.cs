//-----------------------------------------------------------------------
// <copyright file="WatcherTest.cs" company="GRAU DATA AG">
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

    using Moq;

    using NUnit.Framework;

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
        public void SetUp()
        {
            this.localPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            this.localFolder = new DirectoryInfo(this.localPath);
            this.localFolder.Create();
            this.localSubFolder = new DirectoryInfo(Path.Combine(this.localFolder.FullName, Path.GetRandomFileName()));
            this.localSubFolder.Create();
            this.localFile = new FileInfo(Path.Combine(this.localFolder.FullName, Path.GetRandomFileName()));
            using (this.localFile.Create())
            {
            }

            this.queue = new Mock<ISyncEventQueue>();
            this.returnedFileEvent = null;
            this.returnedFolderEvent = null;
        }

        [TearDown]
        public void TearDown()
        {
            this.localFile.Refresh();
            if (this.localFile.Exists)
            {
                this.localFile.Delete();
            }

            this.localSubFolder.Refresh();
            if (this.localSubFolder.Exists)
            {
                this.localSubFolder.Delete(true);
            }

            this.localFolder.Refresh();
            if (this.localFolder.Exists)
            {
                this.localFolder.Delete(true);
            }
        }

        [Test, Category("Fast")]
        public void IgnoreWrongEventsTest()
        {
            this.queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Never());
            var watcher = new WatcherConsumer(this.queue.Object);
            Assert.False(watcher.Handle(new Mock<ISyncEvent>().Object));
            Assert.False(watcher.Handle(new Mock<FileEvent>(new Mock<IFileInfo>().Object, null, null) { CallBase = false }.Object));
            
        }

        [Test, Category("Fast")]
        public void HandleFSFileAddedEvents()
        {
            this.queue.Setup(q => q.AddEvent(It.IsAny<AbstractFolderEvent>()))
                .Callback((ISyncEvent f) => this.returnedFileEvent = f as AbstractFolderEvent);
            var watcher = new WatcherConsumer(this.queue.Object);

            var fileCreatedFSEvent = new FSEvent(WatcherChangeTypes.Created, this.localFile.FullName);
            Assert.True(watcher.Handle(fileCreatedFSEvent));
            Assert.AreEqual(MetaDataChangeType.CREATED, this.returnedFileEvent.Local);
            Assert.AreEqual(ContentChangeType.CREATED, (this.returnedFileEvent as FileEvent).LocalContent);
            Assert.AreEqual(this.localFile.FullName, (this.returnedFileEvent as FileEvent).LocalFile.FullName);
            Assert.IsNull((this.returnedFileEvent as FileEvent).RemoteFile);
            Assert.AreEqual(MetaDataChangeType.NONE, (this.returnedFileEvent as FileEvent).Remote);
            Assert.AreEqual(ContentChangeType.NONE, (this.returnedFileEvent as FileEvent).RemoteContent);
            
        }

        [Test, Category("Fast")]
        public void HandleFSFileChangedEvents()
        {
            this.queue.Setup(q => q.AddEvent(It.IsAny<AbstractFolderEvent>()))
                .Callback((ISyncEvent f) => this.returnedFileEvent = f as AbstractFolderEvent);
            var watcher = new WatcherConsumer(this.queue.Object);
            
            var fileChangedFSEvent = new FSEvent(WatcherChangeTypes.Changed, this.localFile.FullName);
            Assert.True(watcher.Handle(fileChangedFSEvent));
            Assert.AreEqual(MetaDataChangeType.NONE, this.returnedFileEvent.Local);
            Assert.AreEqual(ContentChangeType.CHANGED, (this.returnedFileEvent as FileEvent).LocalContent);
            Assert.AreEqual(this.localFile.FullName, (this.returnedFileEvent as FileEvent).LocalFile.FullName);
            Assert.IsNull((this.returnedFileEvent as FileEvent).RemoteFile);
            Assert.AreEqual(MetaDataChangeType.NONE, (this.returnedFileEvent as FileEvent).Remote);
            Assert.AreEqual(ContentChangeType.NONE, (this.returnedFileEvent as FileEvent).RemoteContent);
            
        }

        [Test, Category("Fast")]
        public void HandleFSFileRemovedEvents()
        {
            this.queue.Setup(q => q.AddEvent(It.IsAny<AbstractFolderEvent>()))
                .Callback((ISyncEvent f) => this.returnedFileEvent = f as AbstractFolderEvent);
            var watcher = new WatcherConsumer(this.queue.Object);
            
            var fileRemovedFSEvent = new FSEvent(WatcherChangeTypes.Deleted, this.localFile.FullName);
            Assert.True(watcher.Handle(fileRemovedFSEvent));
            Assert.AreEqual(MetaDataChangeType.DELETED, this.returnedFileEvent.Local);
            Assert.AreEqual(ContentChangeType.DELETED, (this.returnedFileEvent as FileEvent).LocalContent);
            Assert.AreEqual(this.localFile.FullName, (this.returnedFileEvent as FileEvent).LocalFile.FullName);
            Assert.IsNull((this.returnedFileEvent as FileEvent).RemoteFile);
            Assert.AreEqual(MetaDataChangeType.NONE, (this.returnedFileEvent as FileEvent).Remote);
            Assert.AreEqual(ContentChangeType.NONE, (this.returnedFileEvent as FileEvent).RemoteContent);
            
        }

        [Test, Category("Fast")]
        public void HandleFSFileRenamedEvents()
        {
            string oldpath = Path.Combine(this.localFolder.FullName, Path.GetRandomFileName());
            this.queue.Setup(q => q.AddEvent(It.IsAny<AbstractFolderEvent>()))
                .Callback((ISyncEvent f) => this.returnedFileEvent = f as AbstractFolderEvent);
            var watcher = new WatcherConsumer(this.queue.Object);
            
            var fileRenamedFSEvent = new FSMovedEvent(oldpath, this.localFile.FullName);
            Assert.True(watcher.Handle(fileRenamedFSEvent));
            Assert.AreEqual(MetaDataChangeType.MOVED, this.returnedFileEvent.Local);
            Assert.AreEqual(ContentChangeType.NONE, (this.returnedFileEvent as FileMovedEvent).LocalContent);
            Assert.AreEqual(this.localFile.FullName, (this.returnedFileEvent as FileMovedEvent).LocalFile.FullName);
            Assert.AreEqual(oldpath, (this.returnedFileEvent as FileMovedEvent).OldLocalFile.FullName);
            Assert.IsNull((this.returnedFileEvent as FileEvent).RemoteFile);
            Assert.AreEqual(MetaDataChangeType.NONE, (this.returnedFileEvent as FileMovedEvent).Remote);
            Assert.AreEqual(ContentChangeType.NONE, (this.returnedFileEvent as FileMovedEvent).RemoteContent);
            
        }

        [Test, Category("Fast")]
        public void HandleFSFolderAddedEvents()
        {
            this.queue.Setup(q => q.AddEvent(It.IsAny<AbstractFolderEvent>()))
                .Callback((ISyncEvent f) => this.returnedFolderEvent = f as AbstractFolderEvent);
            var watcher = new WatcherConsumer(this.queue.Object);
            
            var folderCreatedFSEvent = new FSEvent(WatcherChangeTypes.Created, this.localFolder.FullName);
            Assert.True(watcher.Handle(folderCreatedFSEvent));
            Assert.AreEqual(MetaDataChangeType.CREATED, this.returnedFolderEvent.Local);
            Assert.AreEqual(this.localFolder.FullName, (this.returnedFolderEvent as FolderEvent).LocalFolder.FullName);
            Assert.IsNull((this.returnedFolderEvent as FolderEvent).RemoteFolder);
            Assert.AreEqual(MetaDataChangeType.NONE, (this.returnedFolderEvent as FolderEvent).Remote);
            
        }

        [Test, Category("Fast")]
        public void HandleFSFolderChangedEvents()
        {
            this.queue.Setup(q => q.AddEvent(It.IsAny<AbstractFolderEvent>()))
                .Callback((ISyncEvent f) => this.returnedFolderEvent = f as AbstractFolderEvent);
            var watcher = new WatcherConsumer(this.queue.Object);
            
            var folderChangedFSEvent = new FSEvent(WatcherChangeTypes.Changed, this.localFolder.FullName);
            Assert.True(watcher.Handle(folderChangedFSEvent));
            Assert.AreEqual(MetaDataChangeType.CHANGED, this.returnedFolderEvent.Local);
            Assert.AreEqual(this.localFolder.FullName, (this.returnedFolderEvent as FolderEvent).LocalFolder.FullName);
            Assert.IsNull((this.returnedFolderEvent as FolderEvent).RemoteFolder);
            Assert.AreEqual(MetaDataChangeType.NONE, (this.returnedFolderEvent as FolderEvent).Remote);
            
        }

        [Test, Category("Fast")]
        public void HandleFSFolderRemovedEvents()
        {
            this.queue.Setup(q => q.AddEvent(It.IsAny<AbstractFolderEvent>()))
                .Callback((ISyncEvent f) => this.returnedFolderEvent = f as AbstractFolderEvent);
            var watcher = new WatcherConsumer(this.queue.Object);
            
            var folderRemovedFSEvent = new FSEvent(WatcherChangeTypes.Deleted, this.localFolder.FullName);
            Assert.True(watcher.Handle(folderRemovedFSEvent));
            Assert.AreEqual(MetaDataChangeType.DELETED, this.returnedFolderEvent.Local);
            Assert.AreEqual(this.localFolder.FullName, (this.returnedFolderEvent as FolderEvent).LocalFolder.FullName);
            Assert.IsNull((this.returnedFolderEvent as FolderEvent).RemoteFolder);
            Assert.AreEqual(MetaDataChangeType.NONE, (this.returnedFolderEvent as FolderEvent).Remote);
            
        }

        [Test, Category("Fast")]
        public void HandleFSFolderRenamedEvents()
        {
            string oldpath = Path.Combine(this.localFolder.FullName, Path.GetRandomFileName());
            this.queue.Setup(q => q.AddEvent(It.IsAny<AbstractFolderEvent>()))
                .Callback((ISyncEvent f) => this.returnedFolderEvent = f as AbstractFolderEvent);
            var watcher = new WatcherConsumer(this.queue.Object);
            
            var folderRenamedFSEvent = new FSMovedEvent(oldpath, this.localFolder.FullName);
            Assert.True(watcher.Handle(folderRenamedFSEvent));
            Assert.AreEqual(MetaDataChangeType.MOVED, this.returnedFolderEvent.Local);
            Assert.AreEqual(this.localFolder.FullName, (this.returnedFolderEvent as FolderEvent).LocalFolder.FullName);
            Assert.AreEqual(oldpath, (this.returnedFolderEvent as FolderMovedEvent).OldLocalFolder.FullName);
            Assert.IsNull((this.returnedFolderEvent as FolderMovedEvent).RemoteFolder);
            Assert.IsNull((this.returnedFolderEvent as FolderMovedEvent).OldRemoteFolderPath);
            Assert.AreEqual(MetaDataChangeType.NONE, (this.returnedFolderEvent as FolderEvent).Remote);
            
        }
    }
}