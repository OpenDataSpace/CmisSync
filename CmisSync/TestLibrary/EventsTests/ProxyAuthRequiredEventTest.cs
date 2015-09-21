//-----------------------------------------------------------------------
// <copyright file="ProxyAuthRequiredEventTest.cs" company="GRAU DATA AG">
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

    using CmisSync.Lib.Events;

    using DotCMIS.Exceptions;

    using Moq;

    using NUnit.Framework;

    [TestFixture, Category("Fast")]
    public class ProxyAuthRequiredEventTest {
        [Test]
        public void ConstructorWorksWithExceptionAsParameter() {
            new ProxyAuthRequiredEvent(new Mock<CmisRuntimeException>().Object);
        }

        [Test]
        public void ReturnsStringOnToString() {
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