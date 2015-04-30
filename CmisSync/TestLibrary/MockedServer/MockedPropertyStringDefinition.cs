
namespace TestLibrary.MockedServer {
    using System;

    using DotCMIS.Data;
    using DotCMIS.Enums;

    using Moq;

    public class MockedPropertyStringDefinition : MockedPropertyDefinition<IPropertyStringDefinition> {
        public MockedPropertyStringDefinition(MockBehavior behavior = MockBehavior.Strict) : base(behavior) {
            this.PropertyType = PropertyType.String;
        }
    }
}