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

        ///Add tests for FileSystemInfoWrapper members here
        [Test, Category("Medium")]
        public void FileSystemInfoTest() {
            string fileName = "test1";
            string fullPath = Path.Combine(testFolder.FullName, fileName);
            IFileSystemInfoFactory factory = new FileSystemInfoFactory();
            IFileSystemInfo fileInfo = factory.CreateFileInfo(fullPath);
            Assert.That(fileInfo, Is.Not.Null);
            Assert.That(fileInfo.FullName, Is.EqualTo(fullPath));
        }

        [Test, Category("Medium")]
        public void FileInfoTest() {
            string fileName = "test1";
            string fullPath = Path.Combine(testFolder.FullName, fileName);
            IFileSystemInfoFactory factory = new FileSystemInfoFactory();
            IFileInfo fileInfo = factory.CreateFileInfo(fullPath);
            Assert.That(fileInfo, Is.Not.Null);
            //replace this line when Interface gets first member
            Assert.That(fileInfo.FullName, Is.EqualTo(fullPath));
        }

        [Test, Category("Medium")]
        public void DirectoryInfoTest() {
            string fileName = "test1";
            string fullPath = Path.Combine(testFolder.FullName, fileName);
            IFileSystemInfoFactory factory = new FileSystemInfoFactory();
            IDirectoryInfo fileInfo = factory.CreateDirectoryInfo(fullPath);
            Assert.That(fileInfo, Is.Not.Null);
            //replace this line when Interface gets first member
            Assert.That(fileInfo.FullName, Is.EqualTo(fullPath));
        }
    }
}
