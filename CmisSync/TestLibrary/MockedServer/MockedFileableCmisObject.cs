//-----------------------------------------------------------------------
// <copyright file="MockedFileableCmisObject.cs" company="GRAU DATA AG">
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

    using Moq;

    public abstract class MockedFileableCmisObject<T> : MockedCmisObject<T> where T : class, IFileableCmisObject {
        public MockedFileableCmisObject(string name, string id = null, MockBehavior behavior = MockBehavior.Strict) : base(name, id, behavior) {
            this.Setup(m => m.Parents).Returns(() => new List<IFolder>(this.Parents));
            this.Setup(m => m.Move(It.IsAny<IObjectId>(), It.IsAny<IObjectId>())).Returns(this.Object);
            this.Setup(m => m.Paths).Returns(() => this.Paths);
        }

        public IList<IFolder> Parents { get; set; }

        public IList<string> Paths { get; set; }
    }
}