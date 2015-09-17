using DotCMIS.Client;


namespace TestLibrary.IntegrationTests.LinkTests {
    using System;

    using CmisSync.Lib.Cmis.ConvenienceExtenders;

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