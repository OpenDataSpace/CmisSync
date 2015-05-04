
namespace TestLibrary.MockedServer {
    using System;
    using System.Collections.Generic;

    using DotCMIS.Data;

    using Moq;

    public class MockedPermissionMapping : Mock<IPermissionMapping> {
        public MockedPermissionMapping(MockBehavior behavior = MockBehavior.Strict) : base(behavior) {
            this.Setup(m => m.Key).Returns(() => this.Key);
            this.Setup(m => m.Permissions).Returns(() => this.Permissions);
        }

        public string Key { get; set; }

        public IList<string> Permissions { get; set; }
    }
}