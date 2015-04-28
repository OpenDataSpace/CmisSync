
namespace TestLibrary.MockedServer {
    using System;

    using DotCMIS.Data;

    using Moq;

    public class MockedPrincipal : Mock<IPrincipal> {
        public MockedPrincipal(MockBehavior behavior = MockBehavior.Strict) : base(behavior) {
            this.Setup(m => m.Id).Returns(() => this.Id);
        }

        public string Id { get; set; }
    }
}