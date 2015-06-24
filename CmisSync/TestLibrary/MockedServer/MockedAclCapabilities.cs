//-----------------------------------------------------------------------
// <copyright file="MockedAclCapabilities.cs" company="GRAU DATA AG">
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

    using DotCMIS.Data;
    using DotCMIS.Enums;

    using Moq;

    public class MockedAclCapabilities : Mock<IAclCapabilities> {
        public MockedAclCapabilities(MockBehavior behavior = MockBehavior.Strict) : base(behavior) {
            this.Setup(m => m.SupportedPermissions).Returns(() => this.SupportedPermissions);
            this.Setup(m => m.AclPropagation).Returns(() => this.AclPropagation);
            this.Setup(m => m.Permissions).Returns(() => new List<IPermissionDefinition>(this.Permissions));
            this.Setup(m => m.PermissionMapping).Returns(() => new Dictionary<string, IPermissionMapping>(this.PermissionMapping));
        }

        public SupportedPermissions? SupportedPermissions { get; set; }

        public AclPropagation? AclPropagation { get; set; }

        public IList<IPermissionDefinition> Permissions { get; set; }

        public IDictionary<string, IPermissionMapping> PermissionMapping { get; set; }
    }
}