
namespace TestLibrary.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
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

    using log4net;

    #if __COCOA__
    using MonoMac.AppKit;
    #endif

    using Moq;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture, Timeout(900000)]
    public abstract class BaseFullRepoTest : IsTestWithConfiguredLog4Net
    {
        protected RepoInfo repoInfo;
        protected DirectoryInfo localRootDir;
        protected IFolder remoteRootDir;
        protected ISession session;
        protected FullRepoTests.CmisRepoMock repo;
        protected ActiveActivitiesManager transmissionManager;

        private static dynamic config;
        private string subfolder;

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
            string testName = this.GetType().Name;
            object[] attributes = this.GetType().GetCustomAttributes(true);
            foreach (var attr in attributes) {
                if (attr is TestNameAttribute) {
                    testName = (attr as TestNameAttribute).Name;
                }
            }

            this.subfolder = testName + "_" + Guid.NewGuid().ToString();
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
            var activityAggregator = new ActivityListenerAggregator(activityListener.Object, this.transmissionManager);
            var queue = new SingleStepEventQueue(new SyncEventManager());

            this.repo = new FullRepoTests.CmisRepoMock(this.repoInfo, activityAggregator, queue);

            // Session
            var cmisParameters = new Dictionary<string, string>();
            cmisParameters[SessionParameter.BindingType] = BindingType.AtomPub;
            cmisParameters[SessionParameter.AtomPubUrl] = this.repoInfo.Address.ToString();
            cmisParameters[SessionParameter.User] = this.repoInfo.User;
            cmisParameters[SessionParameter.Password] = this.repoInfo.GetPassword().ToString();
            cmisParameters[SessionParameter.RepositoryId] = this.repoInfo.RepositoryId;
            cmisParameters[SessionParameter.UserAgent] = Utils.CreateUserAgent();

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

        protected void WaitUntilQueueIsNotEmpty(SingleStepEventQueue queue, int timeout = 10000) {
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

        protected class BlockingSingleConnectionScheduler : CmisSync.Lib.Queueing.ConnectionScheduler {
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