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

    [TestFixture]
    public class OperationContextFactoryTest
    {
        [Test, Category("Fast")]
        public void CreateContentChangeEventContext()
        {
            var result = Mock.Of<IOperationContext>();
            var session = this.CreateSessionMock(result);

            var context = OperationContextFactory.CreateContentChangeEventContext(session.Object);

            this.VerifyThatAllDefaultValuesAreSet(session);
            Assert.That(context, Is.EqualTo(result));
        }

        [Test, Category("Fast")]
        public void CreateCrawlerContext()
        {
            var result = Mock.Of<IOperationContext>();
            var session = this.CreateSessionMock(result);

            var context = OperationContextFactory.CreateCrawlContext(session.Object);

            this.VerifyThatAllDefaultValuesAreSet(session);
            this.VerifyThatCrawlValuesAreSet(session);
            Assert.That(context, Is.EqualTo(result));
        }

        private void VerifyThatAllDefaultValuesAreSet(Mock<ISession> session) {
            session.Verify(
                s => s.CreateOperationContext(
                It.Is<HashSet<string>>(set =>
                                   set.Contains("cmis:name") &&
                                   set.Contains("cmis:parentId") &&
                                   set.Contains("cmis:objectId") &&
                                   set.Contains("cmis:changeToken") &&
                                   set.Contains("cmis:contentStreamFileName") &&
                                   set.Contains("cmis:lastModificationDate")),
                It.Is<bool>(acls => acls == false),
                It.Is<bool>(includeAllowableActions => includeAllowableActions == true),
                It.Is<bool>(includePolicies => includePolicies == false),
                It.Is<IncludeRelationshipsFlag>(relationship => relationship == IncludeRelationshipsFlag.None),
                It.Is<HashSet<string>>(set => set.Contains("cmis:none") && set.Count == 1),
                It.IsAny<bool>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.Is<int>(i => i > 1)),
                Times.Once());
        }

        private void VerifyThatCrawlValuesAreSet(Mock<ISession> session) {
            session.Verify(
                s => s.CreateOperationContext(
                It.Is<HashSet<string>>(set =>
                                   !set.Contains("cmis:path")),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<IncludeRelationshipsFlag>(),
                It.IsAny<HashSet<string>>(),
                It.IsAny<bool>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<int>()),
                Times.Once());
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