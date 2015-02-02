//-----------------------------------------------------------------------
// <copyright file="MockOfISessionFactory.cs" company="GRAU DATA AG">
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

namespace TestLibrary.MockedServer
{
    using System;
    using System.Collections.Generic;

    using DotCMIS;
    using DotCMIS.Binding;
    using DotCMIS.Client;
    using DotCMIS.Client.Impl.Cache;

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
