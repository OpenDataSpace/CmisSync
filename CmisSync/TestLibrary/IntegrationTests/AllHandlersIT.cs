//-----------------------------------------------------------------------
// <copyright file="AllHandlersIT.cs" company="GRAU DATA AG">
//
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General private License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//
//   This program is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//   GNU General private License for more details.
//
//   You should have received a copy of the GNU General private License
//   along with this program. If not, see http://www.gnu.org/licenses/.
//
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.IO;
using System.Collections.Generic;

using CmisSync.Lib;
using CmisSync.Lib.Data;
using CmisSync.Lib.Events;
using CmisSync.Lib.Storage;
using Strategy = CmisSync.Lib.Sync.Strategy;
using CmisSync.Lib.Sync.Strategy;
using CmisSync.Lib.Events.Filter;

using DBreeze;

using DotCMIS.Client;
using DotCMIS.Data;
using DotCMIS.Data.Extensions;
using DotCMIS.Binding.Services;

using Newtonsoft.Json;

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
            // Use Newtonsoft.Json as Serializator
            DBreeze.Utils.CustomSerializator.Serializator = JsonConvert.SerializeObject; 
            DBreeze.Utils.CustomSerializator.Deserializator = JsonConvert.DeserializeObject;
        }

        DBreezeEngine engine;

        [SetUp]
        public void SetupEngine()
        {
            engine = new DBreezeEngine(new DBreezeConfiguration{ Storage = DBreezeConfiguration.eStorage.MEMORY });
        }
        
        [TearDown]
        public void DestroyEngine() 
        {
            engine.Dispose();
        }
        
        private readonly string localRoot = Path.GetTempPath();
        private readonly string remoteRoot = "remoteroot";

        private readonly bool isPropertyChangesSupported = false;
        private readonly int maxNumberOfContentChanges = 1000;

        private SingleStepEventQueue CreateQueue(Mock<ISession> session, IMetaDataStorage storage) 
        {
            return CreateQueue(session, storage, new ObservableHandler());
        }

        private SingleStepEventQueue CreateQueue(Mock<ISession> session, IMetaDataStorage storage, IFileSystemInfoFactory fsFactory){
            return CreateQueue(session, storage, new ObservableHandler(), fsFactory);
        }

        private IMetaDataStorage GetInitializedStorage()
        {
            IPathMatcher matcher = new PathMatcher(localRoot, remoteRoot);
            return new MetaDataStorage(engine, matcher);
        }

        private SingleStepEventQueue CreateQueue(Mock<ISession> session, IMetaDataStorage storage, ObservableHandler observer, IFileSystemInfoFactory fsFactory = null) {

            var manager = new SyncEventManager();
            SingleStepEventQueue queue = new SingleStepEventQueue(manager);

            manager.AddEventHandler(observer);

            var changes = new ContentChanges (session.Object, storage, queue, maxNumberOfContentChanges, isPropertyChangesSupported);
            manager.AddEventHandler(changes);

            var transformer = new ContentChangeEventTransformer(queue, storage, fsFactory);
            manager.AddEventHandler(transformer);

            var ccaccumulator = new ContentChangeEventAccumulator(session.Object, queue);
            manager.AddEventHandler(ccaccumulator);

            var remoteFetcher = new RemoteObjectFetcher(session.Object, storage);
            manager.AddEventHandler(remoteFetcher);

            var localFetcher = new LocalObjectFetcher(storage.Matcher, fsFactory);
            manager.AddEventHandler(localFetcher);

            var watcher = new Mock<Strategy.Watcher>(queue){CallBase = true};
            manager.AddEventHandler(watcher.Object);

            var localDetection = new LocalSituationDetection();
            var remoteDetection = new RemoteSituationDetection();
            var syncMechanism = new SyncMechanism(localDetection, remoteDetection, queue, session.Object, storage);
            manager.AddEventHandler(syncMechanism);

            var remoteFolder = MockSessionUtil.CreateCmisFolder();
            var localFolder = new Mock<IDirectoryInfo>();
            var crawler = new Crawler(queue, remoteFolder.Object, localFolder.Object);
            manager.AddEventHandler(crawler);

            var permissionDenied = new GenericHandleDublicatedEventsFilter<PermissionDeniedEvent, ConfigChangedEvent>();
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
            var storage = GetInitializedStorage();
            var queue = CreateQueue(session, storage, observer);
            var myEvent = new Mock<ISyncEvent>();
            queue.AddEvent(myEvent.Object);
            queue.Run();
            Assert.That(observer.list.Count, Is.EqualTo(1));
        }

        [Test, Category("Fast")]
        public void RunStartNewSyncEvent ()
        {
            var storage = GetInitializedStorage();
            var session = new Mock<ISession>();
            session.SetupSessionDefaultValues();
            session.SetupChangeLogToken("default");
            var observer = new ObservableHandler();
            var queue = CreateQueue(session, storage, observer);
            queue.RunStartSyncEvent();
            Assert.That(observer.list.Count, Is.EqualTo(1));
            Assert.That(observer.list[0], Is.TypeOf(typeof(FullSyncCompletedEvent)));
        }

        [Test, Category("Fast")]
        public void RunFSEventDeleted ()
        {
            var storage = GetInitializedStorage();
            var path = new Mock<IFileInfo>();
            var name = "a";
            path.Setup(p => p.FullName ).Returns(Path.Combine(localRoot, name));
            string id = "id";
            //storage.AddLocalFile(path.Object, id);
            var mappedObject = new MappedObject();
            mappedObject.Type = MappedObjectType.Folder;
            mappedObject.RemoteObjectId = id;
            mappedObject.Name = name;
            storage.SaveMappedObject(mappedObject);
            
            var session = new Mock<ISession>();
            session.SetupSessionDefaultValues();
            session.SetupChangeLogToken("default");
            IDocument remote = MockSessionUtil.CreateRemoteObjectMock(null, id).Object;
            session.Setup(s => s.GetObject(id)).Returns(remote);
            var myEvent = new FSEvent(WatcherChangeTypes.Deleted, path.Object.FullName);
            var queue = CreateQueue(session, storage);
            queue.AddEvent(myEvent);
            queue.Run();

            session.Verify(f => f.Delete(It.Is<IObjectId>(i=>i.Id==id), true), Times.Once());
            Assert.That(storage.GetObjectByRemoteId(id), Is.Null);

        }

        [Test, Category("Fast")]
        public void ContentChangeIndicatesFolderDeletionOfExistingFolder ()
        {
            var storage = GetInitializedStorage();
            var name = "a";
            string path = Path.Combine(localRoot, name);
            string id = "1";
            Mock<IFileSystemInfoFactory> fsFactory = new Mock<IFileSystemInfoFactory>();
            var dirInfo = new Mock<IDirectoryInfo>();
            dirInfo.Setup(d => d.Exists).Returns(true);
            dirInfo.Setup(d => d.FullName).Returns(path);
            fsFactory.AddIDirectoryInfo(dirInfo.Object);
            var mappedObject = new MappedObject();
            mappedObject.Type = MappedObjectType.Folder;
            mappedObject.RemoteObjectId = id;
            mappedObject.Name = name;
            storage.SaveMappedObject(mappedObject);
            storage.ChangeLogToken = "oldtoken";

            Mock<ISession> session = MockSessionUtil.GetSessionMockReturningFolderChange(DotCMIS.Enums.ChangeType.Deleted, id);

            var queue = CreateQueue(session, storage, fsFactory.Object);
            queue.RunStartSyncEvent();
            dirInfo.Verify(d => d.Delete(true), Times.Once());
            Assert.That(storage.GetObjectByRemoteId(id), Is.Null);
        }

        [Test, Category("Fast")]
        public void ContentChangeIndicatesFolderCreation ()
        {
            string folderName = "folder";
            string parentId = "blafasel";
            string lastChangeToken = "changeToken";
            Mock<IFileSystemInfoFactory> fsFactory = new Mock<IFileSystemInfoFactory>();
            var dirInfo = fsFactory.AddDirectory(Path.Combine(localRoot, folderName));

            string id = "1";
            Mock<ISession> session = MockSessionUtil.GetSessionMockReturningFolderChange(DotCMIS.Enums.ChangeType.Created, id, Path.Combine(remoteRoot, folderName), parentId, lastChangeToken);
            var storage = GetInitializedStorage();
            storage.ChangeLogToken = "oldtoken";
            var queue = CreateQueue(session, storage, fsFactory.Object);
            queue.RunStartSyncEvent();
            dirInfo.Verify(d => d.Create(), Times.Once());
            var mappedObject = storage.GetObjectByRemoteId(id);
            Assert.That(mappedObject, Is.Not.Null);
            Assert.That(mappedObject.RemoteObjectId, Is.EqualTo(id), "RemoteObjectId incorrect");
            Assert.That(mappedObject.Name, Is.EqualTo(folderName), "Name incorrect");
            Assert.That(mappedObject.ParentId, Is.EqualTo(parentId), "ParentId incorrect");
            Assert.That(mappedObject.LastChangeToken, Is.EqualTo(lastChangeToken), "LastChangeToken incorrect");
            Assert.That(mappedObject.Type, Is.EqualTo(MappedObjectType.Folder), "Type incorrect");
        }
    }
}

