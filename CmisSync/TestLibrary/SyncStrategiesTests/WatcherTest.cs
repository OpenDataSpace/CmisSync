using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using CmisSync.Lib.Events;
using CmisSync.Lib.Storage;
using CmisSync.Lib.Sync.Strategy;

using NUnit.Framework;

using Moq;

namespace TestLibrary.SyncStrategiesTests
{
    [TestFixture]
    public class WatcherTest
    {

        private string localPath;
        private DirectoryInfo localFolder;
        private FileInfo localFile;
        private DirectoryInfo localSubFolder;
        private static readonly int RETRIES = 50;
        private static readonly int MILISECONDSWAIT = 1000;
        private Mock<ISyncEventQueue> queue;
        private FSEvent returnedFSEvent;
        private AbstractFolderEvent returnedFileEvent;
        private AbstractFolderEvent returnedFolderEvent;


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
            returnedFileEvent = null;
            returnedFolderEvent = null;
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
            var watcher = new Watcher(fswatcher, queue.Object);
            Assert.False(watcher.EnableEvents);
            Assert.AreEqual(Watcher.DEFAULT_FS_WATCHER_SYNC_STRATEGY_PRIORITY, watcher.Priority);
        }

        [Test, Category("Fast")]
        [ExpectedException( typeof( ArgumentNullException ) )]
        public void ConstructorFailsWithNullWatcher() {
            new Watcher(null, queue.Object);
        }

        [Test, Category("Fast")]
        [ExpectedException( typeof( ArgumentNullException ) )]
        public void ConstructorFailsWithNullQueue() {
            var fswatcher = new Mock<FileSystemWatcher>(localFolder.FullName).Object;
            new Watcher(fswatcher, null);
        }

        [Test, Category("Fast")]
        [ExpectedException( typeof( ArgumentException ) )]
        public void ConstructorFailsWithWatcherOnNullPath() {
            var fswatcher = new Mock<FileSystemWatcher>().Object;
            new Watcher(fswatcher, queue.Object);
        }

        [Test, Category("Fast")]
        public void IgnoreWrongEventsTest() {
            var fswatcher = new Mock<FileSystemWatcher>(localFolder.FullName){CallBase = true}.Object;
            queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Never());
            var watcher = new Watcher(fswatcher, queue.Object);
            Assert.False(watcher.Handle(new Mock<ISyncEvent>().Object));
            Assert.False(watcher.Handle(new Mock<FileEvent>(new Mock<IFileInfo>().Object, null, null){CallBase = false}.Object));
        }

        [Test, Category("Fast")]
        public void HandleFSFileAddedEventsTest () {
            var fswatcher = new Mock<FileSystemWatcher>(localFolder.FullName){CallBase = true}.Object;
            queue.Setup(q => q.AddEvent(It.IsAny<AbstractFolderEvent>()))
                .Callback((ISyncEvent f) => returnedFileEvent = f as AbstractFolderEvent);
            var watcher = new Watcher(fswatcher, queue.Object);
            var fileCreatedFSEvent = new FSEvent(WatcherChangeTypes.Created, localFile.FullName);
            Assert.True(watcher.Handle(fileCreatedFSEvent));
            Assert.AreEqual(MetaDataChangeType.CREATED, returnedFileEvent.Local);
            Assert.AreEqual(ContentChangeType.CREATED, (returnedFileEvent as FileEvent).LocalContent);
            Assert.AreEqual(localFile.FullName, (returnedFileEvent as FileEvent).LocalFile.FullName);
            Assert.IsNull((returnedFileEvent as FileEvent).RemoteFile);
            Assert.AreEqual(MetaDataChangeType.NONE, (returnedFileEvent as FileEvent).Remote);
            Assert.AreEqual(ContentChangeType.NONE, (returnedFileEvent as FileEvent).RemoteContent);
        }

        [Test, Category("Fast")]
        public void HandleFSFileChangedEventsTest () {
            var fswatcher = new Mock<FileSystemWatcher>(localFolder.FullName){CallBase = true}.Object;
            queue.Setup(q => q.AddEvent(It.IsAny<AbstractFolderEvent>()))
                .Callback((ISyncEvent f) => returnedFileEvent = f as AbstractFolderEvent);
            var watcher = new Watcher(fswatcher, queue.Object);
            var fileChangedFSEvent = new FSEvent(WatcherChangeTypes.Changed, localFile.FullName);
            Assert.True(watcher.Handle(fileChangedFSEvent));
            Assert.AreEqual(MetaDataChangeType.NONE, returnedFileEvent.Local);
            Assert.AreEqual(ContentChangeType.CHANGED, (returnedFileEvent as FileEvent).LocalContent);
            Assert.AreEqual(localFile.FullName, (returnedFileEvent as FileEvent).LocalFile.FullName);
            Assert.IsNull((returnedFileEvent as FileEvent).RemoteFile);
            Assert.AreEqual(MetaDataChangeType.NONE, (returnedFileEvent as FileEvent).Remote);
            Assert.AreEqual(ContentChangeType.NONE, (returnedFileEvent as FileEvent).RemoteContent);
        }

        [Test, Category("Fast")]
        public void HandleFSFileRemovedEventsTest () {
            var fswatcher = new Mock<FileSystemWatcher>(localFolder.FullName){CallBase = true}.Object;
            AbstractFolderEvent returnedFileEvent = null;
            queue.Setup(q => q.AddEvent(It.IsAny<AbstractFolderEvent>()))
                .Callback((ISyncEvent f) => returnedFileEvent = f as AbstractFolderEvent);
            var watcher = new Watcher(fswatcher, queue.Object);
            var fileRemovedFSEvent = new FSEvent(WatcherChangeTypes.Deleted, localFile.FullName);
            Assert.True(watcher.Handle(fileRemovedFSEvent));
            Assert.AreEqual(MetaDataChangeType.DELETED, returnedFileEvent.Local);
            Assert.AreEqual(ContentChangeType.DELETED, (returnedFileEvent as FileEvent).LocalContent);
            Assert.AreEqual(localFile.FullName, (returnedFileEvent as FileEvent).LocalFile.FullName);
            Assert.IsNull((returnedFileEvent as FileEvent).RemoteFile);
            Assert.AreEqual(MetaDataChangeType.NONE, (returnedFileEvent as FileEvent).Remote);
            Assert.AreEqual(ContentChangeType.NONE, (returnedFileEvent as FileEvent).RemoteContent);
        }

        [Test, Category("Fast")]
        public void HandleFSFileRenamedEventsTest () {
            string oldpath = Path.Combine(localFolder.FullName, Path.GetRandomFileName());
            var fswatcher = new Mock<FileSystemWatcher>(localFolder.FullName){CallBase = true}.Object;
            queue.Setup(q => q.AddEvent(It.IsAny<AbstractFolderEvent>()))
                .Callback((ISyncEvent f) => returnedFileEvent = f as AbstractFolderEvent);
            var watcher = new Watcher(fswatcher, queue.Object);
            var fileRenamedFSEvent = new FSMovedEvent(oldpath, localFile.FullName);
            Assert.True(watcher.Handle(fileRenamedFSEvent));
            Assert.AreEqual(MetaDataChangeType.MOVED, returnedFileEvent.Local);
            Assert.AreEqual(ContentChangeType.NONE, (returnedFileEvent as FileMovedEvent).LocalContent);
            Assert.AreEqual(localFile.FullName, (returnedFileEvent as FileMovedEvent).LocalFile.FullName);
            Assert.AreEqual(oldpath, (returnedFileEvent as FileMovedEvent).OldLocalFile.FullName);
            Assert.IsNull((returnedFileEvent as FileEvent).RemoteFile);
            Assert.AreEqual(MetaDataChangeType.NONE, (returnedFileEvent as FileMovedEvent).Remote);
            Assert.AreEqual(ContentChangeType.NONE, (returnedFileEvent as FileMovedEvent).RemoteContent);
        }

        [Test, Category("Fast")]
        public void HandleFSFolderAddedEventsTest () {
            var fswatcher = new Mock<FileSystemWatcher>(localFolder.FullName){CallBase = true}.Object;
            queue.Setup(q => q.AddEvent(It.IsAny<AbstractFolderEvent>()))
                .Callback((ISyncEvent f) => returnedFolderEvent = f as AbstractFolderEvent);
            var watcher = new Watcher(fswatcher, queue.Object);
            var folderCreatedFSEvent = new FSEvent(WatcherChangeTypes.Created, localFolder.FullName);
            Assert.True(watcher.Handle(folderCreatedFSEvent));
            Assert.AreEqual(MetaDataChangeType.CREATED, returnedFolderEvent.Local);
            Assert.AreEqual(localFolder.FullName, (returnedFolderEvent as FolderEvent).LocalFolder.FullName);
            Assert.IsNull((returnedFolderEvent as FolderEvent).RemoteFolder);
            Assert.AreEqual(MetaDataChangeType.NONE, (returnedFolderEvent as FolderEvent).Remote);
        }

        [Test, Category("Fast")]
        public void HandleFSFolderChangedEventsTest () {
            var fswatcher = new Mock<FileSystemWatcher>(localFolder.FullName){CallBase = true}.Object;
            queue.Setup(q => q.AddEvent(It.IsAny<AbstractFolderEvent>()))
                .Callback((ISyncEvent f) => returnedFolderEvent = f as AbstractFolderEvent);
            var watcher = new Watcher(fswatcher, queue.Object);
            var folderChangedFSEvent = new FSEvent(WatcherChangeTypes.Changed, localFolder.FullName);
            Assert.True(watcher.Handle(folderChangedFSEvent));
            Assert.AreEqual(MetaDataChangeType.CHANGED, returnedFolderEvent.Local);
            Assert.AreEqual(localFolder.FullName, (returnedFolderEvent as FolderEvent).LocalFolder.FullName);
            Assert.IsNull((returnedFolderEvent as FolderEvent).RemoteFolder);
            Assert.AreEqual(MetaDataChangeType.NONE, (returnedFolderEvent as FolderEvent).Remote);
        }

        [Test, Category("Fast")]
        public void HandleFSFolderRemovedEventsTest () {
            var fswatcher = new Mock<FileSystemWatcher>(localFolder.FullName){CallBase = true}.Object;
            queue.Setup(q => q.AddEvent(It.IsAny<AbstractFolderEvent>()))
                .Callback((ISyncEvent f) => returnedFolderEvent = f as AbstractFolderEvent);
            var watcher = new Watcher(fswatcher, queue.Object);
            var folderRemovedFSEvent = new FSEvent(WatcherChangeTypes.Deleted, localFolder.FullName);
            Assert.True(watcher.Handle(folderRemovedFSEvent));
            Assert.AreEqual(MetaDataChangeType.DELETED, returnedFolderEvent.Local);
            Assert.AreEqual(localFolder.FullName, (returnedFolderEvent as FolderEvent).LocalFolder.FullName);
            Assert.IsNull((returnedFolderEvent as FolderEvent).RemoteFolder);
            Assert.AreEqual(MetaDataChangeType.NONE, (returnedFolderEvent as FolderEvent).Remote);
        }

        [Test, Category("Fast")]
        public void HandleFSFolderRenamedEventsTest () {
            string oldpath = Path.Combine(localFolder.FullName, Path.GetRandomFileName());
            var fswatcher = new Mock<FileSystemWatcher>(localFolder.FullName){CallBase = true}.Object;
            queue.Setup(q => q.AddEvent(It.IsAny<AbstractFolderEvent>()))
                .Callback((ISyncEvent f) => returnedFolderEvent = f as AbstractFolderEvent);
            var watcher = new Watcher(fswatcher, queue.Object);
            var folderRenamedFSEvent = new FSMovedEvent(oldpath, localFolder.FullName);
            Assert.True(watcher.Handle(folderRenamedFSEvent));
            Assert.AreEqual(MetaDataChangeType.MOVED, returnedFolderEvent.Local);
            Assert.AreEqual(localFolder.FullName, (returnedFolderEvent as FolderEvent).LocalFolder.FullName);
            Assert.AreEqual(oldpath, (returnedFolderEvent as FolderMovedEvent).OldLocalFolder.FullName);
            Assert.IsNull((returnedFolderEvent as FolderMovedEvent).RemoteFolder);
            Assert.IsNull((returnedFolderEvent as FolderMovedEvent).OldRemoteFolderPath);
            Assert.AreEqual(MetaDataChangeType.NONE, (returnedFolderEvent as FolderEvent).Remote);
        }


        [Test, Category("Medium")]
        public void ReportFSFileAddedEventTest () {
            localFile.Delete();
            localFile = new FileInfo(Path.Combine(localFolder.FullName, Path.GetRandomFileName()));
            var fswatcher = new FileSystemWatcher(localFolder.FullName);
            queue.Setup(q => q.AddEvent(It.IsAny<FSEvent>()))
                .Callback((ISyncEvent file) => returnedFSEvent = file as FSEvent);
            var watcher = new Watcher(fswatcher, queue.Object);
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
            var watcher = new Watcher(fswatcher, queue.Object);
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
            var watcher = new Watcher(fswatcher, queue.Object);
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
            var watcher = new Watcher(fswatcher, queue.Object);
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
            var watcher = new Watcher(fswatcher, queue.Object);
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
            var watcher = new Watcher(fswatcher, queue.Object);
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
            var watcher = new Watcher(fswatcher, queue.Object);
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
            var watcher = new Watcher(fswatcher, queue.Object);
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
            var watcher = new Watcher(fswatcher, queue.Object);
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

