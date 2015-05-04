
namespace TestLibrary.MockedServer {
    using System;
    using System.Collections.Generic;

    using DotCMIS.Data;
    using DotCMIS.Enums;

    using Moq;

    public abstract class MockedRepositoryInfo<T> : Mock<T> where T : class, IRepositoryInfo {
        public MockedRepositoryInfo(string id = null, string name = null, MockBehavior behavior = MockBehavior.Strict) : base(behavior) {
            this.Id = id ?? Guid.NewGuid().ToString();
            this.RepoName = name ?? "my";
            this.Description = "desc";
            this.ProductName = "GRAU DATA AG in memory cmis repo";
            this.VendorName = "GRAU DATA AG";
            this.Setup(m => m.Name).Returns(() => this.RepoName);
            this.Setup(m => m.Id).Returns(() => this.Id);
            this.Setup(m => m.Description).Returns(() => this.Description);
            this.Setup(m => m.ProductName).Returns(() => this.ProductName);
            this.Setup(m => m.VendorName).Returns(() => this.VendorName);
            this.Setup(m => m.RootFolderId).Returns(() => this.RootFolderId);
            this.Setup(m => m.LatestChangeLogToken).Returns(() => this.LatestChangeLogToken);
            this.Setup(m => m.CmisVersionSupported).Returns(() => this.CmisVersionSupported);
            this.Setup(m => m.ThinClientUri).Returns(() => this.ThinClientUri);
            this.Setup(m => m.ChangesIncomplete).Returns(() => this.ChangesIncomplete);
            this.Setup(m => m.ChangesOnType).Returns(() => this.ChangesOnType);
            this.Setup(m => m.PrincipalIdAnonymous).Returns(() => this.PrincipalIdAnonymous);
            this.Setup(m => m.PrincipalIdAnyone).Returns(() => this.PrincipalIdAnyone);
            this.Setup(r => r.AclCapabilities).Returns(() => this.AclCapabilities);
        }

        public string Id { get; set; }
        public string RepoName { get; set; }
        public string Description { get; set; }
        public string VendorName { get; set; }
        public string ProductName { get; set; }
        public string ProductVersion { get; set; }
        public string RootFolderId { get; set; }
        public IRepositoryCapabilities Capabilities { get; set; }
        public IAclCapabilities AclCapabilities { get; set; }
        public string LatestChangeLogToken { get; set; }
        public string CmisVersionSupported { get; set; }
        public string ThinClientUri { get; set; }
        public bool? ChangesIncomplete { get; set; }
        public IList<BaseTypeId?> ChangesOnType { get; set; }
        public string PrincipalIdAnonymous { get; set; }
        public string PrincipalIdAnyone { get; set; }
    }
}