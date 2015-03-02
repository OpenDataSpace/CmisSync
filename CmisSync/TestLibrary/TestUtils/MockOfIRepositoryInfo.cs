
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