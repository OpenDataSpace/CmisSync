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
        [Test, Category("Fast")]
        public void ConstructorWithVaildEntriesTest()
        {
            var session = new Mock<ISession>().Object;
            var db = new Mock<IDatabase>().Object;
            var queue = new Mock<ISyncEventQueue>().Object;
            int maxNumberOfContentChanges = 1000;
            bool isPropertyChangesSupported = true;
            new ContentChanges(session, db, queue);
            new ContentChanges(session, db, queue, maxNumberOfContentChanges);
            new ContentChanges(session, db, queue, maxNumberOfContentChanges, isPropertyChangesSupported);
            new ContentChanges(session, db, queue, isPropertyChangesSupported: true);
            new ContentChanges(session, db, queue, isPropertyChangesSupported: false);
        }

        [Test, Category("Fast")]
        [ExpectedException( typeof( ArgumentNullException ) )]
        public void ConstructorFailsOnNullDbTest() {
            var session = new Mock<ISession>().Object;
            var queue = new Mock<ISyncEventQueue>().Object;
            new ContentChanges(session, null, queue);
        }

        [Test, Category("Fast")]
        public void ConstructorFailsOnInvalidMaxEventsLimitTest()
        {
            var session = new Mock<ISession>().Object;
            var db = new Mock<IDatabase>().Object;
            var queue = new Mock<ISyncEventQueue>().Object;
            try{
                new ContentChanges(session, db, queue, -1);
                Assert.Fail();
            }catch(ArgumentException){}
            try{
                new ContentChanges(session, db, queue, 0);
                Assert.Fail();
            }catch(ArgumentException){}
            try{
                new ContentChanges(session, db, queue, 1);
                Assert.Fail();
            }catch(ArgumentException){}
        }

        [Test, Category("Fast")]
        [ExpectedException( typeof( ArgumentNullException ) )]
        public void ConstructorFailsOnNullSessionTest()
        {
            var db = new Mock<IDatabase>().Object;
            var queue = new Mock<ISyncEventQueue>().Object;
            new ContentChanges(null, db, queue);
        }

        [Test, Category("Fast")]
        [ExpectedException( typeof( ArgumentNullException ) )]
        public void ConstructorFailsOnNullQueueTest()
        {
            var session = new Mock<ISession>().Object;
            var db = new Mock<IDatabase>().Object;
            new ContentChanges(session, db, null);
        }

        [Test, Category("Fast")]
        public void IgnoreWrongEventTest() {
            var session = new Mock<ISession>().Object;
            var db = new Mock<IDatabase>().Object;
            var queue = new Mock<ISyncEventQueue>().Object;
            var changes = new ContentChanges(session, db, queue);
            var wrongEvent = new Mock<ISyncEvent>().Object;
            Assert.IsFalse(changes.Handle(wrongEvent));
        }

        [Test, Category("Fast")]
        public void HandleFullSyncCompletedEventTest() {
            string changeLogToken = "token";
            int inserted = 0;
            string insertedToken = "";
            var startSyncEvent = new StartNextSyncEvent(false);
            startSyncEvent.SetParam(ContentChanges.FULL_SYNC_PARAM_NAME, changeLogToken);
            var completedEvent = new FullSyncCompletedEvent(startSyncEvent);
            var session = new Mock<ISession>().Object;
            var database = new Mock<IDatabase>();
            database.Setup(db => db.SetChangeLogToken("token")).Callback((string s) => {
                insertedToken = s;
                inserted ++;
            });
            var queue = new Mock<ISyncEventQueue>().Object;
            var changes = new ContentChanges(session, database.Object, queue);
            Assert.IsFalse(changes.Handle(completedEvent));
            Assert.AreEqual(1, inserted);
            Assert.AreEqual(changeLogToken, insertedToken);
        }

        [Test, Category("Fast")]
        public void HandleStartSyncEventOnNoRemoteChangeTest()
        {
            string changeLogToken = "token";
            string repoId = "repoId";
            var startSyncEvent = new StartNextSyncEvent(false);
            var session = new Mock<ISession>();
            session.Setup(s => s.Binding.GetRepositoryService().GetRepositoryInfos(null)).Returns((IList<IRepositoryInfo>)null);
            session.Setup (s => s.RepositoryInfo.Id ).Returns(repoId);
            session.Setup(s => s.Binding.GetRepositoryService().GetRepositoryInfo(repoId, null).LatestChangeLogToken).Returns(changeLogToken);
            var database = new Mock<IDatabase>();
            database.Setup(db => db.GetChangeLogToken()).Returns(changeLogToken);
            var queue = new Mock<ISyncEventQueue>().Object;
            var changes = new ContentChanges(session.Object, database.Object, queue);
            Assert.IsTrue(changes.Handle(startSyncEvent));
        }

        [Ignore]
        [Test, Category("Fast")]
        public void HandleStartSyncEventOnOneRemoteCreationChangeTest()
        {
            int maxNumberOfContentChanges = 1000;
            bool isPropertyChangesSupported = true;
            string changeLogToken = "token";
            string latestChangeLogToken = changeLogToken;
            string lastChangeLogToken = "lastToken";
            string repoId = "repoId";
            string objectId = "objectId";
            var startSyncEvent = new StartNextSyncEvent(false);
            var session = new Mock<ISession>();
            var changeEvents = new Mock<IChangeEvents>();
            var changeList = new List<IChangeEvent>();
            var changeEvent = new Mock<IChangeEvent>();
            var newRemoteObject = new Mock<IDocument>();
            changeEvent.Setup( ce => ce.ObjectId).Returns(objectId);
            changeEvent.Setup( ce => ce.ChangeType).Returns(DotCMIS.Enums.ChangeType.Created);
            changeList.Add(changeEvent.Object);
            changeEvents.Setup (ce => ce.HasMoreItems).Returns((bool?)false);
            changeEvents.Setup (ce => ce.LatestChangeLogToken).Returns(latestChangeLogToken);
            changeEvents.Setup (ce => ce.TotalNumItems).Returns(1);
            changeEvents.Setup (ce => ce.ChangeEventList).Returns(changeList);
            session.Setup (s => s.Binding.GetRepositoryService().GetRepositoryInfos(null)).Returns((IList<IRepositoryInfo>)null);
            session.Setup (s => s.RepositoryInfo.Id ).Returns(repoId);
            session.Setup (s => s.Binding.GetRepositoryService().GetRepositoryInfo(repoId, null).LatestChangeLogToken).Returns(changeLogToken);
            session.Setup (s => s.GetContentChanges(lastChangeLogToken, isPropertyChangesSupported, maxNumberOfContentChanges)).Returns(changeEvents.Object);
            session.Setup (s => s.GetObject(objectId)).Returns(newRemoteObject.Object);
            var database = new Mock<IDatabase>();
            database.Setup(db => db.GetChangeLogToken()).Returns(lastChangeLogToken);
            var queue = new Mock<ISyncEventQueue>();
            queue.Setup (q => q.AddEvent(It.IsAny<ISyncEvent>()));
            var changes = new ContentChanges(session.Object, database.Object, queue.Object, maxNumberOfContentChanges, isPropertyChangesSupported);
            Assert.IsTrue(changes.Handle(startSyncEvent));
            Assert.Fail("TODO");
        }

        [Ignore]
        [Test, Category("Fast")]
        public void HandleStartSyncEventOnOneRemoteUpdatedChangeTest()
        {
            int maxNumberOfContentChanges = 1000;
            bool isPropertyChangesSupported = true;
            string changeLogToken = "token";
            string latestChangeLogToken = changeLogToken;
            string lastChangeLogToken = "lastToken";
            string repoId = "repoId";
            string objectId = "objectId";
            var startSyncEvent = new StartNextSyncEvent(false);
            var session = new Mock<ISession>();
            var changeEvents = new Mock<IChangeEvents>();
            var changeList = new List<IChangeEvent>();
            var changeEvent = new Mock<IChangeEvent>();
            changeEvent.Setup( ce => ce.ObjectId).Returns(objectId);
            changeEvent.Setup( ce => ce.ChangeType).Returns(DotCMIS.Enums.ChangeType.Updated);
            changeList.Add(changeEvent.Object);
            changeEvents.Setup (ce => ce.HasMoreItems).Returns((bool?)false);
            changeEvents.Setup (ce => ce.LatestChangeLogToken).Returns(latestChangeLogToken);
            changeEvents.Setup (ce => ce.TotalNumItems).Returns(1);
            changeEvents.Setup (ce => ce.ChangeEventList).Returns(changeList);
            session.Setup (s => s.Binding.GetRepositoryService().GetRepositoryInfos(null)).Returns((IList<IRepositoryInfo>)null);
            session.Setup (s => s.RepositoryInfo.Id ).Returns(repoId);
            session.Setup (s => s.Binding.GetRepositoryService().GetRepositoryInfo(repoId, null).LatestChangeLogToken).Returns(changeLogToken);
            session.Setup (s => s.GetContentChanges(lastChangeLogToken, isPropertyChangesSupported, maxNumberOfContentChanges)).Returns(changeEvents.Object);
            var database = new Mock<IDatabase>();
            database.Setup(db => db.GetChangeLogToken()).Returns(lastChangeLogToken);
            var queue = new Mock<ISyncEventQueue>().Object;
            var changes = new ContentChanges(session.Object, database.Object, queue, maxNumberOfContentChanges, isPropertyChangesSupported);
            Assert.IsTrue(changes.Handle(startSyncEvent));
            Assert.Fail("TODO");
        }

        [Ignore]
        [Test, Category("Fast")]
        public void HandleStartSyncEventOnOneRemoteDeletionChangeTest()
        {
            int maxNumberOfContentChanges = 1000;
            bool isPropertyChangesSupported = true;
            string changeLogToken = "token";
            string latestChangeLogToken = changeLogToken;
            string lastChangeLogToken = "lastToken";
            string repoId = "repoId";
            string objectId = "objectId";
            var startSyncEvent = new StartNextSyncEvent(false);
            var session = new Mock<ISession>();
            var changeEvents = new Mock<IChangeEvents>();
            var changeList = new List<IChangeEvent>();
            var changeEvent = new Mock<IChangeEvent>();
            changeEvent.Setup( ce => ce.ObjectId).Returns(objectId);
            changeEvent.Setup( ce => ce.ChangeType).Returns(DotCMIS.Enums.ChangeType.Deleted);
            changeList.Add(changeEvent.Object);
            changeEvents.Setup (ce => ce.HasMoreItems).Returns((bool?)false);
            changeEvents.Setup (ce => ce.LatestChangeLogToken).Returns(latestChangeLogToken);
            changeEvents.Setup (ce => ce.TotalNumItems).Returns(1);
            changeEvents.Setup (ce => ce.ChangeEventList).Returns(changeList);
            session.Setup (s => s.Binding.GetRepositoryService().GetRepositoryInfos(null)).Returns((IList<IRepositoryInfo>)null);
            session.Setup (s => s.RepositoryInfo.Id ).Returns(repoId);
            session.Setup (s => s.Binding.GetRepositoryService().GetRepositoryInfo(repoId, null).LatestChangeLogToken).Returns(changeLogToken);
            session.Setup (s => s.GetContentChanges(lastChangeLogToken, isPropertyChangesSupported, maxNumberOfContentChanges)).Returns(changeEvents.Object);
            var database = new Mock<IDatabase>();
            database.Setup(db => db.GetChangeLogToken()).Returns(lastChangeLogToken);
            var queue = new Mock<ISyncEventQueue>().Object;
            var changes = new ContentChanges(session.Object, database.Object, queue, maxNumberOfContentChanges, isPropertyChangesSupported);
            Assert.IsTrue(changes.Handle(startSyncEvent));
            Assert.Fail("TODO");

        }

        [Test, Category("Fast")]
        public void ExecuteCrawlSyncOnNoLocalTokenAvailableTest()
        {
            int queued = 0;
            ISyncEvent queuedEvent = null;
            string changeLogToken = "token";
            string lastChangeLogToken = null;
            string repoId = "repoId";
            var startSyncEvent = new StartNextSyncEvent(false);
            var session = new Mock<ISession>();
            session.Setup(s => s.Binding.GetRepositoryService().GetRepositoryInfos(null)).Returns((IList<IRepositoryInfo>)null);
            session.Setup (s => s.RepositoryInfo.Id ).Returns(repoId);
            session.Setup(s => s.Binding.GetRepositoryService().GetRepositoryInfo(repoId, null).LatestChangeLogToken).Returns(changeLogToken);
            var database = new Mock<IDatabase>();
            database.Setup(db => db.GetChangeLogToken()).Returns(lastChangeLogToken);
            var queue = new Mock<ISyncEventQueue>();
            queue.Setup(q => q.AddEvent(It.IsAny<ISyncEvent>())).Callback<ISyncEvent>((e) => {
                queued ++;
                queuedEvent = e;
            });
            var changes = new ContentChanges(session.Object, database.Object, queue.Object);
            Assert.IsTrue(changes.Handle(startSyncEvent));
            Assert.AreEqual(1, queued);
            Assert.NotNull(queuedEvent);
            Assert.IsTrue(queuedEvent is StartNextSyncEvent);
            Assert.IsTrue(((StartNextSyncEvent)queuedEvent).FullSyncRequested);
            string returnedChangeLogToken;
            Assert.IsTrue(((StartNextSyncEvent)queuedEvent).TryGetParam(ContentChanges.FULL_SYNC_PARAM_NAME, out returnedChangeLogToken));
            Assert.AreEqual(changeLogToken, returnedChangeLogToken);
        }

        [Ignore]
        [Test, Category("Fast")]
        public void IgnoreDuplicatedContentChangesEventTest()
        {
            Assert.Fail("TODO");
        }

        [Test, Category("Fast")]
        public void IgnoreCrawlSyncEventTest() {
            var start = new StartNextSyncEvent(true);
            var database = new Mock<IDatabase>();
            var session = new Mock<ISession>();
            var repositoryService = new Mock<IRepositoryService>();
            repositoryService.Setup(r => r.GetRepositoryInfos(null)).Returns((IList<IRepositoryInfo>)null);
            repositoryService.Setup(r => r.GetRepositoryInfo(It.IsAny<string>(), It.IsAny<IExtensionsData>()).LatestChangeLogToken).Returns("token");
            session.Setup(s => s.Binding.GetRepositoryService()).Returns(repositoryService.Object);
            session.Setup(s => s.RepositoryInfo.Id).Returns("repoid");
            var queue = new Mock<ISyncEventQueue>().Object;
            database.Setup( db => db.GetChangeLogToken()).Returns("token");
            var changes = new ContentChanges(session.Object, database.Object, queue);
            Assert.IsFalse(changes.Handle(start));
            string result;
            Assert.IsFalse(start.TryGetParam(ContentChanges.FULL_SYNC_PARAM_NAME, out result));
            Assert.IsNull(result);
        }

        [Test, Category("Fast")]
        public void ExtendCrawlSyncEventTest() {
            string serverSideChangeLogToken = "token";
            var start = new StartNextSyncEvent(true);
            var database = new Mock<IDatabase>();
            var session = new Mock<ISession>();
            var repositoryService = new Mock<IRepositoryService>();
            repositoryService.Setup(r => r.GetRepositoryInfos(null)).Returns((IList<IRepositoryInfo>)null);
            repositoryService.Setup(r => r.GetRepositoryInfo(It.IsAny<string>(), It.IsAny<IExtensionsData>()).LatestChangeLogToken).Returns("token");
            session.Setup(s => s.Binding.GetRepositoryService()).Returns(repositoryService.Object);
            session.Setup(s => s.RepositoryInfo.Id).Returns("repoid");
            var manager = new Mock<SyncEventManager>().Object;
            var queue = new Mock<SyncEventQueue>(manager).Object;
            database.Setup( db => db.GetChangeLogToken()).Returns((string) null);
            var changes = new ContentChanges(session.Object, database.Object, queue);
            Assert.IsFalse(changes.Handle(start));
            string result;
            Assert.IsTrue(start.TryGetParam(ContentChanges.FULL_SYNC_PARAM_NAME, out result));
            Assert.AreEqual(serverSideChangeLogToken, result);
        }
    }
}

