
namespace TestLibrary.MockedServer {
    using System;

    using DotCMIS.Data;

    using Moq;

    public class MockedPropertyDecimalDefinition : MockedPropertyDefinition<IPropertyDecimalDefinition>{
        public MockedPropertyDecimalDefinition(MockBehavior behavior = MockBehavior.Strict) : base(behavior) {
            this.PropertyType = DotCMIS.Enums.PropertyType.Decimal;
        }
    }
}