//-----------------------------------------------------------------------
// <copyright file="DelayRetryAndNextSyncEventHandlerTest.cs" company="GRAU DATA AG">
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
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.FileSystem;

    using Moq;

    using NUnit.Framework;

    [TestFixture, Category("Fast")]
    public class DelayRetryAndNextSyncEventHandlerTest {
        [Test]
        public void DoesNotHandleNonRetryEvent() {
            var underTest = new DelayRetryAndNextSyncEventHandler(Mock.Of<ISyncEventQueue>());
            var fileEvent = new FileEvent(Mock.Of<IFileInfo>()) { RetryCount = 0 };
            Assert.False(underTest.Handle(fileEvent));
        }

        [Test]
        public void DelaysNextSyncEventUntilQueueEmpty() {
            var queue = new Mock<ISyncEventQueue>();
            var underTest = new DelayRetryAndNextSyncEventHandler(queue.Object);
            queue.Setup(q => q.IsEmpty).Returns(false);

            Assert.That(underTest.Handle(new StartNextSyncEvent()), Is.True);
            queue.Setup(q => q.IsEmpty).Returns(true);

            Assert.That(underTest.Handle(Mock.Of<ISyncEvent>()), Is.False);
            queue.Verify(q => q.AddEvent(It.Is<StartNextSyncEvent>(e => e.FullSyncRequested == false)));
        }

        [Test]
        public void DoNotDelayStartSyncWhenQueueEmpty() {
            var queue = new Mock<ISyncEventQueue>();
            queue.Setup(q => q.IsEmpty).Returns(true);
            var underTest = new DelayRetryAndNextSyncEventHandler(queue.Object);

            Assert.That(underTest.Handle(new StartNextSyncEvent()), Is.False);
        }

        [Test]
        public void ResetNextSyncFlagAfterRequest() {
            var queue = new Mock<ISyncEventQueue>();
            var underTest = new DelayRetryAndNextSyncEventHandler(queue.Object);
            queue.Setup(q => q.IsEmpty).Returns(false);

            Assert.That(underTest.Handle(new StartNextSyncEvent()), Is.True);
            queue.Setup(q => q.IsEmpty).Returns(true);

            Assert.That(underTest.Handle(Mock.Of<ISyncEvent>()), Is.False);
            queue.Verify(q => q.AddEvent(It.Is<StartNextSyncEvent>(e => e.FullSyncRequested == false)), Times.Once());

            Assert.That(underTest.Handle(Mock.Of<ISyncEvent>()), Is.False);
            queue.Verify(q => q.AddEvent(It.Is<StartNextSyncEvent>(e => e.FullSyncRequested == false)), Times.Once());
        }

        [Test]
        public void FullSyncFlagIsStored() {
            var queue = new Mock<ISyncEventQueue>();
            var underTest = new DelayRetryAndNextSyncEventHandler(queue.Object);
            queue.Setup(q => q.IsEmpty).Returns(false);

            Assert.That(underTest.Handle(new StartNextSyncEvent(true)), Is.True);

            queue.Setup(q => q.IsEmpty).Returns(true);
            Assert.That(underTest.Handle(new StartNextSyncEvent(false)), Is.True);

            queue.Verify(q => q.AddEvent(It.Is<StartNextSyncEvent>(e => e.FullSyncRequested == true)), Times.Once());
        }

        [Test]
        public void RetryEventsAreSavedAndReinsertedAfterNonFullSyncRequested() {
            var queue = new Mock<ISyncEventQueue>();
            var underTest = new DelayRetryAndNextSyncEventHandler(queue.Object);
            queue.Setup(q => q.IsEmpty).Returns(true);

            var syncEvent1 = new FileEvent(Mock.Of<IFileInfo>()) { RetryCount = 1 };
            var syncEvent2 = new FileEvent(Mock.Of<IFileInfo>()) { RetryCount = 1 };

            Assert.That(underTest.Handle(syncEvent1), Is.True);
            Assert.That(underTest.Handle(syncEvent2), Is.True);
            queue.Verify(q => q.AddEvent(syncEvent1), Times.Never());
            queue.Verify(q => q.AddEvent(syncEvent2), Times.Never());

            Assert.That(underTest.Handle(new StartNextSyncEvent(false)), Is.True);

            queue.Verify(q => q.AddEvent(syncEvent1), Times.Once());
            queue.Verify(q => q.AddEvent(syncEvent2), Times.Once());
        }

        [Test]
        public void RetryEventsAreDroppedWhenFullSyncRequested() {
            var queue = new Mock<ISyncEventQueue>();
            var underTest = new DelayRetryAndNextSyncEventHandler(queue.Object);
            queue.Setup(q => q.IsEmpty).Returns(true);

            var syncEvent1 = new FileEvent(Mock.Of<IFileInfo>()) { RetryCount = 1 };

            Assert.That(underTest.Handle(syncEvent1), Is.True);

            Assert.That(underTest.Handle(new StartNextSyncEvent(true)), Is.True);

            queue.Verify(q => q.AddEvent(syncEvent1), Times.Never());
        }

        [Test]
        public void FullSyncFlagIsResetAfterOneDelayedFullSyncRequest() {
            var queue = new Mock<ISyncEventQueue>();
            var underTest = new DelayRetryAndNextSyncEventHandler(queue.Object);
            queue.Setup(q => q.IsEmpty).Returns(false);

            Assert.That(underTest.Handle(new StartNextSyncEvent(true)), Is.True);
            queue.Setup(q => q.IsEmpty).Returns(true);
            Assert.That(underTest.Handle(new StartNextSyncEvent(false)), Is.True);

            queue.Verify(q => q.AddEvent(It.Is<StartNextSyncEvent>(e => e.FullSyncRequested == true)), Times.Once());

            queue.Setup(q => q.IsEmpty).Returns(false);
            Assert.That(underTest.Handle(new StartNextSyncEvent(false)), Is.True);
            queue.Setup(q => q.IsEmpty).Returns(true);
            Assert.That(underTest.Handle(new StartNextSyncEvent(false)), Is.True);

            queue.Verify(q => q.AddEvent(It.Is<StartNextSyncEvent>(e => e.FullSyncRequested == true)), Times.Once());
            queue.Verify(q => q.AddEvent(It.Is<StartNextSyncEvent>(e => e.FullSyncRequested == false)), Times.Once());

            queue.Setup(q => q.IsEmpty).Returns(false);
            Assert.That(underTest.Handle(new StartNextSyncEvent(true)), Is.True);
            queue.Setup(q => q.IsEmpty).Returns(true);
            Assert.That(underTest.Handle(new StartNextSyncEvent(false)), Is.True);

            queue.Verify(q => q.AddEvent(It.Is<StartNextSyncEvent>(e => e.FullSyncRequested == true)), Times.Exactly(2));
            queue.Verify(q => q.AddEvent(It.Is<StartNextSyncEvent>(e => e.FullSyncRequested == false)), Times.Once());
        }
    }
}