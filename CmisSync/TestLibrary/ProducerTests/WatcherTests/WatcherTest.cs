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

namespace TestLibrary.ProducerTests.WatcherTests {
    using System;
    using System.IO;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Producer.Watcher;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.FileSystem;

    using Moq;

    using NUnit.Framework;

    using TestUtils;

    [TestFixture, Category("Fast")]
    public class WatcherTest {
        private string localPath;
        private DirectoryInfo localFolder;
        private FileInfo localFile;
        private DirectoryInfo localSubFolder;
        private Mock<ISyncEventQueue> queue;
        private AbstractFolderEvent returnedFileEvent;
        private AbstractFolderEvent returnedFolderEvent;

        [SetUp]
        public void SetUp() {
            this.localPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            this.localFolder = new DirectoryInfo(this.localPath);
            this.localFolder.Create();
            this.localSubFolder = new DirectoryInfo(Path.Combine(this.localFolder.FullName, Path.GetRandomFileName()));
            this.localSubFolder.Create();
            this.localFile = new FileInfo(Path.Combine(this.localFolder.FullName, Path.GetRandomFileName()));
            using (this.localFile.Create());

            this.queue = new Mock<ISyncEventQueue>();
            this.returnedFileEvent = null;
            this.returnedFolderEvent = null;
        }

        [TearDown]
        public void TearDown() {
            this.localFile.Refresh();
            if (this.localFile.Exists) {
                this.localFile.Delete();
            }

            this.localSubFolder.Refresh();
            if (this.localSubFolder.Exists) {
                this.localSubFolder.Delete(true);
            }

            this.localFolder.Refresh();
            if (this.localFolder.Exists) {
                this.localFolder.Delete(true);
            }
        }

        [Test]
        public void IgnoreWrongEventsTest() {
            var underTest = new WatcherConsumer(this.queue.Object);

            Assert.That(underTest.Handle(Mock.Of<ISyncEvent>()), Is.False);
            Assert.That(underTest.Handle(new Mock<FileEvent>(Mock.Of<IFileInfo>(), null) { CallBase = false }.Object), Is.False);
            this.queue.VerifyThatNoEventIsAdded();
        }

        [Test]
        public void HandleFSFileAddedEvents() {
            this.queue.Setup(q => q.AddEvent(It.IsAny<AbstractFolderEvent>()))
                .Callback((ISyncEvent f) => this.returnedFileEvent = f as AbstractFolderEvent);
            var underTest = new WatcherConsumer(this.queue.Object);

            var fileCreatedFSEvent = new FSEvent(WatcherChangeTypes.Created, this.localFile.FullName, false);
            Assert.That(underTest.Handle(fileCreatedFSEvent), Is.True);
            Assert.That(this.returnedFileEvent.Local, Is.EqualTo(MetaDataChangeType.CREATED));
            Assert.AreEqual(ContentChangeType.CREATED, (this.returnedFileEvent as FileEvent).LocalContent);
            Assert.AreEqual(this.localFile.FullName, (this.returnedFileEvent as FileEvent).LocalFile.FullName);
            Assert.That((this.returnedFileEvent as FileEvent).RemoteFile, Is.Null);
            Assert.AreEqual(MetaDataChangeType.NONE, (this.returnedFileEvent as FileEvent).Remote);
            Assert.AreEqual(ContentChangeType.NONE, (this.returnedFileEvent as FileEvent).RemoteContent);
        }

        [Test]
        public void HandleFSFileChangedEvents() {
            this.queue.Setup(q => q.AddEvent(It.IsAny<AbstractFolderEvent>()))
                .Callback((ISyncEvent f) => this.returnedFileEvent = f as AbstractFolderEvent);
            var underTest = new WatcherConsumer(this.queue.Object);

            var fileChangedFSEvent = new FSEvent(WatcherChangeTypes.Changed, this.localFile.FullName, false);
            Assert.That(underTest.Handle(fileChangedFSEvent), Is.True);
            Assert.AreEqual(MetaDataChangeType.NONE, this.returnedFileEvent.Local);
            Assert.AreEqual(ContentChangeType.CHANGED, (this.returnedFileEvent as FileEvent).LocalContent);
            Assert.AreEqual(this.localFile.FullName, (this.returnedFileEvent as FileEvent).LocalFile.FullName);
            Assert.That((this.returnedFileEvent as FileEvent).RemoteFile, Is.Null);
            Assert.AreEqual(MetaDataChangeType.NONE, (this.returnedFileEvent as FileEvent).Remote);
            Assert.AreEqual(ContentChangeType.NONE, (this.returnedFileEvent as FileEvent).RemoteContent);
        }

        [Test]
        public void HandleFSFileRemovedEvents() {
            this.queue.Setup(q => q.AddEvent(It.IsAny<AbstractFolderEvent>()))
                .Callback((ISyncEvent f) => this.returnedFileEvent = f as AbstractFolderEvent);
            var underTest = new WatcherConsumer(this.queue.Object);

            var fileRemovedFSEvent = new FSEvent(WatcherChangeTypes.Deleted, this.localFile.FullName, false);
            Assert.That(underTest.Handle(fileRemovedFSEvent), Is.True);
            Assert.AreEqual(MetaDataChangeType.DELETED, this.returnedFileEvent.Local);
            Assert.AreEqual(ContentChangeType.DELETED, (this.returnedFileEvent as FileEvent).LocalContent);
            Assert.AreEqual(this.localFile.FullName, (this.returnedFileEvent as FileEvent).LocalFile.FullName);
            Assert.That((this.returnedFileEvent as FileEvent).RemoteFile, Is.Null);
            Assert.AreEqual(MetaDataChangeType.NONE, (this.returnedFileEvent as FileEvent).Remote);
            Assert.AreEqual(ContentChangeType.NONE, (this.returnedFileEvent as FileEvent).RemoteContent);
        }

        [Test]
        public void HandleFSFileRenamedEvents() {
            string oldpath = Path.Combine(this.localFolder.FullName, Path.GetRandomFileName());
            this.queue.Setup(q => q.AddEvent(It.IsAny<AbstractFolderEvent>()))
                .Callback((ISyncEvent f) => this.returnedFileEvent = f as AbstractFolderEvent);
            var underTest = new WatcherConsumer(this.queue.Object);

            var fileRenamedFSEvent = new FSMovedEvent(oldpath, this.localFile.FullName, false);
            Assert.That(underTest.Handle(fileRenamedFSEvent), Is.True);
            Assert.AreEqual(MetaDataChangeType.MOVED, this.returnedFileEvent.Local);
            Assert.AreEqual(ContentChangeType.NONE, (this.returnedFileEvent as FileMovedEvent).LocalContent);
            Assert.AreEqual(this.localFile.FullName, (this.returnedFileEvent as FileMovedEvent).LocalFile.FullName);
            Assert.AreEqual(oldpath, (this.returnedFileEvent as FileMovedEvent).OldLocalFile.FullName);
            Assert.That((this.returnedFileEvent as FileEvent).RemoteFile, Is.Null);
            Assert.AreEqual(MetaDataChangeType.NONE, (this.returnedFileEvent as FileMovedEvent).Remote);
            Assert.AreEqual(ContentChangeType.NONE, (this.returnedFileEvent as FileMovedEvent).RemoteContent);
        }

        [Test]
        public void HandleFSFolderAddedEvents() {
            this.queue.Setup(q => q.AddEvent(It.IsAny<AbstractFolderEvent>()))
                .Callback((ISyncEvent f) => this.returnedFolderEvent = f as AbstractFolderEvent);
            var underTest = new WatcherConsumer(this.queue.Object);

            var folderCreatedFSEvent = new FSEvent(WatcherChangeTypes.Created, this.localFolder.FullName, true);
            Assert.That(underTest.Handle(folderCreatedFSEvent), Is.True);
            Assert.AreEqual(MetaDataChangeType.CREATED, this.returnedFolderEvent.Local);
            Assert.AreEqual(this.localFolder.FullName, (this.returnedFolderEvent as FolderEvent).LocalFolder.FullName);
            Assert.That((this.returnedFolderEvent as FolderEvent).RemoteFolder, Is.Null);
            Assert.AreEqual(MetaDataChangeType.NONE, (this.returnedFolderEvent as FolderEvent).Remote);
        }

        [Test]
        public void HandleFSFolderChangedEvents() {
            this.queue.Setup(q => q.AddEvent(It.IsAny<AbstractFolderEvent>()))
                .Callback((ISyncEvent f) => this.returnedFolderEvent = f as AbstractFolderEvent);
            var underTest = new WatcherConsumer(this.queue.Object);

            var folderChangedFSEvent = new FSEvent(WatcherChangeTypes.Changed, this.localFolder.FullName, true);
            Assert.That(underTest.Handle(folderChangedFSEvent), Is.True);
            Assert.AreEqual(MetaDataChangeType.CHANGED, this.returnedFolderEvent.Local);
            Assert.AreEqual(this.localFolder.FullName, (this.returnedFolderEvent as FolderEvent).LocalFolder.FullName);
            Assert.That((this.returnedFolderEvent as FolderEvent).RemoteFolder, Is.Null);
            Assert.AreEqual(MetaDataChangeType.NONE, (this.returnedFolderEvent as FolderEvent).Remote);
        }

        [Test]
        public void HandleFSFolderRemovedEvents() {
            this.queue.Setup(q => q.AddEvent(It.IsAny<AbstractFolderEvent>()))
                .Callback((ISyncEvent f) => this.returnedFolderEvent = f as AbstractFolderEvent);
            var underTest = new WatcherConsumer(this.queue.Object);

            var folderRemovedFSEvent = new FSEvent(WatcherChangeTypes.Deleted, this.localFolder.FullName, true);
            Assert.That(underTest.Handle(folderRemovedFSEvent), Is.True);
            Assert.AreEqual(MetaDataChangeType.DELETED, this.returnedFolderEvent.Local);
            Assert.AreEqual(this.localFolder.FullName, (this.returnedFolderEvent as FolderEvent).LocalFolder.FullName);
            Assert.That((this.returnedFolderEvent as FolderEvent).RemoteFolder, Is.Null);
            Assert.AreEqual(MetaDataChangeType.NONE, (this.returnedFolderEvent as FolderEvent).Remote);
        }

        [Test]
        public void HandleFSFolderRenamedEvents() {
            string oldpath = Path.Combine(this.localFolder.FullName, Path.GetRandomFileName());
            this.queue.Setup(q => q.AddEvent(It.IsAny<AbstractFolderEvent>()))
                .Callback((ISyncEvent f) => this.returnedFolderEvent = f as AbstractFolderEvent);
            var underTest = new WatcherConsumer(this.queue.Object);

            var folderRenamedFSEvent = new FSMovedEvent(oldpath, this.localFolder.FullName, true);
            Assert.That(underTest.Handle(folderRenamedFSEvent), Is.True);
            Assert.AreEqual(MetaDataChangeType.MOVED, this.returnedFolderEvent.Local);
            Assert.AreEqual(this.localFolder.FullName, (this.returnedFolderEvent as FolderEvent).LocalFolder.FullName);
            Assert.AreEqual(oldpath, (this.returnedFolderEvent as FolderMovedEvent).OldLocalFolder.FullName);
            Assert.That((this.returnedFolderEvent as FolderMovedEvent).RemoteFolder, Is.Null);
            Assert.That((this.returnedFolderEvent as FolderMovedEvent).OldRemoteFolderPath, Is.Null);
            Assert.AreEqual(MetaDataChangeType.NONE, (this.returnedFolderEvent as FolderEvent).Remote);
        }
    }
}