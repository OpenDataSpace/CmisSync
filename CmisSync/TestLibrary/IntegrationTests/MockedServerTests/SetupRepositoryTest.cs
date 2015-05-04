
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