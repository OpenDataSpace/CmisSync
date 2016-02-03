//-----------------------------------------------------------------------
// <copyright file="ListLinksTests.cs" company="GRAU DATA AG">
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
﻿namespace TestLibrary.IntegrationTests.LinkTests {
    using System;
    using System.Collections.Generic;

    using CmisSync.Lib.Cmis.ConvenienceExtenders;

    using DotCMIS.Client;
    using DotCMIS.Exceptions;

    using NUnit.Framework;
    using NUnit.Framework.Constraints;

    [TestFixture, Timeout(180000), TestName("ListLinks")]
    public class ListLinksTests : BaseLinkTest {
        [Test]
        public void ListLinks() {
            var doc = this.remoteRootDir.CreateDocument("testfile.bin", "test content");
            var folder = this.remoteRootDir.CreateFolder("uploadTarget");
            this.session.CreateDownloadLink(objectIds: doc.Id);
            this.session.CreateUploadLink(targetFolder: folder);

            var results = new List<IQueryResult>(this.session.GetAllLinks());

            Assert.That(results.Count, Is.GreaterThanOrEqualTo(2));
            VerifyThatResultsAreValid(on: results);
        }

        [Test]
        public void ListLinksByType([Values(LinkType.DownloadLink, LinkType.UploadLink)]LinkType type) {
            this.EnsureThatListingLinksIsSupported(type);
            var doc = this.remoteRootDir.CreateDocument("testfile.bin", "test content");
            var folder = this.remoteRootDir.CreateFolder("uploadTarget");
            int linkCount = new List<IQueryResult>(this.session.GetAllLinks(ofType: type)).Count;
            int expectedLinkCount = linkCount + 1;
            this.session.CreateDownloadLink(objectIds: doc.Id);
            this.session.CreateUploadLink(targetFolder: folder);

            var results = new List<IQueryResult>(this.session.GetAllLinks(ofType: type));

            Assert.That(results.Count, Is.EqualTo(expectedLinkCount));
            VerifyThatResultsAreValid(on: results, andLinkType: Is.EqualTo(type));
        }

        private static void VerifyThatResultsAreValid(IList<IQueryResult> on, IResolveConstraint andLinkType = null) {
            andLinkType = andLinkType ?? Is.EqualTo(LinkType.UploadLink).Or.EqualTo(LinkType.DownloadLink);
            foreach (var link in on) {
                Assert.That(link.GetId(), Is.Not.Null.Or.Empty);
                Assert.That(link.GetLinkType(), andLinkType);
                Assert.That(link.GetUrl().AbsoluteUri, Is.Not.Null.Or.Empty);
            }
        }

        [SetUp]
        public void EnsureThatListingLinksIsSupported(LinkType? of = null) {
            try {
                new List<IQueryResult>(this.session.GetAllLinks(of));
            } catch (CmisNotSupportedException) {
                Assert.Ignore("Server does not support to query links");
            }
        }
    }
}