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

        public MockedFolder MockedRootFolder { get; set; }

        public string RepoName { get; set; }

        public string Id { get; set; }

        public string Description { get; set; }

        public string ProductName { get; set; }

        public string VendorName { get; set; }

        public MockedRepository(string id, string name = "name", MockedFolder rootFolder = null, MockBehavior behavior = MockBehavior.Strict) : base(behavior) {
            this.Id = id;
            this.RepoName = name;
            this.MockedRootFolder = rootFolder ?? new MockedFolder("/");
            this.Description = "desc";
            this.ProductName = "GRAU DATA AG in memory cmis repo";
            this.VendorName = "GRAU DATA AG";
            this.Setup(r => r.Name).Returns(() => this.RepoName);
            this.Setup(r => r.Id).Returns(() => this.Id);
            this.Setup(r => r.Description).Returns(() => this.Description);
            this.Setup(r => r.ProductName).Returns(() => this.ProductName);
            this.Setup(r => r.VendorName).Returns(() => this.VendorName);
            var acls = Mock.Of<IAclCapabilities>(
                c =>
                c.SupportedPermissions == SupportedPermissions.Basic &&
                c.PermissionMapping == new Dictionary<string, IPermissionMapping>());
            this.Setup(r => r.AclCapabilities).Returns(acls);
            this.Setup(r => r.CreateSession()).Returns(() => new MockedSession(this) { RootFolder = this.MockedRootFolder.Object }.Object);
        }
    }
}