
namespace TestLibrary.CmisTests.UiUtilsTests {
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CmisSync.Lib.Cmis.UiUtils;
    using CmisSync.Lib.Config;

    using DotCMIS.Client;

    using MockedServer;

    using Moq;

    using NUnit.Framework;

    using TestUtils;

    [TestFixture]
    public class UiConvenienceExtendersTest {
        [Test, Category("Fast")]
        public void FilterHiddenRepos() {
            var listOfRepos = new List<LogonRepositoryInfo>();
            var visibleOne = new LogonRepositoryInfo(Guid.NewGuid().ToString(), "visible");
            var hiddenOne = new LogonRepositoryInfo(Guid.NewGuid().ToString(), "hidden");
            listOfRepos.Add(visibleOne);
            listOfRepos.Add(hiddenOne);
            var result = listOfRepos.WithoutHiddenOnce(new List<string>(new string[] { "hidden" }));
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result.First(), Is.EqualTo(visibleOne));
        }

        [Test, Category("Fast")]
        public void FilterHiddenReposWithoutGivenList() {
            if (ConfigManager.CurrentConfig.HiddenRepoNames.Count == 0) {
                Assert.Ignore("non repo is hidden by default");
            }

            var listOfRepos = new List<LogonRepositoryInfo>();
            var visibleOne = new LogonRepositoryInfo(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
            var hiddenOne = new LogonRepositoryInfo(Guid.NewGuid().ToString(), ConfigManager.CurrentConfig.HiddenRepoNames.First());
            listOfRepos.Add(visibleOne);
            listOfRepos.Add(hiddenOne);
            var result = listOfRepos.WithoutHiddenOnce();
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result.First(), Is.EqualTo(visibleOne));
        }

        [Test, Category("Fast")]
        public void GetRepositoriesReturnsListOfReposInOrder() {
            var underTest = new ServerCredentials {
                Address = new Uri("https://demo.deutsche-wolke.de/cmis/browser"),
                Binding = DotCMIS.BindingType.Browser,
                UserName = "userName",
                Password = "secret"
            };
            var name = "Name";
            var id = Guid.NewGuid().ToString();
            var sessionFactory = new Mock<ISessionFactory>();
            sessionFactory.SetupRepositories(Mock.Of<IRepository>(r => r.Id == id && r.Name == name), Mock.Of<IRepository>(r => r.Id == Guid.NewGuid().ToString() && r.Name == "other"));
            var repos = underTest.GetRepositories(sessionFactory.Object);

            Assert.That(repos.Count, Is.EqualTo(2));
            Assert.That(repos.First().Id, Is.EqualTo(id));
            Assert.That(repos.First().Name, Is.EqualTo(name));
        }
    }
}