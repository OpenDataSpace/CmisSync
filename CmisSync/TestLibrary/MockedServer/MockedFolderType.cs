
namespace TestLibrary.MockedServer {
    using System;

    using DotCMIS.Client;
    using DotCMIS.Enums;

    using Moq;

    public class MockedFolderType : MockedObjectType<IFolderType> {
        public MockedFolderType(MockBehavior behavior = MockBehavior.Strict) : base(behavior) {
            this.Id = BaseTypeId.CmisFolder.GetCmisValue();
            this.BaseType = this.Object;
            this.IsBaseType = true;
            this.BaseTypeId = BaseTypeId.CmisFolder;
        }
    }
}