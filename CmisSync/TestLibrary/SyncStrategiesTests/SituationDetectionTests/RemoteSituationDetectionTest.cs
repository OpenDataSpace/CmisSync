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
            var remoteObject = new Mock<IDocument>();

            var fileEvent = new FileEvent(remoteFile: remoteObject.Object);
            fileEvent.Remote = MetaDataChangeType.CREATED;

            var detector = new RemoteSituationDetection(SessionMock.Object);

            Assert.AreEqual(SituationType.ADDED, detector.Analyse(StorageMock.Object, fileEvent));
        }

        [Test, Category("Fast")]
        public void FolderAddedDetectionTest()
        {
            var remoteObject = new Mock<IFolder>();
            var folderEvent = new FolderEvent(remoteFolder: remoteObject.Object);
            folderEvent.Remote = MetaDataChangeType.CREATED;

            var detector = new RemoteSituationDetection(SessionMock.Object);

            Assert.AreEqual(SituationType.ADDED, detector.Analyse(StorageMock.Object, folderEvent));
        }


        [Test, Category("Fast")]
        public void FileRemovedDetectionTest()
        {
            var remoteObject = new Mock<IDocument>();

            var fileEvent = new FileEvent(remoteFile: remoteObject.Object);
            fileEvent.Remote = MetaDataChangeType.DELETED;

            var detector = new RemoteSituationDetection(SessionMock.Object);

            Assert.AreEqual(SituationType.REMOVED, detector.Analyse(StorageMock.Object, fileEvent));
        }

        [Test, Category("Fast")]
        public void FolderRemovedDetectionTest()
        {
            var remoteObject = new Mock<IFolder>();
            var folderEvent = new FolderEvent(remoteFolder: remoteObject.Object);
            folderEvent.Remote = MetaDataChangeType.DELETED;

            var detector = new RemoteSituationDetection(SessionMock.Object);

            Assert.AreEqual(SituationType.REMOVED, detector.Analyse(StorageMock.Object, folderEvent));
        }
    }
}

