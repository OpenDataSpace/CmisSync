
namespace TestLibrary.IntegrationTests.MockedServerTests {
    using System;

    using MockedServer;

    using NUnit.Framework;

    [TestFixture]
    public class SetupRepositoryTest {
        [Test, Category("Fast")]
        public void CreateRepository() {
            var repoId = Guid.NewGuid().ToString();
            var name = "my";

            var underTest = new MockedRepository(repoId, name).Object;

            Assert.That(underTest.Name, Is.EqualTo(name));
            Assert.That(underTest.Id, Is.EqualTo(repoId));
        }

        [Test, Category("Fast")]
        public void CreateSession() {
            var repo = new MockedRepository(Guid.NewGuid().ToString());

            var session = repo.Object.CreateSession();

            Assert.That(session, Is.Not.Null);
            Assert.That(session.GetRootFolder(), Is.EqualTo(repo.MockedRootFolder.Object));
        }
    }
}