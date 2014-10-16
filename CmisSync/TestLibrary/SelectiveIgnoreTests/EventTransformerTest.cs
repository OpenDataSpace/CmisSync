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
using DotCMIS.Client;
using CmisSync.Lib.PathMatcher;

namespace TestLibrary.SelectiveIgnoreTests
{
    using System;
    using System.Collections.ObjectModel;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.SelectiveIgnore;
    using CmisSync.Lib.Storage.FileSystem;

    using Moq;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture]
    public class EventTransformerTest
    {
        private readonly string ignoredFolderId = "ignoredId";
        private readonly string ignoredLocalPath = "ignoredlocalpath";
        private Mock<ISyncEventQueue> queue;
        private SelectiveIgnoreEventTransformer underTest;
        private ObservableCollection<IIgnoredEntity> ignores;

        [Test, Category("Fast"), Category("SelectiveIgnore")]
        public void ContructorFailsIfQueueIsNull() {
            var ignores = new ObservableCollection<IIgnoredEntity>();
            Assert.Throws<ArgumentNullException>(() => new SelectiveIgnoreEventTransformer(ignores, null));
        }

        [Test, Category("Fast"), Category("SelectiveIgnore")]
        public void ConstructorFailsIfIgnoresAreNull() {
            Assert.Throws<ArgumentNullException>(() => new SelectiveIgnoreEventTransformer(null,  Mock.Of<ISyncEventQueue>()));
        }

        [Test, Category("Fast"), Category("SelectiveIgnore")]
        public void ContructorTakesIgnoresAndQueue() {
            var ignores = new ObservableCollection<IIgnoredEntity>();
            new SelectiveIgnoreEventTransformer(ignores, Mock.Of<ISyncEventQueue>());
        }

        [Test, Category("Fast"), Category("SelectiveIgnore"), Ignore("TODO")]
        public void TransformFileMovedEventToAddedEvent() {
            this.SetupMocks();
            var oldFile = Mock.Of<IFileInfo>();
            var newFile = Mock.Of<IFileInfo>();
            var moveFile = new FileMovedEvent(oldFile, newFile);

            Assert.That(this.underTest.Handle(moveFile), Is.True);

            this.queue.Verify(q => q.AddEvent(It.Is<FileEvent>(e => !e.IsDirectory && e.LocalFile == newFile && e.Local == MetaDataChangeType.CREATED)), Times.Once);
            this.queue.VerifyThatNoOtherEventIsAddedThan<FileEvent>();
        }

        [Test, Category("Fast"), Category("SelectiveIgnore"), Ignore("TODO")]
        public void TransformFileMovedEventToDeletedEvent() {
            this.SetupMocks();
            var oldFile = Mock.Of<IFileInfo>();
            var newFile = Mock.Of<IFileInfo>();
            var moveFile = new FileMovedEvent(oldFile, newFile);

            Assert.That(this.underTest.Handle(moveFile), Is.True);

            this.queue.Verify(q => q.AddEvent(It.Is<FileEvent>(e => !e.IsDirectory && e.LocalFile == newFile && e.Local == MetaDataChangeType.DELETED)), Times.Once);
            this.queue.VerifyThatNoOtherEventIsAddedThan<FileEvent>();
        }

        [Test, Category("Fast"), Category("SelectiveIgnore"), Ignore("TODO")]
        public void TransformFolderMovedEventToAddedEvent() {
            Assert.Fail("TODO");
        }

        [Test, Category("Fast"), Category("SelectiveIgnore"), Ignore("TODO")]
        public void TransformFolderMovedEventToDeletedEvent() {
            Assert.Fail("TODO");
        }

        private void SetupMocks() {
            this.queue = new Mock<ISyncEventQueue>();
            this.ignores = new ObservableCollection<IIgnoredEntity>();
            var ignoredEntity = Mock.Of<IIgnoredEntity>(i => i.LocalPath == this.ignoredLocalPath && i.ObjectId == ignoredFolderId);
            this.ignores.Add(ignoredEntity);
            this.ignores.CollectionChanged += (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) => Assert.Fail();
            this.underTest = new SelectiveIgnoreEventTransformer(this.ignores, this.queue.Object);
        }
    }
}