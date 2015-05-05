
namespace TestLibrary.MockedServer.MockedServices {
    using System;

    using DotCMIS.Binding.Services;

    using Moq;

    public class MockedObjectService : Mock<IObjectService> {
        public MockedObjectService(MockBehavior behavior = MockBehavior.Strict) : base(behavior) {
        }
    }
}