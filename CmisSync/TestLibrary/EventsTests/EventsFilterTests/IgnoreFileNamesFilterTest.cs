using System;
using System.IO;

using DotCMIS.Client;

using CmisSync.Lib.Events;
using CmisSync.Lib.Events.Filter;

using NUnit.Framework;

using Moq;

namespace TestLibrary.EventsTests.EventsFilterTests
{
    [TestFixture]
    public class IgnoreFileNamesFilterTest
    {

        [Test, Category("Fast")]
        public void ConstructorExceptionOnNullQueueTest() {
            try{
                new IgnoredFileNamesFilter(null);
                Assert.Fail();
            }catch(ArgumentNullException){}
        }

        [Test,Category("Fast")]
        public void AllowCorrectFSEventsTest() {
            var file = new FileInfo(Path.Combine(Path.GetTempPath(), "testfile"));
            var queue = new Mock<ISyncEventQueue>();
            queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Never());
            var filter = new IgnoredFileNamesFilter(queue.Object);
            var fileEvent = new Mock<FSEvent>(WatcherChangeTypes.Changed, file.FullName);
            fileEvent.Setup(e => e.IsDirectory()).Returns(false);
            fileEvent.Setup(e => e.Path).Returns(file.FullName);
            Assert.IsFalse(filter.Handle(fileEvent.Object));
        }

        [Test, Category("Fast")]
        public void HandleIgnoredFileNamesTest() {
            int called = 0;
            var file = new FileInfo(Path.Combine(Path.GetTempPath(), "file~"));
            var queue = new Mock<ISyncEventQueue>();
            queue.Setup(q => q.AddEvent(It.IsAny<RequestIgnoredEvent>())).Callback(()=>called++);
            var filter = new IgnoredFileNamesFilter(queue.Object);
            var fileEvent = new Mock<FSEvent>(WatcherChangeTypes.Changed, file.FullName);
            fileEvent.Setup(e => e.IsDirectory()).Returns(false);
            fileEvent.Setup(e => e.Path).Returns(file.FullName);
            Assert.IsTrue(filter.Handle(fileEvent.Object));
            Assert.AreEqual(1, called);
        }

        [Test, Category("Fast")]
        public void IgnoreFolderFSEventsTest() {
            var queue = new Mock<ISyncEventQueue>();
            queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Never());
            var filter = new IgnoredFileNamesFilter(queue.Object);
            var folderEvent = new Mock<FSEvent>(WatcherChangeTypes.Changed, "");
            folderEvent.Setup(e => e.IsDirectory()).Returns(true);
            folderEvent.VerifyGet( e => e.Path, Times.Never());
            Assert.IsFalse(filter.Handle(folderEvent.Object));
        }

        [Test, Category("Fast")]
        public void IgnoreNonExsitingFileOrFolderFSEventsTest() {
            var queue = new Mock<ISyncEventQueue>();
            queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Never());
            var filter = new IgnoredFileNamesFilter(queue.Object);
            var folderEvent = new Mock<FSEvent>(WatcherChangeTypes.Changed, "");
            folderEvent.Setup(e => e.IsDirectory()).Throws(new FileNotFoundException());
            folderEvent.VerifyGet( e => e.Path, Times.Never());
            Assert.IsFalse(filter.Handle(folderEvent.Object));
        }

        [Test, Category("Fast")]
        public void IgnoreFolderMovedFSEventsTest() {
            var queue = new Mock<ISyncEventQueue>();
            queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Never());
            var filter = new IgnoredFileNamesFilter(queue.Object);
            var moveEvent = new Mock<FSMovedEvent>(" ", " ");
            moveEvent.Setup(e => e.IsDirectory()).Returns(true);
            moveEvent.VerifyGet( m => m.Path, Times.Never());
            moveEvent.VerifyGet( m => m.OldPath, Times.Never());
            Assert.IsFalse(filter.Handle(moveEvent.Object));
        }
    }
}

