//-----------------------------------------------------------------------
// <copyright file="RenamedFileSystemEventHandlerTest.cs" company="GRAU DATA AG">
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
    public class RenamedFileSystemEventHandlerTest
    {
        private static readonly string rootPath = Path.GetTempPath();
        private readonly string oldName = "Cat";
        private readonly string newName = "Dog";

        private Mock<ISyncEventQueue> queue;
        private Mock<IFileSystemInfoFactory> fsFactory;


        [SetUp]
        public void SetUpQueue() {
            this.queue = new Mock<ISyncEventQueue>();
            this.fsFactory = new Mock<IFileSystemInfoFactory>();
        }

        [Test, Category("Fast")]
        public void ConstructorTakesQueueAndPathAndFileSystemFactory()
        {
            new RenamedFileSystemEventHandler(Mock.Of<ISyncEventQueue>(), "path", this.fsFactory.Object);
        }

        [Test, Category("Fast")]
        public void ConstructorTakesQueueAndPath() {
            new RenamedFileSystemEventHandler(Mock.Of<ISyncEventQueue>(), "path");
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorThrowsExceptionIfQueueIsNull() {
            new RenamedFileSystemEventHandler(null, "path");
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorThrowsExceptionIfPathIsNull() {
            new RenamedFileSystemEventHandler(Mock.Of<ISyncEventQueue>(), null);
        }

        [Test, Category("Fast")]
        public void HandleRenameFolderEvent() {
            var handler = new RenamedFileSystemEventHandler(this.queue.Object, rootPath, this.fsFactory.Object);
            string newPath = Path.Combine(rootPath, newName);
            string oldPath = Path.Combine(rootPath, oldName);
            this.fsFactory.Setup(f => f.IsDirectory(newPath)).Returns((bool?)true);

            handler.Handle(null, new RenamedEventArgs(WatcherChangeTypes.Renamed, rootPath, newName, oldPath));

            this.queue.Verify(
                q =>
                q.AddEvent(It.Is<FSMovedEvent>(e => e.OldPath == oldPath && e.Name == newName && e.IsDirectory == true && e.LocalPath == newPath)), Times.Once());
            this.queue.VerifyThatNoOtherEventIsAddedThan<FSMovedEvent>();
        }

        [Test, Category("Fast")]
        public void HandleRenameFileEvent() {
            var handler = new RenamedFileSystemEventHandler(this.queue.Object, rootPath, this.fsFactory.Object);
            string newPath = Path.Combine(rootPath, newName);
            string oldPath = Path.Combine(rootPath, oldName);
            this.fsFactory.Setup(f => f.IsDirectory(newPath)).Returns((bool?)false);

            handler.Handle(null, new RenamedEventArgs(WatcherChangeTypes.Renamed, rootPath, newName, oldPath));

            this.queue.Verify(
                q =>
                q.AddEvent(It.Is<FSMovedEvent>(e => e.OldPath == oldPath && e.Name == newName && e.IsDirectory == false && e.LocalPath == newPath)), Times.Once());
            this.queue.VerifyThatNoOtherEventIsAddedThan<FSMovedEvent>();
        }

        [Test, Category("Fast")]
        public void HandleRenameEventOfNonExistingPath() {
            var handler = new RenamedFileSystemEventHandler(this.queue.Object, rootPath, this.fsFactory.Object);
            string newPath = Path.Combine(rootPath, newName);
            string oldPath = Path.Combine(rootPath, oldName);
            this.fsFactory.Setup(f => f.IsDirectory(newPath)).Returns((bool?)null);

            handler.Handle(null, new RenamedEventArgs(WatcherChangeTypes.Renamed, rootPath, newName, oldPath));

            this.queue.VerifyThatNoOtherEventIsAddedThan<StartNextSyncEvent>();
            this.queue.Verify(q => q.AddEvent(It.Is<StartNextSyncEvent>(e => e.FullSyncRequested == true)), Times.Once);
        }

        [Test, Category("Fast")]
        public void HandleFolderMoveFromOutsideOfRootFolderIntoRootFolder() {
            var handler = new RenamedFileSystemEventHandler(this.queue.Object, rootPath, this.fsFactory.Object);
            string newPath = Path.Combine(rootPath, newName);
            string oldPath = Path.GetFullPath(Path.Combine(rootPath, "..", Path.GetRandomFileName(), oldName));
            this.fsFactory.Setup(f => f.IsDirectory(newPath)).Returns((bool?)true);

            handler.Handle(null, new RenamedEventArgs(WatcherChangeTypes.Renamed, rootPath, newName, oldPath));

            this.queue.Verify(q => q.AddEvent(It.Is<FSEvent>(e => e.LocalPath == newPath && e.IsDirectory == true && e.Type == WatcherChangeTypes.Created)), Times.Once);
            this.queue.VerifyThatNoOtherEventIsAddedThan<FSEvent>();
        }

        [Test, Category("Fast")]
        public void HandleFolderMoveFromInsideTheRootFolderToOutsideTheRootFolder() {
            var handler = new RenamedFileSystemEventHandler(this.queue.Object, rootPath, this.fsFactory.Object);
            string newTargetPath = Path.GetFullPath(Path.Combine(rootPath, "..", Path.GetRandomFileName()));
            string newPath = Path.Combine(newTargetPath, newName);
            string oldPath = Path.Combine(rootPath, oldName);
            this.fsFactory.Setup(f => f.IsDirectory(newPath)).Returns((bool?)true);

            handler.Handle(null, new RenamedEventArgs(WatcherChangeTypes.Renamed, newTargetPath, newName, oldPath));

            this.queue.Verify(q => q.AddEvent(It.Is<FSEvent>(e => e.LocalPath == oldPath && e.IsDirectory == true && e.Type == WatcherChangeTypes.Deleted)), Times.Once);
            this.queue.VerifyThatNoOtherEventIsAddedThan<FSEvent>();
        }
    }
}
#endif