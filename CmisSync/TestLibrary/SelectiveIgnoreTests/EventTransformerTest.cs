//-----------------------------------------------------------------------
// <copyright file="EventTransformerTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.SelectiveIgnoreTests {
    using System;
    using System.Collections.ObjectModel;
    using System.IO;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.SelectiveIgnore;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;

    using Moq;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture, Category("Fast"), Category("SelectiveIgnore")]
    public class EventTransformerTest {
        private readonly string ignoredFolderId = "ignoredId";
        private readonly string ignoredLocalPath = Path.Combine(Path.GetTempPath(), "ignoredlocalpath");
        private Mock<ISyncEventQueue> queue;
        private SelectiveIgnoreEventTransformer underTest;
        private Mock<IIgnoredEntitiesStorage> ignores;

        [Test]
        public void ContructorFailsIfQueueIsNull() {
            var ignores = Mock.Of<IIgnoredEntitiesStorage>();
            Assert.Throws<ArgumentNullException>(() => new SelectiveIgnoreEventTransformer(ignores, null));
        }

        [Test]
        public void ConstructorFailsIfIgnoresAreNull() {
            Assert.Throws<ArgumentNullException>(() => new SelectiveIgnoreEventTransformer(null,  Mock.Of<ISyncEventQueue>()));
        }

        [Test]
        public void ContructorTakesIgnoresAndQueue() {
            var ignores = Mock.Of<IIgnoredEntitiesStorage>();
            new SelectiveIgnoreEventTransformer(ignores, Mock.Of<ISyncEventQueue>());
        }

        [Test]
        public void TransformFileMovedEventToAddedEvent() {
            this.SetupMocks();
            string fileName = "file.txt";
            var oldFile = Path.Combine(this.ignoredLocalPath, fileName);
            var newFile = Path.Combine(Path.GetTempPath(), fileName);
            var moveFile = new FSMovedEvent(oldFile, newFile, false);

            Assert.That(this.underTest.Handle(moveFile), Is.True);

            this.queue.Verify(q => q.AddEvent(It.Is<FSEvent>(e => !e.IsDirectory && e.LocalPath == newFile && e.Type == WatcherChangeTypes.Created)), Times.Once);
            this.queue.VerifyThatNoOtherEventIsAddedThan<FSEvent>();
        }

        [Test]
        public void TransformFSMovedEventToDeletedEvent() {
            this.SetupMocks();
            string fileName = "file.txt";
            var oldFile = Path.Combine(Path.GetTempPath(), fileName);
            var newFile = Path.Combine(this.ignoredLocalPath, fileName);
            var moveFile = new FSMovedEvent(oldFile, newFile, false);

            Assert.That(this.underTest.Handle(moveFile), Is.True);

            this.queue.Verify(q => q.AddEvent(It.Is<FSEvent>(e => !e.IsDirectory && e.LocalPath == oldFile && e.Type == WatcherChangeTypes.Deleted)), Times.Once);
            this.queue.VerifyThatNoOtherEventIsAddedThan<FSEvent>();
        }

        [Test]
        public void TransformFSFolderMovedEventToAddedEvent() {
            this.SetupMocks();
            string fileName = "folder";
            var oldFile = Path.Combine(this.ignoredLocalPath, fileName);
            var newFile = Path.Combine(Path.GetTempPath(), fileName);
            var moveFile = new FSMovedEvent(oldFile, newFile, true);

            Assert.That(this.underTest.Handle(moveFile), Is.True);

            this.queue.Verify(q => q.AddEvent(It.Is<FSEvent>(e => e.IsDirectory && e.LocalPath == newFile && e.Type == WatcherChangeTypes.Created)), Times.Once);
            this.queue.VerifyThatNoOtherEventIsAddedThan<FSEvent>();
        }

        [Test]
        public void TransformFSFolderMovedEventToDeletedEvent() {
            this.SetupMocks();
            string fileName = "folder";
            var oldFile = Path.Combine(Path.GetTempPath(), fileName);
            var newFile = Path.Combine(this.ignoredLocalPath, fileName);
            var moveFile = new FSMovedEvent(oldFile, newFile, true);

            Assert.That(this.underTest.Handle(moveFile), Is.True);

            this.queue.Verify(q => q.AddEvent(It.Is<FSEvent>(e => e.IsDirectory && e.LocalPath == oldFile && e.Type == WatcherChangeTypes.Deleted)), Times.Once);
            this.queue.VerifyThatNoOtherEventIsAddedThan<FSEvent>();
        }

        [Test]
        public void DoNotTransformFSIgnoredFolderMovedEventToAddedEvent() {
            this.SetupMocks();
            string fileName = "folder";
            var oldFile = Path.Combine(this.ignoredLocalPath);
            var newFile = Path.Combine(Path.GetTempPath(), fileName);
            var moveFile = new FSMovedEvent(oldFile, newFile, true);

            Assert.That(this.underTest.Handle(moveFile), Is.False);

            this.queue.VerifyThatNoEventIsAdded();
        }

        [Test]
        public void TransformContentChangeEventFromChangeToDeleteIfTargetIsInsideAnIgnoredFolder() {
            this.SetupMocks();
            var objectId = Guid.NewGuid().ToString();
            var folderObject = Mock.Of<IFolder>(f => f.Id == objectId);
            var contentChangeEvent = new ContentChangeEvent(DotCMIS.Enums.ChangeType.Updated, objectId);
            var session = new Mock<ISession>();
            session.Setup(s => s.GetObject(objectId, It.IsAny<IOperationContext>())).Returns(folderObject);
            contentChangeEvent.UpdateObject(session.Object);
            this.ignores.Setup(i => i.IsIgnored(folderObject)).Returns(IgnoredState.INHERITED);

            Assert.That(this.underTest.Handle(contentChangeEvent), Is.True);

            this.queue.VerifyThatNoOtherEventIsAddedThan<ContentChangeEvent>();
            this.queue.Verify(q => q.AddEvent(It.Is<ContentChangeEvent>(e => e.ObjectId == objectId && e.Type == DotCMIS.Enums.ChangeType.Deleted)));
        }

        [Test]
        public void DoNotTouchDeletedContentChangeEvents() {
            this.SetupMocks();
            var contentChangeEvent = new ContentChangeEvent(DotCMIS.Enums.ChangeType.Deleted, Guid.NewGuid().ToString());

            Assert.That(this.underTest.Handle(contentChangeEvent), Is.False);

            this.queue.VerifyThatNoEventIsAdded();
        }

        private void SetupMocks() {
            this.queue = new Mock<ISyncEventQueue>();
            this.ignores = new Mock<IIgnoredEntitiesStorage>();
            this.ignores.Setup(i => i.IsIgnoredPath(It.Is<string>(s => !s.Contains(this.ignoredLocalPath)))).Returns(IgnoredState.NOT_IGNORED);
            this.ignores.Setup(i => i.IsIgnoredPath(this.ignoredLocalPath)).Returns(IgnoredState.IGNORED);
            this.ignores.Setup(i => i.IsIgnoredPath(It.Is<string>(s => s.StartsWith(this.ignoredLocalPath) && s != this.ignoredLocalPath))).Returns(IgnoredState.INHERITED);
            this.ignores.Setup(i => i.IsIgnoredId(this.ignoredFolderId)).Returns(IgnoredState.IGNORED);
            this.underTest = new SelectiveIgnoreEventTransformer(this.ignores.Object, this.queue.Object);
        }
    }
}