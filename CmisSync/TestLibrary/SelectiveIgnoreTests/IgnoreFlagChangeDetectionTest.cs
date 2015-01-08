//-----------------------------------------------------------------------
// <copyright file="IgnoreFlagChangeDetectionTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.SelectiveIgnoreTests
{
    using System;
    using System.IO;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.PathMatcher;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.SelectiveIgnore;

    using DotCMIS.Client;
    using DotCMIS.Enums;

    using Moq;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture]
    public class IgnoreFlagChangeDetectionTest
    {
        private readonly string folderName = "name";
        private string folderId;
        private string remotePath;
        private string localPath;
        private Mock<IIgnoredEntitiesStorage> ignoreStorage;
        private Mock<IPathMatcher> matcher;
        private Mock<ISyncEventQueue> queue;
        private Mock<ISession> session;
        private IgnoreFlagChangeDetection underTest;

        [Test, Category("Fast"), Category("SelectiveIgnore")]
        public void DefaultConstructor() {
            this.SetUpMocks();
            this.queue.VerifyThatNoEventIsAdded();
        }

        [Test, Category("Fast"), Category("SelectiveIgnore")]
        public void ConstructorFailsIfMatcherIsNull() {
            this.SetUpMocks();
            Assert.Throws<ArgumentNullException>(() => new IgnoreFlagChangeDetection(this.ignoreStorage.Object, null, this.queue.Object));
            this.queue.VerifyThatNoEventIsAdded();
        }

        [Test, Category("Fast"), Category("SelectiveIgnore")]
        public void ConstructorFailsIfStorageIsNull() {
            this.SetUpMocks();
            Assert.Throws<ArgumentNullException>(() => new IgnoreFlagChangeDetection(null, this.matcher.Object, this.queue.Object));
            this.queue.VerifyThatNoEventIsAdded();
        }

        [Test, Category("Fast"), Category("SelectiveIgnore")]
        public void DetectionIgnoresNonContentChangeEvents() {
            this.SetUpMocks();
            var underTest = new IgnoreFlagChangeDetection(this.ignoreStorage.Object, this.matcher.Object, this.queue.Object);
            Assert.That(underTest.Handle(Mock.Of<ISyncEvent>()), Is.False);
        }

        [Test, Category("Fast"), Category("SelectiveIgnore")]
        public void CreatedEventForIgnoredObject() {
            this.SetUpMocks();
            var createdObject = MockOfIFolderUtil.CreateRemoteFolderMock(this.folderId, this.folderName, this.remotePath, Guid.NewGuid().ToString());
            createdObject.SetupIgnore("*");
            var createdEvent = new ContentChangeEvent(ChangeType.Created, this.folderId);
            this.session.Setup(s => s.GetObject(It.IsAny<string>(), It.IsAny<IOperationContext>())).Returns(createdObject.Object);
            createdEvent.UpdateObject(this.session.Object);
            this.matcher.Setup(m => m.CanCreateLocalPath(createdObject.Object)).Returns(true);
            this.matcher.Setup(m => m.CreateLocalPath(createdObject.Object)).Returns(this.localPath);

            Assert.That(this.underTest.Handle(createdEvent), Is.False);

            this.ignoreStorage.Verify(s => s.AddOrUpdateEntryAndDeleteAllChildrenFromStorage(It.Is<IIgnoredEntity>(e => e.LocalPath == this.localPath && e.ObjectId == this.folderId)));
            this.queue.VerifyThatNoEventIsAdded();
        }

        [Test, Category("Fast"), Category("SelectiveIgnore")]
        public void CreatedEventForNotIgnoredObject() {
            this.SetUpMocks();
            var createdObject = MockOfIFolderUtil.CreateRemoteFolderMock(this.folderId, this.folderName, this.remotePath, Guid.NewGuid().ToString());
            var createdEvent = new ContentChangeEvent(ChangeType.Created, this.folderId);
            this.session.Setup(s => s.GetObject(It.IsAny<string>(), It.IsAny<IOperationContext>())).Returns(createdObject.Object);
            createdEvent.UpdateObject(this.session.Object);

            Assert.That(this.underTest.Handle(createdEvent), Is.False);

            this.ignoreStorage.Verify(s => s.AddOrUpdateEntryAndDeleteAllChildrenFromStorage(It.IsAny<IIgnoredEntity>()), Times.Never());
            this.queue.VerifyThatNoEventIsAdded();
        }

        [Test, Category("Fast"), Category("SelectiveIgnore")]
        public void ChangedEventForNewlyIgnoredObject() {
            this.SetUpMocks();
            var changedObject = MockOfIFolderUtil.CreateRemoteFolderMock(this.folderId, this.folderName, this.remotePath, Guid.NewGuid().ToString());
            changedObject.SetupIgnore("*");
            var changeEvent = new ContentChangeEvent(ChangeType.Updated, this.folderId);
            this.session.Setup(s => s.GetObject(It.IsAny<string>(), It.IsAny<IOperationContext>())).Returns(changedObject.Object);
            changeEvent.UpdateObject(this.session.Object);
            this.matcher.Setup(m => m.CanCreateLocalPath(changedObject.Object)).Returns(true);
            this.matcher.Setup(m => m.CreateLocalPath(changedObject.Object)).Returns(this.localPath);

            Assert.That(this.underTest.Handle(changeEvent), Is.False);

            this.ignoreStorage.Verify(s => s.AddOrUpdateEntryAndDeleteAllChildrenFromStorage(It.Is<IIgnoredEntity>(e => e.LocalPath == this.localPath && e.ObjectId == this.folderId)));
            this.queue.VerifyThatNoEventIsAdded();
        }

        [Test, Category("Fast"), Category("SelectiveIgnore")]
        public void ChangedEventForUnchangedIgnoredStateObject() {
            this.SetUpMocks();
            var changedObject = MockOfIFolderUtil.CreateRemoteFolderMock(this.folderId, this.folderName, this.remotePath, Guid.NewGuid().ToString());
            var changeEvent = new ContentChangeEvent(ChangeType.Updated, this.folderId);
            this.session.Setup(s => s.GetObject(It.IsAny<string>(), It.IsAny<IOperationContext>())).Returns(changedObject.Object);
            changeEvent.UpdateObject(this.session.Object);

            Assert.That(this.underTest.Handle(changeEvent), Is.False);

            this.ignoreStorage.Verify(s => s.AddOrUpdateEntryAndDeleteAllChildrenFromStorage(It.IsAny<IIgnoredEntity>()), Times.Never());
            this.queue.VerifyThatNoEventIsAdded();
        }

        [Test, Category("Fast"), Category("SelectiveIgnore")]
        public void ChangeEventForFormerIgnoredObjectAndNowNotIgnoredObject() {
            this.SetUpMocks();
            var changedObject = MockOfIFolderUtil.CreateRemoteFolderMock(this.folderId, this.folderName, this.remotePath, Guid.NewGuid().ToString());
            var changeEvent = new ContentChangeEvent(ChangeType.Updated, this.folderId);
            this.session.Setup(s => s.GetObject(It.IsAny<string>(), It.IsAny<IOperationContext>())).Returns(changedObject.Object);
            this.ignoreStorage.Setup(i => i.IsIgnoredId(this.folderId)).Returns(IgnoredState.IGNORED);
            changeEvent.UpdateObject(this.session.Object);

            Assert.That(this.underTest.Handle(changeEvent), Is.False);

            this.ignoreStorage.Verify(s => s.AddOrUpdateEntryAndDeleteAllChildrenFromStorage(It.IsAny<IIgnoredEntity>()), Times.Never());
            this.ignoreStorage.Verify(s => s.Remove(this.folderId));
            this.queue.Verify(q => q.AddEvent(It.Is<StartNextSyncEvent>(e => e.FullSyncRequested == true)), Times.Once());
            this.queue.VerifyThatNoOtherEventIsAddedThan<StartNextSyncEvent>();
        }

        [Test, Category("Fast"), Category("SelectiveIgnore")]
        public void DeleteEventForANotIgnoredObject() {
            this.SetUpMocks();
            var deleteEvent = new ContentChangeEvent(ChangeType.Deleted, this.folderId);

            Assert.That(this.underTest.Handle(deleteEvent), Is.False);

            this.ignoreStorage.Verify(i => i.Remove(It.IsAny<string>()), Times.Never());
            this.queue.VerifyThatNoEventIsAdded();
        }

        [Test, Category("Fast"), Category("SelectiveIgnore")]
        public void DeleteEventForAFormerIgnoredObject() {
            this.SetUpMocks();
            this.ignoreStorage.Setup(i => i.IsIgnoredId(this.folderId)).Returns(IgnoredState.IGNORED);
            var deleteEvent = new ContentChangeEvent(ChangeType.Deleted, this.folderId);

            Assert.That(this.underTest.Handle(deleteEvent), Is.False);

            this.ignoreStorage.Verify(i => i.Remove(this.folderId), Times.Once());
            this.queue.Verify(q => q.AddEvent(It.Is<StartNextSyncEvent>(e => e.FullSyncRequested == true)));
            this.queue.VerifyThatNoOtherEventIsAddedThan<StartNextSyncEvent>();
        }

        private void SetUpMocks() {
            this.ignoreStorage = new Mock<IIgnoredEntitiesStorage>();
            this.matcher = new Mock<IPathMatcher>();
            this.queue = new Mock<ISyncEventQueue>();
            this.session = new Mock<ISession>();
            this.folderId = Guid.NewGuid().ToString();
            this.remotePath = "/" + this.folderName;
            this.localPath = Path.Combine(Path.GetTempPath(), this.folderName);
            this.underTest = new IgnoreFlagChangeDetection(this.ignoreStorage.Object, this.matcher.Object, this.queue.Object);
        }
    }
}