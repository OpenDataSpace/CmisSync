
namespace TestLibrary.IntegrationTests.MockedServerTests {
    using System;

    using DotCMIS.Client;
    using DotCMIS.Enums;

    using MockedServer;

    using Moq;

    using NUnit.Framework;

    using TestUtils;

    [TestFixture]
    public class SetupFolderTest {
        [Test, Category("Fast")]
        public void Constructor([Values(true, false)]bool withParent, [Values(null, "folderId")]string id) {
            string name = "folder";
            IFolder parent = withParent ? Mock.Of<IFolder>() : null;
            var underTest = new MockedFolder(name, id, parent);

            Assert.That(underTest.Object.BaseTypeId, Is.EqualTo(BaseTypeId.CmisFolder));
            Assert.That(underTest.Object.ObjectType.Id, Is.EqualTo(BaseTypeId.CmisFolder.GetCmisValue()));
            Assert.That(underTest.Object.ChangeToken, Is.Not.Null);
        }
    }
}