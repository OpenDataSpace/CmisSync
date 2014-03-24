using System;
using System.IO;

using DotCMIS.Client;

using CmisSync.Lib.Events;
using CmisSync.Lib.Events.Filter;

using NUnit.Framework;

using Moq;
using System.Collections.Generic;

namespace TestLibrary.EventsTests.EventsFilterTests
{
    [TestFixture]
    public class IgnoreFileNamesFilterTest
    {

        private Mock<ISyncEventQueue> queue;

        [SetUp]
        public void SetUp()
        {
            queue = new Mock<ISyncEventQueue>();
        }

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
            var filter = new IgnoredFileNamesFilter(queue.Object);
            var fileEvent = new Mock<FSEvent>(WatcherChangeTypes.Changed, file.FullName);
            fileEvent.Setup(e => e.IsDirectory()).Returns(false);
            fileEvent.Setup(e => e.Path).Returns(file.FullName);

            Assert.IsFalse(filter.Handle(fileEvent.Object));
            queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Never());
        }

        [Test, Category("Fast")]
        public void HandleIgnoredFileNamesTest() {
            var file = new FileInfo(Path.Combine(Path.GetTempPath(), "file~"));
            List<string> wildcards = new List<string>();
            wildcards.Add("*~");
            var filter = new IgnoredFileNamesFilter(queue.Object) {Wildcards = wildcards};
            var fileEvent = new Mock<FSEvent>(WatcherChangeTypes.Changed, file.FullName);
            fileEvent.Setup(e => e.IsDirectory()).Returns(false);
            fileEvent.Setup(e => e.Path).Returns(file.FullName);

            Assert.IsTrue(filter.Handle(fileEvent.Object));
            queue.Verify(q => q.AddEvent(It.IsAny<RequestIgnoredEvent>()), Times.Once());
        }

        [Test, Category("Fast")]
        public void IgnoreFolderFSEventsTest() {
            var filter = new IgnoredFileNamesFilter(queue.Object);
            var folderEvent = new Mock<FSEvent>(WatcherChangeTypes.Changed, "");
            folderEvent.Setup(e => e.IsDirectory()).Returns(true);

            Assert.IsFalse(filter.Handle(folderEvent.Object));
            folderEvent.VerifyGet( e => e.Path, Times.Never());
            queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Never());
        }

        [Test, Category("Fast")]
        public void IgnoreNonExsitingFileOrFolderFSEventsTest() {
            var filter = new IgnoredFileNamesFilter(queue.Object);
            var folderEvent = new Mock<FSEvent>(WatcherChangeTypes.Changed, "");
            folderEvent.Setup(e => e.IsDirectory()).Throws(new FileNotFoundException());

            Assert.IsFalse(filter.Handle(folderEvent.Object));
            queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Never());
            folderEvent.VerifyGet( e => e.Path, Times.Never());
        }

        [Test, Category("Fast")]
        public void IgnoreFolderMovedFSEventsTest() {
            var filter = new IgnoredFileNamesFilter(queue.Object);
            var moveEvent = new Mock<FSMovedEvent>(" ", " ");
            moveEvent.Setup(e => e.IsDirectory()).Returns(true);

            Assert.IsFalse(filter.Handle(moveEvent.Object));
            queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Never());
            moveEvent.VerifyGet( m => m.Path, Times.Never());
            moveEvent.VerifyGet( m => m.OldPath, Times.Never());
        }
    }
}

