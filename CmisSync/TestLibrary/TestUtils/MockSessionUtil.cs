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
    public static class MockSessionUtil {
        public static void SetupSessionDefaultValues(this Mock<ISession> session) {
            session.Setup (s => s.Binding.GetRepositoryService ().GetRepositoryInfos (null)).Returns ((IList<IRepositoryInfo>)null);
            session.Setup (s => s.RepositoryInfo.Id).Returns ("repoId");
        }

        public static void SetupChangeLogToken(this Mock<ISession> session, string changeLogToken){
            session.Setup (s => s.Binding.GetRepositoryService ().GetRepositoryInfo (It.IsAny<string>(), null).LatestChangeLogToken).Returns (changeLogToken);
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
            changeList.Add (GenerateChangeEvent(type, objectId).Object);
            return changeList;
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
            var session = PrepareSessionMockForSingleChange(type, id);
            var newRemoteObject =  CreateRemoteFolderMock(id);
            session.Setup (s => s.GetObject (It.IsAny<string>())).Returns (newRemoteObject.Object);
         
            return session;
        }

        public static Mock<ISession> GetSessionMockReturning3Changesin2Batches(DotCMIS.Enums.ChangeType type = DotCMIS.Enums.ChangeType.Updated, bool overlapping = false) {
            var changeEvents = new Mock<IChangeEvents> ();
            changeEvents.Setup (ce => ce.HasMoreItems).ReturnsInOrder ((bool?) true, (bool?) false);
            changeEvents.Setup (ce => ce.LatestChangeLogToken).ReturnsInOrder ("A", "B");
            changeEvents.Setup (ce => ce.TotalNumItems).ReturnsInOrder (3, overlapping ? 2 : 1);
            var event1 = MockSessionUtil.GenerateChangeEvent(type, "one");
            var event2 = MockSessionUtil.GenerateChangeEvent(type, "two");
            var event3 = MockSessionUtil.GenerateChangeEvent(type, "three");
            List<IChangeEvent> changeList1 = new List<IChangeEvent>();
            changeList1.Add(event1.Object);
            changeList1.Add(event2.Object);
            List<IChangeEvent> changeList2 = new List<IChangeEvent>();
            if(overlapping) {
                changeList2.Add(event2.Object);
            }
            changeList2.Add(event3.Object);
            changeEvents.Setup (ce => ce.ChangeEventList).ReturnsInOrder (changeList1, changeList2);

            var session = new Mock<ISession> ();
            session.SetupSessionDefaultValues();
            session.Setup (s => s.Binding.GetRepositoryService ().GetRepositoryInfo (It.IsAny<string>(), null).LatestChangeLogToken).Returns ("token");
            session.Setup (s => s.GetContentChanges (It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<long>())).Returns (changeEvents.Object);

            return session;            
        }

        public static Mock<ISession> GetSessionMockReturningDocumentChange(DotCMIS.Enums.ChangeType type, string id, string documentContentStreamId = null) {
            var session = MockSessionUtil.PrepareSessionMockForSingleChange(type, id);

            var newRemoteObject =  MockSessionUtil.CreateRemoteObjectMock(documentContentStreamId, id);
            session.Setup (s => s.GetObject (It.IsAny<string>())).Returns (newRemoteObject.Object);
         
            return session;
        }
    }
}
