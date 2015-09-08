
namespace TestLibrary.StorageTests.AlphaFSTests {
    using System;

    using Alphaleonis.Win32.Filesystem;

    using NUnit.Framework;

    [TestFixture]
    public class FileInfoWithLongPathTest {
        [Test, Category("Medium")]
        public void CreateInstance() {
            var file = new FileInfo(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()), PathFormat.FullPath);
            Assert.That(file.Exists, Is.False);
        }

        [Test, Category("Medium")]
        public void WriteADS() {
            var fileName = Guid.NewGuid().ToString();
            var adsName = "DSS-Test";
            File.WriteAllText(Path.Combine(Path.GetTempPath(), fileName + ":" + adsName, PathFormat.FullPath), fileName);

            var file = new FileInfo(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()) , PathFormat.FullPath);
            Assert.That(file.Exists, Is.True);
            foreach (var stream in file.EnumerateAlternateDataStreams()) {
                Assert.That(stream.StreamName, Is.EqualTo(string.Empty).Or.EqualTo(adsName));
                if (stream.StreamName == adsName) {
                    Assert.That(stream.Size, Is.EqualTo(fileName.Length));
                } else {
                    Assert.That(stream.Size, Is.EqualTo(0));
                }
            }
        }
    }
}