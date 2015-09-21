//-----------------------------------------------------------------------
// <copyright file="ConnectionInterruptedHandlerTest.cs" company="GRAU DATA AG">
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
﻿
namespace TestLibrary.QueueingTests {
    using System;
    using System.Threading;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Queueing;

    using DotCMIS.Exceptions;

    using Moq;

    using NUnit.Framework;

    [TestFixture, Category("Fast")]
    public class ConnectionInterruptedHandlerTest {
        [Test]
        public void ConstructorThrowsExceptionIfQueueIsNull() {
            Assert.Throws<ArgumentNullException>(() => new ConnectionInterruptedHandler(Mock.Of<ISyncEventManager>(), null));
        }

        [Test]
        public void ConstructorThrowsExceptionIfManagerIsNull() {
            Assert.Throws<ArgumentNullException>(() => new ConnectionInterruptedHandler(null, Mock.Of<ISyncEventQueue>()));
        }

        [Test]
        public void ConstructorTakesManagerAndQueue() {
            var queue = new Mock<ISyncEventQueue>(MockBehavior.Strict).Object;
            var manager = new Mock<ISyncEventManager>(MockBehavior.Strict).Object;
            new ConnectionInterruptedHandler(manager, queue);
        }

        [Test]
        public void AddsNewEventToQueueIfCmisConnectionExceptionOccurs() {
            var exception = new Mock<CmisConnectionException>(MockBehavior.Strict).Object;
            var queue = new Mock<ISyncEventQueue>(MockBehavior.Strict);
            queue.Setup(q => q.AddEvent(It.IsAny<CmisConnectionExceptionEvent>()));
            var manager = new Mock<ISyncEventManager>(MockBehavior.Strict);

            new ConnectionInterruptedHandler(manager.Object, queue.Object);

            manager.Raise(m => m.OnException += null, new ThreadExceptionEventArgs(exception));
            queue.Verify(q => q.AddEvent(It.Is<CmisConnectionExceptionEvent>(e => e.Exception == exception)), Times.Once);
        }
    }
}