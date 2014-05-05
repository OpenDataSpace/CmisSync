using CmisSync.Lib.Events;

using NUnit.Framework;
using Moq;

namespace TestLibrary.TestUtils
{
    [TestFixture]
    public class ObservableHandlerTest {
        [Test, Category("Fast")]
        public void TestFetch(){
            var handler = new ObservableHandler();
            var event1 = new Mock<ISyncEvent>().Object;
            var event2 = new Mock<ISyncEvent>().Object;
            Assert.That(handler.Handle(event1), Is.True);
            handler.Handle(event2);
            Assert.That(handler.list[0], Is.EqualTo(event1));
            Assert.That(handler.list[1], Is.EqualTo(event2));
        }
    }
}
