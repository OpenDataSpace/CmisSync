
namespace TestLibrary.ExceptionTests {
    using System;

    using CmisSync.Lib.Exceptions;

    using DotCMIS.Exceptions;

    using NUnit.Framework;

    using Moq;

    [TestFixture, Category("Fast"), Category("Exception")]
    public class VirusDetectedExceptionTest {
        [Test]
        public void ConstructorFailsIfGivenExceptionIsNull() {
            Assert.Throws<ArgumentNullException>(() => new VirusDetectedException(null));
        }

        [Test]
        public void ConstructorFailsIfGivenExceptionIsNotAVirusException() {
            Assert.Throws<ArgumentException>(() => new VirusDetectedException(new CmisConstraintException()));
        }

        [Test]
        public void ConstructorTakesGivenException() {
            var mockedException = new Mock<CmisConstraintException>("Conflict", "infected file") { CallBase = true };

            var underTest = new VirusDetectedException(mockedException.Object);

            Assert.That(underTest.InnerException, Is.EqualTo(mockedException.Object));
        }
    }
}