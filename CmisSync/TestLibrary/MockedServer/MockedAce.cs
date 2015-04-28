//-----------------------------------------------------------------------
// <copyright file="MockedAce.cs" company="GRAU DATA AG">
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

    using DotCMIS.Data;

    using Moq;

    public class MockedAce : Mock<IAce> {
        public MockedAce(string principal, bool isDirect = true, MockBehavior behavior = MockBehavior.Strict) : base(behavior) {
            this.IsDirect = isDirect;
            this.Principal = new MockedPrincipal(behavior) { Id = principal }.Object;
            this.Setup(m => m.IsDirect).Returns(() => this.IsDirect);
            this.Setup(m => m.Principal).Returns(() => this.Principal);
        }

        public bool IsDirect { get; set; }

        public IPrincipal Principal { get; set; }
    }
}