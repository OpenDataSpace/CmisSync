//-----------------------------------------------------------------------
// <copyright file="UpdateLocalFileIT.cs" company="GRAU DATA AG">
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
﻿
namespace TestLibrary.IntegrationTests.SyncScenarioITs {
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;

    using CmisSync.Lib.Cmis.ConvenienceExtenders;

    using DotCMIS.Client;

    using NUnit.Framework;

    [TestFixture, Category("Slow"), TestName("UpdateLocalFile"), Timeout(180000)]
    public class UpdateLocalFileIT : AbstractBaseSyncScenarioIT {
        private readonly string newFileName = "renamedFile.bin";

        [Test]
        public void OneLocalFileRenamed() {
            var filePath = Path.Combine(this.localRootDir.FullName, defaultFileName);
            var fileInfo = new FileInfo(filePath);
            using (StreamWriter sw = fileInfo.CreateText()) {
                sw.Write(defaultContent);
            }

            this.InitializeAndRunRepo();

            fileInfo.MoveTo(Path.Combine(this.localRootDir.FullName, newFileName));
            DateTime modificationDate = fileInfo.LastWriteTimeUtc;

            this.WaitUntilQueueIsNotEmpty();

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
            var filePath = Path.Combine(this.localRootDir.FullName, defaultFileName);
            var fileInfo = new FileInfo(filePath);
            using (StreamWriter sw = fileInfo.CreateText()) {
                sw.Write(defaultContent);
            }

            this.repo.SingleStepQueue.SwallowExceptions = true;

            this.InitializeAndRunRepo();
            new DirectoryInfo(Path.Combine(this.localRootDir.FullName, defaultFolderName)).Create();
            fileInfo.MoveTo(Path.Combine(this.localRootDir.FullName, defaultFolderName, newFileName));
            DateTime modificationDate = fileInfo.LastWriteTimeUtc;

            this.WaitUntilQueueIsNotEmpty();

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
        public void OneLocalFileIsChangedAndRenamed([Values(true, false)]bool contentChanges) {
            this.ContentChangesActive = contentChanges;
            this.remoteRootDir.CreateDocument(defaultFileName, defaultContent);
            Thread.Sleep(100);
            this.InitializeAndRunRepo(swallowExceptions: true);

            var file = this.localRootDir.GetFiles().First();
            using (var stream = file.AppendText()) {
                stream.Write(defaultContent);
            }

            long length = Encoding.UTF8.GetBytes(defaultContent).Length * 2;

            file.MoveTo(Path.Combine(this.localRootDir.FullName, newFileName));
            file.Refresh();

            this.WaitUntilQueueIsNotEmpty();
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
        public void LocalFilesMovedToEachOthersLocationInLocalFolderTree([Values(false)]bool contentChanges, [Values("a", "Z")]string folderName) {
            this.ContentChangesActive = contentChanges;
            string fileNameA = defaultFileName;
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
    }
}