
namespace TestLibrary.IntegrationTests.ContentHashTests {
    using System;

    using CmisSync.Lib.Cmis;
    using CmisSync.Lib.Cmis.ConvenienceExtenders;

    using DotCMIS.Client;

    using NUnit.Framework;

    using TestUtils;

    [TestFixture, Timeout(180000), TestName("HashCRUD")]
    public class CRUDTests : BaseFullRepoTest {
        [Test, Category("Slow")]
        public void CreateDocAndImmediatelyDeleteContent() {
            this.EnsureThatContentHashesAreSupportedByServerTypeSystem();

            string content = "content";

            var doc = this.remoteRootDir.CreateDocument("file.txt", content);
            doc.DeleteContentStream(true);

            Assert.That(doc.VerifyThatIfTimeoutIsExceededContentHashIsEqualTo(string.Empty), Is.True);
        }

        [Test, Category("Slow")]
        public void CreateDocViaPwcAndDeleteContent() {
            this.EnsureThatContentHashesAreSupportedByServerTypeSystem();
            this.EnsureThatPrivateWorkingCopySupportIsAvailable();
            string content = "content";

            var doc = this.remoteRootDir.CreateDocument("file.txt", content, checkedOut: true);
            var newId = doc.CheckIn(true, null, null, null);
            if (newId != null) {
                doc = this.session.GetObject(newId) as IDocument;
                doc.Refresh();
            }

            doc.DeleteContentStream(true);

            Assert.That(doc.VerifyThatIfTimeoutIsExceededContentHashIsEqualTo(string.Empty), Is.True);
        }
    }
}