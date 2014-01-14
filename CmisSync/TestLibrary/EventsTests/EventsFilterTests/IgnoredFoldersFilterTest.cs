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

        [Test, Category("Fast")]
        public void ConstructorExceptionOnNullQueueTest() {
            try{
                new IgnoredFoldersFilter(null);
                Assert.Fail();
            }catch(ArgumentNullException){}
        }

        [Test, Category("Fast")]
        public void NormalConstructorTest() {
            var queue = new Mock<ISyncEventQueue>().Object;
            new IgnoredFoldersFilter(queue);
        }

        [Test,Category("Medium")]
        public void AllowCorrectFSEventsTest() {
            var queue = new Mock<ISyncEventQueue>();
            queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Never());
            var filter = new IgnoredFoldersFilter(queue.Object);
            var folderEvent = new Mock<FSEvent>(WatcherChangeTypes.Changed, Path.GetTempPath());
            folderEvent.Setup(e => e.IsDirectory()).Returns(true);
            folderEvent.Setup(e => e.Path).Returns(Path.GetTempPath());
            Assert.IsFalse(filter.Handle(folderEvent.Object));
        }

        [Test, Category("Medium")]
        public void HandleIgnoredFolderNamesTest() {
            int called = 0;
            var queue = new Mock<ISyncEventQueue>();
            queue.Setup(q => q.AddEvent(It.IsAny<RequestIgnoredEvent>())).Callback(()=>called++);
            var filter = new IgnoredFoldersFilter(queue.Object);
            var folderEvent = new Mock<FSEvent>(WatcherChangeTypes.Changed, Path.GetTempPath());
            folderEvent.Setup(e => e.IsDirectory()).Returns(true);
            folderEvent.Setup(e => e.Path).Returns(Path.GetTempPath());
            var ignoredFolder = new List<string>();
            ignoredFolder.Add(Path.GetTempPath());
            filter.IgnoredPaths = ignoredFolder;
            Assert.IsTrue(filter.Handle(folderEvent.Object));
            Assert.AreEqual(1, called);
        }

        [Test, Category("Fast")]
        public void IgnoreFileFSEventsTest() {
            var queue = new Mock<ISyncEventQueue>();
            queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Never());
            var filter = new IgnoredFoldersFilter(queue.Object);
            var fileEvent = new Mock<FSEvent>(WatcherChangeTypes.Changed, "");
            fileEvent.Setup(e => e.IsDirectory()).Returns(false);
            fileEvent.VerifyGet( e => e.Path, Times.Never());
            Assert.IsFalse(filter.Handle(fileEvent.Object));
        }

        [Test, Category("Fast")]
        public void IgnoreNonExsitingFileOrFolderFSEventsTest() {
            var queue = new Mock<ISyncEventQueue>();
            queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Never());
            var filter = new IgnoredFoldersFilter(queue.Object);
            var folderEvent = new Mock<FSEvent>(WatcherChangeTypes.Changed, Path.Combine(Path.GetTempPath(), Path.GetTempFileName()));
            folderEvent.Setup(e => e.IsDirectory()).Throws(new FileNotFoundException());
            folderEvent.VerifyGet( e => e.Path, Times.Never());
            Assert.IsFalse(filter.Handle(folderEvent.Object));
        }

        [Test, Category("Fast")]
        public void IgnoreFolderMovedFSEventsTest() {
            var queue = new Mock<ISyncEventQueue>();
            queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Never());
            var filter = new IgnoredFoldersFilter(queue.Object);
            var moveEvent = new Mock<FSMovedEvent>(" ", " ");
            moveEvent.Setup(e => e.IsDirectory()).Returns(false);
            moveEvent.VerifyGet( m => m.Path, Times.Never());
            moveEvent.VerifyGet( m => m.OldPath, Times.Never());
            Assert.IsFalse(filter.Handle(moveEvent.Object));
        }

    }
}

