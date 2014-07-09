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
namespace TestLibrary.SyncStrategiesTests
{
    using System;
    using System.IO;

    using CmisSync.Lib.Data;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Storage;
    using CmisSync.Lib.Sync.Strategy;

    using Moq;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture]
    public class NetWatcherTest : BaseWatcherTest
    {
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

        [Test, Category("Fast")]
        public void ConstructorSuccessTest() {
            var fswatcher = new Mock<FileSystemWatcher>(localFolder.FullName).Object;
            using (var watcher = new NetWatcher(fswatcher, queue.Object, Mock.Of<IMetaDataStorage>()))
            {
                Assert.False(watcher.EnableEvents);
            }
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorFailsWithNullWatcher() {
            using (new NetWatcher(null, queue.Object, Mock.Of<IMetaDataStorage>()))
            {
            }
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorFailsWithNullQueue() {
            var fswatcher = new Mock<FileSystemWatcher>(localFolder.FullName).Object;
            using (new NetWatcher(fswatcher, null, Mock.Of<IMetaDataStorage>()))
            {
            }
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentException))]
        public void ConstructorFailsWithWatcherOnNullPath() {
            var fswatcher = new Mock<FileSystemWatcher>().Object;
            using (new NetWatcher(fswatcher, queue.Object, null))
            {
            }
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorFailsWithNullStorage() {
            var fswatcher = new Mock<FileSystemWatcher>(localFolder.FullName).Object;
            using (new NetWatcher(fswatcher, queue.Object, null))
            {
            }
        }

        [Test, Category("Medium")]
        public void ReportFSFileAddedEventTest() {
            this.ReportFSFileAddedEvent();
        }

        [Test, Category("Medium")]
        public void ReportFSFileChangedEventTest() {
            this.ReportFSFileChangedEvent();
        }

        // This test fails on current build slave, retest when these are FC20 or higher
        [Test, Category("Medium"), Category("BrokenOnFC18"), Category("Erratic")]
        public void ReportFSFileRenamedEventTest() {
            this.ReportFSFileRenamedEvent();
        }

        [Test, Category("Medium")]
        public void ReportFSFileMovedEventTest() {
            this.ReportFSFileMovedEvent();
        }

        [Test, Category("Medium")]
        public void ReportFSFileRemovedEventTest() {
            this.storage.AddLocalFile(this.localFile.FullName, "id");
            this.ReportFSFileRemovedEvent();
        }

        [Test, Category("Medium")]
        public void ReportFSFolderAddedEventTest() {
            this.ReportFSFolderAddedEvent();
        }

        [Test, Category("Medium")]
        public void ReportFSFolderChangedEventTest() {
            this.ReportFSFolderChangedEvent();
        }

        [Test, Category("Medium")]
        public void ReportFSFolderRemovedEventTest() {
            this.storage.Setup(s => s.GetObjectByLocalPath(It.IsAny<IFileSystemInfo>())).Returns(Mock.Of<IMappedObject>(o => o.Type == MappedObjectType.Folder));
            this.ReportFSFolderRemovedEvent();
        }

        // This test fails on current build slave, retest when these are FC20 or higher
        [Test, Category("Medium"), Category("BrokenOnFC18"), Category("Erratic")]
        public void ReportFSFolderRenamedEventTest() {
            this.ReportFSFolderRenamedEvent();
        }

        [Test, Category("Medium")]
        public void ReportFSFolderMovedEventTest() {
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
