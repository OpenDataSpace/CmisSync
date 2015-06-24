//-----------------------------------------------------------------------
// <copyright file="MockedObjectData.cs" company="GRAU DATA AG">
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

    public class MockedObjectData : Mock<IObjectData> {
        public MockedObjectData(MockBehavior behavior = MockBehavior.Strict) : base(behavior) {
            this.Setup(m => m.Acl).Returns(() => this.Acl);
            this.Setup(m => m.Id).Returns(() => this.Id);
            this.Setup(m => m.BaseTypeId).Returns(() => this.BaseTypeId);
            this.Setup(m => m.AllowableActions).Returns(() => this.AllowableActions);
            this.Setup(m => m.Properties).Returns(() => this.Properties);
            this.Setup(m => m.Relationships).Returns(() => new List<IObjectData>(this.Relationships));
            this.Setup(m => m.ChangeEventInfo).Returns(() => this.ChangeEventInfo);
            this.Setup(m => m.IsExactAcl).Returns(() => this.IsExactAcl);
            this.Setup(m => m.PolicyIds).Returns(() => this.PolicyIds);
            this.Setup(m => m.Renditions).Returns(() => new List<IRenditionData>(this.Renditions));
        }

        public string Id { get; set; }

        public IAcl Acl { get; set; }

        public BaseTypeId? BaseTypeId { get; set; }

        public IProperties Properties { get; set; }

        public IAllowableActions AllowableActions { get; set; }

        public IList<IObjectData> Relationships { get; set; }

        public IChangeEventInfo ChangeEventInfo { get; set; }

        public bool? IsExactAcl { get; set; }

        public IPolicyIdList PolicyIds { get; set; }

        public IList<IRenditionData> Renditions { get; set; }
    }
}