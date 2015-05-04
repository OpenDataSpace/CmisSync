//-----------------------------------------------------------------------
// <copyright file="MockedRepositoryService.cs" company="GRAU DATA AG">
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

    using DotCMIS.Binding.Services;
    using DotCMIS.Data;
    using DotCMIS.Data.Extensions;

    using Moq;

    public class MockedRepositoryService : Mock<IRepositoryService> {
        public MockedRepositoryService(MockBehavior behavior = MockBehavior.Strict) : base(behavior) {
            this.Setup(m => m.GetRepositoryInfos(It.IsAny<IExtensionsData>())).Returns(() => new List<IRepositoryInfo>(this.RepositoryInfos));
            this.Setup(m => m.GetRepositoryInfo(It.IsAny<string>(), It.IsAny<IExtensionsData>())).Returns<string, IExtensionsData>((repoId, extension) => this.RepositoryInfos.First(repo => repo.Id == repoId));
            this.RepositoryInfos = new List<IRepositoryInfo>();
        }

        public IList<IRepositoryInfo> RepositoryInfos { get; set; }
    }
}