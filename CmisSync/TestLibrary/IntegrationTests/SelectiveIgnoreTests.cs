//-----------------------------------------------------------------------
// <copyright file="FullRepoTests.cs" company="GRAU DATA AG">
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

/**
 * Unit Tests for CmisSync.
 *
 * To use them, first create a JSON file containing the credentials/parameters to your CMIS server(s)
 * Put it in TestLibrary/test-servers.json and use this format:
[
    [
        "unittest1",
        "/mylocalpath",
        "/myremotepath",
        "http://example.com/p8cmis/resources/Service",
        "myuser",
        "mypassword",
        "repository987080"
    ],
    [
        "unittest2",
        "/mylocalpath",
        "/myremotepath",
        "http://example.org:8080/Nemaki/cmis",
        "myuser",
        "mypassword",
        "repo3"
    ]
]
 */

namespace TestLibrary.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading;

    using CmisSync.Lib;
    using CmisSync.Lib.Cmis;
    using CmisSync.Lib.Cmis.ConvenienceExtenders;
    using CmisSync.Lib.Config;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS;
    using DotCMIS.Binding;
    using DotCMIS.Client;
    using DotCMIS.Client.Impl;
    using DotCMIS.Exceptions;

    using log4net;

    #if __COCOA__
    using MonoMac.AppKit;
    #endif

    using Moq;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    // Important!
    // These tests tend to be Erratic Tests, they can not be used for QA
    // This more of a "rapid szenario creation" class
    // Please do write predictable unit tests for all fixes (IT here is not enough)

    // Default timeout per test is 15 minutes
    [TestFixture, Timeout(900000)]
    public class SelectiveIgnoreTests : IsTestWithConfiguredLog4Net
    {
        private static readonly string SubfolderBase = "SelectiveIgnoreTests_";
        private static dynamic config;
        private string subfolder;
        private RepoInfo repoInfo;
        private DirectoryInfo localRootDir;
        private IFolder remoteRootDir;
        private ISession session;
        private FullRepoTests.CmisRepoMock repo;
        private ActiveActivitiesManager transmissionManager;

        [TestFixtureSetUp]
        public void ClassInit()
        {
            // Disable HTTPS Verification
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            #if __COCOA__
            try {
                NSApplication.Init();
            } catch (InvalidOperationException) {
            }
            #endif
            config = ITUtils.GetConfig();
        }

        [TestFixtureTearDown]
        public void ClassTearDown()
        {
            // Reanable HTTPS Verification
            ServicePointManager.ServerCertificateValidationCallback = null;
        }

        [SetUp]
        public void Init()
        {
            this.subfolder = SubfolderBase + Guid.NewGuid().ToString();
            Console.WriteLine("Working on " + this.subfolder);

            // RepoInfo
            this.repoInfo = new RepoInfo {
                AuthenticationType = AuthenticationType.BASIC,
                LocalPath = Path.Combine(config[1].ToString(), this.subfolder),
                RemotePath = config[2].ToString() + "/" + this.subfolder,
                Address = new XmlUri(new Uri(config[3].ToString())),
                User = config[4].ToString(),
                RepositoryId = config[6].ToString()
            };
            this.repoInfo.RemotePath = this.repoInfo.RemotePath.Replace("//", "/");
            this.repoInfo.SetPassword(config[5].ToString());

            // FileSystemDir
            this.localRootDir = new DirectoryInfo(this.repoInfo.LocalPath);
            this.localRootDir.Create();

            // Repo
            var activityListener = new Mock<IActivityListener>();
            this.transmissionManager = new ActiveActivitiesManager();
            var activityAggregator = new ActivityListenerAggregator(activityListener.Object, transmissionManager);
            var queue = new SingleStepEventQueue(new SyncEventManager());

            this.repo = new FullRepoTests.CmisRepoMock(this.repoInfo, activityAggregator, queue);

            // Session
            var cmisParameters = new Dictionary<string, string>();
            cmisParameters[SessionParameter.BindingType] = BindingType.AtomPub;
            cmisParameters[SessionParameter.AtomPubUrl] = this.repoInfo.Address.ToString();
            cmisParameters[SessionParameter.User] = this.repoInfo.User;
            cmisParameters[SessionParameter.Password] = this.repoInfo.GetPassword().ToString();
            cmisParameters[SessionParameter.RepositoryId] = this.repoInfo.RepositoryId;

            SessionFactory factory = SessionFactory.NewInstance();
            this.session = factory.CreateSession(cmisParameters);

            IFolder root = (IFolder)this.session.GetObjectByPath(config[2].ToString());
            this.remoteRootDir = root.CreateFolder(this.subfolder);
        }

        [TearDown]
        public void TestDown()
        {
            this.repo.Dispose();
            if (this.localRootDir.Exists) {
                this.localRootDir.Delete(true);
            }

            this.remoteRootDir.Refresh();
            this.remoteRootDir.DeleteTree(true, null, true);
            this.repo.Dispose();
        }

        [Test, Category("Slow"), Category("SelectiveIgnore")]
        public void IgnoreRemoteFolder() {
            var folder = this.remoteRootDir.CreateFolder("ignored");

            folder.IgnoreAllChildren();

            var context = OperationContextFactory.CreateContext(this.session, false, true);
            var underTest = this.session.GetObject(folder.Id, context) as IFolder;

            Assert.That(folder.AreAllChildrenIgnored(), Is.True);
            Assert.That(underTest.AreAllChildrenIgnored(), Is.True);
        }

        [Test, Category("Slow"), Category("SelectiveIgnore")]
        public void RemoteIgnoredFolderIsNotSynced() {
            var ignoredFolder = this.remoteRootDir.CreateFolder("ignored");
            ignoredFolder.IgnoreAllChildren();
            ignoredFolder.CreateFolder("sub");

            this.repo.Initialize();
            this.repo.Run();

            var folder = this.session.GetObject(ignoredFolder.Id) as IFolder;
            Assert.That(folder.AreAllChildrenIgnored(), Is.True);
            Assert.That(this.localRootDir.GetDirectories(), Is.Empty);
        }

        [Test, Category("Slow"), Category("SelectiveIgnore")]
        public void RemoteFolderIsSyncedAndChangedToIgnored() {
            var ignoredFolder = this.remoteRootDir.CreateFolder("ignored");
            var subFolder = ignoredFolder.CreateFolder("sub");

            this.repo.Initialize();
            this.repo.Run();

            Thread.Sleep(3000);
            this.repo.Queue.AddEvent(new StartNextSyncEvent());
            this.repo.Run();

            ignoredFolder.Refresh();
            ignoredFolder.IgnoreAllChildren();
            subFolder.Refresh();
            subFolder.CreateFolder("bla");

            this.repo.Queue.AddEvent(new StartNextSyncEvent());
            this.repo.Run();

            Assert.That(this.localRootDir.GetDirectories()[0].Name, Is.EqualTo("ignored"));
            Assert.That(this.localRootDir.GetDirectories()[0].GetDirectories()[0].Name, Is.EqualTo("sub"));
            Assert.That(this.localRootDir.GetDirectories()[0].GetDirectories()[0].GetDirectories(), Is.Empty);
        }

        [Test, Category("Slow"), Category("SelectiveIgnore")]
        public void IgnoreMultipleDeviceIds() {
            var folder = this.remoteRootDir.CreateFolder("folder");
            var anotherUuid = Guid.NewGuid();
            folder.IgnoreAllChildren(anotherUuid.ToString());

            Assert.That(folder.AreAllChildrenIgnored(), Is.False);

            folder.IgnoreAllChildren();

            Assert.That(folder.AreAllChildrenIgnored(), Is.True);
        }

        [Test, Category("Slow"), Category("SelectiveIgnore")]
        public void StopIgnoringFolderForThisDeviceId() {
            var folder = this.remoteRootDir.CreateFolder("folder");
            var anotherId = Guid.NewGuid().ToString().ToLower();
            var ownId = ConfigManager.CurrentConfig.DeviceId.ToString().ToLower();
            folder.IgnoreAllChildren(anotherId);
            Assert.That(folder.AreAllChildrenIgnored(), Is.False);
            folder.IgnoreAllChildren(ownId);
            Assert.That(folder.AreAllChildrenIgnored(), Is.True);
            folder.RemoveSyncIgnore(ownId);
            Assert.That(folder.AreAllChildrenIgnored(), Is.False);
            Assert.That(folder.IgnoredDevices().Contains(anotherId));
            Assert.That(folder.IgnoredDevices().Count, Is.EqualTo(1));
            folder.RemoveAllSyncIgnores();
            Assert.That(folder.AreAllChildrenIgnored(), Is.False);
            Assert.That(folder.IgnoredDevices(), Is.Empty);
        }

        [Test, Category("Slow"), Category("SelectiveIgnore")]
        public void RemoveMultipleDeviceIds() {
            var folder = this.remoteRootDir.CreateFolder("folder");
            for (int i = 1; i <= 10; i++) {
                folder.IgnoreAllChildren(Guid.NewGuid().ToString().ToLower());
                Assert.That(folder.IgnoredDevices().Count, Is.EqualTo(i));
            }

            for (int i = 9; i >= 0; i--) {
                folder.RemoveSyncIgnore(folder.IgnoredDevices().First());
                Assert.That(folder.IgnoredDevices().Count, Is.EqualTo(i));
            }
        }

        private void WaitUntilQueueIsNotEmpty(SingleStepEventQueue queue, int timeout = 10000) {
            int waited = 0;
            while (queue.Queue.IsEmpty)
            {
                int interval = 20;

                // Wait for event to kick in
                Thread.Sleep(interval);
                waited += interval;
                if (waited > timeout) {
                    Assert.Fail("Timeout exceeded");
                }
            }
        }

        private void AssertThatDatesAreEqual(DateTime? expected, DateTime? actual, string msg = null) {
            if (msg != null) {
                Assert.That((DateTime)actual, Is.EqualTo((DateTime)expected).Within(1).Seconds, msg);
            } else {
                Assert.That((DateTime)actual, Is.EqualTo((DateTime)expected).Within(1).Seconds);
            }
        }

        private class BlockingSingleConnectionScheduler : CmisSync.Lib.Queueing.ConnectionScheduler {
            public BlockingSingleConnectionScheduler(ConnectionScheduler original) : base(original) {
            }

            public override void Start() {
                if (!base.Connect()) {
                    Assert.Fail("Connection failed");
                }
            }
        }
    }
}
