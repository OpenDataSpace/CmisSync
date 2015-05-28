//-----------------------------------------------------------------------
// <copyright file="MockedCmisBinding.cs" company="GRAU DATA AG">
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

    using DotCMIS.Binding;
    using DotCMIS.Binding.Services;

    using Moq;

    public class MockedCmisBinding : Mock<ICmisBinding> {
        public MockedCmisBinding(MockBehavior behavior = MockBehavior.Strict) : base( behavior) {
            this.Setup(m => m.BindingType).Returns(() => this.BindingType);
            this.Setup(m => m.GetRepositoryService()).Returns(() => this.RepositoryService);
            this.Setup(m => m.GetNavigationService()).Returns(() => this.NavigationService);
            this.Setup(m => m.GetObjectService()).Returns(() => this.ObjectService);
            this.Setup(m => m.GetVersioningService()).Returns(() => this.VersioningService);
            this.Setup(m => m.GetRelationshipService()).Returns(() => this.RelationshipService);
            this.Setup(m => m.GetDiscoveryService()).Returns(() => this.DiscoveryService);
            this.Setup(m => m.GetMultiFilingService()).Returns(() => this.MultiFilingService);
            this.Setup(m => m.GetAclService()).Returns(() => this.AclService);
            this.Setup(m => m.GetPolicyService()).Returns(() => this.PolicyService);
            this.Setup(m => m.GetAuthenticationProvider()).Returns(() => this.AuthenticationProvider);
        }

        public string BindingType { get; set; }

        public IRepositoryService RepositoryService { get; set; }

        public INavigationService NavigationService { get; set; }

        public IObjectService ObjectService { get; set; }

        public IVersioningService VersioningService { get; set; }

        public IRelationshipService RelationshipService { get; set; }

        public IDiscoveryService DiscoveryService { get; set; }

        public IMultiFilingService MultiFilingService { get; set; }

        public IAclService AclService { get; set; }

        public IPolicyService PolicyService { get; set; }

        public IAuthenticationProvider AuthenticationProvider { get; set; }
    }
}