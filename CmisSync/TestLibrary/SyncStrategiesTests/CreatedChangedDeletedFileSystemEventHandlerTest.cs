//-----------------------------------------------------------------------
// <copyright file="CreatedChangedDeletedFileSystemEventHandlerTest.cs" company="GRAU DATA AG">
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
#if ! __COCOA__
namespace TestLibrary.SyncStrategiesTests
{
    using System;
    using System.IO;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Storage;
    using CmisSync.Lib.Sync.Strategy;

    using Moq;

    using NUnit.Framework;

    using TestUtils;

    [TestFixture]
    public class CreatedChangedDeletedFileSystemEventHandlerTest
    {
        private static readonly string Name = "Cat";
        private static readonly string Directory = Path.GetTempPath();
        private static readonly long Threshold = 100;
        private string path;
        private Mock<ISyncEventQueue> queue;
        private Mock<IMetaDataStorage> storage;
        private Mock<IFileSystemInfoFactory> fsFactory;

        [SetUp]
        public void SetUp() {
            this.queue = new Mock<ISyncEventQueue>();
            this.storage = new Mock<IMetaDataStorage>();
            this.fsFactory = new Mock<IFileSystemInfoFactory>();
            this.path = Path.Combine(Directory, Name);
        }

        [Test, Category("Fast")]
        public void ConstructorTakesQueueAndStorageAndFileSystemInfoFactory() {
            new CreatedChangedDeletedFileSystemEventHandler(this.queue.Object, this.storage.Object, this.fsFactory.Object);
        }

        [Test, Category("Fast")]
        public void ConstructorTakesQueueAndStorage() {
            new CreatedChangedDeletedFileSystemEventHandler(this.queue.Object, this.storage.Object);
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorThrowsExceptionIfQueueIsNull() {
            new CreatedChangedDeletedFileSystemEventHandler(null, this.storage.Object);
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorThrowsExceptionIfStorageIsNull() {
            new CreatedChangedDeletedFileSystemEventHandler(this.queue.Object, null);
        }

        [Test, Category("Fast")]
        public void HandlesFileCreatedEvent() {
            var handler = new CreatedChangedDeletedFileSystemEventHandler(this.queue.Object, this.storage.Object, this.fsFactory.Object);
            this.fsFactory.Setup(f => f.IsDirectory(this.path)).Returns((bool?)false);

            handler.Handle(null, new FileSystemEventArgs(WatcherChangeTypes.Created, Directory, Name));

            this.queue.Verify(q => q.AddEvent(It.Is<FSEvent>(e => e.IsDirectory == false && e.Name == Name && e.LocalPath == this.path && e.Type == WatcherChangeTypes.Created)), Times.Once());
            this.queue.VerifyThatNoOtherEventIsAddedThan<FSEvent>();
        }

        [Test, Category("Fast")]
        public void HandlesFileChangedEvent() {
            var handler = new CreatedChangedDeletedFileSystemEventHandler(this.queue.Object, this.storage.Object, this.fsFactory.Object);
            this.fsFactory.Setup(f => f.IsDirectory(this.path)).Returns((bool?)false);

            handler.Handle(null, new FileSystemEventArgs(WatcherChangeTypes.Changed, Directory, Name));

            this.queue.Verify(q => q.AddEvent(It.Is<FSEvent>(e => e.IsDirectory == false && e.Name == Name && e.LocalPath == this.path && e.Type == WatcherChangeTypes.Changed)), Times.Once());
            this.queue.VerifyThatNoOtherEventIsAddedThan<FSEvent>();
        }

        [Test, Category("Fast")]
        public void HandlesFileDeletedEvent() {
            var handler = new CreatedChangedDeletedFileSystemEventHandler(this.queue.Object, this.storage.Object, this.fsFactory.Object, Threshold);
            this.fsFactory.Setup(f => f.IsDirectory(this.path)).Returns((bool?)null);
            var file = this.fsFactory.AddFile(this.path, false);
            this.storage.AddLocalFile(file.Object.FullName, "id", Guid.NewGuid());

            handler.Handle(null, new FileSystemEventArgs(WatcherChangeTypes.Deleted, Directory, Name));
            this.WaitForThreshold();
            this.queue.Verify(q => q.AddEvent(It.Is<FSEvent>(e => e.IsDirectory == false && e.LocalPath == this.path && e.Name == Name && e.Type == WatcherChangeTypes.Deleted)));
            this.queue.VerifyThatNoOtherEventIsAddedThan<FSEvent>();
        }

        [Test, Category("Fast")]
        public void IgnoresEventOnNonExistingPath() {
            var handler = new CreatedChangedDeletedFileSystemEventHandler(this.queue.Object, this.storage.Object, this.fsFactory.Object);
            this.fsFactory.Setup(f => f.IsDirectory(this.path)).Returns((bool?)null);

            handler.Handle(null, new FileSystemEventArgs(WatcherChangeTypes.Created, Directory, Name));

            this.queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Never());
        }

        [Test, Category("Fast")]
        public void IgnoresDeletionOfPathWithNoEntryInStorage() {
            var handler = new CreatedChangedDeletedFileSystemEventHandler(this.queue.Object, this.storage.Object, this.fsFactory.Object);
            handler.Handle(null, new FileSystemEventArgs(WatcherChangeTypes.Deleted, Directory, Name));
            this.queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Never());
        }

        [Test, Category("Fast")]
        public void HandlesFolderCreatedEvent() {
            var handler = new CreatedChangedDeletedFileSystemEventHandler(this.queue.Object, this.storage.Object, this.fsFactory.Object);
            this.fsFactory.Setup(f => f.IsDirectory(this.path)).Returns((bool?)true);

            handler.Handle(null, new FileSystemEventArgs(WatcherChangeTypes.Created, Directory, Name));

            this.queue.Verify(q => q.AddEvent(It.Is<FSEvent>(e => e.IsDirectory == true && e.Name == Name && e.LocalPath == this.path && e.Type == WatcherChangeTypes.Created)), Times.Once());
            this.queue.VerifyThatNoOtherEventIsAddedThan<FSEvent>();
        }

        [Ignore]
        [Test, Category("Fast")]
        public void AggregatesFolderDeletedAndCreatedEventToFSMovedEvent() {
            var handler = new CreatedChangedDeletedFileSystemEventHandler(this.queue.Object, this.storage.Object, this.fsFactory.Object);
            string newName = "new";
            string oldName = "old";
            Guid guid = Guid.NewGuid();
            string newPath = Path.Combine(this.path, newName);
            string oldPath = Path.Combine(Directory, oldName);
            this.fsFactory.Setup(f => f.IsDirectory(newPath)).Returns((bool?)true);
            this.fsFactory.AddDirectory(newPath, guid, true);
            this.storage.AddLocalFolder(oldPath, "id", guid);

            handler.Handle(null, new FileSystemEventArgs(WatcherChangeTypes.Deleted, Directory, oldName));
            handler.Handle(null, new FileSystemEventArgs(WatcherChangeTypes.Created, this.path, newName));

            this.queue.Verify(q => q.AddEvent(It.Is<FSMovedEvent>(e => e.IsDirectory == true && e.Name == newName && e.OldPath == oldPath && e.LocalPath == newPath)), Times.Once());
            this.WaitForThreshold();
            this.queue.VerifyThatNoOtherEventIsAddedThan<FSMovedEvent>();
        }

        private void WaitForThreshold() {
            System.Threading.Thread.Sleep((int)Threshold * 10);
        }
    }
}
#endif