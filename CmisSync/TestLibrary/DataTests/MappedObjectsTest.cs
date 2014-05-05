using System;
using System.IO;

using CmisSync.Lib.Data;

using DotCMIS.Client;

using NUnit.Framework;

using Moq;

namespace TestLibrary.DataTests
{
    [TestFixture]
    public class MappedFolderTest
    {
        private string localRootPath;
        private string remoteRootPath;

        [SetUp]
        public void SetUp() {
            this.localRootPath = Path.GetTempPath();
            this.remoteRootPath = "/";
        }

        [Test, Category("Fast")]
        public void ConstructorTest () {
            var rootFolder = new MappedFolder(this.localRootPath, this.remoteRootPath);
            Assert.IsNull(rootFolder.Parent);
            string child = "child";
            var childFolder = new MappedFolder(rootFolder, child);
            Assert.AreEqual(rootFolder, childFolder.Parent);
            Assert.AreEqual(new DirectoryInfo(this.localRootPath).Name, rootFolder.Name);
            Assert.AreEqual(child, childFolder.Name);
        }

        [Test, Category("Fast")]
        public void GetLocalPathTest() {
            var rootFolder = new MappedFolder(this.localRootPath, this.remoteRootPath);
            Assert.AreEqual(this.localRootPath, rootFolder.GetLocalPath());
            string child = "child";
            var childFolder = new MappedFolder(rootFolder, child);
            Assert.AreEqual(Path.Combine(this.localRootPath, child), childFolder.GetLocalPath());
            string sub = "sub";
            var subFolder = new MappedFolder(childFolder, sub);
            Assert.AreEqual(Path.Combine(this.localRootPath, child, sub), subFolder.GetLocalPath());
        }

        [Test, Category("Fast")]
        public void ExistsLocallyTest() {
            var rootFolder = new MappedFolder(this.localRootPath, this.remoteRootPath);
            string child = "child";
            var childFolder = new MappedFolder(rootFolder, child);
            Assert.IsTrue(rootFolder.ExistsLocally());
            Assert.IsFalse(childFolder.ExistsLocally());
        }
    }

    [TestFixture]
    public class MappedFileTest
    {
        private string localRootPath;
        private string remoteRootPath;

        [SetUp]
        public void SetUp() {
            this.localRootPath = Path.GetTempPath();
            this.remoteRootPath = "/";
        }

        [Test, Category("Fast")]
        public void ConstructorTest () {
            var parent = new MappedFolder(this.localRootPath, this.remoteRootPath);
            var file = new MappedFile(parent);
            Assert.AreEqual(parent, file.Parents[0]);
            file = new MappedFile(parent, null);
            Assert.AreEqual(parent, file.Parents[0]);
            Assert.AreEqual (1, file.Parents.Count);
            var secondParent = new MappedFolder(parent, "sub");
            file = new MappedFile(parent, secondParent);
            Assert.AreEqual(2, file.Parents.Count);
            Assert.IsTrue(file.Parents.Contains(parent));
            Assert.IsTrue(file.Parents.Contains(secondParent));
            Assert.IsNull(file.ChecksumAlgorithmName);
            string checksum = "SHA1";
            file.ChecksumAlgorithmName = checksum;
            Assert.AreEqual(checksum, file.ChecksumAlgorithmName);
            Assert.IsNull(file.Description);
            string desc = "desc";
            file.Description = desc;
            Assert.AreEqual(desc, file.Description);
            Assert.IsNull(file.LastChangeToken);
            Assert.IsNull(file.LastChecksum);
            Assert.IsNull (file.LastLocalWriteTimeUtc);
            Assert.IsNull (file.LastRemoteWriteTimeUtc);
            Assert.IsNull (file.Name);
            Assert.AreEqual(this.localRootPath, file.LocalSyncTargetPath);
            Assert.AreEqual(this.remoteRootPath, file.RemoteSyncTargetPath);
            try{
                new MappedFile(null);
                Assert.Fail ();
            }catch(Exception){}
        }

        [Test, Category("Fast")]
        public void GetLocalPathTest () {
            var rootFolder = new MappedFolder(this.localRootPath, this.remoteRootPath);
            var subFolder = new MappedFolder(rootFolder, "sub");
            var file = new MappedFile(rootFolder);
            string filename = "testfile";
            file.Name = filename;
            Assert.AreEqual(Path.Combine(this.localRootPath, filename), file.GetLocalPath());
            file.Parents.Add(subFolder);
            Assert.AreEqual(Path.Combine(this.localRootPath, filename), file.GetLocalPath(rootFolder));
            Assert.AreEqual(Path.Combine(this.localRootPath, subFolder.Name ,filename), file.GetLocalPath(subFolder));
        }

        [Test, Category("Fast")]
        public void ExistsLocallyTest() {
            var rootFolder = new MappedFolder(this.localRootPath, this.remoteRootPath);
            var file = new MappedFile(rootFolder);
            string filename = Path.GetRandomFileName();
            file.Name = filename;
            Assert.IsFalse(file.ExistsLocally());
            using (File.Create(file.GetLocalPath()));
            Assert.IsTrue(file.ExistsLocally());
            File.Delete(file.GetLocalPath());
            Assert.IsFalse(file.ExistsLocally());
        }

        [Test, Category("Fast")]
        public void HasBeenChangeRemotelyTest () {
            string id = "id";
            string changeToken = "changeToken";
            long filesize = 1024;
            DateTime remoteDate = DateTime.Now;
            DateTime date = remoteDate.ToUniversalTime();

            var rootFolder = new MappedFolder(this.localRootPath, this.remoteRootPath);
            var file = new MappedFile(rootFolder);
            string filename = Path.GetRandomFileName();
            file.RemoteObjectId = id;
            file.Name = filename;
            file.LastRemoteWriteTimeUtc = date;
            file.LastFileSize = filesize;
            var remoteDocument = new Mock<IDocument>();
            remoteDocument.Setup (r => r.Id).Returns(id);
            remoteDocument.Setup (r => r.ChangeToken).Returns((string)null);
            remoteDocument.Setup (r => r.ContentStreamLength).Returns(filesize);
            remoteDocument.Setup (r => r.LastModificationDate).Returns(remoteDate);
            remoteDocument.Setup (r => r.Name).Returns(filename);
            Assert.IsFalse(file.HasBeenChangeRemotely(remoteDocument.Object));
            file.Name = "changed";
            Assert.IsTrue(file.HasBeenChangeRemotely(remoteDocument.Object), file.Name.Equals(filename).ToString() + " " + filename);
            file.Name = filename;
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
        [Test, Category("Fast")]
        public void HasBeenChangedLocallyTest() {
            Assert.Fail ("TODO");
        }
    }
}

