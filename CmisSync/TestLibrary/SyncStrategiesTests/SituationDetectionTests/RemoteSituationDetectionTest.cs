using System;
using System.Collections.Generic;

using CmisSync.Lib.Sync.Strategy;
using CmisSync.Lib.Storage;

using DotCMIS.Client;
using DotCMIS.Exceptions;

using NUnit.Framework;

using Moq;

namespace TestLibrary.SyncStrategiesTests.SituationDetectionTests
{
    [TestFixture]
    public class RemoteSituationDetectionTest
    {
        private Mock<ISession> SessionMock;
        private Mock<IMetaDataStorage> StorageMock;
        private string RemoteChangeToken = "changeToken";
        private string ObjectId = "objectId";
        private string RemotePath = "/object/path";
        private string RemoteName = "path";

        [SetUp]
        public void SetUp() {
            this.SessionMock = new Mock<ISession>();
            this.StorageMock = new Mock<IMetaDataStorage>();
        }

        [Test, Category("Fast")]
        public void ConstructorFailsOnNullSessionTest() {
            try{
                new RemoteSituationDetection(null);
                Assert.Fail();
            }catch (ArgumentNullException) {}
        }

        [Test, Category("Fast")]
        public void ConstructorWithSessionTest() {
            new RemoteSituationDetection(SessionMock.Object);
        }

        [Ignore]
        [Test, Category("Fast")]
        public void NoChangeDetectionTest()
        {
            var lastModificationDate = DateTime.Now;
            var remoteObject = new Mock<ICmisObject>();
            remoteObject.Setup(remote => remote.ChangeToken).Returns(RemoteChangeToken);
            remoteObject.Setup (remote => remote.Id ).Returns(ObjectId);
            remoteObject.Setup (remote => remote.LastModificationDate).Returns(lastModificationDate);
            SessionMock.Setup(s => s.GetObject(ObjectId)).Returns(remoteObject.Object);
            StorageMock.Setup(storage => storage.GetServerSideModificationDate(RemotePath)).Returns(lastModificationDate);
            var detector = new RemoteSituationDetection(SessionMock.Object);
            Assert.AreEqual(SituationType.NOCHANGE, detector.Analyse(StorageMock.Object, ObjectId));
        }

        [Test, Category("Fast")]
        public void FileAddedDetectionTest()
        {
            var lastModificationDate = DateTime.Now;
            var remoteObject = new Mock<IDocument>();
            remoteObject.Setup(remote => remote.ChangeToken).Returns(RemoteChangeToken);
            remoteObject.Setup (remote => remote.Id ).Returns(ObjectId);
            remoteObject.Setup (remote => remote.LastModificationDate).Returns(lastModificationDate);
            remoteObject.Setup (remote => remote.Name).Returns(RemoteName);
            IList<string> paths = new List<string>();
            paths.Add(this.RemotePath);
            remoteObject.Setup (remote => remote.Paths).Returns(paths);
            SessionMock.Setup(s => s.GetObject(ObjectId)).Returns(remoteObject.Object);
            StorageMock.Setup(storage => storage.GetServerSideModificationDate(RemotePath)).Returns((DateTime?)null);
            StorageMock.Setup(storage => storage.GetFilePath(ObjectId)).Returns((string) null);
            var detector = new RemoteSituationDetection(SessionMock.Object);
            Assert.AreEqual(SituationType.ADDED, detector.Analyse(StorageMock.Object, ObjectId));
        }

        [Test, Category("Fast")]
        public void FolderAddedDetectionTest()
        {
            var lastModificationDate = DateTime.Now;
            var remoteObject = new Mock<IFolder>();
            remoteObject.Setup(remote => remote.ChangeToken).Returns(RemoteChangeToken);
            remoteObject.Setup (remote => remote.Id ).Returns(ObjectId);
            remoteObject.Setup (remote => remote.LastModificationDate).Returns(lastModificationDate);
            remoteObject.Setup (remote => remote.Name).Returns(RemoteName);
            IList<string> paths = new List<string>();
            paths.Add(RemotePath);
            remoteObject.Setup (remote => remote.Paths).Returns(paths);
            remoteObject.Setup (remote => remote.Path).Returns(RemotePath);
            SessionMock.Setup(s => s.GetObject(ObjectId)).Returns(remoteObject.Object);
            StorageMock.Setup(storage => storage.GetServerSideModificationDate(RemotePath)).Returns((DateTime?)null);
            StorageMock.Setup(storage => storage.GetFolderPath(ObjectId)).Returns((string) null);
            var detector = new RemoteSituationDetection(SessionMock.Object);
            Assert.AreEqual(SituationType.ADDED, detector.Analyse(StorageMock.Object, ObjectId));
        }

        [Ignore]
        [Test, Category("Fast")]
        public void FileChangedDetectionTest()
        {
            var lastModificationDate = DateTime.Now;
            var actualModificationDate = DateTime.Now.AddDays(1);
            var remoteObject = new Mock<IDocument>();
            var newChangeToken = RemoteChangeToken + "changed";
            remoteObject.Setup (remote => remote.ChangeToken).Returns(newChangeToken);
            remoteObject.Setup (remote => remote.Id ).Returns(ObjectId);
            remoteObject.Setup (remote => remote.LastModificationDate).Returns(actualModificationDate);
            remoteObject.Setup (remote => remote.Name).Returns(RemoteName);
            IList<string> paths = new List<string>();
            paths.Add(RemotePath);
            remoteObject.Setup (remote => remote.Paths).Returns(paths);
            SessionMock.Setup(s => s.GetObject(ObjectId)).Returns(remoteObject.Object);

            StorageMock.Setup(storage => storage.GetServerSideModificationDate(RemotePath)).Returns(lastModificationDate);
            StorageMock.Setup(storage => storage.GetFilePath(ObjectId)).Returns(RemotePath);

            var detector = new RemoteSituationDetection(SessionMock.Object);
            Assert.AreEqual(SituationType.CHANGED, detector.Analyse(StorageMock.Object, ObjectId));
        }

        [Ignore]
        [Test, Category("Fast")]
        public void FileRenamedDetectionTest()
        {
            var lastModificationDate = DateTime.Now;
            var actualModificationDate = DateTime.Now.AddDays(1);
            var remoteObject = new Mock<IDocument>();
            var newName = RemoteName + "renamed";
            var newPath = RemotePath + "renamed";
            var newChangeToken = RemoteChangeToken + "renamed";
            remoteObject.Setup (remote => remote.ChangeToken).Returns(newChangeToken);
            remoteObject.Setup (remote => remote.Id ).Returns(ObjectId);
            remoteObject.Setup (remote => remote.LastModificationDate).Returns(actualModificationDate);
            remoteObject.Setup (remote => remote.Name).Returns(newName);
            IList<string> paths = new List<string>();
            paths.Add(newPath);
            remoteObject.Setup (remote => remote.Paths).Returns(paths);
            SessionMock.Setup(s => s.GetObject(ObjectId)).Returns(remoteObject.Object);

            StorageMock.Setup(storage => storage.GetServerSideModificationDate(RemotePath)).Returns(lastModificationDate);
            StorageMock.Setup(storage => storage.GetFilePath(ObjectId)).Returns(RemotePath);

            var detector = new RemoteSituationDetection(SessionMock.Object);
            Assert.AreEqual(SituationType.CHANGED, detector.Analyse(StorageMock.Object, ObjectId));
        }

        [Ignore]
        [Test, Category("Fast")]
        public void FolderRenamedDetectionTest()
        {
            Assert.Fail ();
        }

        [Ignore]
        [Test, Category("Fast")]
        public void FileMovedDetectionTest()
        {
            var lastModificationDate = DateTime.Now;
            var remoteObject = new Mock<IDocument>();
            remoteObject.Setup (remote => remote.ChangeToken).Returns(RemoteChangeToken);
            remoteObject.Setup (remote => remote.Id ).Returns(ObjectId);
            remoteObject.Setup (remote => remote.LastModificationDate).Returns(lastModificationDate);
            remoteObject.Setup (remote => remote.Name).Returns(RemoteName);
            var newPath = "/new" + RemotePath;
            IList<string> paths = new List<string>();
            paths.Add(newPath);
            remoteObject.Setup (remote => remote.Paths).Returns(paths);
            SessionMock.Setup(s => s.GetObject(ObjectId)).Returns(remoteObject.Object);

            StorageMock.Setup(storage => storage.GetServerSideModificationDate(RemotePath)).Returns(lastModificationDate);
            StorageMock.Setup(storage => storage.GetFilePath(ObjectId)).Returns(RemotePath);

            var detector = new RemoteSituationDetection(SessionMock.Object);
            Assert.AreEqual(SituationType.MOVED, detector.Analyse(StorageMock.Object, ObjectId));
        }

        [Ignore]
        [Test, Category("Fast")]
        public void FolderMovedDetectionTest()
        {
            Assert.Fail ();
        }

        [Test, Category("Fast")]
        public void FileRemovedDetectionTest()
        {
            var lastModificationDate = DateTime.Now;
            SessionMock.Setup(s => s.GetObject(ObjectId)).Throws(new CmisObjectNotFoundException());

            StorageMock.Setup(storage => storage.GetServerSideModificationDate(RemotePath)).Returns(lastModificationDate);
            StorageMock.Setup(storage => storage.GetFilePath(ObjectId)).Returns(RemotePath);

            var detector = new RemoteSituationDetection(SessionMock.Object);
            Assert.AreEqual(SituationType.REMOVED, detector.Analyse(StorageMock.Object, ObjectId));
        }

        [Test, Category("Fast")]
        public void FolderRemovedDetectionTest()
        {
            var lastModificationDate = DateTime.Now;
            SessionMock.Setup(s => s.GetObject(ObjectId)).Throws(new CmisObjectNotFoundException());

            StorageMock.Setup(storage => storage.GetServerSideModificationDate(RemotePath)).Returns(lastModificationDate);
            StorageMock.Setup(storage => storage.GetFolderPath(ObjectId)).Returns(RemotePath);

            var detector = new RemoteSituationDetection(SessionMock.Object);
            Assert.AreEqual(SituationType.REMOVED, detector.Analyse(StorageMock.Object, ObjectId));
        }
    }
}
