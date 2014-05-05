using System;
using System.IO;
using System.Collections.Generic;

using DotCMIS.Client;

using CmisSync.Lib.Events;
using CmisSync.Lib.Events.Filter;

using NUnit.Framework;

using Moq;

namespace TestLibrary.EventsTests.EventsFilterTests
{
    [TestFixture]
    public class IgnoredFoldersFilterTest
    {

        [Test, Category("Fast"), Category("EventFilter")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorExceptionOnNullQueue() {
            new IgnoredFoldersFilter(null);
        }

        [Test, Category("Fast"), Category("EventFilter")]
        public void NormalConstructor() {
            var queue = new Mock<ISyncEventQueue>().Object;
            new IgnoredFoldersFilter(queue);
        }

        [Test,Category("Medium"), Category("EventFilter")]
        public void AllowCorrectFSEvents() {
            var queue = new Mock<ISyncEventQueue>();
            queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Never());
            var filter = new IgnoredFoldersFilter(queue.Object);
            var folderEvent = new Mock<FSEvent>(WatcherChangeTypes.Changed, Path.GetTempPath());
            folderEvent.Setup(e => e.IsDirectory()).Returns(true);
            folderEvent.Setup(e => e.Path).Returns(Path.GetTempPath());

            Assert.IsFalse(filter.Handle(folderEvent.Object));
        }

        [Test, Category("Medium"), Category("EventFilter")]
        public void HandleIgnoredFolderNames() {
            var queue = new Mock<ISyncEventQueue>();
            var filter = new IgnoredFoldersFilter(queue.Object);
            var folderEvent = new Mock<FSEvent>(WatcherChangeTypes.Changed, Path.GetTempPath());
            folderEvent.Setup(e => e.IsDirectory()).Returns(true);
            folderEvent.Setup(e => e.Path).Returns(Path.GetTempPath());
            var ignoredFolder = new List<string>();
            ignoredFolder.Add(Path.GetTempPath());
            filter.IgnoredPaths = ignoredFolder;

            Assert.IsTrue(filter.Handle(folderEvent.Object));
            queue.Verify(q => q.AddEvent(It.IsAny<RequestIgnoredEvent>()), Times.Once());
        }

        [Test, Category("Fast"), Category("EventFilter")]
        public void IgnoreFileFSEvents() {
            var queue = new Mock<ISyncEventQueue>();
            var filter = new IgnoredFoldersFilter(queue.Object);
            var fileEvent = new Mock<FSEvent>(WatcherChangeTypes.Changed, "");
            fileEvent.Setup(e => e.IsDirectory()).Returns(false);

            Assert.IsFalse(filter.Handle(fileEvent.Object));
            fileEvent.VerifyGet( e => e.Path, Times.Once());
            queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Never());
        }

        [Test, Category("Fast"), Category("EventFilter")]
        public void IgnoreNonExsitingFileOrFolderFSEvents() {
            var queue = new Mock<ISyncEventQueue>();
            var filter = new IgnoredFoldersFilter(queue.Object);
            var folderEvent = new Mock<FSEvent>(WatcherChangeTypes.Changed, Path.Combine(Path.GetTempPath(), Path.GetTempFileName()));
            folderEvent.Setup(e => e.IsDirectory()).Throws(new FileNotFoundException());

            Assert.IsFalse(filter.Handle(folderEvent.Object));
            folderEvent.VerifyGet( e => e.Path, Times.Once());
            queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Never());
        }

        [Test, Category("Fast"), Category("EventFilter")]
        public void IgnoreFolderMovedFSEvents() {
            var queue = new Mock<ISyncEventQueue>();
            var filter = new IgnoredFoldersFilter(queue.Object);
            var moveEvent = new Mock<FSMovedEvent>(" ", " ");
            moveEvent.Setup(e => e.IsDirectory()).Returns(false);

            Assert.IsFalse(filter.Handle(moveEvent.Object));
            moveEvent.VerifyGet( m => m.Path, Times.Once());
            moveEvent.VerifyGet( m => m.OldPath, Times.Never());
            queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Never());
        }
    }
}

