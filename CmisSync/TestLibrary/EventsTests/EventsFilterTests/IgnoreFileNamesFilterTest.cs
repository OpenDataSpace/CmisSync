//-----------------------------------------------------------------------
// <copyright file="IgnoreFileNamesFilterTest.cs" company="GRAU DATA AG">
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
    public class IgnoreFileNamesFilterTest
    {
        private Mock<ISyncEventQueue> queue;

        [SetUp]
        public void SetUp()
        {
            this.queue = new Mock<ISyncEventQueue>();
        }

        [Test, Category("Fast"), Category("EventFilter")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorExceptionOnNullQueueTest()
        {
            new IgnoredFileNamesFilter(null);
        }

        [Test, Category("Fast"), Category("EventFilter")]
        public void AllowCorrectEventsTest()
        {
            var filter = new IgnoredFileNamesFilter(this.queue.Object);
            var fileEvent = Mock.Of<IFilterableNameEvent>(
                f =>
                f.Name == "testfile" &&
                f.IsDirectory() == false);

            Assert.IsFalse(filter.Handle(fileEvent));
            this.queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Never());
        }

        [Test, Category("Fast"), Category("EventFilter")]
        public void HandleIgnoredFileNamesTest()
        {
            List<string> wildcards = new List<string>();
            wildcards.Add("*~");
            var filter = new IgnoredFileNamesFilter(this.queue.Object) { Wildcards = wildcards };
            var fileEvent = Mock.Of<IFilterableNameEvent>(
                f =>
                f.IsDirectory() == false &&
                f.Name == "file~");

            Assert.IsTrue(filter.Handle(fileEvent));
            this.queue.Verify(q => q.AddEvent(It.IsAny<RequestIgnoredEvent>()), Times.Once());
        }

        [Test, Category("Fast"), Category("EventFilter")]
        public void IgnoreFolderEventsTest()
        {
            var filter = new IgnoredFileNamesFilter(this.queue.Object);
            var folderEvent = Mock.Of<IFilterableNameEvent>(
                f =>
                f.IsDirectory() == true);

            Assert.IsFalse(filter.Handle(folderEvent));
            this.queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Never());
        }

        [Test, Category("Fast"), Category("EventFilter")]
        public void IgnoreNonExsitingFileOrFolderFSEventsTest()
        {
            var filter = new IgnoredFileNamesFilter(this.queue.Object);
            var folderEvent = new Mock<IFSEvent>();
            folderEvent.Setup(e => e.IsDirectory()).Throws(new FileNotFoundException());
            folderEvent.Setup(e => e.Type).Returns(WatcherChangeTypes.Changed);

            Assert.IsFalse(filter.Handle(folderEvent.Object));
            this.queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Never());
            folderEvent.VerifyGet(e => e.Path, Times.Never());
        }

        [Test, Category("Fast"), Category("EventFilter")]
        public void IgnoreFolderMovedFSEventsTest()
        {
            var filter = new IgnoredFileNamesFilter(this.queue.Object);
            var moveEvent = new Mock<IFSMovedEvent>();
            moveEvent.Setup(e => e.IsDirectory()).Returns(true);

            Assert.IsFalse(filter.Handle(moveEvent.Object));
            this.queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Never());
            moveEvent.VerifyGet(m => m.Path, Times.Never());
            moveEvent.VerifyGet(m => m.OldPath, Times.Never());
        }
    }
}
