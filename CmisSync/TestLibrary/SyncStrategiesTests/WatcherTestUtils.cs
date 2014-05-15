//-----------------------------------------------------------------------
// <copyright file="WatcherTestUtils.cs" company="GRAU DATA AG">
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

namespace TestLibrary.SyncStrategiesTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Sync.Strategy;

    using Moq;

    using NUnit.Framework;

    public class WatcherData
    {
        public Watcher Watcher { get; set; }

        public object Data { get; set; }
    }

    public class BaseWatcherTest
    {
        protected DirectoryInfo localFolder;
        protected FileInfo localFile;
        protected DirectoryInfo localSubFolder;
        protected Mock<ISyncEventQueue> queue;
        protected FSEvent returnedFSEvent;

        private static readonly int RETRIES = 10;
        private static readonly int MILISECONDSWAIT = 1000;

        public void ReportFSFileAddedEvent() {
            this.localFile.Delete();
            this.localFile = new FileInfo(Path.Combine(this.localFolder.FullName, Path.GetRandomFileName()));
            this.queue.Setup(q => q.AddEvent(It.Is<FSEvent>(e => e.Path == this.localFile.FullName)))
                .Callback((ISyncEvent file) => this.returnedFSEvent = file as FSEvent);
            var watcherData = this.GetWatcherData(this.localFolder.FullName, this.queue.Object);
            watcherData.Watcher.EnableEvents = true;
            var t = Task.Factory.StartNew(() => {
                int count = 0;
                while (this.returnedFSEvent == null && count < RETRIES) {
                    WaitWatcherData(watcherData, this.localFile.FullName, WatcherChangeTypes.Created, MILISECONDSWAIT);
                    count++;
                }
            });
            using (this.localFile.Create())
            {
            }

            t.Wait();
            if (this.returnedFSEvent != null)
            {
                Assert.IsFalse(this.returnedFSEvent.IsDirectory());
                Assert.AreEqual(this.localFile.FullName, this.returnedFSEvent.Path);
                Assert.AreEqual(WatcherChangeTypes.Created, this.returnedFSEvent.Type);
            }
            else
            {
                Assert.Inconclusive("Missed file added event");
            }
        }

        public void ReportFSFileChangedEvent() {
            this.queue.Setup(q => q.AddEvent(It.Is<FSEvent>(e => e.Path == this.localFile.FullName && e.Type == WatcherChangeTypes.Changed)))
                .Callback((ISyncEvent file) => this.returnedFSEvent = file as FSEvent);
            var watcherData = this.GetWatcherData(this.localFolder.FullName, this.queue.Object);
            watcherData.Watcher.EnableEvents = true;
            var t = Task.Factory.StartNew(() => {
                int count = 0;
                while (this.returnedFSEvent == null && count < RETRIES) {
                    WaitWatcherData(watcherData, this.localFile.FullName, WatcherChangeTypes.Changed, MILISECONDSWAIT);
                    count++;
                }
            });
            using (FileStream stream = File.OpenWrite(this.localFile.FullName))
            {
                byte[] data = new byte[1024];

                // Write data
                stream.Write(data, 0, data.Length);
            }

            t.Wait();
            if (this.returnedFSEvent != null)
            {
                if (this.returnedFSEvent.Type == WatcherChangeTypes.Changed) {
                    Assert.IsFalse(this.returnedFSEvent.IsDirectory());
                    Assert.AreEqual(this.localFile.FullName, this.returnedFSEvent.Path);
                }
                else
                {
                    Assert.Inconclusive(string.Format("File System Event: \"{0}\"", this.returnedFSEvent.ToString()));
                }
            }
            else
            {
                Assert.Inconclusive("Missed file changed event");
            }
        }

        public void ReportFSFileRenamedEvent() {
            string oldpath = this.localFile.FullName;
            string newpath = Path.Combine(this.localFolder.FullName, Path.GetRandomFileName());
            this.queue.Setup(q => q.AddEvent(It.Is<FSEvent>(e => e.Path == this.localFile.FullName)))
                .Callback((ISyncEvent file) => this.returnedFSEvent = file as FSEvent);
            var watcherData = this.GetWatcherData(this.localFolder.FullName, this.queue.Object);
            watcherData.Watcher.EnableEvents = true;
            var t = Task.Factory.StartNew(() => {
                int count = 0;
                while (this.returnedFSEvent == null && count < RETRIES) {
                    WaitWatcherData(watcherData, newpath, WatcherChangeTypes.Renamed, MILISECONDSWAIT);
                    count++;
                }
            });
            this.localFile.MoveTo(newpath);
            t.Wait();
            if (this.returnedFSEvent != null) {
                if (this.returnedFSEvent.Type == WatcherChangeTypes.Renamed)
                {
                    Assert.IsFalse(this.returnedFSEvent.IsDirectory());
                    Assert.AreEqual(newpath, (this.returnedFSEvent as FSMovedEvent).Path);
                    Assert.AreEqual(oldpath, (this.returnedFSEvent as FSMovedEvent).OldPath);
                    Assert.AreEqual(WatcherChangeTypes.Renamed, (this.returnedFSEvent as FSMovedEvent).Type);
                }
                else
                {
                    Assert.Inconclusive(string.Format("File System Event: \"{0}\"", this.returnedFSEvent.ToString()));
                }
            }
            else
            {
                Assert.Inconclusive("Missed file rename event");
            }

            this.localFile = new FileInfo(newpath);
        }

        public void ReportFSFileRemovedEvent() {
            this.queue.Setup(q => q.AddEvent(It.Is<FSEvent>(e => e.Path == this.localFile.FullName)))
                .Callback((ISyncEvent file) => this.returnedFSEvent = file as FSEvent);
            var watcherData = this.GetWatcherData(this.localFolder.FullName, this.queue.Object);
            watcherData.Watcher.EnableEvents = true;
            var t = Task.Factory.StartNew(() => {
                int count = 0;
                while ((this.returnedFSEvent == null) && count < RETRIES) {
                    WaitWatcherData(watcherData, this.localFile.FullName, WatcherChangeTypes.Deleted, MILISECONDSWAIT);
                    count++;
                }
            });
            this.localFile.Delete();
            t.Wait();
            if (this.returnedFSEvent != null) {
                Assert.AreEqual(WatcherChangeTypes.Deleted, this.returnedFSEvent.Type, this.localFile.FullName + " " + this.returnedFSEvent.Path);
                Assert.AreEqual(this.localFile.FullName, this.returnedFSEvent.Path);
            }
            else
            {
                Assert.Inconclusive("Missed file removed event");
            }
        }

        public void ReportFSFolderAddedEvent() {
            this.localSubFolder.Delete();
            this.queue.Setup(q => q.AddEvent(It.Is<FSEvent>(e => e.Path == this.localSubFolder.FullName)))
                .Callback((ISyncEvent file) => this.returnedFSEvent = file as FSEvent);
            var watcherData = this.GetWatcherData(this.localFolder.FullName, this.queue.Object);
            watcherData.Watcher.EnableEvents = true;
            var t = Task.Factory.StartNew(() => {
                int count = 0;
                while (this.returnedFSEvent == null && count < RETRIES) {
                    WaitWatcherData(watcherData, this.localSubFolder.FullName, WatcherChangeTypes.Created, MILISECONDSWAIT);
                    count++;
                }
            });
            this.localSubFolder.Create();
            t.Wait();
            if (this.returnedFSEvent != null)
            {
                Assert.IsTrue(this.returnedFSEvent.IsDirectory());
                Assert.AreEqual(this.localSubFolder.FullName, this.returnedFSEvent.Path);
                Assert.AreEqual(WatcherChangeTypes.Created, this.returnedFSEvent.Type);
            }
            else
            {
                Assert.Inconclusive("Missed folder added event");
            }
        }

        public void ReportFSFolderChangedEvent() {
            this.queue.Setup(q => q.AddEvent(It.Is<FSEvent>(e => e.Path == this.localSubFolder.FullName && e.Type == WatcherChangeTypes.Changed)))
                .Callback((ISyncEvent file) => this.returnedFSEvent = file as FSEvent);
            var watcherData = this.GetWatcherData(this.localFolder.FullName, this.queue.Object);
            watcherData.Watcher.EnableEvents = true;
            var t = Task.Factory.StartNew(() => {
                int count = 0;
                while (this.returnedFSEvent == null && count < RETRIES) {
                    WaitWatcherData(watcherData, this.localSubFolder.FullName, WatcherChangeTypes.Changed, MILISECONDSWAIT);
                    count++;
                }
            });
            this.localSubFolder.CreationTime = this.localSubFolder.CreationTime.AddDays(1);
            t.Wait();
            if (this.returnedFSEvent != null)
            {
                Assert.IsTrue(this.returnedFSEvent.IsDirectory());
                Assert.AreEqual(this.localSubFolder.FullName, this.returnedFSEvent.Path);
                Assert.AreEqual(WatcherChangeTypes.Changed, this.returnedFSEvent.Type);
            }
            else
            {
                Assert.Inconclusive("Missed folder changed event");
            }
        }

        public void ReportFSFolderRemovedEvent() {
            this.queue.Setup(q => q.AddEvent(It.Is<FSEvent>(e => e.Path == this.localSubFolder.FullName)))
                .Callback((ISyncEvent file) => this.returnedFSEvent = file as FSEvent);
            var watcherData = this.GetWatcherData(this.localFolder.FullName, this.queue.Object);
            watcherData.Watcher.EnableEvents = true;
            var t = Task.Factory.StartNew(() => {
                int count = 0;
                while (this.returnedFSEvent == null && count < RETRIES) {
                    WaitWatcherData(watcherData, this.localSubFolder.FullName, WatcherChangeTypes.Deleted, MILISECONDSWAIT);
                    count++;
                }
            });
            this.localSubFolder.Delete();
            t.Wait();
            if (this.returnedFSEvent != null)
            {
                Assert.AreEqual(this.localSubFolder.FullName, this.returnedFSEvent.Path);
                Assert.AreEqual(WatcherChangeTypes.Deleted, this.returnedFSEvent.Type);
            }
            else
            {
                Assert.Inconclusive("Missed folder removed event");
            }
        }

        public void ReportFSFolderRenamedEvent() {
            string oldpath = this.localSubFolder.FullName;
            string newpath = Path.Combine(this.localFolder.FullName, Path.GetRandomFileName());
            this.queue.Setup(q => q.AddEvent(It.Is<FSEvent>(e => e.Path == newpath)))
                .Callback((ISyncEvent folder) => this.returnedFSEvent = folder as FSEvent);
            var watcherData = this.GetWatcherData(this.localFolder.FullName, this.queue.Object);
            watcherData.Watcher.EnableEvents = true;
            var t = Task.Factory.StartNew(() => {
                int count = 0;
                while (this.returnedFSEvent == null && count < RETRIES) {
                    WaitWatcherData(watcherData, newpath, WatcherChangeTypes.Renamed, MILISECONDSWAIT);
                    count++;
                }
            });
            this.localSubFolder.MoveTo(newpath);
            t.Wait();
            if (this.returnedFSEvent != null)
            {
                Assert.IsTrue(this.returnedFSEvent.IsDirectory());
                if (this.returnedFSEvent.Type == WatcherChangeTypes.Renamed) {
                    Assert.AreEqual(oldpath, (this.returnedFSEvent as FSMovedEvent).OldPath);
                    Assert.AreEqual(newpath, (this.returnedFSEvent as FSMovedEvent).Path);
                }
                else
                {
                    Assert.Inconclusive(string.Format("File System Event: \"{0}\"", this.returnedFSEvent.ToString()));
                }
            }
            else
            {
                Assert.Inconclusive("Missed folder renamed event");
            }

            this.localSubFolder = new DirectoryInfo(newpath);
        }

        public void ReportFSFolderMovedEvent() {
            var anotherSubFolder = new DirectoryInfo(Path.Combine(this.localFolder.FullName, Path.GetRandomFileName()));
            anotherSubFolder.Create();
            string oldpath = this.localSubFolder.FullName;
            string newpath = Path.Combine(anotherSubFolder.FullName, Path.GetRandomFileName());
            List<FSEvent> returnedFSEvents = new List<FSEvent>();
            this.queue.Setup(q => q.AddEvent(It.IsAny<FSEvent>()))
                .Callback((ISyncEvent f) => returnedFSEvents.Add(f as FSEvent));
            var watcherData = this.GetWatcherData(this.localFolder.FullName, this.queue.Object);
            watcherData.Watcher.EnableEvents = true;
            var t = Task.Factory.StartNew(() => {
                int count = 0;
                while (returnedFSEvents.Count < 2 && count < RETRIES) {
                    WaitWatcherData(watcherData, newpath, WatcherChangeTypes.Renamed, MILISECONDSWAIT);
                    count++;
                }
            });
            this.localSubFolder.MoveTo(newpath);
            t.Wait();
            if (returnedFSEvents.Count > 0)
            {
                bool oldpathfound = false;
                bool newpathfound = false;
                foreach (FSEvent fsEvent in returnedFSEvents) {
                    if (fsEvent.Path.Equals(oldpath)) {
                        oldpathfound = true;
                    }

                    if (fsEvent is FSMovedEvent && (fsEvent as FSMovedEvent).OldPath.Equals(oldpath)) {
                        oldpathfound = true;
                    }

                    if (fsEvent.Path.Equals(newpath)) {
                        newpathfound = true;
                    }
                }

                Assert.IsTrue(oldpathfound);
                Assert.IsTrue(newpathfound);
            }
            else
            {
                Assert.Inconclusive("Missed folder moved event(s)");
            }

            this.localSubFolder = new DirectoryInfo(newpath);
        }

        protected void SetUp() {
            string localPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            this.localFolder = new DirectoryInfo(localPath);
            this.localFolder.Create();
            this.localSubFolder = new DirectoryInfo(Path.Combine(this.localFolder.FullName, Path.GetRandomFileName()));
            this.localSubFolder.Create();
            this.localFile = new FileInfo(Path.Combine(this.localFolder.FullName, Path.GetRandomFileName()));
            using (this.localFile.Create())
            {
            }

            this.queue = new Mock<ISyncEventQueue>();
            this.returnedFSEvent = null;
        }

        protected void TearDown() {
            this.localFile.Refresh();
            if (this.localFile.Exists) {
                this.localFile.Delete();
            }

            this.localSubFolder.Refresh();
            if (this.localSubFolder.Exists) {
                this.localSubFolder.Delete(true);
            }

            this.localFolder.Refresh();
            if (this.localFolder.Exists) {
                this.localFolder.Delete(true);
            }
        }

        protected virtual WatcherData GetWatcherData(string pathname, ISyncEventQueue queue) {
            Assert.Fail("to be implemented in sub class");
            return new WatcherData();
        }

        protected virtual void WaitWatcherData(WatcherData watcherData, string pathname, WatcherChangeTypes types, int milliseconds) {
            Assert.Fail("to be implemented in sub class");
        }
    }
}