
namespace TestLibrary.StorageTests.AlphaFSTests {
    using System;

    using Alphaleonis.Win32.Filesystem;

    using NUnit.Framework;

    [TestFixture]
    public class FileInfoWithLongPathTest {
        [Test, Category("Medium")]
        public void TestCase() {
            var file = new FileInfo("test");
        }
    }
}