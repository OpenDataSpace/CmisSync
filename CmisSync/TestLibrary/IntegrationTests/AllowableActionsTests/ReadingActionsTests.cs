
namespace TestLibrary.IntegrationTests.AllowableActionsTests {
    using System;

    using CmisSync.Lib.Cmis.ConvenienceExtenders;

    using NUnit.Framework;

    using TestUtils;

    [TestFixture, Timeout(10000), TestName("AllowableActions")]
    public class ReadingActions : BaseFullRepoTest {
        [Test, Category("Slow")]
        public void NavigationServicesOnRoot() {
            var root = this.session.GetRootFolder();
            bool supportsDescendants = this.session.IsGetDescendantsSupported();
            Assert.That(root.CanGetDescendants(), Is.EqualTo(supportsDescendants), string.Format("getDescendants is {0}allowed", supportsDescendants ? "not " : string.Empty));
            Assert.That(root.CanGetChildren(), Is.True, "getChildren is not allowed");
            Assert.That(root.CanGetObjectParents(), Is.False, "getObjectParent is allowed");
            Assert.That(root.CanGetFolderParent(), Is.False, "getFolderParent is allowed");
        }

        [Test, Category("Slow")]
        public void NavigationServicesOnFolder() {
            bool supportsDescendants = this.session.IsGetDescendantsSupported();
            Assert.That(this.remoteRootDir.CanGetDescendants(), Is.EqualTo(supportsDescendants), string.Format("getDescendants is {0}allowed", supportsDescendants ? "not " : string.Empty));
            Assert.That(this.remoteRootDir.CanGetChildren(), Is.True, "getChildren is not allowed");
            Assert.That(this.remoteRootDir.CanGetObjectParents(), Is.True, "getObjectParent is not allowed");
            Assert.That(this.remoteRootDir.CanGetFolderParent(), Is.True, "getFolderParent is not allowed");
        }

        [Test, Category("Slow")]
        public void NavigationServicesOnDocument() {
            var doc = this.remoteRootDir.CreateDocument("doc.bin", "content");
            Assert.That(doc.CanGetDescendants(), Is.False, "getDescendants is allowed");
            Assert.That(doc.CanGetChildren(), Is.False, "getChildren is allowed");
            Assert.That(doc.CanDeleteObject(), Is.True, "deleteObject is not allowed");
            Assert.That(doc.CanDeleteTree(), Is.False, "deleteTree is allowed");
            Assert.That(doc.CanGetObjectParents(), Is.True, "getObjectParent is not allowed");
            Assert.That(doc.CanGetFolderParent(), Is.False, "getFolderParent is allowed");
        }

        [Test, Category("Slow")]
        public void ObjectServiceOnRoot() {
            var root = this.session.GetRootFolder();
            Assert.That(root.CanCreateDocument(), Is.True, "createDocument is not allowed");
            Assert.That(root.CanCreateFolder(), Is.True, "createFolder is not allowed");
            Assert.That(root.CanGetProperties(), Is.True, "getProperties is not allowed");
            Assert.That(root.CanUpdateProperties(), Is.False, "updateProperties is allowed");
            Assert.That(root.CanDeleteObject(), Is.False, "deleteObject is allowed");
            Assert.That(root.CanDeleteTree(), Is.False, "deleteTree is allowed");
            Assert.That(root.CanGetContentStream(), Is.False, "getContentStream is allowed");
            Assert.That(root.CanSetContentStream(), Is.False, "setContentStream is allowed");
            Assert.That(root.CanDeleteContentStream(), Is.False, "deleteContentStream is allowed");
        }

        [Test, Category("Slow")]
        public void ObjectServiceOnFolder() {
            Assert.That(this.remoteRootDir.CanCreateDocument(), Is.True, "createDocument is not allowed");
            Assert.That(this.remoteRootDir.CanCreateFolder(), Is.True, "createFolder is not allowed");
            Assert.That(this.remoteRootDir.CanGetProperties(), Is.True, "getProperties is not allowed");
            Assert.That(this.remoteRootDir.CanUpdateProperties(), Is.True, "updateProperties is not allowed");
            Assert.That(this.remoteRootDir.CanDeleteObject(), Is.True, "deleteObject is not allowed");
            Assert.That(this.remoteRootDir.CanDeleteTree(), Is.True, "deleteTree is not allowed");
            Assert.That(this.remoteRootDir.CanGetContentStream(), Is.False, "getContentStream is allowed");
            Assert.That(this.remoteRootDir.CanSetContentStream(), Is.False, "setContentStream is allowed");
            Assert.That(this.remoteRootDir.CanDeleteContentStream(), Is.False, "deleteContentStream is allowed");
        }

        [Test, Category("Slow")]
        public void ObjectServiceOnDocument() {
            var doc = this.remoteRootDir.CreateDocument("doc.bin", "content");
            Assert.That(doc.CanCreateDocument(), Is.False, "createDocument is allowed");
            Assert.That(doc.CanCreateFolder(), Is.False, "createFolder is allowed");
            Assert.That(doc.CanGetProperties(), Is.True, "getProperties is not allowed");
            Assert.That(doc.CanUpdateProperties(), Is.True, "updateProperties is not allowed");
            Assert.That(doc.CanDeleteObject(), Is.True, "deleteObject is not allowed");
            Assert.That(doc.CanDeleteTree(), Is.False, "deleteTree is allowed");
            Assert.That(doc.CanGetContentStream(), Is.True, "getContentStream is not allowed");
            Assert.That(doc.CanSetContentStream(), Is.True, "setContentStream is not allowed");
            Assert.That(doc.CanDeleteContentStream(), Is.True, "deleteContentStream is not allowed");
        }

        [Test, Category("Slow")]
        public void FilingServiceOnRoot() {
            var root = this.session.GetRootFolder();
            Assert.That(root.CanAddObjectToFolder(), Is.EqualTo(this.session.IsMultiFilingSupported()));
            Assert.That(root.CanRemoveObjectFromFolder(), Is.EqualTo(this.session.IsUnFilingSupported()).Or.EqualTo(this.session.IsMultiFilingSupported()));
        }

        [Test, Category("Slow")]
        public void FilingServiceOnFolder() {
            Assert.That(this.remoteRootDir.CanAddObjectToFolder(), Is.EqualTo(this.session.IsMultiFilingSupported()));
            Assert.That(this.remoteRootDir.CanRemoveObjectFromFolder(), Is.EqualTo(this.session.IsUnFilingSupported()).Or.EqualTo(this.session.IsMultiFilingSupported()));
        }

        [Test, Category("Slow")]
        public void FilingServiceOnDocument() {
            var doc = this.remoteRootDir.CreateDocument("doc.bin", "content");
            Assert.That(doc.CanAddObjectToFolder(), Is.EqualTo(this.session.IsMultiFilingSupported()));
            Assert.That(doc.CanRemoveObjectFromFolder(), Is.EqualTo(this.session.IsUnFilingSupported()).Or.EqualTo(this.session.IsMultiFilingSupported()));
        }

        [Test, Category("Slow")]
        public void VersioningServiceOnRoot() {
            var root = this.session.GetRootFolder();
            Assert.That(root.CanCheckOut(), Is.False);
            Assert.That(root.CanCheckIn(), Is.False);
            Assert.That(root.CanCancelCheckOut(), Is.False);
        }

        [Test, Category("Slow")]
        public void VersioningServiceOnFolder() {
            Assert.That(this.remoteRootDir.CanCheckOut(), Is.False);
            Assert.That(this.remoteRootDir.CanCheckIn(), Is.False);
            Assert.That(this.remoteRootDir.CanCancelCheckOut(), Is.False);
        }

        [Test, Category("Slow"), Ignore("TODO: Versioning detection not yet implemented")]
        public void VersioningServiceOnDocument() {
        }
    }
}