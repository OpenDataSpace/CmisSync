using CmisSync.Lib.Storage;

using NUnit.Framework;

using Moq;
namespace TestLibrary.StorageTests {
          
    [TestFixture]
    public class FileSystemWrapperTests {

        [Test, Category("Medium")]
        public void FileInfoTest() {
            IFileSystemInfoFactory factory = new FileSystemInfoFactory();
            IFileInfo fileInfo = factory.CreateFileInfo("");
            Assert.That(fileInfo, Is.Not.Null);
        }

        [Test, Category("Medium")]
        public void DirectoryInfoTest() {
            IFileSystemInfoFactory factory = new FileSystemInfoFactory();
            IDirectoryInfo fileInfo = factory.CreateDirectoryInfo("");
            Assert.That(fileInfo, Is.Not.Null);
        }
    }
}
