
namespace TestLibrary.MockedServer {
    using System;

    using DotCMIS.Data;
    using DotCMIS.Enums;

    using Moq;

    public class MockedPropertyBooleanDefinition : MockedPropertyDefinition<IPropertyBooleanDefinition> {
        public MockedPropertyBooleanDefinition(MockBehavior behavior = MockBehavior.Strict) : base(behavior) {
            this.PropertyType = PropertyType.Boolean;
        }
    }
}