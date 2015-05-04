
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