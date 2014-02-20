using System;
using System.IO;

using CmisSync.Lib.Storage;

using NUnit.Framework;

using Moq;
namespace TestLibrary.StorageTests {
          
    [TestFixture]
    public class FileSystemWrapperTests {
        DirectoryInfo testFolder;

        [SetUp]
        public void Init() {
            string tempPath = Path.GetTempPath();
            var tempFolder = new DirectoryInfo(tempPath);
            Assert.That(tempFolder.Exists, Is.True);
            testFolder = tempFolder.CreateSubdirectory("DSSFileSystemWrapperTest");
            Console.WriteLine(testFolder.FullName);
        }

        [TearDown]
        public void Cleanup() {
            testFolder.Delete(true);
        }

        [Test, Category("Medium")]
        public void FileInfoTest() {
            IFileSystemInfoFactory factory = new FileSystemInfoFactory();
            IFileInfo fileInfo = factory.CreateFileInfo("");
            Assert.That(fileInfo, Is.Not.Null);
            Assert.That(fileInfo.FullName, Is.Not.Null);
        }

        [Test, Category("Medium")]
        public void DirectoryInfoTest() {
            IFileSystemInfoFactory factory = new FileSystemInfoFactory();
            IDirectoryInfo directoryInfo = factory.CreateDirectoryInfo("");
            Assert.That(directoryInfo, Is.Not.Null);
            Assert.That(directoryInfo.FullName, Is.Not.Null);
        }
    }
}
