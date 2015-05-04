//-----------------------------------------------------------------------
// <copyright file="SetupRepositoryTest.cs" company="GRAU DATA AG">
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

    using MockedServer;

    using NUnit.Framework;

    [TestFixture]
    public class SetupRepositoryTest {
        [Test, Category("Fast")]
        public void CreateRepository([Values(true, false)]bool withGivenId) {
            string repoId = withGivenId ? Guid.NewGuid().ToString() : null;
            var name = "my";

            var underTest = new MockedRepository(repoId, name).Object;

            Assert.That(underTest.Name, Is.EqualTo(name));
            if (withGivenId) {
                Assert.That(underTest.Id, Is.EqualTo(repoId));
            } else {
                Assert.That(underTest.Id, Is.Not.Null);
            }
        }

        [Test, Category("Fast")]
        public void CreateSession() {
            var repo = new MockedRepository();

            var session = repo.Object.CreateSession();

            Assert.That(session, Is.Not.Null);
            Assert.That(session.GetRootFolder(), Is.EqualTo(repo.MockedRootFolder.Object));
        }
    }
}