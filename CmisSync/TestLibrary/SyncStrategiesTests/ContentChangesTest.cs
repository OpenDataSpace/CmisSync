//-----------------------------------------------------------------------
// <copyright file="ContentChangesTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.SyncStrategiesTests
{
    using System;
    using System.Collections.Generic;

    using CmisSync.Lib;
    using CmisSync.Lib.Config;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Storage;
    using CmisSync.Lib.Sync.Strategy;

    using DotCMIS.Binding.Services;
    using DotCMIS.Client;
    using DotCMIS.Data;
    using DotCMIS.Data.Extensions;

    using Moq;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture]
    public class ContentChangesTest
    {
        private readonly bool isPropertyChangesSupported = false;
        private readonly string changeLogToken = "token";
        private readonly string latestChangeLogToken = "latestChangeLogToken";
        private readonly string repoId = "repoId";
        private readonly int maxNumberOfContentChanges = 1000;

        [TestFixtureSetUp]
        public void ClassInit()
        {
            log4net.Config.XmlConfigurator.Configure(ConfigManager.CurrentConfig.GetLog4NetConfig());
        }

        [Test, Category("Fast"), Category("ContentChange")]
        public void ConstructorWithVaildEntriesTest()
        {
            var storage = new Mock<IMetaDataStorage>();
            var queue = new Mock<ISyncEventQueue>();
            var session = new Mock<ISession>();
            bool isPropertyChangesSupported = true;
            new ContentChanges(session.Object, storage.Object, queue.Object);
            new ContentChanges(session.Object, storage.Object, queue.Object, this.maxNumberOfContentChanges);
            new ContentChanges(session.Object, storage.Object, queue.Object, this.maxNumberOfContentChanges, isPropertyChangesSupported);
            new ContentChanges(session.Object, storage.Object, queue.Object, isPropertyChangesSupported: true);
            new ContentChanges(session.Object, storage.Object, queue.Object, isPropertyChangesSupported: false);
        }

        [Test, Category("Fast"), Category("ContentChange")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorFailsIfDbIsNull()
        {
            var queue = new Mock<ISyncEventQueue>();
            var session = new Mock<ISession>();
            new ContentChanges(session.Object, null, queue.Object);
        }

        [Test, Category("Fast"), Category("ContentChange")]
        [ExpectedException(typeof(ArgumentException))]
        public void ConstructorThrowsExceptionIfPagingLimitIsNegative()
        {
            new ContentChanges(Mock.Of<ISession>(), Mock.Of<IMetaDataStorage>(), Mock.Of<ISyncEventQueue>(), -1);
        }

        [Test, Category("Fast"), Category("ContentChange")]
        [ExpectedException(typeof(ArgumentException))]
        public void ConstructorThrowsExceptionIfPagingLimitIsZero()
        {
            new ContentChanges(Mock.Of<ISession>(), Mock.Of<IMetaDataStorage>(), Mock.Of<ISyncEventQueue>(), 0);
        }

        [Test, Category("Fast"), Category("ContentChange")]
        [ExpectedException(typeof(ArgumentException))]
        public void ConstructorThrowsExceptionIfPagingLimitIsOne()
        {
            new ContentChanges(Mock.Of<ISession>(), Mock.Of<IMetaDataStorage>(), Mock.Of<ISyncEventQueue>(), 1);
        }

        [Test, Category("Fast"), Category("ContentChange")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorFailsIfSessionIsNull()
        {
            var storage = new Mock<IMetaDataStorage>();
            var queue = new Mock<ISyncEventQueue>();
            new ContentChanges(null, storage.Object, queue.Object);
        }

        [Test, Category("Fast"), Category("ContentChange")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorFailsIfQueueIsNull()
        {
            var storage = new Mock<IMetaDataStorage>();
            var session = new Mock<ISession>().Object;
            new ContentChanges(session, storage.Object, null);
        }

        [Test, Category("Fast"), Category("ContentChange")]
        public void IgnoresWrongEvent()
        {
            var storage = new Mock<IMetaDataStorage>();
            var queue = new Mock<ISyncEventQueue>();
            var session = new Mock<ISession>();
            var changes = new ContentChanges(session.Object, storage.Object, queue.Object);
            var startSyncEvent = new Mock<ISyncEvent>().Object;
            Assert.IsFalse(changes.Handle(startSyncEvent));
        }

        [Test, Category("Fast"), Category("ContentChange")]
        public void RetrunFalseOnError() {
            // TODO: this might not be the best behavior, this test verifies the current implementation not the desired one
            var storage = new Mock<IMetaDataStorage>();
            var queue = new Mock<ISyncEventQueue>();
            var session = new Mock<ISession>();
            session.Setup(x => x.Binding).Throws(new Exception("SOME EXCEPTION"));
            var changes = new ContentChanges(session.Object, storage.Object, queue.Object);
            var wrongEvent = new Mock<ISyncEvent>().Object;
            Assert.IsFalse(changes.Handle(wrongEvent));
        }

        [Test, Category("Fast"), Category("ContentChange")]
        public void HandleFullSyncCompletedEvent()
        {
            var startSyncEvent = new StartNextSyncEvent(false);
            startSyncEvent.LastTokenOnServer = this.changeLogToken;
            var completedEvent = new FullSyncCompletedEvent(startSyncEvent);
            var storage = new Mock<IMetaDataStorage>();
            storage.SetupProperty(db => db.ChangeLogToken);
            var queue = new Mock<ISyncEventQueue>();
            var session = new Mock<ISession>();
            var changes = new ContentChanges(session.Object, storage.Object, queue.Object);
            Assert.IsFalse(changes.Handle(completedEvent));
            storage.VerifySet(db => db.ChangeLogToken = this.changeLogToken);
            Assert.AreEqual(this.changeLogToken, storage.Object.ChangeLogToken);
        }

        [Test, Category("Fast"), Category("ContentChange")]
        public void HandleStartSyncEventOnNoRemoteChange()
        {
            var startSyncEvent = new StartNextSyncEvent(false);
            var session = new Mock<ISession>();
            session.SetupSessionDefaultValues();
            session.Setup(s => s.Binding.GetRepositoryService().GetRepositoryInfo(this.repoId, null).LatestChangeLogToken).Returns(this.changeLogToken);
            var storage = new Mock<IMetaDataStorage>();
            storage.Setup(db => db.ChangeLogToken).Returns(this.changeLogToken);
            var queue = new Mock<ISyncEventQueue>();
            var changes = new ContentChanges(session.Object, storage.Object, queue.Object);
            Assert.IsTrue(changes.Handle(startSyncEvent));
        }

        [Test, Category("Fast"), Category("ContentChange")]
        public void ExecuteCrawlSyncOnNoLocalTokenAvailable()
        {
            var startSyncEvent = new StartNextSyncEvent(false);
            var session = new Mock<ISession>();
            session.SetupSessionDefaultValues();
            session.Setup(s => s.Binding.GetRepositoryService().GetRepositoryInfo(this.repoId, null).LatestChangeLogToken).Returns(this.changeLogToken);
            var storage = new Mock<IMetaDataStorage>();
            storage.Setup(db => db.ChangeLogToken).Returns((string)null);
            var queue = new Mock<ISyncEventQueue>();
            var changes = new ContentChanges(session.Object, storage.Object, queue.Object);

            Assert.IsTrue(changes.Handle(startSyncEvent));
            queue.Verify(q => q.AddEvent(It.Is<StartNextSyncEvent>(e => e.FullSyncRequested == true)), Times.Once());
        }

        [Test, Category("Fast"), Category("ContentChange")]
        public void IgnoreCrawlSyncEvent()
        {
            var start = new StartNextSyncEvent(true);
            var repositoryService = new Mock<IRepositoryService>();
            repositoryService.Setup(r => r.GetRepositoryInfos(null)).Returns((IList<IRepositoryInfo>)null);
            repositoryService.Setup(r => r.GetRepositoryInfo(It.IsAny<string>(), It.IsAny<IExtensionsData>()).LatestChangeLogToken).Returns(this.latestChangeLogToken);
            var session = new Mock<ISession>();
            session.Setup(s => s.Binding.GetRepositoryService()).Returns(repositoryService.Object);
            session.Setup(s => s.RepositoryInfo.Id).Returns(this.repoId);
            var storage = new Mock<IMetaDataStorage>();
            storage.Setup(db => db.ChangeLogToken).Returns(this.changeLogToken);
            var queue = new Mock<ISyncEventQueue>();
            var changes = new ContentChanges(session.Object, storage.Object, queue.Object);
            Assert.IsFalse(changes.Handle(start));
            Assert.IsNull(start.LastTokenOnServer);
        }

        [Test, Category("Fast"), Category("ContentChange")]
        public void ExtendCrawlSyncEvent()
        {
            var start = new StartNextSyncEvent(true);
            var repositoryService = new Mock<IRepositoryService>();
            repositoryService.Setup(r => r.GetRepositoryInfos(null)).Returns((IList<IRepositoryInfo>)null);
            repositoryService.Setup(r => r.GetRepositoryInfo(It.IsAny<string>(), It.IsAny<IExtensionsData>()).LatestChangeLogToken).Returns(this.latestChangeLogToken);
            var session = new Mock<ISession>();
            session.Setup(s => s.Binding.GetRepositoryService()).Returns(repositoryService.Object);
            session.Setup(s => s.RepositoryInfo.Id).Returns(this.repoId);
            var manager = new Mock<ISyncEventManager>().Object;
            var storage = new Mock<IMetaDataStorage>();
            storage.Setup(db => db.ChangeLogToken).Returns((string)null);
            var queue = new Mock<ISyncEventQueue>();
            var changes = new ContentChanges(session.Object, storage.Object, queue.Object);
            Assert.IsFalse(changes.Handle(start));
            Assert.AreEqual(this.latestChangeLogToken, start.LastTokenOnServer);
        }

        [Test, Category("Fast"), Category("ContentChange")]
        public void GivesCorrectContentChangeEvent()
        {
            ContentChangeEvent contentChangeEvent = null;
            var queue = new Mock<ISyncEventQueue>();
            queue.Setup(q => q.AddEvent(It.IsAny<ContentChangeEvent>())).Callback((ISyncEvent f) => {
                contentChangeEvent = f as ContentChangeEvent;
            });
            string id = "myId";

            Mock<IMetaDataStorage> storage = MockMetaDataStorageUtil.GetMetaStorageMockWithToken();
            var session = MockSessionUtil.PrepareSessionMockForSingleChange(DotCMIS.Enums.ChangeType.Created, id);
            var changes = new ContentChanges(session.Object, storage.Object, queue.Object, this.maxNumberOfContentChanges, this.isPropertyChangesSupported);

            var startSyncEvent = new StartNextSyncEvent(false);
            Assert.IsTrue(changes.Handle(startSyncEvent));

            queue.Verify(foo => foo.AddEvent(It.IsAny<ContentChangeEvent>()), Times.Once());
            Assert.That(contentChangeEvent.Type, Is.EqualTo(DotCMIS.Enums.ChangeType.Created));
            Assert.That(contentChangeEvent.ObjectId, Is.EqualTo(id));
        }

        [Test, Category("Fast"), Category("ContentChange")]
        public void PagingTest()
        {
            var queue = new Mock<ISyncEventQueue>();

            Mock<IMetaDataStorage> storage = MockMetaDataStorageUtil.GetMetaStorageMockWithToken();

            Mock<ISession> session = MockSessionUtil.GetSessionMockReturning3Changesin2Batches();

            var startSyncEvent = new StartNextSyncEvent(false);
            var changes = new ContentChanges(session.Object, storage.Object, queue.Object, this.maxNumberOfContentChanges, this.isPropertyChangesSupported);

            Assert.IsTrue(changes.Handle(startSyncEvent));
            queue.Verify(foo => foo.AddEvent(It.IsAny<ContentChangeEvent>()), Times.Exactly(3));
        }

        [Test, Category("Fast"), Category("ContentChange")]
        public void IgnoreDuplicatedContentChangesEventTestCreated()
        {
            var queue = new Mock<ISyncEventQueue>();

            Mock<IMetaDataStorage> storage = MockMetaDataStorageUtil.GetMetaStorageMockWithToken();

            Mock<ISession> session = MockSessionUtil.GetSessionMockReturning3Changesin2Batches(DotCMIS.Enums.ChangeType.Created, true);

            var startSyncEvent = new StartNextSyncEvent(false);
            var changes = new ContentChanges(session.Object, storage.Object, queue.Object, this.maxNumberOfContentChanges, this.isPropertyChangesSupported);

            Assert.IsTrue(changes.Handle(startSyncEvent));
            queue.Verify(foo => foo.AddEvent(It.IsAny<ContentChangeEvent>()), Times.Exactly(3));
        }

        [Test, Category("Fast"), Category("ContentChange")]
        public void IgnoreDuplicatedContentChangesEventTestDeleted()
        {
            var queue = new Mock<ISyncEventQueue>();

            Mock<IMetaDataStorage> storage = MockMetaDataStorageUtil.GetMetaStorageMockWithToken();

            Mock<ISession> session = MockSessionUtil.GetSessionMockReturning3Changesin2Batches(DotCMIS.Enums.ChangeType.Deleted, true);

            var startSyncEvent = new StartNextSyncEvent(false);
            var changes = new ContentChanges(session.Object, storage.Object, queue.Object, this.maxNumberOfContentChanges, this.isPropertyChangesSupported);

            Assert.IsTrue(changes.Handle(startSyncEvent));
            queue.Verify(foo => foo.AddEvent(It.IsAny<ContentChangeEvent>()), Times.Exactly(3));
        }

        [Test, Category("Fast"), Category("ContentChange")]
        public void DropAllStartNextSyncEventsInQueueWhichAreAvailableUntilRequestIsDone()
        {
            var queue = new Mock<ISyncEventQueue>();
            ISyncEvent resetToken = null;
            queue.Setup(q => q.AddEvent(It.Is<ISyncEvent>(e => !(e is ContentChangeEvent)))).Callback<ISyncEvent>(e => resetToken = e);
            var storage = new Mock<IMetaDataStorage>();
            storage.SetupProperty(s => s.ChangeLogToken, "lastToken");

            Mock<ISession> session = MockSessionUtil.GetSessionMockReturning3Changesin2Batches();

            var startSyncEvent = new StartNextSyncEvent(false);
            var changes = new ContentChanges(session.Object, storage.Object, queue.Object, this.maxNumberOfContentChanges, this.isPropertyChangesSupported);

            // Start the first regular sync
            Assert.That(changes.Handle(startSyncEvent), Is.True);
            Assert.That(resetToken, Is.Not.Null);
            queue.Verify(foo => foo.AddEvent(It.IsAny<ISyncEvent>()), Times.Exactly(4));

            // Drop next incomming start sync events
            Assert.That(changes.Handle(startSyncEvent), Is.True);
            Assert.That(changes.Handle(startSyncEvent), Is.True);

            // Handle reset event
            Assert.That(changes.Handle(resetToken), Is.True);

            // Executes next sync and passes a new reset token to queue
            Assert.That(changes.Handle(startSyncEvent), Is.True);
            queue.Verify(foo => foo.AddEvent(It.IsAny<ContentChangeEvent>()), Times.Exactly(3));
            queue.Verify(foo => foo.AddEvent(It.IsAny<ISyncEvent>()), Times.Exactly(5));
        }
    }
}