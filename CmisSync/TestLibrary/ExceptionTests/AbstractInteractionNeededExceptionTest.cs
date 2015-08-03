//-----------------------------------------------------------------------
// <copyright file="AbstractInteractionNeededExceptionTest.cs" company="GRAU DATA AG">
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
﻿
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