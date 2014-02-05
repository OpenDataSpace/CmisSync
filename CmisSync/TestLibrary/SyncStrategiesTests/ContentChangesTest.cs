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
        private int maxNumberOfContentChanges;
        private bool isPropertyChangesSupported;
        private string changeLogToken = "token";
        private string lastChangeLogToken = "lastToken";
        private string latestChangeLogToken = "latestChangeLogToken";
        private string repoId = "repoId";
        private string objectId = "objectId";
        private int handled = 0;
        private Mock<ISyncEventQueue> queue;
        private Mock<IDatabase> database;
        private Mock<ISession> session;



        [SetUp]
        public void SetUp ()
        {
            maxNumberOfContentChanges = 1000;
            isPropertyChangesSupported = false;
            changeLogToken = "token";
            lastChangeLogToken = "lastToken";
            latestChangeLogToken = "latestChangeLogToken";
            repoId = "repoId";
            objectId = "objectId";
            handled = 0;
            queue = new Mock<ISyncEventQueue>();
            database = new Mock<IDatabase>();
            session = new Mock<ISession> ();

        }

        private ContentChanges fillContentChangesWithChanges(DotCMIS.Enums.ChangeType type, Mock<ISyncEventQueue> queueMock, string documentContentStreamId = null) {
            var changeEvents = new Mock<IChangeEvents> ();
            var changeList = new List<IChangeEvent> ();
            var changeEvent = new Mock<IChangeEvent> ();
            var newRemoteObject = new Mock<IDocument> ();
            changeEvent.Setup (ce => ce.ObjectId).Returns (objectId);
            changeEvent.Setup (ce => ce.ChangeType).Returns (type);
            changeList.Add (changeEvent.Object);
            changeEvents.Setup (ce => ce.HasMoreItems).Returns ((bool?)false);
            changeEvents.Setup (ce => ce.LatestChangeLogToken).Returns (latestChangeLogToken);
            changeEvents.Setup (ce => ce.TotalNumItems).Returns (1);
            changeEvents.Setup (ce => ce.ChangeEventList).Returns (changeList);
            session.Setup (s => s.Binding.GetRepositoryService ().GetRepositoryInfos (null)).Returns ((IList<IRepositoryInfo>)null);
            session.Setup (s => s.RepositoryInfo.Id).Returns (repoId);
            session.Setup (s => s.Binding.GetRepositoryService ().GetRepositoryInfo (repoId, null).LatestChangeLogToken).Returns (changeLogToken);
            session.Setup (s => s.GetContentChanges (lastChangeLogToken, isPropertyChangesSupported, maxNumberOfContentChanges)).Returns (changeEvents.Object);
            newRemoteObject.Setup(d => d.ContentStreamId).Returns(documentContentStreamId);
            newRemoteObject.Setup(d => d.ContentStreamLength).Returns(documentContentStreamId==null? 0 : 1);
            session.Setup (s => s.GetObject (objectId)).Returns (newRemoteObject.Object);
            database.Setup (db => db.GetChangeLogToken ()).Returns (lastChangeLogToken);
            var changes = new ContentChanges (session.Object, database.Object, queueMock.Object, maxNumberOfContentChanges, isPropertyChangesSupported);
            return changes;

        }

        [Test, Category("Fast")]
        public void ConstructorWithVaildEntriesTest ()
        {
            int maxNumberOfContentChanges = 1000;
            bool isPropertyChangesSupported = true;
            new ContentChanges (session.Object, database.Object, queue.Object);
            new ContentChanges (session.Object, database.Object, queue.Object, maxNumberOfContentChanges);
            new ContentChanges (session.Object, database.Object, queue.Object, maxNumberOfContentChanges, isPropertyChangesSupported);
            new ContentChanges (session.Object, database.Object, queue.Object, isPropertyChangesSupported: true);
            new ContentChanges (session.Object, database.Object, queue.Object, isPropertyChangesSupported: false);
        }

        [Test, Category("Fast")]
        [ExpectedException( typeof( ArgumentNullException ) )]
        public void ConstructorFailsOnNullDbTest ()
        {
            new ContentChanges (session.Object, null, queue.Object);
        }

        [Test, Category("Fast")]
        public void ConstructorFailsOnInvalidMaxEventsLimitTest ()
        {
            try {
                new ContentChanges (session.Object, database.Object, queue.Object, -1);
                Assert.Fail ();
            } catch (ArgumentException) {
            }
            try {
                new ContentChanges (session.Object, database.Object, queue.Object, 0);
                Assert.Fail ();
            } catch (ArgumentException) {
            }
            try {
                new ContentChanges (session.Object, database.Object, queue.Object, 1);
                Assert.Fail ();
            } catch (ArgumentException) {
            }
        }

        [Test, Category("Fast")]
        [ExpectedException( typeof( ArgumentNullException ) )]
        public void ConstructorFailsOnNullSessionTest ()
        {
            new ContentChanges (null, database.Object, queue.Object);
        }

        [Test, Category("Fast")]
        [ExpectedException( typeof( ArgumentNullException ) )]
        public void ConstructorFailsOnNullQueueTest ()
        {
            var session = new Mock<ISession> ().Object;
            new ContentChanges (session, database.Object, null);
        }

        [Test, Category("Fast")]
        public void IgnoreWrongEventTest ()
        {
            var changes = new ContentChanges (session.Object, database.Object, queue.Object);
            var wrongEvent = new Mock<ISyncEvent> ().Object;
            Assert.IsFalse (changes.Handle (wrongEvent));
        }

        [Test, Category("Fast")]
        public void HandleFullSyncCompletedEventTest ()
        {
            string insertedToken = "";
            var startSyncEvent = new StartNextSyncEvent (false);
            startSyncEvent.SetParam (ContentChanges.FULL_SYNC_PARAM_NAME, changeLogToken);
            var completedEvent = new FullSyncCompletedEvent (startSyncEvent);
            database.Setup (db => db.SetChangeLogToken ("token")).Callback ((string s) => {
                insertedToken = s;
                handled ++;
            }
            );
            var changes = new ContentChanges (session.Object, database.Object, queue.Object);
            Assert.IsFalse (changes.Handle (completedEvent));
            Assert.AreEqual (1, handled);
            Assert.AreEqual (changeLogToken, insertedToken);
        }

        [Test, Category("Fast")]
        public void HandleStartSyncEventOnNoRemoteChangeTest ()
        {
            var startSyncEvent = new StartNextSyncEvent (false);
            session.Setup (s => s.Binding.GetRepositoryService ().GetRepositoryInfos (null)).Returns ((IList<IRepositoryInfo>)null);
            session.Setup (s => s.RepositoryInfo.Id).Returns (repoId);
            session.Setup (s => s.Binding.GetRepositoryService ().GetRepositoryInfo (repoId, null).LatestChangeLogToken).Returns (changeLogToken);
            database.Setup (db => db.GetChangeLogToken ()).Returns (changeLogToken);
            var changes = new ContentChanges (session.Object, database.Object, queue.Object);
            Assert.IsTrue (changes.Handle (startSyncEvent));
        }

        private FileEvent LogIt()
        {
            return Match.Create<FileEvent>(f => f.RemoteContent == ContentChangeType.DELETED);
        }

        [Test, Category("Fast")]
        public void HandleStartSyncEventOnOneRemoteDocumentCreationWithContent ()
        {
            ISyncEvent syncEvent = null;
            queue.Setup(q => q.AddEvent (It.IsAny<FileEvent>())).Callback ((ISyncEvent f) => {
                syncEvent = f;
            }
            );
            var startSyncEvent = new StartNextSyncEvent (false);
            var changes = fillContentChangesWithChanges(DotCMIS.Enums.ChangeType.Created, queue, "streamId");
            Assert.IsTrue (changes.Handle (startSyncEvent));
            queue.Verify(foo => foo.AddEvent(It.IsAny<FileEvent>()), Times.Once());
            Assert.That(syncEvent, Is.TypeOf(typeof(FileEvent)));
            var fileEvent = syncEvent as FileEvent;
            Assert.That(fileEvent.Remote, Is.EqualTo(MetaDataChangeType.CREATED));
            Assert.That(fileEvent.RemoteContent, Is.EqualTo(ContentChangeType.CREATED));
        }

        [Test, Category("Fast")]
        public void HandleStartSyncEventOnOneRemoteDocumentCreationWithoutContent ()
        {
            ISyncEvent syncEvent = null;
            queue.Setup(q => q.AddEvent (It.IsAny<FileEvent>())).Callback ((ISyncEvent f) => {
                syncEvent = f;
            }
            );
            var startSyncEvent = new StartNextSyncEvent (false);
            var changes = fillContentChangesWithChanges(DotCMIS.Enums.ChangeType.Created, queue);
            Assert.IsTrue (changes.Handle (startSyncEvent));
            queue.Verify(foo => foo.AddEvent(It.IsAny<FileEvent>()), Times.Once());
            Assert.That(syncEvent, Is.TypeOf(typeof(FileEvent)));
            var fileEvent = syncEvent as FileEvent;
            Assert.That(fileEvent.Remote, Is.EqualTo(MetaDataChangeType.CREATED));
            Assert.That(fileEvent.RemoteContent, Is.EqualTo(ContentChangeType.NONE));
        }

        [Test, Category("Fast")]
        public void HandleStartSyncEventOnOneLocallyNotExistingRemoteDocumentUpdated ()
        {
            ISyncEvent syncEvent = null;
            queue.Setup(q => q.AddEvent (It.IsAny<FileEvent>())).Callback ((ISyncEvent f) => {
                syncEvent = f;
            }
            );
            var startSyncEvent = new StartNextSyncEvent (false);
            var changes = fillContentChangesWithChanges(DotCMIS.Enums.ChangeType.Updated, queue);
            Assert.IsTrue (changes.Handle (startSyncEvent));
            queue.Verify(foo => foo.AddEvent(It.IsAny<FileEvent>()), Times.Once());
            Assert.That(syncEvent, Is.TypeOf(typeof(FileEvent)));
            var fileEvent = syncEvent as FileEvent;
            Assert.That(fileEvent.Remote, Is.EqualTo(MetaDataChangeType.CREATED));
            Assert.That(fileEvent.RemoteContent, Is.EqualTo(ContentChangeType.CREATED));
        }

        [Test, Category("Fast")]
        public void HandleStartSyncEventOnOneLocallyExistingRemoteDocumentUpdated ()
        {
            ISyncEvent syncEvent = null;
            queue.Setup(q => q.AddEvent (It.IsAny<FileEvent>())).Callback ((ISyncEvent f) => {
                syncEvent = f;
            }
            );
            database.Setup(db => db.GetFilePath(It.IsAny<string>())).Returns("path");
            var startSyncEvent = new StartNextSyncEvent (false);
            var changes = fillContentChangesWithChanges(DotCMIS.Enums.ChangeType.Updated, queue);
            Assert.IsTrue (changes.Handle (startSyncEvent));
            queue.Verify(foo => foo.AddEvent(It.IsAny<FileEvent>()), Times.Once());
            Assert.That(syncEvent, Is.TypeOf(typeof(FileEvent)));
            var fileEvent = syncEvent as FileEvent;
            Assert.That(fileEvent.Remote, Is.EqualTo(MetaDataChangeType.CHANGED));
            Assert.That(fileEvent.RemoteContent, Is.EqualTo(ContentChangeType.CHANGED));
        }

        [Ignore]
        [Test, Category("Fast")]
        public void HandleStartSyncEventOnOneRemoteDeletionChangeTest ()
        {
            ISyncEvent syncEvent = null;
            queue.Setup(q => q.AddEvent (It.IsAny<FileEvent>())).Callback ((ISyncEvent f) => {
                syncEvent = f;
            }
            );
            var startSyncEvent = new StartNextSyncEvent (false);
            queue.Verify(foo => foo.AddEvent(It.IsAny<FileEvent>()), Times.Once());
            var changes = fillContentChangesWithChanges(DotCMIS.Enums.ChangeType.Deleted, queue);
            Assert.IsTrue (changes.Handle (startSyncEvent));
            Assert.That(syncEvent, Is.TypeOf(typeof(FileEvent)));
            var fileEvent = syncEvent as FileEvent;
            Assert.That(fileEvent.Remote, Is.EqualTo(MetaDataChangeType.DELETED));
            Assert.That(fileEvent.RemoteContent, Is.EqualTo(ContentChangeType.DELETED));
        }

        [Test, Category("Fast")]
        public void ExecuteCrawlSyncOnNoLocalTokenAvailableTest ()
        {
            ISyncEvent queuedEvent = null;
            var startSyncEvent = new StartNextSyncEvent (false);
            session.Setup (s => s.Binding.GetRepositoryService ().GetRepositoryInfos (null)).Returns ((IList<IRepositoryInfo>)null);
            session.Setup (s => s.RepositoryInfo.Id).Returns (repoId);
            session.Setup (s => s.Binding.GetRepositoryService ().GetRepositoryInfo (repoId, null).LatestChangeLogToken).Returns (changeLogToken);
            database.Setup (db => db.GetChangeLogToken ()).Returns ((string)null);
            queue.Setup (q => q.AddEvent (It.IsAny<ISyncEvent> ())).Callback<ISyncEvent> ((e) => {
                handled ++;
                queuedEvent = e;
            }
            );
            var changes = new ContentChanges (session.Object, database.Object, queue.Object);
            Assert.IsTrue (changes.Handle (startSyncEvent));
            Assert.AreEqual (1, handled);
            Assert.NotNull (queuedEvent);
            Assert.IsTrue (queuedEvent is StartNextSyncEvent);
            Assert.IsTrue (((StartNextSyncEvent)queuedEvent).FullSyncRequested);
            string returnedChangeLogToken;
            Assert.IsTrue (((StartNextSyncEvent)queuedEvent).TryGetParam (ContentChanges.FULL_SYNC_PARAM_NAME, out returnedChangeLogToken));
            Assert.AreEqual (changeLogToken, returnedChangeLogToken);
        }

        [Ignore]
        [Test, Category("Fast")]
        public void IgnoreDuplicatedContentChangesEventTest ()
        {
            /* Paging is fucked up in a way that the last event of page 1 can be also the first of page 2*/
            Assert.Fail ("TODO");
        }

        [Test, Category("Fast")]
        public void IgnoreCrawlSyncEventTest ()
        {
            var start = new StartNextSyncEvent (true);
            var repositoryService = new Mock<IRepositoryService> ();
            repositoryService.Setup (r => r.GetRepositoryInfos (null)).Returns ((IList<IRepositoryInfo>)null);
            repositoryService.Setup (r => r.GetRepositoryInfo (It.IsAny<string> (), It.IsAny<IExtensionsData> ()).LatestChangeLogToken).Returns (latestChangeLogToken);
            session.Setup (s => s.Binding.GetRepositoryService ()).Returns (repositoryService.Object);
            session.Setup (s => s.RepositoryInfo.Id).Returns (repoId);
            database.Setup (db => db.GetChangeLogToken ()).Returns (changeLogToken);
            var changes = new ContentChanges (session.Object, database.Object, queue.Object);
            Assert.IsFalse (changes.Handle (start));
            string result;
            Assert.IsFalse (start.TryGetParam (ContentChanges.FULL_SYNC_PARAM_NAME, out result));
            Assert.IsNull (result);
        }

        [Test, Category("Fast")]
        public void ExtendCrawlSyncEventTest ()
        {
            var start = new StartNextSyncEvent (true);
            var repositoryService = new Mock<IRepositoryService> ();
            repositoryService.Setup (r => r.GetRepositoryInfos (null)).Returns ((IList<IRepositoryInfo>)null);
            repositoryService.Setup (r => r.GetRepositoryInfo (It.IsAny<string> (), It.IsAny<IExtensionsData> ()).LatestChangeLogToken).Returns (latestChangeLogToken);
            session.Setup (s => s.Binding.GetRepositoryService ()).Returns (repositoryService.Object);
            session.Setup (s => s.RepositoryInfo.Id).Returns (repoId);
            var manager = new Mock<SyncEventManager> ().Object;
            database.Setup (db => db.GetChangeLogToken ()).Returns ((string)null);
            var changes = new ContentChanges (session.Object, database.Object, queue.Object);
            Assert.IsFalse (changes.Handle (start));
            string result;
            Assert.IsTrue (start.TryGetParam (ContentChanges.FULL_SYNC_PARAM_NAME, out result));
            Assert.AreEqual (latestChangeLogToken, result);
        }

        [Ignore]
        [Test, Category("Fast")]
        public void PagingTest ()
        {
            Assert.Fail ("TODO");
        }
    }
}

