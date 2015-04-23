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
    using DotCMIS.Enums;

    using Moq;

    using TestUtils;

    public class MockedDocument : Mock<IDocument> {
        private string id = Guid.NewGuid().ToString();
        private string name = Guid.NewGuid().ToString();
        private IDocumentType docType = new MockedDocumentType().Object;
        private string changeToken = Guid.NewGuid().ToString();
        private DateTime lastModification = DateTime.UtcNow;
        private IFolder parent;

        public MockedDocument(
            string id = null,
            IFolder parent = null,
            MockBehavior behavior = MockBehavior.Strict) : base(behavior) {
            this.id = id ?? this.id;
            this.parent = parent ?? Mock.Of<IFolder>();
            this.Setup(m => m.Id).Returns(this.id);
            this.Setup(m => m.Name).Returns(this.name);
            this.Setup(m => m.UpdateProperties(It.IsAny<IDictionary<string, object>>())).Callback<IDictionary<string, object>>((dict) => this.UpdateProperties(dict)).Returns(this.Object);
            this.Setup(m => m.BaseType).Returns(this.docType);
            this.Setup(m => m.BaseTypeId).Returns(BaseTypeId.CmisDocument);
            this.Setup(m => m.ChangeToken).Returns(this.changeToken);
            this.SetupParent(this.parent);
        }

        private void UpdateProperties(IDictionary<string, object> props) {
            bool updated = false;
            bool updateModificationDateIfUpdated = true;
            foreach (var prop in props) {
                switch (prop.Key) {
                case PropertyIds.Name:
                    if (!(prop.Value is string)) {
                        throw new ArgumentException("Given name is not a string, but a " + prop.Value.GetType().ToString());
                    }

                    var newName = prop.Value as string;
                    if (newName != this.name) {
                        this.name = newName;
                        updated = true;
                    }

                    break;
                case PropertyIds.LastModificationDate:
                    if (!(prop.Value.GetType() == typeof(DateTime))) {
                        throw new ArgumentException("Given modification date is not a DateTime, but a " + prop.Value.GetType().ToString());
                    }

                    var newDate = (DateTime)prop.Value;
                    if (newDate != this.lastModification) {
                        this.lastModification = newDate;
                        updateModificationDateIfUpdated = false;
                    }

                    break;
                }
            }

            if (updated) {
                if (updateModificationDateIfUpdated) {
                    this.lastModification = DateTime.UtcNow;
                }

                this.changeToken = Guid.NewGuid().ToString();
            }
        }
    }
}