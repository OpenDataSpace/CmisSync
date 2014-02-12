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

namespace TestLibrary.SyncStrategiesTests
{
    [TestFixture]
    public class ContentChangesTest
    {
        private readonly int maxNumberOfContentChanges = 1000;
        private readonly bool isPropertyChangesSupported = false;
        private readonly string changeLogToken = "token";
        private readonly string latestChangeLogToken = "latestChangeLogToken";
        private readonly string repoId = "repoId";
        
        private Mock<IChangeEvent> GenerateChangeEvent(DotCMIS.Enums.ChangeType type, string objectId) {
            var changeEvent = new Mock<IChangeEvent> ();
            changeEvent.Setup (ce => ce.ObjectId).Returns (objectId);
            changeEvent.Setup (ce => ce.ChangeType).Returns (type);

            return changeEvent;
        }

        private List<IChangeEvent> generateSingleChangeListMock (DotCMIS.Enums.ChangeType type, string objectId = "objId") {
            var changeList = new List<IChangeEvent> ();
            changeList.Add (GenerateChangeEvent(type, objectId).Object);
            return changeList;
        }

        [Test, Category("Fast")]
        public void generateChangeListHelperWorksCorrectly () {
            List<IChangeEvent> list = generateSingleChangeListMock(DotCMIS.Enums.ChangeType.Deleted);
            Assert.That(list.Count, Is.EqualTo(1));
            Assert.That(list[0].ChangeType, Is.EqualTo(DotCMIS.Enums.ChangeType.Deleted));
        }

        private Mock<IDocument> createRemoteObjectMock(string documentContentStreamId){
            var newRemoteObject = new Mock<IDocument> ();
            newRemoteObject.Setup(d => d.ContentStreamId).Returns(documentContentStreamId);
            newRemoteObject.Setup(d => d.ContentStreamLength).Returns(documentContentStreamId==null? 0 : 1);
            return newRemoteObject;
        }

        private Mock<IFolder> createRemoteFolderMock(string folderId){
            var newRemoteObject = new Mock<IFolder> ();
            newRemoteObject.Setup(d => d.Id).Returns(folderId);
            return newRemoteObject;
        }

        private void setupSessionDefaultValues(Mock<ISession> session) {
            session.Setup (s => s.Binding.GetRepositoryService ().GetRepositoryInfos (null)).Returns ((IList<IRepositoryInfo>)null);
            session.Setup (s => s.RepositoryInfo.Id).Returns (repoId);
        }

        [Test, Category("Fast")]
        public void CorrectDefaultPriority ()
        {
            var database = new Mock<IDatabase>();
            var queue = new Mock<ISyncEventQueue>();
            var session = new Mock<ISession>();
            var cc = new ContentChanges (session.Object, database.Object, queue.Object);
            Assert.That(cc.Priority, Is.EqualTo(1000));
        }


        [Test, Category("Fast")]
        public void ConstructorWithVaildEntriesTest ()
        {
            var database = new Mock<IDatabase>();
            var queue = new Mock<ISyncEventQueue>();
            var session = new Mock<ISession>();
            bool isPropertyChangesSupported = true;
            new ContentChanges (session.Object, database.Object, queue.Object);
            new ContentChanges (session.Object, database.Object, queue.Object, maxNumberOfContentChanges);
            new ContentChanges (session.Object, database.Object, queue.Object, maxNumberOfContentChanges, isPropertyChangesSupported);
            new ContentChanges (session.Object, database.Object, queue.Object, isPropertyChangesSupported: true);
            new ContentChanges (session.Object, database.Object, queue.Object, isPropertyChangesSupported: false);
        }

        [Test, Category("Fast")]
        [ExpectedException( typeof( ArgumentNullException ) )]
        public void ConstructorFailsOnNullDbTest ()
        {
            var queue = new Mock<ISyncEventQueue>();
            var session = new Mock<ISession>();
            new ContentChanges (session.Object, null, queue.Object);
        }

        [Test, Category("Fast")]
        public void ConstructorFailsOnInvalidMaxEventsLimitTest ()
        {
            var database = new Mock<IDatabase>();
            var queue = new Mock<ISyncEventQueue>();
            var session = new Mock<ISession>();
            try {
                new ContentChanges (session.Object, database.Object, queue.Object, -1);
                Assert.Fail ();
            } catch (ArgumentException) {
            }
            try {
                new ContentChanges (session.Object, database.Object, queue.Object, 0);
                Assert.Fail ();
            } catch (ArgumentException) {
            }
            try {
                new ContentChanges (session.Object, database.Object, queue.Object, 1);
                Assert.Fail ();
            } catch (ArgumentException) {
            }
        }

        [Test, Category("Fast")]
        [ExpectedException( typeof( ArgumentNullException ) )]
        public void ConstructorFailsOnNullSessionTest ()
        {
            var database = new Mock<IDatabase>();
            var queue = new Mock<ISyncEventQueue>();
            new ContentChanges (null, database.Object, queue.Object);
        }

        [Test, Category("Fast")]
        [ExpectedException( typeof( ArgumentNullException ) )]
        public void ConstructorFailsOnNullQueueTest ()
        {
            var database = new Mock<IDatabase>();
            var session = new Mock<ISession> ().Object;
            new ContentChanges (session, database.Object, null);
        }

        [Test, Category("Fast")]
        public void IgnoreWrongEventTest ()
        {
            var database = new Mock<IDatabase>();
            var queue = new Mock<ISyncEventQueue>();
            var session = new Mock<ISession>();
            var changes = new ContentChanges (session.Object, database.Object, queue.Object);
            var startSyncEvent = new StartNextSyncEvent (false);
            Assert.IsFalse (changes.Handle (startSyncEvent));
        }


        [Test, Category("Fast")]
        public void RetrunFalseOnError () {
            //TODO: this might not be the best behavior, this test verifies the current implementation not the desired one
            var database = new Mock<IDatabase>();
            var queue = new Mock<ISyncEventQueue>();
            var session = new Mock<ISession>();
            session.Setup(x => x.Binding).Throws(new Exception("SOME EXCEPTION"));
            var changes = new ContentChanges (session.Object, database.Object, queue.Object);
            var wrongEvent = new Mock<ISyncEvent> ().Object;
            Assert.IsFalse (changes.Handle (wrongEvent));
        }

        [Test, Category("Fast")]
        public void HandleFullSyncCompletedEventTest ()
        {
            string insertedToken = "";
            var startSyncEvent = new StartNextSyncEvent (false);
            startSyncEvent.SetParam (ContentChanges.FULL_SYNC_PARAM_NAME, changeLogToken);
            var completedEvent = new FullSyncCompletedEvent (startSyncEvent);
            int handled = 0;
            var database = new Mock<IDatabase>();
            database.Setup (db => db.SetChangeLogToken ("token")).Callback ((string s) => {
                insertedToken = s;
                handled ++;
            }
            );
            var queue = new Mock<ISyncEventQueue>();
            var session = new Mock<ISession>();
            var changes = new ContentChanges (session.Object, database.Object, queue.Object);
            Assert.IsFalse (changes.Handle (completedEvent));
            Assert.AreEqual (1, handled);
            Assert.AreEqual (changeLogToken, insertedToken);
        }

        [Test, Category("Fast")]
        public void HandleStartSyncEventOnNoRemoteChangeTest ()
        {
            var startSyncEvent = new StartNextSyncEvent (false);
            var session = new Mock<ISession>();
            setupSessionDefaultValues(session);
            session.Setup (s => s.Binding.GetRepositoryService ().GetRepositoryInfo (repoId, null).LatestChangeLogToken).Returns (changeLogToken);
            var database = new Mock<IDatabase>();
            database.Setup (db => db.GetChangeLogToken ()).Returns (changeLogToken);
            var queue = new Mock<ISyncEventQueue>();
            var changes = new ContentChanges (session.Object, database.Object, queue.Object);
            Assert.IsTrue (changes.Handle (startSyncEvent));
        }

        private Mock<IDatabase> GetDbMockWithToken(string token = "lastToken"){
            var database = new Mock<IDatabase>();
            database.Setup (db => db.GetChangeLogToken ()).Returns (token);
            return database;
        }

        private static void AddLocalFile(Mock<IDatabase> db, string path = "path"){
            db.Setup(foo => foo.GetFilePath(It.IsAny<string>())).Returns(path);
        }

        private Mock<ISession> PrepareSessionMockForSingleChange(DotCMIS.Enums.ChangeType type) {
            var changeEvents = new Mock<IChangeEvents> ();
            var changeList = generateSingleChangeListMock(type); 
            changeEvents.Setup (ce => ce.HasMoreItems).Returns ((bool?) false);
            changeEvents.Setup (ce => ce.LatestChangeLogToken).Returns (latestChangeLogToken);
            changeEvents.Setup (ce => ce.TotalNumItems).Returns (1);
            changeEvents.Setup (ce => ce.ChangeEventList).Returns (changeList);

            var session = new Mock<ISession> ();
            setupSessionDefaultValues(session);
            session.Setup (s => s.Binding.GetRepositoryService ().GetRepositoryInfo (It.IsAny<string>(), null).LatestChangeLogToken).Returns (changeLogToken);
            session.Setup (s => s.GetContentChanges (It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<long>())).Returns (changeEvents.Object);
            return session;

        }
        
        private Mock<ISession> GetSessionMockReturningDocumentChange(DotCMIS.Enums.ChangeType type, string documentContentStreamId = null) {
            var session = PrepareSessionMockForSingleChange(type);

            var newRemoteObject =  createRemoteObjectMock(documentContentStreamId);
            session.Setup (s => s.GetObject (It.IsAny<string>())).Returns (newRemoteObject.Object);
         
            return session;
        }

        private Mock<ISession> GetSessionMockReturningFolderChange(DotCMIS.Enums.ChangeType type) {
            var session = PrepareSessionMockForSingleChange(type);
            var newRemoteObject =  createRemoteFolderMock("folderId");
            session.Setup (s => s.GetObject (It.IsAny<string>())).Returns (newRemoteObject.Object);
         
            return session;
        }


        private Mock<ISession> GetSessionMockReturning3Changesin2Batches(DotCMIS.Enums.ChangeType type = DotCMIS.Enums.ChangeType.Updated, bool overlapping = false) {
            var changeEvents = new Mock<IChangeEvents> ();
            changeEvents.Setup (ce => ce.HasMoreItems).ReturnsInOrder ((bool?) true, (bool?) false);
            changeEvents.Setup (ce => ce.LatestChangeLogToken).ReturnsInOrder ("A", "B");
            changeEvents.Setup (ce => ce.TotalNumItems).ReturnsInOrder (3, overlapping ? 2 : 1);
            var event1 = GenerateChangeEvent(type, "one");
            var event2 = GenerateChangeEvent(type, "two");
            var event3 = GenerateChangeEvent(type, "three");
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
            setupSessionDefaultValues(session);
            session.Setup (s => s.Binding.GetRepositoryService ().GetRepositoryInfo (It.IsAny<string>(), null).LatestChangeLogToken).Returns (changeLogToken);
            session.Setup (s => s.GetContentChanges (It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<long>())).Returns (changeEvents.Object);
            var newRemoteObject =  createRemoteObjectMock(null);
            session.Setup (s => s.GetObject (It.IsAny<string>())).Returns (newRemoteObject.Object);

            return session;            
        }

        [Test, Category("Fast")]
        public void HandleStartSyncEventOnOneRemoteSecurityChangeOfExistingFile ()
        {
            FileEvent fileEvent = null;
            var queue = new Mock<ISyncEventQueue>();
            queue.Setup(q => q.AddEvent (It.IsAny<FileEvent>())).Callback ((ISyncEvent f) => {
                fileEvent = f as FileEvent;
            }
            );

            Mock<IDatabase> database = GetDbMockWithToken();
            AddLocalFile(database);

            Mock<ISession> session = GetSessionMockReturningDocumentChange(DotCMIS.Enums.ChangeType.Security);

            var startSyncEvent = new StartNextSyncEvent (false);
            var changes = new ContentChanges (session.Object, database.Object, queue.Object, maxNumberOfContentChanges, isPropertyChangesSupported);

            Assert.IsTrue (changes.Handle (startSyncEvent));
            queue.Verify(foo => foo.AddEvent(It.IsAny<FileEvent>()), Times.Once());
            Assert.That(fileEvent.Remote, Is.EqualTo(MetaDataChangeType.CHANGED));
            Assert.That(fileEvent.RemoteContent, Is.EqualTo(ContentChangeType.NONE));
        }

        [Test, Category("Fast")]
        public void HandleStartSyncEventOnOneRemoteSecurityChangeOfNonExistingFile ()
        {
            FileEvent fileEvent = null;
            var queue = new Mock<ISyncEventQueue>();
            queue.Setup(q => q.AddEvent (It.IsAny<FileEvent>())).Callback ((ISyncEvent f) => {
                fileEvent = f as FileEvent;
            }
            );

            Mock<IDatabase> database = GetDbMockWithToken();

            Mock<ISession> session = GetSessionMockReturningDocumentChange(DotCMIS.Enums.ChangeType.Security);

            var startSyncEvent = new StartNextSyncEvent (false);
            var changes = new ContentChanges (session.Object, database.Object, queue.Object, maxNumberOfContentChanges, isPropertyChangesSupported);

            Assert.IsTrue (changes.Handle (startSyncEvent));
            queue.Verify(foo => foo.AddEvent(It.IsAny<FileEvent>()), Times.Once());
            Assert.That(fileEvent.Remote, Is.EqualTo(MetaDataChangeType.CREATED));
            Assert.That(fileEvent.RemoteContent, Is.EqualTo(ContentChangeType.CREATED));
        }

        [Test, Category("Fast")]
        public void HandleStartSyncEventOnOneRemoteDocumentCreationWithContent ()
        {
            FileEvent fileEvent = null;
            var queue = new Mock<ISyncEventQueue>();
            queue.Setup(q => q.AddEvent (It.IsAny<FileEvent>())).Callback ((ISyncEvent f) => {
                fileEvent = f as FileEvent;
            }
            );

            Mock<IDatabase> database = GetDbMockWithToken();

            Mock<ISession> session = GetSessionMockReturningDocumentChange(DotCMIS.Enums.ChangeType.Created, "someStreamId");

            var startSyncEvent = new StartNextSyncEvent (false);
            var changes = new ContentChanges (session.Object, database.Object, queue.Object, maxNumberOfContentChanges, isPropertyChangesSupported);

            Assert.IsTrue (changes.Handle (startSyncEvent));
            queue.Verify(foo => foo.AddEvent(It.IsAny<FileEvent>()), Times.Once());
            Assert.That(fileEvent.Remote, Is.EqualTo(MetaDataChangeType.CREATED));
            Assert.That(fileEvent.RemoteContent, Is.EqualTo(ContentChangeType.CREATED));
        }

        [Test, Category("Fast")]
        public void HandleStartSyncEventOnOneRemoteDocumentCreationWithoutContent ()
        {
            FileEvent fileEvent = null;
            var queue = new Mock<ISyncEventQueue>();
            queue.Setup(q => q.AddEvent (It.IsAny<FileEvent>())).Callback ((ISyncEvent f) => {
                fileEvent = f as FileEvent;
            }
            );

            Mock<IDatabase> database = GetDbMockWithToken();

            Mock<ISession> session = GetSessionMockReturningDocumentChange(DotCMIS.Enums.ChangeType.Created, null);

            var startSyncEvent = new StartNextSyncEvent (false);
            var changes = new ContentChanges (session.Object, database.Object, queue.Object, maxNumberOfContentChanges, isPropertyChangesSupported);

            Assert.IsTrue (changes.Handle (startSyncEvent));
            queue.Verify(foo => foo.AddEvent(It.IsAny<FileEvent>()), Times.Once());
            Assert.That(fileEvent.Remote, Is.EqualTo(MetaDataChangeType.CREATED));
            Assert.That(fileEvent.RemoteContent, Is.EqualTo(ContentChangeType.NONE));
        }

        [Test, Category("Fast")]
        public void HandleStartSyncEventOnOneRemoteFolderCreation ()
        {
            FolderEvent folderEvent = null;
            var queue = new Mock<ISyncEventQueue>();
            queue.Setup(q => q.AddEvent (It.IsAny<FolderEvent>())).Callback ((ISyncEvent f) => {
                folderEvent = f as FolderEvent;
            }
            );

            Mock<IDatabase> database = GetDbMockWithToken();

            Mock<ISession> session = GetSessionMockReturningFolderChange(DotCMIS.Enums.ChangeType.Created);

            var startSyncEvent = new StartNextSyncEvent (false);
            var changes = new ContentChanges (session.Object, database.Object, queue.Object, maxNumberOfContentChanges, isPropertyChangesSupported);

            Assert.IsTrue (changes.Handle (startSyncEvent));
            queue.Verify(foo => foo.AddEvent(It.IsAny<FolderEvent>()), Times.Once());
            Assert.That(folderEvent.Remote, Is.EqualTo(MetaDataChangeType.CREATED));
        }

        [Test, Category("Fast")]
        public void HandleStartSyncEventOnOneLocallyNotExistingRemoteDocumentUpdated ()
        {
            FileEvent fileEvent = null;
            var queue = new Mock<ISyncEventQueue>();
            queue.Setup(q => q.AddEvent (It.IsAny<FileEvent>())).Callback ((ISyncEvent f) => {
                fileEvent = f as FileEvent;
            }
            );

            Mock<IDatabase> database = GetDbMockWithToken();

            Mock<ISession> session = GetSessionMockReturningDocumentChange(DotCMIS.Enums.ChangeType.Updated, null);

            var startSyncEvent = new StartNextSyncEvent (false);
            var changes = new ContentChanges (session.Object, database.Object, queue.Object, maxNumberOfContentChanges, isPropertyChangesSupported);

            Assert.IsTrue (changes.Handle (startSyncEvent));
            queue.Verify(foo => foo.AddEvent(It.IsAny<FileEvent>()), Times.Once());
            Assert.That(fileEvent.Remote, Is.EqualTo(MetaDataChangeType.CREATED));
            Assert.That(fileEvent.RemoteContent, Is.EqualTo(ContentChangeType.CREATED));
        }

        [Test, Category("Fast")]
        public void HandleStartSyncEventOnOneLocallyExistingRemoteDocumentUpdated ()
        {
            FileEvent fileEvent = null;
            var queue = new Mock<ISyncEventQueue>();
            queue.Setup(q => q.AddEvent (It.IsAny<FileEvent>())).Callback ((ISyncEvent f) => {
                fileEvent = f as FileEvent;
            }
            );

            Mock<IDatabase> database = GetDbMockWithToken();
            AddLocalFile(database);

            Mock<ISession> session = GetSessionMockReturningDocumentChange(DotCMIS.Enums.ChangeType.Updated, null);

            var startSyncEvent = new StartNextSyncEvent (false);
            var changes = new ContentChanges (session.Object, database.Object, queue.Object, maxNumberOfContentChanges, isPropertyChangesSupported);

            Assert.IsTrue (changes.Handle (startSyncEvent));
            queue.Verify(foo => foo.AddEvent(It.IsAny<FileEvent>()), Times.Once());
            Assert.That(fileEvent.Remote, Is.EqualTo(MetaDataChangeType.CHANGED));
            Assert.That(fileEvent.RemoteContent, Is.EqualTo(ContentChangeType.CHANGED));
        }
        
        [Test, Category("Fast")]
        public void HandleStartSyncEventOnOneRemoteDeletionChangeWithoutLocalFile ()
        {
            var queue = new Mock<ISyncEventQueue>();

            Mock<IDatabase> database = GetDbMockWithToken();

            Mock<ISession> session = GetSessionMockReturningDocumentChange(DotCMIS.Enums.ChangeType.Deleted, null);

            var startSyncEvent = new StartNextSyncEvent (false);
            var changes = new ContentChanges (session.Object, database.Object, queue.Object, maxNumberOfContentChanges, isPropertyChangesSupported);

            Assert.IsTrue (changes.Handle (startSyncEvent));
            queue.Verify(foo => foo.AddEvent(It.IsAny<FileEvent>()), Times.Never());
        }

        [Test, Category("Fast")]
        public void HandleStartSyncEventOnOneRemoteDeletionChangeTest ()
        {
            FileEvent fileEvent = null;
            var queue = new Mock<ISyncEventQueue>();
            queue.Setup(q => q.AddEvent (It.IsAny<FileEvent>())).Callback ((ISyncEvent f) => {
                fileEvent = f as FileEvent;
            }
            );

            Mock<IDatabase> database = GetDbMockWithToken();
            AddLocalFile(database);

            Mock<ISession> session = GetSessionMockReturningDocumentChange(DotCMIS.Enums.ChangeType.Deleted, null);

            var startSyncEvent = new StartNextSyncEvent (false);
            var changes = new ContentChanges (session.Object, database.Object, queue.Object, maxNumberOfContentChanges, isPropertyChangesSupported);

            Assert.IsTrue (changes.Handle (startSyncEvent));
            queue.Verify(foo => foo.AddEvent(It.IsAny<FileEvent>()), Times.Once());
            Assert.That(fileEvent.Remote, Is.EqualTo(MetaDataChangeType.DELETED));
            Assert.That(fileEvent.RemoteContent, Is.EqualTo(ContentChangeType.NONE));
        }

        [Test, Category("Fast")]
        public void ExecuteCrawlSyncOnNoLocalTokenAvailableTest ()
        {
            ISyncEvent queuedEvent = null;
            var startSyncEvent = new StartNextSyncEvent (false);
            var session = new Mock<ISession>();
            setupSessionDefaultValues(session);
            session.Setup (s => s.Binding.GetRepositoryService ().GetRepositoryInfo (repoId, null).LatestChangeLogToken).Returns (changeLogToken);
            var database = new Mock<IDatabase>();
            database.Setup (db => db.GetChangeLogToken ()).Returns ((string)null);
            int handled = 0;
            var queue = new Mock<ISyncEventQueue>();
            queue.Setup (q => q.AddEvent (It.IsAny<ISyncEvent> ())).Callback<ISyncEvent> ((e) => {
                handled ++;
                queuedEvent = e;
            }
            );
            var changes = new ContentChanges (session.Object, database.Object, queue.Object);
            Assert.IsTrue (changes.Handle (startSyncEvent));
            Assert.AreEqual (1, handled);
            Assert.NotNull (queuedEvent);
            Assert.IsTrue (queuedEvent is StartNextSyncEvent);
            Assert.IsTrue (((StartNextSyncEvent)queuedEvent).FullSyncRequested);
            string returnedChangeLogToken;
            Assert.IsTrue (((StartNextSyncEvent)queuedEvent).TryGetParam (ContentChanges.FULL_SYNC_PARAM_NAME, out returnedChangeLogToken));
            Assert.AreEqual (changeLogToken, returnedChangeLogToken);
        }


        [Test, Category("Fast")]
        public void IgnoreCrawlSyncEventTest ()
        {
            var start = new StartNextSyncEvent (true);
            var repositoryService = new Mock<IRepositoryService> ();
            repositoryService.Setup (r => r.GetRepositoryInfos (null)).Returns ((IList<IRepositoryInfo>)null);
            repositoryService.Setup (r => r.GetRepositoryInfo (It.IsAny<string> (), It.IsAny<IExtensionsData> ()).LatestChangeLogToken).Returns (latestChangeLogToken);
            var session = new Mock<ISession>();
            session.Setup (s => s.Binding.GetRepositoryService ()).Returns (repositoryService.Object);
            session.Setup (s => s.RepositoryInfo.Id).Returns (repoId);
            var database = new Mock<IDatabase>();
            database.Setup (db => db.GetChangeLogToken ()).Returns (changeLogToken);
            var queue = new Mock<ISyncEventQueue>();
            var changes = new ContentChanges (session.Object, database.Object, queue.Object);
            Assert.IsFalse (changes.Handle (start));
            string result;
            Assert.IsFalse (start.TryGetParam (ContentChanges.FULL_SYNC_PARAM_NAME, out result));
            Assert.IsNull (result);
        }

        [Test, Category("Fast")]
        public void ExtendCrawlSyncEventTest ()
        {
            var start = new StartNextSyncEvent (true);
            var repositoryService = new Mock<IRepositoryService> ();
            repositoryService.Setup (r => r.GetRepositoryInfos (null)).Returns ((IList<IRepositoryInfo>)null);
            repositoryService.Setup (r => r.GetRepositoryInfo (It.IsAny<string> (), It.IsAny<IExtensionsData> ()).LatestChangeLogToken).Returns (latestChangeLogToken);
            var session = new Mock<ISession>();
            session.Setup (s => s.Binding.GetRepositoryService ()).Returns (repositoryService.Object);
            session.Setup (s => s.RepositoryInfo.Id).Returns (repoId);
            var manager = new Mock<SyncEventManager> ().Object;
            var database = new Mock<IDatabase>();
            database.Setup (db => db.GetChangeLogToken ()).Returns ((string)null);
            var queue = new Mock<ISyncEventQueue>();
            var changes = new ContentChanges (session.Object, database.Object, queue.Object);
            Assert.IsFalse (changes.Handle (start));
            string result;
            Assert.IsTrue (start.TryGetParam (ContentChanges.FULL_SYNC_PARAM_NAME, out result));
            Assert.AreEqual (latestChangeLogToken, result);
        }

        [Test, Category("Fast")]
        public void PagingTest ()
        {
            var queue = new Mock<ISyncEventQueue>();

            Mock<IDatabase> database = GetDbMockWithToken();

            Mock<ISession> session = GetSessionMockReturning3Changesin2Batches();

            var startSyncEvent = new StartNextSyncEvent (false);
            var changes = new ContentChanges (session.Object, database.Object, queue.Object, maxNumberOfContentChanges, isPropertyChangesSupported);

            Assert.IsTrue (changes.Handle (startSyncEvent));
            queue.Verify(foo => foo.AddEvent(It.IsAny<FileEvent>()), Times.Exactly(3));
        }

        [Test, Category("Fast")]
        public void IgnoreDuplicatedContentChangesEventTest ()
        {
            var queue = new Mock<ISyncEventQueue>();

            Mock<IDatabase> database = GetDbMockWithToken();

            Mock<ISession> session = GetSessionMockReturning3Changesin2Batches(DotCMIS.Enums.ChangeType.Created, true);

            var startSyncEvent = new StartNextSyncEvent (false);
            var changes = new ContentChanges (session.Object, database.Object, queue.Object, maxNumberOfContentChanges, isPropertyChangesSupported);

            Assert.IsTrue (changes.Handle (startSyncEvent));
            queue.Verify(foo => foo.AddEvent(It.IsAny<FileEvent>()), Times.Exactly(3));
        }
    }
}

