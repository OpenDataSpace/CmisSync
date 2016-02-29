//-----------------------------------------------------------------------
// <copyright file="RegexIgnoreEventTransformerTest.cs" company="GRAU DATA AG">
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
﻿
namespace TestLibrary.FilterTests.RegexFilterTests {
    using System;
    using System.IO;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Filter;
    using CmisSync.Lib.Filter.RegexIgnore;
    using CmisSync.Lib.PathMatcher;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.FileSystem;
    using CmisSync.Lib.Storage.Database;

    using Moq;

    using NUnit.Framework;

    using TestUtils;

    [TestFixture, Category("Fast")]
    public class RegexIgnoreEventTransformerTest {
        private Mock<ISyncEventQueue> queue;
        private Mock<IgnoredFolderNameFilter> filter;
        private Mock<IMetaDataStorage> storage;
        private Mock<IPathMatcher> matcher;
        [Test]
        public void Constructor() {
            var underTest = SetUpMocksAndCreateTransformer();
            Assert.That(underTest.Priority, Is.Positive);
        }

        [Test]
        public void ConstructorFailsIfFilterIsNull() {
            SetUpMocksAndCreateTransformer();
            Assert.Throws<ArgumentNullException>(() => new RegexIgnoreEventTransformer(null, queue.Object, matcher.Object, storage.Object));
        }

        [Test]
        public void ConstructorFailsIfQueueIsNull() {
            SetUpMocksAndCreateTransformer();
            Assert.Throws<ArgumentNullException>(() => new RegexIgnoreEventTransformer(filter.Object, null, matcher.Object, storage.Object));
        }

        [Test]
        public void ConstructorFailsIfMatcherIsNull() {
            SetUpMocksAndCreateTransformer();
            Assert.Throws<ArgumentNullException>(() => new RegexIgnoreEventTransformer(filter.Object, queue.Object, null, storage.Object));
        }

        [Test]
        public void ConstructorFailsIfStorageIsNull() {
            SetUpMocksAndCreateTransformer();
            Assert.Throws<ArgumentNullException>(() => new RegexIgnoreEventTransformer(filter.Object, queue.Object, matcher.Object, null));
        }

        [Test]
        public void TransformsFSMoveEventToDeletedEventIfTargetIsIgnored([Values(true, false)]bool isDirectory) {
            var underTest = SetUpMocksAndCreateTransformer();
            var oldPath = isDirectory ? new DirectoryInfo("sourcePath").FullName : new FileInfo("sourcePath").FullName;
            var newPath = "targetPath";
            FSMovedEvent moved = new FSMovedEvent(oldPath, newPath, isDirectory);
            string reason;
            this.filter.Setup(f => f.CheckFolderPath(moved.LocalPath, out reason)).Returns(true);
            this.filter.Setup(f => f.CheckFolderPath(moved.OldPath, out reason)).Returns(false);

            Assert.That(underTest.Handle(moved), Is.True);

            queue.VerifyThatNoOtherEventIsAddedThan<FSEvent>();
            queue.Verify(q => q.AddEvent(It.IsAny<FSEvent>()), Times.Once());
            Console.WriteLine(moved.OldPath);
            queue.Verify(q => q.AddEvent(It.Is<FSEvent>(e => e.LocalPath.Equals(moved.OldPath) && e.Type == WatcherChangeTypes.Deleted && e.IsDirectory == isDirectory)));
        }

        [Test]
        public void TransformsFSMoveEventToCreatedEventAndTriggerCrawlSyncIfSourceIsIgnored([Values(true, false)]bool isDirectory) {
            var underTest = SetUpMocksAndCreateTransformer();
            var oldPath = isDirectory ? new DirectoryInfo("sourcePath").FullName : new FileInfo("sourcePath").FullName;
            var newPath = "targetPath";
            FSMovedEvent moved = new FSMovedEvent(oldPath, newPath, isDirectory);
            string reason;
            this.filter.Setup(f => f.CheckFolderPath(moved.LocalPath, out reason)).Returns(false);
            this.filter.Setup(f => f.CheckFolderPath(moved.OldPath, out reason)).Returns(true);

            Assert.That(underTest.Handle(moved), Is.True);

            queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Exactly(2));
            queue.Verify(q => q.AddEvent(It.Is<StartNextSyncEvent>(e => e.FullSyncRequested == true)), Times.Once());
            queue.Verify(q => q.AddEvent(It.Is<FSEvent>(e => e.LocalPath.Equals(moved.LocalPath) && e.Type == WatcherChangeTypes.Created && e.IsDirectory == isDirectory)), Times.Once());
        }

        private RegexIgnoreEventTransformer SetUpMocksAndCreateTransformer() {
            this.queue = new Mock<ISyncEventQueue>();
            this.filter = new Mock<IgnoredFolderNameFilter>(Mock.Of<IDirectoryInfo>());
            this.storage = new Mock<IMetaDataStorage>();
            this.matcher = new Mock<IPathMatcher>();
            return new RegexIgnoreEventTransformer(
                filter.Object, queue.Object, matcher.Object, storage.Object);
        }
    }
}