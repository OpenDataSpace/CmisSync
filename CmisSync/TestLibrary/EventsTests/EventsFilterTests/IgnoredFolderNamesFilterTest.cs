//-----------------------------------------------------------------------
// <copyright file="IgnoredFolderNamesFilterTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.EventsTests.EventsFilterTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Events.Filter;

    using Moq;

    using NUnit.Framework;

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
            var fsEvent = new Mock<FSEvent>(WatcherChangeTypes.Created, ".test").Object;
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
            string path = Path.Combine(Path.GetTempPath(), ".test");
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
