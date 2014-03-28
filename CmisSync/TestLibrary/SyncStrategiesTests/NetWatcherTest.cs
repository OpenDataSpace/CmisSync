#if __COCOA__
#else
using System;
using System.IO;

using CmisSync.Lib.Events;
using CmisSync.Lib.Sync.Strategy;

using NUnit.Framework;

using Moq;

namespace TestLibrary.SyncStrategiesTests
{
    [TestFixture]
    public class NetWatcherTest : BaseWatcherTest
    {

        [SetUp]
        public new void SetUp() {
            base.SetUp ();
        }

        [TearDown]
        public new void TearDown() {
            base.TearDown ();
        }

        [Test, Category("Fast")]
        public void ConstructorSuccessTest() {
            var fswatcher = new Mock<FileSystemWatcher>(localFolder.FullName).Object;
            using (var watcher = new NetWatcher(fswatcher, queue.Object))
            {
                Assert.False(watcher.EnableEvents);
            }
        }

        [Test, Category("Fast")]
        [ExpectedException( typeof( ArgumentNullException ) )]
        public void ConstructorFailsWithNullWatcher() {
            using (new NetWatcher(null, queue.Object));
        }

        [Test, Category("Fast")]
        [ExpectedException( typeof( ArgumentNullException ) )]
        public void ConstructorFailsWithNullQueue() {
            var fswatcher = new Mock<FileSystemWatcher>(localFolder.FullName).Object;
            using (new NetWatcher(fswatcher, null));
        }

        [Test, Category("Fast")]
        [ExpectedException( typeof( ArgumentException ) )]
        public void ConstructorFailsWithWatcherOnNullPath() {
            var fswatcher = new Mock<FileSystemWatcher>().Object;
            using (new NetWatcher(fswatcher, queue.Object));
        }

        protected override WatcherData GetWatcherData (string pathname, ISyncEventQueue queue) {
            WatcherData watcherData = new WatcherData ();
            watcherData.Data = new FileSystemWatcher (pathname);
            watcherData.Watcher = new NetWatcher (watcherData.Data as FileSystemWatcher, queue);
            return watcherData;
        }

        protected override void WaitWatcherData (WatcherData watcherData, string pathname, WatcherChangeTypes types, int milliseconds) {
            FileSystemWatcher watcher = watcherData.Data as FileSystemWatcher;
            watcher.WaitForChanged (types, milliseconds);
        }

        [Test, Category("Medium")]
        public void ReportFSFileAddedEventTest () {
            ReportFSFileAddedEvent ();
        }

        [Test, Category("Medium")]
        public void ReportFSFileChangedEventTest () {
            ReportFSFileChangedEvent ();
        }

        [Test, Category("Medium")]
        public void ReportFSFileRenamedEventTest () {
            ReportFSFileRenamedEvent ();
        }

        [Test, Category("Medium")]
        public void ReportFSFileRemovedEventTest () {
            ReportFSFileRemovedEvent ();
        }

        [Test, Category("Medium")]
        public void ReportFSFolderAddedEventTest () {
            ReportFSFolderAddedEvent ();
        }

        [Test, Category("Medium")]
        public void ReportFSFolderChangedEventTest () {
            ReportFSFolderChangedEvent ();
        }

        [Test, Category("Medium")]
        public void ReportFSFolderRemovedEventTest () {
            ReportFSFolderRemovedEvent ();
        }

        [Test, Category("Medium")]
        public void ReportFSFolderRenamedEventTest () {
            ReportFSFolderRenamedEvent ();
        }

        [Test, Category("Medium")]
        public void ReportFSFolderMovedEventTest () {
            ReportFSFolderMovedEvent ();
        }
    }
}
#endif
