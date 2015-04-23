
namespace TestLibrary.MockedServer {
    using System;

    using DotCMIS.Client;
    using DotCMIS.Enums;

    using Moq;

    public class MockedDocumentType : Mock<IDocumentType> {
        public MockedDocumentType(MockBehavior behavior = MockBehavior.Strict) : base(behavior) {
            this.Setup(m => m.BaseTypeId).Returns(BaseTypeId.CmisDocument);
            this.Setup(m => m.Id).Returns(BaseTypeId.CmisDocument.GetCmisValue());
            this.Setup(m => m.Description).Returns("mocked C# document type");
            this.Setup(m => m.DisplayName).Returns("mocked document type");
            this.Setup(m => m.GetBaseType()).Returns(this.Object);
            this.Setup(m => m.ContentStreamAllowed).Returns(ContentStreamAllowed.Allowed);
            this.Setup(m => m.GetChildren()).Returns(new MockedItemList<IObjectType>().Object);
        }
    }
}