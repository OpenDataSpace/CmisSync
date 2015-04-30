
namespace TestLibrary.MockedServer {
    using System;

    using DotCMIS.Data;
    using DotCMIS.Enums;

    using Moq;

    public class MockedPropertyIntegerDefinition : MockedPropertyDefinition<IPropertyIntegerDefinition> {
        public MockedPropertyIntegerDefinition(MockBehavior behavior = MockBehavior.Strict) : base(behavior) {
            this.PropertyType = PropertyType.Integer;
        }
    }
}