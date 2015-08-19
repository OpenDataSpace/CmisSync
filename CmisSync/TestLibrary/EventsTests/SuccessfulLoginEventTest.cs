
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
        public void ConstructorTakesUrlAndSessionAndRootFolder() {
            var underTest = new SuccessfulLoginEvent(this.url, this.session, this.rootFolder);

            Assert.That(underTest.Session, Is.EqualTo(this.session));
            Assert.That(underTest.RootFolder, Is.EqualTo(this.rootFolder));
        }

        [Test]
        public void ConstructorFailsIfUrlIsNull() {
            Assert.Throws<ArgumentNullException>(() => new SuccessfulLoginEvent(null, this.session, this.rootFolder));
        }

        [Test]
        public void ConstructorFailsIfSessionIsNull() {
            Assert.Throws<ArgumentNullException>(() => new SuccessfulLoginEvent(this.url, null, this.rootFolder));
        }

        [Test]
        public void ConstructorFailsIfRootFolderIsNull() {
            Assert.Throws<ArgumentNullException>(() => new SuccessfulLoginEvent(this.url, this.session, null));
        }
    }
}