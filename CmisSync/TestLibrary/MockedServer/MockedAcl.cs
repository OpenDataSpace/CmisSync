//-----------------------------------------------------------------------
// <copyright file="MockedAcl.cs" company="GRAU DATA AG">
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

    public class MockedAcl : Mock<IAcl> {
        public MockedAcl(MockBehavior behavior = MockBehavior.Strict) : base(behavior) {
            this.Aces = new List<IAce>();
            this.Setup(m => m.Aces).Returns(() => new List<IAce>(this.Aces));
            this.Setup(m => m.IsExact).Returns(() => this.IsExact);
        }

        public bool? IsExact { get; set; }

        public List<IAce> Aces { get; set; }
    }
}