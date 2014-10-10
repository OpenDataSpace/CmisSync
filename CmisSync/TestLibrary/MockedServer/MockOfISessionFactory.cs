using DotCMIS;
using DotCMIS.Binding;
using DotCMIS.Client.Impl.Cache;

namespace TestLibrary.MockedServer
{
    using System;
    using System.Collections.Generic;

    using DotCMIS.Client;

    using Moq;

    using TestLibrary.TestUtils;

    public static class MockOfISessionFactory
    {
        public static void SetupRepositories(this Mock<ISessionFactory> factory, params IRepository[] repos) {
            factory.Setup(f => f.GetRepositories(It.IsAny<IDictionary<string, string>>())).Returns(new List<IRepository>(repos));
            foreach (var repo in repos) {
                factory.Setup(
                    f => f.CreateSession(
                    It.Is<IDictionary<string, string>>(d => d[SessionParameter.RepositoryId] == repo.Id)))
                    .Returns(repo.CreateSession());
                factory.Setup(
                    f => f.CreateSession(
                    It.Is<IDictionary<string, string>>(d => d[SessionParameter.RepositoryId] == repo.Id),
                    It.IsAny<IObjectFactory>(),
                    It.IsAny<IAuthenticationProvider>(),
                    It.IsAny<ICache>()))
                    .Returns(repo.CreateSession());
            }
        }
    }
}