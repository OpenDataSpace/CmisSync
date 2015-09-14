//-----------------------------------------------------------------------
// <copyright file="EventManagerInitializerTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.QueueingTests {
    using System;
    using System.IO;

    using CmisSync.Lib;
    using CmisSync.Lib.Accumulator;
    using CmisSync.Lib.Cmis;
    using CmisSync.Lib.Config;
    using CmisSync.Lib.Consumer;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.FileTransmission;
    using CmisSync.Lib.Filter;
    using CmisSync.Lib.Producer.ContentChange;
    using CmisSync.Lib.Producer.Crawler;
    using CmisSync.Lib.Producer.Watcher;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.SelectiveIgnore;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;
    using DotCMIS.Enums;

    using Moq;

    using NUnit.Framework;

    using TestLibrary.IntegrationTests;
    using TestLibrary.TestUtils;

    [TestFixture, Category("Fast")]
    public class EventManagerInitializerTest {
        private ActivityListenerAggregator listener;
        private Mock<ISyncEventQueue> queue;
        private Mock<IMetaDataStorage> storage;

        [SetUp]
        public void SetUp() {
            this.queue = new Mock<ISyncEventQueue>();
            var manager = new TransmissionManager();
            this.listener = new ActivityListenerAggregator(Mock.Of<IActivityListener>(), manager);
            this.storage = new Mock<IMetaDataStorage>(MockBehavior.Strict);
            this.storage.Setup(s => s.SaveMappedObject(It.Is<IMappedObject>(m => m.ParentId == null)));
        }

        [Test]
        public void ConstructorTakesQueueAndManagerAndStorage() {
            new EventManagerInitializer(
                this.queue.Object,
                this.storage.Object,
                Mock.Of<IFileTransmissionStorage>(),
                Mock.Of<IIgnoredEntitiesStorage>(),
                CreateRepoInfo(),
                MockOfIFilterAggregatorUtil.CreateFilterAggregator().Object,
                this.listener,
                Mock.Of<ITransmissionFactory>());
        }

        [Test]
        public void ConstructorThrowsExceptionIfQueueIsNull() {
            Assert.Throws<ArgumentNullException>(() => new EventManagerInitializer(
                null,
                this.storage.Object,
                Mock.Of<IFileTransmissionStorage>(),
                Mock.Of<IIgnoredEntitiesStorage>(),
                CreateRepoInfo(),
                MockOfIFilterAggregatorUtil.CreateFilterAggregator().Object,
                this.listener,
                Mock.Of<ITransmissionFactory>()));
        }

        [Test]
        public void ConstructorThrowsExceptionIfStorageIsNull() {
            Assert.Throws<ArgumentNullException>(() => new EventManagerInitializer(
                Mock.Of<ISyncEventQueue>(),
                null,
                Mock.Of<IFileTransmissionStorage>(),
                Mock.Of<IIgnoredEntitiesStorage>(),
                CreateRepoInfo(),
                MockOfIFilterAggregatorUtil.CreateFilterAggregator().Object,
                this.listener,
                Mock.Of<ITransmissionFactory>()));
        }

        [Test]
        public void ConstructorThrowsExceptionIfFileTransmissionStorageIsNull() {
            Assert.Throws<ArgumentNullException>(() => new EventManagerInitializer(
                Mock.Of<ISyncEventQueue>(),
                this.storage.Object,
                null,
                Mock.Of<IIgnoredEntitiesStorage>(),
                CreateRepoInfo(),
                MockOfIFilterAggregatorUtil.CreateFilterAggregator().Object,
                this.listener,
                Mock.Of<ITransmissionFactory>()));
        }

        [Test]
        public void ConstructorThrowsExceptionIfRepoInfoIsNull() {
            Assert.Throws<ArgumentNullException>(() => new EventManagerInitializer(
                Mock.Of<ISyncEventQueue>(),
                this.storage.Object,
                Mock.Of<IFileTransmissionStorage>(),
                Mock.Of<IIgnoredEntitiesStorage>(),
                null,
                MockOfIFilterAggregatorUtil.CreateFilterAggregator().Object,
                this.listener,
                Mock.Of<ITransmissionFactory>()));
        }

        [Test]
        public void ConstructorThrowsExceptionIfTransmissionFactoryIsNull() {
            Assert.Throws<ArgumentNullException>(() => new EventManagerInitializer(
                Mock.Of<ISyncEventQueue>(),
                this.storage.Object,
                Mock.Of<IFileTransmissionStorage>(),
                Mock.Of<IIgnoredEntitiesStorage>(),
                CreateRepoInfo(),
                MockOfIFilterAggregatorUtil.CreateFilterAggregator().Object,
                this.listener,
                null));
        }

        [Test]
        public void IgnoresWrongEventsTest() {
            var queue = new Mock<ISyncEventQueue>();
            var handler = new EventManagerInitializer(
                queue.Object,
                this.storage.Object,
                Mock.Of<IFileTransmissionStorage>(),
                Mock.Of<IIgnoredEntitiesStorage>(),
                CreateRepoInfo(),
                MockOfIFilterAggregatorUtil.CreateFilterAggregator().Object,
                this.listener,
                Mock.Of<ITransmissionFactory>());
            var e = new Mock<ISyncEvent>();
            Assert.That(handler.Handle(e.Object), Is.False);
            this.queue.VerifyThatNoEventIsAdded();
        }

        [Test]
        public void RootFolderGetsAddedToStorage() {
            string id = "id";
            string token = "token";
            var manager = new Mock<ISyncEventManager>();
            this.RunSuccessfulLoginEvent(
                storage: this.storage.Object,
                manager: manager.Object,
                listener: this.listener,
                id: id,
                token: token);

            MappedObject rootObject = new MappedObject("/", id, MappedObjectType.Folder, null, token);
            this.storage.Verify(s => s.SaveMappedObject(It.Is<MappedObject>(m => AssertMappedObjectEqualExceptGUID(rootObject, m))));
            this.queue.Verify(q => q.AddEvent(It.Is<StartNextSyncEvent>(e => e.FullSyncRequested == true)), Times.Once());
            this.queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Once());
        }

        [Test]
        public void HandlersAddedInitiallyWithoutContentChanges() {
            var manager = new Mock<ISyncEventManager>();
            this.RunSuccessfulLoginEvent(
                storage: this.storage.Object,
                manager: manager.Object,
                listener: this.listener,
                changeEventSupported: false);

            manager.Verify(m => m.AddEventHandler(It.IsAny<SyncEventHandler>()), Times.Exactly(7));
            VerifyNonContentChangeHandlersAdded(manager, Times.Once());
            VerifyContentChangeHandlersAdded(manager, Times.Never());
            this.queue.Verify(q => q.AddEvent(It.Is<StartNextSyncEvent>(e => e.FullSyncRequested == true)), Times.Once());
            this.queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Once());
        }

        [Test]
        public void HandlersAddedInitiallyWithContentChanges() {
            var manager = new Mock<ISyncEventManager>();
            this.RunSuccessfulLoginEvent(
                storage: this.storage.Object,
                manager: manager.Object,
                listener: this.listener,
                changeEventSupported: true);

            manager.Verify(m => m.AddEventHandler(It.IsAny<SyncEventHandler>()), Times.Exactly(10));
            VerifyNonContentChangeHandlersAdded(manager, Times.Once());
            VerifyContentChangeHandlersAdded(manager, Times.Once());
            this.queue.Verify(q => q.AddEvent(It.Is<StartNextSyncEvent>(e => e.FullSyncRequested == true)), Times.Once());
            this.queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Once());
        }

        [Test]
        public void SelectiveIgnoreFilterAddedIsSupported() {
            var manager = new Mock<ISyncEventManager>();
            this.RunSuccessfulLoginEvent(
                storage: this.storage.Object,
                manager: manager.Object,
                listener: this.listener,
                changeEventSupported: true);

            manager.Verify(m => m.AddEventHandler(It.IsAny<SelectiveIgnoreFilter>()), Times.Once());
            manager.Verify(m => m.AddEventHandler(It.IsAny<SelectiveIgnoreEventTransformer>()), Times.Once());
            manager.Verify(m => m.AddEventHandler(It.IsAny<IgnoreFlagChangeDetection>()), Times.Once());
        }

        [Test]
        public void SelectiveIgnoreFilterAreNotAddedIfUnsupported() {
            var manager = new Mock<ISyncEventManager>();
            this.RunSuccessfulLoginEvent(
                storage: this.storage.Object,
                manager: manager.Object,
                listener: this.listener,
                changeEventSupported: true,
                supportsSelectiveIgnore: false);

            manager.Verify(m => m.AddEventHandler(It.IsAny<SelectiveIgnoreFilter>()), Times.Never());
            manager.Verify(m => m.AddEventHandler(It.IsAny<SelectiveIgnoreEventTransformer>()), Times.Never());
            manager.Verify(m => m.AddEventHandler(It.IsAny<IgnoreFlagChangeDetection>()), Times.Never());
        }

        [Test]
        public void ReinitiallizationContentChangeBeforeAndAfter() {
            var manager = new Mock<ISyncEventManager>();

            var handler = this.CreateStrategyInitializer(this.storage.Object, manager.Object, this.listener);

            var e = CreateNewSessionEvent(changeEventSupported: true);
            handler.Handle(e);

            manager.Verify(m => m.RemoveEventHandler(It.IsAny<SyncEventHandler>()), Times.Never());
            this.queue.Verify(q => q.AddEvent(It.Is<StartNextSyncEvent>(s => s.FullSyncRequested == true)), Times.Once());

            handler.Handle(e);

            manager.Verify(m => m.AddEventHandler(It.IsAny<SyncEventHandler>()), Times.Exactly(20));
            this.queue.Verify(q => q.AddEvent(It.Is<StartNextSyncEvent>(s => s.FullSyncRequested == true)), Times.Exactly(2));
            VerifyNonContentChangeHandlersAdded(manager, Times.Exactly(2));
            VerifyContentChangeHandlersAdded(manager, Times.Exactly(2));
            manager.Verify(m => m.RemoveEventHandler(It.IsAny<SyncEventHandler>()), Times.Exactly(10));
            VerifyNonContentChangeHandlersRemoved(manager, Times.Once());
            VerifyContentChangeHandlersRemoved(manager, Times.Once());
            this.queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Exactly(2));
        }

        [Test]
        public void ReinitiallizationContentChangeSupportAdded() {
            var manager = new Mock<ISyncEventManager>();

            var handler = this.CreateStrategyInitializer(this.storage.Object, manager.Object, this.listener);

            var e = CreateNewSessionEvent(changeEventSupported: false);
            handler.Handle(e);

            manager.Verify(m => m.RemoveEventHandler(It.IsAny<SyncEventHandler>()), Times.Never());
            this.queue.Verify(q => q.AddEvent(It.Is<StartNextSyncEvent>(s => s.FullSyncRequested == true)), Times.Once());

            e = CreateNewSessionEvent(changeEventSupported: true);
            handler.Handle(e);

            manager.Verify(m => m.AddEventHandler(It.IsAny<SyncEventHandler>()), Times.Exactly(17));
            this.queue.Verify(q => q.AddEvent(It.Is<StartNextSyncEvent>(s => s.FullSyncRequested == true)), Times.Exactly(2));
            VerifyNonContentChangeHandlersAdded(manager, Times.Exactly(2));
            VerifyContentChangeHandlersAdded(manager, Times.Exactly(1));
            manager.Verify(m => m.RemoveEventHandler(It.IsAny<SyncEventHandler>()), Times.Exactly(7));
            VerifyNonContentChangeHandlersRemoved(manager, Times.Exactly(1));
            VerifyContentChangeHandlersRemoved(manager, Times.Never());
            this.queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Exactly(2));
        }

        private static RepoInfo CreateRepoInfo() {
            return new RepoInfo {
                Address = new Uri("http://example.com"),
                LocalPath = ITUtils.GetConfig()[1].ToString(),
                RemotePath = "/"
            };
        }

        private static SuccessfulLoginEvent CreateNewSessionEvent(
            bool changeEventSupported,
            bool supportsSelectiveIgnore = true,
            bool pwcIsSupported = true,
            string id = "i",
            string token = "t")
        {
            var session = new Mock<ISession>(MockBehavior.Strict).SetupCreateOperationContext().Object;
            var remoteObject = new Mock<IFolder>().SetupId(id).SetupChangeToken(token).SetupPath("path").Object;
            return new SuccessfulLoginEvent(new Uri("http://example.com"), session, remoteObject, pwcIsSupported, supportsSelectiveIgnore, changeEventSupported);
        }

        private static void VerifyNonContentChangeHandlersAdded(Mock<ISyncEventManager> manager, Times times) {
            manager.Verify(m => m.AddEventHandler(It.IsAny<DescendantsCrawler>()), times);
            manager.Verify(m => m.AddEventHandler(It.IsAny<RemoteObjectFetcher>()), times);
            manager.Verify(m => m.AddEventHandler(It.IsAny<SyncMechanism>()), times);
            manager.Verify(m => m.AddEventHandler(It.IsAny<RemoteObjectMovedOrRenamedAccumulator>()), times);
        }

        private static void VerifyContentChangeHandlersAdded(Mock<ISyncEventManager> manager, Times times) {
            manager.Verify(m => m.AddEventHandler(It.IsAny<ContentChanges>()), times);
            manager.Verify(m => m.AddEventHandler(It.IsAny<ContentChangeEventAccumulator>()), times);
            manager.Verify(m => m.AddEventHandler(It.IsAny<IgnoreAlreadyHandledContentChangeEventsFilter>()), times);
        }

        private static void VerifyNonContentChangeHandlersRemoved(Mock<ISyncEventManager> manager, Times times) {
            manager.Verify(m => m.RemoveEventHandler(It.IsAny<DescendantsCrawler>()), times);
            manager.Verify(m => m.RemoveEventHandler(It.IsAny<RemoteObjectFetcher>()), times);
            manager.Verify(m => m.RemoveEventHandler(It.IsAny<SyncMechanism>()), times);
            manager.Verify(m => m.RemoveEventHandler(It.IsAny<RemoteObjectMovedOrRenamedAccumulator>()), times);
        }

        private static void VerifyContentChangeHandlersRemoved(Mock<ISyncEventManager> manager, Times times) {
            manager.Verify(m => m.RemoveEventHandler(It.IsAny<ContentChanges>()), times);
            manager.Verify(m => m.RemoveEventHandler(It.IsAny<ContentChangeEventAccumulator>()), times);
            manager.Verify(m => m.RemoveEventHandler(It.IsAny<IgnoreAlreadyHandledContentChangeEventsFilter>()), times);
        }

        private static bool AssertMappedObjectEqualExceptGUID(IMappedObject expected, IMappedObject actual) {
            Assert.That(actual.ParentId, Is.EqualTo(expected.ParentId));
            Assert.That(actual.Type, Is.EqualTo(expected.Type));
            Assert.That(actual.RemoteObjectId, Is.EqualTo(expected.RemoteObjectId));
            Assert.That(actual.LastChangeToken, Is.EqualTo(expected.LastChangeToken));
            Assert.That(actual.LastRemoteWriteTimeUtc, Is.EqualTo(expected.LastRemoteWriteTimeUtc));
            Assert.That(actual.LastLocalWriteTimeUtc, Is.EqualTo(expected.LastLocalWriteTimeUtc));
            Assert.That(actual.LastChecksum, Is.EqualTo(expected.LastChecksum));
            Assert.That(actual.ChecksumAlgorithmName, Is.EqualTo(expected.ChecksumAlgorithmName));
            Assert.That(actual.Name, Is.EqualTo(expected.Name));
            Assert.That(actual.Guid, Is.Not.EqualTo(Guid.Empty));
            return true;
        }

        private EventManagerInitializer CreateStrategyInitializer(IMetaDataStorage storage, ISyncEventManager manager, ActivityListenerAggregator listener) {
            this.queue.Setup(s => s.EventManager).Returns(manager);
            return new EventManagerInitializer(
                this.queue.Object,
                storage,
                Mock.Of<IFileTransmissionStorage>(),
                Mock.Of<IIgnoredEntitiesStorage>(),
                CreateRepoInfo(),
                MockOfIFilterAggregatorUtil.CreateFilterAggregator().Object,
                listener,
                Mock.Of<ITransmissionFactory>());
        }

        private void RunSuccessfulLoginEvent(
            IMetaDataStorage storage,
            ISyncEventManager manager,
            ActivityListenerAggregator listener,
            bool changeEventSupported = false,
            bool supportsSelectiveIgnore = true,
            bool pwcIsSupported = true,
            string id = "i",
            string token = "t")
        {
            var e = CreateNewSessionEvent(changeEventSupported, supportsSelectiveIgnore, pwcIsSupported, id, token);

            var handler = this.CreateStrategyInitializer(storage, manager, listener);

            Assert.That(handler.Handle(e), Is.True);
        }
    }
}