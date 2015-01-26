using DotCMIS.Client;
using DotCMIS.Client.Impl;


namespace TestLibrary.IntegrationTests {
    using System;
    using System.Collections.Generic;

    using CmisSync.Lib.Cmis.ConvenienceExtenders;

    using DotCMIS;

    using NUnit.Framework;

    using TestUtils;

    [TestFixture, Timeout(10000), TestName("DownloadLink"), Ignore("Just for the future")]
    public class DownloadLinkTests : BaseFullRepoTest {
        [Test, Category("Slow")]
        public void CreateDownloadLink() {
            this.EnsureThatDownloadLinksAreSupported();
            var doc = this.remoteRootDir.CreateDocument("testfile.bin", "test content");

            var url = CreateDownloadLink(null, null, null, doc.Id);

            Assert.That(url, Is.Not.Null, "No download link available");
        }

        [Test, Category("Slow")]
        public void CreateDownloadLinkWithPassword() {
            this.EnsureThatDownloadLinksAreSupported();
            var doc = this.remoteRootDir.CreateDocument("testfile.bin", "test content");

            var url = CreateDownloadLink(null, "password", null, doc.Id);

            Assert.That(url, Is.Not.Null, "No download link available");
        }

        [Test, Category("Slow")]
        public void CreatDownloadLinkWithMail() {
            this.EnsureThatDownloadLinksAreSupported();
            var doc = this.remoteRootDir.CreateDocument("testfile.bin", "test content");

            var url = CreateDownloadLink(null, null, "jenkins@dataspace.cc", doc.Id);

            Assert.That(url, Is.Not.Null, "No download link available");
        }

        [Test, Category("Slow")]
        public void CreateDownloadLinkWithExpirationTime() {
            this.EnsureThatDownloadLinksAreSupported();
            var doc = this.remoteRootDir.CreateDocument("testfile.bin", "test content");

            var url = CreateDownloadLink(new TimeSpan(1, 0, 0), null, null, doc.Id);

            Assert.That(url, Is.Not.Null, "No download link available");
        }

        private Uri CreateDownloadLink(TimeSpan? expirationIn = null, string password = null, string mailAddress = null, params string[] objectIds) {
            IDictionary<string,object> properties = new Dictionary<string,object>();
            properties.Add(PropertyIds.ObjectTypeId, "cmis:item");
            List<string> idsSecondary = new List<string>();
            idsSecondary.Add("cmis:rm_clientMgtRetention");
            idsSecondary.Add("gds:downloadLink");
            properties.Add(PropertyIds.SecondaryObjectTypeIds, idsSecondary);

            properties.Add("cmis:rm_expirationDate", DateTime.UtcNow + (TimeSpan)(expirationIn ?? new TimeSpan(24, 0, 0)));

            List<string> idsObject = new List<string>();
            foreach(var objectId in objectIds) {
                idsObject.Add(objectId);
            }

            properties.Add("gds:objectIds", idsObject);
            properties.Add("gds:comment", "Create download link comment");
            properties.Add("gds:subject", "Create download link subject");
            properties.Add("gds:message", "Create download link message");
            if (mailAddress != null) {
                properties.Add("gds:emailAddress", mailAddress);
            }

            if (password != null) {
                properties.Add("gds:password", password);
            }

            var id = this.session.CreateItem(properties, this.session.GetObject(this.session.RepositoryInfo.RootFolderId));
            ICmisObject cmis = this.session.GetObject(id);
            var url = cmis.GetPropertyValue("gds:url") as string;
            return url == null ? null : new Uri(url);
        }

        private void EnsureThatDownloadLinksAreSupported() {
            if (double.Parse(this.session.RepositoryInfo.CmisVersionSupported) < 1.1) {
                Assert.Ignore("Server does not support to create download link");
            }
        }
    }
}