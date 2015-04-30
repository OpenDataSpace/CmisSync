
namespace TestLibrary.MockedServer {
    using System;

    using DotCMIS.Data;
    using DotCMIS.Enums;

    using Moq;

    public class MockedPropertyUriDefinition : MockedPropertyDefinition<IPropertyUriDefinition> {
        public MockedPropertyUriDefinition(MockBehavior behavior = MockBehavior.Strict) : base(behavior) {
            this.PropertyType = PropertyType.Uri;
        }
    }
}