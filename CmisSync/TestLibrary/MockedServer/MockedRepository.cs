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

    public class MockedRepository : Mock<IRepository> {
        private static Dictionary<string, MockedRepository> repositories = new Dictionary<string, MockedRepository>();

        private MockedFolder rootFolder = new MockedFolder("/");

        public IFolder RootFolder {
            get {
                return this.rootFolder.Object;
            }
        }

        public static MockedRepository GetRepository(string id) {
            lock (repositories) {
                MockedRepository repo;
                if (!repositories.TryGetValue(id, out repo)) {
                    repo = new MockedRepository(id);
                    repositories[id] = repo;
                }

                return repo;
            }
        }

        public void SetupName(string name) {
            this.Setup(r => r.Name).Returns(name);
        }

        public void Destroy() {
            repositories.Remove(this.Object.Id);
        }

        private MockedRepository(string id) : base(MockBehavior.Strict) {
            this.Setup(r => r.Name).Returns("name");
            this.Setup(r => r.Id).Returns(id);
            this.Setup(r => r.Description).Returns("desc");
            this.Setup(r => r.ProductName).Returns("GRAU DATA AG in memory cmis repo");
            this.Setup(r => r.VendorName).Returns("GRAU DATA AG");
            var acls = Mock.Of<IAclCapabilities>(
                c =>
                c.SupportedPermissions == SupportedPermissions.Basic &&
                c.PermissionMapping == new Dictionary<string, IPermissionMapping>());
            this.Setup(r => r.AclCapabilities).Returns(acls);
            this.Setup(r => r.CreateSession()).Returns(new MockedSession(this).Object);
        }
    }
}