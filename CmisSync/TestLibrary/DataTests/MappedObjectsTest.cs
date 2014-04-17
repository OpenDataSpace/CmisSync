using System;
using System.IO;

using CmisSync.Lib.Data;

using DotCMIS.Client;

using NUnit.Framework;

using Moq;
using CmisSync.Lib.Storage;

namespace TestLibrary.DataTests
{

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
    public class MappedFolderTest
    {
        private readonly string localRootPathName = "folder";
        private readonly string localRootPath = Path.Combine("local", "test", "folder");
        private readonly string remoteRootPath = "/";

        private Mock<IFileSystemInfoFactory> createFactoryWithLocalPathInfos()
        {
            return MappedObjectMockUtils.CreateFsFactory(localRootPath,localRootPathName);
        }

        [Test, Category("Fast"), Category("MappedObjects")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorFailsIfStorageIsNull()
        {
            new MappedFolder(null, null);
        }

        [Test, Category("Fast"), Category("MappedObjects")]
        public void ConstructorWorksIfFsFactoryIsNull()
        {
            var storage = Mock.Of<IMetaDataStorage>();
            var folder = new MappedFolder(null, storage, null);
            Assert.IsNull(folder.Name);
            Assert.IsNull(folder.ParentId);
        }

        [Test, Category("Fast"), Category("MappedObjects")]
        public void ConstructorTakesFsFactory()
        {
            var factory = Mock.Of<IFileSystemInfoFactory>();
            var storage = Mock.Of<IMetaDataStorage>();
            new MappedFolder(null, storage, factory);
        }

        [Test, Category("Fast"), Category("MappedObjects")]
        public void ConstructorTakesData()
        {
            var factory = Mock.Of<IFileSystemInfoFactory>();
            var storage = Mock.Of<IMetaDataStorage>();
            var data = new MappedObjectData
            {
                Name = "name",
                ParentId = null,
                Description = string.Empty,
                Guid = Guid.NewGuid(),
                Type = MappedObjectType.Folder
            };

            var folder = new MappedFolder(data, storage, factory);

            Assert.AreEqual(folder as MappedObjectData, data);
        }

        [Test, Category("Fast"), Category("MappedObjects")]
        public void ExistsLocally()
        {
            string childName = "child";
            var storage = new Mock<IMetaDataStorage>();
            var factory = createFactoryWithLocalPathInfos();
            var childInfo = new Mock<IDirectoryInfo>();
            childInfo.Setup(dir => dir.Name).Returns(childName);
            childInfo.Setup(dir => dir.Exists).Returns(false);
            storage.Setup(s => s.GetLocalPath(It.Is<IMappedObject>(o => o.Name == localRootPathName))).Returns(localRootPath);
            storage.Setup(s => s.GetLocalPath(It.Is<IMappedObject>(o => o.Name == childName))).Returns(Path.Combine(localRootPath, childName));
            factory.Setup(f => f.CreateDirectoryInfo(It.Is<string>(path => path == Path.Combine(localRootPath, childName)))).Returns(childInfo.Object);

            var rootFolder = new MappedFolder(new MappedObjectData{ Name = localRootPathName}, storage.Object, factory.Object);
            var childFolder = new MappedFolder(new MappedObjectData {Name = childName}, storage.Object, factory.Object);
            Assert.IsTrue(rootFolder.ExistsLocally());
            Assert.IsFalse(childFolder.ExistsLocally());
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
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorFailsWithoutStorageInstance()
        {
            new MappedObject(null, null);
        }

        [Test, Category("Fast"), Category("MappedObjects")]
        public void ConstructorTakesStorage()
        {
            var storage = Mock.Of<IMetaDataStorage>();
            new MappedObject(null, storage);
        }

        [Test, Category("Fast"), Category("MappedObjects")]
        public void ConstructorTakesStorageAndFsFactory()
        {
            var factory = Mock.Of<IFileSystemInfoFactory>();
            var storage = Mock.Of<IMetaDataStorage>();
            var obj = new MappedObject(null, storage, factory);
            Assert.AreEqual(factory, obj.FsFactory);
        }

        [Test, Category("Fast"), Category("MappedObjects")]
        public void ConstructorTakesData() {
            var storage = Mock.Of<IMetaDataStorage>();
            var data = new MappedObjectData
            {
                Name = "name",
                Description = string.Empty,
                Guid = Guid.NewGuid(),
                ParentId = "parentId",
                Type = MappedObjectType.File
            };

            var file = new MappedObject(data, storage);

            Assert.AreEqual(data, file as MappedObjectData);
        }

        [Test, Category("Fast"), Category("MappedObjects")]
        public void ConstructorSetsDefaultParamsToNull()
        {
            var file = new MappedObject(null, Mock.Of<IMetaDataStorage>());
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
            var file = new MappedObject(new MappedObjectData{Type = MappedObjectType.File, ChecksumAlgorithmName = "MD5"}, Mock.Of<IMetaDataStorage>());
            Assert.AreEqual("MD5", file.ChecksumAlgorithmName);

            file.ChecksumAlgorithmName = "SHA1";
            Assert.AreEqual("SHA1", file.ChecksumAlgorithmName);
        }

        [Test, Category("Fast"), Category("MappedObjects")]
        public void DescriptionProperty()
        {
            var file = new MappedObject(new MappedObjectData{Type = MappedObjectType.File, Description = "desc"}, Mock.Of<IMetaDataStorage>());
            Assert.AreEqual("desc", file.Description);

            file.Description = "other desc";
            Assert.AreEqual("other desc", file.Description);
        }

        [Test, Category("Fast"), Category("MappedObjects")]
        public void GetLocalPath()
        {
            var factory = createFactoryWithLocalPathInfos();
            var storage = new Mock<IMetaDataStorage>();
            string remoteId = "remoteId";
            storage.Setup(s => s.GetLocalPath(It.Is<IMappedObject>(o => o.RemoteObjectId == remoteId))).Returns(localFilePath);

            var file = new MappedObject(new MappedObjectData{Type = MappedObjectType.File, RemoteObjectId = remoteId}, storage.Object, factory.Object);
            file.Name = localFileName;

            Assert.AreEqual(localFilePath, file.LocalSyncTargetPath);
        }

        [Test, Category("Fast"), Category("MappedObjects")]
        public void GetRemotePath()
        {
            var factory = createFactoryWithLocalPathInfos();
            var storage = new Mock<IMetaDataStorage>();
            string remoteId = "remoteId";
            storage.Setup(s => s.GetRemotePath(It.Is<IMappedObject>(o => o.RemoteObjectId == remoteId))).Returns(remoteRootPath + localFileName);
        
            var file = new MappedObject(new MappedObjectData{Type = MappedObjectType.File, RemoteObjectId = remoteId}, storage.Object, factory.Object);
            file.Name = localFileName;

            Assert.AreEqual(remoteRootPath + localFileName, file.RemoteSyncTargetPath);
        }

        [Test, Category("Fast"), Category("MappedObjects")]
        public void ExistsLocally() {
            var factory = createFactoryWithLocalPathInfos();
            var storage = new Mock<IMetaDataStorage>();
            string remoteId = "remoteId";
            storage.Setup(s => s.GetLocalPath(It.Is<IMappedObject>(o => o.RemoteObjectId == remoteId))).Returns(localFilePath);

            var file = new MappedObject(new MappedObjectData{Type = MappedObjectType.File, RemoteObjectId = remoteId}, storage.Object, factory.Object);
            file.Name = localFileName;

            Assert.IsTrue(file.ExistsLocally());
            var fileInfo = new Mock<IFileInfo>();
            fileInfo.Setup (f => f.Name).Returns(localFileName);
            fileInfo.Setup (f => f.Exists).Returns(false);
            factory.Setup(f => f.CreateFileInfo(It.Is<string>(path => path == localFilePath))).Returns(fileInfo.Object);

            Assert.IsFalse(file.ExistsLocally());
        }
    }
}

