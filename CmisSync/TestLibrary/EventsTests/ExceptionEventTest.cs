using System;

using CmisSync.Lib.Events;

using NUnit.Framework;

using Moq;

namespace TestLibrary.EventsTests.ExceptionEventsTests
{
    [TestFixture]
    public class BaseExceptionEventTest
    {
        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorFailsWithNullException()
        {
            new ExceptionEvent(null);
        }

        [Test, Category("Fast")]
        public void ConstructorWithValidInput()
        {
            var exception = new Mock<Exception>().Object;
            var ev = new ExceptionEvent(exception);
            Assert.AreEqual(exception, ev.Exception);
        }

        [Test, Category("Fast")]
        public void ToStringIsImplemented()
        {
            var exception = new Mock<Exception>(""){CallBase=true}.Object;
            var ev = new ExceptionEvent(exception);
            Assert.IsNotNull(ev.ToString());
        }
    }

    [TestFixture]
    public class PermissionDeniedEventTest
    {
        [Test, Category("Fast")]
        public void ConstructorWithValidInput()
        {
            var exception = new Mock<DotCMIS.Exceptions.CmisPermissionDeniedException>().Object;
            var ev = new PermissionDeniedEvent(exception);
            Assert.AreEqual(exception, ev.Exception);
        }
    }
}

