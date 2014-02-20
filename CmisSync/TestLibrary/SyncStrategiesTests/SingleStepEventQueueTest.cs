using NUnit.Framework;

using Moq;

using CmisSync.Lib.Events;
namespace TestLibrary.SyncStrategiesTests {

    [TestFixture]
    public class SingleStepEventQueueTest {

        [Test, Category("Fast")]
        public void InitialState() {
            var manager = new Mock<SyncEventManager>();
            var queue = new SingleStepEventQueue(manager.Object);
            Assert.That(queue.IsStopped, Is.True,"Queue starts stopped");
        }

        [Test, Category("Fast")]
        public void StartAndStopWorks() {
            var manager = new Mock<SyncEventManager>();
            var queue = new SingleStepEventQueue(manager.Object);
            var syncEvent = new Mock<ISyncEvent>();
            queue.AddEvent(syncEvent.Object);
            Assert.That(queue.IsStopped, Is.False,"Queue should not start immediatly");
            queue.Step();
            Assert.That(queue.IsStopped, Is.True,"Queue should be Stopped if empty again");
        }
        
        [Test, Category("Fast")]
        public void EventsGetForwarded() {
            var manager = new Mock<SyncEventManager>();
            var queue = new SingleStepEventQueue(manager.Object);
            var syncEvent = new Mock<ISyncEvent>();
            queue.AddEvent(syncEvent.Object);
            queue.Step();
            manager.Verify(m => m.Handle(syncEvent.Object), Times.Once());

        }

        [Test, Category("Fast")]
        public void QueueIsFifo() {
            var manager = new Mock<SyncEventManager>();
            var queue = new SingleStepEventQueue(manager.Object);
            var syncEvent1 = new Mock<ISyncEvent>();
            var syncEvent2 = new Mock<ISyncEvent>();
            queue.AddEvent(syncEvent1.Object);
            queue.AddEvent(syncEvent2.Object);
            queue.Step();
            manager.Verify(m => m.Handle(syncEvent1.Object), Times.Once());
            manager.Verify(m => m.Handle(syncEvent2.Object), Times.Never());
            queue.Step();
            manager.Verify(m => m.Handle(syncEvent1.Object), Times.Once());
            manager.Verify(m => m.Handle(syncEvent2.Object), Times.Once());

        }

    }
}
