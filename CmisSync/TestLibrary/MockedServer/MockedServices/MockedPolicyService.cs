
namespace TestLibrary.MockedServer.MockedServices {
    using System;

    using DotCMIS.Binding.Services;

    using Moq;

    public class MockedPolicyService : Mock<IPolicyService> {
        public MockedPolicyService(MockBehavior behavior = MockBehavior.Strict) : base(behavior) {
        }
    }
}