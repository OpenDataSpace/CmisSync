//-----------------------------------------------------------------------
// <copyright file="BaseLinkTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.IntegrationTests.LinkTests {
    using System;

    using CmisSync.Lib.Cmis.ConvenienceExtenders;

    ﻿using DotCMIS.Client;

    using NUnit.Framework;

    using TestUtils;

    [TestFixture, Category("Links"), Category("Slow")]
    public abstract class BaseLinkTest : BaseFullRepoTest {
        [SetUp]
        public void EnsureThatDownloadLinksAreSupported() {
            if (!this.session.AreLinksSupported()) {
                Assert.Ignore("Server does not support to create download link");
            }
        }

        protected static void VerifyThatLinkIsEqualToGivenParamsAndContainsUrl(
            ICmisObject link,
            string subject,
            bool notifyAboutLinkUsage,
            bool withExpiration,
            LinkType type)
        {
            Assert.That(link, Is.Not.Null, "No download link available");
            Assert.That(link.GetUrl(), Is.Not.Null, "no Url is available");
            Assert.That(link.GetNotificationStatus(), Is.EqualTo(notifyAboutLinkUsage), "Notification Status is wrong");
            Assert.That(link.GetSubject(), Is.EqualTo(subject), "Subject is wrong");
            Assert.That(link.GetLinkType(), Is.EqualTo(type), "Link Type is wrong");
            if (withExpiration) {
                Assert.That(link.GetExpirationDate(), Is.EqualTo(DateTime.UtcNow.AddHours(1)).Within(10).Minutes, "Expiration date is wrong");
            } else {
                Assert.That(() => link.GetExpirationDate(), Throws.Nothing, "Requesting Expiration date failed");
            }
        }
    }
}