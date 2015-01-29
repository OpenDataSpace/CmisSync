//-----------------------------------------------------------------------
// <copyright file="FilterTest.cs" company="GRAU DATA AG">
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
    using System.Collections.ObjectModel;
    using System.IO;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.SelectiveIgnore;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;
    using DotCMIS.Enums;
    using DotCMIS.Exceptions;

    using Moq;

    using NUnit.Framework;

    using TestUtils;

    [TestFixture]
    public class FilterTest
    {
        private readonly string ignoredObjectId = "ignoredObjectId";
        private readonly string ignoredPath = Path.Combine(Path.GetTempPath(), "IgnoredLocalPath");
        private Mock<IIgnoredEntitiesStorage> storage;
        private SelectiveIgnoreFilter underTest;

        [Test, Category("Fast"), Category("SelectiveIgnore")]
        public void ConstructorFailsIfStorageIsNull() {
            Assert.Throws<ArgumentNullException>(
                () => new SelectiveIgnoreFilter(
                null));
        }

        [Test, Category("Fast"), Category("SelectiveIgnore")]
        public void ConstructorTest() {
            new SelectiveIgnoreFilter(
                Mock.Of<IIgnoredEntitiesStorage>());
        }

        [Test, Category("Fast"), Category("SelectiveIgnore")]
        public void FilterLocalFSAddedEvents() {
            this.SetupMocks();
            var fileEvent = new FSEvent(WatcherChangeTypes.Created, Path.Combine(this.ignoredPath, "file.txt"), false);

            Assert.That(this.underTest.Handle(fileEvent), Is.True);
        }

        [Test, Category("Fast"), Category("SelectiveIgnore")]
        public void DoNotFilterValidLocalFSAddedEvents() {
            this.SetupMocks();
            var fileEvent = new FSEvent(WatcherChangeTypes.Created, Path.Combine(Path.GetTempPath(), "file.txt"), false);

            Assert.That(this.underTest.Handle(fileEvent), Is.False);
        }

        [Test, Category("Fast"), Category("SelectiveIgnore")]
        public void DoNotFilterValidLocalFileAddedEvents() {
            this.SetupMocks();
            IFileInfo file = Mock.Of<IFileInfo>(f => f.FullName == Path.Combine(Path.GetTempPath(), "file.txt"));
            var fileEvent = new FileEvent(file) { Local = MetaDataChangeType.CREATED };

            Assert.That(this.underTest.Handle(fileEvent), Is.False);
        }

        [Test, Category("Fast"), Category("SelectiveIgnore")]
        public void FilterLocalFSDeletionEvents() {
            this.SetupMocks();
            var fileEvent = new FSEvent(WatcherChangeTypes.Deleted, Path.Combine(this.ignoredPath, "file.txt"), false);

            Assert.That(this.underTest.Handle(fileEvent), Is.True);
        }

        [Test, Category("Fast"), Category("SelectiveIgnore")]
        public void FilterLocalFSRenamedEvents() {
            this.SetupMocks();
            var fileEvent = new FSMovedEvent(Path.Combine(this.ignoredPath, "old_file.txt"), Path.Combine(this.ignoredPath, "file.txt"), false);

            Assert.That(this.underTest.Handle(fileEvent), Is.True);
        }

        [Test, Category("Fast"), Category("SelectiveIgnore")]
        public void FilterRemoteObjectCreatedEvents() {
            this.SetupMocks();
            var doc = Mock.Of<IDocument>();
            this.storage.Setup(s => s.IsIgnored(doc)).Returns(IgnoredState.INHERITED);
            var fileEvent = new FileEvent(null, doc) { Remote = MetaDataChangeType.CREATED };

            Assert.That(this.underTest.Handle(fileEvent), Is.True);
        }

        [Test, Category("Fast"), Category("SelectiveIgnore")]
        public void FilterRemoteObjectDeletedEvents() {
            this.SetupMocks();
            var fileEvent = new FileEvent(Mock.Of<IFileInfo>(f => f.FullName == Path.Combine(this.ignoredPath, "file.txt")), null) { Remote = MetaDataChangeType.DELETED };

            Assert.That(this.underTest.Handle(fileEvent), Is.True);
        }

        [Test, Category("Fast"), Category("SelectiveIgnore")]
        public void DoNotFilterIgnoredFolderRenameEvent() {
            this.SetupMocks();
            var folderEvent = new FolderMovedEvent(Mock.Of<IDirectoryInfo>(d => d.FullName == this.ignoredPath), Mock.Of<IDirectoryInfo>(d => d.FullName == Path.Combine(Path.GetTempPath(), "newPath")), null, null, null);

            Assert.That(this.underTest.Handle(folderEvent), Is.False);
        }

        [Test, Category("Fast"), Category("SelectiveIgnore")]
        public void DoNotFilterIgnoredLocalFolderChangedEvent() {
            this.SetupMocks();
            var folderEvent = new FolderEvent(Mock.Of<IDirectoryInfo>(d => d.FullName == this.ignoredPath), null) { Local = MetaDataChangeType.CHANGED };

            Assert.That(this.underTest.Handle(folderEvent), Is.False);
        }

        [Test, Category("Fast"), Category("SelectiveIgnore")]
        public void DoNotFilterIgnoredLocalFolderDeleteEvent() {
            this.SetupMocks();
            var folderEvent = new FolderEvent(Mock.Of<IDirectoryInfo>(d => d.FullName == this.ignoredPath), null) { Local = MetaDataChangeType.DELETED };

            Assert.That(this.underTest.Handle(folderEvent), Is.False);
        }

        [Test, Category("Fast"), Category("SelectiveIgnore")]
        public void DoNotFilterIgnoredLocalFolderAddEvent() {
            this.SetupMocks();
            var folderEvent = new FolderEvent(Mock.Of<IDirectoryInfo>(d => d.FullName == this.ignoredPath), null) { Local = MetaDataChangeType.CREATED };

            Assert.That(this.underTest.Handle(folderEvent), Is.False);
        }

        [Test, Category("Fast"), Category("SelectiveIgnore")]
        public void DoNotFilterIgnoredRemoteFolderAddedEvent() {
            this.SetupMocks();
            var folderEvent = new FolderEvent(null, Mock.Of<IFolder>(f => f.Id == this.ignoredObjectId)) { Local = MetaDataChangeType.CREATED };

            Assert.That(this.underTest.Handle(folderEvent), Is.False);
        }

        private void SetupMocks() {
            this.storage = new Mock<IIgnoredEntitiesStorage>();
            this.storage.Setup(s => s.IsIgnoredPath(this.ignoredPath)).Returns(IgnoredState.IGNORED);
            this.storage.Setup(s => s.IsIgnoredPath(It.Is<string>(path => path.StartsWith(this.ignoredPath) && path != this.ignoredPath))).Returns(IgnoredState.INHERITED);
            this.storage.Setup(s => s.IsIgnoredPath(It.Is<string>(path => !path.StartsWith(this.ignoredPath)))).Returns(IgnoredState.NOT_IGNORED);
            this.underTest = new SelectiveIgnoreFilter(this.storage.Object);
        }
    }
}