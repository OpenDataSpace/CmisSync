//-----------------------------------------------------------------------
// <copyright file="MockedCmisExtensionElement.cs" company="GRAU DATA AG">
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