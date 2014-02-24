using NUnit.Framework;
using Moq;
using CmisSync.Lib.Events;
using CmisSync.Lib.Storage;

namespace TestLibrary.EventsTests {
    [TestFixture]
    public class FileEventTest {
        [Test]
        public void TestConstructor() {
            var fe = new FileEvent(new Mock<IFileInfo>().Object);
        }

        [Test]
        public void EqualityNull() {
            var localFile = new Mock<IFileInfo>();
            localFile.Setup(f => f.FullName).Returns("bla");
            var fe = new FileEvent(localFile.Object);
            Assert.That(fe, Is.Not.EqualTo(null));
        }

        [Test]
        public void EqualitySame() {
            var localFile = new Mock<IFileInfo>();
            localFile.Setup(f => f.FullName).Returns("bla");
            var fe = new FileEvent(localFile.Object);
            Assert.That(fe, Is.EqualTo(fe));
        }

    }
}
