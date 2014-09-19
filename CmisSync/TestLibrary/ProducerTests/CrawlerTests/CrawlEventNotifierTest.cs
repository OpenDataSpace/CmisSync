//-----------------------------------------------------------------------
// <copyright file="CrawlEventNotifierTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.ProducerTests.CrawlerTests
{
    using System;
    using System.Collections.Generic;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Producer.Crawler;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;

    using Moq;

    using NUnit.Framework;

    using TestUtils;

    [TestFixture]
    public class CrawlEventNotifierTest
    {
        private CrawlEventNotifier underTest;
        private Mock<ISyncEventQueue> queue;
        private CrawlEventCollection collection;

        [SetUp]
        public void SetUp()
        {
            this.queue = new Mock<ISyncEventQueue>();
            this.underTest = new CrawlEventNotifier(this.queue.Object);
            this.collection = new CrawlEventCollection() {
                creationEvents = new List<AbstractFolderEvent>(),
                mergableEvents = new Dictionary<string, Tuple<AbstractFolderEvent, AbstractFolderEvent>>()
            };
        }

        [Test, Category("Fast")]
        public void ConstructorTakesQueue() {
            new CrawlEventNotifier(Mock.Of<ISyncEventQueue>());
        }

        [Test, Category("Fast")]
        public void ConstructorFailsIfGivenQueueIsNull() {
            Assert.Throws<ArgumentNullException>(() => new CrawlEventNotifier(null));
        }

        [Test, Category("Fast")]
        public void NoNotificationIsCreatedAndCallFailsIfEmptyStructIsPassed() {
            CrawlEventCollection emptyCollection = new CrawlEventCollection();
            Assert.Throws<ArgumentNullException>(() => this.underTest.MergeEventsAndAddToQueue(emptyCollection));

            this.queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Never());
        }

        [Test, Category("Fast")]
        public void NoNotificationIsCreatedIfAllEventsAreEmpty() {
            CrawlEventCollection emptyCollection = new CrawlEventCollection() {
                creationEvents = new List<AbstractFolderEvent>(),
                mergableEvents = new Dictionary<string, Tuple<AbstractFolderEvent, AbstractFolderEvent>>()
            };

            this.underTest.MergeEventsAndAddToQueue(emptyCollection);

            this.queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Never());
        }

        [Test, Category("Fast")]
        public void NotifyOneLocalFileCreatedEventToQueue() {
            var file = Mock.Of<IFileInfo>();
            var fileEvent = new FileEvent(file) {
                Local = MetaDataChangeType.CREATED
            };
            this.collection.creationEvents.Add(fileEvent);

            this.underTest.MergeEventsAndAddToQueue(this.collection);

            this.queue.VerifyThatNoOtherEventIsAddedThan<FileEvent>();
            this.queue.Verify(q => q.AddEvent(fileEvent), Times.Once());
            this.queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Exactly(1));
        }

        [Test, Category("Fast")]
        public void NotifyOneLocalFolderCreatedEventToQueue() {
            var dir = Mock.Of<IDirectoryInfo>();
            var dirEvent = new FolderEvent(dir, null) {
                Local = MetaDataChangeType.CREATED
            };
            this.collection.creationEvents.Add(dirEvent);

            this.underTest.MergeEventsAndAddToQueue(this.collection);

            this.queue.VerifyThatNoOtherEventIsAddedThan<FolderEvent>();
            this.queue.Verify(q => q.AddEvent(dirEvent), Times.Once());
            this.queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Exactly(1));
        }

        [Test, Category("Fast")]
        public void NotifyOneRemoteFileCreatedEventToQueue() {
            var doc = Mock.Of<IDocument>();
            var docEvent = new FileEvent(null, doc) {
                Remote = MetaDataChangeType.CREATED
            };
            this.collection.creationEvents.Add(docEvent);

            this.underTest.MergeEventsAndAddToQueue(this.collection);

            this.queue.VerifyThatNoOtherEventIsAddedThan<FileEvent>();
            this.queue.Verify(q => q.AddEvent(docEvent), Times.Once());
            this.queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Exactly(1));
        }

        [Test, Category("Fast")]
        public void NotifyOneRemoteFolderCreatedEventToQueue() {
            var folder = Mock.Of<IFolder>();
            var folderEvent = new FolderEvent(null, folder) {
                Remote = MetaDataChangeType.CREATED
            };
            this.collection.creationEvents.Add(folderEvent);

            this.underTest.MergeEventsAndAddToQueue(this.collection);

            this.queue.VerifyThatNoOtherEventIsAddedThan<FolderEvent>();
            this.queue.Verify(q => q.AddEvent(folderEvent), Times.Once());
            this.queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Exactly(1));
        }

        [Test, Category("Fast")]
        public void NotifyAboutMultipleCreatedEvents() {
            var file = Mock.Of<IFileInfo>();
            var fileEvent = new FileEvent(file) {
                Local = MetaDataChangeType.CREATED
            };
            this.collection.creationEvents.Add(fileEvent);
            var folder = Mock.Of<IFolder>();
            var folderEvent = new FolderEvent(null, folder) {
                Remote = MetaDataChangeType.CREATED
            };
            this.collection.creationEvents.Add(folderEvent);
            var dir = Mock.Of<IDirectoryInfo>();
            var dirEvent = new FolderEvent(dir, null) {
                Local = MetaDataChangeType.CREATED
            };
            this.collection.creationEvents.Add(dirEvent);
            var doc = Mock.Of<IDocument>();
            var docEvent = new FileEvent(null, doc) {
                Remote = MetaDataChangeType.CREATED
            };
            this.collection.creationEvents.Add(docEvent);

            this.underTest.MergeEventsAndAddToQueue(this.collection);

            this.queue.VerifyThatNoOtherEventIsAddedThan<AbstractFolderEvent>();
            this.queue.Verify(q => q.AddEvent(fileEvent), Times.Once());
            this.queue.Verify(q => q.AddEvent(docEvent), Times.Once());
            this.queue.Verify(q => q.AddEvent(folderEvent), Times.Once());
            this.queue.Verify(q => q.AddEvent(dirEvent), Times.Once());
            this.queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Exactly(4));
        }

        [Test, Category("Fast")]
        public void MergeLocalAndRemoteFileDeletionEvents() {
            string remoteId = "remoteId";
            var file = Mock.Of<IFileInfo>();
            var fileEvent = new FileEvent(file) {
                Local = MetaDataChangeType.DELETED
            };
            var doc = Mock.Of<IDocument>();
            var docEvent = new FileEvent(null, doc) {
                Remote = MetaDataChangeType.DELETED
            };
            this.collection.mergableEvents.Add(remoteId, new Tuple<AbstractFolderEvent, AbstractFolderEvent>(fileEvent, docEvent));

            this.underTest.MergeEventsAndAddToQueue(this.collection);
            this.queue.VerifyThatNoOtherEventIsAddedThan<FileEvent>();
            this.queue.Verify(q => q.AddEvent(It.Is<FileEvent>(e => e.LocalFile == file && e.RemoteFile == doc && e.Local == MetaDataChangeType.DELETED && e.Remote == MetaDataChangeType.DELETED)), Times.Once());
            this.queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Exactly(1));
        }

        [Test, Category("Fast")]
        public void NotifyOneLocalFileChangedEvent() {
            var file = Mock.Of<IFileInfo>();
            var fileEvent = new FileEvent(file) {
                Local = MetaDataChangeType.CHANGED
            };
            this.collection.mergableEvents.Add("remoteId", new Tuple<AbstractFolderEvent, AbstractFolderEvent>(fileEvent, null));

            this.underTest.MergeEventsAndAddToQueue(this.collection);

            this.queue.VerifyThatNoOtherEventIsAddedThan<FileEvent>();
            this.queue.Verify(q => q.AddEvent(fileEvent), Times.Once());
            this.queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Exactly(1));
        }

        [Test, Category("Fast")]
        public void MergeLocalMovedAndRemoteDeletionEvents() {
            var file = Mock.Of<IFileInfo>();
            var oldfile = Mock.Of<IFileInfo>();
            var fileEvent = new FileMovedEvent(oldfile, file);
            var doc = Mock.Of<IDocument>();
            var docEvent = new FileEvent(null, doc) {
                Remote = MetaDataChangeType.DELETED
            };
            this.collection.mergableEvents.Add("remoteId", new Tuple<AbstractFolderEvent, AbstractFolderEvent>(fileEvent, docEvent));

            this.underTest.MergeEventsAndAddToQueue(this.collection);
            this.queue.VerifyThatNoOtherEventIsAddedThan<FileEvent>();
            this.queue.Verify(q => q.AddEvent(It.Is<FileEvent>(e => e.LocalFile == file && e.RemoteFile == doc && e.Local == MetaDataChangeType.MOVED && e.Remote == MetaDataChangeType.DELETED)), Times.Once());
            this.queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Exactly(1));
        }

        [Test, Category("Fast")]
        public void NotifyMultipleChangeEvents() {
            var file = Mock.Of<IFileInfo>();
            var oldfile = Mock.Of<IFileInfo>();
            var fileEvent = new FileMovedEvent(oldfile, file);
            var doc = Mock.Of<IDocument>();
            var docEvent = new FileEvent(null, doc) {
                Remote = MetaDataChangeType.DELETED
            };
            var doc2 = Mock.Of<IDocument>();
            var doc2Event = new FileEvent(null, doc2) {
                Remote = MetaDataChangeType.CHANGED
            };
            this.collection.mergableEvents.Add("remoteId", new Tuple<AbstractFolderEvent, AbstractFolderEvent>(fileEvent, docEvent));
            this.collection.mergableEvents.Add("otherRemoteId", new Tuple<AbstractFolderEvent, AbstractFolderEvent>(null, doc2Event));

            this.underTest.MergeEventsAndAddToQueue(this.collection);
            this.queue.VerifyThatNoOtherEventIsAddedThan<FileEvent>();
            this.queue.Verify(q => q.AddEvent(It.Is<FileEvent>(e => e.LocalFile == file && e.RemoteFile == doc && e.Local == MetaDataChangeType.MOVED && e.Remote == MetaDataChangeType.DELETED)), Times.Once());
            this.queue.Verify(q => q.AddEvent(It.Is<FileEvent>(e => e.LocalFile == null && e.RemoteFile == doc2 && e.Local == MetaDataChangeType.NONE && e.Remote == MetaDataChangeType.CHANGED)), Times.Once());
            this.queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Exactly(2));
        }
    }
}