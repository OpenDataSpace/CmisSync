using System;

using CmisSync.Lib.Events;

using NUnit.Framework;

using Moq;
namespace TestLibrary.EventsTests
{ 
    [TestFixture]
    public class EncapsuledEventTest
    {
        [Test, Category("Fast")]
        public void ContructorTest() {
            var inner = new Mock<ISyncEvent>().Object;
            var outer = new EncapsuledEvent(inner);
            Assert.AreEqual(inner, outer.Event);
            try{
                new EncapsuledEvent(null);
                Assert.Fail();
            }catch(ArgumentNullException) {}
        }
    }
}

