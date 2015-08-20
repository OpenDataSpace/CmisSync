
namespace TestLibrary.EventsTests {
    using System;

    using CmisSync.Lib.Events;

    using DotCMIS.Client;

    using NUnit.Framework;

    using Moq;

    [TestFixture, Category("Fast")]
    public class SuccessfulLoginEventTest {
        private readonly Uri url = new Uri("https://demo.deutsche-wolke.de/cmis/browser");
        private readonly ISession session = new Mock<ISession>(MockBehavior.Strict).Object;
        private readonly IFolder rootFolder = new Mock<IFolder>(MockBehavior.Strict).Object;

        [Test]
        public void ConstructorTakesUrlAndSessionAndRootFolder([Values]bool pwcIsSupported) {
            var underTest = new SuccessfulLoginEvent(this.url, this.session, this.rootFolder, pwcIsSupported);

            Assert.That(underTest.Session, Is.EqualTo(this.session));
            Assert.That(underTest.RootFolder, Is.EqualTo(this.rootFolder));
            Assert.That(underTest.PrivateWorkingCopySupported, Is.EqualTo(pwcIsSupported));
        }

        [Test]
        public void ConstructorFailsIfUrlIsNull([Values]bool pwcIsSupported) {
            Assert.Throws<ArgumentNullException>(() => new SuccessfulLoginEvent(null, this.session, this.rootFolder, pwcIsSupported));
        }

        [Test]
        public void ConstructorFailsIfSessionIsNull([Values]bool pwcIsSupported) {
            Assert.Throws<ArgumentNullException>(() => new SuccessfulLoginEvent(this.url, null, this.rootFolder, pwcIsSupported));
        }

        [Test]
        public void ConstructorFailsIfRootFolderIsNull([Values]bool pwcIsSupported) {
            Assert.Throws<ArgumentNullException>(() => new SuccessfulLoginEvent(this.url, this.session, null, pwcIsSupported));
        }
    }
}