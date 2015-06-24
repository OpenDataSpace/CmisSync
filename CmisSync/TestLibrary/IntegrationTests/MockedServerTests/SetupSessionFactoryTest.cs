//-----------------------------------------------------------------------
// <copyright file="SetupSessionFactoryTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.IntegrationTests.MockedServerTests {
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DotCMIS;

    using MockedServer;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class SetupSessionFactoryTest {
        [Test, Category("Fast")]
        public void CreateFactoryAndReturnRepositories() {
            var repo = new MockedRepository();
            var underTest = new MockedSessionFactory(repos: repo.Object);
            var cmisParameters = new Dictionary<string, string>();

            var repos = underTest.Object.GetRepositories(cmisParameters);

            Assert.That(repos.First(), Is.EqualTo(repo.Object));
        }

        [Test, Category("Fast")]
        public void CreateFactoryAndCreateSession() {
            var repo = new MockedRepository();
            var underTest = new MockedSessionFactory(repos: repo.Object);
            var cmisParameters = new Dictionary<string, string>();
            cmisParameters[SessionParameter.RepositoryId] = repo.Id;

            var session = underTest.Object.CreateSession(cmisParameters);

            Assert.That(session, Is.Not.Null);
            repo.Verify(r => r.CreateSession(), Times.Once);
        }
    }
}