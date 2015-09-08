//-----------------------------------------------------------------------
// <copyright file="RepositoryTests.cs" company="GRAU DATA AG">
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

namespace TestLibrary.CmisTests {
    using System;
    using System.IO;
    using System.Threading;
#if !__MonoCS__
    using FileInfo = Alphaleonis.Win32.Filesystem.FileInfo;
    using DirectoryInfo = Alphaleonis.Win32.Filesystem.DirectoryInfo;
    using Path = Alphaleonis.Win32.Filesystem.Path;
#endif
    using CmisSync.Lib;
    using CmisSync.Lib.Cmis;
    using CmisSync.Lib.Config;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Exceptions;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;

    using Moq;

    using NUnit.Framework;

    using TestLibrary.IntegrationTests;

    [TestFixture, Category("Fast")]
    public class RepositoryTests : IDisposable {
        private static dynamic config;
        private ActivityListenerAggregator listener;
        private RepoInfo repoInfo;
        private DirectoryInfo localPath;
        private SingleStepEventQueue queue;

        [TestFixtureSetUp]
        public void ClassInit() {
            config = ITUtils.GetConfig();
#if __COCOA__
            try {
                MonoMac.AppKit.NSApplication.Init();
            } catch (InvalidOperationException) {
            }
#endif
        }

        [Test]
        public void SyncStatusStartsWithOfflineStatus() {
            this.SetupMocks();

            var underTest = new TestRepository(this.repoInfo, this.listener, this.queue);

            Assert.That(underTest.Status, Is.EqualTo(SyncStatus.Disconnected));
        }

        [Test]
        public void SyncStatusIsDeactivatedIfRootFolderDoesNotExists() {
            this.SetupMocks();
            bool notified = false;
            var underTest = new TestRepository(this.repoInfo, this.listener, this.queue);

            underTest.ShowException += (object sender, RepositoryExceptionEventArgs e) => {
                Assert.That(sender, Is.EqualTo(underTest));
                Assert.That(e.Level, Is.EqualTo(ExceptionLevel.Fatal));
                Assert.That(e.Type, Is.EqualTo(ExceptionType.LocalSyncTargetDeleted));
                notified = true;
            };

            this.localPath.Delete();
            this.queue.AddEvent(new FSEvent(WatcherChangeTypes.Deleted, this.localPath.FullName, true));
            this.queue.Run();
            Assert.That(underTest.Status, Is.EqualTo(SyncStatus.Deactivated));
            this.localPath.Create();
            this.queue.AddEvent(new FSEvent(WatcherChangeTypes.Created, this.localPath.FullName, true));
            this.queue.Run();
            Assert.That(underTest.Status, Is.EqualTo(SyncStatus.Disconnected));
            Assert.That(notified, Is.True);
        }

        [Test]
        public void SyncStatusSwitchesFromOfflineToIdleIfLoginWasSuccessful(
            [Values(true, false)]bool pwcIsSupported,
            [Values(true, false)]bool selectiveSyncSupported)
        {
            this.SetupMocks();
            bool notified = false;
            var underTest = new TestRepository(this.repoInfo, this.listener, this.queue);
            underTest.PropertyChanged += (object sender, System.ComponentModel.PropertyChangedEventArgs e) => {
                notified = true;
                Assert.That(e.PropertyName, Is.EqualTo("Status"));
            };

            underTest.Queue.AddEvent(this.CreateSuccessfulLogin(pwcIsSupported, selectiveSyncSupported));
            this.queue.Step();

            Assert.That(underTest.Status, Is.EqualTo(SyncStatus.Idle));
            Assert.That(notified, Is.True);
        }

        [Test]
        public void SyncStatusSwitchesFromOfflineToErrorIfConfigurationIsNeeded() {
            this.SetupMocks();
            bool notified = false;
            var underTest = new TestRepository(this.repoInfo, this.listener, this.queue);
            underTest.PropertyChanged += (object sender, System.ComponentModel.PropertyChangedEventArgs e) => {
                notified = true;
                Assert.That(e.PropertyName, Is.EqualTo("Status"));
            };
            underTest.Queue.AddEvent(new ConfigurationNeededEvent(Mock.Of<Exception>()));

            this.queue.Step();
            Assert.That(underTest.Status, Is.EqualTo(SyncStatus.Warning));
            Assert.That(notified, Is.True);
        }

        [Test]
        public void SyncDateUpdatesIfSyncIsDoneAndQueueDoesNotContainsChanges(
            [Values(true, false)]bool pwcIsSupported,
            [Values(true, false)]bool selectiveSync)
        {
            this.SetupMocks();
            bool notified = false;
            var underTest = new TestRepository(this.repoInfo, this.listener, this.queue);
            underTest.PropertyChanged += (object sender, System.ComponentModel.PropertyChangedEventArgs e) => {
                if (e.PropertyName == "LastFinishedSync") {
                    notified = true;
                }
            };
            underTest.Queue.AddEvent(this.CreateSuccessfulLogin(pwcIsSupported, selectiveSync));
            this.queue.Step();
            underTest.Queue.AddEvent(new StartNextSyncEvent(fullSyncRequested: true));
            this.queue.Step();
            Assert.That(underTest.LastFinishedSync, Is.EqualTo(DateTime.Now).Within(1).Seconds);
            Assert.That(underTest.NumberOfChanges, Is.EqualTo(0));
            Assert.That(notified, Is.True);
        }

        [Test]
        public void LastFinishedSyncIsNullOnInitializationAndNumberOfChangesIsZero() {
            this.SetupMocks();

            var underTest = new TestRepository(this.repoInfo, this.listener, this.queue);

            Assert.That(underTest.LastFinishedSync, Is.Null);
            Assert.That(underTest.NumberOfChanges, Is.EqualTo(0));
        }

        [Test]
        public void NumberOfChangesAreUpdatedIfEventIsAddedToQueue() {
            this.SetupMocks();

            var underTest = new TestRepository(this.repoInfo, this.listener, this.queue);

            underTest.Queue.AddEvent(Mock.Of<ICountableEvent>(e => e.Category == EventCategory.DetectedChange));
            Assert.That(underTest.NumberOfChanges, Is.EqualTo(1));
            this.queue.Step();
            Assert.That(underTest.NumberOfChanges, Is.EqualTo(0));
        }

        private SuccessfulLoginEvent CreateSuccessfulLogin(bool pwcSupport = false, bool selectiveSync = false, bool changeEvents = false) {
            var url = new Uri("https://demo.deutsche-wolke.de/cmis/browser");
            return new SuccessfulLoginEvent(url, Mock.Of<ISession>(), Mock.Of<IFolder>(), pwcSupport, selectiveSync, changeEvents);
        }

        private void SetupMocks() {
            this.listener = new ActivityListenerAggregator(Mock.Of<IActivityListener>(), new TransmissionManager());
            var subfolder = Guid.NewGuid().ToString();
            this.queue = new SingleStepEventQueue(new SyncEventManager());
            this.queue.SwallowExceptions = true;
            this.repoInfo = new RepoInfo {
                AuthenticationType = AuthenticationType.BASIC,
                LocalPath = Path.Combine(config[1].ToString(), subfolder),
                RemotePath = config[2].ToString() + "/" + subfolder,
                Address = new XmlUri(new Uri(config[3].ToString())),
                User = config[4].ToString(),
                RepositoryId = config[6].ToString()
            };

            // FileSystemDir
            this.localPath = new DirectoryInfo(this.repoInfo.LocalPath);
            this.localPath.Create();
            if (!new DirectoryInfoWrapper(this.localPath).IsExtendedAttributeAvailable()) {
                Assert.Fail(string.Format("The local path {0} does not support extended attributes", this.localPath.FullName));
            }
        }

        private class TestRepository : Repository {
            public TestRepository(
                RepoInfo repoInfo,
                ActivityListenerAggregator activityListener,
                SingleStepEventQueue queue) : base(repoInfo, activityListener, true, queue) {
            }
        }

        #region boilerplatecode
        public void Dispose() {
            if (this.queue != null) {
                this.queue.Dispose();
                this.queue = null;
            }
        }
        #endregion
    }
}