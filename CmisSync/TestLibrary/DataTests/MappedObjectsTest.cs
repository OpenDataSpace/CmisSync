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
        [ExpectedException(typeof(ArgumentException))]
        public void ConstructorFailsIfLocalPathIsNull()
        {
            new MappedFolder((string) null, remoteRootPath);
        }

        [Test, Category("Fast"), Category("MappedObjects")]
        [ExpectedException(typeof(ArgumentException))]
        public void ConstructorFailsIfRemotePathIsNull()
        {
            new MappedFolder(localRootPath, null);
        }

        [Test, Category("Fast"), Category("MappedObjects")]
        public void ConstructorWorksIfFsFactoryIsNull()
        {
            var rootFolder = new MappedFolder(localRootPath, remoteRootPath, null);
            Assert.IsNull(rootFolder.Parent);
            rootFolder = new MappedFolder(localRootPath, remoteRootPath);
            Assert.IsNull(rootFolder.Parent);
        }

        [Test, Category("Fast"), Category("MappedObjects")]
        public void ConstructorTakesFsFactory()
        {
            var factory = createFactoryWithLocalPathInfos();
            new MappedFolder(localRootPath, remoteRootPath, factory.Object);
        }

        [Test, Category("Fast"), Category("MappedObjects")]
        public void ConstructorTakesParentFolder ()
        {
            var factory = createFactoryWithLocalPathInfos();
            var rootFolder = new MappedFolder(localRootPath, remoteRootPath, factory.Object);
            string child = "child";
            var childFolder = new MappedFolder(rootFolder, child);
            Assert.AreEqual(rootFolder, childFolder.Parent);
            Assert.AreEqual(localRootPathName, rootFolder.Name);
            Assert.AreEqual(child, childFolder.Name);
        }

        [Test, Category("Fast"), Category("MappedObjects")]
        public void GetLocalPath()
        {
            var factory = createFactoryWithLocalPathInfos();
            var rootFolder = new MappedFolder(this.localRootPath, this.remoteRootPath, factory.Object);
            Assert.AreEqual(this.localRootPath, rootFolder.GetLocalPath());
            string child = "child";
            var childFolder = new MappedFolder(rootFolder, child);
            Assert.AreEqual(Path.Combine(this.localRootPath, child), childFolder.GetLocalPath());
            string sub = "sub";
            var subFolder = new MappedFolder(childFolder, sub);
            Assert.AreEqual(Path.Combine(this.localRootPath, child, sub), subFolder.GetLocalPath());
        }

        [Test, Category("Fast"), Category("MappedObjects")]
        public void ExistsLocally()
        {
            string childName = "child";
            var factory = createFactoryWithLocalPathInfos();
            var childInfo = new Mock<IDirectoryInfo>();
            childInfo.Setup(dir => dir.Name).Returns(childName);
            childInfo.Setup(dir => dir.Exists).Returns(false);
            factory.Setup(f => f.CreateDirectoryInfo(It.Is<string>(path => path == Path.Combine(localRootPath, childName)))).Returns(childInfo.Object);
            var rootFolder = new MappedFolder(this.localRootPath, this.remoteRootPath, factory.Object);
            var childFolder = new MappedFolder(rootFolder, childName);
            Assert.IsTrue(rootFolder.ExistsLocally());
            Assert.IsFalse(childFolder.ExistsLocally());
        }
    }

    [TestFixture]
    public class MappedFileTest
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
        [ExpectedException(typeof(NullReferenceException))]
        public void ConstructorFailsWithoutParent ()
        {
            new MappedFile(null);
        }

        [Test, Category("Fast"), Category("MappedObjects")]
        public void ConstructorTakesFsFactory()
        {
            var factory = createFactoryWithLocalPathInfos();
            var parent = new MappedFolder(this.localRootPath, this.remoteRootPath);
            new MappedFile(parent, factory.Object);
        }

        [Test, Category("Fast"), Category("MappedObjects")]
        public void ConstructorTest () {
            var factory = createFactoryWithLocalPathInfos();
            var parent = new MappedFolder(this.localRootPath, this.remoteRootPath, factory.Object);
            var file = new MappedFile(parent, factory.Object);
            Assert.AreEqual(parent, file.Parents[0]);
            file = new MappedFile(parent, null);
            Assert.AreEqual(parent, file.Parents[0]);
            Assert.AreEqual (1, file.Parents.Count);
        }

        [Test, Category("Fast"), Category("MappedObjects")]
        public void ConstructorWithMultipleParents()
        {
            var factory = createFactoryWithLocalPathInfos();
            var parent = new MappedFolder(this.localRootPath, this.remoteRootPath, factory.Object);
            var file = new MappedFile(parent, factory.Object);
            var secondParent = new MappedFolder(parent, "sub");
            file = new MappedFile(parent, null, secondParent);
            Assert.AreEqual(2, file.Parents.Count);
            Assert.IsTrue(file.Parents.Contains(parent));
            Assert.IsTrue(file.Parents.Contains(secondParent));
        }

        [Test, Category("Fast"), Category("MappedObjects")]
        public void ConstructorSetsDefaultParamsToNull()
        {
            var factory = createFactoryWithLocalPathInfos();
            var parent = new MappedFolder(this.localRootPath, this.remoteRootPath, factory.Object);
            var file = new MappedFile(parent, factory.Object);
            Assert.IsNull (file.ChecksumAlgorithmName);
            Assert.IsNull (file.Description);
            Assert.IsNull (file.LastChangeToken);
            Assert.IsNull (file.LastChecksum);
            Assert.IsNull (file.LastLocalWriteTimeUtc);
            Assert.IsNull (file.LastRemoteWriteTimeUtc);
            Assert.IsNull (file.Name);
            Assert.AreEqual(this.localRootPath, file.LocalSyncTargetPath);
            Assert.AreEqual(this.remoteRootPath, file.RemoteSyncTargetPath);
        }

        [Test, Category("Fast"), Category("MappedObjects")]
        public void HashAlgorithmProperty()
        {
            var factory = createFactoryWithLocalPathInfos();
            var parent = new MappedFolder(this.localRootPath, this.remoteRootPath, factory.Object);
            var file = new MappedFile(parent, factory.Object);

            string checksum = "SHA1";
            file.ChecksumAlgorithmName = checksum;
            Assert.AreEqual(checksum, file.ChecksumAlgorithmName);

        }

        [Test, Category("Fast"), Category("MappedObjects")]
        public void DescriptionProperty ()
        {
            var factory = createFactoryWithLocalPathInfos();
            var parent = new MappedFolder(this.localRootPath, this.remoteRootPath, factory.Object);
            var file = new MappedFile(parent, factory.Object);

            string desc = "desc";
            file.Description = desc;
            Assert.AreEqual(desc, file.Description);
        }

        [Test, Category("Fast"), Category("MappedObjects")]
        public void GetLocalPath () {
            var factory = createFactoryWithLocalPathInfos();
            var rootFolder = new MappedFolder(this.localRootPath, this.remoteRootPath, factory.Object);
            var subFolder = new MappedFolder(rootFolder, "sub");
            var file = new MappedFile(rootFolder, factory.Object);
            file.Name = localFileName;
            Assert.AreEqual(localFilePath, file.GetLocalPath());
            file.Parents.Add(subFolder);
            Assert.AreEqual(Path.Combine(this.localRootPath, localFileName), file.GetLocalPath(rootFolder));
            Assert.AreEqual(Path.Combine(this.localRootPath, subFolder.Name , localFileName), file.GetLocalPath(subFolder));
        }

        [Test, Category("Fast"), Category("MappedObjects")]
        public void ExistsLocally() {
            var factory = createFactoryWithLocalPathInfos();
            var rootFolder = new MappedFolder(this.localRootPath, this.remoteRootPath, factory.Object);
            var file = new MappedFile(rootFolder,factory.Object);
            file.Name = localFileName;
            Assert.IsTrue(file.ExistsLocally());
            var fileInfo = new Mock<IFileInfo>();
            fileInfo.Setup (f => f.Name).Returns(localFileName);
            fileInfo.Setup (f => f.Exists).Returns(false);
            factory.Setup(f => f.CreateFileInfo(It.Is<string>(path => path == localFilePath))).Returns(fileInfo.Object);
            Assert.IsFalse(file.ExistsLocally());
        }

        [Test, Category("Fast"), Category("MappedObjects")]
        public void HasBeenChangeRemotely ()
        {
            var factory = createFactoryWithLocalPathInfos();
            string id = "id";
            string changeToken = "changeToken";
            long filesize = 1024;
            DateTime remoteDate = DateTime.Now;
            DateTime date = remoteDate.ToUniversalTime();

            var rootFolder = new MappedFolder(this.localRootPath, this.remoteRootPath, factory.Object);
            var file = new MappedFile(rootFolder, factory.Object);
            file.RemoteObjectId = id;
            file.Name = localFileName;
            file.LastRemoteWriteTimeUtc = date;
            file.LastFileSize = filesize;
            var remoteDocument = new Mock<IDocument>();
            remoteDocument.Setup (r => r.Id).Returns(id);
            remoteDocument.Setup (r => r.ChangeToken).Returns((string)null);
            remoteDocument.Setup (r => r.ContentStreamLength).Returns(filesize);
            remoteDocument.Setup (r => r.LastModificationDate).Returns(remoteDate);
            remoteDocument.Setup (r => r.Name).Returns(localFileName);
            Assert.IsFalse(file.HasBeenChangeRemotely(remoteDocument.Object));
            file.Name = "changed";
            Assert.IsTrue(file.HasBeenChangeRemotely(remoteDocument.Object), file.Name.Equals(localFileName).ToString() + " " + localFileName);
            file.Name = localFileName;
            file.LastRemoteWriteTimeUtc = DateTime.UtcNow.AddDays(1);
            Assert.IsFalse(file.HasBeenChangeRemotely(remoteDocument.Object));
            remoteDocument.Setup(r => r.LastModificationDate).Returns(remoteDate.AddDays(2));
            Assert.IsTrue(file.HasBeenChangeRemotely(remoteDocument.Object));
            file.LastFileSize = 0;
            Assert.IsTrue(file.HasBeenChangeRemotely(remoteDocument.Object));
            file.LastFileSize = filesize;
            file.LastChangeToken = "oldToken";
            Assert.IsTrue(file.HasBeenChangeRemotely(remoteDocument.Object));
            file.LastChangeToken = changeToken;
            remoteDocument.Setup(r => r.LastModificationDate).Returns(remoteDate);
            remoteDocument.Setup (r => r.ChangeToken).Returns(changeToken);
            file.LastChangeToken = changeToken;
            Assert.IsFalse(file.HasBeenChangeRemotely(remoteDocument.Object));
            remoteDocument.Setup (r => r.Id).Returns("wrongID");
            try{
                file.HasBeenChangeRemotely(remoteDocument.Object);
                Assert.Fail();
            }catch(ArgumentException){}
        }

        [Ignore]
        [Test, Category("Fast"), Category("MappedObjects")]
        public void HasBeenChangedLocallyTest() {
            Assert.Fail ("TODO");
        }
    }
}

