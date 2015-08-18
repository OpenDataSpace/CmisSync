//-----------------------------------------------------------------------
// <copyright file="BaseFullRepoTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.IntegrationTests {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Security.Cryptography;
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

    using log4net;

    #if __COCOA__
    using MonoMac.AppKit;
    #endif

    using Moq;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture, Timeout(900000)]
    public abstract class BaseFullRepoTest : IsTestWithConfiguredLog4Net, IDisposable {
        protected RepoInfo repoInfo;
        protected DirectoryInfo localRootDir;
        protected IFolder remoteRootDir;
        protected ISession session;
        protected FullRepoTests.CmisRepoMock repo;
        protected TransmissionManager transmissionManager;

        private static dynamic config;
        private string subfolder;
        private bool contentChanges;
        private bool disposed = false;

        ~BaseFullRepoTest() {
            this.Dispose(false);
        }

        protected bool ContentChangesActive {
            get {
                return this.contentChanges;
            }

            set {
                this.contentChanges = value;
                if (value) {
                    this.EnsureThatContentChangesAreSupported();
                }
            }
        }

        protected ISessionFactory SessionFactory { get; set; }

        protected IAuthenticationProvider AuthProvider { get; set; }

        protected string TestName { get; private set; }

        protected Guid TestUuid { get; private set; }

        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        [TestFixtureSetUp]
        public void ClassInit() {
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
        public void ClassTearDown() {
            // Reanable HTTPS Verification
            ServicePointManager.ServerCertificateValidationCallback = null;
        }

        [SetUp]
        public void Init() {
            this.TestName = this.GetType().Name;
            object[] attributes = this.GetType().GetCustomAttributes(true);
            foreach (var attr in attributes) {
                if (attr is TestNameAttribute) {
                    this.TestName = (attr as TestNameAttribute).Name;
                }
            }

            this.TestUuid = Guid.NewGuid();

            this.subfolder = this.TestName + "_" + this.TestUuid.ToString();
            Console.WriteLine("Working on " + this.subfolder);

            // RepoInfo
            this.repoInfo = new RepoInfo {
                AuthenticationType = AuthenticationType.BASIC,
                LocalPath = Path.Combine(config[1].ToString(), this.subfolder),
                RemotePath = config[2].ToString() + "/" + this.subfolder,
                Address = new XmlUri(new Uri(config[3].ToString())),
                User = config[4].ToString(),
                RepositoryId = config[6].ToString(),
                Binding = config[7] != null ? config[7].ToString() : BindingType.AtomPub,
                HttpMaximumRetries = 0
            };
            this.repoInfo.RemotePath = this.repoInfo.RemotePath.Replace("//", "/");
            this.repoInfo.SetPassword(config[5].ToString());

            // FileSystemDir
            this.localRootDir = new DirectoryInfo(this.repoInfo.LocalPath);
            this.localRootDir.Create();
            if (!new DirectoryInfoWrapper(this.localRootDir).IsExtendedAttributeAvailable()) {
                Assert.Fail(string.Format("The local path {0} does not support extended attributes", this.localRootDir.FullName));
            }

            // Repo
            var activityListener = new Mock<IActivityListener>();
            this.transmissionManager = new TransmissionManager();
            var activityAggregator = new ActivityListenerAggregator(activityListener.Object, this.transmissionManager);
            var queue = new SingleStepEventQueue(new SyncEventManager());

            this.repo = new FullRepoTests.CmisRepoMock(this.repoInfo, activityAggregator, queue);

            // Session
            var cmisParameters = new Dictionary<string, string>();
            cmisParameters[SessionParameter.BindingType] = repoInfo.Binding;
            switch (repoInfo.Binding) {
            case BindingType.AtomPub:
                cmisParameters[SessionParameter.AtomPubUrl] = this.repoInfo.Address.ToString();
                break;
            case BindingType.Browser:
                cmisParameters[SessionParameter.BrowserUrl] = this.repoInfo.Address.ToString();
                break;
            default:
                Assert.Fail(string.Format("Unknown binding type {0}", repoInfo.Binding));
                break;
            }

            cmisParameters[SessionParameter.User] = this.repoInfo.User;
            cmisParameters[SessionParameter.Password] = this.repoInfo.GetPassword().ToString();
            cmisParameters[SessionParameter.RepositoryId] = this.repoInfo.RepositoryId;
            cmisParameters[SessionParameter.UserAgent] = Utils.CreateUserAgent();
            cmisParameters[SessionParameter.MaximumRequestRetries] = "0";

            this.SessionFactory = this.SessionFactory ?? DotCMIS.Client.Impl.SessionFactory.NewInstance();
            this.session = this.SessionFactory.CreateSession(cmisParameters, null, null, null);
            this.ContentChangesActive = this.session.AreChangeEventsSupported();
            IFolder root = (IFolder)this.session.GetObjectByPath(config[2].ToString());
            this.remoteRootDir = root.CreateFolder(this.subfolder);
        }

        [TearDown]
        public void TestDown() {
            InvalidDataException repoDBException = null;
            try {
                this.repo.RunDbObjectValidationCheck();
            } catch (InvalidDataException e) {
                repoDBException = e;
            }

            this.repo.Dispose();
            if (this.localRootDir.Exists) {
                this.localRootDir.Delete(true);
            }

            this.remoteRootDir.Refresh();
            this.remoteRootDir.DeleteTree(true, null, true);
            this.repo.Dispose();

            if (repoDBException != null) {
                throw new Exception(repoDBException.Message, repoDBException);
            }

            this.SessionFactory = null;
        }

        protected void WaitUntilQueueIsNotEmpty(SingleStepEventQueue queue = null, int timeout = 10000) {
            if (queue == null) {
                queue = this.repo.SingleStepQueue;
            }

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

        protected void WaitForRemoteChanges(int sleepDuration = 5000) {
            Thread.Sleep(this.ContentChangesActive ? sleepDuration : 0);
        }

        protected void EnsureThatContentChangesAreSupported() {
            if (!this.session.AreChangeEventsSupported()) {
                Assert.Ignore("skipped because the server does not support content changes");
            }
        }

        protected void EnsureThatPrivateWorkingCopySupportIsAvailable() {
            if (!this.session.ArePrivateWorkingCopySupported()) {
                Assert.Ignore("This session does not support updates on private working copies");
            }
        }

        protected void EnsureThatContentHashesAreSupportedByServerTypeSystem() {
            if (!this.session.IsContentStreamHashSupported()) {
                Assert.Ignore("Server type system does not support content hashes");
            }
        }

        protected void AssertThatDatesAreEqual(DateTime? expected, DateTime? actual, string msg = null) {
            if (msg != null) {
                Assert.That((DateTime)actual, Is.EqualTo((DateTime)expected).Within(1).Seconds, msg);
            } else {
                Assert.That((DateTime)actual, Is.EqualTo((DateTime)expected).Within(1).Seconds);
            }
        }

        protected void InitializeAndRunRepo(bool swallowExceptions = false) {
            this.repo.Initialize();
            this.repo.SingleStepQueue.SwallowExceptions = swallowExceptions;
            this.repo.Run();
        }

        protected void AddStartNextSyncEvent(bool forceCrawl = false) {
            if (!this.ContentChangesActive) {
                forceCrawl = true;
            }

            this.repo.SingleStepQueue.AddEvent(new StartNextSyncEvent(forceCrawl));
        }

        protected void AssertThatContentHashIsEqualToExceptedIfSupported(IDocument doc, string content) {
            if (this.session.IsContentStreamHashSupported()) {
                Assert.That(doc.VerifyThatIfTimeoutIsExceededContentHashIsEqualTo(content), Is.True, "Timout exceeded");
            }
        }

        protected virtual void Dispose(bool disposing) {
            if (this.disposed) {
                return;
            }

            if (disposing) {
                if (this.repo != null) {
                    this.repo.Dispose();
                }
            }

            this.disposed = true;
        }

        protected class BlockingSingleConnectionScheduler : CmisSync.Lib.Queueing.ConnectionScheduler {
            public BlockingSingleConnectionScheduler(ConnectionScheduler original, IAuthenticationProvider authProvider = null, ISessionFactory sessionFactory = null) : base(original) {
                if (authProvider != null) {
                    this.AuthProvider = authProvider;
                }

                if (sessionFactory != null) {
                    this.SessionFactory = sessionFactory;
                }
            }

            public override void Start() {
                if (!base.Connect()) {
                    Assert.Fail("Connection failed");
                }
            }
        }

        protected class CmisRepoMock : CmisSync.Lib.Cmis.Repository {
            public SingleStepEventQueue SingleStepQueue;
            public IAuthenticationProvider AuthProvider;
            public ISessionFactory SessionFactory;
            public CmisRepoMock(RepoInfo repoInfo, ActivityListenerAggregator activityListener, SingleStepEventQueue queue) : base(repoInfo, activityListener, true, queue) {
                this.SingleStepQueue = queue;
            }

            public void Run() {
                this.SingleStepQueue.Run();
                Thread.Sleep(500);
                this.SingleStepQueue.Run();
            }

            public void RunDbObjectValidationCheck() {
                this.storage.ValidateObjectStructure();
            }

            public override void Initialize() {
                ConnectionScheduler original = this.connectionScheduler;
                this.connectionScheduler = new BlockingSingleConnectionScheduler(original, this.AuthProvider, this.SessionFactory);
                original.Dispose();
                base.Initialize();
                this.Queue.EventManager.RemoveEventHandler(this.Scheduler);
                this.Scheduler.Stop();
            }
        }
    }
}