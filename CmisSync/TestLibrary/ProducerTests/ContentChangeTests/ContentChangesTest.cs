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

namespace TestLibrary.ProducerTests.ContentChangeTests {
    using System;
    using System.Collections.Generic;

    using CmisSync.Lib.Config;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Producer.ContentChange;
    using CmisSync.Lib.Producer.Watcher;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.Database;

    using DotCMIS.Binding.Services;
    using DotCMIS.Client;
    using DotCMIS.Data;
    using DotCMIS.Data.Extensions;

    using Moq;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture, Category("Fast"), Category("ContentChange")]
    public class ContentChangesTest : IsTestWithConfiguredLog4Net {
        private readonly bool isPropertyChangesSupported = false;
        private readonly string changeLogToken = "token";
        private readonly string latestChangeLogToken = "latestChangeLogToken";
        private readonly string repoId = "repoId";
        private readonly int maxNumberOfContentChanges = 1000;

        [Test]
        public void ConstructorWithVaildEntriesTest() {
            var storage = new Mock<IMetaDataStorage>(MockBehavior.Strict).Object;
            var queue = new Mock<ISyncEventQueue>(MockBehavior.Strict).Object;
            var session = new Mock<ISession>(MockBehavior.Strict).Object;
            bool isPropertyChangesSupported = true;
            new ContentChanges(session, storage, queue);
            new ContentChanges(session, storage, queue, this.maxNumberOfContentChanges);
            new ContentChanges(session, storage, queue, this.maxNumberOfContentChanges, isPropertyChangesSupported);
            new ContentChanges(session, storage, queue, isPropertyChangesSupported: true);
            new ContentChanges(session, storage, queue, isPropertyChangesSupported: false);
        }

        [Test]
        public void ConstructorFailsIfDbIsNull() {
            var queue = new Mock<ISyncEventQueue>(MockBehavior.Strict).Object;
            var session = new Mock<ISession>(MockBehavior.Strict).Object;
            Assert.Throws<ArgumentNullException>(() => new ContentChanges(session, null, queue));
        }

        [Test]
        public void ConstructorThrowsExceptionIfPagingLimitIsWrong(
            [Values(-1, 0, 1)]int wrongLimit)
        {
            var storage = new Mock<IMetaDataStorage>(MockBehavior.Strict).Object;
            var queue = new Mock<ISyncEventQueue>(MockBehavior.Strict).Object;
            var session = new Mock<ISession>(MockBehavior.Strict).Object;
            Assert.Throws<ArgumentException>(() => new ContentChanges(session, storage, queue, wrongLimit));
        }

        [Test]
        public void ConstructorFailsIfSessionIsNull() {
            var storage = new Mock<IMetaDataStorage>(MockBehavior.Strict).Object;
            var queue = new Mock<ISyncEventQueue>(MockBehavior.Strict).Object;
            Assert.Throws<ArgumentNullException>(() => new ContentChanges(null, storage, queue));
        }

        [Test]
        public void ConstructorFailsIfQueueIsNull() {
            var storage = new Mock<IMetaDataStorage>(MockBehavior.Strict).Object;
            var session = new Mock<ISession>(MockBehavior.Strict).Object;
            Assert.Throws<ArgumentNullException>(() => new ContentChanges(session, storage, null));
        }

        [Test]
        public void IgnoresWrongEvent() {
            var storage = new Mock<IMetaDataStorage>();
            var queue = new Mock<ISyncEventQueue>();
            var session = new Mock<ISession>();
            var changes = new ContentChanges(session.Object, storage.Object, queue.Object);
            var startSyncEvent = new Mock<ISyncEvent>().Object;
            Assert.That(changes.Handle(startSyncEvent), Is.False);
        }

        [Test]
        public void ReturnFalseOnError() {
            var storage = new Mock<IMetaDataStorage>();
            var queue = new Mock<ISyncEventQueue>();
            var session = new Mock<ISession>(MockBehavior.Strict);
            var changes = new ContentChanges(session.Object, storage.Object, queue.Object);
            var wrongEvent = new Mock<ISyncEvent>().Object;

            Assert.That(changes.Handle(wrongEvent), Is.False);
        }

        [Test]
        public void HandleFullSyncCompletedEvent() {
            var startSyncEvent = new StartNextSyncEvent(false);
            startSyncEvent.LastTokenOnServer = this.changeLogToken;
            var completedEvent = new FullSyncCompletedEvent(startSyncEvent);
            var storage = new Mock<IMetaDataStorage>();
            storage.SetupProperty(db => db.ChangeLogToken);
            var queue = new Mock<ISyncEventQueue>();
            var session = new Mock<ISession>();
            var changes = new ContentChanges(session.Object, storage.Object, queue.Object);

            Assert.That(changes.Handle(completedEvent), Is.False);

            storage.VerifySet(db => db.ChangeLogToken = this.changeLogToken);
            Assert.That(storage.Object.ChangeLogToken, Is.EqualTo(this.changeLogToken));
        }

        [Test]
        public void HandleStartSyncEventOnNoRemoteChange() {
            var startSyncEvent = new StartNextSyncEvent(false);
            var session = new Mock<ISession>();
            session.SetupSessionDefaultValues();
            session.Setup(s => s.Binding.GetRepositoryService().GetRepositoryInfo(this.repoId, null).LatestChangeLogToken).Returns(this.changeLogToken);
            var storage = new Mock<IMetaDataStorage>();
            storage.Setup(db => db.ChangeLogToken).Returns(this.changeLogToken);
            var queue = new Mock<ISyncEventQueue>();
            var changes = new ContentChanges(session.Object, storage.Object, queue.Object);

            Assert.That(changes.Handle(startSyncEvent), Is.True);
        }

        [Test]
        public void ExecuteCrawlSyncOnNoLocalTokenAvailable() {
            var startSyncEvent = new StartNextSyncEvent(false);
            var session = new Mock<ISession>();
            session.SetupSessionDefaultValues();
            session.Setup(s => s.Binding.GetRepositoryService().GetRepositoryInfo(this.repoId, null).LatestChangeLogToken).Returns(this.changeLogToken);
            var storage = new Mock<IMetaDataStorage>();
            storage.Setup(db => db.ChangeLogToken).Returns((string)null);
            var queue = new Mock<ISyncEventQueue>();
            var changes = new ContentChanges(session.Object, storage.Object, queue.Object);

            Assert.That(changes.Handle(startSyncEvent), Is.True);
            queue.Verify(q => q.AddEvent(It.Is<StartNextSyncEvent>(e => e.FullSyncRequested == true)), Times.Once());
        }

        [Test]
        public void IgnoreCrawlSyncEvent() {
            var start = new StartNextSyncEvent(true);
            var repositoryService = new Mock<IRepositoryService>();
            repositoryService.Setup(r => r.GetRepositoryInfos(null)).Returns((IList<IRepositoryInfo>)null);
            repositoryService.Setup(r => r.GetRepositoryInfo(It.IsAny<string>(), It.IsAny<IExtensionsData>()).LatestChangeLogToken).Returns(this.latestChangeLogToken);
            var session = new Mock<ISession>();
            session.Setup(s => s.Binding.GetRepositoryService()).Returns(repositoryService.Object);
            session.Setup(s => s.RepositoryInfo.Id).Returns(this.repoId);
            var storage = new Mock<IMetaDataStorage>();
            storage.Setup(db => db.ChangeLogToken).Returns(this.changeLogToken);
            var queue = new Mock<ISyncEventQueue>().Object;
            var changes = new ContentChanges(session.Object, storage.Object, queue);

            Assert.That(changes.Handle(start), Is.False);

            Assert.That(start.LastTokenOnServer, Is.Null);
        }

        [Test]
        public void ExtendCrawlSyncEvent() {
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

            Assert.That(changes.Handle(start), Is.False);

            Assert.That(start.LastTokenOnServer, Is.EqualTo(this.latestChangeLogToken));
        }

        [Test]
        public void GivesCorrectContentChangeEvent() {
            ContentChangeEvent contentChangeEvent = null;
            var queue = new Mock<ISyncEventQueue>();
            queue.Setup(q => q.AddEvent(It.IsAny<ContentChangeEvent>())).Callback((ISyncEvent f) => {
                contentChangeEvent = f as ContentChangeEvent;
            });
            string id = "myId";
            var storage = MockMetaDataStorageUtil.GetMetaStorageMockWithToken();
            var session = MockSessionUtil.PrepareSessionMockForSingleChange(DotCMIS.Enums.ChangeType.Created, id);
            var changes = new ContentChanges(session.Object, storage.Object, queue.Object, this.maxNumberOfContentChanges, this.isPropertyChangesSupported);
            var startSyncEvent = new StartNextSyncEvent(false);

            Assert.That(changes.Handle(startSyncEvent), Is.True);

            queue.Verify(foo => foo.AddEvent(It.IsAny<ContentChangeEvent>()), Times.Once());
            Assert.That(contentChangeEvent.Type, Is.EqualTo(DotCMIS.Enums.ChangeType.Created));
            Assert.That(contentChangeEvent.ObjectId, Is.EqualTo(id));
        }

        [Test]
        public void PagingTest() {
            var queue = new Mock<ISyncEventQueue>();
            var storage = MockMetaDataStorageUtil.GetMetaStorageMockWithToken();
            var session = MockSessionUtil.GetSessionMockReturning3Changesin2Batches();

            var startSyncEvent = new StartNextSyncEvent(false);
            var changes = new ContentChanges(session.Object, storage.Object, queue.Object, this.maxNumberOfContentChanges, this.isPropertyChangesSupported);

            Assert.That(changes.Handle(startSyncEvent), Is.True);
            queue.Verify(foo => foo.AddEvent(It.IsAny<ContentChangeEvent>()), Times.Exactly(3));
        }

        [Test]
        public void IgnoreDuplicatedContentChangesEventTestCreated() {
            var queue = new Mock<ISyncEventQueue>();
            var storage = MockMetaDataStorageUtil.GetMetaStorageMockWithToken();
            var session = MockSessionUtil.GetSessionMockReturning3Changesin2Batches(DotCMIS.Enums.ChangeType.Created, true);

            var startSyncEvent = new StartNextSyncEvent(false);
            var changes = new ContentChanges(session.Object, storage.Object, queue.Object, this.maxNumberOfContentChanges, this.isPropertyChangesSupported);

            Assert.That(changes.Handle(startSyncEvent), Is.True);
            queue.Verify(foo => foo.AddEvent(It.IsAny<ContentChangeEvent>()), Times.Exactly(3));
        }

        [Test]
        public void IgnoreDuplicatedContentChangesEventTestDeleted() {
            var queue = new Mock<ISyncEventQueue>();
            var storage = MockMetaDataStorageUtil.GetMetaStorageMockWithToken();
            var session = MockSessionUtil.GetSessionMockReturning3Changesin2Batches(DotCMIS.Enums.ChangeType.Deleted, true);

            var startSyncEvent = new StartNextSyncEvent(false);
            var changes = new ContentChanges(session.Object, storage.Object, queue.Object, this.maxNumberOfContentChanges, this.isPropertyChangesSupported);

            Assert.That(changes.Handle(startSyncEvent), Is.True);
            queue.Verify(foo => foo.AddEvent(It.IsAny<ContentChangeEvent>()), Times.Exactly(3));
        }
    }
}