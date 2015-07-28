
namespace TestLibrary.ExceptionTests {
    using System;
    using System.Runtime.Serialization;

    using CmisSync.Lib.Exceptions;

    using NUnit.Framework;

    using Moq;

    [TestFixture, Category("Fast")]
    public class AbstractInteractionNeededExceptionTest {
        [Test]
        public void ConstructorInitializes() {
            var underTest = new TestClass();
            Assert.That(underTest.Level, Is.EqualTo(ExceptionLevel.Undecided));
        }

        [Test]
        public void ConstructorTakesMessage() {
            var msg = "Message";
            var underTest = new TestClass(msg);

            Assert.That(underTest.Level, Is.EqualTo(ExceptionLevel.Undecided));
            Assert.That(underTest.Message, Is.EqualTo(msg));
        }

        [Test]
        public void ConstructorTakesMessageAndException() {
            var msg = "Message";
            var exception = Mock.Of<Exception>();
            var underTest = new TestClass(msg, exception);

            Assert.That(underTest.Level, Is.EqualTo(ExceptionLevel.Undecided));
            Assert.That(underTest.Message, Is.EqualTo(msg));
            Assert.That(underTest.InnerException, Is.EqualTo(exception));
        }

        private class TestClass : AbstractInteractionNeededException {
            public TestClass() : base() { }
            public TestClass(string msg) : base(msg) { }
            public TestClass(string msg, Exception inner) : base(msg, inner) { }
        }
    }
}