//-----------------------------------------------------------------------
// <copyright file="MockedRepository.cs" company="GRAU DATA AG">
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

    using DotCMIS.Client;
    using DotCMIS.Data;
    using DotCMIS.Enums;

    using Moq;

    using TestLibrary.TestUtils;

    public class MockedRepository : MockedRepositoryInfo<IRepository> {

        public MockedFolder MockedRootFolder { get; set; }

        public MockedRepository(string id = null, string name = "name", MockedFolder rootFolder = null, MockBehavior behavior = MockBehavior.Strict) : base(id, name, behavior) {
            this.MockedRootFolder = rootFolder ?? new MockedFolder("/");
            this.RootFolderId = this.MockedRootFolder.Object.Id;
            this.Setup(r => r.CreateSession()).Returns(() => {
                var session = new MockedSession(this.Object) { RootFolder = this.MockedRootFolder.Object };
                session.AddObjects(this.MockedRootFolder.Object);
                return session.Object;
            });
        }
    }
}