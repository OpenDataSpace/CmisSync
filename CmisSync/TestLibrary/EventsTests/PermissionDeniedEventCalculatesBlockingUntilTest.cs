//-----------------------------------------------------------------------
// <copyright file="PermissionDeniedEventCalculatesBlockingUntilTest.cs" company="GRAU DATA AG">
//
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General private License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//
//   This program is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//   GNU General private License for more details.
//
//   You should have received a copy of the GNU General private License
//   along with this program. If not, see http://www.gnu.org/licenses/.
//
// </copyright>
//-----------------------------------------------------------------------

namespace TestLibrary.EventsTests {
    using System;
    using System.Collections.Generic;

    using CmisSync.Lib.Events;

    using DotCMIS.Exceptions;

    using Moq;

    using NUnit.Framework;

    [TestFixture, Category("Fast")]
    public class PermissionDeniedEventCalculatesBlockingUntilTest {
        private readonly string formatedDate = "Fri, 07 Nov 2014 23:59:59 GMT";

        public static Dictionary<object, object> CreateHeader(params string[] entries) {
            var dictionary = new Dictionary<object, object>();
            dictionary.Add("Retry-After", entries);
            return dictionary;
        }

        [Test]
        public void FailsIfNoExceptionIsPassed() {
            Assert.Throws<ArgumentNullException>(() => new PermissionDeniedEvent(null));
        }

        [Test]
        public void PermissionDeniedExceptionWithoutHeaders() {
            Assert.That(new PermissionDeniedEvent(new CmisPermissionDeniedException()).IsBlockedUntil, Is.Null);
        }

        [Test]
        public void CalculatesBlockingUntilDateWithFormattedDateAsInput() {
            var exception = Mock.Of<CmisPermissionDeniedException>(
                e => e.Data == CreateHeader(this.formatedDate));

            var underTest = new PermissionDeniedEvent(exception);

            Assert.That(underTest.IsBlockedUntil, Is.EqualTo(DateTime.Parse(this.formatedDate)));
        }

        [Test]
        public void CalculatesBlockingUntilDate() {
            long seconds = 120;
            var exception = Mock.Of<CmisPermissionDeniedException>(
                e => e.Data == CreateHeader(seconds.ToString()));

            var underTest = new PermissionDeniedEvent(exception);

            Assert.That(underTest.IsBlockedUntil, Is.EqualTo(DateTime.UtcNow + TimeSpan.FromSeconds(seconds)).Within(1).Seconds);
        }

        [Test]
        public void DoesNotCalculatesUntilDateIfHeaderContainsNotParsableDate() {
            var exception = Mock.Of<CmisPermissionDeniedException>(
                e => e.Data == CreateHeader("no date"));

            var underTest = new PermissionDeniedEvent(exception);

            Assert.That(underTest.IsBlockedUntil, Is.Null);
        }
    }
}