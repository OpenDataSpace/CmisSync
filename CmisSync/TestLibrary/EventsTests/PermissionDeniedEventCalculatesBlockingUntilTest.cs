
namespace TestLibrary.EventsTests
{
    using System;

    using CmisSync.Lib.Events;

    using DotCMIS.Exceptions;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class PermissionDeniedEventCalculatesBlockingUntilTest
    {
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        [Test, Category("Fast")]
        public void FailsIfNoExceptionIsPassed()
        {
            Assert.Throws<ArgumentNullException>(() => new PermissionDeniedEvent(null));
        }

        [Test, Category("Fast")]
        public void PermissionDeniedExceptionWithoutErrorContent() {
            Assert.That(new PermissionDeniedEvent(new CmisPermissionDeniedException()).IsBlockedUntil, Is.Null);
        }

        [Test, Category("Fast")]
        public void CalculatesBlockingUntilDateWithZeroAsInput() {
            var exception = Mock.Of<CmisPermissionDeniedException>(
                e => e.ErrorContent == "0");

            var underTest = new PermissionDeniedEvent(exception);

            Assert.That(underTest.IsBlockedUntil, Is.EqualTo(UnixEpoch));
        }

        [Test, Category("Fast")]
        public void CalculatesBlockingUntilDateWithZeroAndLineBreakAsInput() {
            var exception = Mock.Of<CmisPermissionDeniedException>(
                e => e.ErrorContent == "0\n");

            var underTest = new PermissionDeniedEvent(exception);

            Assert.That(underTest.IsBlockedUntil, Is.EqualTo(UnixEpoch));
        }

        [Test, Category("Fast")]
        public void CalculatesBlockingUntilDate() {
            var now = DateTime.UtcNow;
            var timespan = now - UnixEpoch;
            var exception = Mock.Of<CmisPermissionDeniedException>(
                e => e.ErrorContent == ((long)timespan.TotalMilliseconds).ToString());

            var underTest = new PermissionDeniedEvent(exception);

            Assert.That(underTest.IsBlockedUntil, Is.EqualTo(now).Within(1).Seconds);
        }

        [Test, Category("Fast")]
        public void DoesNotCalculatesUntilDateIfContentIsNoNumber() {
            var exception = Mock.Of<CmisPermissionDeniedException>(
                e => e.ErrorContent == "text");

            var underTest = new PermissionDeniedEvent(exception);

            Assert.That(underTest.IsBlockedUntil, Is.Null);
        }

        [Test, Category("Fast")]
        public void DoesNotCalculatesUntilDateIfContentIsEmptyString() {
            var exception = Mock.Of<CmisPermissionDeniedException>(
                e => e.ErrorContent == string.Empty);
            Assert.That(new PermissionDeniedEvent(exception).IsBlockedUntil, Is.Null);
        }
    }
}