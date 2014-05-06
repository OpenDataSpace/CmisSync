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

namespace TestLibrary.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using CmisSync.Lib;
    using CmisSync.Lib.Config;
    using CmisSync.Lib.Data;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Events.Filter;
    using CmisSync.Lib.Storage;
    using CmisSync.Lib.Sync.Strategy;

    using DBreeze;

    using DotCMIS.Binding.Services;
    using DotCMIS.Client;
    using DotCMIS.Data;
    using DotCMIS.Data.Extensions;

    using Moq;

    using Newtonsoft.Json;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    using Strategy = CmisSync.Lib.Sync.Strategy;

    [TestFixture]
    public class AllHandlersIT
    {
        private readonly string localRoot = Path.GetTempPath();
        private readonly string remoteRoot = "remoteroot";

        private readonly bool isPropertyChangesSupported = false;
        private readonly int maxNumberOfContentChanges = 1000;

        private DBreezeEngine engine;

        [TestFixtureSetUp]
        public void ClassInit()
        {
            log4net.Config.XmlConfigurator.Configure(ConfigManager.CurrentConfig.GetLog4NetConfig());

            // Use Newtonsoft.Json as Serializator
            DBreeze.Utils.CustomSerializator.Serializator = JsonConvert.SerializeObject; 
            DBreeze.Utils.CustomSerializator.Deserializator = JsonConvert.DeserializeObject;
        }

        [SetUp]
        public void SetupEngine()
        {
            this.engine = new DBreezeEngine(new DBreezeConfiguration { Storage = DBreezeConfiguration.eStorage.MEMORY });
        }
        
        [TearDown]
        public void DestroyEngine() 
        {
            this.engine.Dispose();
        }

        [Test, Category("Fast")]
        public void RunFakeEvent()
        {
            var session = new Mock<ISession>();
            var observer = new ObservableHandler();
            var storage = this.GetInitializedStorage();
            var queue = this.CreateQueue(session, storage, observer);
            var myEvent = new Mock<ISyncEvent>();
            queue.AddEvent(myEvent.Object);
            queue.Run();
            Assert.That(observer.list.Count, Is.EqualTo(1));
        }

        [Test, Category("Fast")]
        public void RunStartNewSyncEvent()
        {
            var storage = this.GetInitializedStorage();
            var session = new Mock<ISession>();
            session.SetupSessionDefaultValues();
            session.SetupChangeLogToken("default");
            var observer = new ObservableHandler();
            var queue = this.CreateQueue(session, storage, observer);
            queue.RunStartSyncEvent();
            Assert.That(observer.list.Count, Is.EqualTo(1));
            Assert.That(observer.list[0], Is.TypeOf(typeof(FullSyncCompletedEvent)));
        }

        [Test, Category("Fast")]
        public void RunFSEventDeleted()
        {
            var storage = this.GetInitializedStorage();
            var path = new Mock<IFileInfo>();
            var name = "a";
            path.Setup(p => p.FullName).Returns(Path.Combine(this.localRoot, name));
            string id = "id";

            // storage.AddLocalFile(path.Object, id);
            var mappedObject = new MappedObject(name, id, MappedObjectType.Folder, null, null);
            storage.SaveMappedObject(mappedObject);
            
            var session = new Mock<ISession>();
            session.SetupSessionDefaultValues();
            session.SetupChangeLogToken("default");
            IDocument remote = MockSessionUtil.CreateRemoteObjectMock(null, id).Object;
            session.Setup(s => s.GetObject(id)).Returns(remote);
            var myEvent = new FSEvent(WatcherChangeTypes.Deleted, path.Object.FullName);
            var queue = this.CreateQueue(session, storage);
            queue.AddEvent(myEvent);
            queue.Run();

            session.Verify(f => f.Delete(It.Is<IObjectId>(i => i.Id == id), true), Times.Once());
            Assert.That(storage.GetObjectByRemoteId(id), Is.Null);
        }

        [Test, Category("Fast")]
        public void ContentChangeIndicatesFolderDeletionOfExistingFolder()
        {
            var storage = this.GetInitializedStorage();
            var name = "a";
            string path = Path.Combine(this.localRoot, name);
            string id = "1";
            Mock<IFileSystemInfoFactory> fsFactory = new Mock<IFileSystemInfoFactory>();
            var dirInfo = new Mock<IDirectoryInfo>();
            dirInfo.Setup(d => d.Exists).Returns(true);
            dirInfo.Setup(d => d.FullName).Returns(path);
            fsFactory.AddIDirectoryInfo(dirInfo.Object);
            var mappedObject = new MappedObject(name, id, MappedObjectType.Folder, null, null);
            storage.SaveMappedObject(mappedObject);
            storage.ChangeLogToken = "oldtoken";

            Mock<ISession> session = MockSessionUtil.GetSessionMockReturningFolderChange(DotCMIS.Enums.ChangeType.Deleted, id);

            var queue = this.CreateQueue(session, storage, fsFactory.Object);
            queue.RunStartSyncEvent();
            dirInfo.Verify(d => d.Delete(true), Times.Once());
            Assert.That(storage.GetObjectByRemoteId(id), Is.Null);
        }

        [Test, Category("Fast")]
        public void ContentChangeIndicatesFolderRenameOfExistingFolder()
        {
            var storage = this.GetInitializedStorage();
            string name = "a";
            string newName = "b";
            string parentId = "parentId";
            string path = Path.Combine(this.localRoot, name);
            string newPath = Path.Combine(this.localRoot, newName);
            string id = "1";
            string lastChangeToken = "changeToken";
            Mock<IFileSystemInfoFactory> fsFactory = new Mock<IFileSystemInfoFactory>();
            var dirInfo = new Mock<IDirectoryInfo>();
            dirInfo.Setup(d => d.Exists).Returns(true);
            dirInfo.Setup(d => d.FullName).Returns(path);
            dirInfo.Setup(d => d.Parent).Returns(Mock.Of<IDirectoryInfo>(r => r.FullName == this.localRoot));
            fsFactory.AddIDirectoryInfo(dirInfo.Object);
            var mappedObject = new MappedObject(name, id, MappedObjectType.Folder, null, null);
            storage.SaveMappedObject(mappedObject);
            storage.ChangeLogToken = "oldChangeToken";

            Mock<ISession> session = MockSessionUtil.GetSessionMockReturningFolderChange(DotCMIS.Enums.ChangeType.Updated, id, Path.Combine(this.remoteRoot, newName), parentId, lastChangeToken);

            var queue = this.CreateQueue(session, storage, fsFactory.Object);
            queue.RunStartSyncEvent();
            dirInfo.Verify(d => d.MoveTo(It.Is<string>(p => p.Equals(newPath))), Times.Once());
            Assert.That(storage.GetObjectByRemoteId(id), Is.Not.Null);
            Assert.That(storage.GetObjectByRemoteId(id).Name, Is.EqualTo(newName));
            Assert.That(storage.GetObjectByLocalPath(Mock.Of<IDirectoryInfo>(d => d.FullName == path)), Is.Null);
            Assert.That(storage.GetObjectByLocalPath(Mock.Of<IDirectoryInfo>(d => d.FullName == newPath)), Is.Not.Null);
        }

        [Test, Category("Fast")]
        public void ContentChangeIndicatesFolderCreation()
        {
            string folderName = "folder";
            string parentId = "blafasel";
            string lastChangeToken = "changeToken";
            Mock<IFileSystemInfoFactory> fsFactory = new Mock<IFileSystemInfoFactory>();
            var dirInfo = fsFactory.AddDirectory(Path.Combine(this.localRoot, folderName));

            string id = "1";
            Mock<ISession> session = MockSessionUtil.GetSessionMockReturningFolderChange(DotCMIS.Enums.ChangeType.Created, id, Path.Combine(this.remoteRoot, folderName), parentId, lastChangeToken);
            var storage = this.GetInitializedStorage();
            storage.ChangeLogToken = "oldtoken";
            var queue = this.CreateQueue(session, storage, fsFactory.Object);
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

        private SingleStepEventQueue CreateQueue(Mock<ISession> session, IMetaDataStorage storage) 
        {
            return this.CreateQueue(session, storage, new ObservableHandler());
        }

        private SingleStepEventQueue CreateQueue(Mock<ISession> session, IMetaDataStorage storage, IFileSystemInfoFactory fsFactory) {
            return this.CreateQueue(session, storage, new ObservableHandler(), fsFactory);
        }

        private IMetaDataStorage GetInitializedStorage()
        {
            IPathMatcher matcher = new PathMatcher(this.localRoot, this.remoteRoot);
            return new MetaDataStorage(this.engine, matcher);
        }

        private SingleStepEventQueue CreateQueue(Mock<ISession> session, IMetaDataStorage storage, ObservableHandler observer, IFileSystemInfoFactory fsFactory = null)
        {
            var manager = new SyncEventManager();
            SingleStepEventQueue queue = new SingleStepEventQueue(manager);

            manager.AddEventHandler(observer);

            var changes = new ContentChanges(session.Object, storage, queue, this.maxNumberOfContentChanges, this.isPropertyChangesSupported);
            manager.AddEventHandler(changes);

            var transformer = new ContentChangeEventTransformer(queue, storage, fsFactory);
            manager.AddEventHandler(transformer);

            var ccaccumulator = new ContentChangeEventAccumulator(session.Object, queue);
            manager.AddEventHandler(ccaccumulator);

            var remoteFetcher = new RemoteObjectFetcher(session.Object, storage);
            manager.AddEventHandler(remoteFetcher);

            var localFetcher = new LocalObjectFetcher(storage.Matcher, fsFactory);
            manager.AddEventHandler(localFetcher);

            var watcher = new Mock<Strategy.Watcher>(queue) { CallBase = true };
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
    }
}