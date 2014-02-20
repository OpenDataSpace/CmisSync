using System;
using System.IO;

using CmisSync.Lib.Storage;

using NUnit.Framework;

using Moq;
namespace TestLibrary.StorageTests {
          
    [TestFixture]
    public class FileSystemWrapperTests {
        private static readonly IFileSystemInfoFactory factory = new FileSystemInfoFactory();
        DirectoryInfo testFolder;

        [SetUp]
        public void Init() {
            string tempPath = Path.GetTempPath();
            var tempFolder = new DirectoryInfo(tempPath);
            Assert.That(tempFolder.Exists, Is.True);
            testFolder = tempFolder.CreateSubdirectory("DSSFileSystemWrapperTest");
        }

        [TearDown]
        public void Cleanup() {
            testFolder.Delete(true);
        }

        [Test, Category("Medium")]
        public void FileInfoConstruction() {
            string fileName = "test1";
            string fullPath = Path.Combine(testFolder.FullName, fileName);
            IFileInfo fileInfo = factory.CreateFileInfo(fullPath);
            Assert.That(fileInfo, Is.Not.Null);
        }

        [Test, Category("Medium")]
        public void DirectoryInfoConstruction() {
            string fileName = "test1";
            string fullPath = Path.Combine(testFolder.FullName, fileName);
            IDirectoryInfo fileInfo = factory.CreateDirectoryInfo(fullPath);
            Assert.That(fileInfo, Is.Not.Null);
        }

        [Test, Category("Medium")]
        public void FullPath() {
            string fileName = "test1";
            string fullPath = Path.Combine(testFolder.FullName, fileName);
            IFileSystemInfo fileInfo = factory.CreateFileInfo(fullPath);
            Assert.That(fileInfo.FullName, Is.EqualTo(fullPath));
        }

        [Test, Category("Medium")]
        public void Exists() {
            string fileName = "test1";
            string fullPath = Path.Combine(testFolder.FullName, fileName);
            IFileSystemInfo fileInfo = factory.CreateFileInfo(fullPath);
            Assert.That(fileInfo.Exists, Is.EqualTo(false));
            new FileInfo(fullPath).Create();
            fileInfo = factory.CreateFileInfo(fullPath);
            Assert.That(fileInfo.Exists, Is.EqualTo(true));
        }

        [Test, Category("Medium")]
        public void Refresh() {
            string fileName = "test1";
            string fullPath = Path.Combine(testFolder.FullName, fileName);
            IFileSystemInfo fileInfo = factory.CreateFileInfo(fullPath);
            //trigger lacy loading
            Assert.That(fileInfo.Exists, Is.EqualTo(false));
            new FileInfo(fullPath).Create();
            fileInfo.Refresh();
            Assert.That(fileInfo.Exists, Is.EqualTo(true));
        }

        [Test, Category("Medium")]
        public void Name() {
            string fileName = "test1";
            string fullPath = Path.Combine(testFolder.FullName, fileName);
            IFileSystemInfo fileInfo = factory.CreateFileInfo(fullPath);
            Assert.That(fileInfo.Name, Is.EqualTo(fileName));
        }

        [Test, Category("Medium")]
        public void Attributes() {
            string fileName = "test1";
            string fullPath = Path.Combine(testFolder.FullName, fileName);
            testFolder.CreateSubdirectory(fileName);
            IFileSystemInfo fileInfo = factory.CreateDirectoryInfo(fullPath);
            Assert.That(fileInfo.Attributes, Is.EqualTo(FileAttributes.Directory));
        }

        [Test, Category("Medium")]
        public void Create() {
            string fileName = "test1";
            string fullPath = Path.Combine(testFolder.FullName, fileName);
            IDirectoryInfo dirInfo = factory.CreateDirectoryInfo(fullPath);
            dirInfo.Create();
            Assert.That(dirInfo.Exists, Is.EqualTo(true));
        }

        [Test, Category("Medium")]
        public void Directory() {
            string fileName = "test1";
            string fullPath = Path.Combine(testFolder.FullName, fileName);
            IFileInfo fileInfo = factory.CreateFileInfo(fullPath);
            Assert.That(fileInfo.Directory.FullName, Is.EqualTo(testFolder.FullName));
        }

        [Test, Category("Medium")]
        public void Parent() {
            string fileName = "test1";
            string fullPath = Path.Combine(testFolder.FullName, fileName);
            IDirectoryInfo dirInfo = factory.CreateDirectoryInfo(fullPath);
            Assert.That(dirInfo.Parent.FullName, Is.EqualTo(testFolder.FullName));
        }
    }
}
