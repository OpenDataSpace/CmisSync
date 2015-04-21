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

    // Default timeout per test is 2 minutes but test fails if it need more then 1 minute
    [TestFixture, Timeout(180000), TestName("FullRepo")]
    public class FullRepoTests : BaseFullRepoTest {
        [Test, Category("Slow"), MaxTime(180000)]
        public void OneLocalFolderCreated() {
            this.localRootDir.CreateSubdirectory("Cat");

            this.InitializeAndRunRepo();
            var children = this.remoteRootDir.GetChildren();
            Assert.AreEqual(children.TotalNumItems, 1);
        }

        [Test, Category("Slow"), MaxTime(180000), Category("Erratic")]
        public void OneLocalFolderRemoved() {
            this.localRootDir.CreateSubdirectory("Cat");

            this.InitializeAndRunRepo();

            this.localRootDir.GetDirectories().First().Delete();

            this.WaitUntilQueueIsNotEmpty(this.repo.SingleStepQueue, 15000);

            this.repo.Run();

            Assert.That(this.remoteRootDir.GetChildren(), Is.Empty);
        }

        [Test, Category("Slow"), MaxTime(180000)]
        public void OneRemoteFolderCreated() {
            this.remoteRootDir.CreateFolder("Cat");

            this.InitializeAndRunRepo();

            Assert.That(this.localRootDir.GetDirectories().Length, Is.EqualTo(1));
            Assert.That(this.localRootDir.GetDirectories()[0].Name, Is.EqualTo("Cat"));
        }

        [Test, Category("Slow"), MaxTime(180000)]
        public void OneRemoteFolderIsDeleted() {
            this.remoteRootDir.CreateFolder("Cat");

            this.InitializeAndRunRepo();

            (this.remoteRootDir.GetChildren().First() as IFolder).DeleteTree(true, null, true);

            this.AddStartNextSyncEvent(forceCrawl: true);
            this.repo.Run();

            Assert.That(this.localRootDir.GetDirectories().Length, Is.EqualTo(0));
            Assert.That(this.remoteRootDir.GetChildren().Count(), Is.EqualTo(0));
        }

        [Test, Category("Slow"), MaxTime(180000), Category("Conflict")]
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

        [Test, Category("Slow"), MaxTime(180000)]
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
        }

        [Test, Category("Slow"), MaxTime(180000)]
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
        }

        [Test, Category("Slow"), MaxTime(180000)]
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
        }

        [Test, Category("Slow"), MaxTime(180000)]
        public void OneLocalFileCreated([Values(false)]bool contentChanges) {
            this.ContentChangesActive = contentChanges;
            string fileName = "file";
            string content = "content";
            var filePath = Path.Combine(this.localRootDir.FullName, fileName);
            var fileInfo = new FileInfo(filePath);
            using (StreamWriter sw = fileInfo.CreateText()) {
                sw.WriteLine(content);
            }

            DateTime modificationDate = fileInfo.LastWriteTimeUtc;

            this.InitializeAndRunRepo();
            Thread.Sleep(5000);
            this.remoteRootDir.Refresh();
            var children = this.remoteRootDir.GetChildren();
            Assert.That(children.TotalNumItems, Is.EqualTo(1));
            var child = children.First();
            Assert.That(child, Is.InstanceOf(typeof(IDocument)));
            var doc = child as IDocument;
            Assert.That(doc.ContentStreamLength, Is.GreaterThan(0), "ContentStream not set");
            doc.AssertThatIfContentHashExistsItIsEqualTo(content);
            Assert.That(this.localRootDir.GetFiles().First().LastWriteTimeUtc, Is.EqualTo(modificationDate));
        }

        [Test, Category("Slow"), MaxTime(180000)]
        public void OneLocalFileCreatedAndModificationDateIsSynced() {
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

            this.InitializeAndRunRepo();
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

        [Test, Category("Slow"), MaxTime(180000)]
        public void OneLocalFileRenamed() {
            string fileName = "file";
            string newFileName = "renamedFile";
            string content = "content";
            var filePath = Path.Combine(this.localRootDir.FullName, fileName);
            var fileInfo = new FileInfo(filePath);
            using (StreamWriter sw = fileInfo.CreateText()) {
                sw.WriteLine(content);
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

        [Test, Category("Slow"), MaxTime(180000)]
        public void OneLocalFileRenamedAndMoved() {
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

        [Test, Category("Slow"), MaxTime(180000)]
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

        [Test, Category("Slow"), MaxTime(180000)]
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

        [Test, Category("Slow"), MaxTime(180000)]
        public void OneEmptyRemoteFileCreated([Values(true, false)]bool contentChanges) {
            this.ContentChangesActive = contentChanges;
            this.InitializeAndRunRepo();
            string fileName = "file";
            var doc = this.remoteRootDir.CreateDocument(fileName, null);

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
        [Test, Category("Slow"), Timeout(600000)]
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

        [Test, Category("Slow"), MaxTime(180000)]
        public void OneRemoteFileContentIsDeleted([Values(true, false)]bool contentChanges) {
            this.ContentChangesActive = contentChanges;

            string fileName = "file";
            string content = "content";
            var doc = this.remoteRootDir.CreateDocument(fileName, content);

            this.InitializeAndRunRepo();

            doc.Refresh();
            doc.AssertThatIfContentHashExistsItIsEqualTo(content);
            string oldChangeToken = doc.ChangeToken;
            doc.DeleteContentStream(true);
            string newChangeToken = doc.ChangeToken;
            Assert.That(oldChangeToken, Is.Not.EqualTo(newChangeToken));
            Assert.That(doc.ContentStreamLength, Is.Not.EqualTo(content.Length));
            doc.AssertThatIfContentHashExistsItIsEqualTo(string.Empty);
            this.WaitForRemoteChanges();
            this.AddStartNextSyncEvent();
            this.repo.Run();

            var children = this.localRootDir.GetFiles();
            Assert.That(children.Length, Is.EqualTo(1));
            var child = children.First();
            Assert.That(child, Is.InstanceOf(typeof(FileInfo)));
            Assert.That(child.Length, Is.EqualTo(0), child.ToString());
        }

        [Test, Category("Slow"), MaxTime(180000)]
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

            this.WaitForRemoteChanges();

            this.AddStartNextSyncEvent();
            this.repo.Run();

            var file = this.localRootDir.GetFiles().First();
            Assert.That(file, Is.InstanceOf(typeof(FileInfo)));
            Assert.That(file.Length, Is.EqualTo(content.Length));
        }

        [Test, Category("Slow"), MaxTime(180000)]
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

        [Test, Category("Slow"), Category("Conflict"), MaxTime(180000)]
        public void OneLocalFileAndOneRemoteFileIsCreatedAndOneConflictFileIsCreated([Values(true, false)]bool contentChanges) {
            this.ContentChangesActive = contentChanges;
            string fileName = "fileConflictTest.bin";
            string remoteContent = "remotecontent";
            string localContent = "local";

            this.remoteRootDir.CreateDocument(fileName, remoteContent);
            var localDoc = Path.Combine(this.localRootDir.FullName, fileName);
            var fileInfo = new FileInfo(localDoc);
            using (StreamWriter sw = fileInfo.CreateText()) {
                sw.WriteLine(localContent);
            }

            Thread.Sleep(200);

            this.InitializeAndRunRepo(swallowExceptions: true);

            this.remoteRootDir.Refresh();
            Assert.That(this.localRootDir.GetFiles().Length, Is.EqualTo(2));
            Assert.That(new FileInfo(localDoc).Length, Is.EqualTo(remoteContent.Length));
            Assert.That(this.remoteRootDir.GetChildren().Count(), Is.EqualTo(2));
        }

        [Test, Category("Slow"), Category("Conflict"), MaxTime(180000)]
        public void OneLocalFileIsChangedAndTheRemoteFileIsRemoved() {
            string fileName = "fileConflictTest.bin";
            string changedLocalContent = "changedContent";
            string localContent = "local";
            var localPath = Path.Combine(this.localRootDir.FullName, fileName);
            var fileInfo = new FileInfo(localPath);

            using (StreamWriter sw = fileInfo.CreateText()) {
                sw.WriteLine(localContent);
            }

            this.InitializeAndRunRepo(swallowExceptions: true);

            this.remoteRootDir.GetChildren().First().Delete(true);
            Assert.That(this.remoteRootDir.GetChildren().Count(), Is.EqualTo(0));
            using (StreamWriter sw = fileInfo.CreateText()) {
                sw.WriteLine(changedLocalContent);
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

        [Test, Category("Slow"), Category("Conflict"), MaxTime(180000)]
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

        [Test, Category("Slow"), MaxTime(180000)]
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
            Assert.That(this.repo.NumberOfChanges, Is.EqualTo(0));
        }

        [Test, Category("Slow"), MaxTime(180000)]
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
        [Test, Category("Slow"), Timeout(1800000), Ignore("Just for benchmarks")]
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

        [Test, Category("Slow"), MaxTime(180000)]
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

        [Test, Category("Slow"), MaxTime(180000)]
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

        [Test, Category("Slow"), MaxTime(180000)]
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

        [Test, Category("Slow"), MaxTime(180000)]
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

        [Test, Category("Slow"), MaxTime(180000)]
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

        [Test, Category("Slow"), MaxTime(180000)]
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

        [Test, Category("Slow"), MaxTime(180000)]
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
            Assert.That(this.repo.NumberOfChanges, Is.EqualTo(0));
        }

        [Test, Category("Slow"), Timeout(360000)]
        public void OneFileIsCopiedAFewTimes([Values(true, false)]bool contentChanges, [Values(1,2,5,10)]int times) {
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
            for (int i = 0; i < times; i++) {
                var fileCopy = fsFactory.CreateFileInfo(Path.Combine(this.localRootDir.FullName, string.Format("{0}{1}.bin", fileName, i)));
                file.CopyTo(fileCopy.FullName);
                Thread.Sleep(50);
                fileCopy.Refresh();
                fileCopy.Uuid = uuid;
                fileNames.Add(fileCopy.FullName);
            }

            Thread.Sleep(500);

            this.AddStartNextSyncEvent(forceCrawl: true);
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

            Assert.That(this.repo.NumberOfChanges, Is.EqualTo(0));
        }

        [Test, Category("Slow"), MaxTime(180000)]
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

        [Test, Category("Slow"), MaxTime(180000)]
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
                this.AssertThatDatesAreEqual(file.LastWriteTimeUtc, doc.LastModificationDate);
            }
        }

        [Test, Category("Slow"), MaxTime(180000)]
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

            folder.Refresh();
            Assert.That(this.localRootDir.GetDirectories().Count(), Is.EqualTo(1));
            Assert.That(this.localRootDir.GetDirectories().First().Name, Is.EqualTo(newFolderName).Or.EqualTo(oldFolderName));
            Assert.That(this.remoteRootDir.GetChildren().Count(), Is.EqualTo(1));
            Assert.That(this.remoteRootDir.GetChildren().First().Name, Is.EqualTo(newFolderName));
        }

        [Test, Category("Slow"), MaxTime(180000)]
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

        [Test, Category("Slow"), MaxTime(180000)]
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

        [Test, Category("Slow"), MaxTime(180000), Ignore("Ignore this until the server does not change the changetoken on move operation")]
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
        [Test, Category("Slow"), MaxTime(180000), Category("Erratic")]
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

        [Test, Category("Slow"), MaxTime(180000)]
        public void ExecutingTheSameFolderMoveTwiceThrowsCmisException() {
            var source = this.remoteRootDir.CreateFolder("source");
            var target = this.remoteRootDir.CreateFolder("target");
            var folder = source.CreateFolder("folder");
            var anotherFolderInstance = this.session.GetObject(folder) as IFolder;

            folder.Move(source, target);

            Assert.Throws<CmisConstraintException>(() => anotherFolderInstance.Move(source, target));
        }

        [Test, Category("Slow"), MaxTime(180000)]
        public void DoNotTransferDataIfLocalAndRemoteFilesAreEqual([Values(true, false)]bool contentChanges) {
            this.ContentChangesActive = contentChanges;
            this.InitializeAndRunRepo();
            this.repo.SingleStepQueue.SwallowExceptions = true;

            string content = "a";
            string fileName = "file.bin";
            var remoteFile = this.remoteRootDir.CreateDocument(fileName, content);
            if (remoteFile.ContentStreamHash() == null) {
                Assert.Ignore("Server does not support hash of content stream");
            }

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

        [Ignore("Mantis issue 4285")]
        [Test, Category("Slow"), MaxTime(180000)]
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