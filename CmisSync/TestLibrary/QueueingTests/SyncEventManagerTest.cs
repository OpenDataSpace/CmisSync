//-----------------------------------------------------------------------
// <copyright file="SyncEventManagerTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.QueueingTests {
    using System;
    using System.IO;

    using CmisSync.Lib.Config;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Exceptions;
    using CmisSync.Lib.Queueing;

    using DotCMIS.Exceptions;

    using log4net;
    using log4net.Config;

    using Moq;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture, Category("Fast")]
    public class SyncEventManagerTest : IsTestWithConfiguredLog4Net {
        [Test]
        public void DefaultConstructorWithoutParamWorks() {
            new SyncEventManager();
        }

        [Test]
        public void AddHandlerTest() {
            var handlerMock = new Mock<SyncEventHandler>();
            var mockedEvent = Mock.Of<ISyncEvent>();

            var underTest = new SyncEventManager();
            underTest.AddEventHandler(handlerMock.Object);
            underTest.Handle(mockedEvent);

            handlerMock.Verify(foo => foo.Handle(mockedEvent), Times.Once());
        }

        [Test]
        public void BreaksIfHandlerSucceedsTest([Values(true, false)]bool highestFirst) {
            var handlerMock1 = new Mock<SyncEventHandler>() { CallBase = true };
            handlerMock1.Setup(foo => foo.Handle(It.IsAny<ISyncEvent>())).Returns(true);
            handlerMock1.Setup(foo => foo.Priority).Returns(2);

            var handlerMock2 = new Mock<SyncEventHandler>() { CallBase = true };
            handlerMock2.Setup(foo => foo.Priority).Returns(1);

            var eventMock = new Mock<ISyncEvent>();

            var underTest = new SyncEventManager();
            if (highestFirst) {
                underTest.AddEventHandler(handlerMock1.Object);
                underTest.AddEventHandler(handlerMock2.Object);
            } else {
                underTest.AddEventHandler(handlerMock2.Object);
                underTest.AddEventHandler(handlerMock1.Object);
            }

            underTest.Handle(eventMock.Object);

            handlerMock1.Verify(foo => foo.Handle(eventMock.Object), Times.Once());
            handlerMock2.Verify(foo => foo.Handle(eventMock.Object), Times.Never());
        }

        [Test]
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

        [Test]
        public void FirstInsertedHandlerWithSamePrioWinsTest() {
            var handlerMock1 = new Mock<SyncEventHandler>() { CallBase = true };
            handlerMock1.Setup(foo => foo.Handle(It.IsAny<ISyncEvent>())).Returns(true);
            handlerMock1.Setup(foo => foo.Priority).Returns(1);

            var handlerMock2 = new Mock<SyncEventHandler>() { CallBase = true };
            handlerMock2.Setup(foo => foo.Handle(It.IsAny<ISyncEvent>())).Returns(true);
            handlerMock2.Setup(foo => foo.Priority).Returns(1);

            var eventMock = new Mock<ISyncEvent>();

            var underTest = new SyncEventManager();
            underTest.AddEventHandler(handlerMock1.Object);
            underTest.AddEventHandler(handlerMock2.Object);
            underTest.Handle(eventMock.Object);

            handlerMock1.Verify(foo => foo.Handle(eventMock.Object), Times.Once());
            handlerMock2.Verify(foo => foo.Handle(eventMock.Object), Times.Never());
        }

        [Test]
        public void DeleteWorksCorrectlyTest() {
            var handlerMock1 = new Mock<SyncEventHandler>() { CallBase = true };
            handlerMock1.Setup(foo => foo.Handle(It.IsAny<ISyncEvent>())).Returns(false);
            handlerMock1.Setup(foo => foo.Priority).Returns(1);

            var handlerMock2 = new Mock<SyncEventHandler>() { CallBase = true };
            handlerMock2.Setup(foo => foo.Handle(It.IsAny<ISyncEvent>())).Returns(false);
            handlerMock2.Setup(foo => foo.Priority).Returns(1);

            var handlerMock3 = new Mock<SyncEventHandler>() { CallBase = true };
            handlerMock3.Setup(foo => foo.Handle(It.IsAny<ISyncEvent>())).Returns(false);
            handlerMock3.Setup(foo => foo.Priority).Returns(1);

            var eventMock = new Mock<ISyncEvent>();

            var underTest = new SyncEventManager();
            underTest.AddEventHandler(handlerMock1.Object);
            underTest.AddEventHandler(handlerMock2.Object);
            underTest.AddEventHandler(handlerMock3.Object);
            underTest.RemoveEventHandler(handlerMock2.Object);
            underTest.Handle(eventMock.Object);

            handlerMock1.Verify(foo => foo.Handle(eventMock.Object), Times.Once());
            handlerMock2.Verify(foo => foo.Handle(eventMock.Object), Times.Never());
            handlerMock3.Verify(foo => foo.Handle(eventMock.Object), Times.Once());
        }

        [Test]
        public void ConnectionExceptionsPassedToListenerAndThrown() {
            var connectionException = new Mock<CmisConnectionException>().Object;
            var underTest = new SyncEventManager();
            int raised = 0;
            underTest.OnException += (sender, e) => {
                Assert.That(sender, Is.EqualTo(underTest));
                Assert.That(e.Exception, Is.EqualTo(connectionException));
                raised++;
            };

            underTest.AddEventHandler(new GenericSyncEventHandler<ISyncEvent>(1, delegate(ISyncEvent e) {
                throw connectionException;
            }));

            var thrown = Assert.Catch<CmisConnectionException>(() => underTest.Handle(Mock.Of<ISyncEvent>()));
            Assert.That(thrown, Is.EqualTo(connectionException));
            Assert.That(raised, Is.EqualTo(1));
        }

        [Test]
        public void InteractionExceptionPassedToListener() {
            var interactionException = Mock.Of<AbstractInteractionNeededException>();
            var underTest = new SyncEventManager();
            int raised = 0;
            underTest.OnException += (sender, e) => {
                Assert.That(sender, Is.EqualTo(underTest));
                Assert.That(e.Exception, Is.EqualTo(interactionException));
                raised++;
            };

            underTest.AddEventHandler(new GenericSyncEventHandler<ISyncEvent>(1, delegate(ISyncEvent e) {
                throw interactionException;
            }));

            underTest.Handle(Mock.Of<ISyncEvent>());
            Assert.That(raised, Is.EqualTo(1));
        }
    }
}