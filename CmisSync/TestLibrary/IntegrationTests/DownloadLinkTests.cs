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

    using NUnit.Framework;

    using TestUtils;

    [TestFixture, Timeout(10000), TestName("DownloadLink")]
    public class DownloadLinkTests : BaseFullRepoTest {
        [Test, Category("Slow")]
        public void CreateDownloadLinkWithoutSubject() {
            this.EnsureThatDownloadLinksAreSupported();
            var doc = this.remoteRootDir.CreateDocument("testfile.bin", "test content");

            var url = this.session.CreateDownloadLink(objectIds: doc.Id);

            Assert.That(url, Is.Not.Null, "No download link available");
        }

        [Test, Category("Slow")]
        public void CreateDownloadLink() {
            this.EnsureThatDownloadLinksAreSupported();
            var doc = this.remoteRootDir.CreateDocument("testfile.bin", "test content");

            var url = this.session.CreateDownloadLink(subject: "Download Link", objectIds: doc.Id);

            Assert.That(url, Is.Not.Null, "No download link available");
        }

        [Test, Category("Slow")]
        public void CreateDownloadLinkWithPassword() {
            this.EnsureThatDownloadLinksAreSupported();
            var doc = this.remoteRootDir.CreateDocument("testfile.bin", "test content");

            var url = this.session.CreateDownloadLink(subject: "Download Link", password: "password", objectIds: doc.Id);

            Assert.That(url, Is.Not.Null, "No download link available");
        }

        [Test, Category("Slow")]
        public void CreatDownloadLinkWithMail() {
            this.EnsureThatDownloadLinksAreSupported();
            var doc = this.remoteRootDir.CreateDocument("testfile.bin", "test content");

            var url = this.session.CreateDownloadLink(subject: "Download Link", mailAddress: "jenkins@dataspace.cc", objectIds: doc.Id);

            Assert.That(url, Is.Not.Null, "No download link available");
        }

        [Test, Category("Slow")]
        public void CreateDownloadLinkWithExpirationTime() {
            this.EnsureThatDownloadLinksAreSupported();
            var doc = this.remoteRootDir.CreateDocument("testfile.bin", "test content");

            var url = this.session.CreateDownloadLink(subject: "Download Link", expirationIn: new TimeSpan(1, 0, 0), objectIds: doc.Id);

            Assert.That(url, Is.Not.Null, "No download link available");
        }

        private void EnsureThatDownloadLinksAreSupported() {
            if (!session.AreDownloadLinksSupported()) {
                Assert.Ignore("Server does not support to create download link");
            }
        }
    }
}