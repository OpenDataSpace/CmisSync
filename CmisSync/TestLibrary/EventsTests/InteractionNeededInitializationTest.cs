//-----------------------------------------------------------------------
// <copyright file="InteractionNeededInitializationTest.cs" company="GRAU DATA AG">
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

    using CmisSync.Lib.Consumer;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Exceptions;

    using DotCMIS.Exceptions;

    using Moq;

    using NUnit.Framework;

    [TestFixture, Category("Fast")]
    public class InteractionNeededInitializationTest {
        [Test]
        public void InitializeWithException() {
            var ex = new InteractionNeededException();
            var underTest = new InteractionNeededEvent(ex);
            Assert.That(underTest.Exception, Is.EqualTo(ex));
            Assert.That(underTest.Actions, Is.Empty);
            Assert.That(underTest.AffectedFiles, Is.Empty);
            Assert.That(underTest.Title, Is.EqualTo(ex.GetType().Name));
            Assert.That(underTest.Description, Is.EqualTo(ex.Message));
            Assert.That(underTest.Details, Is.Not.Null);
        }

        [Test]
        public void InitializeWithoutExceptionFails() {
            Assert.Throws<ArgumentNullException>(() => new InteractionNeededEvent((InteractionNeededException)null));
        }

        [Test]
        public void InitializeWithNullString() {
            var underTest = new InteractionNeededEvent((string)null);
            Assert.That(underTest.Title, Is.Not.Null);
            Assert.That(underTest.Description, Is.Not.Null);
            Assert.That(underTest.Details, Is.Not.Null);
        }

        [Test]
        public void InitializeWithCmisException() {
            string errorContent = "error content";
            string message = "message";
            var ex = new CmisBaseException(message, errorContent);
            var exception = new InteractionNeededException(message, ex);
            var underTest = new InteractionNeededEvent(exception);

            Assert.That(underTest.Description, Is.EqualTo(message));
            Assert.That(underTest.Details, Is.EqualTo(errorContent));
        }

        [Test]
        public void InitializeWithString() {
            string desc = "desc";
            string message = "message";

            var underTest = new InteractionNeededEvent(message) { Details = desc };

            Assert.That(underTest.Description, Is.EqualTo(message));
            Assert.That(underTest.Details, Is.EqualTo(desc));
        }

        [Test]
        public void InitializeActions() {
            int called = 0;
            var action = new Action(delegate() { called++; });
            var ex = new InteractionNeededException();
            ex.Actions.Add("invoke", action);
            var underTest = new InteractionNeededEvent(ex);
            underTest.Actions["invoke"]();

            Assert.That(underTest.Actions.Count, Is.EqualTo(1));
            Assert.That(called, Is.EqualTo(1));
        }
    }
}