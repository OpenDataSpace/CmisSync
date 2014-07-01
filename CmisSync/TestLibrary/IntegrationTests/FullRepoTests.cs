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
    using System.Threading;

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

    // Important!
    // These tests tend to be Erratic Tests, they can not be used for QA
    // This more of a "rapid szenario creation" class
    // Please do write predictable unit tests for all fixes (IT here is not enough)

    // Default timeout per test is 15 minutes
    [TestFixture, Timeout(900000)]
    public class FullRepoTests : IsTestWithConfiguredLog4Net
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
            this.repoInfo.RemotePath = this.repoInfo.RemotePath.Replace("//", "/");
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
            foreach (var child in root.GetChildren()) {
                if (child is IFolder && child.Name == Subfolder) {
                    (child as IFolder).DeleteTree(true, null, true);
                }
            }

            this.remoteRootDir = root.CreateFolder(Subfolder);
        }

        [TearDown]
        public void TestDown()
        {
            this.localRootDir.Delete(true);
            this.remoteRootDir.DeleteTree(true, null, true);
            this.repo.Dispose();
        }

        // Ignored because it works but the IT is unpredictable
        // [Ignore]
        [Test, Category("Slow")]
        public void OneLocalFolderCreated()
        {
            this.localRootDir.CreateSubdirectory("Cat");

            this.repo.Initialize();

            this.repo.Run();
            var children = this.remoteRootDir.GetChildren();
            Assert.AreEqual(children.TotalNumItems, 1);
        }

        // Ignored because it works but the IT is unpredictable
        // [Ignore]
        [Test, Category("Slow")]
        public void OneLocalFolderRemoved()
        {
            this.localRootDir.CreateSubdirectory("Cat");

            this.repo.Initialize();

            this.repo.Run();

            this.localRootDir.GetDirectories().First().Delete();

            this.WaitUntilQueueIsNotEmpty(this.repo.SingleStepQueue);

            this.repo.Run();

            Assert.That(this.remoteRootDir.GetChildren(), Is.Empty);
        }

        // Ignored because it works but the IT is unpredictable
        // [Ignore]
        [Test, Category("Slow")]
        public void OneRemoteFolderCreated()
        {
            this.remoteRootDir.CreateFolder("Cat");

            this.repo.Initialize();

            this.repo.Run();

            Assert.That(this.localRootDir.GetDirectories().Length, Is.EqualTo(1));
            Assert.That(this.localRootDir.GetDirectories()[0].Name, Is.EqualTo("Cat"));
        }

        // Ignored because it works but the IT is unpredictable
        // [Ignore]
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

        // Ignored because it works but the IT is unpredictable
        // [Ignore]
        [Test, Category("Slow")]
        public void OneRemoteFolderIsMovedIntoAnotherRemoteFolder()
        {
            var remoteFolder = this.remoteRootDir.CreateFolder("Cat");
            var remoteTargetFolder = this.remoteRootDir.CreateFolder("target");

            this.repo.Initialize();

            this.repo.Run();

            remoteFolder.Move(this.remoteRootDir, remoteTargetFolder);
            Thread.Sleep(5000);

            this.repo.Queue.AddEvent(new StartNextSyncEvent(true));

            this.repo.Run();

            Assert.That(this.localRootDir.GetDirectories().Length, Is.EqualTo(1));
            Assert.That(this.localRootDir.GetDirectories()[0].Name, Is.EqualTo("target"));
            Assert.That(this.localRootDir.GetDirectories()[0].GetDirectories().Length, Is.EqualTo(1));
            Assert.That(this.localRootDir.GetDirectories()[0].GetDirectories()[0].Name, Is.EqualTo("Cat"));
        }

        // Ignored because it works but the IT is unpredictable
        // [Ignore]
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

        // Ignored because it works but the IT is unpredictable
        // [Ignore]
        [Test, Category("Slow")]
        public void OneLocalFileRenamed()
        {
            string fileName = "file";
            string newFileName = "renamedFile";
            string content = "content";
            var filePath = Path.Combine(this.localRootDir.FullName, fileName);
            var fileInfo = new FileInfo(filePath);
            using (StreamWriter sw = fileInfo.CreateText()) {
                sw.WriteLine(content);
            }

            this.repo.Initialize();

            this.repo.Run();

            fileInfo.MoveTo(Path.Combine(this.localRootDir.FullName, newFileName));

            this.WaitUntilQueueIsNotEmpty(this.repo.SingleStepQueue);

            this.repo.Run();

            var children = this.remoteRootDir.GetChildren();
            Assert.That(children.TotalNumItems, Is.EqualTo(1));
            var child = children.First();
            Assert.That(child, Is.InstanceOf(typeof(IDocument)));
            var doc = child as IDocument;
            Assert.That(doc.ContentStreamLength, Is.GreaterThan(0), "ContentStream not set");
            Assert.That(doc.Name, Is.EqualTo(newFileName));
        }

        // Ignored because it works but the IT is unpredictable
        // [Ignore]
        [Test, Category("Slow")]
        public void OneLocalFileRenamedAndMoved()
        {
            string fileName = "file";
            string newFileName = "renamedFile";
            string folderName = "folder";
            string content = "content";
            var filePath = Path.Combine(this.localRootDir.FullName, fileName);
            var fileInfo = new FileInfo(filePath);
            using (StreamWriter sw = fileInfo.CreateText()) {
                sw.WriteLine(content);
            }

            this.repo.SingleStepQueue.SwallowExceptions = true;

            this.repo.Initialize();

            this.repo.Run();
            new DirectoryInfo(Path.Combine(this.localRootDir.FullName, folderName)).Create();
            fileInfo.MoveTo(Path.Combine(this.localRootDir.FullName, folderName, newFileName));

            this.WaitUntilQueueIsNotEmpty(this.repo.SingleStepQueue);

            this.repo.Run();

            var children = this.remoteRootDir.GetChildren();
            Assert.That(children.TotalNumItems, Is.EqualTo(1));
            var child = children.First();
            Assert.That(child, Is.InstanceOf(typeof(IFolder)));
            Assert.That((child as IFolder).GetChildren().TotalNumItems, Is.EqualTo(1));
            var doc = (child as IFolder).GetChildren().First() as IDocument;
            Assert.That(doc.ContentStreamLength, Is.EqualTo(fileInfo.Length), "ContentStream not set");
            Assert.That(doc.Name, Is.EqualTo(newFileName));
        }

        // Ignored because it works but the IT is unpredictable
        // [Ignore]
        [Test, Category("Slow")]
        public void OneLocalFileIsRemoved()
        {
            string fileName = "removingFile.txt";
            string content = string.Empty;
            var filePath = Path.Combine(this.localRootDir.FullName, fileName);
            this.remoteRootDir.CreateDocument(fileName, content);

            this.repo.Initialize();

            this.repo.Run();

            new FileInfo(filePath).Delete();

            this.WaitUntilQueueIsNotEmpty(this.repo.SingleStepQueue);

            this.repo.Run();

            Assert.That(this.remoteRootDir.GetChildren(), Is.Empty);
        }

        // Ignored because it works but the IT is unpredictable
        // [Ignore]
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

        // Ignored because it works but the IT is unpredictable
        // [Ignore]
        [Test, Category("Slow")]
        public void OneRemoteFileUpdated()
        {
            string fileName = "file.txt";
            string content = "cat";
            var doc = this.remoteRootDir.CreateDocument(fileName, content);

            this.repo.Initialize();

            this.repo.Run();

            content += content;
            doc.SetContent(content);

            Thread.Sleep(5000);

            this.repo.Queue.AddEvent(new StartNextSyncEvent(false));

            this.repo.Run();

            var file = this.localRootDir.GetFiles().First();
            Assert.That(file, Is.InstanceOf(typeof(FileInfo)));
            Assert.That(file.Length, Is.EqualTo(content.Length));
        }

        // Ignored because it works but the IT is unpredictable
        // [Ignore]
        [Test, Category("Slow")]
        public void RemoteCreatedFileIsDeletedLocally()
        {
            string fileName = "file.txt";
            string content = "cat";
            this.remoteRootDir.CreateDocument(fileName, content);

            this.repo.Initialize();

            this.repo.Run();

            this.localRootDir.GetFiles().First().Delete();

            this.WaitUntilQueueIsNotEmpty(this.repo.SingleStepQueue);

            this.repo.Run();

            Assert.That(this.remoteRootDir.GetChildren(), Is.Empty);
            Assert.That(this.localRootDir.GetFiles(), Is.Empty);
        }

        [Test, Category("Slow"), Category("Conflict")]
        public void OneLocalFileAndOneRemoteFileIsCreatedAndOneConflictFileIsCreated()
        {
            string fileName = "fileConflictTest.txt";
            string remoteContent = "remotecontent";
            string localContent = "local";

            this.remoteRootDir.CreateDocument(fileName, remoteContent);
            var localDoc = Path.Combine(this.localRootDir.FullName, fileName);
            var fileInfo = new FileInfo(localDoc);
            using (StreamWriter sw = fileInfo.CreateText()) {
                sw.WriteLine(localContent);
            }

            Thread.Sleep(200);

            this.repo.Initialize();
            this.repo.SingleStepQueue.SwallowExceptions = true;
            this.repo.Run();

            Assert.That(this.localRootDir.GetFiles().Length, Is.EqualTo(2));
            Assert.That(new FileInfo(localDoc).Length, Is.EqualTo(remoteContent.Length));
            Assert.That(this.remoteRootDir.GetChildren().Count(), Is.EqualTo(2));
        }

        // [Ignore]
        [Test, Category("Slow"), Category("Conflict")]
        public void OneLocalFileIsChangedAndTheRemoteFileIsRemoved()
        {
            string fileName = "fileConflictTest.txt";
            string remoteContent = "remote";
            string localContent = "local";
            var localPath = Path.Combine(this.localRootDir.FullName, fileName);
            var fileInfo = new FileInfo(localPath);

            using (StreamWriter sw = fileInfo.CreateText()) {
                sw.WriteLine(remoteContent);
            }

            this.repo.Initialize();
            this.repo.SingleStepQueue.SwallowExceptions = true;
            this.repo.Run();

            this.remoteRootDir.GetChildren().First().Delete(true);
            using (StreamWriter sw = fileInfo.CreateText()) {
                sw.WriteLine(localContent);
            }

            this.WaitUntilQueueIsNotEmpty(this.repo.SingleStepQueue);

            this.repo.Queue.AddEvent(new StartNextSyncEvent());
            this.repo.Run();

            Assert.That(this.localRootDir.GetFiles(), Is.Empty);
        }

        // Conflict solver is not yet implemented
        [Ignore]
        [Test, Category("Slow"), Category("Conflict")]
        public void OneLocalAndTheRemoteFileAreBothRenamed() {
            string originalName = "original.txt";
            string localName = "local.txt";
            string remoteName = "remote.txt";

            this.remoteRootDir.CreateDocument(originalName, string.Empty);

            this.repo.Initialize();

            this.repo.Run();

            this.localRootDir.GetFiles().First().MoveTo(Path.Combine(this.localRootDir.FullName, localName));
            this.remoteRootDir.GetChildren().First().Rename(remoteName);

            this.WaitUntilQueueIsNotEmpty(this.repo.SingleStepQueue);

            this.repo.Run();

            Assert.That(this.localRootDir.GetFiles().First().Name, Is.EqualTo(remoteName));
        }

        // Ignored because it works but the IT is unpredictable
        // [Ignore]
        [Test, Category("Slow")]
        public void OneLocalFileContentIsChanged()
        {
            string fileName = "file.txt";
            string content = "cat";
            string newContent = "new born citty";
            this.remoteRootDir.CreateDocument(fileName, content);

            this.repo.Initialize();

            this.repo.Run();

            using (var filestream = this.localRootDir.GetFiles().First().CreateText()) {
                filestream.Write(newContent);
            }

            this.WaitUntilQueueIsNotEmpty(this.repo.SingleStepQueue);

            this.repo.Run();

            var remoteDoc = this.remoteRootDir.GetChildren().First() as IDocument;
            var localDoc = this.localRootDir.GetFiles().First();
            Assert.That(remoteDoc.ContentStreamLength, Is.EqualTo(newContent.Length));
            Assert.That(localDoc.Length, Is.EqualTo(newContent.Length));
            Assert.That((localDoc.LastWriteTimeUtc - remoteDoc.LastModificationDate).Value.Seconds, Is.EqualTo(0));
        }

        // Ignored because it works but it takes a long time
        [Ignore]
        [Test, Category("Slow"), Timeout(1800000)]
        public void CreateHundredFilesAndSync()
        {
            int count = 100;
            this.repo.Initialize();
            this.repo.Run();
            this.repo.SingleStepQueue.SwallowExceptions = true;

            for (int i = 1; i <= count; i++) {
                var filePath = Path.Combine(this.localRootDir.FullName, string.Format("file_{0}.bin", i.ToString()));
                var fileInfo = new FileInfo(filePath);
                using (StreamWriter sw = fileInfo.CreateText()) {
                    sw.WriteLine(string.Format("content of file \"{0}\"", filePath));
                }
            }

            this.WaitUntilQueueIsNotEmpty(this.repo.SingleStepQueue);

            this.repo.Run();

            Assert.That(this.remoteRootDir.GetChildren().Count(), Is.EqualTo(count));
        }

        private void WaitUntilQueueIsNotEmpty(SingleStepEventQueue queue, int timeout = 5000) {
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
