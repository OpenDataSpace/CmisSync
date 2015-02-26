
namespace TestLibrary.EventsTests.ExceptionEventsTests {
    using System;

    using CmisSync.Lib.Events;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class PermissionDeniedEventTest {
        [Test, Category("Fast")]
        public void ConstructorWithValidInput() {
            var exception = new Mock<DotCMIS.Exceptions.CmisPermissionDeniedException>().Object;
            var ev = new PermissionDeniedEvent(exception);
            Assert.AreEqual(exception, ev.Exception);
        }
    }
}