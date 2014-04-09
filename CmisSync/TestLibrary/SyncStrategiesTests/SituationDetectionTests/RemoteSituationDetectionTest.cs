using System;
using System.IO;
using System.Collections.Generic;

using CmisSync.Lib.Sync.Strategy;
using CmisSync.Lib.Storage;
using CmisSync.Lib.Data;

using DotCMIS.Client;
using DotCMIS.Exceptions;

using NUnit.Framework;

using Moq;

using TestLibrary.TestUtils;
using CmisSync.Lib.Events;

namespace TestLibrary.SyncStrategiesTests.SituationDetectionTests
{
    [TestFixture]
    public class RemoteSituationDetectionTest
    {
        private Mock<ISession> SessionMock;
        private Mock<IMetaDataStorage> StorageMock;
        private string RemoteChangeToken = "changeToken";
        private readonly IObjectId ObjectId = Mock.Of<IObjectId>(ob => ob.Id == "objectId");
        private readonly string RemotePath = "/object/path";
        private readonly string RemoteName = "path";
        //private readonly string LocalPath = Path.Combine("object", "path");
        //private readonly string LocalName = "path";


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

        [Test, Category("Fast")]
        public void NoChangeDetectionForFileTest()
        {
            var lastModificationDate = DateTime.Now;
            var remoteObject = new Mock<IDocument>();
            var remotePaths = new List<string>();
            remotePaths.Add(RemotePath);
            remoteObject.Setup (remote => remote.ChangeToken).Returns(RemoteChangeToken);
            remoteObject.Setup (remote => remote.Id ).Returns(ObjectId.Id);
            remoteObject.Setup (remote => remote.LastModificationDate).Returns(lastModificationDate);
            remoteObject.Setup (remote => remote.Paths).Returns(remotePaths);
            SessionMock.Setup(s => s.GetObject(ObjectId)).Returns(remoteObject.Object);
            var file = Mock.Of<IMappedFile>( f =>
                                            f.LastRemoteWriteTimeUtc == lastModificationDate &&
                                            f.RemoteObjectId == ObjectId.Id &&
                                            f.GetLocalPath() == "path" &&
                                            f.LastChangeToken == RemoteChangeToken);
            StorageMock.AddMappedFile(file);
            var fileEvent = new FileEvent(remoteFile: remoteObject.Object);

            var detector = new RemoteSituationDetection(SessionMock.Object);

            Assert.AreEqual(SituationType.NOCHANGE, detector.Analyse(StorageMock.Object, fileEvent));
        }

        [Test, Category("Fast")]
        public void FileAddedDetectionTest()
        {
            var lastModificationDate = DateTime.Now;
            var remoteObject = new Mock<IDocument>();
            remoteObject.Setup(remote => remote.ChangeToken).Returns(RemoteChangeToken);
            remoteObject.Setup (remote => remote.Id ).Returns(ObjectId.Id);
            remoteObject.Setup (remote => remote.LastModificationDate).Returns(lastModificationDate);
            remoteObject.Setup (remote => remote.Name).Returns(RemoteName);
            IList<string> paths = new List<string>();
            paths.Add(this.RemotePath);
            remoteObject.Setup (remote => remote.Paths).Returns(paths);
            SessionMock.Setup(s => s.GetObject(ObjectId)).Returns(remoteObject.Object);
            StorageMock.Setup( storage => storage.GetObjectByLocalPath(It.IsAny<IFileSystemInfo>())).Returns((AbstractMappedObject)null);
            StorageMock.Setup( storage => storage.GetObjectByRemoteId(It.IsAny<string>())).Returns((AbstractMappedObject)null);
            var fileEvent = new FileEvent(remoteFile: remoteObject.Object);

            var detector = new RemoteSituationDetection(SessionMock.Object);

            Assert.AreEqual(SituationType.ADDED, detector.Analyse(StorageMock.Object, fileEvent));
        }

        [Test, Category("Fast")]
        public void FolderAddedDetectionTest()
        {
            var lastModificationDate = DateTime.Now;
            var remoteObject = new Mock<IFolder>();
            remoteObject.Setup(remote => remote.ChangeToken).Returns(RemoteChangeToken);
            remoteObject.Setup (remote => remote.Id ).Returns(ObjectId.Id);
            remoteObject.Setup (remote => remote.LastModificationDate).Returns(lastModificationDate);
            remoteObject.Setup (remote => remote.Name).Returns(RemoteName);
            IList<string> paths = new List<string>();
            paths.Add(RemotePath);
            remoteObject.Setup (remote => remote.Paths).Returns(paths);
            remoteObject.Setup (remote => remote.Path).Returns(RemotePath);
            SessionMock.Setup(s => s.GetObject(ObjectId)).Returns(remoteObject.Object);
            StorageMock.Setup( storage => storage.GetObjectByLocalPath(It.IsAny<IFileSystemInfo>())).Returns((AbstractMappedObject)null);
            StorageMock.Setup( storage => storage.GetObjectByRemoteId(It.IsAny<string>())).Returns((AbstractMappedObject)null);
            var folderEvent = new FolderEvent(remoteFolder: remoteObject.Object);

            var detector = new RemoteSituationDetection(SessionMock.Object);

            Assert.AreEqual(SituationType.ADDED, detector.Analyse(StorageMock.Object, folderEvent));
        }

        // Not yet implemented by detector
        [Ignore]
        [Test, Category("Fast")]
        public void FileChangedDetectionTest()
        {
            var lastModificationDate = DateTime.Now;
            var actualModificationDate = DateTime.Now.AddDays(1);
            var remoteObject = new Mock<IDocument>();
            var newChangeToken = RemoteChangeToken + "changed";
            remoteObject.Setup (remote => remote.ChangeToken).Returns(newChangeToken);
            remoteObject.Setup (remote => remote.Id ).Returns(ObjectId.Id);
            remoteObject.Setup (remote => remote.LastModificationDate).Returns(actualModificationDate);
            remoteObject.Setup (remote => remote.Name).Returns(RemoteName);
            IList<string> paths = new List<string>();
            paths.Add(RemotePath);
            remoteObject.Setup (remote => remote.Paths).Returns(paths);
            SessionMock.Setup(s => s.GetObject(ObjectId)).Returns(remoteObject.Object);
            var file = Mock.Of<IMappedFile>( f =>
                                           f.LastRemoteWriteTimeUtc == lastModificationDate &&
                                           f.RemoteObjectId == ObjectId.Id);
            StorageMock.AddMappedFile(file);
            var fileEvent = new FileEvent(remoteFile: remoteObject.Object);

            var detector = new RemoteSituationDetection(SessionMock.Object);

            Assert.AreEqual(SituationType.CHANGED, detector.Analyse(StorageMock.Object, fileEvent));
        }

        // Not yet implemented by detector
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
            remoteObject.Setup (remote => remote.Id ).Returns(ObjectId.Id);
            remoteObject.Setup (remote => remote.LastModificationDate).Returns(actualModificationDate);
            remoteObject.Setup (remote => remote.Name).Returns(newName);
            IList<string> paths = new List<string>();
            paths.Add(newPath);
            remoteObject.Setup (remote => remote.Paths).Returns(paths);
            SessionMock.Setup(s => s.GetObject(ObjectId)).Returns(remoteObject.Object);
            var file = Mock.Of<IMappedFile>( f =>
                                           f.RemoteSyncTargetPath == RemotePath &&
                                           f.LastRemoteWriteTimeUtc == lastModificationDate &&
                                           f.RemoteObjectId == ObjectId.Id);
            StorageMock.AddMappedFile(file);
            var fileEvent = new FileEvent(remoteFile: remoteObject.Object);

            var detector = new RemoteSituationDetection(SessionMock.Object);

            Assert.AreEqual(SituationType.CHANGED, detector.Analyse(StorageMock.Object, fileEvent));
        }

        // Not yet implemented by detector
        [Ignore]
        [Test, Category("Fast")]
        public void FolderRenamedDetectionTest()
        {
            var lastModificationDate = DateTime.Now;
            var actualModificationDate = DateTime.Now.AddDays(1);
            var newPath = RemotePath + "renamed";
            var newName = RemoteName + "renamed";
            var newChangeToken = RemoteChangeToken + "renamed";
            var remoteFolder = Mock.Of<IFolder>( folder =>
                             folder.ChangeToken == newChangeToken &&
                             folder.Name == newName &&
                             folder.Path == newPath &&
                             folder.Id == ObjectId.Id &&
                             folder.LastModificationDate == actualModificationDate);
            SessionMock.Setup(s => s.GetObject(ObjectId.Id)).Returns(remoteFolder);

            var dir = Mock.Of<MappedFolder>( f =>
                                            f.Name == RemoteName &&
                                            f.RemoteSyncTargetPath == RemotePath &&
                                            f.RemoteObjectId == ObjectId.Id &&
                                            f.LastRemoteWriteTimeUtc == lastModificationDate);
            StorageMock.AddMappedFolder(dir);
            var folderEvent = new FolderEvent(remoteFolder: remoteFolder);

            var detector = new RemoteSituationDetection(SessionMock.Object);

            Assert.AreEqual(SituationType.RENAMED, detector.Analyse(StorageMock.Object, folderEvent));
        }

        // Not yet implemented by detector
        [Ignore]
        [Test, Category("Fast")]
        public void FileMovedDetectionTest()
        {
            var lastModificationDate = DateTime.Now;
            var remoteObject = new Mock<IDocument>();
            remoteObject.Setup (remote => remote.ChangeToken).Returns(RemoteChangeToken);
            remoteObject.Setup (remote => remote.Id ).Returns(ObjectId.Id);
            remoteObject.Setup (remote => remote.LastModificationDate).Returns(lastModificationDate);
            remoteObject.Setup (remote => remote.Name).Returns(RemoteName);
            var newPath = "/new" + RemotePath;
            IList<string> paths = new List<string>();
            paths.Add(newPath);
            remoteObject.Setup (remote => remote.Paths).Returns(paths);
            SessionMock.Setup(s => s.GetObject(ObjectId)).Returns(remoteObject.Object);
            var file = Mock.Of<IMappedFile>( f =>
                                           f.Name == RemoteName &&
                                           f.RemoteSyncTargetPath == RemotePath &&
                                           f.LastRemoteWriteTimeUtc == lastModificationDate &&
                                           f.RemoteObjectId == ObjectId.Id);
            StorageMock.AddMappedFile(file);
            var fileEvent = new FileEvent(remoteFile: remoteObject.Object);

            var detector = new RemoteSituationDetection(SessionMock.Object);

            Assert.AreEqual(SituationType.MOVED, detector.Analyse(StorageMock.Object, fileEvent));
        }

        // Not yet implemented by detector
        [Ignore]
        [Test, Category("Fast")]
        public void FolderMovedDetectionTest()
        {
            var lastModificationDate = DateTime.Now;
            var newPath = "/new" + RemotePath;
            var remoteFolder = Mock.Of<IFolder>( folder =>
                                                folder.ChangeToken == RemoteChangeToken &&
                                                folder.Name == RemoteName &&
                                                folder.Path == newPath &&
                                                folder.Id == ObjectId.Id &&
                                                folder.LastModificationDate == lastModificationDate);
            SessionMock.Setup(s => s.GetObject(ObjectId)).Returns(remoteFolder);
            var localFolder = Mock.Of<MappedFolder>( folder =>
                                                    folder.Name == RemoteName &&
                                                    folder.RemoteObjectId == ObjectId.Id &&
                                                    folder.LastRemoteWriteTimeUtc == lastModificationDate);
            StorageMock.AddMappedFolder(localFolder);
            var folderEvent = new FolderEvent(remoteFolder: remoteFolder);

            var detector = new RemoteSituationDetection(SessionMock.Object);

            Assert.AreEqual(SituationType.MOVED, detector.Analyse(StorageMock.Object, folderEvent));
        }

        [Test, Category("Fast")]
        public void FileRemovedDetectionTest()
        {
            var lastModificationDate = DateTime.Now;
            SessionMock.Setup(s => s.GetObject(ObjectId)).Throws(new CmisObjectNotFoundException());
            var file = Mock.Of<IMappedFile>( f =>
                                           f.RemoteObjectId == ObjectId.Id &&
                                           f.GetLocalPath() == "path" &&
                                           f.LastRemoteWriteTimeUtc == lastModificationDate);
            StorageMock.AddMappedFile(file);
            var fileInfo = Mock.Of<IFileInfo>( f =>
                                              f.FullName == "path" &&
                                              f.LastWriteTimeUtc == lastModificationDate);
            var fileEvent = new FileEvent(localFile: fileInfo);

            var detector = new RemoteSituationDetection(SessionMock.Object);

            Assert.AreEqual(SituationType.REMOVED, detector.Analyse(StorageMock.Object, fileEvent));
        }

        [Test, Category("Fast")]
        public void FolderRemovedDetectionTest()
        {
            var lastModificationDate = DateTime.Now;
            SessionMock.Setup(s => s.GetObject(ObjectId)).Throws(new CmisObjectNotFoundException());

            var folder = Mock.Of<IMappedFolder>( f =>
                                               f.RemoteObjectId == ObjectId.Id &&
                                               f.LastRemoteWriteTimeUtc == lastModificationDate &&
                                               f.Name == RemoteName &&
                                               f.GetRemotePath() == RemotePath &&
                                                f.GetLocalPath() == "path");
            StorageMock.AddMappedFolder(folder);
            var folderEvent = new FolderEvent(localFolder: Mock.Of<IDirectoryInfo>( d =>
                                                                                   d.FullName == "path" &&
                                                                                   d.Exists == true
                ));

            var detector = new RemoteSituationDetection(SessionMock.Object);

            Assert.AreEqual(SituationType.REMOVED, detector.Analyse(StorageMock.Object, folderEvent));
        }
    }
}

