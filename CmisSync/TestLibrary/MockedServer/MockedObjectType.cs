//-----------------------------------------------------------------------
// <copyright file="MockedObjectType.cs" company="GRAU DATA AG">
//
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General private License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//
//   This program is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//   GNU General private License for more details.
//
//   You should have received a copy of the GNU General private License
//   along with this program. If not, see http://www.gnu.org/licenses/.
//
// </copyright>
//-----------------------------------------------------------------------

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