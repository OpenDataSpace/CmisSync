using System;

using CmisSync.Lib.Events;
using CmisSync.Lib.Events.Filter;

using NUnit.Framework;

using Moq;
using System.IO;
using System.Collections.Generic;

namespace TestLibrary.EventsTests.EventsFilterTests
{
    [TestFixture]
    public class IgnoredFolderNamesFilterTest
    {
        [Test, Category("Fast"), Category("EventFilter")]
        public void ConstructorWorksWithGivenQueue()
        {
            new IgnoredFolderNameFilter(new Mock<ISyncEventQueue>().Object);
        }

        [Test, Category("Fast"), Category("EventFilter")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorFailsIfQueueIsNull()
        {
            new IgnoredFolderNameFilter(null);
        }

        [Test, Category("Fast"), Category("EventFilter")]
        public void FilterIgnoresNonFittingISyncEvents()
        {
            var genericEvent = new Mock<ISyncEvent>().Object;
            var queue = new Mock<ISyncEventQueue>();
            var filter = new IgnoredFolderNameFilter(queue.Object);

            Assert.IsFalse(filter.Handle(genericEvent));
            queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Never());
        }

        [Test, Category("Fast"), Category("EventFilter")]
        public void FilterLetsFSEventsPassIfNoWildcardsAreSet()
        {
            var fsEvent = new Mock<FSEvent>(WatcherChangeTypes.Created , ".test").Object;
            var queue = new Mock<ISyncEventQueue>();
            var filter = new IgnoredFolderNameFilter(queue.Object);

            Assert.IsFalse(filter.Handle(fsEvent));
            queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Never());
        }

        [Test, Category("Fast"), Category("EventFilter")]
        public void FilterTakesWildcardsWithoutFailure()
        {
            var filter = new IgnoredFolderNameFilter(new Mock<ISyncEventQueue>().Object);
            var wildcards = new List<string>();
            wildcards.Add("*.tmp");
            filter.Wildcards = wildcards;
        }

        [Test, Category("Fast"), Category("EventFilter")]
        public void FilterTakesEmptyWildcardsWithoutFailure()
        {
            var filter = new IgnoredFolderNameFilter(new Mock<ISyncEventQueue>().Object);
            filter.Wildcards = new List<string>();
        }

        [Test, Category("Fast"), Category("EventFilter")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FilterFailsTakingNullWildcard()
        {
            var filter = new IgnoredFolderNameFilter(new Mock<ISyncEventQueue>().Object);
            filter.Wildcards = null;
        }

        [Test, Category("Fast"), Category("EventFilter")]
        public void FilterFiltersDirectoryFSEventsMatchingWildcard()
        {
            string path = Path.Combine(Path.GetTempPath() , ".test");
            var queue = new Mock<ISyncEventQueue>();
            var filter = new IgnoredFolderNameFilter(queue.Object);
            var fsEvent = new Mock<FSEvent>(WatcherChangeTypes.Deleted, path);
            fsEvent.Setup(fs => fs.IsDirectory()).Returns(true);
            fsEvent.Setup(fs => fs.Path).Returns(path);
            var wildcards = new List<string>();
            wildcards.Add(".*");
            filter.Wildcards = wildcards;

            Assert.IsTrue(filter.Handle(fsEvent.Object));
            queue.Verify(q => q.AddEvent(It.IsAny<RequestIgnoredEvent>()), Times.Once());
        }

        [Test, Category("Fast"), Category("EventFilter")]
        public void FilterFiltersFileFSEventsParentMatchingWildcard()
        {
            string path = Path.Combine(Path.GetTempPath(), ".test", "file.tmp");
            var queue = new Mock<ISyncEventQueue>();
            var filter = new IgnoredFolderNameFilter(queue.Object);
            var fsEvent = new Mock<FSEvent>(WatcherChangeTypes.Deleted, path);
            fsEvent.Setup(fs => fs.IsDirectory()).Returns(false);
            fsEvent.Setup(fs => fs.Path).Returns(path);
            var wildcards = new List<string>();
            wildcards.Add(".*");
            filter.Wildcards = wildcards;

            Assert.IsTrue(filter.Handle(fsEvent.Object));
            queue.Verify(q => q.AddEvent(It.IsAny<RequestIgnoredEvent>()), Times.Once());
        }

        [Test, Category("Fast"), Category("EventFilter")]
        public void FilterLetsFSEventsPassIfNotMatchingWildcard()
        {
            var queue = new Mock<ISyncEventQueue>();
            var filter = new IgnoredFolderNameFilter(queue.Object);
            var fsEvent = new FSEvent(WatcherChangeTypes.Deleted, Path.Combine(Path.GetTempPath(), ".cache"));
            var wildcards = new List<string>();
            wildcards.Add(".tmp");
            filter.Wildcards = wildcards;
            Assert.IsFalse(filter.Handle(fsEvent));
            queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Never());
        }
    }
}

