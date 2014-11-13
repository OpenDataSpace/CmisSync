
namespace TestLibrary.EventsTests
{
    using System;
    using System.Collections.Generic;

    using CmisSync.Lib.Events;

    using DotCMIS.Exceptions;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class PermissionDeniedEventCalculatesBlockingUntilTest
    {
        private readonly string formatedDate = "Fri, 07 Nov 2014 23:59:59 GMT";

        public static Dictionary<object, object> CreateHeader(params string[] entries) {
            var dictionary = new Dictionary<object, object>();
            dictionary.Add("Retry-After", entries);
            return dictionary;
        }

        [Test, Category("Fast")]
        public void FailsIfNoExceptionIsPassed()
        {
            Assert.Throws<ArgumentNullException>(() => new PermissionDeniedEvent(null));
        }

        [Test, Category("Fast")]
        public void PermissionDeniedExceptionWithoutHeaders() {
            Assert.That(new PermissionDeniedEvent(new CmisPermissionDeniedException()).IsBlockedUntil, Is.Null);
        }

        [Test, Category("Fast")]
        public void CalculatesBlockingUntilDateWithFormattedDateAsInput() {
            var exception = Mock.Of<CmisPermissionDeniedException>(
                e => e.Data == CreateHeader(this.formatedDate));

            var underTest = new PermissionDeniedEvent(exception);

            Assert.That(underTest.IsBlockedUntil, Is.EqualTo(DateTime.Parse(this.formatedDate)));
        }

        [Test, Category("Fast")]
        public void CalculatesBlockingUntilDate() {
            long seconds = 120;
            var exception = Mock.Of<CmisPermissionDeniedException>(
                e => e.Data == CreateHeader(seconds.ToString()));

            var underTest = new PermissionDeniedEvent(exception);

            Assert.That(underTest.IsBlockedUntil, Is.EqualTo(DateTime.UtcNow + TimeSpan.FromSeconds(seconds)).Within(1).Seconds);
        }

        [Test, Category("Fast")]
        public void DoesNotCalculatesUntilDateIfHeaderContainsNotParsableDate() {
            var exception = Mock.Of<CmisPermissionDeniedException>(
                e => e.Data == CreateHeader("no date"));

            var underTest = new PermissionDeniedEvent(exception);

            Assert.That(underTest.IsBlockedUntil, Is.Null);
        }
    }
}