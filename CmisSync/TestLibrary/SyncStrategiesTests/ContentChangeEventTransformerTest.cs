using System;
using DotCMIS.Client;

using CmisSync.Lib;
using CmisSync.Lib.Sync.Strategy;
using CmisSync.Lib.Events;

using NUnit.Framework;

using Moq;

using TestLibrary.TestUtils;

namespace TestLibrary.SyncStrategiesTests {
    [TestFixture]
    public class ContentChangeEventTransformerTest {

        [Test, Category("Fast"), Category("ContentChange")]
        public void ConstructorTest() {
            var db = new Mock<IDatabase>();
            var queue  = new Mock<ISyncEventQueue>();
            var transformer = new ContentChangeEventTransformer(queue.Object, db.Object);
            Assert.That(transformer.Priority, Is.EqualTo(1000));
        }

        [Test, Category("Fast"), Category("ContentChange")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DbNullContstructorTest() {
            var queue  = new Mock<ISyncEventQueue>();
            new ContentChangeEventTransformer(queue.Object, null);
        }

        [Test, Category("Fast"), Category("ContentChange")]
        public void IgnoreDifferentEvent()
        {
            var db = new Mock<IDatabase>();
            var queue = new Mock<ISyncEventQueue>();
            var e = new Mock<ISyncEvent>();
            var transformer = new ContentChangeEventTransformer(queue.Object, db.Object);
            Assert.That(transformer.Handle(e.Object), Is.False);
        }

        [Test, Category("Fast"), Category("ContentChange")]
        [ExpectedException(typeof(InvalidOperationException))]
        public void IgnoreNotAccumulatedNonDeleteEvent()
        {
            var db = new Mock<IDatabase>();
            var queue = new Mock<ISyncEventQueue>();
            var e = new ContentChangeEvent(DotCMIS.Enums.ChangeType.Created, id);
            var transformer = new ContentChangeEventTransformer(queue.Object, db.Object);
            transformer.Handle(e);
        }

        private static readonly string id = "myId";
        private ContentChangeEvent prepareEvent(DotCMIS.Enums.ChangeType type, bool hasContentStream) {
            var e = new ContentChangeEvent(type, id);
            var remoteObject = MockUtil.CreateRemoteObjectMock(hasContentStream ? "streamId" : null);
            var session = new Mock<ISession>();
            session.Setup (s => s.GetObject (It.IsAny<string>())).Returns (remoteObject.Object);

            e.UpdateObject(session.Object);
            return e;
        }

        private ContentChangeEvent prepareFolderEvent(DotCMIS.Enums.ChangeType type) {
            var e = new ContentChangeEvent(type, id);
            var remoteObject = new Mock<IFolder>();
            var session = new Mock<ISession>();
            session.Setup (s => s.GetObject (It.IsAny<string>())).Returns (remoteObject.Object);

            e.UpdateObject(session.Object);
            return e;
        }

        [Test, Category("Fast"), Category("ContentChange")]
        public void DocumentCreationWithContent() {
            var db = new Mock<IDatabase>();
            FileEvent fileEvent = null;
            var queue = new Mock<ISyncEventQueue>();
            queue.Setup(h => h.AddEvent(It.IsAny<FileEvent>()))
                    .Callback<ISyncEvent>(e => fileEvent = e as FileEvent);

            var transformer = new ContentChangeEventTransformer(queue.Object, db.Object);
            var contentChangeEvent = prepareEvent(DotCMIS.Enums.ChangeType.Created, true);

            Assert.That(transformer.Handle(contentChangeEvent), Is.True);
            queue.Verify(q => q.AddEvent(It.IsAny<FileEvent>()), Times.Once());           
            Assert.That(fileEvent.Remote, Is.EqualTo(MetaDataChangeType.CREATED));
            Assert.That(fileEvent.RemoteContent, Is.EqualTo(ContentChangeType.CREATED));
        }

        [Test, Category("Fast"), Category("ContentChange")]
        public void DocumentCreationWithOutContent() {
            var db = new Mock<IDatabase>();
            FileEvent fileEvent = null;
            var queue = new Mock<ISyncEventQueue>();
            queue.Setup(h => h.AddEvent(It.IsAny<FileEvent>()))
                    .Callback<ISyncEvent>(e => fileEvent = e as FileEvent);

            var transformer = new ContentChangeEventTransformer(queue.Object, db.Object);
            var contentChangeEvent = prepareEvent(DotCMIS.Enums.ChangeType.Created, false);

            Assert.That(transformer.Handle(contentChangeEvent), Is.True);
            queue.Verify(q => q.AddEvent(It.IsAny<FileEvent>()), Times.Once());           
            Assert.That(fileEvent.Remote, Is.EqualTo(MetaDataChangeType.CREATED));
            Assert.That(fileEvent.RemoteContent, Is.EqualTo(ContentChangeType.NONE));
        }


        [Test, Category("Fast"), Category("ContentChange")]
        public void RemoteSecurityChangeOfExistingFile ()
        {
            var db = new Mock<IDatabase>();
            db.AddLocalFile();
            FileEvent fileEvent = null;
            var queue = new Mock<ISyncEventQueue>();
            queue.Setup(h => h.AddEvent(It.IsAny<FileEvent>()))
                    .Callback<ISyncEvent>(e => fileEvent = e as FileEvent);

            var transformer = new ContentChangeEventTransformer(queue.Object, db.Object);
            var contentChangeEvent = prepareEvent(DotCMIS.Enums.ChangeType.Security, false);

            Assert.That(transformer.Handle(contentChangeEvent), Is.True);
            queue.Verify(q => q.AddEvent(It.IsAny<FileEvent>()), Times.Once());           
            Assert.That(fileEvent.Remote, Is.EqualTo(MetaDataChangeType.CHANGED));
            Assert.That(fileEvent.RemoteContent, Is.EqualTo(ContentChangeType.NONE));
        }

        [Test, Category("Fast"), Category("ContentChange")]
        public void RemoteSecurityChangeOfNonExistingFile ()
        {
            var db = new Mock<IDatabase>();
            FileEvent fileEvent = null;
            var queue = new Mock<ISyncEventQueue>();
            queue.Setup(h => h.AddEvent(It.IsAny<FileEvent>()))
                    .Callback<ISyncEvent>(e => fileEvent = e as FileEvent);

            var transformer = new ContentChangeEventTransformer(queue.Object, db.Object);
            var contentChangeEvent = prepareEvent(DotCMIS.Enums.ChangeType.Security, false);

            Assert.That(transformer.Handle(contentChangeEvent), Is.True);
            queue.Verify(q => q.AddEvent(It.IsAny<FileEvent>()), Times.Once());           
            Assert.That(fileEvent.Remote, Is.EqualTo(MetaDataChangeType.CREATED));
            Assert.That(fileEvent.RemoteContent, Is.EqualTo(ContentChangeType.CREATED));
        }

        [Test, Category("Fast"), Category("ContentChange")]
        public void LocallyNotExistingRemoteDocumentUpdated ()
        {
            var db = new Mock<IDatabase>();
            FileEvent fileEvent = null;
            var queue = new Mock<ISyncEventQueue>();
            queue.Setup(h => h.AddEvent(It.IsAny<FileEvent>()))
                    .Callback<ISyncEvent>(e => fileEvent = e as FileEvent);

            var transformer = new ContentChangeEventTransformer(queue.Object, db.Object);
            var contentChangeEvent = prepareEvent(DotCMIS.Enums.ChangeType.Updated, false);

            Assert.That(transformer.Handle(contentChangeEvent), Is.True);
            queue.Verify(q => q.AddEvent(It.IsAny<FileEvent>()), Times.Once());           
            Assert.That(fileEvent.Remote, Is.EqualTo(MetaDataChangeType.CREATED));
            Assert.That(fileEvent.RemoteContent, Is.EqualTo(ContentChangeType.CREATED));
        }

        [Test, Category("Fast"), Category("ContentChange")]
        public void LocallyExistingRemoteDocumentUpdated ()
        {
            var db = new Mock<IDatabase>();
            db.AddLocalFile();
            FileEvent fileEvent = null;
            var queue = new Mock<ISyncEventQueue>();
            queue.Setup(h => h.AddEvent(It.IsAny<FileEvent>()))
                    .Callback<ISyncEvent>(e => fileEvent = e as FileEvent);

            var transformer = new ContentChangeEventTransformer(queue.Object, db.Object);
            var contentChangeEvent = prepareEvent(DotCMIS.Enums.ChangeType.Updated, false);

            Assert.That(transformer.Handle(contentChangeEvent), Is.True);
            queue.Verify(q => q.AddEvent(It.IsAny<FileEvent>()), Times.Once());           
            Assert.That(fileEvent.Remote, Is.EqualTo(MetaDataChangeType.CHANGED));
            Assert.That(fileEvent.RemoteContent, Is.EqualTo(ContentChangeType.CHANGED));
        }

        [Test, Category("Fast"), Category("ContentChange")]
        public void RemoteDeletionChangeWithoutLocalFile ()
        {
            var db = new Mock<IDatabase>();
            var queue = new Mock<ISyncEventQueue>();

            var transformer = new ContentChangeEventTransformer(queue.Object, db.Object);
            var contentChangeEvent = prepareEvent(DotCMIS.Enums.ChangeType.Deleted, false);

            Assert.That(transformer.Handle(contentChangeEvent), Is.True);
            queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Never());           

        }

        [Test, Category("Fast"), Category("ContentChange")]
        public void RemoteDeletionChangeTest ()
        {
            var db = new Mock<IDatabase>();
            db.AddLocalFile();
            FileEvent fileEvent = null;
            var queue = new Mock<ISyncEventQueue>();
            queue.Setup(h => h.AddEvent(It.IsAny<FileEvent>()))
                    .Callback<ISyncEvent>(e => fileEvent = e as FileEvent);

            var transformer = new ContentChangeEventTransformer(queue.Object, db.Object);
            var contentChangeEvent = prepareEvent(DotCMIS.Enums.ChangeType.Deleted, false);

            Assert.That(transformer.Handle(contentChangeEvent), Is.True);
            queue.Verify(q => q.AddEvent(It.IsAny<FileEvent>()), Times.Once());           
            Assert.That(fileEvent.Remote, Is.EqualTo(MetaDataChangeType.DELETED));
            Assert.That(fileEvent.RemoteContent, Is.EqualTo(ContentChangeType.NONE));
        }

        [Test, Category("Fast"), Category("ContentChange")]
        public void RemoteFolderDeletionWithoutLocalFolder ()
        {
            var db = new Mock<IDatabase>();
            var queue = new Mock<ISyncEventQueue>();

            var transformer = new ContentChangeEventTransformer(queue.Object, db.Object);
            var contentChangeEvent = prepareFolderEvent(DotCMIS.Enums.ChangeType.Deleted);

            Assert.That(transformer.Handle(contentChangeEvent), Is.True);
            queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Never());           
        }

        [Test, Category("Fast"), Category("ContentChange")]
        public void RemoteFolderDeletion ()
        {
            var db = new Mock<IDatabase>();
            db.AddLocalFolder();
            FolderEvent folderEvent = null;
            var queue = new Mock<ISyncEventQueue>();
            queue.Setup(h => h.AddEvent(It.IsAny<FolderEvent>()))
                    .Callback<ISyncEvent>(e => folderEvent = e as FolderEvent);

            var transformer = new ContentChangeEventTransformer(queue.Object, db.Object);
            var contentChangeEvent = prepareFolderEvent(DotCMIS.Enums.ChangeType.Deleted);

            Assert.That(transformer.Handle(contentChangeEvent), Is.True);
            queue.Verify(q => q.AddEvent(It.IsAny<FolderEvent>()), Times.Once());           
            Assert.That(folderEvent.Remote, Is.EqualTo(MetaDataChangeType.DELETED));
        }

        [Test, Category("Fast"), Category("ContentChange")]
        public void RemoteFolderCreation ()
        {
            var db = new Mock<IDatabase>();
            db.AddLocalFolder();
            FolderEvent folderEvent = null;
            var queue = new Mock<ISyncEventQueue>();
            queue.Setup(h => h.AddEvent(It.IsAny<FolderEvent>()))
                    .Callback<ISyncEvent>(e => folderEvent = e as FolderEvent);

            var transformer = new ContentChangeEventTransformer(queue.Object, db.Object);
            var contentChangeEvent = prepareFolderEvent(DotCMIS.Enums.ChangeType.Created);

            Assert.That(transformer.Handle(contentChangeEvent), Is.True);
            queue.Verify(q => q.AddEvent(It.IsAny<FolderEvent>()), Times.Once());           
            Assert.That(folderEvent.Remote, Is.EqualTo(MetaDataChangeType.CREATED));
        }

        [Test, Category("Fast"), Category("ContentChange")]
        public void RemoteFolderUpdate ()
        {
            var db = new Mock<IDatabase>();
            db.AddLocalFolder();
            FolderEvent folderEvent = null;
            var queue = new Mock<ISyncEventQueue>();
            queue.Setup(h => h.AddEvent(It.IsAny<FolderEvent>()))
                    .Callback<ISyncEvent>(e => folderEvent = e as FolderEvent);

            var transformer = new ContentChangeEventTransformer(queue.Object, db.Object);
            var contentChangeEvent = prepareFolderEvent(DotCMIS.Enums.ChangeType.Updated);

            Assert.That(transformer.Handle(contentChangeEvent), Is.True);
            queue.Verify(q => q.AddEvent(It.IsAny<FolderEvent>()), Times.Once());           
            Assert.That(folderEvent.Remote, Is.EqualTo(MetaDataChangeType.CHANGED));
        }

        [Test, Category("Fast"), Category("ContentChange")]
        public void RemoteFolderSecurity ()
        {
            var db = new Mock<IDatabase>();
            db.AddLocalFolder();
            FolderEvent folderEvent = null;
            var queue = new Mock<ISyncEventQueue>();
            queue.Setup(h => h.AddEvent(It.IsAny<FolderEvent>()))
                    .Callback<ISyncEvent>(e => folderEvent = e as FolderEvent);

            var transformer = new ContentChangeEventTransformer(queue.Object, db.Object);
            var contentChangeEvent = prepareFolderEvent(DotCMIS.Enums.ChangeType.Security);

            Assert.That(transformer.Handle(contentChangeEvent), Is.True);
            queue.Verify(q => q.AddEvent(It.IsAny<FolderEvent>()), Times.Once());           
            Assert.That(folderEvent.Remote, Is.EqualTo(MetaDataChangeType.CHANGED));
        }
    }
}
