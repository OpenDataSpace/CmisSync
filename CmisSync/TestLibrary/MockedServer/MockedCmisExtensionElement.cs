
namespace TestLibrary.MockedServer {
    using System;
    using System.Collections.Generic;

    using DotCMIS.Data.Extensions;

    using Moq;

    public class MockedCmisExtensionElement : Mock<ICmisExtensionElement> {
        public MockedCmisExtensionElement(MockBehavior behavior = MockBehavior.Strict) : base(behavior) {
            this.Attributes = new Dictionary<string, string>();
            this.Children = new List<ICmisExtensionElement>();
            this.Setup(m => m.Name).Returns(() => this.ExtensionName);
            this.Setup(m => m.Namespace).Returns(() => this.NameSpace);
            this.Setup(m => m.Children).Returns(() => new List<ICmisExtensionElement>(this.Children));
            this.Setup(m => m.Attributes).Returns(() => new Dictionary<string, string>(this.Attributes));
            this.Setup(m => m.Value).Returns(() => this.Value);
        }

        public string ExtensionName { get; set; }

        public string NameSpace { get; set; }

        public List<ICmisExtensionElement> Children { get; set; }

        public Dictionary<string, string> Attributes { get; set; }

        public string Value { get; set; }
    }
}