
namespace TestLibrary {
    using System;

    using DotCMIS.Data;
    using DotCMIS.Enums;

    using Moq;

    public class MockedPropertyDefinition<T> : Mock<T> where T: class, IPropertyDefinition {
        public MockedPropertyDefinition(MockBehavior behavior = MockBehavior.Strict) : base(behavior) {
            this.Setup(m => m.Id).Returns(() => this.Id);
            this.Setup(m => m.LocalName).Returns(() => this.LocalName);
            this.Setup(m => m.LocalNamespace).Returns(() => this.LocalNamespace);
            this.Setup(m => m.DisplayName).Returns(() => this.DisplayName);
            this.Setup(m => m.QueryName).Returns(() => this.QueryName);
            this.Setup(m => m.Description).Returns(() => this.Description);

            this.Setup(m => m.PropertyType).Returns(() => this.PropertyType);
            this.Setup(m => m.Cardinality).Returns(() => this.Cardinality);
            this.Setup(m => m.Updatability).Returns(() => this.Updatability);

            this.Setup(m => m.IsInherited).Returns(() => this.IsInherited);
            this.Setup(m => m.IsRequired).Returns(() => this.IsRequired);
            this.Setup(m => m.IsQueryable).Returns(() => this.IsQueryable);
            this.Setup(m => m.IsOrderable).Returns(() => this.IsOrderable);
            this.Setup(m => m.IsOpenChoice).Returns(() => this.IsOpenChoice);
        }

        public string Id { get; set; }
        public string LocalName { get; set; }
        public string LocalNamespace { get; set; }
        public string DisplayName { get; set; }
        public string QueryName { get; set; }
        public string Description { get; set; }
        public PropertyType PropertyType { get; set; }
        public Cardinality Cardinality { get; set; }
        public Updatability Updatability { get; set; }
        public bool? IsInherited { get; set; }
        public bool? IsRequired { get; set; }
        public bool? IsQueryable { get; set; }
        public bool? IsOrderable { get; set; }
        public bool? IsOpenChoice { get; set; }
    }
}