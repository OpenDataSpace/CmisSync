//-----------------------------------------------------------------------
// <copyright file="MockedSessionFactory.cs" company="GRAU DATA AG">
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

namespace TestLibrary.MockedServer {
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DotCMIS;
    using DotCMIS.Binding;
    using DotCMIS.Client;
    using DotCMIS.Client.Impl.Cache;

    using Moq;

    public class MockedSessionFactory : Mock<ISessionFactory> {
        public MockedSessionFactory(MockBehavior behavior = MockBehavior.Strict, params IRepository[] repos) : base(behavior) {
            this.Repositories = new List<IRepository>(repos);
            this.Setup(
                m => m.GetRepositories(
                It.IsAny<IDictionary<string, string>>()))
                .Returns(() => new List<IRepository>(this.Repositories));
            this.Setup(
                m => m.CreateSession(
                It.IsAny<IDictionary<string, string>>()))
                .Returns<IDictionary<string, string>>((parameters) => this.Repositories.First(entry => entry.Id == parameters[SessionParameter.RepositoryId]).CreateSession());
            this.Setup(
                m => m.CreateSession(
                It.IsAny<IDictionary<string, string>>(),
                It.IsAny<IObjectFactory>(),
                It.IsAny<IAuthenticationProvider>(),
                It.IsAny<ICache>()))
                .Returns<IDictionary<string, string>, IObjectFactory, IAuthenticationProvider, ICache>((parameters, fact, auth, cache) => this.Repositories.First(entry => entry.Id == parameters[SessionParameter.RepositoryId]).CreateSession());
        }

        public IList<IRepository> Repositories { get; set; }
    }
}