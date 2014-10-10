

namespace TestLibrary.MockedServer
{
    using System;
    using System.Collections.Generic;

    using DotCMIS.Client;
    using DotCMIS.Data;
    using DotCMIS.Enums;

    using Moq;

    using TestLibrary.TestUtils;

    public class MockOfIRepository : Mock<IRepository>
    {
        private static Dictionary<string, MockOfIRepository> repositories = new Dictionary<string, MockOfIRepository>();

        public Mock<IRepository> GetRepository(string id) {
            lock(repositories) {
                var repo = repositories[id];
                if (repo == null) {
                    repo = new MockOfIRepository(id);
                    repositories[id] = repo;
                }

                return repo;
            }
        }

        public void Destroy() {
            repositories.Remove(this.Object.Id);
        }

        private MockOfIRepository(string id) {
            this.Setup(r => r.Name).Returns("name");
            this.Setup(r => r.Id).Returns(id);
            this.Setup(r => r.Description).Returns("desc");
            this.Setup(r => r.ProductName).Returns("GRAU DATA AG in memory cmis repo");
            this.Setup(r => r.VendorName).Returns("GRAU DATA AG");
            var acls = Mock.Of<IAclCapabilities>(
                c =>
                c.SupportedPermissions == SupportedPermissions.Basic &&
                c.PermissionMapping == new Dictionary<string, IPermissionMapping>());
            this.Setup(r => r.AclCapabilities).Returns(acls);
        }
    }
}