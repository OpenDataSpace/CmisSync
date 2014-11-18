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
    public class FullRepoTests : IsTestWithConfiguredLog4Net
    {
        private static readonly string SubfolderBase = "FullRepoTests_";
        private static dynamic config;
        private string subfolder;
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
            var transmissionManager = new ActiveActivitiesManager();
            var activityAggregator = new ActivityListenerAggregator(activityListener.Object, transmissionManager);
            var queue = new SingleStepEventQueue(new SyncEventManager());

            this.repo = new CmisRepoMock(this.repoInfo, activityAggregator, queue);

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

        [Test, Category("Slow")]
        public void OneLocalFolderCreated()
        {
            this.localRootDir.CreateSubdirectory("Cat");

            this.repo.Initialize();

            this.repo.Run();
            var children = this.remoteRootDir.GetChildren();
            Assert.AreEqual(children.TotalNumItems, 1);
        }

        [Test, Category("Slow"), Category("Erratic")]
        public void OneLocalFolderRemoved()
        {
            this.localRootDir.CreateSubdirectory("Cat");

            this.repo.Initialize();

            this.repo.Run();

            this.localRootDir.GetDirectories().First().Delete();

            this.WaitUntilQueueIsNotEmpty(this.repo.SingleStepQueue, 15000);

            this.repo.Run();

            Assert.That(this.remoteRootDir.GetChildren(), Is.Empty);
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
        public void OneRemoteFolderIsDeleted()
        {
            this.remoteRootDir.CreateFolder("Cat");

            this.repo.Initialize();
            this.repo.Run();

            (this.remoteRootDir.GetChildren().First() as IFolder).DeleteTree(true, null, true);

            this.repo.SingleStepQueue.AddEvent(new StartNextSyncEvent(true));
            this.repo.Run();

            Assert.That(this.localRootDir.GetDirectories().Length, Is.EqualTo(0));
            Assert.That(this.remoteRootDir.GetChildren().Count(), Is.EqualTo(0));
        }

        [Test, Category("Slow"), Category("Conflict")]
        public void OneRemoteFolderIsDeletedAndOneUnsyncedFileExistsInTheCorrespondingLocalFolder()
        {
            string folderName = "Cat";
            string fileName = "localFile.bin";
            var folder = this.remoteRootDir.CreateFolder(folderName);
            folder.CreateDocument("foo.txt", "bar");
            this.repo.Initialize();
            this.repo.Run();
            this.repo.SingleStepQueue.SwallowExceptions = true;

            using (var file = File.Open(Path.Combine(this.localRootDir.GetDirectories().First().FullName, fileName), FileMode.Create)) {
            }

            this.remoteRootDir.Refresh();
            (this.remoteRootDir.GetChildren().First() as IFolder).DeleteTree(false, null, true);
            Assert.That(this.remoteRootDir.GetChildren().Count(), Is.EqualTo(0));

            this.WaitUntilQueueIsNotEmpty(this.repo.SingleStepQueue);

            this.repo.Run();

            Assert.That(this.localRootDir.GetDirectories().First().Name, Is.EqualTo(folderName));
            Assert.That(this.localRootDir.GetDirectories().First().GetFiles().First().Name, Is.EqualTo(fileName));
            Assert.That(this.localRootDir.GetDirectories().First().GetFiles().Count(), Is.EqualTo(1));
            Assert.That(this.remoteRootDir.GetChildren().First().Name, Is.EqualTo(folderName));
            Assert.That((this.remoteRootDir.GetChildren().First() as IFolder).GetChildren().First().Name, Is.EqualTo(fileName));
            Assert.That((this.remoteRootDir.GetChildren().First() as IFolder).GetChildren().Count(), Is.EqualTo(1));
        }

        [Test, Category("Slow"), Category("Erratic")]
        public void OneRemoteFolderIsRenamedAndOneCrawlSyncShouldDetectIt()
        {
            var remoteFolder = this.remoteRootDir.CreateFolder("Cat");

            this.repo.Initialize();

            this.repo.Run();

            remoteFolder.Refresh();
            remoteFolder.Rename("Dog", true);

            this.repo.Queue.AddEvent(new StartNextSyncEvent(true));
            this.repo.SingleStepQueue.SwallowExceptions = true;
            this.repo.Run();

            Assert.That(this.localRootDir.GetDirectories().Length, Is.EqualTo(1));
            Assert.That(this.localRootDir.GetDirectories()[0].Name, Is.EqualTo("Dog"));
        }

        [Test, Category("Slow"), Category("Erratic")]
        public void OneRemoteFolderIsMovedIntoAnotherRemoteFolderAndDetectedByCrawler()
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

        [Test, Category("Slow"), Category("Erratic")]
        public void OneRemoteFolderIsMovedIntoAnotherRemoteFolderAndDetectedByContentChange()
        {
            var remoteFolder = this.remoteRootDir.CreateFolder("Cat");
            var remoteTargetFolder = this.remoteRootDir.CreateFolder("target");

            this.repo.Initialize();

            this.repo.Run();

            remoteFolder.Move(this.remoteRootDir, remoteTargetFolder);
            Thread.Sleep(30000);

            this.repo.Queue.AddEvent(new StartNextSyncEvent());

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

            DateTime modificationDate = fileInfo.LastWriteTimeUtc;

            this.repo.Initialize();

            this.repo.Run();
            var children = this.remoteRootDir.GetChildren();
            Assert.That(children.TotalNumItems, Is.EqualTo(1));
            var child = children.First();
            Assert.That(child, Is.InstanceOf(typeof(IDocument)));
            var doc = child as IDocument;
            Assert.That(doc.ContentStreamLength, Is.GreaterThan(0), "ContentStream not set");
            Assert.That(this.localRootDir.GetFiles().First().LastWriteTimeUtc, Is.EqualTo(modificationDate));
        }

        [Test, Category("Slow")]
        public void OneLocalFileCreatedAndModificationDateIsSynced()
        {
            if (!this.session.IsServerAbleToUpdateModificationDate()) {
                Assert.Ignore("Server does not support the synchronization of modification dates");
            }

            string fileName = "file";
            string content = "content";
            var filePath = Path.Combine(this.localRootDir.FullName, fileName);
            var fileInfo = new FileInfo(filePath);
            using (StreamWriter sw = fileInfo.CreateText()) {
                sw.WriteLine(content);
            }

            DateTime modificationDate = DateTime.UtcNow - TimeSpan.FromHours(1);
            fileInfo.LastWriteTimeUtc = modificationDate;
            modificationDate = fileInfo.LastWriteTimeUtc;

            DateTime creationDate = DateTime.UtcNow - TimeSpan.FromDays(1);
            fileInfo.CreationTimeUtc = creationDate;
            creationDate = fileInfo.CreationTimeUtc;

            this.repo.Initialize();

            this.repo.Run();
            var children = this.remoteRootDir.GetChildren();
            Assert.That(children.TotalNumItems, Is.EqualTo(1));
            var child = children.First();
            Assert.That(child, Is.InstanceOf(typeof(IDocument)));
            var doc = child as IDocument;
            Assert.That(doc.ContentStreamLength, Is.GreaterThan(0), "ContentStream not set");
            this.AssertThatDatesAreEqual(doc.LastModificationDate, modificationDate, "Modification date is not equal");
            this.AssertThatDatesAreEqual(doc.CreationDate, creationDate, "Creation Date is not equal");
            Assert.That(this.localRootDir.GetFiles().First().LastWriteTimeUtc, Is.EqualTo(modificationDate));
        }

        [Test, Category("Slow"), Category("Erratic")]
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
            DateTime modificationDate = fileInfo.LastWriteTimeUtc;

            this.WaitUntilQueueIsNotEmpty(this.repo.SingleStepQueue);

            this.repo.Run();

            var children = this.remoteRootDir.GetChildren();
            Assert.That(children.TotalNumItems, Is.EqualTo(1));
            var child = children.First();
            Assert.That(child, Is.InstanceOf(typeof(IDocument)));
            var doc = child as IDocument;
            Assert.That(doc.ContentStreamLength, Is.GreaterThan(0), "ContentStream not set");
            Assert.That(doc.Name, Is.EqualTo(newFileName));
            Assert.That(this.localRootDir.GetFiles().First().LastWriteTimeUtc, Is.EqualTo(modificationDate));
        }

        [Test, Category("Slow"), Category("Erratic")]
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
            DateTime modificationDate = fileInfo.LastWriteTimeUtc;

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
            Assert.That(this.localRootDir.GetDirectories().First().GetFiles().First().LastWriteTimeUtc, Is.EqualTo(modificationDate));
        }

        [Test, Category("Slow")]
        public void OneLocalFileIsRemoved()
        {
            string fileName = "removingFile.txt";
            string content = string.Empty;
            var filePath = Path.Combine(this.localRootDir.FullName, fileName);
            this.remoteRootDir.CreateDocument(fileName, content);

            this.repo.Initialize();

            this.repo.Run();

            // Stabilize test by waiting for all delayed fs events
            Thread.Sleep(500);

            // Process the delayed fs events
            this.repo.Run();

            new FileInfo(filePath).Delete();

            this.WaitUntilQueueIsNotEmpty(this.repo.SingleStepQueue);

            this.repo.Run();

            Assert.That(this.remoteRootDir.GetChildren().Count(), Is.EqualTo(0));
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

        [Test, Category("Slow")]
        public void OneEmptyRemoteFileCreated()
        {
            string fileName = "file";
            this.remoteRootDir.CreateDocument(fileName, null);

            this.repo.Initialize();

            this.repo.Run();

            var children = this.localRootDir.GetFiles();
            Assert.That(children.Length, Is.EqualTo(1));
            var child = children.First();
            Assert.That(child, Is.InstanceOf(typeof(FileInfo)));
            Assert.That(child.Length, Is.EqualTo(0));
        }

        [Test, Category("Slow")]
        public void OneRemoteFileContentIsDeleted()
        {
            string fileName = "file";
            string content = "content";
            var doc = this.remoteRootDir.CreateDocument(fileName, content);

            this.repo.Initialize();

            this.repo.Run();

            doc.Refresh();
            doc.DeleteContentStream(true);

            this.repo.SingleStepQueue.AddEvent(new StartNextSyncEvent(true));
            this.repo.Run();

            var children = this.localRootDir.GetFiles();
            Assert.That(children.Length, Is.EqualTo(1));
            var child = children.First();
            Assert.That(child, Is.InstanceOf(typeof(FileInfo)));
            Assert.That(child.Length, Is.EqualTo(0));
        }

        [Test, Category("Slow")]
        public void OneRemoteFileUpdated()
        {
            string fileName = "file.txt";
            string content = "cat";
            var doc = this.remoteRootDir.CreateDocument(fileName, content);

            this.repo.Initialize();

            this.repo.Run();

            content += content;
            doc.Refresh();
            doc.SetContent(content);

            Thread.Sleep(5000);

            this.repo.Queue.AddEvent(new StartNextSyncEvent(false));

            this.repo.Run();

            var file = this.localRootDir.GetFiles().First();
            Assert.That(file, Is.InstanceOf(typeof(FileInfo)));
            Assert.That(file.Length, Is.EqualTo(content.Length));
        }

        [Test, Category("Slow")]
        public void OneRemoteFileUpdatedAndRecognizedByContentChanges()
        {
            string fileName = "file.txt";
            string content = "cat";
            var doc = this.remoteRootDir.CreateDocument(fileName, content);

            this.repo.Initialize();

            this.repo.Run();
            this.repo.Queue.AddEvent(new StartNextSyncEvent(false));
            this.repo.Run();
            Thread.Sleep(5000);
            this.repo.Queue.AddEvent(new StartNextSyncEvent(false));
            this.repo.Run();

            content += content;
            doc.Refresh();
            doc.SetContent(content);

            Thread.Sleep(5000);

            this.repo.Queue.AddEvent(new StartNextSyncEvent(false));

            this.repo.Run();

            var file = this.localRootDir.GetFiles().First();
            Assert.That(file, Is.InstanceOf(typeof(FileInfo)));
            Assert.That(file.Length, Is.EqualTo(content.Length));
        }

        [Test, Category("Slow")]
        public void OneRemoteFileUpdatedAndDetectedByCrawlSync()
        {
            string fileName = "file.txt";
            string content = "cat";
            var doc = this.remoteRootDir.CreateDocument(fileName, content);

            this.repo.Initialize();

            this.repo.Run();

            content += content;
            doc.Refresh();
            doc.SetContent(content);

            this.repo.Queue.AddEvent(new StartNextSyncEvent(true));

            this.repo.Run();

            var file = this.localRootDir.GetFiles().First();
            Assert.That(file, Is.InstanceOf(typeof(FileInfo)));
            Assert.That(file.Length, Is.EqualTo(content.Length));
        }

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

            this.session.GetObject(this.remoteRootDir.Id);
            this.remoteRootDir.Refresh();
            Assert.That(this.remoteRootDir.GetChildren().Count(), Is.EqualTo(0));
            Assert.That(this.localRootDir.GetFiles(), Is.Empty);
        }

        [Test, Category("Slow"), Category("Conflict"), Category("Erratic")]
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

        [Test, Category("Slow"), Category("Conflict"), Category("Erratic")]
        public void OneLocalFileIsChangedAndTheRemoteFileIsRemoved()
        {
            string fileName = "fileConflictTest.txt";
            string changedLocalContent = "changedContent";
            string localContent = "local";
            var localPath = Path.Combine(this.localRootDir.FullName, fileName);
            var fileInfo = new FileInfo(localPath);

            using (StreamWriter sw = fileInfo.CreateText()) {
                sw.WriteLine(localContent);
            }

            this.repo.Initialize();
            this.repo.SingleStepQueue.SwallowExceptions = true;
            this.repo.Run();

            this.remoteRootDir.GetChildren().First().Delete(true);
            Assert.That(this.remoteRootDir.GetChildren().Count(), Is.EqualTo(0));
            using (StreamWriter sw = fileInfo.CreateText()) {
                sw.WriteLine(changedLocalContent);
            }

            fileInfo.Refresh();
            long expectedLength = fileInfo.Length;

            this.WaitUntilQueueIsNotEmpty(this.repo.SingleStepQueue);

            this.repo.Queue.AddEvent(new StartNextSyncEvent());
            this.repo.Run();

            Assert.That(this.localRootDir.GetFiles().Count(), Is.EqualTo(1));
            Assert.That(this.remoteRootDir.GetChildren().Count(), Is.EqualTo(1));
            Assert.That((long)(this.remoteRootDir.GetChildren().First() as IDocument).ContentStreamLength, Is.EqualTo(expectedLength));
        }

        [Test, Category("Slow"), Category("Conflict")]
        public void OneLocalAndTheRemoteFileAreBothRenamed() {
            string originalName = "original.txt";
            string localName = "local.txt";
            string remoteName = "remote.txt";

            this.remoteRootDir.CreateDocument(originalName, "content");

            this.repo.Initialize();

            this.repo.Run();

            this.localRootDir.GetFiles().First().MoveTo(Path.Combine(this.localRootDir.FullName, localName));
            this.remoteRootDir.GetChildren().First().Rename(remoteName);

            this.WaitUntilQueueIsNotEmpty(this.repo.SingleStepQueue);

            this.repo.SingleStepQueue.SwallowExceptions = true;
            this.repo.SingleStepQueue.AddEvent(new StartNextSyncEvent(false));
            this.repo.Run();

            Assert.That(this.localRootDir.GetFiles().First().Name, Is.EqualTo(localName));
            Assert.That(this.remoteRootDir.GetChildren().First().Name, Is.EqualTo(remoteName));

            // In Conflict situation
            // Resolve the conflict by renaming the remote file to the local name
            this.remoteRootDir.GetChildren().First().Rename(localName);

            Thread.Sleep(5000);
            this.repo.SingleStepQueue.AddEvent(new StartNextSyncEvent(false));
            this.repo.Run();

            // Conflict is solved
            Assert.That(this.localRootDir.GetFiles().First().Name, Is.EqualTo(localName));
            Assert.That(this.remoteRootDir.GetChildren().First().Name, Is.EqualTo(localName));

            this.remoteRootDir.GetChildren().First().Rename(remoteName);

            Thread.Sleep(5000);
            this.repo.SingleStepQueue.AddEvent(new StartNextSyncEvent(false));
            this.repo.SingleStepQueue.SwallowExceptions = false;
            this.repo.Run();

            Assert.That(this.localRootDir.GetFiles().First().Name, Is.EqualTo(remoteName));
        }

        [Test, Category("Slow")]
        public void LocalAndRemoteFolderAreMovedIntoTheSameSubfolder() {
            string oldParentName = "oldParent";
            string newParentName = "newParent";
            string oldName = "moveThis";
            var source = this.remoteRootDir.CreateFolder(oldParentName);
            var folder = source.CreateFolder(oldName);
            var target = this.remoteRootDir.CreateFolder(newParentName);
            this.repo.Initialize();
            this.repo.Run();

            folder.Refresh();
            folder.Move(source, target);

            this.repo.SingleStepQueue.SwallowExceptions = true;
            this.repo.SingleStepQueue.AddEvent(new StartNextSyncEvent(true));
            this.repo.Run();

            var localSource = this.localRootDir.GetDirectories(oldParentName).First();
            var localTarget = this.localRootDir.GetDirectories(newParentName).First();
            Assert.That(localSource.GetFileSystemInfos(), Is.Empty);
            Assert.That(localTarget.GetFileSystemInfos().Count(), Is.EqualTo(1));
            var localFolder = localTarget.GetDirectories().First();
            folder.Refresh();
            Assert.That(localFolder.Name, Is.EqualTo(folder.Name));
            Assert.That(folder.Name, Is.EqualTo(oldName));
        }

        [Test, Category("Slow"), Category("Erratic")]
        public void OneLocalFileContentIsChanged()
        {
            string fileName = "file.txt";
            string content = "cat";
            byte[] newContent = Encoding.UTF8.GetBytes("new born citty");
            this.remoteRootDir.CreateDocument(fileName, content);

            this.repo.Initialize();

            this.repo.Run();

            using (var filestream = this.localRootDir.GetFiles().First().Open(FileMode.Truncate, FileAccess.Write, FileShare.None)) {
                filestream.Write(newContent, 0, newContent.Length);
            }

            DateTime modificationDate = this.localRootDir.GetFiles().First().LastWriteTimeUtc;

            this.WaitUntilQueueIsNotEmpty(this.repo.SingleStepQueue);

            this.repo.Run();

            var remoteDoc = this.remoteRootDir.GetChildren().First() as IDocument;
            var localDoc = this.localRootDir.GetFiles().First();
            Assert.That(remoteDoc.ContentStreamLength, Is.EqualTo(newContent.Length));
            Assert.That(localDoc.Length, Is.EqualTo(newContent.Length));
            Assert.That(localDoc.LastWriteTimeUtc, Is.EqualTo(modificationDate));
        }

        /// <summary>
        /// Creates the hundred files and sync.
        /// </summary>
        [Test, Category("Slow"), Category("Erratic"), Timeout(1800000), Ignore("Just for benchmarks")]
        public void CreateHundredFilesAndSync()
        {
            DateTime modificationDate = DateTime.UtcNow - TimeSpan.FromDays(1);
            DateTime creationDate = DateTime.UtcNow - TimeSpan.FromDays(2);
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

                fileInfo.Refresh();
                fileInfo.CreationTimeUtc = creationDate;
                fileInfo.LastWriteTimeUtc = modificationDate;
            }

            this.WaitUntilQueueIsNotEmpty(this.repo.SingleStepQueue);

            this.repo.Run();

            Assert.That(this.remoteRootDir.GetChildren().Count(), Is.EqualTo(count));
            if (this.session.IsServerAbleToUpdateModificationDate()) {
                foreach (var remoteFile in this.remoteRootDir.GetChildren()) {
                    this.AssertThatDatesAreEqual(modificationDate, remoteFile.LastModificationDate, string.Format("remote modification date of {0}", remoteFile.Name));
                    this.AssertThatDatesAreEqual(creationDate, remoteFile.CreationDate, string.Format("remote creation date of {0}", remoteFile.Name));
                }

                foreach (var localFile in this.localRootDir.GetFiles()) {
                    this.AssertThatDatesAreEqual(modificationDate, localFile.LastWriteTimeUtc, string.Format("local modification date of {0}", localFile.Name));
                    this.AssertThatDatesAreEqual(creationDate, localFile.CreationTimeUtc, string.Format("local creation date of {0}", localFile.Name));
                }
            }
        }

        [Test, Category("Slow")]
        public void OneLocalFileIsChangedAndRenamed() {
            string fileName = "file.txt";
            string newFileName = "file_1.txt";
            string content = "cat";
            this.remoteRootDir.CreateDocument(fileName, content);
            Thread.Sleep(100);
            this.repo.Initialize();
            this.repo.Run();

            var file = this.localRootDir.GetFiles().First();
            using (var stream = file.AppendText()) {
                stream.Write(content);
            }

            long length = Encoding.UTF8.GetBytes(content).Length * 2;

            file.MoveTo(Path.Combine(this.localRootDir.FullName, newFileName));
            file.Refresh();

            this.WaitUntilQueueIsNotEmpty(this.repo.SingleStepQueue);
            Thread.Sleep(200);
            this.repo.SingleStepQueue.SwallowExceptions = true;
            this.repo.Run();

            var document = this.remoteRootDir.GetChildren().First() as IDocument;
            file = this.localRootDir.GetFiles().First();

            Assert.That(this.localRootDir.GetFiles().Length, Is.EqualTo(1));
            Assert.That(this.remoteRootDir.GetChildren().Count(), Is.EqualTo(1));
            Assert.That(document.Name, Is.EqualTo(newFileName));
            Assert.That(file.Name, Is.EqualTo(newFileName));
            Assert.That(file.Length, Is.EqualTo(length));
            Assert.That(document.ContentStreamLength, Is.EqualTo(length));
        }

        [Test, Category("Slow")]
        public void OneRemoteFileIsChangedAndRenamedDetectedByCrawler() {
            string fileName = "file.txt";
            string newFileName = "file_1.txt";
            string content = "cat";
            var document = this.remoteRootDir.CreateDocument(fileName, content);

            this.repo.Initialize();
            this.repo.Run();
            this.repo.SingleStepQueue.SwallowExceptions = true;

            document.Refresh();
            document.SetContent(content + content, true, true);
            long length = (long)document.ContentStreamLength;
            document.Rename(newFileName);

            this.repo.Queue.AddEvent(new StartNextSyncEvent(true));
            this.repo.Run();

            document = this.remoteRootDir.GetChildren().First() as IDocument;
            var file = this.localRootDir.GetFiles().First();

            Assert.That(this.localRootDir.GetFiles().Length, Is.EqualTo(1));
            Assert.That(this.remoteRootDir.GetChildren().Count(), Is.EqualTo(1));
            Assert.That(document.Name, Is.EqualTo(newFileName));
            Assert.That(file.Name, Is.EqualTo(newFileName));
            Assert.That(file.Length, Is.EqualTo(length));
            Assert.That(document.ContentStreamLength, Is.EqualTo(length));
        }

        [Test, Category("Slow")]
        public void OneRemoteFileIsChangedAndRenamedDetectedByContentChanges() {
            string fileName = "file.txt";
            string newFileName = "file_1.txt";
            string content = "cat";
            var document = this.remoteRootDir.CreateDocument(fileName, content);

            this.repo.Initialize();
            this.repo.Run();
            this.repo.SingleStepQueue.SwallowExceptions = true;

            Thread.Sleep(5000);
            this.repo.SingleStepQueue.AddEvent(new StartNextSyncEvent(false));
            this.repo.Run();

            document.Refresh();
            document.SetContent(content + content, true, true);
            long length = (long)document.ContentStreamLength;
            document.Rename(newFileName);

            Thread.Sleep(5000);
            this.repo.Queue.AddEvent(new StartNextSyncEvent(false));
            this.repo.Run();

            document = this.remoteRootDir.GetChildren().First() as IDocument;
            var file = this.localRootDir.GetFiles().First();

            Assert.That(this.localRootDir.GetFiles().Length, Is.EqualTo(1));
            Assert.That(this.remoteRootDir.GetChildren().Count(), Is.EqualTo(1));
            Assert.That(document.Name, Is.EqualTo(newFileName));
            Assert.That(file.Name, Is.EqualTo(newFileName));
            Assert.That(file.Length, Is.EqualTo(length));
            Assert.That(document.ContentStreamLength, Is.EqualTo(length));
        }

        [Test, Category("Slow")]
        public void OneLocalAndTheCorrespondingRemoteFolderAreBothRenamedToTheSameName() {
            string oldFolderName = "oldName";
            string newFolderName = "newName";

            var folder = this.remoteRootDir.CreateFolder(oldFolderName);

            this.repo.Initialize();
            this.repo.Run();
            this.repo.SingleStepQueue.SwallowExceptions = true;

            // Wait for all fs change events
            Thread.Sleep(500);

            // Stabilize sync process to process all delayed fs events
            this.repo.Run();

            folder.Refresh();
            folder.Rename(newFolderName);
            this.localRootDir.GetDirectories().First().MoveTo(Path.Combine(this.localRootDir.FullName, newFolderName));

            this.repo.SingleStepQueue.AddEvent(new StartNextSyncEvent(true));
            this.repo.Run();

            Assert.That(this.localRootDir.GetDirectories().First().Name, Is.EqualTo(newFolderName));
            Assert.That((this.remoteRootDir.GetChildren().First() as IFolder).Name, Is.EqualTo(newFolderName));
        }

        [Test, Category("Slow")]
        public void EmptyLocalFileIsCreatedAndChangedRemotely() {
            string fileName = "file";
            string content = "content";
            var filePath = Path.Combine(this.localRootDir.FullName, fileName);
            var fileInfo = new FileInfo(filePath);
            using (fileInfo.Open(FileMode.CreateNew)) {
            }

            DateTime modificationDate = fileInfo.LastWriteTimeUtc;

            this.repo.Initialize();
            this.repo.Run();

            Thread.Sleep(5000);
            this.repo.SingleStepQueue.AddEvent(new StartNextSyncEvent(false));
            this.repo.Run();
            Thread.Sleep(5000);
            this.repo.SingleStepQueue.AddEvent(new StartNextSyncEvent(false));
            this.repo.Run();

            var children = this.remoteRootDir.GetChildren();
            Assert.That(children.TotalNumItems, Is.EqualTo(1));
            var child = children.First();
            Assert.That(child, Is.InstanceOf(typeof(IDocument)));
            var doc = child as IDocument;
            doc.SetContent(content, true, true);
            Assert.That(doc.ContentStreamLength, Is.EqualTo(content.Length), "ContentStream not set correctly");

            Thread.Sleep(5000);
            this.repo.SingleStepQueue.AddEvent(new StartNextSyncEvent(false));
            this.repo.Run();

            doc.Refresh();
            Assert.That((this.localRootDir.GetFiles().First().LastWriteTimeUtc - (DateTime)doc.LastModificationDate).Seconds, Is.Not.GreaterThan(1));
            Assert.That(this.localRootDir.GetFiles().First().Length, Is.EqualTo(content.Length));
        }

        [Test, Category("Slow")]
        public void LocalFileRenamedAndDeletedRemotely() {
            string newName = "newtestfile.txt";
            string oldName = "testfile.txt";
            string content = "text";
            this.remoteRootDir.CreateDocument(oldName, content);
            this.repo.Initialize();
            this.repo.Run();

            this.localRootDir.GetFiles().First().MoveTo(Path.Combine(this.localRootDir.FullName, newName));
            this.remoteRootDir.Refresh();
            this.remoteRootDir.GetChildren().First().Delete(true);

            Thread.Sleep(5000);
            this.repo.SingleStepQueue.SwallowExceptions = true;
            this.repo.SingleStepQueue.AddEvent(new StartNextSyncEvent(false));
            this.repo.Run();

            Assert.That(this.localRootDir.GetDirectories(), Is.Empty);
            Assert.That(this.localRootDir.GetFiles().Length, Is.EqualTo(1));
            Assert.That(this.localRootDir.GetFiles().First().Length, Is.EqualTo(content.Length));
            Assert.That(this.localRootDir.GetFiles().First().Name, Is.EqualTo(newName));
            this.remoteRootDir.Refresh();
            Assert.That(this.remoteRootDir.GetChildren().Count(), Is.EqualTo(1));
            var doc = this.remoteRootDir.GetChildren().First() as IDocument;
            Assert.That(doc.ContentStreamLength, Is.EqualTo(content.Length));
            Assert.That(doc.Name, Is.EqualTo(newName));
        }

        [Test, Category("Slow")]
        public void LocalFileMovedAndDeletedRemotely() {
            string newName = "newtestfile.txt";
            string oldName = "testfile.txt";
            string content = "text";

            this.remoteRootDir.CreateFolder("folder").CreateDocument(oldName, content);

            this.repo.Initialize();
            this.repo.Run();

            this.localRootDir.GetDirectories().First().GetFiles().First().MoveTo(Path.Combine(this.localRootDir.FullName, newName));
            this.remoteRootDir.Refresh();
            ((this.remoteRootDir.GetChildren().First() as IFolder).GetChildren().First() as IDocument).Delete(true);

            Thread.Sleep(5000);
            this.repo.SingleStepQueue.SwallowExceptions = true;
            this.repo.SingleStepQueue.AddEvent(new StartNextSyncEvent(false));
            this.repo.Run();

            Assert.That(this.localRootDir.GetDirectories().Length, Is.EqualTo(1));
            Assert.That(this.localRootDir.GetDirectories().First().Name, Is.EqualTo("folder"));
            Assert.That(this.localRootDir.GetFiles().Length, Is.EqualTo(1));
            Assert.That(this.localRootDir.GetFiles().First().Length, Is.EqualTo(content.Length));
            Assert.That(this.localRootDir.GetFiles().First().Name, Is.EqualTo(newName));
            this.remoteRootDir.Refresh();
            Assert.That(this.remoteRootDir.GetChildren().Count(), Is.EqualTo(2));
            foreach (var child in this.remoteRootDir.GetChildren()) {
                if (child is IFolder) {
                    Assert.That(child.Name, Is.EqualTo("folder"));
                } else if (child is IDocument) {
                    var doc = child as IDocument;
                    Assert.That(doc.Name, Is.EqualTo(newName));
                    Assert.That(doc.ContentStreamLength, Is.EqualTo(content.Length));
                } else {
                    Assert.Fail("Child is neither folder nor document");
                }
            }
        }

        [Test, Category("Slow")]
        public void LocalFolderWithContentRenamedAndDeletedRemotely() {
            string newName = "newtestfile.txt";
            string oldName = "testfile.txt";
            string content = "text";
            this.remoteRootDir.CreateFolder("folder").CreateFolder(oldName).CreateDocument("doc", content);
            this.repo.Initialize();
            this.repo.Run();

            this.localRootDir.GetDirectories().First().GetDirectories().First().MoveTo(Path.Combine(this.localRootDir.FullName, newName));
            this.remoteRootDir.Refresh();
            (this.remoteRootDir.GetChildren().First() as IFolder).DeleteTree(true, null, true);

            Thread.Sleep(5000);
            this.repo.SingleStepQueue.SwallowExceptions = true;
            this.repo.SingleStepQueue.AddEvent(new StartNextSyncEvent(false));
            this.repo.Run();

            Assert.That(this.localRootDir.GetFiles(), Is.Empty);
            Assert.That(this.localRootDir.GetDirectories().Length, Is.EqualTo(1));
            Assert.That(this.localRootDir.GetDirectories().First().Name, Is.EqualTo(newName));
            Assert.That(this.localRootDir.GetDirectories().First().GetFiles().First().Name, Is.EqualTo("doc"));
            this.remoteRootDir.Refresh();
            Assert.That(this.remoteRootDir.GetChildren().Count(), Is.EqualTo(1));
            Assert.That(this.remoteRootDir.GetChildren().First().Name, Is.EqualTo(newName));
            var doc = (this.remoteRootDir.GetChildren().First() as IFolder).GetChildren().First() as IDocument;
            Assert.That(doc.ContentStreamLength, Is.EqualTo(content.Length));
            Assert.That(doc.Name, Is.EqualTo("doc"));
        }

        [Test, Category("Slow")]
        public void OneFileIsCopiedAFewTimes() {
            FileSystemInfoFactory fsFactory = new FileSystemInfoFactory();
            var fileNames = new List<string>();
            string fileName = "file";
            string content = "content";
            this.remoteRootDir.CreateDocument(fileName + ".txt", content);
            this.repo.Initialize();
            this.repo.Run();

            var file = this.localRootDir.GetFiles().First();
            fileNames.Add(file.FullName);
            var fileInfo = fsFactory.CreateFileInfo(file.FullName);
            Guid uuid = (Guid)fileInfo.Uuid;
            for (int i = 0; i < 10; i++) {
                var fileCopy = fsFactory.CreateFileInfo(Path.Combine(this.localRootDir.FullName, string.Format("{0}{1}.txt", fileName, i)));
                file.CopyTo(fileCopy.FullName);
                Thread.Sleep(50);
                fileCopy.Refresh();
                fileCopy.Uuid = uuid;
                fileNames.Add(fileCopy.FullName);
            }

            Thread.Sleep(500);

            this.repo.SingleStepQueue.AddEvent(new StartNextSyncEvent(true));
            this.repo.Run();

            Assert.That(this.localRootDir.GetFiles().Length, Is.EqualTo(fileNames.Count));
            foreach (var localFile in this.localRootDir.GetFiles()) {
                Assert.That(fileNames.Contains(localFile.FullName));
                var syncedFileInfo = fsFactory.CreateFileInfo(localFile.FullName);
                Assert.That(syncedFileInfo.Length, Is.EqualTo(content.Length));
                if (localFile.FullName.Equals(file.FullName)) {
                    Assert.That(syncedFileInfo.Uuid, Is.EqualTo(uuid));
                } else {
                    Assert.That(syncedFileInfo.Uuid, Is.Not.Null);
                    Assert.That(syncedFileInfo.Uuid, Is.Not.EqualTo(uuid));
                }
            }
        }

        [Test, Category("Slow")]
        public void OneFileIsCopiedAndTheCopyIsRemoved() {
            FileSystemInfoFactory fsFactory = new FileSystemInfoFactory();
            var fileNames = new List<string>();
            string fileName = "file";
            string content = "content";
            this.remoteRootDir.CreateDocument(fileName + ".txt", content);
            this.repo.Initialize();
            this.repo.Run();

            var file = this.localRootDir.GetFiles().First();
            fileNames.Add(file.FullName);
            var fileInfo = fsFactory.CreateFileInfo(file.FullName);
            Guid uuid = (Guid)fileInfo.Uuid;
            var fileCopy = fsFactory.CreateFileInfo(Path.Combine(this.localRootDir.FullName, fileName + " - copy.txt"));
            file.CopyTo(fileCopy.FullName);
            fileCopy.Refresh();
            fileCopy.Uuid = uuid;
            fileCopy.Delete();
            Thread.Sleep(500);

            this.repo.SingleStepQueue.SwallowExceptions = true;
            this.repo.SingleStepQueue.AddEvent(new StartNextSyncEvent(true));
            this.repo.Run();

            Assert.That(this.localRootDir.GetFiles().Length, Is.EqualTo(1));
            var child = this.localRootDir.GetFiles().First();
            Assert.That(child.Length, Is.EqualTo(content.Length));
            Assert.That(child.Name, Is.EqualTo(fileName + ".txt"));
        }

        [Test, Category("Slow"), Ignore("Not needed anymore")]
        public void CreateFilesWithLongNames() {
            this.repo.Initialize();
            this.repo.Run();
            string content = "content";
            int count = 40;
            string fileNameFormat = "Toller_Langer_Name mit Leerzeichen - Kopie ({0}) - Kopie.pdf";
            for (int i = 0; i < count; i++) {
                var file = new FileInfo(Path.Combine(this.localRootDir.FullName, string.Format(fileNameFormat, i)));
                using (var stream = file.CreateText()) {
                    stream.Write(content);
                }
            }

            Thread.Sleep(500);
            this.repo.SingleStepQueue.SwallowExceptions = true;
            this.repo.SingleStepQueue.AddEvent(new StartNextSyncEvent(false));
            this.repo.Run();

            this.remoteRootDir.Refresh();
            Assert.That(this.remoteRootDir.GetChildren().Count(), Is.EqualTo(count));
            Assert.That(this.localRootDir.GetFiles().Length, Is.EqualTo(count));
            for (int i = 0; i < count; i++) {
                var file = new FileInfo(Path.Combine(this.localRootDir.FullName, string.Format(fileNameFormat, i)));
                Assert.That(file.Length, Is.EqualTo(content.Length), file.FullName);
            }
        }

        [Test, Category("Slow")]
        public void OneLocalAndOneRemoteFileAreBothChangedToTheSameContent() {
            string oldContent = "a";
            string newContent = "bbb";
            this.remoteRootDir.CreateDocument("fileName.txt", oldContent);
            this.repo.Initialize();
            this.repo.SingleStepQueue.SwallowExceptions = true;
            this.repo.Run();

            this.remoteRootDir.Refresh();
            var doc = this.remoteRootDir.GetChildren().First() as IDocument;
            doc.SetContent(newContent);
            var file = this.localRootDir.GetFiles().First();
            using (var stream = file.Open(FileMode.Open, FileAccess.Write, FileShare.None)) {
                byte[] content = Encoding.UTF8.GetBytes(newContent);
                stream.Write(content, 0, content.Length);
            }

            Thread.Sleep(500);
            this.repo.SingleStepQueue.AddEvent(new StartNextSyncEvent());
            this.repo.Run();

            this.remoteRootDir.Refresh();
            doc.Refresh();
            file.Refresh();
            Assert.That(this.remoteRootDir.GetChildren().Count(), Is.EqualTo(1));
            Assert.That(this.localRootDir.GetFiles().Count(), Is.EqualTo(1));
            Assert.That(file.Length, Is.EqualTo(newContent.Length));
            Assert.That(file.Length, Is.EqualTo(doc.ContentStreamLength));
            if (this.session.IsServerAbleToUpdateModificationDate()) {
                this.AssertThatDatesAreEqual(file.LastWriteTimeUtc, doc.LastModificationDate);
            }
        }

        [Test, Category("Slow")]
        public void OneRemoteFolderIsRenamedToLowerCase() {
            string oldFolderName = "A";
            string newFolderName = oldFolderName.ToLower();
            var folder = this.remoteRootDir.CreateFolder(oldFolderName);

            this.repo.Initialize();
            this.repo.Run();

            folder.Refresh();
            folder.Rename(newFolderName);
            Thread.Sleep(5000);

            this.repo.SingleStepQueue.AddEvent(new StartNextSyncEvent(false));
            this.repo.Run();

            folder.Refresh();
            Assert.That(this.localRootDir.GetDirectories().Count(), Is.EqualTo(1));
            Assert.That(this.localRootDir.GetDirectories().First().Name, Is.EqualTo(newFolderName).Or.EqualTo(oldFolderName));
            Assert.That(this.remoteRootDir.GetChildren().Count(), Is.EqualTo(1));
            Assert.That(this.remoteRootDir.GetChildren().First().Name, Is.EqualTo(newFolderName));
        }

        [Test, Category("Slow")]
        public void SyncLocalSavedMails() {
            string mailName1 = "mail1.msg";
            var mailPath1 = Path.Combine(this.localRootDir.FullName, mailName1);
            var mailInfo1 = new FileInfo(mailPath1);
            using (StreamWriter sw = mailInfo1.CreateText());
            string mailName2 = "mail2.eml";
            var mailPath2 = Path.Combine(this.localRootDir.FullName, mailName2);
            var mailInfo2 = new FileInfo(mailPath2);
            using (StreamWriter sw = mailInfo2.CreateText());

            this.repo.Initialize();
            this.WaitUntilQueueIsNotEmpty(this.repo.SingleStepQueue);
            this.repo.Run();

            Assert.That(this.remoteRootDir.GetChildren().Count(), Is.EqualTo(2));
            foreach (var mail in this.remoteRootDir.GetChildren()) {
                Assert.That(mail.Name, Is.EqualTo(mailName1).Or.EqualTo(mailName2));
            }
        }

        [Test, Category("Slow")]
        public void OneLocalFileIsRemovedAndChangedRemotely() {
            string fileName = "file.bin";
            string oldContent = "old content";
            string newContent = "new content replaces old content";
            this.remoteRootDir.CreateDocument(fileName, oldContent);

            this.repo.Initialize();
            this.repo.SingleStepQueue.SwallowExceptions = true;
            this.repo.SingleStepQueue.AddEvent(new StartNextSyncEvent());
            this.repo.Run();

            this.localRootDir.GetFiles().First().Delete();
            (this.remoteRootDir.GetChildren().First() as IDocument).SetContent(newContent);

            this.WaitUntilQueueIsNotEmpty(this.repo.SingleStepQueue);
            this.repo.Run();

            Assert.That(this.localRootDir.GetFiles().First().Length, Is.EqualTo(newContent.Length));
        }

        [Test, Category("Slow"), Ignore("Ignore this until the server does not change the changetoken on move operation")]
        public void LocalFileMovedAndRemoteFileMovedToOtherFolder() {
            string fileName = "file.bin";
            string content = "content";
            var doc = this.remoteRootDir.CreateDocument(fileName, content);
            var a = this.remoteRootDir.CreateFolder("A");
            var b = this.remoteRootDir.CreateFolder("B");

            this.repo.Initialize();
            this.repo.Run();

            this.localRootDir.GetFiles().First().MoveTo(Path.Combine(this.localRootDir.FullName, a.Name, fileName));
            doc.Refresh();
            this.remoteRootDir.Refresh();
            b.Refresh();
            doc.Move(this.remoteRootDir, b);

            this.repo.SingleStepQueue.SwallowExceptions = true;
            this.WaitUntilQueueIsNotEmpty(this.repo.SingleStepQueue);
            this.repo.SingleStepQueue.AddEvent(new StartNextSyncEvent());
            this.repo.Run();

            doc.Refresh();
            a.Refresh();
            b.Refresh();
            this.remoteRootDir.Refresh();
            var localA = this.localRootDir.GetDirectories().First().Name == "A" ? this.localRootDir.GetDirectories().First() : this.localRootDir.GetDirectories().Last();
            var localB = this.localRootDir.GetDirectories().First().Name == "B" ? this.localRootDir.GetDirectories().First() : this.localRootDir.GetDirectories().Last();

            Assert.That(this.remoteRootDir.GetChildren().Count(), Is.EqualTo(2));
            Assert.That(this.localRootDir.GetDirectories().Count(), Is.EqualTo(2));
            Assert.That(b.GetChildren().Count(), Is.EqualTo(1));
            Assert.That(b.GetChildren().First().Name, Is.EqualTo(fileName));
            Assert.That(doc.ContentStreamLength, Is.EqualTo(content.Length));
            Assert.That(a.GetChildren().Count(), Is.EqualTo(0));
            Assert.That(this.repo.SingleStepQueue.IsEmpty);
            Assert.That(localA.GetFiles().First().Name, Is.EqualTo(fileName));
            Assert.That(localB.GetFiles().Count(), Is.EqualTo(0));
        }

        [Ignore("It is not possible to handle this situation at the moment")]
        [Test, Category("Slow"), Category("Erratic")]
        public void CyclicRenaming() {
            string folderName1 = "A";
            string folderName2 = "B";
            string subFolderName = "sub";
            var folder1 = this.localRootDir.CreateSubdirectory(folderName1);
            var folder2 = this.localRootDir.CreateSubdirectory(folderName2);
            folder1.CreateSubdirectory(subFolderName);

            this.repo.Initialize();
            this.repo.Run();

            var children = this.remoteRootDir.GetChildren();
            IFolder remoteA = null;
            IFolder remoteB = null;
            foreach (var child in children) {
                if (child.Name.Equals(folderName1)) {
                    remoteA = child as IFolder;
                } else if (child.Name.Equals(folderName2)) {
                    remoteB = child as IFolder;
                }
            }

            Assert.That(remoteA, Is.Not.Null);
            Assert.That(remoteB, Is.Not.Null);

            string fullName1 = folder1.FullName;
            string fullName2 = folder2.FullName;
            folder1.MoveTo(folder1.FullName + "_renamed");
            folder2.MoveTo(fullName1);
            new DirectoryInfo(fullName1 + "_renamed").MoveTo(fullName2);
            this.WaitUntilQueueIsNotEmpty(this.repo.SingleStepQueue);

            this.repo.SingleStepQueue.AddEvent(new StartNextSyncEvent(false));
            this.repo.SingleStepQueue.SwallowExceptions = true;
            this.repo.Run();

            var dirs = this.localRootDir.GetDirectories();
            Assert.That(dirs.Count(), Is.EqualTo(2));
            var a = dirs.First().Name.Equals(folderName1) ? dirs.First() : dirs.Last();
            var b = dirs.First().Name.Equals(folderName2) ? dirs.First() : dirs.Last();
            Assert.That(a.GetDirectories(), Is.Empty);
            Assert.That(b.GetDirectories().Count(), Is.EqualTo(1));
            Assert.That(b.GetDirectories().First().Name, Is.EqualTo(subFolderName));

            remoteA.Refresh();
            remoteB.Refresh();
            Assert.That(remoteA.Name, Is.EqualTo(folderName2));
            Assert.That(remoteB.Name, Is.EqualTo(folderName1));
            Assert.That((remoteA as IFolder).GetChildren().First().Name, Is.EqualTo(subFolderName));
            Assert.That((remoteB as IFolder).GetChildren().Count(), Is.EqualTo(0));
        }

        [Test, Category("Slow")]
        public void ExecutingTheSameFolderMoveTwiceThrowsCmisException() {
            var source = this.remoteRootDir.CreateFolder("source");
            var target = this.remoteRootDir.CreateFolder("target");
            var folder = source.CreateFolder("folder");
            var anotherFolderInstance = this.session.GetObject(folder) as IFolder;

            folder.Move(source, target);

            Assert.Throws<CmisConstraintException>(() => anotherFolderInstance.Move(source, target));
        }

        [Ignore("Mantis issue 4285")]
        [Test, Category("Slow")]
        public void ExecutingTheSameFolderMoveToDifferentTargetsThrowsCmisException() {
            var source = this.remoteRootDir.CreateFolder("source");
            var target1 = this.remoteRootDir.CreateFolder("target1");
            var target2 = this.remoteRootDir.CreateFolder("target2");
            var folder = source.CreateFolder("folder");
            var anotherFolderInstance = this.session.GetObject(folder) as IFolder;

            folder.Move(source, target1);

            Assert.Throws<CmisConstraintException>(() => anotherFolderInstance.Move(source, target2));
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

        private class CmisRepoMock : CmisSync.Lib.Cmis.Repository
        {
            public SingleStepEventQueue SingleStepQueue;

            public CmisRepoMock(RepoInfo repoInfo, ActivityListenerAggregator activityListener, SingleStepEventQueue queue) : base(repoInfo, activityListener, true, queue)
            {
                this.SingleStepQueue = queue;
            }

            public void Run()
            {
                this.SingleStepQueue.Run();
                Thread.Sleep(500);
                this.SingleStepQueue.Run();
            }

            public override void Initialize() {
                ConnectionScheduler original = this.connectionScheduler;
                this.connectionScheduler = new BlockingSingleConnectionScheduler(original);
                original.Dispose();
                base.Initialize();
                this.Queue.EventManager.RemoveEventHandler(this.Scheduler);
                this.Scheduler.Stop();
            }
        }
    }
}
