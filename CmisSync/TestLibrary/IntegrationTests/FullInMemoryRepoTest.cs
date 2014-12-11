//-----------------------------------------------------------------------
// <copyright file="FullInMemoryRepoTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using CmisSync.Lib;
    using CmisSync.Lib.Cmis.ConvenienceExtenders;
    using CmisSync.Lib.Config;
    using CmisSync.Lib.Queueing;

    using DotCMIS;
    using DotCMIS.Client;
    using DotCMIS.Client.Impl;

    #if __COCOA__
    using MonoMac.AppKit;
    #endif

    using Moq;
    using NUnit.Framework;

    using TestLibrary.MockedServer;
    using TestLibrary.TestUtils;

    [TestFixture, Timeout(900000), Ignore]
    public class FullInMemoryRepoTest : IsTestWithConfiguredLog4Net
    {
        private static readonly string SubfolderBase = "FullInMemoryRepoTests_";
        private static dynamic config;
        private string subfolder;
        private DirectoryInfo localRootDir;
        private IFolder remoteRootDir;
        private ISession session;
        private TestLibrary.IntegrationTests.FullRepoTests.CmisRepoMock repo;

        [TestFixtureSetUp]
        public void ClassInit()
        {
#if __COCOA__
            try {
                NSApplication.Init();
            } catch (InvalidOperationException) {
            }
#endif
            config = ITUtils.GetConfig();
        }

        [SetUp]
        public void Init()
        {
            this.subfolder = SubfolderBase + Guid.NewGuid().ToString();
            Console.WriteLine("Working on " + this.subfolder);

            // RepoInfo
            var repoInfo = new RepoInfo {
                AuthenticationType = AuthenticationType.BASIC,
                LocalPath = Path.Combine(config[1].ToString(), this.subfolder),
                RemotePath = "/",
                Address = new XmlUri(new Uri("http://in.memory.de/")),
                User = "user",
                RepositoryId = Guid.NewGuid().ToString(),
            };

            repoInfo.SetPassword("password");

            // FileSystemDir
            this.localRootDir = new DirectoryInfo(repoInfo.LocalPath);
            this.localRootDir.Create();

            // Repo
            var activityListener = new Mock<IActivityListener>();
            var transmissionManager = new ActiveActivitiesManager();
            var activityAggregator = new ActivityListenerAggregator(activityListener.Object, transmissionManager);
            var queue = new SingleStepEventQueue(new SyncEventManager());

            this.repo = new TestLibrary.IntegrationTests.FullRepoTests.CmisRepoMock(repoInfo, activityAggregator, queue);

            // Session
            var cmisParameters = new Dictionary<string, string>();
            cmisParameters[SessionParameter.BindingType] = BindingType.AtomPub;
            cmisParameters[SessionParameter.AtomPubUrl] = repoInfo.Address.ToString();
            cmisParameters[SessionParameter.User] = repoInfo.User;
            cmisParameters[SessionParameter.Password] = repoInfo.GetPassword().ToString();
            cmisParameters[SessionParameter.RepositoryId] = repoInfo.RepositoryId;

            var factory = new Mock<ISessionFactory>();
            var repository = MockOfIRepository.GetRepository(repoInfo.RepositoryId);
            factory.SetupRepositories(repository.Object);
            this.session = factory.Object.CreateSession(cmisParameters);

            IFolder root = (IFolder)this.session.GetObjectByPath(repoInfo.RemotePath);
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

        [Test, Category("Medium")]
        public void TestToConnect() {
        }
    }
}
