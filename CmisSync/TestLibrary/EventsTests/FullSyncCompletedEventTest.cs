using System;

using CmisSync.Lib.Events;

using NUnit.Framework;

using Moq;
namespace TestLibrary.EventsTests
{ 
    [TestFixture]
    public class FullSyncCompletedEventTest
    {
        [Test, Category("Fast")]
        public void ContructorTest() {
            var start = new Mock<StartNextSyncEvent>(false).Object;
            var complete = new FullSyncCompletedEvent(start);
            Assert.AreEqual(start, complete.StartEvent);
        }

        [Test, Category("Fast")]
        public void ConstructorFailsOnNullParameterTest()
        {
            try{
                new FullSyncCompletedEvent(null);
                Assert.Fail();
            }catch(ArgumentNullException) {}
        }

        [Test, Category("Fast")]
        public void ParamTest () {
            string key = "key";
            string value = "value";
            string result;
            var start = new StartNextSyncEvent(false);
            start.SetParam(key, value);
            var complete = new FullSyncCompletedEvent(start);
            Assert.IsTrue(complete.StartEvent.TryGetParam(key, out result));
            Assert.AreEqual(value, result);
        }
    }
}

