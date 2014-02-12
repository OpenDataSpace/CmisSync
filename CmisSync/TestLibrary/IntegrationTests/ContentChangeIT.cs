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



        [Test, Category("Fast")]
        public void HandleStartSyncEventOnOneRemoteSecurityChangeOfExistingFile ()
        {
            FileEvent fileEvent = null;
            var queue = new Mock<ISyncEventQueue>();
            queue.Setup(q => q.AddEvent (It.IsAny<FileEvent>())).Callback ((ISyncEvent f) => {
                fileEvent = f as FileEvent;
            }
            );

            Mock<IDatabase> database = MockUtil.GetDbMockWithToken();
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

            Mock<IDatabase> database = MockUtil.GetDbMockWithToken();

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

            Mock<IDatabase> database = MockUtil.GetDbMockWithToken();

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

            Mock<IDatabase> database = MockUtil.GetDbMockWithToken();

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

            Mock<IDatabase> database = MockUtil.GetDbMockWithToken();

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

            Mock<IDatabase> database = MockUtil.GetDbMockWithToken();

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

            Mock<IDatabase> database = MockUtil.GetDbMockWithToken();
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

            Mock<IDatabase> database = MockUtil.GetDbMockWithToken();

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

            Mock<IDatabase> database = MockUtil.GetDbMockWithToken();
            AddLocalFile(database);

            Mock<ISession> session = GetSessionMockReturningDocumentChange(DotCMIS.Enums.ChangeType.Deleted, null);

            var startSyncEvent = new StartNextSyncEvent (false);
            var changes = new ContentChanges (session.Object, database.Object, queue.Object, maxNumberOfContentChanges, isPropertyChangesSupported);

            Assert.IsTrue (changes.Handle (startSyncEvent));
            queue.Verify(foo => foo.AddEvent(It.IsAny<FileEvent>()), Times.Once());
            Assert.That(fileEvent.Remote, Is.EqualTo(MetaDataChangeType.DELETED));
            Assert.That(fileEvent.RemoteContent, Is.EqualTo(ContentChangeType.NONE));
        }

    }
}

