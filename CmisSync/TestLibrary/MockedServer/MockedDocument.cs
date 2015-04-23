
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