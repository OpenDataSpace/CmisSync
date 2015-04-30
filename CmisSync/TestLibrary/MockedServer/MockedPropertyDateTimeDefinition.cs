
namespace TestLibrary.MockedServer {
    using System;

    using DotCMIS.Data;

    using Moq;

    public class MockedPropertyDateTimeDefinition : MockedPropertyDefinition<IPropertyDateTimeDefinition> {
        public MockedPropertyDateTimeDefinition(MockBehavior behavior = MockBehavior.Strict) : base(behavior) {
            this.PropertyType = DotCMIS.Enums.PropertyType.DateTime;
        }
    }
}