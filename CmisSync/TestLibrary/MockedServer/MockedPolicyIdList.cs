//-----------------------------------------------------------------------
// <copyright file="MockedPolicyIdList.cs" company="GRAU DATA AG">
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

    using Moq;

    public class MockedPolicyIdList : Mock<IPolicyIdList> {
        public MockedPolicyIdList(MockBehavior behavior = MockBehavior.Strict, params string[] policyIds) : base(behavior) {
            this.PolicyIds = new List<string>(policyIds);
            this.Setup(m => m.PolicyIds).Returns(() => this.PolicyIds);
        }

        public IList<string> PolicyIds { get; set; }
    }
}