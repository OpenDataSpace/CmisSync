//-----------------------------------------------------------------------
// <copyright file="MockedProperty.cs" company="GRAU DATA AG">
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
    using System.Linq;

    using DotCMIS.Client;
    using DotCMIS.Data;
    using DotCMIS.Enums;

    using Moq;

    using TestUtils;

    public class MockedProperty : Mock<IProperty> {
        public MockedProperty(string id, MockBehavior behavior = MockBehavior.Strict, params object[] values) : base(behavior) {
            this.Id = id;
            this.Values = new List<object>(values);
            this.Setup(m => m.DisplayName).Returns(() => this.DisplayName);
            this.Setup(m => m.Id).Returns(() => this.Id);
            this.Setup(m => m.LocalName).Returns(() => this.LocalName);
            this.Setup(m => m.PropertyType).Returns(() => this.PropertyType);
            this.Setup(m => m.QueryName).Returns(() => this.QueryName);
            this.Setup(m => m.Value).Returns(() => this.Values.First());
            this.Setup(m => m.Values).Returns(() => new List<object>(this.Values));
            this.Setup(m => m.IsMultiValued).Returns(() => this.IsMultiValued);
            this.Setup(m => m.FirstValue).Returns(() => this.Values.First());
            this.Setup(m => m.ValueAsString).Returns(() => this.Values.First() != null ? this.Values.First().ToString() : null);
            this.Setup(m => m.ValuesAsString).Returns(() => string.Join(",", this.Values));
        }

        public bool IsMultiValued { get; set; }

        public string DisplayName { get; set; }

        public string Id { get; set; }

        public string LocalName { get; set; }

        public string QueryName { get; set; }

        public PropertyType? PropertyType { get; set; }

        public IPropertyDefinition PropertyDefinition { get; set; }

        public IList<object> Values { get; private set; }
    }
}