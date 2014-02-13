using System;
using System.Collections.Generic;

using CmisSync.Lib;
using CmisSync.Lib.Events;
using CmisSync.Lib.Sync.Strategy;

using DotCMIS.Client;
using DotCMIS.Data;
using DotCMIS.Data.Extensions;
using DotCMIS.Binding.Services;

using NUnit.Framework;

using Moq;

using TestLibrary.TestUtils;

namespace TestLibrary.IntegrationTests
{
    [TestFixture]
    public class ContentChangeIT
    {
        private readonly bool isPropertyChangesSupported = false;
        private readonly string changeLogToken = "token";
        private readonly string latestChangeLogToken = "latestChangeLogToken";
        private readonly string repoId = "repoId";
        private readonly int maxNumberOfContentChanges = 1000;
        

        private List<IChangeEvent> generateSingleChangeListMock (DotCMIS.Enums.ChangeType type, string objectId = "objId") {
            var changeList = new List<IChangeEvent> ();
            changeList.Add (MockUtil.GenerateChangeEvent(type, objectId).Object);
            return changeList;
        }

        [Test, Category("Fast")]
        public void generateChangeListHelperWorksCorrectly () {
            List<IChangeEvent> list = generateSingleChangeListMock(DotCMIS.Enums.ChangeType.Deleted);
            Assert.That(list.Count, Is.EqualTo(1));
            Assert.That(list[0].ChangeType, Is.EqualTo(DotCMIS.Enums.ChangeType.Deleted));
        }


        private Mock<IFolder> CreateRemoteFolderMock(string folderId){
            var newRemoteObject = new Mock<IFolder> ();
            newRemoteObject.Setup(d => d.Id).Returns(folderId);
            return newRemoteObject;
        }


        private static void AddLocalFile(Mock<IDatabase> db, string path = "path"){
            db.Setup(foo => foo.GetFilePath(It.IsAny<string>())).Returns(path);
        }

        private static void AddLocalFolder(Mock<IDatabase> db, string path = "path"){
            db.Setup(foo => foo.GetFolderPath(It.IsAny<string>())).Returns(path);
        }

        private Mock<ISession> PrepareSessionMockForSingleChange(DotCMIS.Enums.ChangeType type) {
            var changeEvents = new Mock<IChangeEvents> ();
            var changeList = generateSingleChangeListMock(type); 
            changeEvents.Setup (ce => ce.HasMoreItems).Returns ((bool?) false);
            changeEvents.Setup (ce => ce.LatestChangeLogToken).Returns (latestChangeLogToken);
            changeEvents.Setup (ce => ce.TotalNumItems).Returns (1);
            changeEvents.Setup (ce => ce.ChangeEventList).Returns (changeList);

            var session = new Mock<ISession> ();
            session.SetupSessionDefaultValues();
            session.Setup (s => s.Binding.GetRepositoryService ().GetRepositoryInfo (It.IsAny<string>(), null).LatestChangeLogToken).Returns (changeLogToken);
            session.Setup (s => s.GetContentChanges (It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<long>())).Returns (changeEvents.Object);
            return session;

        }
        
        private Mock<ISession> GetSessionMockReturningDocumentChange(DotCMIS.Enums.ChangeType type, string documentContentStreamId = null) {
            var session = PrepareSessionMockForSingleChange(type);

            var newRemoteObject =  MockUtil.CreateRemoteObjectMock(documentContentStreamId);
            session.Setup (s => s.GetObject (It.IsAny<string>())).Returns (newRemoteObject.Object);
         
            return session;
        }

        private Mock<ISession> GetSessionMockReturningFolderChange(DotCMIS.Enums.ChangeType type) {
            var session = PrepareSessionMockForSingleChange(type);
            var newRemoteObject =  CreateRemoteFolderMock("folderId");
            session.Setup (s => s.GetObject (It.IsAny<string>())).Returns (newRemoteObject.Object);
         
            return session;
        }


        private ObservableHandler RunQueue(Mock<ISession> session, Mock<IDatabase> database) {
            var manager = new SyncEventManager();

            var observer = new ObservableHandler();
            manager.AddEventHandler(observer);

            SingleStepEventQueue queue = new SingleStepEventQueue(manager);

            var changes = new ContentChanges (session.Object, database.Object, queue, maxNumberOfContentChanges, isPropertyChangesSupported);
            manager.AddEventHandler(changes);

            var startSyncEvent = new StartNextSyncEvent (false);
            queue.AddEvent(startSyncEvent);
            queue.Run();

            return observer;
        }

        private void AssertHandlerGotSingleFolderEvent(ObservableHandler observer, MetaDataChangeType metaType) {
            Assert.That(observer.list.Count, Is.EqualTo(1));
            Assert.That(observer.list[0], Is.TypeOf(typeof(FolderEvent)));
            var folderEvent = observer.list[0] as FolderEvent;
            Assert.That(folderEvent.Remote, Is.EqualTo(metaType), "MetaDataChangeType incorrect");
            
        }

        private void AssertHandlerGotSingleFileEvent(ObservableHandler observer, MetaDataChangeType metaType, ContentChangeType contentType) {
            Assert.That(observer.list.Count, Is.EqualTo(1));
            Assert.That(observer.list[0], Is.TypeOf(typeof(FileEvent)));
            var fileEvent = observer.list[0] as FileEvent;

            Assert.That(fileEvent.Remote, Is.EqualTo(metaType), "MetaDataChangeType incorrect");
            Assert.That(fileEvent.RemoteContent, Is.EqualTo(contentType), "ContentChangeType incorrect");
            
        }

        [Test, Category("Fast")]
        public void RemoteSecurityChangeOfExistingFile ()
        {
            Mock<IDatabase> database = MockUtil.GetDbMockWithToken();
            AddLocalFile(database);

            Mock<ISession> session = GetSessionMockReturningDocumentChange(DotCMIS.Enums.ChangeType.Security);
            ObservableHandler observer = RunQueue(session, database);

            AssertHandlerGotSingleFileEvent(observer, MetaDataChangeType.CHANGED, ContentChangeType.NONE);

        }

        [Test, Category("Fast")]
        public void RemoteSecurityChangeOfNonExistingFile ()
        {
            Mock<IDatabase> database = MockUtil.GetDbMockWithToken();

            Mock<ISession> session = GetSessionMockReturningDocumentChange(DotCMIS.Enums.ChangeType.Security);
            ObservableHandler observer = RunQueue(session, database);

            AssertHandlerGotSingleFileEvent(observer, MetaDataChangeType.CREATED, ContentChangeType.CREATED);
        }

        [Test, Category("Fast")]
        public void RemoteDocumentCreationWithContent ()
        {
            Mock<IDatabase> database = MockUtil.GetDbMockWithToken();

            Mock<ISession> session = GetSessionMockReturningDocumentChange(DotCMIS.Enums.ChangeType.Created, "someStreamId");

            ObservableHandler observer = RunQueue(session, database);
            
            AssertHandlerGotSingleFileEvent(observer, MetaDataChangeType.CREATED, ContentChangeType.CREATED);
        }

        [Test, Category("Fast")]
        public void RemoteDocumentCreationWithoutContent ()
        {
            Mock<IDatabase> database = MockUtil.GetDbMockWithToken();

            Mock<ISession> session = GetSessionMockReturningDocumentChange(DotCMIS.Enums.ChangeType.Created, null);

            ObservableHandler observer = RunQueue(session, database);
            
            AssertHandlerGotSingleFileEvent(observer, MetaDataChangeType.CREATED, ContentChangeType.NONE);
        }


        [Test, Category("Fast")]
        public void LocallyNotExistingRemoteDocumentUpdated ()
        {
            Mock<IDatabase> database = MockUtil.GetDbMockWithToken();

            Mock<ISession> session = GetSessionMockReturningDocumentChange(DotCMIS.Enums.ChangeType.Updated, null);

            ObservableHandler observer = RunQueue(session, database);

            AssertHandlerGotSingleFileEvent(observer, MetaDataChangeType.CREATED, ContentChangeType.CREATED);
        }

        [Test, Category("Fast")]
        public void LocallyExistingRemoteDocumentUpdated ()
        {
            Mock<IDatabase> database = MockUtil.GetDbMockWithToken();
            AddLocalFile(database);

            Mock<ISession> session = GetSessionMockReturningDocumentChange(DotCMIS.Enums.ChangeType.Updated, null);

            ObservableHandler observer = RunQueue(session, database);

            AssertHandlerGotSingleFileEvent(observer, MetaDataChangeType.CHANGED, ContentChangeType.CHANGED);
        }
        
        [Test, Category("Fast")]
        public void RemoteDeletionChangeWithoutLocalFile ()
        {
            var queue = new Mock<ISyncEventQueue>();

            Mock<IDatabase> database = MockUtil.GetDbMockWithToken();

            Mock<ISession> session = GetSessionMockReturningDocumentChange(DotCMIS.Enums.ChangeType.Deleted, null);

            ObservableHandler observer = RunQueue(session, database);
            Assert.That(observer.list.Count, Is.EqualTo(0));
        }

        [Test, Category("Fast")]
        public void RemoteDeletionChangeTest ()
        {
            Mock<IDatabase> database = MockUtil.GetDbMockWithToken();
            AddLocalFile(database);

            Mock<ISession> session = GetSessionMockReturningDocumentChange(DotCMIS.Enums.ChangeType.Deleted, null);
            ObservableHandler observer = RunQueue(session, database);

            AssertHandlerGotSingleFileEvent(observer, MetaDataChangeType.DELETED, ContentChangeType.NONE);
        }

        [Test, Category("Fast")]
        public void RemoteFolderCreation ()
        {
            Mock<IDatabase> database = MockUtil.GetDbMockWithToken();

            Mock<ISession> session = GetSessionMockReturningFolderChange(DotCMIS.Enums.ChangeType.Created);
            ObservableHandler observer = RunQueue(session, database);

            AssertHandlerGotSingleFolderEvent(observer, MetaDataChangeType.CREATED);
        }

        [Test, Category("Fast")]
        public void RemoteFolderDeletionWithoutLocalFolder ()
        {
            Mock<IDatabase> database = MockUtil.GetDbMockWithToken();

            Mock<ISession> session = GetSessionMockReturningFolderChange(DotCMIS.Enums.ChangeType.Deleted);
            ObservableHandler observer = RunQueue(session, database);
            Assert.That(observer.list.Count, Is.EqualTo(0));

        }

        [Test, Category("Fast")]
        public void RemoteFolderDeletion ()
        {
            Mock<IDatabase> database = MockUtil.GetDbMockWithToken();
            AddLocalFolder(database);

            Mock<ISession> session = GetSessionMockReturningFolderChange(DotCMIS.Enums.ChangeType.Deleted);
            ObservableHandler observer = RunQueue(session, database);
            AssertHandlerGotSingleFolderEvent(observer, MetaDataChangeType.DELETED);

        }
    }
}

