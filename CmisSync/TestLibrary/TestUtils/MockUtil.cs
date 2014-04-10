using System;
using System.Collections.Generic;

using CmisSync.Lib;
using CmisSync.Lib.Events;
using CmisSync.Lib.Sync.Strategy;
using CmisSync.Lib.Data;

using DotCMIS.Client;
using DotCMIS.Data;
using DotCMIS.Data.Extensions;
using DotCMIS.Binding.Services;
using CmisSync.Lib.Storage;
using Moq;

namespace TestLibrary.TestUtils
{
    public static class MockUtil {
        public static void SetupSessionDefaultValues(this Mock<ISession> session) {
            session.Setup (s => s.Binding.GetRepositoryService ().GetRepositoryInfos (null)).Returns ((IList<IRepositoryInfo>)null);
            session.Setup (s => s.RepositoryInfo.Id).Returns ("repoId");
        }

        public static void SetupChangeLogToken(this Mock<ISession> session, string changeLogToken){
            session.Setup (s => s.Binding.GetRepositoryService ().GetRepositoryInfo (It.IsAny<string>(), null).LatestChangeLogToken).Returns (changeLogToken);
        }


        public static Mock<IMetaDataStorage> GetMetaStorageMockWithToken(string token = "lastToken"){
            var storage = new Mock<IMetaDataStorage>();
            storage.Setup (db => db.ChangeLogToken ).Returns (token);
            return storage;
        }
        
        public static Mock<IChangeEvent> GenerateChangeEvent(DotCMIS.Enums.ChangeType type, string objectId) {
            var changeEvent = new Mock<IChangeEvent> ();
            changeEvent.Setup (ce => ce.ObjectId).Returns (objectId);
            changeEvent.Setup (ce => ce.ChangeType).Returns (type);

            return changeEvent;
        }

        public static Mock<IDocument> CreateRemoteObjectMock(string documentContentStreamId, string id){
            var newRemoteObject = new Mock<IDocument> ();
            newRemoteObject.Setup(d => d.ContentStreamId).Returns(documentContentStreamId);
            newRemoteObject.Setup(d => d.ContentStreamLength).Returns(documentContentStreamId==null? 0 : 1);
            newRemoteObject.Setup(d => d.Id).Returns(id);
            return newRemoteObject;
        }
        
        public static Mock<IFolder> CreateRemoteFolderMock(string id){
            var newRemoteObject = new Mock<IFolder> ();
            newRemoteObject.Setup(d => d.Id).Returns(id);
            return newRemoteObject;
        }

        public static Mock<ISession> PrepareSessionMockForSingleChange(DotCMIS.Enums.ChangeType type, string objectId = "objectId", string changeLogToken = "token", string latestChangeLogToken = "latestChangeLogToken") {
            var changeEvents = new Mock<IChangeEvents> ();
            var changeList = GenerateSingleChangeListMock(type, objectId); 
            changeEvents.Setup (ce => ce.HasMoreItems).Returns ((bool?) false);
            changeEvents.Setup (ce => ce.LatestChangeLogToken).Returns (latestChangeLogToken);
            changeEvents.Setup (ce => ce.TotalNumItems).Returns (1);
            changeEvents.Setup (ce => ce.ChangeEventList).Returns (changeList);

            var session = new Mock<ISession> ();
            session.SetupSessionDefaultValues();
            session.SetupChangeLogToken(changeLogToken);
            session.Setup (s => s.GetContentChanges (It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<long>())).Returns (changeEvents.Object);
            return session;

        }

        private static List<IChangeEvent> GenerateSingleChangeListMock (DotCMIS.Enums.ChangeType type, string objectId = "objId") {
            var changeList = new List<IChangeEvent> ();
            changeList.Add (MockUtil.GenerateChangeEvent(type, objectId).Object);
            return changeList;
        }

        public static void AddLocalFile(this Mock<IMetaDataStorage> db, string path, string id) {
            var file = Mock.Of<IFileInfo>(f =>
                                          f.FullName == path &&
                                          f.Exists == true);
            db.AddLocalFile(file, id);
        }

        public static void AddLocalFile(this Mock<IMetaDataStorage> db, IFileInfo path, string id ) {
            var file = Mock.Of<IMappedFile>( f =>
                                            f.RemoteObjectId == id &&
                                            f.GetLocalPath() == path.FullName &&
                                            f.ExistsLocally() == path.Exists);
            db.AddMappedFile(file);
        }

        public static Mock<MappedFolder> AddLocalFolder( this Mock<IMetaDataStorage> db, string path, string id) {
            var folder = Mock.Of<IDirectoryInfo>(d => d.FullName == path);
            return db.AddLocalFolder(folder, id);
        }

        public static Mock<MappedFolder> AddLocalFolder(this Mock<IMetaDataStorage> db, IDirectoryInfo path, string id ) {
            var folder = new Mock<MappedFolder>("path","/", null) { CallBase = true};
            folder.Setup(f => f.GetLocalPath()).Returns(path.FullName);
            folder.Setup (f => f.RemoteObjectId).Returns(id);
            folder.Setup (f => f.Remove());
            db.AddMappedFolder(folder.Object);
            return folder;
        }

        public static void AddMappedFile(this Mock<IMetaDataStorage> db, IMappedFile file) {
            db.Setup( foo => foo.GetObjectByLocalPath(It.Is<IFileInfo>(s => s.FullName == file.GetLocalPath()))).Returns(file);
            db.Setup( foo => foo.GetObjectByRemoteId(It.Is<string>(s => s == file.RemoteObjectId))).Returns(file);
        }

        public static void AddMappedFolder(this Mock<IMetaDataStorage> db, IMappedFolder folder) {
            db.Setup( foo => foo.GetObjectByLocalPath(It.Is<IDirectoryInfo>(s => s.FullName == folder.GetLocalPath()))).Returns(folder);
            db.Setup( foo => foo.GetObjectByRemoteId(It.Is<string>(s => s == folder.RemoteObjectId))).Returns(folder);
        }

        public static void AddRemoteObject(this Mock<ISession> session, ICmisObject remoteObject) {
            session.Setup( s => s.GetObject(It.Is<string>(id => id == remoteObject.Id))).Returns(remoteObject);
            HashSet<string> paths = new HashSet<string>();
            if(remoteObject is IFolder)
            {
                paths.Add((remoteObject as IFolder).Path);
                if((remoteObject as IFolder).Paths != null)
                {
                    foreach(string path in (remoteObject as IFolder).Paths)
                    {
                        paths.Add(path);
                    }
                }
            }
            else if (remoteObject is IDocument)
            {
                foreach(string path in (remoteObject as IDocument).Paths)
                {
                    paths.Add(path);
                }
            }
            foreach(string path in paths)
            {
                session.Setup( s => s.GetObjectByPath(It.Is<string>(p => p == path))).Returns(remoteObject);
            }
        }

        public static Mock<IFolder> CreateCmisFolder(List<string> fileNames = null, List<string> folderNames = null, bool contentStream = false) {
            var remoteFolder = new Mock<IFolder>();
            var remoteChildren = new Mock<IItemEnumerable<ICmisObject>>();
            var list = new List<ICmisObject>();
            if(fileNames != null) {
                foreach(var name in fileNames) {
                    var doc = new Mock<IDocument>();
                    doc.Setup(d => d.Name).Returns(name);
                    if(contentStream){
                        doc.Setup(d => d.ContentStreamId).Returns(name);
                    }
                    list.Add(doc.Object);
                }
            }
            if(folderNames != null){
                foreach(var name in folderNames) {
                    var folder = new Mock<IFolder>();
                    folder.Setup(d => d.Name).Returns(name);
                    list.Add(folder.Object);
                }
            }
            remoteChildren.Setup(r => r.GetEnumerator()).Returns(list.GetEnumerator());
            remoteFolder.Setup(r => r.GetChildren()).Returns(remoteChildren.Object);
            return remoteFolder;
        }

        public static Mock<ISession> GetSessionMockReturningFolderChange(DotCMIS.Enums.ChangeType type, string id = "folderid") {
            var session = MockUtil.PrepareSessionMockForSingleChange(type, id);
            var newRemoteObject =  CreateRemoteFolderMock(id);
            session.Setup (s => s.GetObject (It.IsAny<string>())).Returns (newRemoteObject.Object);
         
            return session;
        }

    }

}
