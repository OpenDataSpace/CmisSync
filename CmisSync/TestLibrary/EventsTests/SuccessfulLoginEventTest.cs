
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
        public void ConstructorTakesUrlAndSessionAndRootFolder(
            [Values(true, false)]bool pwcIsSupported,
            [Values(true, false)]bool selectiveSyncSupported,
            [Values(true, false)]bool changeEventsSupported)
        {
            var underTest = new SuccessfulLoginEvent(this.url, this.session, this.rootFolder, pwcIsSupported, selectiveSyncSupported, changeEventsSupported);

            Assert.That(underTest.Session, Is.EqualTo(this.session));
            Assert.That(underTest.RootFolder, Is.EqualTo(this.rootFolder));
            Assert.That(underTest.PrivateWorkingCopySupported, Is.EqualTo(pwcIsSupported));
            Assert.That(underTest.SelectiveSyncSupported, Is.EqualTo(selectiveSyncSupported));
            Assert.That(underTest.ChangeEventsSupported, Is.EqualTo(changeEventsSupported));
        }

        [Test]
        public void ConstructorFailsIfUrlIsNull(
            [Values(true, false)]bool pwcIsSupported,
            [Values(true, false)]bool selectiveSyncSupported,
            [Values(true, false)]bool changeEventsSupported)
        {
            Assert.Throws<ArgumentNullException>(() => new SuccessfulLoginEvent(null, this.session, this.rootFolder, pwcIsSupported, selectiveSyncSupported, changeEventsSupported));
        }

        [Test]
        public void ConstructorFailsIfSessionIsNull(
            [Values(true, false)]bool pwcIsSupported,
            [Values(true, false)]bool selectiveSyncSupported,
            [Values(true, false)]bool changeEventsSupported)
        {
            Assert.Throws<ArgumentNullException>(() => new SuccessfulLoginEvent(this.url, null, this.rootFolder, pwcIsSupported, selectiveSyncSupported, changeEventsSupported));
        }

        [Test]
        public void ConstructorFailsIfRootFolderIsNull(
            [Values(true, false)]bool pwcIsSupported,
            [Values(true, false)]bool selectiveSyncSupported,
            [Values(true, false)]bool changeEventsSupported)
        {
            Assert.Throws<ArgumentNullException>(() => new SuccessfulLoginEvent(this.url, this.session, null, pwcIsSupported, selectiveSyncSupported, changeEventsSupported));
        }
    }
}