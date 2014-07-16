//-----------------------------------------------------------------------
// <copyright file="ContentChangeEventTransformerTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.ProducerTests.ContentChangeTests
{
    using System;

    using CmisSync.Lib;
    using CmisSync.Lib.Data;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Storage;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Producer.ContentChange;
    using CmisSync.Lib.Sync.Strategy;

    using DotCMIS.Client;

    using Moq;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture]
    public class ContentChangeEventTransformerTest
    {
        private static readonly string Id = "myId";

        [Test, Category("Fast"), Category("ContentChange")]
        public void ConstructorTest() {
            var storage = new Mock<IMetaDataStorage>();
            var queue  = new Mock<ISyncEventQueue>();
            var transformer = new ContentChangeEventTransformer(queue.Object, storage.Object);
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
            var storage = new Mock<IMetaDataStorage>();
            var queue = new Mock<ISyncEventQueue>();
            var e = new Mock<ISyncEvent>();
            var transformer = new ContentChangeEventTransformer(queue.Object, storage.Object);
            Assert.That(transformer.Handle(e.Object), Is.False);
        }

        [Test, Category("Fast"), Category("ContentChange")]
        [ExpectedException(typeof(InvalidOperationException))]
        public void IgnoreNotAccumulatedNonDeleteEvent()
        {
            var storage = new Mock<IMetaDataStorage>();
            var queue = new Mock<ISyncEventQueue>();
            var e = new ContentChangeEvent(DotCMIS.Enums.ChangeType.Created, Id);
            var transformer = new ContentChangeEventTransformer(queue.Object, storage.Object);
            transformer.Handle(e);
        }

        [Test, Category("Fast"), Category("ContentChange")]
        public void DocumentCreationWithContent() {
            var storage = new Mock<IMetaDataStorage>();
            FileEvent fileEvent = null;
            var queue = new Mock<ISyncEventQueue>();
            queue.Setup(h => h.AddEvent(It.IsAny<FileEvent>()))
                    .Callback<ISyncEvent>(e => fileEvent = e as FileEvent);

            var transformer = new ContentChangeEventTransformer(queue.Object, storage.Object);
            var contentChangeEvent = this.PrepareEvent(DotCMIS.Enums.ChangeType.Created, true);

            Assert.That(transformer.Handle(contentChangeEvent), Is.True);
            queue.Verify(q => q.AddEvent(It.IsAny<FileEvent>()), Times.Once());
            Assert.That(fileEvent.Remote, Is.EqualTo(MetaDataChangeType.CREATED));
            Assert.That(fileEvent.RemoteContent, Is.EqualTo(ContentChangeType.CREATED));
        }

        [Test, Category("Fast"), Category("ContentChange")]
        public void DocumentCreationWithOutContent() {
            var storage = new Mock<IMetaDataStorage>();
            FileEvent fileEvent = null;
            var queue = new Mock<ISyncEventQueue>();
            queue.Setup(h => h.AddEvent(It.IsAny<FileEvent>()))
                    .Callback<ISyncEvent>(e => fileEvent = e as FileEvent);

            var transformer = new ContentChangeEventTransformer(queue.Object, storage.Object);
            var contentChangeEvent = this.PrepareEvent(DotCMIS.Enums.ChangeType.Created, false);

            Assert.That(transformer.Handle(contentChangeEvent), Is.True);
            queue.Verify(q => q.AddEvent(It.IsAny<FileEvent>()), Times.Once());
            Assert.That(fileEvent.Remote, Is.EqualTo(MetaDataChangeType.CREATED));
            Assert.That(fileEvent.RemoteContent, Is.EqualTo(ContentChangeType.NONE));
        }

        [Test, Category("Fast"), Category("ContentChange")]
        public void RemoteSecurityChangeOfExistingFile()
        {
            var storage = new Mock<IMetaDataStorage>();
            storage.AddLocalFile("path", Id);
            FileEvent fileEvent = null;
            var queue = new Mock<ISyncEventQueue>();
            queue.Setup(h => h.AddEvent(It.IsAny<FileEvent>()))
                    .Callback<ISyncEvent>(e => fileEvent = e as FileEvent);

            var transformer = new ContentChangeEventTransformer(queue.Object, storage.Object);
            var contentChangeEvent = this.PrepareEvent(DotCMIS.Enums.ChangeType.Security, false);

            Assert.That(transformer.Handle(contentChangeEvent), Is.True);
            queue.Verify(q => q.AddEvent(It.IsAny<FileEvent>()), Times.Once());
            Assert.That(fileEvent.Remote, Is.EqualTo(MetaDataChangeType.CHANGED));
            Assert.That(fileEvent.RemoteContent, Is.EqualTo(ContentChangeType.NONE));
        }

        [Test, Category("Fast"), Category("ContentChange")]
        public void RemoteSecurityChangeOfNonExistingFile()
        {
            var storage = new Mock<IMetaDataStorage>();
            FileEvent fileEvent = null;
            var queue = new Mock<ISyncEventQueue>();
            queue.Setup(h => h.AddEvent(It.IsAny<FileEvent>()))
                    .Callback<ISyncEvent>(e => fileEvent = e as FileEvent);

            var transformer = new ContentChangeEventTransformer(queue.Object, storage.Object);
            var contentChangeEvent = this.PrepareEvent(DotCMIS.Enums.ChangeType.Security, false);

            Assert.That(transformer.Handle(contentChangeEvent), Is.True);
            queue.Verify(q => q.AddEvent(It.IsAny<FileEvent>()), Times.Once());
            Assert.That(fileEvent.Remote, Is.EqualTo(MetaDataChangeType.CREATED));
            Assert.That(fileEvent.RemoteContent, Is.EqualTo(ContentChangeType.CREATED));
        }

        [Test, Category("Fast"), Category("ContentChange")]
        public void LocallyNotExistingRemoteDocumentUpdated()
        {
            var storage = new Mock<IMetaDataStorage>();
            FileEvent fileEvent = null;
            var queue = new Mock<ISyncEventQueue>();
            queue.Setup(h => h.AddEvent(It.IsAny<FileEvent>()))
                    .Callback<ISyncEvent>(e => fileEvent = e as FileEvent);

            var transformer = new ContentChangeEventTransformer(queue.Object, storage.Object);
            var contentChangeEvent = this.PrepareEvent(DotCMIS.Enums.ChangeType.Updated, false);

            Assert.That(transformer.Handle(contentChangeEvent), Is.True);
            queue.Verify(q => q.AddEvent(It.IsAny<FileEvent>()), Times.Once());
            Assert.That(fileEvent.Remote, Is.EqualTo(MetaDataChangeType.CREATED));
            Assert.That(fileEvent.RemoteContent, Is.EqualTo(ContentChangeType.CREATED));
        }

        [Test, Category("Fast"), Category("ContentChange")]
        public void LocallyExistingRemoteDocumentUpdated()
        {
            string fileName = "file.bin";
            var storage = new Mock<IMetaDataStorage>();
            var file = Mock.Of<IMappedObject>(f =>
                                              f.RemoteObjectId == Id &&
                                              f.Name == fileName &&
                                              f.Type == MappedObjectType.File);
            storage.AddMappedFile(file);
            storage.Setup(s => s.GetLocalPath(It.Is<IMappedObject>(o => o.Equals(file)))).Returns("path");
            FileEvent fileEvent = null;
            var queue = new Mock<ISyncEventQueue>();
            queue.Setup(h => h.AddEvent(It.IsAny<FileEvent>()))
                    .Callback<ISyncEvent>(e => fileEvent = e as FileEvent);

            var transformer = new ContentChangeEventTransformer(queue.Object, storage.Object);
            var contentChangeEvent = this.PrepareEvent(DotCMIS.Enums.ChangeType.Updated, false);

            Assert.That(transformer.Handle(contentChangeEvent), Is.True);
            queue.Verify(q => q.AddEvent(It.IsAny<FileEvent>()), Times.Once());
            Assert.That(fileEvent.Remote, Is.EqualTo(MetaDataChangeType.CHANGED));
            Assert.That(fileEvent.RemoteContent, Is.EqualTo(ContentChangeType.CHANGED));
        }

        [Test, Category("Fast"), Category("ContentChange")]
        public void RemoteDeletionChangeWithoutLocalFile()
        {
            var storage = new Mock<IMetaDataStorage>();
            var queue = new Mock<ISyncEventQueue>();

            var transformer = new ContentChangeEventTransformer(queue.Object, storage.Object);
            var contentChangeEvent = this.PrepareEvent(DotCMIS.Enums.ChangeType.Deleted, false);

            Assert.That(transformer.Handle(contentChangeEvent), Is.True);
            queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Never());
        }

        [Test, Category("Fast"), Category("ContentChange")]
        public void RemoteDeletionChangeTest()
        {
            var storage = new Mock<IMetaDataStorage>();
            storage.AddLocalFile("path", Id);
            FileEvent fileEvent = null;
            var queue = new Mock<ISyncEventQueue>();
            queue.Setup(h => h.AddEvent(It.IsAny<FileEvent>()))
                    .Callback<ISyncEvent>(e => fileEvent = e as FileEvent);

            var transformer = new ContentChangeEventTransformer(queue.Object, storage.Object);
            var contentChangeEvent = this.PrepareEvent(DotCMIS.Enums.ChangeType.Deleted, false);

            Assert.That(transformer.Handle(contentChangeEvent), Is.True);
            queue.Verify(q => q.AddEvent(It.IsAny<FileEvent>()), Times.Once());
            Assert.That(fileEvent.Remote, Is.EqualTo(MetaDataChangeType.DELETED));
            Assert.That(fileEvent.RemoteContent, Is.EqualTo(ContentChangeType.NONE));
        }

        [Test, Category("Fast"), Category("ContentChange")]
        public void RemoteFolderDeletionWithoutLocalFolder()
        {
            var storage = new Mock<IMetaDataStorage>();
            var queue = new Mock<ISyncEventQueue>();

            var transformer = new ContentChangeEventTransformer(queue.Object, storage.Object);
            var contentChangeEvent = this.PrepareFolderEvent(DotCMIS.Enums.ChangeType.Deleted);

            Assert.That(transformer.Handle(contentChangeEvent), Is.True);
            queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Never());
        }

        [Test, Category("Fast"), Category("ContentChange")]
        public void RemoteFolderDeletion()
        {
            var storage = new Mock<IMetaDataStorage>();
            storage.AddLocalFolder("path", Id);
            FolderEvent folderEvent = null;
            var queue = new Mock<ISyncEventQueue>();
            queue.Setup(h => h.AddEvent(It.IsAny<FolderEvent>()))
                    .Callback<ISyncEvent>(e => folderEvent = e as FolderEvent);

            var transformer = new ContentChangeEventTransformer(queue.Object, storage.Object);
            var contentChangeEvent = this.PrepareFolderEvent(DotCMIS.Enums.ChangeType.Deleted);

            Assert.That(transformer.Handle(contentChangeEvent), Is.True);
            queue.Verify(q => q.AddEvent(It.IsAny<FolderEvent>()), Times.Once());
            Assert.That(folderEvent.Remote, Is.EqualTo(MetaDataChangeType.DELETED));
        }

        [Test, Category("Fast"), Category("ContentChange")]
        public void RemoteFolderCreation()
        {
            var storage = new Mock<IMetaDataStorage>();
            storage.AddLocalFolder("path", Id);
            FolderEvent folderEvent = null;
            var queue = new Mock<ISyncEventQueue>();
            queue.Setup(h => h.AddEvent(It.IsAny<FolderEvent>()))
                    .Callback<ISyncEvent>(e => folderEvent = e as FolderEvent);

            var transformer = new ContentChangeEventTransformer(queue.Object, storage.Object);
            var contentChangeEvent = this.PrepareFolderEvent(DotCMIS.Enums.ChangeType.Created);

            Assert.That(transformer.Handle(contentChangeEvent), Is.True);
            queue.Verify(q => q.AddEvent(It.IsAny<FolderEvent>()), Times.Once());
            Assert.That(folderEvent.Remote, Is.EqualTo(MetaDataChangeType.CREATED));
        }

        [Test, Category("Fast"), Category("ContentChange")]
        public void RemoteFolderUpdate()
        {
            var storage = new Mock<IMetaDataStorage>();
            storage.AddLocalFolder("path", Id);
            FolderEvent folderEvent = null;
            var queue = new Mock<ISyncEventQueue>();
            queue.Setup(h => h.AddEvent(It.IsAny<FolderEvent>()))
                    .Callback<ISyncEvent>(e => folderEvent = e as FolderEvent);

            var transformer = new ContentChangeEventTransformer(queue.Object, storage.Object);
            var contentChangeEvent = this.PrepareFolderEvent(DotCMIS.Enums.ChangeType.Updated);

            Assert.That(transformer.Handle(contentChangeEvent), Is.True);
            queue.Verify(q => q.AddEvent(It.IsAny<FolderEvent>()), Times.Once());
            Assert.That(folderEvent.Remote, Is.EqualTo(MetaDataChangeType.CHANGED));
        }

        [Test, Category("Fast"), Category("ContentChange")]
        public void RemoteFolderSecurity()
        {
            var storage = new Mock<IMetaDataStorage>();
            storage.AddLocalFolder("path", Id);
            FolderEvent folderEvent = null;
            var queue = new Mock<ISyncEventQueue>();
            queue.Setup(h => h.AddEvent(It.IsAny<FolderEvent>()))
                    .Callback<ISyncEvent>(e => folderEvent = e as FolderEvent);

            var transformer = new ContentChangeEventTransformer(queue.Object, storage.Object);
            var contentChangeEvent = this.PrepareFolderEvent(DotCMIS.Enums.ChangeType.Security);

            Assert.That(transformer.Handle(contentChangeEvent), Is.True);
            queue.Verify(q => q.AddEvent(It.IsAny<FolderEvent>()), Times.Once());
            Assert.That(folderEvent.Remote, Is.EqualTo(MetaDataChangeType.CHANGED));
        }

        private ContentChangeEvent PrepareEvent(DotCMIS.Enums.ChangeType type, bool hasContentStream) {
            var e = new ContentChangeEvent(type, Id);
            var remoteObject = MockOfIDocumentUtil.CreateRemoteDocumentMock(hasContentStream ? "streamId" : null, Id, "name", (string)null);
            var session = new Mock<ISession>();
            session.Setup(s => s.GetObject(It.IsAny<string>(), It.IsAny<IOperationContext>())).Returns(remoteObject.Object);

            e.UpdateObject(session.Object);
            return e;
        }

        private ContentChangeEvent PrepareFolderEvent(DotCMIS.Enums.ChangeType type) {
            var e = new ContentChangeEvent(type, Id);
            var remoteObject = new Mock<IFolder>();
            var session = new Mock<ISession>();
            session.Setup(s => s.GetObject(It.IsAny<string>(), It.IsAny<IOperationContext>())).Returns(remoteObject.Object);

            e.UpdateObject(session.Object);
            return e;
        }
    }
}