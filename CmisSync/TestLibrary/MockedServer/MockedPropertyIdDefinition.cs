
namespace TestLibrary.MockedServer {
    using System;

    using DotCMIS.Data;
    using DotCMIS.Enums;

    using Moq;

    public class MockedPropertyIdDefinition : MockedPropertyDefinition<IPropertyIdDefinition> {
        public MockedPropertyIdDefinition(MockBehavior behavior = MockBehavior.Strict) : base(behavior) {
            this.PropertyType = PropertyType.Id;
        }
    }
}