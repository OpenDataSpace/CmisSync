//-----------------------------------------------------------------------
// <copyright file="MockedTypeDefinition.cs" company="GRAU DATA AG">
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

    using DotCMIS.Data;
    using DotCMIS.Enums;

    using Moq;

    public class MockedTypeDefinition<T> : Mock<T> where T: class, ITypeDefinition {
        public MockedTypeDefinition(MockBehavior behavior = MockBehavior.Strict) : base(behavior) {
            this.PropertyDefinitions = new List<IPropertyDefinition>();
            this.Setup(m => m.Description).Returns(() => this.Description);
            this.Setup(m => m.DisplayName).Returns(() => this.DisplayName);
            this.Setup(m => m.Id).Returns(() => this.Id);
            this.Setup(m => m.LocalName).Returns(() => this.LocalName);
            this.Setup(m => m.LocalNamespace).Returns(() => this.LocalNamespace);
            this.Setup(m => m.QueryName).Returns(() => this.QueryName);
            this.Setup(m => m.BaseTypeId).Returns(() => this.BaseTypeId);
            this.Setup(m => m.ParentTypeId).Returns(() => this.ParentTypeId);
            this.Setup(m => m.PropertyDefinitions).Returns(() => new List<IPropertyDefinition>(this.PropertyDefinitions));
        }

        public string Description { get; set; }

        public string DisplayName { get; set; }

        public string Id { get; set; }

        public string LocalName { get; set; }

        public string LocalNamespace { get; set; }

        public string QueryName { get; set; }

        public BaseTypeId BaseTypeId { get; set; }

        public string ParentTypeId { get; set; }

        public IList<IPropertyDefinition> PropertyDefinitions { get; set; }
    }
}