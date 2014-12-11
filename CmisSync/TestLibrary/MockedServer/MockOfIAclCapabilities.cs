//-----------------------------------------------------------------------
// <copyright file="MockOfIAclCapabilities.cs" company="GRAU DATA AG">
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

    using DotCMIS.Client;
    using DotCMIS.Data;
    using DotCMIS.Enums;

    using Moq;

    public static class MockOfIAclCapabilities
    {
        public static void SetupBasicAcls(this Mock<IRepository> repo) {
            var aclCaps = new Mock<IAclCapabilities>();
            aclCaps.Setup(a => a.SupportedPermissions).Returns(SupportedPermissions.Basic);
            var permissions = new List<IPermissionDefinition>();
            permissions.Add(Mock.Of<IPermissionDefinition>(p => p.Description == "readonly access" && p.Id == "cmis:read"));
            permissions.Add(Mock.Of<IPermissionDefinition>(p => p.Description == "read and write access" && p.Id == "cmis:write"));
            permissions.Add(Mock.Of<IPermissionDefinition>(p => p.Description == "all accesses" && p.Id == "cmis:all"));
            aclCaps.Setup(a => a.Permissions).Returns(permissions);
            aclCaps.Setup(c => c.PermissionMapping).Returns(new Dictionary<string, IPermissionMapping>());
            repo.Setup(r => r.AclCapabilities).Returns(aclCaps.Object);
        }
    }
}