//-----------------------------------------------------------------------
// <copyright file="DownloadLinkTests.cs" company="GRAU DATA AG">
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

namespace TestLibrary.IntegrationTests {
    using System;
    using System.Collections.Generic;

    using CmisSync.Lib.Cmis.ConvenienceExtenders;

    using DotCMIS;
    using DotCMIS.Client;
    using DotCMIS.Client.Impl;
    using DotCMIS.Enums;

    using NUnit.Framework;

    using TestUtils;

    [TestFixture, Timeout(180000), TestName("DownloadLink")]
    public class DownloadLinkTests : BaseFullRepoTest {
        [Test]
        public void CreateDownloadLink(
            [Values(true, false)]bool withExpiration,
            [Values(null, "password")]string password,
            [Values(null, "jenkins@dataspace.cc")]string mail,
            [Values(null, "", "mailSubject")]string subject,
            [Values(null, "", "message")]string message)
        {
            this.EnsureThatDownloadLinksAreSupported();
            var doc = this.remoteRootDir.CreateDocument("testfile.bin", "test content");

            var url = this.CreateDownloadLink(
                expirationIn: withExpiration ? (TimeSpan?)new TimeSpan(1,0,0) : (TimeSpan?)null,
                password: password,
                mailAddress: mail,
                subject: subject,
                message: message,
                objectIds: doc.Id);

            Assert.That(url, Is.Not.Null, "No download link available");
        }

        private Uri CreateDownloadLink(
            TimeSpan? expirationIn = null,
            string password = null,
            string mailAddress = null,
            string subject = null,
            string message = null,
            params string[] objectIds)
        {
            IDictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add(PropertyIds.ObjectTypeId, BaseTypeId.CmisItem.GetCmisValue());
            List<string> idsSecondary = new List<string>();
            idsSecondary.Add("gds:link");
            properties.Add(PropertyIds.SecondaryObjectTypeIds, idsSecondary);
            properties.Add("gds:linkType", "gds:downloadLink");
            properties.Add("cmis:rm_expirationDate", DateTime.UtcNow + (TimeSpan)(expirationIn ?? new TimeSpan(24, 0, 0)));
            properties.Add("gds:subject", subject ?? string.Empty);
            properties.Add("gds:message", message ?? string.Empty);
            properties.Add("gds:emailAddress", mailAddress ?? string.Empty);
            properties.Add("gds:password", password);
            var linkItem = this.session.CreateItem(properties, this.remoteRootDir);
            foreach (var objectId in objectIds) {
                IDictionary<string, object> relProperties = new Dictionary<string, object>();
                relProperties.Add(PropertyIds.ObjectTypeId, BaseTypeId.CmisRelationship.GetCmisValue());
                relProperties.Add(PropertyIds.SourceId, linkItem.Id);
                relProperties.Add(PropertyIds.TargetId, objectId);
                this.session.CreateRelationship(relProperties);
            }

            ICmisObject link = this.session.GetObject(linkItem);
            var url = link.GetPropertyValue("gds:url") as string;
            return url == null ? null : new Uri(url);
        }

        private void EnsureThatDownloadLinksAreSupported() {
            if (!this.session.AreDownloadLinksSupported()) {
                Assert.Ignore("Server does not support to create download link");
            }
        }
    }
}