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

namespace TestLibrary.IntegrationTests {
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

    using Moq;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture, Category("Slow"), TestName("FullRepo"), Timeout(180000)]
    public class FullRepoTests : BaseFullRepoTest {
        [Test]
        public void OneLocalFolderRemoved() {
            this.localRootDir.CreateSubdirectory("Cat");

            this.InitializeAndRunRepo();

            this.localRootDir.GetDirectories().First().Delete();

            this.WaitUntilQueueIsNotEmpty(this.repo.SingleStepQueue, 15000);

            this.repo.Run();

            Assert.That(this.remoteRootDir.GetChildren(), Is.Empty);
        }

        [Test]
        public void OneRemoteFolderCreated() {
            this.remoteRootDir.CreateFolder("Cat");

            this.InitializeAndRunRepo();

            Assert.That(this.localRootDir.GetDirectories().Length, Is.EqualTo(1));
            Assert.That(this.localRootDir.GetDirectories()[0].Name, Is.EqualTo("Cat"));
            AssertThatFolderStructureIsEqual();
        }

        [Test]
        public void OneRemoteFolderIsDeleted() {
            this.remoteRootDir.CreateFolder("Cat");

            this.InitializeAndRunRepo();

            (this.remoteRootDir.GetChildren().First() as IFolder).DeleteTree(true, null, true);

            this.AddStartNextSyncEvent(forceCrawl: true);
            this.repo.Run();

            Assert.That(this.localRootDir.GetDirectories().Length, Is.EqualTo(0));
            Assert.That(this.remoteRootDir.GetChildren().Count(), Is.EqualTo(0));
            AssertThatFolderStructureIsEqual();
        }

        [Test, Category("Conflict")]
        public void OneRemoteFolderIsDeletedAndOneUnsyncedFileExistsInTheCorrespondingLocalFolder() {
            string folderName = "Cat";
            string fileName = "localFile.bin";
            var folder = this.remoteRootDir.CreateFolder(folderName);
            folder.CreateDocument("foo.bin", "bar");
            this.InitializeAndRunRepo();
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

        [Test]
        public void OneRemoteFolderIsRenamedAndOneCrawlSyncShouldDetectIt() {
            var remoteFolder = this.remoteRootDir.CreateFolder("Cat");

            this.InitializeAndRunRepo();

            remoteFolder.Refresh();
            remoteFolder.Rename("Dog", true);

            this.AddStartNextSyncEvent(forceCrawl: true);
            this.repo.SingleStepQueue.SwallowExceptions = true;
            this.repo.Run();

            Assert.That(this.localRootDir.GetDirectories().Length, Is.EqualTo(1));
            Assert.That(this.localRootDir.GetDirectories()[0].Name, Is.EqualTo("Dog"));
            AssertThatFolderStructureIsEqual();
        }

        [Test]
        public void OneRemoteFolderIsMovedIntoAnotherRemoteFolderAndDetectedByCrawler() {
            var remoteFolder = this.remoteRootDir.CreateFolder("Cat");
            var remoteTargetFolder = this.remoteRootDir.CreateFolder("target");

            this.InitializeAndRunRepo();

            remoteFolder.Move(this.remoteRootDir, remoteTargetFolder);

            this.AddStartNextSyncEvent(forceCrawl: true);

            this.repo.Run();

            Assert.That(this.localRootDir.GetDirectories().Length, Is.EqualTo(1));
            Assert.That(this.localRootDir.GetDirectories()[0].Name, Is.EqualTo("target"));
            Assert.That(this.localRootDir.GetDirectories()[0].GetDirectories().Length, Is.EqualTo(1));
            Assert.That(this.localRootDir.GetDirectories()[0].GetDirectories()[0].Name, Is.EqualTo("Cat"));
            AssertThatFolderStructureIsEqual();
        }

        [Test]
        public void OneRemoteFolderIsMovedIntoAnotherRemoteFolderAndDetectedByContentChange() {
            this.EnsureThatContentChangesAreSupported();
            var remoteFolder = this.remoteRootDir.CreateFolder("Cat");
            var remoteTargetFolder = this.remoteRootDir.CreateFolder("target");

            this.InitializeAndRunRepo();

            remoteFolder.Move(this.remoteRootDir, remoteTargetFolder);
            this.WaitForRemoteChanges();

            this.AddStartNextSyncEvent();

            this.repo.Run();

            Assert.That(this.localRootDir.GetDirectories().Length, Is.EqualTo(1));
            Assert.That(this.localRootDir.GetDirectories()[0].Name, Is.EqualTo("target"));
            Assert.That(this.localRootDir.GetDirectories()[0].GetDirectories().Length, Is.EqualTo(1));
            Assert.That(this.localRootDir.GetDirectories()[0].GetDirectories()[0].Name, Is.EqualTo("Cat"));
            AssertThatFolderStructureIsEqual();
        }

        [Test]
        public void OneLocalFileRenamed() {
            string fileName = "file";
            string newFileName = "renamedFile";
            string content = "content";
            var filePath = Path.Combine(this.localRootDir.FullName, fileName);
            var fileInfo = new FileInfo(filePath);
            using (StreamWriter sw = fileInfo.CreateText()) {
                sw.Write(content);
            }

            this.InitializeAndRunRepo();

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

        [Test]
        public void OneLocalFileRenamedAndMoved() {
            string fileName = "file";
            string newFileName = "renamedFile";
            string folderName = "folder";
            string content = "content";
            var filePath = Path.Combine(this.localRootDir.FullName, fileName);
            var fileInfo = new FileInfo(filePath);
            using (StreamWriter sw = fileInfo.CreateText()) {
                sw.Write(content);
            }

            this.repo.SingleStepQueue.SwallowExceptions = true;

            this.InitializeAndRunRepo();
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

        [Test]
        public void OneLocalFileIsRemoved() {
            string fileName = "removingFile.bin";
            string content = string.Empty;
            var filePath = Path.Combine(this.localRootDir.FullName, fileName);
            this.remoteRootDir.CreateDocument(fileName, content);

            this.InitializeAndRunRepo();

            // Stabilize test by waiting for all delayed fs events
            Thread.Sleep(500);

            // Process the delayed fs events
            this.repo.Run();

            new FileInfo(filePath).Delete();

            this.WaitUntilQueueIsNotEmpty(this.repo.SingleStepQueue);

            this.repo.Run();

            Assert.That(this.remoteRootDir.GetChildren().Count(), Is.EqualTo(0));
        }

        [Test]
        public void OneRemoteFileCreated([Values(true, false)]bool contentChanges) {
            this.ContentChangesActive = contentChanges;
            string fileName = "file";
            string content = "content";
            var doc = this.remoteRootDir.CreateDocument(fileName, content);

            this.InitializeAndRunRepo();

            var children = this.localRootDir.GetFiles();
            Assert.That(children.Length, Is.EqualTo(1));
            var child = children.First();
            Assert.That(child, Is.InstanceOf(typeof(FileInfo)));
            Assert.That(child.Length, Is.EqualTo(content.Length));
            doc.Refresh();
            doc.AssertThatIfContentHashExistsItIsEqualTo(content);
        }

        [Test]
        public void OneEmptyRemoteFileCreated([Values(true, false)]bool contentChanges) {
            this.ContentChangesActive = contentChanges;
            this.InitializeAndRunRepo();
            string fileName = "file";
            var doc = this.remoteRootDir.CreateDocument(fileName, (string)null);

            this.WaitForRemoteChanges();
            this.AddStartNextSyncEvent();
            this.repo.Run();

            var children = this.localRootDir.GetFiles();
            Assert.That(children.Length, Is.EqualTo(1));
            var child = children.First();
            Assert.That(child, Is.InstanceOf(typeof(FileInfo)));
            Assert.That(child.Length, Is.EqualTo(0));
            Assert.That(this.repo.NumberOfChanges, Is.EqualTo(0));
            doc.Refresh();
            doc.AssertThatIfContentHashExistsItIsEqualTo(new byte[0]);
        }

        // Timeout is set to 10 minutes for 10 x 1 MB file
        [Test, Timeout(600000), MaxTime(600000)]
        public void ManyRemoteFilesCreated([Values(10)]int fileNumber) {
            string content = new string('A', 1024 * 1024);
            for (int i = 0; i < fileNumber; ++i) {
                string fileName = "file" + i.ToString();
                this.remoteRootDir.CreateDocument(fileName, content);
            }

            this.InitializeAndRunRepo();

            var localFiles = this.localRootDir.GetFiles();
            Assert.That(localFiles.Length, Is.EqualTo(fileNumber));
            foreach (var localFile in localFiles) {
                Assert.That(localFile, Is.InstanceOf(typeof(FileInfo)));
                Assert.That(localFile.Length, Is.EqualTo(content.Length));
            }

            var remoteFiles = this.remoteRootDir.GetChildren();
            Assert.That(remoteFiles.TotalNumItems, Is.EqualTo(fileNumber));
            foreach (IDocument remoteFile in remoteFiles.OfType<IDocument>()) {
                Assert.That(remoteFile.ContentStreamLength, Is.EqualTo(content.Length));
                remoteFile.AssertThatIfContentHashExistsItIsEqualTo(content);
            }
        }

        [Test]
        public void OneRemoteFileContentIsDeleted([Values(true, false)]bool contentChanges) {
            this.ContentChangesActive = contentChanges;

            string fileName = "file";
            string content = "content";
            var doc = this.remoteRootDir.CreateDocument(fileName, content);

            this.InitializeAndRunRepo();

            doc.Refresh();
            doc.AssertThatIfContentHashExistsItIsEqualTo(content);
            byte[] hash = doc.ContentStreamHash();
            string oldChangeToken = doc.ChangeToken;
            doc.DeleteContentStream(true);
            string newChangeToken = doc.ChangeToken;
            Assert.That(oldChangeToken, Is.Not.EqualTo(newChangeToken));
            Assert.That(doc.ContentStreamLength, Is.Null.Or.EqualTo(0));
            doc.AssertThatIfContentHashExistsItIsEqualTo(string.Empty, string.Format("old hash was {0}", hash != null ? Utils.ToHexString(hash) : "null"));
            this.WaitForRemoteChanges();
            this.AddStartNextSyncEvent();
            this.repo.Run();

            var children = this.localRootDir.GetFiles();
            Assert.That(children.Length, Is.EqualTo(1));
            var child = children.First();
            Assert.That(child.Length, Is.EqualTo(0), child.ToString());
        }

        [Test]
        public void OneRemoteFileUpdated([Values(true, false)]bool contentChanges) {
            this.ContentChangesActive = contentChanges;
            string fileName = "file.bin";
            string content = "cat";
            var doc = this.remoteRootDir.CreateDocument(fileName, content);

            this.InitializeAndRunRepo();

            content += content;
            doc.Refresh();
            string changeToken = doc.ChangeToken;
            doc.SetContent(content);

            this.WaitForRemoteChanges(sleepDuration: 15000);

            this.AddStartNextSyncEvent();
            this.repo.Run();

            var file = this.localRootDir.GetFiles().First();
            Assert.That(file, Is.InstanceOf(typeof(FileInfo)));
            Assert.That(file.Length, Is.EqualTo(content.Length));
        }

        [Test]
        public void RemoteCreatedFileIsDeletedLocally() {
            string fileName = "file.bin";
            string content = "cat";
            this.remoteRootDir.CreateDocument(fileName, content);

            this.InitializeAndRunRepo();

            this.localRootDir.GetFiles().First().Delete();

            this.WaitUntilQueueIsNotEmpty(this.repo.SingleStepQueue);

            this.repo.Run();

            this.session.GetObject(this.remoteRootDir.Id);
            this.remoteRootDir.Refresh();
            Assert.That(this.remoteRootDir.GetChildren().Count(), Is.EqualTo(0));
            Assert.That(this.localRootDir.GetFiles(), Is.Empty);
        }

        [Test, Category("Conflict")]
        public void OneLocalFileAndOneRemoteFileIsCreatedAndOneConflictFileIsCreated([Values(true, false)]bool contentChanges) {
            this.ContentChangesActive = contentChanges;
            string fileName = "fileConflictTest.bin";
            string remoteContent = "remotecontent";
            string localContent = "local";

            this.remoteRootDir.CreateDocument(fileName, remoteContent);
            var localDoc = Path.Combine(this.localRootDir.FullName, fileName);
            var fileInfo = new FileInfo(localDoc);
            using (StreamWriter sw = fileInfo.CreateText()) {
                sw.Write(localContent);
            }

            Thread.Sleep(200);

            this.InitializeAndRunRepo(swallowExceptions: true);

            this.remoteRootDir.Refresh();
            Assert.That(this.localRootDir.GetFiles().Length, Is.EqualTo(2));
            Assert.That(new FileInfo(localDoc).Length, Is.EqualTo(remoteContent.Length));
            Assert.That(this.remoteRootDir.GetChildren().Count(), Is.EqualTo(2));
        }

        [Test, Category("Conflict")]
        public void OneLocalFileIsChangedAndTheRemoteFileIsRemoved() {
            string fileName = "fileConflictTest.bin";
            string changedLocalContent = "changedContent";
            string localContent = "local";
            var localPath = Path.Combine(this.localRootDir.FullName, fileName);
            var fileInfo = new FileInfo(localPath);

            using (StreamWriter sw = fileInfo.CreateText()) {
                sw.Write(localContent);
            }

            this.InitializeAndRunRepo(swallowExceptions: true);

            this.remoteRootDir.GetChildren().First().Delete(true);
            Assert.That(this.remoteRootDir.GetChildren().Count(), Is.EqualTo(0));
            using (StreamWriter sw = fileInfo.CreateText()) {
                sw.Write(changedLocalContent);
            }

            fileInfo.Refresh();
            long expectedLength = fileInfo.Length;

            this.WaitUntilQueueIsNotEmpty(this.repo.SingleStepQueue);

            this.AddStartNextSyncEvent();
            this.repo.Run();

            Assert.That(this.localRootDir.GetFiles().Count(), Is.EqualTo(1));
            Assert.That(this.remoteRootDir.GetChildren().Count(), Is.EqualTo(1));
            Assert.That((long)(this.remoteRootDir.GetChildren().First() as IDocument).ContentStreamLength, Is.EqualTo(expectedLength));
        }

        [Test, Category("Conflict")]
        public void OneLocalAndTheRemoteFileAreBothRenamed([Values(true, false)]bool contentChanges) {
            this.ContentChangesActive = contentChanges;
            string originalName = "original.bin";
            string localName = "local.bin";
            string remoteName = "remote.bin";

            this.remoteRootDir.CreateDocument(originalName, "content");

            this.InitializeAndRunRepo(swallowExceptions: true);

            this.localRootDir.GetFiles().First().MoveTo(Path.Combine(this.localRootDir.FullName, localName));
            this.remoteRootDir.GetChildren().First().Rename(remoteName);

            this.WaitUntilQueueIsNotEmpty(this.repo.SingleStepQueue);

            this.AddStartNextSyncEvent();
            this.repo.Run();

            Assert.That(this.localRootDir.GetFiles().First().Name, Is.EqualTo(localName));
            Assert.That(this.remoteRootDir.GetChildren().First().Name, Is.EqualTo(remoteName));

            // In Conflict situation
            // Resolve the conflict by renaming the remote file to the local name
            this.remoteRootDir.GetChildren().First().Rename(localName);

            this.WaitForRemoteChanges();
            this.AddStartNextSyncEvent();
            this.repo.Run();

            this.remoteRootDir.Refresh();

            // Conflict is solved
            Assert.That(this.localRootDir.GetFiles().First().Name, Is.EqualTo(localName));
            Assert.That(this.remoteRootDir.GetChildren().First().Name, Is.EqualTo(localName));

            this.remoteRootDir.GetChildren().First().Rename(remoteName);

            this.WaitForRemoteChanges();
            this.AddStartNextSyncEvent();
            this.repo.Run();

            this.remoteRootDir.Refresh();

            Assert.That(this.localRootDir.GetFiles().First().Name, Is.EqualTo(remoteName));
        }

        [Test]
        public void LocalAndRemoteFolderAreMovedIntoTheSameSubfolder([Values(true, false)]bool contentChanges) {
            this.ContentChangesActive = contentChanges;
            string oldParentName = "oldParent";
            string newParentName = "newParent";
            string oldName = "moveThis";
            var source = this.remoteRootDir.CreateFolder(oldParentName);
            var folder = source.CreateFolder(oldName);
            var target = this.remoteRootDir.CreateFolder(newParentName);
            this.InitializeAndRunRepo(swallowExceptions: true);

            folder.Refresh();
            folder.Move(source, target);

            this.WaitForRemoteChanges();
            this.AddStartNextSyncEvent();
            this.repo.Run();

            var localSource = this.localRootDir.GetDirectories(oldParentName).First();
            var localTarget = this.localRootDir.GetDirectories(newParentName).First();
            Assert.That(localSource.GetFileSystemInfos(), Is.Empty);
            Assert.That(localTarget.GetFileSystemInfos().Count(), Is.EqualTo(1));
            var localFolder = localTarget.GetDirectories().First();
            folder.Refresh();
            Assert.That(localFolder.Name, Is.EqualTo(folder.Name));
            Assert.That(folder.Name, Is.EqualTo(oldName));
            AssertThatEventCounterIsZero();
            AssertThatFolderStructureIsEqual();
        }

        [Test]
        public void OneLocalFileContentIsChanged([Values(true, false)]bool contentChanges) {
            this.ContentChangesActive = contentChanges;
            string fileName = "file.bin";
            string content = "cat";
            byte[] newContent = Encoding.UTF8.GetBytes("new born citty");
            this.remoteRootDir.CreateDocument(fileName, content);

            this.InitializeAndRunRepo();

            using (var filestream = this.localRootDir.GetFiles().First().Open(FileMode.Truncate, FileAccess.Write, FileShare.None)) {
                filestream.Write(newContent, 0, newContent.Length);
            }

            DateTime modificationDate = this.localRootDir.GetFiles().First().LastWriteTimeUtc;

            this.WaitUntilQueueIsNotEmpty(this.repo.SingleStepQueue);
            this.AddStartNextSyncEvent();
            this.repo.Run();

            var remoteDoc = this.remoteRootDir.GetChildren().First() as IDocument;
            var localDoc = this.localRootDir.GetFiles().First();
            Assert.That(remoteDoc.ContentStreamLength, Is.EqualTo(newContent.Length));
            remoteDoc.AssertThatIfContentHashExistsItIsEqualTo(newContent);
            Assert.That(localDoc.Length, Is.EqualTo(newContent.Length));
            Assert.That(localDoc.LastWriteTimeUtc, Is.EqualTo(modificationDate));
        }

        /// <summary>
        /// Creates the hundred files and sync.
        /// </summary>
        [Test, Timeout(1800000), MaxTime(1800000), Ignore("Just for benchmarks")]
        public void CreateHundredFilesAndSync() {
            DateTime modificationDate = DateTime.UtcNow - TimeSpan.FromDays(1);
            DateTime creationDate = DateTime.UtcNow - TimeSpan.FromDays(2);
            int count = 100;

            this.InitializeAndRunRepo();
            this.repo.SingleStepQueue.SwallowExceptions = true;

            for (int i = 1; i <= count; i++) {
                var filePath = Path.Combine(this.localRootDir.FullName, string.Format("file_{0}.bin", i.ToString()));
                var fileInfo = new FileInfo(filePath);
                using (StreamWriter sw = fileInfo.CreateText()) {
                    sw.Write(string.Format("content of file \"{0}\"", filePath));
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

        [Test]
        public void OneLocalFileIsChangedAndRenamed([Values(true, false)]bool contentChanges) {
            this.ContentChangesActive = contentChanges;
            string fileName = "file.bin";
            string newFileName = "file_1.bin";
            string content = "cat";
            this.remoteRootDir.CreateDocument(fileName, content);
            Thread.Sleep(100);
            this.InitializeAndRunRepo(swallowExceptions: true);

            var file = this.localRootDir.GetFiles().First();
            using (var stream = file.AppendText()) {
                stream.Write(content);
            }

            long length = Encoding.UTF8.GetBytes(content).Length * 2;

            file.MoveTo(Path.Combine(this.localRootDir.FullName, newFileName));
            file.Refresh();

            this.WaitUntilQueueIsNotEmpty(this.repo.SingleStepQueue);
            this.AddStartNextSyncEvent();
            this.repo.Run();

            this.remoteRootDir.Refresh();
            var document = this.remoteRootDir.GetChildren().First() as IDocument;
            file = this.localRootDir.GetFiles().First();

            Assert.That(this.localRootDir.GetFiles().Length, Is.EqualTo(1));
            Assert.That(this.remoteRootDir.GetChildren().Count(), Is.EqualTo(1));
            Assert.That(document.Name, Is.EqualTo(newFileName));
            Assert.That(file.Name, Is.EqualTo(newFileName));
            Assert.That(file.Length, Is.EqualTo(length));
            Assert.That(document.ContentStreamLength, Is.EqualTo(length));
        }

        [Test]
        public void OneRemoteFileIsChangedAndRenamed([Values(true, false)]bool contentChanges) {
            this.ContentChangesActive = contentChanges;
            string fileName = "file.bin";
            string newFileName = "file_1.bin";
            string content = "cat";
            var document = this.remoteRootDir.CreateDocument(fileName, content);

            this.InitializeAndRunRepo();
            this.repo.SingleStepQueue.SwallowExceptions = true;

            document.Refresh();
            document.SetContent(content + content, true, true);
            long length = (long)document.ContentStreamLength;
            document.Rename(newFileName);

            this.WaitForRemoteChanges();
            this.AddStartNextSyncEvent();
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

        [Test]
        public void OneLocalAndTheCorrespondingRemoteFolderAreBothRenamedToTheSameName() {
            string oldFolderName = "oldName";
            string newFolderName = "newName";

            var folder = this.remoteRootDir.CreateFolder(oldFolderName);

            this.InitializeAndRunRepo();
            this.repo.SingleStepQueue.SwallowExceptions = true;

            // Wait for all fs change events
            Thread.Sleep(500);

            // Stabilize sync process to process all delayed fs events
            this.repo.Run();

            folder.Refresh();
            folder.Rename(newFolderName);
            this.localRootDir.GetDirectories().First().MoveTo(Path.Combine(this.localRootDir.FullName, newFolderName));

            this.AddStartNextSyncEvent(forceCrawl: true);
            this.repo.Run();

            Assert.That(this.localRootDir.GetDirectories().First().Name, Is.EqualTo(newFolderName));
            Assert.That((this.remoteRootDir.GetChildren().First() as IFolder).Name, Is.EqualTo(newFolderName));
        }

        [Test]
        public void EmptyLocalFileIsCreatedAndChangedRemotely([Values(true, false)]bool contentChanges) {
            this.ContentChangesActive = contentChanges;
            string fileName = "file";
            string content = "content";
            var filePath = Path.Combine(this.localRootDir.FullName, fileName);
            var fileInfo = new FileInfo(filePath);
            using (fileInfo.Open(FileMode.CreateNew)) {
            }

            DateTime modificationDate = fileInfo.LastWriteTimeUtc;

            this.InitializeAndRunRepo();

            this.WaitForRemoteChanges();
            this.AddStartNextSyncEvent();
            this.repo.Run();
            this.WaitForRemoteChanges();
            this.AddStartNextSyncEvent();
            this.repo.Run();

            var children = this.remoteRootDir.GetChildren();
            Assert.That(children.TotalNumItems, Is.EqualTo(1));
            var child = children.First();
            Assert.That(child, Is.InstanceOf(typeof(IDocument)));
            var doc = child as IDocument;
            doc.SetContent(content, true, true);
            Assert.That(doc.ContentStreamLength, Is.EqualTo(content.Length), "ContentStream not set correctly");

            this.WaitForRemoteChanges();
            this.AddStartNextSyncEvent();
            this.repo.Run();

            doc.Refresh();
            Assert.That((this.localRootDir.GetFiles().First().LastWriteTimeUtc - (DateTime)doc.LastModificationDate).Seconds, Is.Not.GreaterThan(1));
            Assert.That(this.localRootDir.GetFiles().First().Length, Is.EqualTo(content.Length));
            Assert.That(this.repo.NumberOfChanges, Is.EqualTo(0));
        }

        [Test]
        public void LocalFileRenamedAndDeletedRemotely([Values(true, false)]bool contentChanges) {
            this.ContentChangesActive = contentChanges;
            string newName = "newtestfile.bin";
            string oldName = "testfile.bin";
            string content = "text";
            this.remoteRootDir.CreateDocument(oldName, content);
            this.InitializeAndRunRepo();

            this.localRootDir.GetFiles().First().MoveTo(Path.Combine(this.localRootDir.FullName, newName));
            this.remoteRootDir.Refresh();
            this.remoteRootDir.GetChildren().First().Delete(true);

            this.WaitForRemoteChanges();
            this.repo.SingleStepQueue.SwallowExceptions = true;
            this.AddStartNextSyncEvent();
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
            Assert.That(this.repo.NumberOfChanges, Is.EqualTo(0));
        }

        [Test]
        public void LocalFileMovedAndDeletedRemotely([Values(true, false)]bool contentChanges) {
            this.ContentChangesActive = contentChanges;
            string newName = "newtestfile.bin";
            string oldName = "testfile.bin";
            string content = "text";

            this.remoteRootDir.CreateFolder("folder").CreateDocument(oldName, content);

            this.InitializeAndRunRepo();

            this.localRootDir.GetDirectories().First().GetFiles().First().MoveTo(Path.Combine(this.localRootDir.FullName, newName));
            this.remoteRootDir.Refresh();
            ((this.remoteRootDir.GetChildren().First() as IFolder).GetChildren().First() as IDocument).Delete(true);

            this.WaitForRemoteChanges();
            this.repo.SingleStepQueue.SwallowExceptions = true;
            this.AddStartNextSyncEvent();
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

            Assert.That(this.repo.NumberOfChanges, Is.EqualTo(0));
        }

        [Test]
        public void LocalFilesMovedToEachOthersLocationInLocalFolderTree([Values(false)]bool contentChanges, [Values("a", "Z")]string folderName) {
            this.ContentChangesActive = contentChanges;
            string fileNameA = "testfile.bin";
            string fileNameB = "anotherFile.bin";
            string contentA = "text";
            string contentB = "another text";
            var sourceFolderA = this.remoteRootDir.CreateFolder(folderName).CreateFolder("folder").CreateFolder("B");
            sourceFolderA.CreateDocument(fileNameA, contentA);

            var sourceFolderB = this.remoteRootDir.CreateFolder("C").CreateFolder("Folder").CreateFolder("D");
            sourceFolderB.CreateDocument(fileNameB, contentB);

            this.InitializeAndRunRepo(swallowExceptions: false);
            this.repo.SingleStepQueue.DropAllLocalFileSystemEvents = true;
            var rootDirs = this.localRootDir.GetDirectories();
            var folderA = rootDirs.First().Name == folderName ? rootDirs.First() : rootDirs.Last();
            var folderC = rootDirs.First().Name == "C" ? rootDirs.First() : rootDirs.Last();
            var folderB = folderA.GetDirectories().First().GetDirectories().First();
            var folderD = folderC.GetDirectories().First().GetDirectories().First();
            var fileToBeMovedA = folderB.GetFiles().First();
            var fileToBeMovedB = folderD.GetFiles().First();
            fileToBeMovedA.MoveTo(Path.Combine(folderD.FullName, fileNameA));
            fileToBeMovedB.MoveTo(Path.Combine(folderB.FullName, fileNameB));
            this.AddStartNextSyncEvent();
            this.repo.Run();

            this.remoteRootDir.Refresh();
            var remoteRootDirs = this.remoteRootDir.GetChildren();
            var first = remoteRootDirs.First();
            var last = remoteRootDirs.Last();
            var remoteFolderC = first.Name == "C" ? first as IFolder : last as IFolder;
            Assert.That(remoteFolderC.Name, Is.EqualTo("C"));
            var remoteFolderFolder = remoteFolderC.GetChildren().First() as IFolder;
            var remoteFolderD = remoteFolderFolder.GetChildren().First() as IFolder;
            remoteFolderD.Refresh();
            var remoteFile = remoteFolderD.GetChildren().First() as IDocument;

            Assert.That(remoteFile.Name, Is.EqualTo(fileNameA));
            Assert.That(remoteFile.ContentStreamLength, Is.EqualTo(contentA.Length));
            folderB.Refresh();
            Assert.That(folderB.GetFiles().First().Name, Is.Not.EqualTo(fileNameA));
            folderD.Refresh();
            Assert.That(folderD.GetFileSystemInfos().Count(), Is.EqualTo(folderD.GetFiles().Count()));
            Assert.That(folderD.GetFiles().First().Name, Is.EqualTo(fileNameA));
            Assert.That(this.repo.NumberOfChanges, Is.EqualTo(0));
        }

        [Test]
        public void LocalFolderWithContentRenamedAndDeletedRemotely([Values(true, false)]bool contentChanges) {
            this.ContentChangesActive = contentChanges;
            string newName = "newtestfile.bin";
            string oldName = "testfile.bin";
            string content = "text";
            this.remoteRootDir.CreateFolder("folder").CreateFolder(oldName).CreateDocument("doc", content);
            this.InitializeAndRunRepo();

            this.localRootDir.GetDirectories().First().GetDirectories().First().MoveTo(Path.Combine(this.localRootDir.FullName, newName));
            this.remoteRootDir.Refresh();
            (this.remoteRootDir.GetChildren().First() as IFolder).DeleteTree(true, null, true);

            this.WaitForRemoteChanges();
            this.repo.SingleStepQueue.SwallowExceptions = true;
            this.AddStartNextSyncEvent();
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
            AssertThatEventCounterIsZero();
            AssertThatFolderStructureIsEqual();
        }

        [Test]
        public void OneFileIsCopiedAndTheCopyIsRemoved([Values(true, false)]bool contentChanges) {
            this.ContentChangesActive = contentChanges;
            FileSystemInfoFactory fsFactory = new FileSystemInfoFactory();
            var fileNames = new List<string>();
            string fileName = "file";
            string content = "content";
            this.remoteRootDir.CreateDocument(fileName + ".bin", content);
            this.InitializeAndRunRepo();

            var file = this.localRootDir.GetFiles().First();
            fileNames.Add(file.FullName);
            var fileInfo = fsFactory.CreateFileInfo(file.FullName);
            Guid uuid = (Guid)fileInfo.Uuid;
            var fileCopy = fsFactory.CreateFileInfo(Path.Combine(this.localRootDir.FullName, fileName + " - copy.bin"));
            file.CopyTo(fileCopy.FullName);
            fileCopy.Refresh();
            fileCopy.Uuid = uuid;
            fileCopy.Delete();
            Thread.Sleep(500);

            this.repo.SingleStepQueue.SwallowExceptions = true;
            this.AddStartNextSyncEvent();
            this.repo.Run();

            Assert.That(this.localRootDir.GetFiles().Length, Is.EqualTo(1));
            var child = this.localRootDir.GetFiles().First();
            Assert.That(child.Length, Is.EqualTo(content.Length));
            Assert.That(child.Name, Is.EqualTo(fileName + ".bin"));
        }

        [Test]
        public void OneLocalAndOneRemoteFileAreBothChangedToTheSameContent([Values(true, false)]bool contentChanges) {
            this.ContentChangesActive = contentChanges;
            string oldContent = "a";
            string newContent = "bbb";
            this.remoteRootDir.CreateDocument("fileName.bin", oldContent);
            this.InitializeAndRunRepo(swallowExceptions: true);

            this.remoteRootDir.Refresh();
            var doc = this.remoteRootDir.GetChildren().First() as IDocument;
            doc.SetContent(newContent);
            var file = this.localRootDir.GetFiles().First();
            using (var stream = file.Open(FileMode.Open, FileAccess.Write, FileShare.None)) {
                byte[] content = Encoding.UTF8.GetBytes(newContent);
                stream.Write(content, 0, content.Length);
            }

            Thread.Sleep(500);
            this.AddStartNextSyncEvent();
            this.repo.Run();

            this.remoteRootDir.Refresh();
            doc.Refresh();
            file.Refresh();
            Assert.That(this.remoteRootDir.GetChildren().Count(), Is.EqualTo(1));
            Assert.That(this.localRootDir.GetFiles().Count(), Is.EqualTo(1));
            Assert.That(file.Length, Is.EqualTo(newContent.Length));
            Assert.That(file.Length, Is.EqualTo(doc.ContentStreamLength));
            if (this.session.IsServerAbleToUpdateModificationDate()) {
                AssertThatDatesAreEqual(file.LastWriteTimeUtc, doc.LastModificationDate);
            }
        }

        [Test]
        public void OneRemoteFolderIsRenamedToLowerCase([Values(true, false)]bool contentChanges) {
            this.ContentChangesActive = contentChanges;
            string oldFolderName = "A";
            string newFolderName = oldFolderName.ToLower();
            var folder = this.remoteRootDir.CreateFolder(oldFolderName);

            this.InitializeAndRunRepo();

            folder.Refresh();
            folder.Rename(newFolderName);
            this.WaitForRemoteChanges();

            this.AddStartNextSyncEvent();
            this.repo.Run();

            AssertThatEventCounterIsZero();
            AssertThatFolderStructureIsEqual();
        }

        [Test]
        public void OneLocalFileIsRemovedAndChangedRemotely([Values(true, false)]bool contentChanges) {
            this.ContentChangesActive = contentChanges;
            string fileName = "file.bin";
            string oldContent = "old content";
            string newContent = "new content replaces old content";
            this.remoteRootDir.CreateDocument(fileName, oldContent);

            this.repo.Initialize();
            this.repo.SingleStepQueue.SwallowExceptions = true;
            this.AddStartNextSyncEvent();
            this.repo.Run();

            this.localRootDir.GetFiles().First().Delete();
            (this.remoteRootDir.GetChildren().First() as IDocument).SetContent(newContent);

            this.WaitUntilQueueIsNotEmpty();
            this.AddStartNextSyncEvent();
            this.repo.Run();

            Assert.That(this.localRootDir.GetFiles().First().Length, Is.EqualTo(newContent.Length));
        }

        [Test]
        public void OneAlreadySyncedFileIsMovedToNewlyCreatedFolder(
            [Values(true, false)]bool contentChanges,
            [Values(true, false)]bool withLocalFSEvents)
        {
            this.ContentChangesActive = contentChanges;
            string fileName = "file.bin";
            string content = "content";
            string folderName = "target";
            var remoteDoc = this.remoteRootDir.CreateDocument(fileName, content);
            this.repo.Initialize();
            this.repo.SingleStepQueue.SwallowExceptions = true;
            if (!withLocalFSEvents) {
                this.repo.SingleStepQueue.EventManager.AddEventHandler(new GenericSyncEventHandler<IFSEvent>(int.MaxValue, (e) => { return true; }, "FilterOfAllFSEvents"));
            }

            this.AddStartNextSyncEvent();
            this.repo.Run();

            var testDir = this.localRootDir.CreateSubdirectory(folderName);
            this.localRootDir.GetFiles().First().MoveTo(Path.Combine(testDir.FullName, fileName));
            if (withLocalFSEvents) {
                this.WaitUntilQueueIsNotEmpty();
            }

            this.AddStartNextSyncEvent();
            this.repo.Run();
            this.AddStartNextSyncEvent(forceCrawl: true);
            this.repo.Run();
            this.remoteRootDir.Refresh();
            var children = this.remoteRootDir.GetChildren();
            Assert.That(children.TotalNumItems, Is.EqualTo(1));
            foreach (var child in children) {
                Assert.That(child.Name, Is.EqualTo(folderName));
                var subChildren = (child as IFolder).GetChildren();
                Assert.That(subChildren.TotalNumItems, Is.EqualTo(1));
                foreach (var subChild in subChildren) {
                    Assert.That(subChild is IDocument);
                    Assert.That(subChild.Id, Is.EqualTo(remoteDoc.Id));
                }
            }

            remoteDoc.Refresh();
            Assert.That(remoteDoc.Parents.First().Name, Is.EqualTo(folderName));
            AssertThatFolderStructureIsEqual();
        }

        [Test, Ignore("Ignore this until the server does not change the changetoken on move operation")]
        public void LocalFileMovedAndRemoteFileMovedToOtherFolder() {
            string fileName = "file.bin";
            string content = "content";
            var doc = this.remoteRootDir.CreateDocument(fileName, content);
            var a = this.remoteRootDir.CreateFolder("A");
            var b = this.remoteRootDir.CreateFolder("B");

            this.InitializeAndRunRepo();

            this.localRootDir.GetFiles().First().MoveTo(Path.Combine(this.localRootDir.FullName, a.Name, fileName));
            doc.Refresh();
            this.remoteRootDir.Refresh();
            b.Refresh();
            doc.Move(this.remoteRootDir, b);

            this.repo.SingleStepQueue.SwallowExceptions = true;
            this.WaitUntilQueueIsNotEmpty(this.repo.SingleStepQueue);
            this.AddStartNextSyncEvent();
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
        [Test, Category("Erratic")]
        public void CyclicRenaming() {
            string folderName1 = "A";
            string folderName2 = "B";
            string subFolderName = "sub";
            var folder1 = this.localRootDir.CreateSubdirectory(folderName1);
            var folder2 = this.localRootDir.CreateSubdirectory(folderName2);
            folder1.CreateSubdirectory(subFolderName);

            this.InitializeAndRunRepo();

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

            this.AddStartNextSyncEvent();
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

        [Test, Ignore("https://mantis.dataspace.cc/view.php?id=4285")]
        public void ExecutingTheSameFolderMoveTwiceThrowsCmisException() {
            var source = this.remoteRootDir.CreateFolder("source");
            var target = this.remoteRootDir.CreateFolder("target");
            var folder = source.CreateFolder("folder");
            var anotherFolderInstance = this.session.GetObject(folder) as IFolder;

            folder.Move(source, target);

            Assert.Throws<CmisConstraintException>(() => anotherFolderInstance.Move(source, target));
        }

        [Test]
        public void DoNotTransferDataIfLocalAndRemoteFilesAreEqual([Values(true, false)]bool contentChanges) {
            this.EnsureThatContentHashesAreSupportedByServerTypeSystem();
            this.ContentChangesActive = contentChanges;
            this.InitializeAndRunRepo();
            this.repo.SingleStepQueue.SwallowExceptions = true;

            string content = "a";
            string fileName = "file.bin";
            var remoteFile = this.remoteRootDir.CreateDocument(fileName, content);
            remoteFile.VerifyThatIfTimeoutIsExceededContentHashIsEqualTo(content);

            var file = new FileInfo(Path.Combine(this.localRootDir.FullName, fileName));
            using (StreamWriter sw = file.CreateText()) {
                sw.Write(content);
            }

            this.transmissionManager.ActiveTransmissions.CollectionChanged += (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) => {
                this.repo.SingleStepQueue.SwallowExceptions = false;
                Assert.Fail("There should be no transmission, but a new transmission object is added");
            };

            this.AddStartNextSyncEvent();
            this.repo.Run();

            Assert.That(this.localRootDir.GetFiles()[0].Length, Is.EqualTo(content.Length));
            Assert.That((this.remoteRootDir.GetChildren().First() as IDocument).ContentStreamLength, Is.EqualTo(content.Length));
            Assert.That(this.repo.NumberOfChanges, Is.EqualTo(0));
        }

        [Ignore("https://mantis.dataspace.cc/view.php?id=4285")]
        [Test]
        public void ExecutingTheSameFolderMoveToDifferentTargetsThrowsCmisException() {
            var source = this.remoteRootDir.CreateFolder("source");
            var target1 = this.remoteRootDir.CreateFolder("target1");
            var target2 = this.remoteRootDir.CreateFolder("target2");
            var folder = source.CreateFolder("folder");
            var anotherFolderInstance = this.session.GetObject(folder) as IFolder;

            folder.Move(source, target1);

            Assert.Throws<CmisConstraintException>(() => anotherFolderInstance.Move(source, target2));
        }
    }
}