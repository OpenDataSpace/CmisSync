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
namespace TestLibrary.QueueingTests
{
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.FileSystem;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class DelayRetryAndNextSyncEventHandlerTest
    {
        [Test, Category("Fast")]
        public void ConstructorTest() {
            var underTest = new DelayRetryAndNextSyncEventHandler(Mock.Of<ISyncEventQueue>());
        }

        [Test, Category("Fast")]
        public void DoesNotHandleNonRetryEvent() {
            var underTest = new DelayRetryAndNextSyncEventHandler(Mock.Of<ISyncEventQueue>());
            var fileEvent = new FileEvent(Mock.Of<IFileInfo>()) { RetryCount = 0 };
            Assert.False(underTest.Handle(fileEvent));
        }

        [Test, Category("Fast")]
        public void DelaysNextSyncEventUntilQueueEmpty() {
            var queue = new Mock<ISyncEventQueue>();
            var underTest = new DelayRetryAndNextSyncEventHandler(queue.Object);
            queue.Setup(q => q.IsEmpty).Returns(false);

            Assert.True(underTest.Handle(new StartNextSyncEvent()));
            queue.Setup(q => q.IsEmpty).Returns(true);

            Assert.False(underTest.Handle(Mock.Of<ISyncEvent>()));
            queue.Verify(q => q.AddEvent(It.Is<StartNextSyncEvent>(e => e.FullSyncRequested == false)));
        }

        [Test, Category("Fast")]
        public void DoNotDelayStartSyncWhenQueueEmpty() {
            var queue = new Mock<ISyncEventQueue>();
            queue.Setup(q => q.IsEmpty).Returns(true);
            var underTest = new DelayRetryAndNextSyncEventHandler(queue.Object);

            Assert.False(underTest.Handle(new StartNextSyncEvent()));
        }

        [Test, Category("Fast")]
        public void ResetNextSyncFlagAfterRequest() {
            var queue = new Mock<ISyncEventQueue>();
            var underTest = new DelayRetryAndNextSyncEventHandler(queue.Object);
            queue.Setup(q => q.IsEmpty).Returns(false);

            Assert.True(underTest.Handle(new StartNextSyncEvent()));
            queue.Setup(q => q.IsEmpty).Returns(true);

            Assert.False(underTest.Handle(Mock.Of<ISyncEvent>()));
            queue.Verify(q => q.AddEvent(It.Is<StartNextSyncEvent>(e => e.FullSyncRequested == false)), Times.Once());

            Assert.False(underTest.Handle(Mock.Of<ISyncEvent>()));
            queue.Verify(q => q.AddEvent(It.Is<StartNextSyncEvent>(e => e.FullSyncRequested == false)), Times.Once());
        }

        [Test, Category("Fast")]
        public void FullSyncFlagIsStored() {
            var queue = new Mock<ISyncEventQueue>();
            var underTest = new DelayRetryAndNextSyncEventHandler(queue.Object);
            queue.Setup(q => q.IsEmpty).Returns(false);

            Assert.True(underTest.Handle(new StartNextSyncEvent(true)));

            queue.Setup(q => q.IsEmpty).Returns(true);
            Assert.True(underTest.Handle(new StartNextSyncEvent(false)));

            queue.Verify(q => q.AddEvent(It.Is<StartNextSyncEvent>(e => e.FullSyncRequested == true)), Times.Once());
        }

        [Test, Category("Fast")]
        public void RetryEventsAreSavedAndReinsertedAfterNonFullSyncRequested() {
            var queue = new Mock<ISyncEventQueue>();
            var underTest = new DelayRetryAndNextSyncEventHandler(queue.Object);
            queue.Setup(q => q.IsEmpty).Returns(true);

            var syncEvent1 = new FileEvent(Mock.Of<IFileInfo>()) { RetryCount = 1 };
            var syncEvent2 = new FileEvent(Mock.Of<IFileInfo>()) { RetryCount = 1 };

            Assert.True(underTest.Handle(syncEvent1));
            Assert.True(underTest.Handle(syncEvent2));
            queue.Verify(q => q.AddEvent(syncEvent1), Times.Never());
            queue.Verify(q => q.AddEvent(syncEvent2), Times.Never());

            Assert.True(underTest.Handle(new StartNextSyncEvent(false)));

            queue.Verify(q => q.AddEvent(syncEvent1), Times.Once());
            queue.Verify(q => q.AddEvent(syncEvent2), Times.Once());
        }

        [Test, Category("Fast")]
        public void RetryEventsAreDroppedWhenFullSyncRequested() {
            var queue = new Mock<ISyncEventQueue>();
            var underTest = new DelayRetryAndNextSyncEventHandler(queue.Object);
            queue.Setup(q => q.IsEmpty).Returns(true);

            var syncEvent1 = new FileEvent(Mock.Of<IFileInfo>()) { RetryCount = 1 };

            Assert.True(underTest.Handle(syncEvent1));

            Assert.True(underTest.Handle(new StartNextSyncEvent(true)));

            queue.Verify(q => q.AddEvent(syncEvent1), Times.Never());
        }
    }
}
