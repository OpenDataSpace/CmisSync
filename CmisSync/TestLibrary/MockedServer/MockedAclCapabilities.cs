
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