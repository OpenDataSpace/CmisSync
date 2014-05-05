using System;

using DotCMIS.Client;
using CmisSync.Lib.Events;
using CmisSync.Lib.Events.Filter;

using NUnit.Framework;

using Moq;

namespace TestLibrary.EventsTests.EventsFilterTests
{
    [TestFixture]
    public class InvalidFolderNameFilterTest
    {
        [Test, Category("Fast"), Category("EventFilter")]
        public void ConstructorWorksWithQueue() {
            var queuemock = new Mock<ISyncEventQueue>();
            new InvalidFolderNameFilter(queuemock.Object);
        }

        [Test, Category("Fast"), Category("EventFilter")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorFailsIfQueueIsNull() {
                new InvalidFolderNameFilter(null);
        }

        [Test, Category("Fast"), Category("EventFilter")]
        public void HandleAndReportTest()
        {
            var queuemock = new Mock<ISyncEventQueue>();
            var documentmock = new Mock<IDocument>();
            int called = 0;
            queuemock.Setup( q => q.AddEvent(It.IsAny<ISyncEvent>())).Callback( () => called++);
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

