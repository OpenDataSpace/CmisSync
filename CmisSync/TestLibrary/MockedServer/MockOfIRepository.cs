

namespace TestLibrary.MockedServer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DotCMIS.Client;
    using DotCMIS.Data;
    using DotCMIS.Enums;

    using Moq;

    using TestLibrary.TestUtils;

    public class MockOfIRepository : Mock<IRepository>
    {
        private static Dictionary<string, MockOfIRepository> repositories = new Dictionary<string, MockOfIRepository>();

        private MockedFolder rootFolder = new MockedFolder("/");
        public IFolder RootFolder {
            get {
                return this.rootFolder.Object;
            }
        }

        public void SetupName(string name) {
            this.Setup(r => r.Name).Returns(name);
        }

        public static MockOfIRepository GetRepository(string id) {
            lock(repositories) {
                MockOfIRepository repo;
                if (!repositories.TryGetValue(id, out repo)) {
                    repo = new MockOfIRepository(id);
                    repositories[id] = repo;
                }

                return repo;
            }
        }

        public void Destroy() {
            repositories.Remove(this.Object.Id);
        }

        private MockOfIRepository(string id) : base(MockBehavior.Strict) {
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
            this.Setup(r => r.CreateSession()).Returns(new MockOfISession(this).Object);
        }
    }
}