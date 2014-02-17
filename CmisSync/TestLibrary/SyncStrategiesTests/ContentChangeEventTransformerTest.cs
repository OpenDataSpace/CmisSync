using System;

using CmisSync.Lib.Sync.Strategy;
using CmisSync.Lib.Events;

using NUnit.Framework;

using Moq;

namespace TestLibrary.SyncStrategiesTests {
    [TestFixture]
    public class ContentChangeEventTransformerTest {

        [Test]
        public void ConstructorTest() {
            var queue  = new Mock<ISyncEventQueue>();
            var transformer = new ContentChangeEventTransformer(queue.Object);
            Assert.That(transformer.Priority, Is.EqualTo(1000));
        }

        [Test]
        public void DocumentCreation() {
            
            throw new NotImplementedException();
        }
    }
}
