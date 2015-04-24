//-----------------------------------------------------------------------
// <copyright file="MockedDocument.cs" company="GRAU DATA AG">
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

    using DotCMIS;
    using DotCMIS.Client;
    using DotCMIS.Data;
    using DotCMIS.Enums;

    using Moq;

    using TestUtils;

    public class MockedDocument : MockedCmisObject<IDocument> {
        private IDocumentType docType = new MockedDocumentType().Object;
        private IFolder parent;

        public MockedDocument(
            string name,
            string content = null,
            string id = null,
            IFolder parent = null,
            MockBehavior behavior = MockBehavior.Strict) : base(name, id, behavior) {
            this.ObjectType = this.docType;
            this.parent = parent ?? Mock.Of<IFolder>();
            this.Setup(m => m.BaseType).Returns(this.docType);
            this.Setup(m => m.BaseTypeId).Returns(BaseTypeId.CmisDocument);
            this.SetupParent(this.parent);
            this.Setup(m => m.ContentStreamId).Returns((string)null);
            this.Setup(m => m.ContentStreamLength).Returns(content == null ? (long?)null : content.Length);
            this.Setup(m => m.AppendContentStream(It.IsAny<IContentStream>(), It.IsAny<bool>())).Callback<IContentStream, bool>((stream, lastChunk) => Console.WriteLine("vla")).Returns(this.Object);
            this.Setup(m => m.SetContentStream(It.IsAny<IContentStream>(), It.IsAny<bool>())).Callback<IContentStream, bool>((stream, overwrite) => Console.Write("override")).Returns(this.Object);
        }
    }
}