using CmisSync.Lib.Sync.Strategy;

using NUnit.Framework;

using Moq;
namespace TestLibrary.SyncStrategiesTests {

    [TestFixture]
    public class ContentChangeEventAccumulatorTest 
    {
        [Test, Category("Fast")]
        public void ConstructorTest () {
            var accumulator = new ContentChangeEventAccumulator ();
            Assert.That(accumulator.Priority, Is.EqualTo(2000));
        }
    }
}
