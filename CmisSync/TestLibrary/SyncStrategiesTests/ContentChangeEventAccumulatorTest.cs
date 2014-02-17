using DotCMIS.Client;

using CmisSync.Lib.Events;
using CmisSync.Lib.Sync.Strategy;

using NUnit.Framework;

using Moq;
namespace TestLibrary.SyncStrategiesTests {

    [TestFixture]
    public class ContentChangeEventAccumulatorTest 
    {
        private static readonly string id = "myId";
        [Test, Category("Fast")]
        public void ConstructorTest () {
            var session = new Mock<ISession>();
            var accumulator = new ContentChangeEventAccumulator (session.Object);
            Assert.That(accumulator.Priority, Is.EqualTo(2000));
        }

        [Test, Category("Fast")]
        public void DocumentCreationAccumulated() {
            var session = new Mock<ISession>();
            var remoteObject = new Mock<ICmisObject>();
            session.Setup (s => s.GetObject (It.IsAny<string>())).Returns (remoteObject.Object);
            var accumulator = new ContentChangeEventAccumulator (session.Object);
            var contentChange = new ContentChangeEvent(DotCMIS.Enums.ChangeType.Created, id);

            Assert.That(accumulator.Handle(contentChange), Is.False);
            Assert.That(contentChange.CmisObject, Is.EqualTo(remoteObject.Object));
        }

        [Test, Category("Fast")]
        public void DocumentDeletionNotAccumulated() {
            var session = new Mock<ISession>();
            var contentChange = new ContentChangeEvent(DotCMIS.Enums.ChangeType.Deleted, id);
            var accumulator = new ContentChangeEventAccumulator (session.Object);

            accumulator.Handle(contentChange);
            Assert.That(contentChange.CmisObject, Is.Null);
            session.Verify(foo => foo.GetObject(It.IsAny<string>()), Times.Never());
        }

        [Test, Category("Fast")]
        public void DoesNotHandleWrongEvents() {
            var session = new Mock<ISession>();
            var contentChange = new Mock<ISyncEvent>().Object;
            var accumulator = new ContentChangeEventAccumulator (session.Object);

            Assert.That(accumulator.Handle(contentChange), Is.False);
        }
    }
}
