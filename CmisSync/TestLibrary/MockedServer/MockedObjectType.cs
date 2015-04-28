
namespace TestLibrary.MockedServer {
    using System;
    using System.Collections.Generic;

    using DotCMIS.Client;
    using DotCMIS.Enums;

    using Moq;

    public abstract class MockedObjectType<T> : MockedTypeDefinition<T> where T: class, IObjectType {
        public MockedObjectType(MockBehavior behavior = MockBehavior.Strict) : base(behavior) {
            this.Setup(m => m.GetParentType()).Returns(() => this.ParentType);
            this.Setup(m => m.GetBaseType()).Returns(() => this.BaseType);
            this.Setup(m => m.IsBaseType).Returns(() => this.IsBaseType);
            this.Setup(m => m.GetChildren()).Returns(() => new MockedItemList<IObjectType>(this.Children.ToArray()).Object);
        }

        public IObjectType ParentType { get; protected set; }

        public IObjectType BaseType { get; protected set; }

        public bool IsBaseType { get; protected set; }

        public List<IObjectType> Children { get; protected set; }
    }
}