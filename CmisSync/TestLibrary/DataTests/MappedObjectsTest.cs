using System;
using System.IO;

using CmisSync.Lib.Data;

using DotCMIS.Client;

using NUnit.Framework;

using Moq;
using CmisSync.Lib.Storage;

namespace TestLibrary.DataTests
{
    using TestUtils;
    
    public class MappedObjectMockUtils
    {
        public static Mock<IFileSystemInfoFactory> CreateFsFactory(string localRootPath, string localRootPathName, string localFilePath = null, string localFileName = null)
        {
            var factory = new Mock<IFileSystemInfoFactory>();
            var dirinfo = new Mock<IDirectoryInfo>();
            dirinfo.Setup(dir => dir.Name).Returns(localRootPathName);
            dirinfo.Setup(dir => dir.Exists).Returns(true);
            factory.Setup(f => f.CreateDirectoryInfo(It.Is<string>(path => path == localRootPath))).Returns(dirinfo.Object);
            var fileInfo = new Mock<IFileInfo>();
            fileInfo.Setup (file => file.Name).Returns(localFileName);
            fileInfo.Setup (file => file.Exists).Returns(true);
            factory.Setup(f => f.CreateFileInfo(It.Is<string>(path => path == localFilePath))).Returns(fileInfo.Object);
            return factory;
        }
    }

    [TestFixture]
    public class MappedObjectTest
    {
        private readonly string localRootPathName = "folder";
        private readonly string localRootPath = Path.Combine("local", "test", "folder");
        private readonly string remoteRootPath = "/";
        private readonly string localFileName = "file.test";
        private readonly string localFilePath = Path.Combine("local", "test", "folder", "file.test");

        private Mock<IFileSystemInfoFactory> createFactoryWithLocalPathInfos()
        {
            return MappedObjectMockUtils.CreateFsFactory(localRootPath,localRootPathName, localFilePath, localFileName);
        }

        [Test, Category("Fast"), Category("MappedObjects")]
        public void ConstructorTakesData() {
            var data = new MappedObject
            {
                Name = "name",
                Description = string.Empty,
                Guid = Guid.NewGuid(),
                ParentId = "parentId",
                Type = MappedObjectType.File
            };

            var file = new MappedObject(data);

            Assert.AreEqual(data, file as MappedObject);
        }

        [Test, Category("Fast"), Category("MappedObjects")]
        public void ConstructorSetsDefaultParamsToNull()
        {
            var file = new MappedObject();
            Assert.IsNull(file.ChecksumAlgorithmName);
            Assert.IsNull(file.Description);
            Assert.IsNull(file.LastChangeToken);
            Assert.IsNull(file.LastChecksum);
            Assert.IsNull(file.LastLocalWriteTimeUtc);
            Assert.IsNull(file.LastRemoteWriteTimeUtc);
            Assert.IsNull(file.Name);
            Assert.AreEqual(MappedObjectType.Unkown, file.Type);
            Assert.AreEqual(-1, file.LastContentSize);
        }

        [Test, Category("Fast"), Category("MappedObjects")]
        public void HashAlgorithmProperty()
        {
            var file = new MappedObject(new MappedObject{Type = MappedObjectType.File, ChecksumAlgorithmName = "MD5"});
            Assert.AreEqual("MD5", file.ChecksumAlgorithmName);

            file.ChecksumAlgorithmName = "SHA1";
            Assert.AreEqual("SHA1", file.ChecksumAlgorithmName);
        }

        [Test, Category("Fast"), Category("MappedObjects")]
        public void DescriptionProperty()
        {
            var file = new MappedObject(new MappedObject{Type = MappedObjectType.File, Description = "desc"});
            Assert.AreEqual("desc", file.Description);

            file.Description = "other desc";
            Assert.AreEqual("other desc", file.Description);
        }


        [Test, Category("Fast"), Category("MappedObjects")]
        public void IFolderConstructor ()
        {
            string folderName = "a";
            string path = Path.Combine(Path.GetTempPath(), folderName);
            string id = "id";
            string parentId = "papa";
            string lastChangeToken = "token";
            Mock<IFolder> remoteObject = MockSessionUtil.CreateRemoteFolderMock(id, path, parentId, lastChangeToken);
            MappedObject mappedObject = new MappedObject(remoteObject.Object);
            Assert.That(mappedObject.RemoteObjectId, Is.EqualTo(id), "RemoteObjectId incorrect");
            Assert.That(mappedObject.Name, Is.EqualTo(folderName), "Name incorrect");
            Assert.That(mappedObject.ParentId, Is.EqualTo(parentId), "ParentId incorrect");
            Assert.That(mappedObject.LastChangeToken, Is.EqualTo(lastChangeToken), "LastChangeToken incorrect");
            Assert.That(mappedObject.Type, Is.EqualTo(MappedObjectType.Folder), "Type incorrect");
        }
    }
}

