using System;

using CmisSync.Lib;
using CmisSync.Lib.Events;

using DotCMIS.Exceptions;

using NUnit.Framework;

using Moq;

namespace TestLibrary.EventsTests
{
    [TestFixture]
    public class ProxyAuthRequiredEventTest
    {
        [Test, Category("Fast")]
        public void ConstructorWorksWithExceptionAsParameter()
        {
            new ProxyAuthRequiredEvent(new Mock<CmisRuntimeException>().Object);
        }

        [Test, Category("Fast")]
        public void ReturnsStringOnToString()
        {
            var exception = new Mock<CmisRuntimeException>();
            string message = "AuthRequiredException";
            exception.Setup(e => e.ToString()).Returns(message);
            exception.Setup(e => e.Message).Returns(message);
            var ev = new ProxyAuthRequiredEvent(exception.Object);
            string s = ev.ToString();
            Assert.IsNotNullOrEmpty(s);
        }
    }
}

