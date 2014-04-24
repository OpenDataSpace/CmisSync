using log4net;
using log4net.Config;

using System;
using System.IO;


namespace TestLibrary.EventsTests
{
    using NUnit.Framework;
    using Moq;
    using CmisSync.Lib;
    using CmisSync.Lib.Config;
    using CmisSync.Lib.Events;

    [TestFixture]
    public class SyncEventManagerTest
    {
        [TestFixtureSetUp]
        public void ClassInit()
        {
            log4net.Config.XmlConfigurator.Configure(ConfigManager.CurrentConfig.GetLog4NetConfig());
        }

        [Test, Category("Fast")]
        public void DefaultConstructorWithoutParamWorks()
        {
            new SyncEventManager();
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorWithNameIsNullFails()
        {
            new SyncEventManager(null);
        }

        [Test, Category("Fast")]
        public void ConstructorTakesNameAsParameter()
        {
            new SyncEventManager("");
        }

        [Test, Category("Fast")]
        public void AddHandlerTest() {
            var handlerMock = new Mock<SyncEventHandler>();
            var eventMock = new Mock<ISyncEvent>();

            SyncEventManager manager = new SyncEventManager();
            manager.AddEventHandler(handlerMock.Object);
            manager.Handle(eventMock.Object);

            handlerMock.Verify(foo => foo.Handle(eventMock.Object), Times.Once());
        }

        [Test, Category("Fast")]
        public void BreaksIfHandlerSucceedsTest() {
            var handlerMock1 = new Mock<SyncEventHandler>();
            handlerMock1.Setup(foo => foo.Handle(It.IsAny<ISyncEvent>())).Returns(true);
            handlerMock1.Setup(foo => foo.Priority).Returns(2);

            var handlerMock2 = new Mock<SyncEventHandler>();
            handlerMock2.Setup(foo => foo.Priority).Returns(1);

            var eventMock = new Mock<ISyncEvent>();

            SyncEventManager manager = new SyncEventManager();
            manager.AddEventHandler(handlerMock1.Object);
            manager.AddEventHandler(handlerMock2.Object);
            manager.Handle(eventMock.Object);

            handlerMock1.Verify(foo => foo.Handle(eventMock.Object), Times.Once());
            handlerMock2.Verify(foo => foo.Handle(eventMock.Object), Times.Never());
        }

        [Test, Category("Fast")]
        public void ContinueIfHandlerNotSucceedsTest() {
            var handlerMock1 = new Mock<SyncEventHandler>();
            handlerMock1.Setup(foo => foo.Handle(It.IsAny<ISyncEvent>())).Returns(false);
            handlerMock1.Setup(foo => foo.Priority).Returns(2);

            var handlerMock2 = new Mock<SyncEventHandler>();
            handlerMock2.Setup(foo => foo.Priority).Returns(1);

            var eventMock = new Mock<ISyncEvent>();

            SyncEventManager manager = new SyncEventManager();
            manager.AddEventHandler(handlerMock1.Object);
            manager.AddEventHandler(handlerMock2.Object);
            manager.Handle(eventMock.Object);

            handlerMock1.Verify(foo => foo.Handle(eventMock.Object), Times.Once());
            handlerMock2.Verify(foo => foo.Handle(eventMock.Object), Times.Once());
        }

        [Test, Category("Fast")]
        public void FirstInsertedHandlerWithSamePrioWinsTest() {
            var handlerMock1 = new Mock<SyncEventHandler>();
            handlerMock1.Setup(foo => foo.Handle(It.IsAny<ISyncEvent>())).Returns(true);
            handlerMock1.Setup(foo => foo.Priority).Returns(1);

            var handlerMock2 = new Mock<SyncEventHandler>();
            handlerMock2.Setup(foo => foo.Handle(It.IsAny<ISyncEvent>())).Returns(true);
            handlerMock2.Setup(foo => foo.Priority).Returns(1);

            var eventMock = new Mock<ISyncEvent>();

            SyncEventManager manager = new SyncEventManager();
            manager.AddEventHandler(handlerMock1.Object);
            manager.AddEventHandler(handlerMock2.Object);
            manager.Handle(eventMock.Object);

            handlerMock1.Verify(foo => foo.Handle(eventMock.Object), Times.Once());
            handlerMock2.Verify(foo => foo.Handle(eventMock.Object), Times.Never());
        }

        [Test, Category("Fast")]
        public void DeleteWorksCorrectlyTest() {
            var handlerMock1 = new Mock<SyncEventHandler>();
            handlerMock1.Setup(foo => foo.Handle(It.IsAny<ISyncEvent>())).Returns(false);
            handlerMock1.Setup(foo => foo.Priority).Returns(1);

            var handlerMock2 = new Mock<SyncEventHandler>();
            handlerMock2.Setup(foo => foo.Handle(It.IsAny<ISyncEvent>())).Returns(false);
            handlerMock2.Setup(foo => foo.Priority).Returns(1);

            var handlerMock3 = new Mock<SyncEventHandler>();
            handlerMock3.Setup(foo => foo.Handle(It.IsAny<ISyncEvent>())).Returns(false);
            handlerMock3.Setup(foo => foo.Priority).Returns(1);

            var eventMock = new Mock<ISyncEvent>();

            SyncEventManager manager = new SyncEventManager();
            manager.AddEventHandler(handlerMock1.Object);
            manager.AddEventHandler(handlerMock2.Object);
            manager.AddEventHandler(handlerMock3.Object);
            manager.RemoveEventHandler(handlerMock2.Object);
            manager.Handle(eventMock.Object);

            handlerMock1.Verify(foo => foo.Handle(eventMock.Object), Times.Once());
            handlerMock2.Verify(foo => foo.Handle(eventMock.Object), Times.Never());
            handlerMock3.Verify(foo => foo.Handle(eventMock.Object), Times.Once());
        }
    }
}
