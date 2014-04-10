using System;
using System.Collections.Generic;

using CmisSync.Lib;
using CmisSync.Lib.Events;
using CmisSync.Lib.Sync.Strategy;
using CmisSync.Lib.Storage;

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
        [TestFixtureSetUp]
        public void ClassInit()
        {
            log4net.Config.XmlConfigurator.Configure(ConfigManager.CurrentConfig.GetLog4NetConfig());
        }
        private readonly bool isPropertyChangesSupported = false;
        private readonly int maxNumberOfContentChanges = 1000;
        
        private static readonly string defaultId = "defaultId";
        

        private ObservableHandler RunQueue(Mock<ISession> session, Mock<IMetaDataStorage> storage) {
            var manager = new SyncEventManager();

            var observer = new ObservableHandler();
            manager.AddEventHandler(observer);

            SingleStepEventQueue queue = new SingleStepEventQueue(manager);

            var changes = new ContentChanges (session.Object, storage.Object, queue, maxNumberOfContentChanges, isPropertyChangesSupported);
            manager.AddEventHandler(changes);

            var transformer = new ContentChangeEventTransformer(queue, storage.Object);
            manager.AddEventHandler(transformer);

            var accumulator = new ContentChangeEventAccumulator(session.Object, queue);
            manager.AddEventHandler(accumulator);

            queue.RunStartSyncEvent();

            return observer;
        }

        [Test, Category("Fast"), Category("ContentChange")]
        public void RemoteSecurityChangeOfExistingFile ()
        {
            Mock<IMetaDataStorage> storage = MockMetaDataStorageUtil.GetMetaStorageMockWithToken();
            var path = Mock.Of<IFileInfo>( f => f.FullName == "path");
            storage.AddLocalFile(path, defaultId);

            Mock<ISession> session = MockSessionUtil.GetSessionMockReturningDocumentChange(DotCMIS.Enums.ChangeType.Security, defaultId);
            ObservableHandler observer = RunQueue(session, storage);

            storage.Verify(s=>s.GetObjectByRemoteId(defaultId), Times.Once());

            observer.AssertGotSingleFileEvent(MetaDataChangeType.CHANGED, ContentChangeType.NONE);

        }


        [Test, Category("Fast"), Category("ContentChange")]
        public void RemoteDocumentCreationWithContent ()
        {
            Mock<IMetaDataStorage> storage = MockMetaDataStorageUtil.GetMetaStorageMockWithToken();

            Mock<ISession> session = MockSessionUtil.GetSessionMockReturningDocumentChange(DotCMIS.Enums.ChangeType.Created, defaultId, "someStreamId");

            ObservableHandler observer = RunQueue(session, storage);
            
            observer.AssertGotSingleFileEvent(MetaDataChangeType.CREATED, ContentChangeType.CREATED);
        }


        [Test, Category("Fast"), Category("ContentChange")]
        public void LocallyNotExistingRemoteDocumentUpdated ()
        {
            Mock<IMetaDataStorage> storage = MockMetaDataStorageUtil.GetMetaStorageMockWithToken();

            Mock<ISession> session = MockSessionUtil.GetSessionMockReturningDocumentChange(DotCMIS.Enums.ChangeType.Updated, defaultId, null);

            ObservableHandler observer = RunQueue(session, storage);

            observer.AssertGotSingleFileEvent(MetaDataChangeType.CREATED, ContentChangeType.CREATED);
        }


        [Test, Category("Fast"), Category("ContentChange")]
        public void RemoteDeletionChangeTest ()
        {
            Mock<IMetaDataStorage> storage = MockMetaDataStorageUtil.GetMetaStorageMockWithToken();
            var file = Mock.Of<IFileInfo>(f => f.FullName == "path");
            storage.AddLocalFile(file, defaultId);

            Mock<ISession> session = MockSessionUtil.GetSessionMockReturningDocumentChange(DotCMIS.Enums.ChangeType.Deleted, defaultId, null);
            ObservableHandler observer = RunQueue(session, storage);

            observer.AssertGotSingleFileEvent(MetaDataChangeType.DELETED, ContentChangeType.NONE);
        }

        [Test, Category("Fast"), Category("ContentChange")]
        public void RemoteFolderCreation ()
        {
            Mock<IMetaDataStorage> storage = MockMetaDataStorageUtil.GetMetaStorageMockWithToken();

            Mock<ISession> session = MockSessionUtil.GetSessionMockReturningFolderChange(DotCMIS.Enums.ChangeType.Created);
            ObservableHandler observer = RunQueue(session, storage);

            observer.AssertGotSingleFolderEvent(MetaDataChangeType.CREATED);
        }

        [Test, Category("Fast"), Category("ContentChange")]
        public void RemoteFolderDeletionWithoutLocalFolder ()
        {
            Mock<IMetaDataStorage> storage = MockMetaDataStorageUtil.GetMetaStorageMockWithToken();

            Mock<ISession> session = MockSessionUtil.GetSessionMockReturningFolderChange(DotCMIS.Enums.ChangeType.Deleted);
            ObservableHandler observer = RunQueue(session, storage);
            Assert.That(observer.list.Count, Is.EqualTo(0));

        }

    }
}

