using System;

using DotCMIS.Client;
using DotCMIS.Exceptions;

using CmisSync.Lib.Events;
using CmisSync.Lib.Sync.Strategy;

using NUnit.Framework;

using Moq;
namespace TestLibrary.SyncStrategiesTests {

    [TestFixture]
    public class ContentChangeEventAccumulatorTest 
    {
        private static readonly string id = "myId";
        [Test, Category("Fast"), Category("ContentChange")]
        public void ConstructorTest () {
            var session = new Mock<ISession>();
            var accumulator = new ContentChangeEventAccumulator (session.Object, new Mock<ISyncEventQueue>().Object);
            Assert.That(accumulator.Priority, Is.EqualTo(2000));
        }

        [Test, Category("Fast"), Category("ContentChange")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DbNullContstructorTest() {
            new ContentChangeEventAccumulator(null, new Mock<ISyncEventQueue>().Object);
        }

        [Test, Category("Fast"), Category("ContentChange")]
        public void DocumentCreationAccumulated() {
            var session = new Mock<ISession>();
            var remoteObject = new Mock<ICmisObject>();
            session.Setup (s => s.GetObject (It.IsAny<string>())).Returns (remoteObject.Object);
            var accumulator = new ContentChangeEventAccumulator (session.Object, new Mock<ISyncEventQueue>().Object);
            var contentChange = new ContentChangeEvent(DotCMIS.Enums.ChangeType.Created, id);

            Assert.That(accumulator.Handle(contentChange), Is.False);
            Assert.That(contentChange.CmisObject, Is.EqualTo(remoteObject.Object));
        }

        [Test, Category("Fast"), Category("ContentChange")]
        public void DocumentDeletionNotAccumulated() {
            var session = new Mock<ISession>();
            var contentChange = new ContentChangeEvent(DotCMIS.Enums.ChangeType.Deleted, id);
            var accumulator = new ContentChangeEventAccumulator (session.Object, new Mock<ISyncEventQueue>().Object);

            accumulator.Handle(contentChange);
            Assert.That(contentChange.CmisObject, Is.Null);
            session.Verify(foo => foo.GetObject(It.IsAny<string>()), Times.Never());
        }

        [Test, Category("Fast"), Category("ContentChange")]
        public void DoesNotHandleWrongEvents() {
            var session = new Mock<ISession>();
            var contentChange = new Mock<ISyncEvent>().Object;

            var accumulator = new ContentChangeEventAccumulator (session.Object, new Mock<ISyncEventQueue>().Object);
            Assert.That(accumulator.Handle(contentChange), Is.False);
        }

        [Test, Category("Fast"), Category("ContentChange")]
        public void IgnoreEventsThatHaveBeenDeleted() {
            var queue = new Mock<ISyncEventQueue>();
            var session = new Mock<ISession>();
            session.Setup (s => s.GetObject (It.IsAny<string>())).Throws (new CmisObjectNotFoundException());
            var accumulator = new ContentChangeEventAccumulator (session.Object, queue.Object);
            var contentChange = new ContentChangeEvent(DotCMIS.Enums.ChangeType.Created, id);

            Assert.That(accumulator.Handle(contentChange), Is.True);
            queue.Verify(q => q.AddEvent(It.IsAny<StartNextSyncEvent>()), Times.Never());
        }

        [Test, Category("Fast"), Category("ContentChange")]
        public void IgnoreEventsThatWeDontHaveAccessTo() {
            var queue = new Mock<ISyncEventQueue>();
            var session = new Mock<ISession>();
            session.Setup (s => s.GetObject (It.IsAny<string>())).Throws (new CmisPermissionDeniedException());
            var accumulator = new ContentChangeEventAccumulator (session.Object, queue.Object);
            var contentChange = new ContentChangeEvent(DotCMIS.Enums.ChangeType.Created, id);

            Assert.That(accumulator.Handle(contentChange), Is.True);
            queue.Verify(q => q.AddEvent(It.IsAny<StartNextSyncEvent>()), Times.Never());
        }

        [Test, Category("Fast"), Category("ContentChange")]
        public void ExceptionTriggersFullSync() {
            var queue = new Mock<ISyncEventQueue>();
            var session = new Mock<ISession>();
            session.Setup (s => s.GetObject (It.IsAny<string>())).Throws (new Exception());
            var accumulator = new ContentChangeEventAccumulator (session.Object, queue.Object);
            var contentChange = new ContentChangeEvent(DotCMIS.Enums.ChangeType.Created, id);

            Assert.That(accumulator.Handle(contentChange), Is.True);
            queue.Verify(q => q.AddEvent(It.IsAny<StartNextSyncEvent>()), Times.Once());
        }
    }
}
