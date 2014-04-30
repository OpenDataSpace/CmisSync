//-----------------------------------------------------------------------
// <copyright file="IgnoredFoldersFilterTest.cs" company="GRAU DATA AG">
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

    using DotCMIS.Client;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class IgnoredFoldersFilterTest
    {
        [Test, Category("Fast"), Category("EventFilter")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorExceptionOnNullQueue()
        {
            new IgnoredFoldersFilter(null);
        }

        [Test, Category("Fast"), Category("EventFilter")]
        public void NormalConstructor()
        {
            var queue = new Mock<ISyncEventQueue>().Object;
            new IgnoredFoldersFilter(queue);
        }

        [Test, Category("Medium"), Category("EventFilter")]
        public void AllowCorrectFSEvents()
        {
            var queue = new Mock<ISyncEventQueue>();
            queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Never());
            var filter = new IgnoredFoldersFilter(queue.Object);
            var folderEvent = new Mock<FSEvent>(WatcherChangeTypes.Changed, Path.GetTempPath());
            folderEvent.Setup(e => e.IsDirectory()).Returns(true);
            folderEvent.Setup(e => e.Path).Returns(Path.GetTempPath());

            Assert.IsFalse(filter.Handle(folderEvent.Object));
        }

        [Test, Category("Medium"), Category("EventFilter")]
        public void HandleIgnoredFolderNames()
        {
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
        public void IgnoreFileFSEvents()
        {
            var queue = new Mock<ISyncEventQueue>();
            var filter = new IgnoredFoldersFilter(queue.Object);
            var fileEvent = new Mock<FSEvent>(WatcherChangeTypes.Changed, string.Empty);
            fileEvent.Setup(e => e.IsDirectory()).Returns(false);

            Assert.IsFalse(filter.Handle(fileEvent.Object));
            fileEvent.VerifyGet(e => e.Path, Times.Once());
            queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Never());
        }

        [Test, Category("Fast"), Category("EventFilter")]
        public void IgnoreNonExsitingFileOrFolderFSEvents()
        {
            var queue = new Mock<ISyncEventQueue>();
            var filter = new IgnoredFoldersFilter(queue.Object);
            var folderEvent = new Mock<FSEvent>(WatcherChangeTypes.Changed, Path.Combine(Path.GetTempPath(), Path.GetTempFileName()));
            folderEvent.Setup(e => e.IsDirectory()).Throws(new FileNotFoundException());

            Assert.IsFalse(filter.Handle(folderEvent.Object));
            folderEvent.VerifyGet(e => e.Path, Times.Once());
            queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Never());
        }

        [Test, Category("Fast"), Category("EventFilter")]
        public void IgnoreFolderMovedFSEvents()
        {
            var queue = new Mock<ISyncEventQueue>();
            var filter = new IgnoredFoldersFilter(queue.Object);
            var moveEvent = new Mock<FSMovedEvent>(" ", " ");
            moveEvent.Setup(e => e.IsDirectory()).Returns(false);

            Assert.IsFalse(filter.Handle(moveEvent.Object));
            moveEvent.VerifyGet(m => m.Path, Times.Once());
            moveEvent.VerifyGet(m => m.OldPath, Times.Never());
            queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Never());
        }
    }
}
