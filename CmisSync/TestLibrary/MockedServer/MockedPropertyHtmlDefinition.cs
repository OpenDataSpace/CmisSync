
namespace TestLibrary.MockedServer {
    using System;

    using DotCMIS.Data;
    using DotCMIS.Enums;

    using Moq;

    public class MockedPropertyHtmlDefinition : MockedPropertyDefinition<IPropertyHtmlDefinition> {
        public MockedPropertyHtmlDefinition(MockBehavior behavior = MockBehavior.Strict) : base(behavior) {
            this.PropertyType = PropertyType.Html;
        }
    }
}