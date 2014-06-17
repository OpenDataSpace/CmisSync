//-----------------------------------------------------------------------
// <copyright file="InvalidFolderNameFilterTest.cs" company="GRAU DATA AG">
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

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Events.Filter;

    using DotCMIS.Client;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class InvalidFolderNameFilterTest
    {
        [Test, Category("Fast"), Category("EventFilter")]
        public void ConstructorWorksWithQueue()
        {
            var queuemock = new Mock<ISyncEventQueue>();
            new InvalidFolderNameFilter(queuemock.Object);
        }

        [Test, Category("Fast"), Category("EventFilter")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorFailsIfQueueIsNull()
        {
                new InvalidFolderNameFilter(null);
        }

        [Test, Category("Fast"), Category("EventFilter")]
        public void HandleAndReportTest()
        {
            var queuemock = new Mock<ISyncEventQueue>();
            var documentmock = new Mock<IDocument>();
            int called = 0;
            queuemock.Setup(q => q.AddEvent(It.IsAny<ISyncEvent>())).Callback(() => called++);
            InvalidFolderNameFilter filter = new InvalidFolderNameFilter(queuemock.Object);
            bool handled = filter.Handle(new FileDownloadRequest(documentmock.Object, "*"));
            Assert.True(handled, "*");
            Assert.AreEqual(1, called);
            handled = filter.Handle(new FileDownloadRequest(documentmock.Object, "?"));
            Assert.True(handled, "?");
            Assert.AreEqual(2, called);
            handled = filter.Handle(new FileDownloadRequest(documentmock.Object, ":"));
            Assert.True(handled, ":");
            Assert.AreEqual(3, called);
            handled = filter.Handle(new FileDownloadRequest(documentmock.Object, "test"));
            Assert.False(handled, "test");
            Assert.AreEqual(3, called);
            handled = filter.Handle(new FileDownloadRequest(documentmock.Object, "test_test"));
            Assert.False(handled);
            Assert.AreEqual(3, called);
            handled = filter.Handle(new FileDownloadRequest(documentmock.Object, "test Test/ test"));
            Assert.False(handled);
            Assert.AreEqual(3, called);
        }
    }
}
