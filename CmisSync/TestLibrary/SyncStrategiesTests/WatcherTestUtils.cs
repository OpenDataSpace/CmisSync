using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

using CmisSync.Lib.Events;
using CmisSync.Lib.Sync.Strategy;

using NUnit.Framework;

using Moq;

namespace TestLibrary.SyncStrategiesTests
{
    public class WatcherData
    {
        public Watcher Watcher;
        public Object Data;
    };

    public class BaseWatcherTest
    {
        protected DirectoryInfo localFolder;
        protected FileInfo localFile;
        protected DirectoryInfo localSubFolder;
        protected Mock<ISyncEventQueue> queue;
        protected FSEvent returnedFSEvent;

        private static readonly int RETRIES = 10;
        private static readonly int MILISECONDSWAIT = 1000;

        protected void SetUp() {
            string localPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            localFolder = new DirectoryInfo(localPath);
            localFolder.Create();
            localSubFolder = new DirectoryInfo(Path.Combine(localFolder.FullName, Path.GetRandomFileName()));
            localSubFolder.Create();
            localFile = new FileInfo(Path.Combine(localFolder.FullName, Path.GetRandomFileName()));
            using(localFile.Create());
            queue = new Mock<ISyncEventQueue>();
            returnedFSEvent = null;
        }

        protected void TearDown() {
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

        protected virtual WatcherData GetWatcherData (string pathname, ISyncEventQueue queue) {
            Assert.Fail ("to be implemented in sub class");
            return new WatcherData();
        }

        protected virtual void WaitWatcherData (WatcherData watcherData, WatcherChangeTypes types, int milliseconds) {
            Assert.Fail ("to be implemented in sub class");
        }

        public void ReportFSFileAddedEvent () {
            localFile.Delete();
            localFile = new FileInfo(Path.Combine(localFolder.FullName, Path.GetRandomFileName()));
            queue.Setup(q => q.AddEvent(It.IsAny<FSEvent>()))
                .Callback((ISyncEvent file) => returnedFSEvent = file as FSEvent);
            var watcherData = GetWatcherData (localFolder.FullName, queue.Object);
            watcherData.Watcher.EnableEvents = true;
            var t = Task.Factory.StartNew(() => {
                int count = 0;
                while(returnedFSEvent == null && count < RETRIES) {
                    WaitWatcherData(watcherData,WatcherChangeTypes.Created, MILISECONDSWAIT);
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

        public void ReportFSFileChangedEvent () {
            queue.Setup(q => q.AddEvent(It.IsAny<FSEvent>()))
                .Callback((ISyncEvent file) => returnedFSEvent = file as FSEvent);
            var watcherData = GetWatcherData (localFolder.FullName, queue.Object);
            watcherData.Watcher.EnableEvents = true;
            var t = Task.Factory.StartNew(() => {
                int count = 0;
                while(returnedFSEvent == null && count < RETRIES) {
                    WaitWatcherData(watcherData,WatcherChangeTypes.Changed, MILISECONDSWAIT);
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

        public void ReportFSFileRenamedEvent () {
            string oldpath = localFile.FullName;
            string newpath = Path.Combine(localFolder.FullName, Path.GetRandomFileName());
            queue.Setup(q => q.AddEvent(It.IsAny<FSEvent>()))
                .Callback((ISyncEvent file) => returnedFSEvent = file as FSEvent);
            var watcherData = GetWatcherData (localFolder.FullName, queue.Object);
            watcherData.Watcher.EnableEvents = true;
            var t = Task.Factory.StartNew(() => {
                int count = 0;
                while(returnedFSEvent == null && count < RETRIES) {
                    WaitWatcherData(watcherData,WatcherChangeTypes.Renamed, MILISECONDSWAIT);
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

        public void ReportFSFileRemovedEvent () {
            queue.Setup(q => q.AddEvent(It.IsAny<FSEvent>()))
                .Callback((ISyncEvent file) => returnedFSEvent = file as FSEvent);
            var watcherData = GetWatcherData (localFolder.FullName, queue.Object);
            watcherData.Watcher.EnableEvents = true;
            var t = Task.Factory.StartNew(() => {
                int count = 0;
                while(returnedFSEvent == null && count < RETRIES) {
                    WaitWatcherData(watcherData,WatcherChangeTypes.Deleted, MILISECONDSWAIT);
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

        public void ReportFSFolderAddedEvent () {
            localSubFolder.Delete();
            queue.Setup(q => q.AddEvent(It.IsAny<FSEvent>()))
                .Callback((ISyncEvent file) => returnedFSEvent = file as FSEvent);
            var watcherData = GetWatcherData (localFolder.FullName, queue.Object);
            watcherData.Watcher.EnableEvents = true;
            var t = Task.Factory.StartNew(() => {
                int count = 0;
                while(returnedFSEvent == null && count < RETRIES) {
                    WaitWatcherData(watcherData,WatcherChangeTypes.Created, MILISECONDSWAIT);
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

        public void ReportFSFolderChangedEvent () {
            queue.Setup(q => q.AddEvent(It.IsAny<FSEvent>()))
                .Callback((ISyncEvent file) => returnedFSEvent = file as FSEvent);
            var watcherData = GetWatcherData (localFolder.FullName, queue.Object);
            watcherData.Watcher.EnableEvents = true;
            var t = Task.Factory.StartNew(() => {
                int count = 0;
                while(returnedFSEvent == null && count < RETRIES) {
                    WaitWatcherData(watcherData,WatcherChangeTypes.Changed, MILISECONDSWAIT);
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

        public void ReportFSFolderRemovedEvent () {
            queue.Setup(q => q.AddEvent(It.IsAny<FSEvent>()))
                .Callback((ISyncEvent file) => returnedFSEvent = file as FSEvent);
            var watcherData = GetWatcherData (localFolder.FullName, queue.Object);
            watcherData.Watcher.EnableEvents = true;
            var t = Task.Factory.StartNew(() => {
                int count = 0;
                while(returnedFSEvent == null && count < RETRIES) {
                    WaitWatcherData(watcherData,WatcherChangeTypes.Deleted, MILISECONDSWAIT);
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

        public void ReportFSFolderRenamedEvent () {
            string oldpath = localSubFolder.FullName;
            string newpath = Path.Combine(localFolder.FullName, Path.GetRandomFileName());
            queue.Setup(q => q.AddEvent(It.IsAny<FSEvent>()))
                .Callback((ISyncEvent folder) => returnedFSEvent = folder as FSEvent);
            var watcherData = GetWatcherData (localFolder.FullName, queue.Object);
            watcherData.Watcher.EnableEvents = true;
            var t = Task.Factory.StartNew(() => {
                int count = 0;
                while(returnedFSEvent == null && count < RETRIES) {
                    WaitWatcherData(watcherData,WatcherChangeTypes.Renamed, MILISECONDSWAIT);
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

        public void ReportFSFolderMovedEvent () {
            var anotherSubFolder = new DirectoryInfo(Path.Combine(localFolder.FullName, Path.GetRandomFileName()));
            anotherSubFolder.Create();
            string oldpath = localSubFolder.FullName;
            string newpath = Path.Combine(anotherSubFolder.FullName, Path.GetRandomFileName());
            List<FSEvent> returnedFSEvents = new List<FSEvent>();
            queue.Setup(q => q.AddEvent(It.IsAny<FSEvent>()))
                .Callback((ISyncEvent f) => returnedFSEvents.Add(f as FSEvent));
            var watcherData = GetWatcherData (localFolder.FullName, queue.Object);
            watcherData.Watcher.EnableEvents = true;
            var t = Task.Factory.StartNew(() => {
                int count = 0;
                while(returnedFSEvents.Count < 2 && count < RETRIES) {
                    WaitWatcherData(watcherData,WatcherChangeTypes.Renamed, MILISECONDSWAIT);
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

