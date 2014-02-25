#if __COCOA__

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

using MonoMac.Foundation;
using MonoMac.AppKit;

using CmisSync.Lib.Events;
using CmisSync.Lib.Sync.Strategy;

using NUnit.Framework;

using Moq;

namespace TestLibrary.SyncStrategiesTests
{
    [TestFixture]
    public class MacWatcherTest : BaseWatcherTest
    {
        private bool StopRunLoop = false;
        private NSRunLoop RunLoop = null;
        private Thread RunLoopThread = null;

        [TestFixtureSetUp]
        public void ClassSetUp()
        {
            NSApplication.Init ();
            RunLoopThread = new Thread (() =>
            {
                RunLoop = NSRunLoop.Current;
                while (!StopRunLoop) {
                    RunLoop.RunUntil(NSDate.FromTimeIntervalSinceNow(1));
                }
            });
            RunLoopThread.Start ();
            while (RunLoop == null) {
                Thread.Sleep(10);
            }
        }

        [TestFixtureTearDown]
        public void ClassTearDown()
        {
            StopRunLoop = true;
            RunLoopThread.Join ();
        }

        [SetUp]
        public new void SetUp() {
//            base.SetUp ();
            string localPath = Path.Combine (System.Environment.CurrentDirectory, Path.GetRandomFileName ());
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
        public new void TearDown() {
            base.TearDown ();
        }

        [Test, Category("Fast")]
        public void ConstructorSuccessTest() {
            var watcher = new MacWatcher(localFolder.FullName, queue.Object, RunLoop);
            Assert.False(watcher.EnableEvents);
            Assert.AreEqual(Watcher.DEFAULT_FS_WATCHER_SYNC_STRATEGY_PRIORITY, watcher.Priority);
        }

        [Test, Category("Fast")]
        [ExpectedException( typeof( ArgumentNullException ) )]
        public void ConstructorFailsWithNullWatcher() {
            new MacWatcher(null, queue.Object, RunLoop);
        }

        [Test, Category("Fast")]
        [ExpectedException( typeof( ArgumentNullException ) )]
        public void ConstructorFailsWithNullQueue() {
            new MacWatcher(localFolder.FullName, null, RunLoop);
        }

        [Test, Category("Fast")]
        [ExpectedException( typeof( ArgumentNullException ) )]
        public void ConstructorFailsWithNullRunLoop() {
            new MacWatcher(localFolder.FullName, queue.Object, null);
        }

        public class EventQueue : ISyncEventQueue
        {
            private ISyncEventQueue Queue;
            public List<FSEvent> Events = new List<FSEvent>();

            public EventQueue(ISyncEventQueue queue)
            {
                Queue = queue;
            }

            public void AddEvent(ISyncEvent newEvent)
            {
                lock (Events) {
                    FSEvent fsEvent = newEvent as FSEvent;
                    if (fsEvent != null) {
                        Events.Add (fsEvent);
                    }
                    Queue.AddEvent (newEvent);
                }
            }

            public bool IsStopped
            {
                get { return Queue.IsStopped; }
            } 
        }

        protected override WatcherData GetWatcherData (string pathname, ISyncEventQueue queue) {
            WatcherData watcherData = new WatcherData ();
            watcherData.Data = new EventQueue(queue);
            watcherData.Watcher = new MacWatcher (pathname, watcherData.Data as ISyncEventQueue, RunLoop);
            return watcherData;
        }

        protected override void WaitWatcherData (WatcherData watcherData, string pathname, WatcherChangeTypes types, int milliseconds) {
            EventQueue queue = watcherData.Data as EventQueue;
            while (milliseconds >= 0) {
                FSEvent[] events;
                lock (queue.Events) {
                    events = queue.Events.ToArray ();
                }
                foreach (FSEvent fsEvent in events) {
                    if (fsEvent.Path == pathname && fsEvent.Type == types) {
                        return;
                    }
                }
                Thread.Sleep (10);
                milliseconds -= 10;
            }
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
