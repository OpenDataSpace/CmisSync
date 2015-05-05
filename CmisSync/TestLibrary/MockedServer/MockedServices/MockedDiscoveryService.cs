
namespace TestLibrary.MockedServer.MockedServices {
    using System;

    using DotCMIS.Binding.Services;

    using Moq;

    public class MockedDiscoveryService : Mock<IDiscoveryService> {
        public MockedDiscoveryService(MockBehavior behavior = MockBehavior.Strict) : base(behavior) {
        }
    }
}