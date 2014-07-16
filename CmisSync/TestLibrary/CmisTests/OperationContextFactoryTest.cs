//-----------------------------------------------------------------------
// <copyright file="OperationContextFactoryTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.UtilsTests
{
    using System;
    using System.Collections.Generic;

    using CmisSync.Lib.Cmis;

    using DotCMIS.Client;
    using DotCMIS.Enums;

    using Moq;

    using NUnit.Framework;

    using TestUtils;

    [TestFixture]
    public class OperationContextFactoryTest
    {
        [Test, Category("Fast")]
        public void CreateContentChangeEventContext()
        {
            var result = Mock.Of<IOperationContext>();
            var session = this.CreateSessionMock(result);

            var context = OperationContextFactory.CreateContentChangeEventContext(session.Object);

            session.VerifyThatAllDefaultValuesAreSet();
            Assert.That(context, Is.EqualTo(result));
        }

        [Test, Category("Fast")]
        public void CreateCrawlerContext()
        {
            var result = Mock.Of<IOperationContext>();
            var session = this.CreateSessionMock(result);

            var context = OperationContextFactory.CreateCrawlContext(session.Object);

            session.VerifyThatAllDefaultValuesAreSet();
            session.VerifyThatCrawlValuesAreSet();
            Assert.That(context, Is.EqualTo(result));
        }

        [Test, Category("Fast")]
        public void CreateDefaultContext()
        {
            var result = Mock.Of<IOperationContext>();
            var session = this.CreateSessionMock(result);

            var context = OperationContextFactory.CreateDefaultContext(session.Object);

            session.VerifyThatAllDefaultValuesAreSet();
            session.VerifyThatFilterContainsPath();
            Assert.That(context, Is.EqualTo(result));
        }

        [Test, Category("Fast")]
        public void CreateNonCachingAndPathIncludingContext()
        {
            var result = Mock.Of<IOperationContext>();
            var session = this.CreateSessionMock(result);

            var context = OperationContextFactory.CreateNonCachingPathIncludingContext(session.Object);

            session.VerifyThatAllDefaultValuesAreSet();
            session.VerifyThatFilterContainsPath();
            session.VerifyThatCachingIsDisabled();
            Assert.That(context, Is.EqualTo(result));
        }

        private Mock<ISession> CreateSessionMock(IOperationContext result)
        {
            Mock<ISession> sessionMock = new Mock<ISession>();
            sessionMock.Setup(s => s.CreateOperationContext(
                It.IsAny<HashSet<string>>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<IncludeRelationshipsFlag>(),
                It.IsAny<HashSet<string>>(),
                It.IsAny<bool>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<int>())).Returns(result);
            return sessionMock;
        }
    }
}