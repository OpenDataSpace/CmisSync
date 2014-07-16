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
using CmisSync.Lib.Storage.Database.Entities;
using TestLibrary.IntegrationTests;

namespace TestLibrary.ProducerTests.WatcherTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage;
    using CmisSync.Lib.Producer.Watcher;

    using Moq;

    using NUnit.Framework;

    public class WatcherData
    {
        public IWatcherProducer Watcher { get; set; }

        public object Data { get; set; }
    }

    public class BaseWatcherTest
    {
        protected DirectoryInfo localFolder;
        protected FileInfo localFile;
        protected DirectoryInfo localSubFolder;
        protected Mock<ISyncEventQueue> queue;
        protected FSEvent returnedFSEvent;
        protected Guid uuid;

        private static readonly int RETRIES = 10;
        private static readonly int MILISECONDSWAIT = 1000;

        public void ReportFSFileAddedEvent() {
            this.localFile.Delete();
            this.localFile = new FileInfo(Path.Combine(this.localFolder.FullName, Path.GetRandomFileName()));
            this.queue.Setup(q => q.AddEvent(It.Is<FSEvent>(e => e.LocalPath == this.localFile.FullName)))
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
                Assert.IsFalse(this.returnedFSEvent.IsDirectory);
                Assert.AreEqual(this.localFile.FullName, this.returnedFSEvent.LocalPath);
                Assert.AreEqual(WatcherChangeTypes.Created, this.returnedFSEvent.Type);
            }
            else
            {
                Assert.Inconclusive("Missed file added event");
            }
        }

        public void ReportFSFileChangedEvent() {
            this.queue.Setup(q => q.AddEvent(It.Is<FSEvent>(e => e.LocalPath == this.localFile.FullName && e.Type == WatcherChangeTypes.Changed)))
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
                    Assert.IsFalse(this.returnedFSEvent.IsDirectory);
                    Assert.AreEqual(this.localFile.FullName, this.returnedFSEvent.LocalPath);
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
            this.queue.Setup(q => q.AddEvent(It.Is<FSEvent>(e => e.LocalPath == this.localFile.FullName)))
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
                    Assert.IsFalse(this.returnedFSEvent.IsDirectory);
                    Assert.AreEqual(newpath, (this.returnedFSEvent as FSMovedEvent).LocalPath);
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

        public void ReportFSFileMovedEvent() {

            var anotherSubFolder = new DirectoryInfo(Path.Combine(this.localFolder.FullName, Path.GetRandomFileName()));
            anotherSubFolder.Create();
            string oldpath = Path.Combine(this.localFile.FullName);
            string newpath = Path.Combine(anotherSubFolder.FullName, Path.GetRandomFileName());
            this.queue.Setup(q => q.AddEvent(It.Is<FSMovedEvent>(e => e.LocalPath == newpath)))
                .Callback((ISyncEvent file) => this.returnedFSEvent = file as FSMovedEvent);
            var watcherData = this.GetWatcherData(this.localFolder.FullName, this.queue.Object);
            watcherData.Watcher.EnableEvents = true;
            var t = Task.Factory.StartNew(() => {
                int count = 0;
                while (this.returnedFSEvent == null && count < RETRIES) {
                    WaitWatcherData(watcherData, newpath, WatcherChangeTypes.Renamed, MILISECONDSWAIT);
                    count++;
                }
            });
            new FileInfoWrapper(this.localFile).MoveTo(newpath);
            t.Wait();
            if (this.returnedFSEvent != null) {
                if (this.returnedFSEvent.Type == WatcherChangeTypes.Renamed)
                {
                    Assert.That(this.returnedFSEvent.IsDirectory, Is.False);
                    Assert.That((this.returnedFSEvent as FSMovedEvent).LocalPath, Is.EqualTo(newpath));
                    Assert.That((this.returnedFSEvent as FSMovedEvent).OldPath, Is.EqualTo(oldpath));
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
            this.queue.Setup(q => q.AddEvent(It.Is<FSEvent>(e => e.LocalPath == this.localFile.FullName)))
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
                Assert.AreEqual(WatcherChangeTypes.Deleted, this.returnedFSEvent.Type, this.localFile.FullName + " " + this.returnedFSEvent.LocalPath);
                Assert.AreEqual(this.localFile.FullName, this.returnedFSEvent.LocalPath);
            }
            else
            {
                Assert.Inconclusive("Missed file removed event");
            }
        }

        public void ReportFSFolderAddedEvent() {
            this.localSubFolder.Delete();
            this.queue.Setup(q => q.AddEvent(It.Is<FSEvent>(e => e.LocalPath == this.localSubFolder.FullName)))
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
                Assert.IsTrue(this.returnedFSEvent.IsDirectory);
                Assert.AreEqual(this.localSubFolder.FullName, this.returnedFSEvent.LocalPath);
                Assert.AreEqual(WatcherChangeTypes.Created, this.returnedFSEvent.Type);
            }
            else
            {
                Assert.Inconclusive("Missed folder added event");
            }
        }

        public void ReportFSFolderChangedEvent() {
            this.queue.Setup(q => q.AddEvent(It.Is<FSEvent>(e => e.LocalPath == this.localSubFolder.FullName && e.Type == WatcherChangeTypes.Changed)))
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
                Assert.IsTrue(this.returnedFSEvent.IsDirectory);
                Assert.AreEqual(this.localSubFolder.FullName, this.returnedFSEvent.LocalPath);
                Assert.AreEqual(WatcherChangeTypes.Changed, this.returnedFSEvent.Type);
            }
            else
            {
                Assert.Inconclusive("Missed folder changed event");
            }
        }

        public void ReportFSFolderRemovedEvent() {
            this.queue.Setup(q => q.AddEvent(It.Is<FSEvent>(e => e.LocalPath == this.localSubFolder.FullName)))
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
                Assert.AreEqual(this.localSubFolder.FullName, this.returnedFSEvent.LocalPath);
                Assert.AreEqual(WatcherChangeTypes.Deleted, this.returnedFSEvent.Type);
                Assert.That(this.returnedFSEvent.IsDirectory, Is.True);
            }
            else
            {
                Assert.Inconclusive("Missed folder removed event");
            }
        }

        public void ReportFSFolderRenamedEvent() {
            string oldpath = this.localSubFolder.FullName;
            string newpath = Path.Combine(this.localFolder.FullName, Path.GetRandomFileName());
            this.queue.Setup(q => q.AddEvent(It.Is<FSEvent>(e => e.LocalPath == newpath)))
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
                Assert.IsTrue(this.returnedFSEvent.IsDirectory);
                if (this.returnedFSEvent.Type == WatcherChangeTypes.Renamed) {
                    Assert.AreEqual(oldpath, (this.returnedFSEvent as FSMovedEvent).OldPath);
                    Assert.AreEqual(newpath, (this.returnedFSEvent as FSMovedEvent).LocalPath);
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
            this.queue.Setup(q => q.AddEvent(It.IsAny<FSMovedEvent>()))
                .Callback((ISyncEvent f) => this.returnedFSEvent = f as FSMovedEvent);
            var watcherData = this.GetWatcherData(this.localFolder.FullName, this.queue.Object);
            watcherData.Watcher.EnableEvents = true;
            var t = Task.Factory.StartNew(() => {
                int count = 0;
                while (this.returnedFSEvent == null && count < RETRIES) {
                    WaitWatcherData(watcherData, newpath, WatcherChangeTypes.Renamed, MILISECONDSWAIT);
                    count++;
                }
            });
            new DirectoryInfoWrapper(this.localSubFolder).MoveTo(newpath);
            t.Wait();
            if (this.returnedFSEvent != null)
            {
                FSMovedEvent movedEvent = this.returnedFSEvent as FSMovedEvent;

                Assert.That(movedEvent.OldPath, Is.EqualTo(oldpath));
                Assert.That(movedEvent.LocalPath, Is.EqualTo(newpath));
                Assert.That(movedEvent.Type, Is.EqualTo(WatcherChangeTypes.Renamed));
                Assert.That(movedEvent.IsDirectory, Is.True);
            }
            else
            {
                Assert.Inconclusive("Missed folder moved event(s)");
            }

            this.localSubFolder = new DirectoryInfo(newpath);
        }

        protected void SetUp() {
            var config = ITUtils.GetConfig();
            this.uuid = Guid.NewGuid();
            string localPath = Path.Combine(config[1].ToString(), Path.GetRandomFileName());
            this.localFolder = new DirectoryInfo(localPath);
            this.localFolder.Create();
            this.localSubFolder = new DirectoryInfo(Path.Combine(this.localFolder.FullName, Path.GetRandomFileName()));
            this.localSubFolder.Create();
            this.localFile = new FileInfo(Path.Combine(this.localFolder.FullName, Path.GetRandomFileName()));
            using (this.localFile.Create())
            {
            }

            if (AreExtendedAttributesAvailable(this.localFile.FullName)) {
                new FileInfoWrapper(this.localFile).SetExtendedAttribute(MappedObject.ExtendedAttributeKey, this.uuid.ToString());
            }

            if (AreExtendedAttributesAvailable(this.localSubFolder.FullName)) {
                new DirectoryInfoWrapper(this.localSubFolder).SetExtendedAttribute(MappedObject.ExtendedAttributeKey, this.uuid.ToString());
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

        protected void IgnoreIfExtendedAttributesAreNotAvailable() {
            IgnoreIfExtendedAttributesAreNotAvailable(this.localFolder.FullName);
        }

        public static void IgnoreIfExtendedAttributesAreNotAvailable(string fullName) {
            if (!AreExtendedAttributesAvailable(fullName)) {
                Assert.Ignore("Extended Attribute not available on path: " + fullName);
            }
        }

        public static bool AreExtendedAttributesAvailable(string fullName) {
            #if __MonoCS__ || __COCOA__
            return new ExtendedAttributeReaderUnix().IsFeatureAvailable(fullName);
            #else
            return new ExtendedAttributeReaderDos().IsFeatureAvailable(fullName);
            #endif
        }
    }
}