//-----------------------------------------------------------------------
// <copyright file="UploadLinkTests.cs" company="GRAU DATA AG">
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
﻿
namespace TestLibrary.IntegrationTests.LinkTests {
    using System;
    using System.Collections.Generic;

    using CmisSync.Lib.Cmis.ConvenienceExtenders;

    using DotCMIS.Exceptions;

    using NUnit.Framework;

    using TestUtils;

    [TestFixture, TestName("UploadLink")]
    public class UploadLinkTests : BaseLinkTest {
        [Test, Pairwise]
        public void CreateUploadLink(
            [Values(true, false)]bool withExpiration,
            [Values(null, "password")]string password,
            [Values(null, "", "justDropThis@test.dataspace.cc", "justDropThis@test.dataspace.cc,alsoDropThis@test.dataspace.cc")]string mail,
            [Values(null, "", "mailSubject")]string subject,
            [Values(null, "", "message")]string message,
            [Values(true, false)]bool notifyAboutLinkUsage)
        {
            var folder = this.remoteRootDir.CreateFolder("uploadTarget");
            IList<string> mails = mail == null ? null : new List<string>(mail.Split(','));
            var link = session.CreateUploadLink(
                expirationIn: withExpiration ? (TimeSpan?)new TimeSpan(1,0,0) : (TimeSpan?)null,
                password: password,
                mailAddresses: mails,
                subject: subject,
                message: message,
                notifyAboutLinkUsage: notifyAboutLinkUsage,
                objectId: folder.Id);

            VerifyThatLinkIsEqualToGivenParamsAndContainsUrl(link, subject, notifyAboutLinkUsage, withExpiration, LinkType.UploadLink);
        }

        [Test, Ignore("https://mantis.dataspace.cc/view.php?id=4727")]
        public void CreateUploadLinkWithDocumentIdMustFail() {
            var doc = this.remoteRootDir.CreateDocument("testFile.bin", "content");
            Assert.Catch<CmisBaseException>(() =>
                session.CreateUploadLink(
                    objectId: doc.Id));
        }

        [Test, Ignore("https://mantis.dataspace.cc/view.php?id=4726")]
        public void CreateLinkWithWrongMailAddressMustFail(
            [Values("wrongMail", "wrong Mail@test.dataspace.cc", "@test.dataspace.cc")]string wrongMail,
            [Values(LinkType.DownloadLink, LinkType.UploadLink)]LinkType type)
        {
            var folder = this.remoteRootDir.CreateFolder("uploadTarget");
            IList<string> mails = new List<string>();
            mails.Add(wrongMail);
            Assert.Throws<CmisInvalidArgumentException>(() =>
                session.CreateLink(
                    linkType: type,
                    mailAddresses: mails,
                    objectIds: folder.Id));
        }
    }
}