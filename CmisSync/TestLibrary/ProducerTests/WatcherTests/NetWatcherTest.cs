//-----------------------------------------------------------------------
// <copyright file="NetWatcherTest.cs" company="GRAU DATA AG">
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

#if ! __COCOA__
namespace TestLibrary.ProducerTests.WatcherTests {
    using System;
    using System.IO;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Producer.Watcher;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;

    using Moq;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture, Category("Medium")]
    public class NetWatcherTest : BaseWatcherTest {
        private Mock<IMetaDataStorage> storage;

        [SetUp]
        public new void SetUp() {
            base.SetUp();
            this.storage = new Mock<IMetaDataStorage>();
        }

        [TearDown]
        public new void TearDown() {
            base.TearDown();
        }

        [Test]
        public void ConstructorSuccessTest() {
            var fswatcher = new Mock<FileSystemWatcher>(localFolder.FullName).Object;
            using (var watcher = new NetWatcher(fswatcher, queue.Object, Mock.Of<IMetaDataStorage>())) {
                Assert.That(watcher.EnableEvents, Is.False);
            }
        }

        [Test]
        public void ConstructorFailsWithNullWatcher() {
            Assert.Throws<ArgumentNullException>(() => { using (new NetWatcher(null, queue.Object, Mock.Of<IMetaDataStorage>())); });
        }

        [Test]
        public void ConstructorFailsWithNullQueue() {
            var fswatcher = new Mock<FileSystemWatcher>(localFolder.FullName).Object;
            Assert.Throws<ArgumentNullException>(() => { using (new NetWatcher(fswatcher, null, Mock.Of<IMetaDataStorage>())); });
        }

        [Test]
        public void ConstructorFailsWithWatcherOnNullPath() {
            var fswatcher = new Mock<FileSystemWatcher>().Object;
            Assert.Throws<ArgumentException>(() => { using (new NetWatcher(fswatcher, queue.Object, null)); });
        }

        [Test]
        public void ConstructorFailsWithNullStorage() {
            var fswatcher = new Mock<FileSystemWatcher>(localFolder.FullName).Object;
            Assert.Throws<ArgumentNullException>(() => { using (new NetWatcher(fswatcher, queue.Object, null)); });
        }

        [Test]
        public void ReportFSFileAddedEventTest() {
            this.ReportFSFileAddedEvent();
        }

        [Test]
        public void ReportFSFileChangedEventTest() {
            this.ReportFSFileChangedEvent();
        }

        // This test fails on current build slave, retest when these are FC20 or higher
        [Test, Category("BrokenOnFC18"), Category("Erratic")]
        public void ReportFSFileRenamedEventTest() {
            this.ReportFSFileRenamedEvent();
        }

        [Test]
        public void ReportFSFileMovedEventTest() {
            this.IgnoreIfExtendedAttributesAreNotAvailable();
            string oldPath = this.localFile.FullName;
            this.storage.Setup(s => s.GetObjectByLocalPath(It.Is<IFileInfo>(f => f.FullName == oldPath))).Returns(Mock.Of<IMappedObject>(o => o.Guid == this.uuid && o.Type == MappedObjectType.File));
            this.ReportFSFileMovedEvent();
        }

        [Test]
        public void ReportFSFileRemovedEventTest() {
            this.storage.AddLocalFile(this.localFile.FullName, "id");
            this.ReportFSFileRemovedEvent();
        }

        [Test]
        public void ReportFSFolderAddedEventTest() {
            this.ReportFSFolderAddedEvent();
        }

        [Test]
        public void ReportFSFolderChangedEventTest() {
            this.ReportFSFolderChangedEvent();
        }

        [Test]
        public void ReportFSFolderRemovedEventTest() {
            this.storage.Setup(s => s.GetObjectByLocalPath(It.IsAny<IFileSystemInfo>())).Returns(Mock.Of<IMappedObject>(o => o.Type == MappedObjectType.Folder));
            this.ReportFSFolderRemovedEvent();
        }

        [Test]
        public void FSWatcherRootFolderRemovedTest() {
            this.storage.Setup(s => s.GetObjectByLocalPath(It.IsAny<IFileSystemInfo>())).Returns(Mock.Of<IMappedObject>(o => o.Type == MappedObjectType.Folder));
            this.ReportFSWatcherRootFolderRemoved();
        }

        // This test fails on current build slave, retest when these are FC20 or higher
        [Test, Category("BrokenOnFC18"), Category("Erratic")]
        public void ReportFSFolderRenamedEventTest() {
            this.ReportFSFolderRenamedEvent();
        }

        [Test]
        public void ReportFSFolderMovedEventTest() {
            this.IgnoreIfExtendedAttributesAreNotAvailable();
            string oldPath = this.localSubFolder.FullName;
            this.storage.Setup(s => s.GetObjectByLocalPath(It.Is<IFileSystemInfo>(d => d.FullName == oldPath))).Returns(Mock.Of<IMappedObject>(o => o.Guid == this.uuid && o.Type == MappedObjectType.Folder));
            this.ReportFSFolderMovedEvent();
        }

        protected override WatcherData GetWatcherData(string pathname, ISyncEventQueue queue) {
            WatcherData watcherData = new WatcherData();
            watcherData.Data = new Tuple<FileSystemWatcher, IMetaDataStorage>(new FileSystemWatcher(pathname), this.storage.Object);
            watcherData.Watcher = new NetWatcher((watcherData.Data as Tuple<FileSystemWatcher, IMetaDataStorage>).Item1, queue, this.storage.Object);
            return watcherData;
        }

        protected override void WaitWatcherData(WatcherData watcherData, string pathname, WatcherChangeTypes types, int milliseconds) {
            FileSystemWatcher watcher = (watcherData.Data as Tuple<FileSystemWatcher, IMetaDataStorage>).Item1;
            watcher.WaitForChanged(types, milliseconds);
        }
    }
}
#endif