
namespace TestLibrary.MockedServer.MockedServices {
    using System;

    using DotCMIS.Binding.Services;

    using Moq;

    public class MockedNavigationService : Mock<INavigationService> {
        public MockedNavigationService(MockBehavior behavior = MockBehavior.Strict) : base(behavior) {
        }
    }
}