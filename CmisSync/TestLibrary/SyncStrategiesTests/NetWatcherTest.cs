using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using CmisSync.Lib.Events;
using CmisSync.Lib.Sync.Strategy;

using NUnit.Framework;

using Moq;

namespace TestLibrary.SyncStrategiesTests
{
    [TestFixture]
    public class NetWatcherTest
    {

        private string localPath;
        private DirectoryInfo localFolder;
        private FileInfo localFile;
        private DirectoryInfo localSubFolder;
        private static readonly int RETRIES = 50;
        private static readonly int MILISECONDSWAIT = 1000;
        private Mock<ISyncEventQueue> queue;
        private FSEvent returnedFSEvent;


        [SetUp]
        public void SetUp() {
            localPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            localFolder = new DirectoryInfo(localPath);
            localFolder.Create();
            localSubFolder = new DirectoryInfo(Path.Combine(localFolder.FullName, Path.GetRandomFileName()));
            localSubFolder.Create();
            localFile = new FileInfo(Path.Combine(localFolder.FullName, Path.GetRandomFileName()));
            using(localFile.Create());
            queue = new Mock<ISyncEventQueue>();
            returnedFSEvent = null;
        }

        [TearDown]
        public void TearDown() {
            localFile.Refresh();
            if(localFile.Exists)
                localFile.Delete();
            localSubFolder.Refresh();
            if(localSubFolder.Exists)
                localSubFolder.Delete(true);
            localFolder.Refresh();
            if(localFolder.Exists)
                localFolder.Delete(true);
        }

        [Test, Category("Fast")]
        public void ConstructorSuccessTest() {
            var fswatcher = new Mock<FileSystemWatcher>(localFolder.FullName).Object;
            var watcher = new NetWatcher(fswatcher, queue.Object);
            Assert.False(watcher.EnableEvents);
            Assert.AreEqual(Watcher.DEFAULT_FS_WATCHER_SYNC_STRATEGY_PRIORITY, watcher.Priority);
        }

        [Test, Category("Fast")]
        [ExpectedException( typeof( ArgumentNullException ) )]
        public void ConstructorFailsWithNullWatcher() {
            new NetWatcher(null, queue.Object);
        }

        [Test, Category("Fast")]
        [ExpectedException( typeof( ArgumentNullException ) )]
        public void ConstructorFailsWithNullQueue() {
            var fswatcher = new Mock<FileSystemWatcher>(localFolder.FullName).Object;
            new NetWatcher(fswatcher, null);
        }

        [Test, Category("Fast")]
        [ExpectedException( typeof( ArgumentException ) )]
        public void ConstructorFailsWithWatcherOnNullPath() {
            var fswatcher = new Mock<FileSystemWatcher>().Object;
            new NetWatcher(fswatcher, queue.Object);
        }

        [Test, Category("Medium")]
        public void ReportFSFileAddedEventTest () {
            localFile.Delete();
            localFile = new FileInfo(Path.Combine(localFolder.FullName, Path.GetRandomFileName()));
            var fswatcher = new FileSystemWatcher(localFolder.FullName);
            queue.Setup(q => q.AddEvent(It.IsAny<FSEvent>()))
                .Callback((ISyncEvent file) => returnedFSEvent = file as FSEvent);
            var watcher = new NetWatcher(fswatcher, queue.Object);
            watcher.EnableEvents = true;
            var t = Task.Factory.StartNew(() => {
                int count = 0;
                while(returnedFSEvent == null && count < RETRIES) {
                    fswatcher.WaitForChanged(WatcherChangeTypes.Created, MILISECONDSWAIT);
                    count ++;
                }
            });
            using(localFile.Create());
            t.Wait();
            if(returnedFSEvent != null)
            {
                Assert.IsFalse(returnedFSEvent.IsDirectory());
                Assert.AreEqual(localFile.FullName, returnedFSEvent.Path);
                Assert.AreEqual(WatcherChangeTypes.Created, returnedFSEvent.Type);
            }
            else
                Assert.Inconclusive("Missed file added event");
        }

        [Test, Category("Medium")]
        public void ReportFSFileChangedEventTest () {
            var fswatcher = new FileSystemWatcher(localFolder.FullName);
            queue.Setup(q => q.AddEvent(It.IsAny<FSEvent>()))
                .Callback((ISyncEvent file) => returnedFSEvent = file as FSEvent);
            var watcher = new NetWatcher(fswatcher, queue.Object);
            watcher.EnableEvents = true;
            var t = Task.Factory.StartNew(() => {
                int count = 0;
                while(returnedFSEvent == null && count < RETRIES) {
                    fswatcher.WaitForChanged(WatcherChangeTypes.Changed, MILISECONDSWAIT);
                    count ++;
                }
            });
            using (FileStream stream = File.OpenWrite(localFile.FullName))
            {
                byte[] data = new byte[1024];
                // Write data
                stream.Write(data, 0, data.Length);
            }
            t.Wait();
            if(returnedFSEvent != null)
            {
                if(returnedFSEvent.Type == WatcherChangeTypes.Changed) {
                    Assert.IsFalse(returnedFSEvent.IsDirectory());
                    Assert.AreEqual(localFile.FullName, returnedFSEvent.Path);
                }else
                    Assert.Inconclusive(String.Format("File System Event: \"{0}\"", returnedFSEvent.ToString()));
            }
            else
                Assert.Inconclusive("Missed file changed event");
        }

        [Test, Category("Medium")]
        public void ReportFSFileRenamedEventTest () {
            var fswatcher = new FileSystemWatcher(localFolder.FullName);
            string oldpath = localFile.FullName;
            string newpath = Path.Combine(localFolder.FullName, Path.GetRandomFileName());
            queue.Setup(q => q.AddEvent(It.IsAny<FSEvent>()))
                .Callback((ISyncEvent file) => returnedFSEvent = file as FSEvent);
            var watcher = new NetWatcher(fswatcher, queue.Object);
            watcher.EnableEvents = true;
            var t = Task.Factory.StartNew(() => {
                int count = 0;
                while(returnedFSEvent == null && count < RETRIES) {
                    fswatcher.WaitForChanged(WatcherChangeTypes.Renamed, MILISECONDSWAIT);
                    count ++;
                }
            });
            localFile.MoveTo(newpath);
            t.Wait();
            if(returnedFSEvent != null) {
                if(returnedFSEvent.Type == WatcherChangeTypes.Renamed)
                {
                    Assert.IsFalse(returnedFSEvent.IsDirectory());
                    Assert.AreEqual(newpath, (returnedFSEvent as FSMovedEvent).Path);
                    Assert.AreEqual(oldpath, (returnedFSEvent as FSMovedEvent).OldPath);
                    Assert.AreEqual(WatcherChangeTypes.Renamed, (returnedFSEvent as FSMovedEvent).Type);
                } else {
                    Assert.Inconclusive(String.Format("File System Event: \"{0}\"", returnedFSEvent.ToString()));
                }
            } else {
                Assert.Inconclusive("Missed file rename event");
            }
            localFile = new FileInfo(newpath);
        }

        [Test, Category("Medium")]
        public void ReportFSFileRemovedEventTest () {
            var fswatcher = new FileSystemWatcher(localFolder.FullName);
            queue.Setup(q => q.AddEvent(It.IsAny<FSEvent>()))
                .Callback((ISyncEvent file) => returnedFSEvent = file as FSEvent);
            var watcher = new NetWatcher(fswatcher, queue.Object);
            watcher.EnableEvents = true;
            var t = Task.Factory.StartNew(() => {
                int count = 0;
                while(returnedFSEvent == null && count < RETRIES) {
                    fswatcher.WaitForChanged(WatcherChangeTypes.Deleted, MILISECONDSWAIT);
                    count ++;
                }
            });
            localFile.Delete();
            t.Wait();
            if(returnedFSEvent!= null) {
                Assert.AreEqual(localFile.FullName, returnedFSEvent.Path);
                Assert.AreEqual(WatcherChangeTypes.Deleted, returnedFSEvent.Type);
            }
            else
                Assert.Inconclusive("Missed file removed event");
        }

        [Test, Category("Medium")]
        public void ReportFSFolderAddedEventTest () {
            localSubFolder.Delete();
            var fswatcher = new FileSystemWatcher(localFolder.FullName);
            queue.Setup(q => q.AddEvent(It.IsAny<FSEvent>()))
                .Callback((ISyncEvent file) => returnedFSEvent = file as FSEvent);
            var watcher = new NetWatcher(fswatcher, queue.Object);
            watcher.EnableEvents = true;
            var t = Task.Factory.StartNew(() => {
                int count = 0;
                while(returnedFSEvent == null && count < RETRIES) {
                    fswatcher.WaitForChanged(WatcherChangeTypes.Created, MILISECONDSWAIT);
                    count ++;
                }
            });
            localSubFolder.Create();
            t.Wait();
            if(returnedFSEvent != null)
            {
                Assert.IsTrue(returnedFSEvent.IsDirectory());
                Assert.AreEqual(localSubFolder.FullName, returnedFSEvent.Path);
                Assert.AreEqual(WatcherChangeTypes.Created, returnedFSEvent.Type);
            }
            else
                Assert.Inconclusive("Missed folder added event");
        }

        [Test, Category("Medium")]
        public void ReportFSFolderChangedEventTest () {
            var fswatcher = new FileSystemWatcher(localFolder.FullName);
            queue.Setup(q => q.AddEvent(It.IsAny<FSEvent>()))
                .Callback((ISyncEvent file) => returnedFSEvent = file as FSEvent);
            var watcher = new NetWatcher(fswatcher, queue.Object);
            watcher.EnableEvents = true;
            var t = Task.Factory.StartNew(() => {
                int count = 0;
                while(returnedFSEvent == null && count < RETRIES) {
                    fswatcher.WaitForChanged(WatcherChangeTypes.Changed, MILISECONDSWAIT);
                    count ++;
                }
            });
            localSubFolder.CreationTime = localSubFolder.CreationTime.AddDays(1);
            t.Wait();
            if(returnedFSEvent != null)
            {
                Assert.IsTrue(returnedFSEvent.IsDirectory());
                Assert.AreEqual(localSubFolder.FullName, returnedFSEvent.Path);
                Assert.AreEqual(WatcherChangeTypes.Changed, returnedFSEvent.Type);
            }
            else
                Assert.Inconclusive("Missed folder changed event");
        }

        [Test, Category("Medium")]
        public void ReportFSFolderRemovedEventTest () {
            var fswatcher = new FileSystemWatcher(localFolder.FullName);
            queue.Setup(q => q.AddEvent(It.IsAny<FSEvent>()))
                .Callback((ISyncEvent file) => returnedFSEvent = file as FSEvent);
            var watcher = new NetWatcher(fswatcher, queue.Object);
            watcher.EnableEvents = true;
            var t = Task.Factory.StartNew(() => {
                int count = 0;
                while(returnedFSEvent == null && count < RETRIES) {
                    fswatcher.WaitForChanged(WatcherChangeTypes.Deleted, MILISECONDSWAIT);
                    count ++;
                }
            });
            localSubFolder.Delete();
            t.Wait();
            if(returnedFSEvent != null)
            {
                Assert.AreEqual(localSubFolder.FullName, returnedFSEvent.Path);
                Assert.AreEqual(WatcherChangeTypes.Deleted, returnedFSEvent.Type);
            }
            else
                Assert.Inconclusive("Missed folder removed event");
        }

        [Test, Category("Medium")]
        public void ReportFSFolderRenamedEventTest () {
            var fswatcher = new FileSystemWatcher(localFolder.FullName);
            string oldpath = localSubFolder.FullName;
            string newpath = Path.Combine(localFolder.FullName, Path.GetRandomFileName());
            queue.Setup(q => q.AddEvent(It.IsAny<FSEvent>()))
                .Callback((ISyncEvent folder) => returnedFSEvent = folder as FSEvent);
            var watcher = new NetWatcher(fswatcher, queue.Object);
            watcher.EnableEvents = true;
            var t = Task.Factory.StartNew(() => {
                int count = 0;
                while(returnedFSEvent == null && count < RETRIES) {
                    fswatcher.WaitForChanged(WatcherChangeTypes.Renamed, MILISECONDSWAIT);
                    count ++;
                }
            });
            localSubFolder.MoveTo(newpath);
            t.Wait();
            if(returnedFSEvent != null)
            {
                Assert.IsTrue(returnedFSEvent.IsDirectory());
                if(returnedFSEvent.Type == WatcherChangeTypes.Renamed) {
                    Assert.AreEqual(oldpath, (returnedFSEvent as FSMovedEvent).OldPath);
                    Assert.AreEqual(newpath, (returnedFSEvent as FSMovedEvent).Path);
                } else {
                    Assert.Inconclusive(String.Format("File System Event: \"{0}\"", returnedFSEvent.ToString()));
                }
            }
            else
                Assert.Inconclusive("Missed folder renamed event");
            localSubFolder = new DirectoryInfo(newpath);
        }

        [Test, Category("Medium")]
        public void ReportFSFolderMovedEventTest () {
            var anotherSubFolder = new DirectoryInfo(Path.Combine(localFolder.FullName, Path.GetRandomFileName()));
            anotherSubFolder.Create();
            var fswatcher = new FileSystemWatcher(localFolder.FullName);
            string oldpath = localSubFolder.FullName;
            string newpath = Path.Combine(anotherSubFolder.FullName, Path.GetRandomFileName());
            List<FSEvent> returnedFSEvents = new List<FSEvent>();
            queue.Setup(q => q.AddEvent(It.IsAny<FSEvent>()))
                .Callback((ISyncEvent f) => returnedFSEvents.Add(f as FSEvent));
            var watcher = new NetWatcher(fswatcher, queue.Object);
            watcher.EnableEvents = true;
            var t = Task.Factory.StartNew(() => {
                int count = 0;
                while(returnedFSEvents.Count < 2 && count < RETRIES) {
                    fswatcher.WaitForChanged(WatcherChangeTypes.Renamed, MILISECONDSWAIT);
                    count ++;
                }
            });
            localSubFolder.MoveTo(newpath);
            t.Wait();
            if(returnedFSEvents.Count > 0)
            {
                bool oldpathfound = false;
                bool newpathfound = false;
                foreach(FSEvent fsEvent in returnedFSEvents) {
                    if(fsEvent.Path.Equals(oldpath))
                        oldpathfound = true;
                    if(fsEvent is FSMovedEvent && (fsEvent as FSMovedEvent).OldPath.Equals(oldpath))
                        oldpathfound = true;
                    if(fsEvent.Path.Equals(newpath))
                        newpathfound = true;
                }
                Assert.IsTrue(oldpathfound);
                Assert.IsTrue(newpathfound);
            }
            else
                Assert.Inconclusive("Missed folder moved event(s)");
            localSubFolder = new DirectoryInfo(newpath);
        }
    }
}

