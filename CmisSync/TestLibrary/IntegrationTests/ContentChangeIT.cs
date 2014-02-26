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
        private readonly string repoId = "repoId";
        private readonly int maxNumberOfContentChanges = 1000;
        
        private Mock<IFolder> CreateRemoteFolderMock(string folderId){
            var newRemoteObject = new Mock<IFolder> ();
            newRemoteObject.Setup(d => d.Id).Returns(folderId);
            return newRemoteObject;
        }


        
        private Mock<ISession> GetSessionMockReturningDocumentChange(DotCMIS.Enums.ChangeType type, string documentContentStreamId = null) {
            var session = MockUtil.PrepareSessionMockForSingleChange(type);

            var newRemoteObject =  MockUtil.CreateRemoteObjectMock(documentContentStreamId);
            session.Setup (s => s.GetObject (It.IsAny<string>())).Returns (newRemoteObject.Object);
         
            return session;
        }

        private Mock<ISession> GetSessionMockReturningFolderChange(DotCMIS.Enums.ChangeType type) {
            var session = MockUtil.PrepareSessionMockForSingleChange(type);
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

            var transformer = new ContentChangeEventTransformer(queue, database.Object);
            manager.AddEventHandler(transformer);

            var accumulator = new ContentChangeEventAccumulator(session.Object, queue);
            manager.AddEventHandler(accumulator);

            var startSyncEvent = new StartNextSyncEvent (false);
            queue.AddEvent(startSyncEvent);
            queue.Run();

            return observer;
        }

        [Test, Category("Fast"), Category("ContentChange")]
        public void RemoteSecurityChangeOfExistingFile ()
        {
            Mock<IDatabase> database = MockUtil.GetDbMockWithToken();
            database.AddLocalFile();

            Mock<ISession> session = GetSessionMockReturningDocumentChange(DotCMIS.Enums.ChangeType.Security);
            ObservableHandler observer = RunQueue(session, database);

            observer.AssertGotSingleFileEvent(MetaDataChangeType.CHANGED, ContentChangeType.NONE);

        }


        [Test, Category("Fast"), Category("ContentChange")]
        public void RemoteDocumentCreationWithContent ()
        {
            Mock<IDatabase> database = MockUtil.GetDbMockWithToken();

            Mock<ISession> session = GetSessionMockReturningDocumentChange(DotCMIS.Enums.ChangeType.Created, "someStreamId");

            ObservableHandler observer = RunQueue(session, database);
            
            observer.AssertGotSingleFileEvent(MetaDataChangeType.CREATED, ContentChangeType.CREATED);
        }


        [Test, Category("Fast"), Category("ContentChange")]
        public void LocallyNotExistingRemoteDocumentUpdated ()
        {
            Mock<IDatabase> database = MockUtil.GetDbMockWithToken();

            Mock<ISession> session = GetSessionMockReturningDocumentChange(DotCMIS.Enums.ChangeType.Updated, null);

            ObservableHandler observer = RunQueue(session, database);

            observer.AssertGotSingleFileEvent(MetaDataChangeType.CREATED, ContentChangeType.CREATED);
        }


        [Test, Category("Fast"), Category("ContentChange")]
        public void RemoteDeletionChangeTest ()
        {
            Mock<IDatabase> database = MockUtil.GetDbMockWithToken();
            database.AddLocalFile();

            Mock<ISession> session = GetSessionMockReturningDocumentChange(DotCMIS.Enums.ChangeType.Deleted, null);
            ObservableHandler observer = RunQueue(session, database);

            observer.AssertGotSingleFileEvent(MetaDataChangeType.DELETED, ContentChangeType.NONE);
        }

        [Test, Category("Fast"), Category("ContentChange")]
        public void RemoteFolderCreation ()
        {
            Mock<IDatabase> database = MockUtil.GetDbMockWithToken();

            Mock<ISession> session = GetSessionMockReturningFolderChange(DotCMIS.Enums.ChangeType.Created);
            ObservableHandler observer = RunQueue(session, database);

            observer.AssertGotSingleFolderEvent(MetaDataChangeType.CREATED);
        }

        [Test, Category("Fast"), Category("ContentChange")]
        public void RemoteFolderDeletionWithoutLocalFolder ()
        {
            Mock<IDatabase> database = MockUtil.GetDbMockWithToken();

            Mock<ISession> session = GetSessionMockReturningFolderChange(DotCMIS.Enums.ChangeType.Deleted);
            ObservableHandler observer = RunQueue(session, database);
            Assert.That(observer.list.Count, Is.EqualTo(0));

        }

    }
}

