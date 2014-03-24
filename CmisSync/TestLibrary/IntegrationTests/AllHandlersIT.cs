using System;
using System.IO;
using System.Collections.Generic;

using CmisSync.Lib;
using CmisSync.Lib.Events;
using CmisSync.Lib.Storage;
using Strategy = CmisSync.Lib.Sync.Strategy;
using CmisSync.Lib.Sync.Strategy;
using CmisSync.Lib.Events.Filter;

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
    public class AllHandlersIT
    {
        [TestFixtureSetUp]
        public void ClassInit()
        {
            log4net.Config.XmlConfigurator.Configure(ConfigManager.CurrentConfig.GetLog4NetConfig());
        }

        private readonly bool isPropertyChangesSupported = false;
        private readonly string repoId = "repoId";
        private readonly int maxNumberOfContentChanges = 1000;

        private SingleStepEventQueue CreateQueue(Mock<ISession> session, Mock<IMetaDataStorage> storage) 
        {
            return CreateQueue(session, storage, new ObservableHandler());
        }

        private SingleStepEventQueue CreateQueue(Mock<ISession> session, Mock<IMetaDataStorage> storage, ObservableHandler observer) {

            var manager = new SyncEventManager();
            SingleStepEventQueue queue = new SingleStepEventQueue(manager);

            manager.AddEventHandler(observer);

            var changes = new ContentChanges (session.Object, storage.Object, queue, maxNumberOfContentChanges, isPropertyChangesSupported);
            manager.AddEventHandler(changes);

            var transformer = new ContentChangeEventTransformer(queue, storage.Object);
            manager.AddEventHandler(transformer);

            var ccaccumulator = new ContentChangeEventAccumulator(session.Object, queue);
            manager.AddEventHandler(ccaccumulator);

            var feaccumulator = new FileSystemEventAccumulator(queue, session.Object, storage.Object);
            manager.AddEventHandler(feaccumulator);

            var watcher = new Mock<Strategy.Watcher>(queue){CallBase = true};
            manager.AddEventHandler(watcher.Object);

            var localDetection = new LocalSituationDetection();
            var remoteDetection = new RemoteSituationDetection(session.Object);
            var syncMechanism = new SyncMechanism(localDetection, remoteDetection, queue, session.Object, storage.Object);
            manager.AddEventHandler(syncMechanism);

            var remoteFolder = MockUtil.CreateCmisFolder();
            var localFolder = new Mock<IDirectoryInfo>();
            var crawler = new Crawler(queue, remoteFolder.Object, localFolder.Object);
            manager.AddEventHandler(crawler);

            var permissionDenied = new GenericHandleDoublicatedEventsFilter<PermissionDeniedEvent, ConfigChangedEvent>();
            manager.AddEventHandler(permissionDenied);

            var invalidFolderNameFilter = new InvalidFolderNameFilter(queue);
            manager.AddEventHandler(invalidFolderNameFilter);

            var ignoreFolderFilter = new IgnoredFoldersFilter(queue);
            manager.AddEventHandler(ignoreFolderFilter);

            /* This is not implemented yet
            var ignoreFileFilter = new IgnoredFilesFilter(queue);
            manager.AddEventHandler(ignoreFileFilter);

            var failedOperationsFilder = new FailedOperationsFilter(queue);
            manager.AddEventHandler(failedOperationsFilder);
            */
            var ignoreFileNamesFilter = new IgnoredFileNamesFilter(queue);
            manager.AddEventHandler(ignoreFileNamesFilter);


            var debugHandler = new DebugLoggingHandler();
            manager.AddEventHandler(debugHandler);

            return queue;
        }
        
        [Test, Category("Fast")]
        public void RunFakeEvent ()
        {
            var session = new Mock<ISession>();
            var observer = new ObservableHandler();
            var storage = new Mock<IMetaDataStorage>();
            var queue = CreateQueue(session, storage, observer);
            var myEvent = new Mock<ISyncEvent>();
            queue.AddEvent(myEvent.Object);
            queue.Run();
            Assert.That(observer.list.Count, Is.EqualTo(1));
        }

        [Test, Category("Fast")]
        public void RunStartNewSyncEvent ()
        {
            var storage = new Mock<IMetaDataStorage>();
            var session = new Mock<ISession>();
            session.SetupSessionDefaultValues();
            session.SetupChangeLogToken("default");
            var myEvent = new StartNextSyncEvent();
            var observer = new ObservableHandler();
            var queue = CreateQueue(session, storage, observer);
            queue.AddEvent(myEvent);
            queue.Run();
            Assert.That(observer.list.Count, Is.EqualTo(1));
            Assert.That(observer.list[0], Is.TypeOf(typeof(FullSyncCompletedEvent)));
        }

        [Test, Category("Fast")]
        public void RunFSEventDeleted ()
        {
            var storage = new Mock<IMetaDataStorage>();
            string path = "/tmp/a/b";
            string id = "id";
            storage.AddLocalFile(path, id);
            
            var session = new Mock<ISession>();
            session.SetupSessionDefaultValues();
            session.SetupChangeLogToken("default");
            IDocument remote = MockUtil.CreateRemoteObjectMock(null, id).Object;
            session.Setup(s => s.GetObject(id)).Returns(remote);
            var myEvent = new FSEvent(WatcherChangeTypes.Deleted, path);
            var queue = CreateQueue(session, storage);
            queue.AddEvent(myEvent);
            queue.Run();

            session.Verify(f => f.Delete(It.Is<IObjectId>(i=>i.Id==id), true), Times.Once());
        }

    }
}

