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

namespace TestLibrary.IntegrationTests.LinkTests {
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
    public class DownloadLinkTests : BaseLinkTest {
        [Test, Pairwise]
        public void CreateDownloadLink(
            [Values(true, false)]bool withExpiration,
            [Values(null, "password")]string password,
            [Values(null, "", "justDropThis@test.dataspace.cc", "justDropThis@test.dataspace.cc,alsoDropThis@test.dataspace.cc")]string mail,
            [Values(null, "", "mailSubject")]string subject,
            [Values(null, "", "message")]string message,
            [Values(true, false)]bool notifyAboutLinkUsage)
        {
            var doc = this.remoteRootDir.CreateDocument("testfile.bin", "test content");
            IList<string> mails = mail == null ? null : new List<string>(mail.Split(','));
            var link = session.CreateDownloadLink(
                expirationIn: withExpiration ? (TimeSpan?)new TimeSpan(1,0,0) : (TimeSpan?)null,
                password: password,
                mailAddresses: mails,
                subject: subject,
                message: message,
                notifyAboutLinkUsage: notifyAboutLinkUsage,
                objectIds: doc.Id);

            VerifyThatLinkIsEqualToGivenParamsAndContainsUrl(link, subject, notifyAboutLinkUsage, withExpiration, LinkType.DownloadLink);
        }
    }
}