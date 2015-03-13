//-----------------------------------------------------------------------
// <copyright file="MockOfISession.cs" company="GRAU DATA AG">
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
    using DotCMIS.Binding;
    using DotCMIS.Binding.Services;
    using DotCMIS.Client;
    using DotCMIS.Data;
    using DotCMIS.Data.Extensions;
    using DotCMIS.Enums;

    using Moq;

    using TestLibrary.TestUtils;

    public class MockOfISession : Mock<ISession> {
        private Mock<ICmisBinding> binding = new Mock<ICmisBinding>(MockBehavior.Strict);
        private Mock<IRepositoryService> repoService = new Mock<IRepositoryService>(MockBehavior.Strict);
        public MockOfISession(MockOfIRepository repo) : base(MockBehavior.Strict) {
            // TypeSystem
            IList<IPropertyDefinition> props = new List<IPropertyDefinition>();
            props.Add(Mock.Of<IPropertyDefinition>(p => p.Id == PropertyIds.LastModificationDate && p.Updatability == DotCMIS.Enums.Updatability.ReadWrite));
            var docType = Mock.Of<IObjectType>(d => d.PropertyDefinitions == props);
            var folderType = Mock.Of<IObjectType>(d => d.PropertyDefinitions == props);
            this.repoService.Setup(s => s.GetTypeDefinition(repo.Object.Id, BaseTypeId.CmisDocument, null)).Returns(docType);
            this.repoService.Setup(s => s.GetTypeDefinition(repo.Object.Id, BaseTypeId.CmisFolder, null)).Returns(folderType);

            this.repoService.Setup(s => s.GetRepositoryInfos(It.IsAny<IExtensionsData>())).Returns((IList<IRepositoryInfo>)null);
            this.binding.Setup(b => b.GetRepositoryService()).Returns(this.repoService.Object);
            this.Setup(s => s.Binding).Returns(this.binding.Object);
            this.Setup(s => s.RepositoryInfo.Id).Returns(repo.Object.Id);
        }
    }
}