
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