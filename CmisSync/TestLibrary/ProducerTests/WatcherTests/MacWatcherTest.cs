//-----------------------------------------------------------------------
// <copyright file="MacWatcherTest.cs" company="GRAU DATA AG">
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

#if __COCOA__

namespace TestLibrary.ProducerTests.WatcherTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Producer.Watcher;
    using CmisSync.Lib.Queueing;

    using MonoMac.Foundation;
    using MonoMac.AppKit;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class MacWatcherTest : BaseWatcherTest
    {
        [TestFixtureSetUp]
        public void ClassSetUp()
        {
            try {
                NSApplication.Init();
            } catch (InvalidOperationException) {
            }
        }

        [SetUp]
        public new void SetUp() {
//            base.SetUp();
            string localPath = Path.Combine(System.Environment.CurrentDirectory, Path.GetRandomFileName());
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
            base.TearDown();
        }

        [Test, Category("Fast")]
        public void ConstructorSuccessTest() {
            using (var watcher = new MacWatcher(localFolder.FullName, queue.Object))
            {
                Assert.False(watcher.EnableEvents);
            }
        }

        [Test, Category("Fast")]
        public void ConstructorWithCustomLatency()
        {
            using (new MacWatcher(localFolder.FullName, queue.Object, TimeSpan.FromMilliseconds(100)));
        }

        [Test, Category("Fast")]
        [ExpectedException( typeof( ArgumentNullException ) )]
        public void ConstructorFailsWithNullWatcher() {
            using(new MacWatcher(null, queue.Object));
        }

        [Test, Category("Fast")]
        [ExpectedException( typeof( ArgumentNullException ) )]
        public void ConstructorFailsWithNullQueue() {
            using(new MacWatcher(localFolder.FullName, null));
        }

        public class EventQueue : ISyncEventQueue
        {
            private ISyncEventQueue Queue;
            public List<FSEvent> Events = new List<FSEvent>();
            public ISyncEventManager EventManager { get; private set; }

            public EventQueue(ISyncEventQueue queue)
            {
                Queue = queue;
            }

            public void AddEvent(ISyncEvent newEvent)
            {
                lock (Events) {
                    FSEvent fsEvent = newEvent as FSEvent;
                    if (fsEvent != null) {
                        Events.Add(fsEvent);
                    }

                    Queue.AddEvent(newEvent);
                }
            }

            public bool IsStopped {
                get { return Queue.IsStopped; }
            }

            public void Suspend() {
            }

            public void Continue() {
            }
        }

        protected override WatcherData GetWatcherData(string pathname, ISyncEventQueue queue) {
            WatcherData watcherData = new WatcherData();
            watcherData.Data = new EventQueue(queue);
            watcherData.Watcher = new MacWatcher(pathname, watcherData.Data as ISyncEventQueue, TimeSpan.FromMilliseconds(100));
            return watcherData;
        }

        protected override void WaitWatcherData(WatcherData watcherData, string pathname, WatcherChangeTypes types, int milliseconds) {
            EventQueue queue = watcherData.Data as EventQueue;
            while (milliseconds >= 0) {
                FSEvent[] events;
                lock (queue.Events) {
                    events = queue.Events.ToArray();
                }
                foreach (FSEvent fsEvent in events) {
                    if (fsEvent.LocalPath == pathname && fsEvent.Type == types) {
                        return;
                    }
                }
                Thread.Sleep(10);
                milliseconds -= 10;
            }
        }

        [Test, Category("Medium")]
        public void ReportFSFileAddedEventTest() {
            ReportFSFileAddedEvent();
        }

        [Test, Category("Medium")]
        public void ReportFSFileChangedEventTest() {
            ReportFSFileChangedEvent();
        }

        [Test, Category("Medium")]
        public void ReportFSFileRenamedEventTest() {
            ReportFSFileRenamedEvent();
        }

        [Test, Category("Medium")]
        public void ReportFSFileMovedEventTest() {
            this.ReportFSFileMovedEvent();
        }

        [Test, Category("Medium")]
        public void ReportFSFileRemovedEventTest() {
            ReportFSFileRemovedEvent();
        }

        [Test, Category("Medium")]
        public void ReportFSFolderAddedEventTest() {
            ReportFSFolderAddedEvent();
        }

        [Test, Category("Medium")]
        public void ReportFSFolderChangedEventTest() {
            ReportFSFolderChangedEvent();
        }

        [Test, Category("Medium")]
        public void ReportFSFolderRemovedEventTest() {
            ReportFSFolderRemovedEvent();
        }

        [Test, Category("Medium")]
        public void ReportFSFolderRenamedEventTest() {
            ReportFSFolderRenamedEvent();
        }

        [Test, Category("Medium")]
        public void ReportFSFolderMovedEventTest() {
            ReportFSFolderMovedEvent();
        }
    }
}

#endif
