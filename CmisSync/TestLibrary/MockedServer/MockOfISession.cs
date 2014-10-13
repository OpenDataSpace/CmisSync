
namespace TestLibrary.MockedServer
{
    using System;
    using System.Collections.Generic;

    using DotCMIS.Binding;
    using DotCMIS.Binding.Services;
    using DotCMIS.Client;
    using DotCMIS.Data;
    using DotCMIS.Data.Extensions;

    using Moq;

    using TestLibrary.TestUtils;

    public class MockOfISession : Mock<ISession>
    {
        private Mock<ICmisBinding> binding = new Mock<ICmisBinding>(MockBehavior.Strict);
        private Mock<IRepositoryService> repoService = new Mock<IRepositoryService>(MockBehavior.Strict);
        public MockOfISession(MockOfIRepository repo) : base(MockBehavior.Strict)
        {
            // TypeSystem
            IList<IPropertyDefinition> props = new List<IPropertyDefinition>();
            props.Add(Mock.Of<IPropertyDefinition>(p => p.Id == "cmis:lastModificationDate" && p.Updatability == DotCMIS.Enums.Updatability.ReadWrite));
            var docType = Mock.Of<IObjectType>(d => d.PropertyDefinitions == props);
            var folderType = Mock.Of<IObjectType>(d => d.PropertyDefinitions == props);
            this.repoService.Setup(s => s.GetTypeDefinition(repo.Object.Id, "cmis:document", null)).Returns(docType);
            this.repoService.Setup(s => s.GetTypeDefinition(repo.Object.Id, "cmis:folder", null)).Returns(folderType);

            this.repoService.Setup(s => s.GetRepositoryInfos(It.IsAny<IExtensionsData>())).Returns((IList<IRepositoryInfo>)null);
            this.binding.Setup(b => b.GetRepositoryService()).Returns(this.repoService.Object);
            this.Setup(s => s.Binding).Returns(this.binding.Object);
            this.Setup(s => s.RepositoryInfo.Id).Returns(repo.Object.Id);

        }
    }
}