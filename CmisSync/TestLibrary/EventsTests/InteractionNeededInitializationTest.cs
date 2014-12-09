
namespace TestLibrary.EventsTests
{
    using System;

    using CmisSync.Lib.Consumer;
    using CmisSync.Lib.Events;

    using DotCMIS.Exceptions;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class InteractionNeededInitializationTest
    {
        [Test, Category("Fast")]
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

        [Test, Category("Fast")]
        public void InitializeWithoutExceptionFails() {
            Assert.Throws<ArgumentNullException>(() => new InteractionNeededEvent((InteractionNeededException)null));
        }

        [Test, Category("Fast")]
        public void InitializeWithNullString() {
            var underTest = new InteractionNeededEvent((string)null);
            Assert.That(underTest.Title, Is.Not.Null);
            Assert.That(underTest.Description, Is.Not.Null);
            Assert.That(underTest.Details, Is.Not.Null);
        }

        [Test, Category("Fast")]
        public void InitializeWithCmisException() {
            string errorContent = "error content";
            string message = "message";
            var ex = new CmisBaseException(message, errorContent);
            var exception = new InteractionNeededException(message, ex);
            var underTest = new InteractionNeededEvent(exception);

            Assert.That(underTest.Description, Is.EqualTo(message));
            Assert.That(underTest.Details, Is.EqualTo(errorContent));
        }

        [Test, Category("Fast")]
        public void InitializeWithString() {
            string desc = "desc";
            string message = "message";

            var underTest = new InteractionNeededEvent(message) { Details = desc};

            Assert.That(underTest.Description, Is.EqualTo(message));
            Assert.That(underTest.Details, Is.EqualTo(desc));
        }

        [Test, Category("Fast")]
        public void InitializeActions() {
            int called = 0;
            var action = new Action(delegate(){called++;});
            var ex = new InteractionNeededException();
            ex.Actions.Add("invoke", action);
            var underTest = new InteractionNeededEvent(ex);
            underTest.Actions["invoke"]();

            Assert.That(underTest.Actions.Count, Is.EqualTo(1));
            Assert.That(called, Is.EqualTo(1));
        }
    }
}