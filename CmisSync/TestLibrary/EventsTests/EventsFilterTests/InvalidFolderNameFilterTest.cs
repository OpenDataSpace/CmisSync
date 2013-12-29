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
        [Test, Category("Fast")]
        public void ConstructorTest() {
            var managermock = new Mock<SyncEventManager>();
            var queuemock = new Mock<SyncEventQueue>(managermock.Object);
            new InvalidFolderNameFilter(queuemock.Object);
            try{
                new InvalidFolderNameFilter(null);
                Assert.Fail();
            }catch(ArgumentNullException) {}
        }

        [Test, Category("Fast")]
        public void HandleAndReportTest()
        {
            var managermock = new Mock<SyncEventManager>();
            var queuemock = new Mock<SyncEventQueue>(managermock.Object);
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

