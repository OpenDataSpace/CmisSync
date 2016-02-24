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
    using DotCMIS.Enums;
    using DotCMIS.Exceptions;

    using NUnit.Framework;
    using NUnit.Framework.Constraints;

    [TestFixture, TestName("ListLinks"), Timeout(180000)]
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
            VerifyThatResultsAreValid(on: results, andLinkTypeIs: type);
        }

        private void VerifyThatResultsAreValid(IList<IQueryResult> on, LinkType? andLinkTypeIs = null) {
            foreach (var link in on) {
                Assert.That(link.GetId(), Is.Not.Null.Or.Empty);
                Assert.That(link.GetUrl().AbsoluteUri, Is.Not.Null.Or.Empty);
                var linkType = link.GetLinkType();
                if (andLinkTypeIs == null) {
                    Assert.That(linkType, Is.EqualTo(LinkType.DownloadLink).Or.EqualTo(LinkType.UploadLink));
                } else {
                    Assert.That(linkType, Is.EqualTo(andLinkTypeIs));
                }

                ICmisObject item = link.GetLinkItem(this.session);
                Assert.That(item.GetUrl(), Is.Not.Null);
            }
        }

        [SetUp]
        public void EnsureThatListingLinksIsSupported() {
            EnsureThatListingLinksIsSupported(null);
        }

        public void EnsureThatListingLinksIsSupported(LinkType? of) {
            try {
                var linkList = this.session.GetAllLinks(of);
                new List<IQueryResult>(linkList);
            } catch (CmisNotSupportedException ex) {
                Assert.Ignore("Server does not support to query links:" + ex.ErrorContent);
            } catch (CmisObjectNotFoundException) {
            }
        }
    }
}