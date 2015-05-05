
namespace TestLibrary.MockedServer.MockedServices {
    using System;

    using DotCMIS.Binding.Services;

    using Moq;

    public class MockedAclService : Mock<IAclService> {
        public MockedAclService(MockBehavior behavior = MockBehavior.Strict) : base(behavior) {
        }
    }
}