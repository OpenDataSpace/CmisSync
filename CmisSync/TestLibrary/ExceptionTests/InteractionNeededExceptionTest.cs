
namespace TestLibrary.ExceptionTests {
    using System;

    using CmisSync.Lib.Exceptions;

    using NUnit.Framework;

    using Moq;

    [TestFixture, Category("Fast")]
    public class InteractionNeededExceptionTest {
        [Test]
        public void ExceptionLevelIsInfo() {
            Assert.That(new InteractionNeededException().Level, Is.EqualTo(ExceptionLevel.Info));
            Assert.That(new InteractionNeededException("msg").Level, Is.EqualTo(ExceptionLevel.Info));
            Assert.That(new InteractionNeededException("msg", Mock.Of<Exception>()).Level, Is.EqualTo(ExceptionLevel.Info));
        }
    }
}