//-----------------------------------------------------------------------
// <copyright file="MockedFolder.cs" company="GRAU DATA AG">
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

    using TestLibrary.TestUtils;

    public class MockedFolder : Mock<IFolder> {
        private IFolder parent;
        private string name;
        private string id;
        private MockedItemList<ICmisObject> children = new MockedItemList<ICmisObject>();
        private DateTime lastModification = DateTime.UtcNow;
        private DateTime creationDate = DateTime.UtcNow;

        public MockedFolder(string name, string id = null, IFolder parent = null) : base(MockBehavior.Strict) {
            this.name = name;
            this.id = id;
            this.parent = parent;
            this.Setup(m => m.ParentId).Returns(this.parent != null ? this.parent.Id : (string)null);
            this.Setup(m => m.Name).Returns(this.name);
            this.Setup(m => m.GetChildren()).Returns(this.children.Object);
            this.Setup(m => m.GetChildren(It.IsAny<IOperationContext>())).Returns(this.children.Object);
            this.Setup(m => m.IsRootFolder).Returns(this.parent == null);
            this.Setup(m => m.Id).Returns(this.id);
            this.Setup(m => m.FolderParent).Returns(this.parent);
            this.Setup(m => m.Path).Returns(this.parent.Path + "/" + this.name);
            this.Setup(m => m.LastModificationDate).Returns(this.lastModification);
            this.Setup(m => m.CreationDate).Returns(this.creationDate);
//            this.Setup(m => m.Move(It.Is<IObjectId>(obj => obj.Id == this.Object.ParentId), It.IsAny<IObjectId>())).Returns(this.Object);
        }
    }
}