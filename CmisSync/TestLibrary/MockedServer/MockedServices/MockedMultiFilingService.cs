
namespace TestLibrary.MockedServer.MockedServices {
    using System;

    using DotCMIS.Binding.Services;

    using Moq;

    public class MockedMultiFilingService : Mock<IMultiFilingService> {
        public MockedMultiFilingService(MockBehavior behavior = MockBehavior.Strict) : base(behavior) {
        }
    }
}