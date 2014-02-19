using CmisSync.Lib.Storage;

using NUnit.Framework;

using Moq;
namespace TestLibrary.StorageTests {

    [TestFixture]
    public class FileSystemWrapperTests {

        [Test, Category=("Medium")]
        public void FileInfoTest() {
            IFileSystemInfoFactory factory = new FileSystemInfoFactory();
        }
    }
}
