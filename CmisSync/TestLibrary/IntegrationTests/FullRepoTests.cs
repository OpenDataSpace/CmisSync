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

        [Test, Category("Conflict")]
        public void OneRemoteFolderIsDeletedAndOneUnsyncedFileExistsInTheCorrespondingLocalFolder() {
            string folderName = "Cat";
            string fileName = "localFile.bin";
            var folder = this.remoteRootDir.CreateFolder(folderName);
            folder.CreateDocument("foo.bin", "bar");
            InitializeAndRunRepo(swallowExceptions: true);

            using (var file = File.Open(Path.Combine(this.localRootDir.GetDirectories().First().FullName, fileName), FileMode.Create)) {
            }

            this.remoteRootDir.Refresh();
            (this.remoteRootDir.GetChildren().First() as IFolder).DeleteTree(false, null, true);
            Assert.That(this.remoteRootDir.GetChildren().Count(), Is.EqualTo(0));

            WaitUntilQueueIsNotEmpty();

            this.repo.Run();

            Assert.That(this.localRootDir.GetDirectories().First().Name, Is.EqualTo(folderName));
            Assert.That(this.localRootDir.GetDirectories().First().GetFiles().First().Name, Is.EqualTo(fileName));
            Assert.That(this.localRootDir.GetDirectories().First().GetFiles().Count(), Is.EqualTo(1));
            Assert.That(this.remoteRootDir.GetChildren().First().Name, Is.EqualTo(folderName));
            Assert.That((this.remoteRootDir.GetChildren().First() as IFolder).GetChildren().First().Name, Is.EqualTo(fileName));
            Assert.That((this.remoteRootDir.GetChildren().First() as IFolder).GetChildren().Count(), Is.EqualTo(1));
        }

        [Test]
        public void RemoteCreatedFileIsDeletedLocally() {
            string fileName = "file.bin";
            string content = "cat";
            this.remoteRootDir.CreateDocument(fileName, content);

            InitializeAndRunRepo();

            this.localRootDir.GetFiles().First().Delete();

            WaitUntilQueueIsNotEmpty();

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

            InitializeAndRunRepo(swallowExceptions: true);

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

            InitializeAndRunRepo(swallowExceptions: true);

            this.remoteRootDir.GetChildren().First().Delete(true);
            Assert.That(this.remoteRootDir.GetChildren().Count(), Is.EqualTo(0));
            using (StreamWriter sw = fileInfo.CreateText()) {
                sw.Write(changedLocalContent);
            }

            fileInfo.Refresh();
            long expectedLength = fileInfo.Length;

            WaitUntilQueueIsNotEmpty();

            AddStartNextSyncEventAndRun();

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

            InitializeAndRunRepo(swallowExceptions: true);

            this.localRootDir.GetFiles().First().MoveTo(Path.Combine(this.localRootDir.FullName, localName));
            this.remoteRootDir.GetChildren().First().Rename(remoteName);

            WaitUntilQueueIsNotEmpty();

            AddStartNextSyncEventAndRun();

            Assert.That(this.localRootDir.GetFiles().First().Name, Is.EqualTo(localName));
            Assert.That(this.remoteRootDir.GetChildren().First().Name, Is.EqualTo(remoteName));

            // In Conflict situation
            // Resolve the conflict by renaming the remote file to the local name
            this.remoteRootDir.GetChildren().First().Rename(localName);

            WaitForRemoteChanges();
            AddStartNextSyncEventAndRun();

            this.remoteRootDir.Refresh();

            // Conflict is solved
            Assert.That(this.localRootDir.GetFiles().First().Name, Is.EqualTo(localName));
            Assert.That(this.remoteRootDir.GetChildren().First().Name, Is.EqualTo(localName));

            this.remoteRootDir.GetChildren().First().Rename(remoteName);

            WaitForRemoteChanges();
            AddStartNextSyncEventAndRun();

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
            InitializeAndRunRepo(swallowExceptions: true);

            folder.Refresh();
            folder.Move(source, target);

            WaitForRemoteChanges(sleepDuration: 15000);
            AddStartNextSyncEventAndRun();

            var localSource = this.localRootDir.GetDirectories(oldParentName).First();
            var localTarget = this.localRootDir.GetDirectories(newParentName).First();
            Assert.That(localSource.GetFileSystemInfos(), Is.Empty);
            Assert.That(localTarget.GetFileSystemInfos().Count(), Is.EqualTo(1));
            var localFolder = localTarget.GetDirectories().First();
            folder.Refresh();
            Assert.That(localFolder.Name, Is.EqualTo(folder.Name));
            Assert.That(folder.Name, Is.EqualTo(oldName));
            repo.Run();
            AssertThatEventCounterIsZero();
            AssertThatFolderStructureIsEqual();
        }

        [Test]
        public void OneRemoteFileIsChangedAndRenamed([Values(true, false)]bool contentChanges) {
            this.ContentChangesActive = contentChanges;
            string fileName = "file.bin";
            string newFileName = "file_1.bin";
            string content = "cat";
            var document = this.remoteRootDir.CreateDocument(fileName, content);

            InitializeAndRunRepo();
            this.repo.SingleStepQueue.SwallowExceptions = true;

            document.Refresh();
            document.SetContent(content + content, true, true);
            long length = (long)document.ContentStreamLength;
            document.Rename(newFileName);

            WaitForRemoteChanges(sleepDuration: 15000);
            AddStartNextSyncEventAndRun();

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

            InitializeAndRunRepo(swallowExceptions: true);

            // Wait for all fs change events
            Thread.Sleep(500);

            // Stabilize sync process to process all delayed fs events
            this.repo.Run();

            folder.Refresh();
            folder.Rename(newFolderName);
            this.localRootDir.GetDirectories().First().MoveTo(Path.Combine(this.localRootDir.FullName, newFolderName));

            AddStartNextSyncEventAndRun(forceCrawl: true);

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

            InitializeAndRunRepo();

            var children = this.remoteRootDir.GetChildren();
            Assert.That(children.TotalNumItems, Is.EqualTo(1));
            var child = children.First();
            Assert.That(child, Is.InstanceOf(typeof(IDocument)));
            var doc = child as IDocument;
            doc.SetContent(content, true, true);
            Assert.That(doc.ContentStreamLength, Is.EqualTo(content.Length), "ContentStream not set correctly");

            WaitForRemoteChanges(sleepDuration: 15000);
            AddStartNextSyncEventAndRun();

            doc.Refresh();
            Assert.That((this.localRootDir.GetFiles().First().LastWriteTimeUtc - (DateTime)doc.LastModificationDate).Seconds, Is.Not.GreaterThan(1));
            Assert.That(this.localRootDir.GetFiles().First().Length, Is.EqualTo(content.Length));
            AssertThatEventCounterIsZero();
        }

        [Test]
        public void LocalFileRenamedAndDeletedRemotely([Values(true, false)]bool contentChanges) {
            this.ContentChangesActive = contentChanges;
            string newName = "newtestfile.bin";
            string oldName = "testfile.bin";
            string content = "text";
            this.remoteRootDir.CreateDocument(oldName, content);
            InitializeAndRunRepo();

            this.localRootDir.GetFiles().First().MoveTo(Path.Combine(this.localRootDir.FullName, newName));
            this.remoteRootDir.Refresh();
            this.remoteRootDir.GetChildren().First().Delete(true);

            WaitForRemoteChanges();
            this.repo.SingleStepQueue.SwallowExceptions = true;
            AddStartNextSyncEventAndRun();

            Assert.That(this.localRootDir.GetDirectories(), Is.Empty);
            Assert.That(this.localRootDir.GetFiles().Length, Is.EqualTo(1));
            Assert.That(this.localRootDir.GetFiles().First().Length, Is.EqualTo(content.Length));
            Assert.That(this.localRootDir.GetFiles().First().Name, Is.EqualTo(newName));
            this.remoteRootDir.Refresh();
            Assert.That(this.remoteRootDir.GetChildren().Count(), Is.EqualTo(1));
            var doc = this.remoteRootDir.GetChildren().First() as IDocument;
            Assert.That(doc.ContentStreamLength, Is.EqualTo(content.Length));
            Assert.That(doc.Name, Is.EqualTo(newName));
            AssertThatEventCounterIsZero();
        }

        [Test]
        public void LocalFileMovedAndDeletedRemotely([Values(true, false)]bool contentChanges) {
            this.ContentChangesActive = contentChanges;
            string newName = "newtestfile.bin";
            string oldName = "testfile.bin";
            string content = "text";

            this.remoteRootDir.CreateFolder("folder").CreateDocument(oldName, content);

            InitializeAndRunRepo();

            this.localRootDir.GetDirectories().First().GetFiles().First().MoveTo(Path.Combine(this.localRootDir.FullName, newName));
            this.remoteRootDir.Refresh();
            ((this.remoteRootDir.GetChildren().First() as IFolder).GetChildren().First() as IDocument).Delete(true);

            WaitForRemoteChanges();
            this.repo.SingleStepQueue.SwallowExceptions = true;
            AddStartNextSyncEventAndRun();

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

            AssertThatEventCounterIsZero();
        }

        [Test]
        public void LocalFolderWithContentRenamedAndDeletedRemotely([Values(true, false)]bool contentChanges) {
            this.ContentChangesActive = contentChanges;
            string newName = "newtestfile.bin";
            string oldName = "testfile.bin";
            string content = "text";
            this.remoteRootDir.CreateFolder("folder").CreateFolder(oldName).CreateDocument("doc", content);
            InitializeAndRunRepo();

            this.localRootDir.GetDirectories().First().GetDirectories().First().MoveTo(Path.Combine(this.localRootDir.FullName, newName));
            this.remoteRootDir.Refresh();
            (this.remoteRootDir.GetChildren().First() as IFolder).DeleteTree(true, null, true);

            WaitForRemoteChanges();
            this.repo.SingleStepQueue.SwallowExceptions = true;
            AddStartNextSyncEventAndRun();

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
            InitializeAndRunRepo();

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
            AddStartNextSyncEventAndRun();

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
            InitializeAndRunRepo(swallowExceptions: true);

            this.remoteRootDir.Refresh();
            var doc = this.remoteRootDir.GetChildren().First() as IDocument;
            doc.SetContent(newContent);
            var file = this.localRootDir.GetFiles().First();
            using (var stream = file.Open(FileMode.Open, FileAccess.Write, FileShare.None)) {
                byte[] content = Encoding.UTF8.GetBytes(newContent);
                stream.Write(content, 0, content.Length);
            }

            Thread.Sleep(500);
            AddStartNextSyncEventAndRun();

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
        public void OneLocalFileIsRemovedAndChangedRemotely([Values(true, false)]bool contentChanges) {
            this.ContentChangesActive = contentChanges;
            string fileName = "file.bin";
            string oldContent = "old content";
            string newContent = "new content replaces old content";
            this.remoteRootDir.CreateDocument(fileName, oldContent);

            InitializeAndRunRepo(swallowExceptions: true);

            this.localRootDir.GetFiles().First().Delete();
            (this.remoteRootDir.GetChildren().First() as IDocument).SetContent(newContent);

            WaitUntilQueueIsNotEmpty();
            AddStartNextSyncEventAndRun();

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

            AddStartNextSyncEventAndRun();

            var testDir = this.localRootDir.CreateSubdirectory(folderName);
            this.localRootDir.GetFiles().First().MoveTo(Path.Combine(testDir.FullName, fileName));
            if (withLocalFSEvents) {
                WaitUntilQueueIsNotEmpty();
            }

            AddStartNextSyncEventAndRun();
            AddStartNextSyncEventAndRun(forceCrawl: true);

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

            InitializeAndRunRepo();

            this.localRootDir.GetFiles().First().MoveTo(Path.Combine(this.localRootDir.FullName, a.Name, fileName));
            doc.Refresh();
            this.remoteRootDir.Refresh();
            b.Refresh();
            doc.Move(this.remoteRootDir, b);

            this.repo.SingleStepQueue.SwallowExceptions = true;
            WaitUntilQueueIsNotEmpty();
            AddStartNextSyncEventAndRun();

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

            InitializeAndRunRepo();

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
            WaitUntilQueueIsNotEmpty();

            this.repo.SingleStepQueue.SwallowExceptions = true;
            AddStartNextSyncEventAndRun();

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
            InitializeAndRunRepo(swallowExceptions: true);

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

            AddStartNextSyncEventAndRun();

            Assert.That(this.localRootDir.GetFiles()[0].Length, Is.EqualTo(content.Length));
            Assert.That((this.remoteRootDir.GetChildren().First() as IDocument).ContentStreamLength, Is.EqualTo(content.Length));
            AssertThatEventCounterIsZero();
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