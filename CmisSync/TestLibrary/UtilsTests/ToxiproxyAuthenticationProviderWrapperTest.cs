//-----------------------------------------------------------------------
// <copyright file="ToxiproxyAuthenticationProviderWrapperTest.cs" company="GRAU DATA AG">
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
namespace TestLibrary.UtilsTests {
    using System;

    using DotCMIS.Binding;

    using Moq;

    using NUnit.Framework;

    using TestUtils.ToxiproxyUtils;

    [TestFixture, Category("Fast")]
    public class ToxiproxyAuthenticationProviderWrapperTest {
        [Test]
        public void ConstructorTakesOriginal() {
            new ToxiproxyAuthenticationProviderWrapper(Mock.Of<IAuthenticationProvider>());
        }

        [Test]
        public void ConstructorThrowsExceptionIfNullIsPassed() {
            Assert.Throws<ArgumentNullException>(() => new ToxiproxyAuthenticationProviderWrapper(null));
        }

        [Test]
        public void SessionPropertyIsWrapped() {
            var auth = new Mock<IAuthenticationProvider>(MockBehavior.Strict);
            var session = Mock.Of<IBindingSession>();
            auth.SetupProperty(a => a.Session);
            var underTest = new ToxiproxyAuthenticationProviderWrapper(auth.Object);

            auth.VerifyGet(a => a.Session, Times.Never());
            auth.VerifySet(a => a.Session = It.IsAny<IBindingSession>(), Times.Never());
            Assert.That(underTest.Session, Is.Null);
            auth.VerifyGet(a => a.Session, Times.Once());

            underTest.Session = session;

            auth.VerifySet(a => a.Session = It.IsAny<IBindingSession>(), Times.Once());
            Assert.That(underTest.Session, Is.EqualTo(session));
            auth.VerifyGet(a => a.Session, Times.Exactly(2));
        }

        [Test]
        public void HandleResponseIsWrapped() {
            var auth = new Mock<IAuthenticationProvider>(MockBehavior.Strict);
            var response = new Mock<object>(MockBehavior.Strict).Object;
            auth.Setup(a => a.HandleResponse(response));
            var underTest = new ToxiproxyAuthenticationProviderWrapper(auth.Object);

            auth.Verify(a => a.HandleResponse(It.IsAny<object>()), Times.Never());

            underTest.HandleResponse(response);

            auth.Verify(a => a.HandleResponse(It.IsAny<object>()), Times.Once());
        }

        [Test]
        public void AuthenticateIsWrapped() {
            var auth = new Mock<IAuthenticationProvider>(MockBehavior.Strict);
            var connection = new Mock<object>(MockBehavior.Strict).Object;
            auth.Setup(a => a.Authenticate(connection));
            var underTest = new ToxiproxyAuthenticationProviderWrapper(auth.Object);

            auth.Verify(a => a.Authenticate(It.IsAny<object>()), Times.Never());

            underTest.Authenticate(connection);

            auth.Verify(a => a.Authenticate(connection), Times.Once());
        }

        [Test]
        public void NotificationOnAuthenticate() {
            int notified = 0;
            object connection = Mock.Of<object>();
            var underTest = new ToxiproxyAuthenticationProviderWrapper(Mock.Of<IAuthenticationProvider>());
            underTest.OnAuthenticate += (object c) => {
                Assert.That(c, Is.EqualTo(connection));
                notified++;
            };
            underTest.Authenticate(connection);
            Assert.That(notified, Is.EqualTo(1));
        }

        [Test]
        public void NotificationOnResponse() {
            int notified = 0;
            object response = Mock.Of<object>();
            var underTest = new ToxiproxyAuthenticationProviderWrapper(Mock.Of<IAuthenticationProvider>());
            underTest.OnResponse += (object r) => {
                Assert.That(r, Is.EqualTo(response));
                notified++;
            };
            underTest.HandleResponse(response);
            Assert.That(notified, Is.EqualTo(1));
        }
    }
}