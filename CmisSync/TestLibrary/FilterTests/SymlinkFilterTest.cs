
namespace TestLibrary.FilterTests {
    using System;

    using CmisSync.Lib.Filter;
    using CmisSync.Lib.Storage.FileSystem;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class SymlinkFilterTest {
        [Test, Category("Fast")]
        public void DetectSymlinksCorrectly([Values(true, false)]bool exists, [Values(true, false)]bool isSymlink) {
            var underTest = new SymlinkFilter();
            string path = "path";
            string reason;
            var fileInfo = new Mock<IFileSystemInfo>(MockBehavior.Strict);
            fileInfo.Setup(f => f.Exists).Returns(exists);
            fileInfo.Setup(f => f.IsSymlink).Returns(isSymlink);
            fileInfo.Setup(f => f.FullName).Returns(path);
            var result = underTest.IsSymlink(fileInfo.Object, out reason);
            Assert.That(result, Is.EqualTo(exists && isSymlink));
            Assert.That(reason, Is.Not.Null);
            if (result) {
                Assert.That(reason, Is.StringContaining(path));
                fileInfo.Verify(f => f.FullName, Times.Once());
            } else {
                Assert.That(reason, Is.EqualTo(string.Empty));
                fileInfo.Verify(f => f.FullName, Times.Never());
            }

            fileInfo.Verify(f => f.Exists, Times.AtMostOnce());
            fileInfo.Verify(f => f.IsSymlink, Times.AtMostOnce());
        }
    }
}