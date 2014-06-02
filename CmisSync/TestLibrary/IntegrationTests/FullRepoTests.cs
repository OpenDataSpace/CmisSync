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

    using CmisSync.Lib;
    using CmisSync.Lib.Config;
    using CmisSync.Lib.Credentials;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Sync;

    using DotCMIS;
    using DotCMIS.Client;
    using DotCMIS.Client.Impl;

    using log4net;

    using Moq;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    // Default timeout per test is 15 minutes
    [TestFixture, Timeout(900000)]
    public class FullRepoTests
    {
        private static readonly string Subfolder = "FullRepoTests";
        private RepoInfo repoInfo;
        private DirectoryInfo localRootDir;
        private IFolder remoteRootDir;
        private ISession session;
        private CmisRepoMock repo;

        [TestFixtureSetUp]
        public void ClassInit()
        {
            // Disable HTTPS Verification
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            log4net.Config.XmlConfigurator.Configure(ConfigManager.CurrentConfig.GetLog4NetConfig());
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
            var config = ITUtils.GetConfig();

            // RepoInfo
            this.repoInfo = new RepoInfo {
                AuthenticationType = AuthenticationType.BASIC,
                LocalPath = Path.Combine(config[1].ToString(), Subfolder),
                RemotePath = config[2].ToString() + "/" + Subfolder,
                Address = new XmlUri(new Uri(config[3].ToString())),
                User = config[4].ToString(),
                RepositoryId = config[6].ToString()
            };
            this.repoInfo.SetPassword(config[5].ToString());

            // FileSystemDir
            this.localRootDir = new DirectoryInfo(this.repoInfo.LocalPath);
            this.localRootDir.Create();

            // Repo
            var activityListener = new Mock<IActivityListener>();
            var queue = new SingleStepEventQueue(new SyncEventManager());
            this.repo = new CmisRepoMock(this.repoInfo, activityListener.Object, queue);

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
            this.remoteRootDir = root.CreateFolder(Subfolder);
        }

        [TearDown]
        public void TestDown()
        {
            this.localRootDir.Delete(true);
            this.remoteRootDir.DeleteTree(true, null, true);
            this.repo.Dispose();
        }

        [Test, Category("Slow")]
        public void OneLocalFolderCreated()
        {
            this.localRootDir.CreateSubdirectory("Cat");

            this.repo.Initialize();

            this.repo.Run();
            var children = this.remoteRootDir.GetChildren();
            Assert.AreEqual(children.TotalNumItems, 1);
        }

        [Test, Category("Slow")]
        public void OneRemoteFolderCreated()
        {
            this.remoteRootDir.CreateFolder("Cat");

            this.repo.Initialize();

            this.repo.Run();

            Assert.That(this.localRootDir.GetDirectories().Length, Is.EqualTo(1));
            Assert.That(this.localRootDir.GetDirectories()[0].Name, Is.EqualTo("Cat"));
        }

        [Test, Category("Slow")]
        public void OneRemoteFolderIsRenamedAndOneCrawlSyncShouldDetectIt()
        {
            var remoteFolder = this.remoteRootDir.CreateFolder("Cat");

            this.repo.Initialize();

            this.repo.Run();

            remoteFolder.Rename("Dog", true);

            this.repo.Queue.AddEvent(new StartNextSyncEvent(true));

            this.repo.Run();

            Assert.That(this.localRootDir.GetDirectories().Length, Is.EqualTo(1));
            Assert.That(this.localRootDir.GetDirectories()[0].Name, Is.EqualTo("Dog"));
        }

        [Test, Category("Slow")]
        public void OneRemoteFolderIsMovedIntoAnotherRemoteFolder()
        {
            var remoteFolder = this.remoteRootDir.CreateFolder("Cat");
            var remoteTargetFolder = this.remoteRootDir.CreateFolder("target");

            this.repo.Initialize();

            this.repo.Run();

            remoteFolder.Move(this.remoteRootDir, remoteTargetFolder);

            this.repo.Queue.AddEvent(new StartNextSyncEvent(true));

            this.repo.Run();

            Assert.That(this.localRootDir.GetDirectories().Length, Is.EqualTo(1));
            Assert.That(this.localRootDir.GetDirectories()[0].Name, Is.EqualTo("target"));
            Assert.That(this.localRootDir.GetDirectories()[0].GetDirectories().Length, Is.EqualTo(1));
            Assert.That(this.localRootDir.GetDirectories()[0].GetDirectories()[0].Name, Is.EqualTo("Cat"));
        }

        [Test, Category("Slow")]
        public void OneLocalFileCreated()
        {
            string fileName = "file";
            string content = "content";
            var filePath = Path.Combine(this.localRootDir.FullName, fileName);
            var fileInfo = new FileInfo(filePath);
            using (StreamWriter sw = fileInfo.CreateText()) {
                sw.WriteLine(content);
            }

            this.repo.Initialize();

            this.repo.Run();
            var children = this.remoteRootDir.GetChildren();
            Assert.That(children.TotalNumItems, Is.EqualTo(1));
            var child = children.First();
            Assert.That(child, Is.InstanceOf(typeof(IDocument)));
            var doc = child as IDocument;
            Assert.That(doc.ContentStreamLength, Is.GreaterThan(0), "ContentStream not set");
        }

        [Test, Category("Slow")]
        public void OneRemoteFileCreated()
        {
            string fileName = "file";
            string content = "content";
            this.remoteRootDir.CreateDocument(fileName, content);

            this.repo.Initialize();

            this.repo.Run();

            var children = this.localRootDir.GetFiles();
            Assert.That(children.Length, Is.EqualTo(1));
            var child = children.First();
            Assert.That(child, Is.InstanceOf(typeof(FileInfo)));
            Assert.That(child.Length, Is.EqualTo(content.Length));
        }

        private class CmisRepoMock : CmisRepo
        {
            public SingleStepEventQueue SingleStepQueue;

            public CmisRepoMock(RepoInfo repoInfo, IActivityListener activityListener, SingleStepEventQueue queue) : base(repoInfo, activityListener, true, queue)
            {
                this.SingleStepQueue = queue;
            }

            public void Run()
            {
                this.SingleStepQueue.Run();
            }

            public override void Initialize() {
                base.Initialize();
                this.Queue.EventManager.RemoveEventHandler(this.Scheduler);
                this.Scheduler.Stop();
            }
        }
    }
}
