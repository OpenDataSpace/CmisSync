using System;
using NUnit.Framework;
using Moq;
using CmisSync.Lib.Events;
using CmisSync.Lib.Storage;

namespace TestLibrary.EventsTests {
    [TestFixture]
    public class FileEventTest {
        [Test, Category("Fast")]
        public void ConstructorTakesIFileInfoInstance()
        {
            new FileEvent(new Mock<IFileInfo>().Object);
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorFailsOnNullParameter()
        {
            new FileEvent();
        }

        [Test, Category("Fast")]
        public void EqualityNull() {
            var localFile = new Mock<IFileInfo>();
            localFile.Setup(f => f.FullName).Returns("bla");
            var fe = new FileEvent(localFile.Object);
            Assert.That(fe, Is.Not.EqualTo(null));
        }

        [Test, Category("Fast")]
        public void EqualitySame() {
            var localFile = new Mock<IFileInfo>();
            localFile.Setup(f => f.FullName).Returns("bla");
            var fe = new FileEvent(localFile.Object);
            Assert.That(fe, Is.EqualTo(fe));
        }
    }
}
