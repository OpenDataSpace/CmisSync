//-----------------------------------------------------------------------
// <copyright file="MockOfIRepositoryInfo.cs" company="GRAU DATA AG">
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

namespace TestLibrary.TestUtils{
    using System;

    using DotCMIS.Data;
    using DotCMIS.Enums;

    using Moq;

    public static class MockOfIRepositoryInfo {
        public static void SetupId(this Mock<IRepositoryInfo> repoInfo, string id) {
            repoInfo.Setup(r => r.Id).Returns(id);
        }

        public static void SetupName(this Mock<IRepositoryInfo> repoInfo, string name) {
            repoInfo.Setup(r => r.Name).Returns(name);
        }

        public static void SetupVendor(this Mock<IRepositoryInfo> repoInfo, string vendor) {
            repoInfo.Setup(r => r.VendorName).Returns(vendor);
        }

        public static void SetupProductName(this Mock<IRepositoryInfo> repoInfo, string productName) {
            repoInfo.Setup(r => r.ProductName).Returns(productName);
        }

        public static void SetupProductVersion(this Mock<IRepositoryInfo> repoInfo, string version) {
            repoInfo.Setup(r => r.ProductVersion).Returns(version);
        }

        public static void SetupProductVersion(this Mock<IRepositoryInfo> repoInfo, Version version) {
            repoInfo.SetupProductVersion(version.ToString());
        }

        public static void SetupCapabilities(this Mock<IRepositoryInfo> repoInfo, IRepositoryCapabilities capabilities) {
            repoInfo.Setup(r => r.Capabilities).Returns(capabilities);
        }

        public static void SetupCapabilities(
            this Mock<IRepositoryInfo> repoInfo,
            CapabilityChanges? changes = CapabilityChanges.ObjectIdsOnly,
            bool? allVersionsSearchable = null,
            bool? descendantsSupported = true,
            CapabilityAcl? acls = CapabilityAcl.None)
        {
            var capabilities = repoInfo.Object.Capabilities ?? Mock.Of<IRepositoryCapabilities>();
            Mock.Get(capabilities).Setup(c => c.ChangesCapability).Returns(changes);
            Mock.Get(capabilities).Setup(c => c.IsAllVersionsSearchableSupported).Returns(allVersionsSearchable);
            Mock.Get(capabilities).Setup(c => c.IsGetDescendantsSupported).Returns(descendantsSupported);
            Mock.Get(capabilities).Setup(c => c.AclCapability).Returns(acls);
            repoInfo.SetupCapabilities(capabilities);
        }
    }
}