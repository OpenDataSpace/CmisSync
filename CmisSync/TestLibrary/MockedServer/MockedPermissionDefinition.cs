
namespace TestLibrary.MockedServer {
    using System;

    using DotCMIS.Data;

    using Moq;

    public class MockedPermissionDefinition : Mock<IPermissionDefinition> {
        public MockedPermissionDefinition(MockBehavior behavior = MockBehavior.Strict) : base(behavior) {
            this.Setup(m => m.Id).Returns(() => this.Id);
            this.Setup(m => m.Description).Returns(() => this.Description);
        }

        public string Id { get; set; }

        public string Description { get; set; }
    }
}