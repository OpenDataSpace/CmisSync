

namespace TestLibrary.MockedServer
{
    using System;
    using System.Collections.Generic;

    using DotCMIS.Client;

    using Moq;

    public static class MockOfISessionFactory
    {
        public static ISessionFactory CreateSessionFactory(params IRepository[] repos) {
            var factory = new Mock<ISessionFactory>();
            factory.Setup(f => f.GetRepositories(It.IsAny<IDictionary<string, string>>())).Returns(new List<IRepository>(repos));
            factory.Setup(f => f.CreateSession(It.IsAny<IDictionary<string, string>>())).Returns((ISession)null);
            return factory.Object;
        }
    }
}