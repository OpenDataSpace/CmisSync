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
namespace TestLibrary.ProducerTests.WatcherTests
{
    using System;
    using System.IO;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Storage;
    using CmisSync.Lib.Producer.Watcher;

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

            handler.Handle(null, this.CreateEvent(oldName, newName));

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

            handler.Handle(null, this.CreateEvent(oldName, newName));

            this.queue.Verify(
                q =>
                q.AddEvent(It.Is<FSMovedEvent>(e => e.OldPath == oldPath && e.Name == newName && e.IsDirectory == false && e.LocalPath == newPath)), Times.Once());
            this.queue.VerifyThatNoOtherEventIsAddedThan<FSMovedEvent>();
        }

        [Test, Category("Fast")]
        public void HandleRenameEventOfNonExistingPath() {
            var handler = new RenamedFileSystemEventHandler(this.queue.Object, rootPath, this.fsFactory.Object);
            string newPath = Path.Combine(rootPath, newName);
            this.fsFactory.Setup(f => f.IsDirectory(newPath)).Returns((bool?)null);

            handler.Handle(null, this.CreateEvent(oldName, newName));

            this.queue.VerifyThatNoOtherEventIsAddedThan<StartNextSyncEvent>();
            this.queue.Verify(q => q.AddEvent(It.Is<StartNextSyncEvent>(e => e.FullSyncRequested == true)), Times.Once);
        }

        [Test, Category("Fast")]
        public void RenamedEventArgsConstructor()
        {
            var args = this.CreateEvent(oldName, newName);
            Assert.That(args.FullPath, Is.EqualTo(Path.Combine(rootPath, newName)));
            Assert.That(args.OldFullPath, Is.EqualTo(Path.Combine(rootPath, oldName)));
        }

        private RenamedEventArgs CreateEvent(string oldName, string newName)
        {
#if __MonoCS__
            return new RenamedEventArgs(WatcherChangeTypes.Renamed, rootPath, newName, Path.Combine(rootPath, oldName));
#else
            return new RenamedEventArgs(WatcherChangeTypes.Renamed, rootPath, newName, oldName);
#endif
        }
    }
}
#endif