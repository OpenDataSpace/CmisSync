
namespace TestLibrary.MockedServer {
    using System;

    using DotCMIS.Data;
    using DotCMIS.Enums;

    using Moq;

    public class MockedTypeDefinition<T> : Mock<T> where T: class, ITypeDefinition {
        public MockedTypeDefinition(MockBehavior behavior = MockBehavior.Strict) : base(behavior) {
            this.Setup(m => m.Description).Returns(() => this.Description);
            this.Setup(m => m.DisplayName).Returns(() => this.DisplayName);
            this.Setup(m => m.Id).Returns(() => this.Id);
            this.Setup(m => m.LocalName).Returns(() => this.LocalName);
            this.Setup(m => m.LocalNamespace).Returns(() => this.LocalNamespace);
            this.Setup(m => m.QueryName).Returns(() => this.QueryName);
            this.Setup(m => m.BaseTypeId).Returns(() => this.BaseTypeId);
            this.Setup(m => m.ParentTypeId).Returns(() => this.ParentTypeId);
        }

        public string Description { get; set; }

        public string DisplayName { get; set; }

        public string Id { get; set; }

        public string LocalName { get; set; }

        public string LocalNamespace { get; set; }

        public string QueryName { get; set; }

        public BaseTypeId BaseTypeId { get; set; }

        public string ParentTypeId { get; set; }
    }
}