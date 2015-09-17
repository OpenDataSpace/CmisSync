
namespace TestLibrary.IntegrationTests.LinkTests {
    using System;
    using System.Collections.Generic;

    using CmisSync.Lib.Cmis.ConvenienceExtenders;

    using DotCMIS.Exceptions;

    using NUnit.Framework;

    using TestUtils;

    [TestFixture, Timeout(180000), TestName("UploadLink")]
    public class UploadLinkTests : BaseLinkTest {
        [Test]
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