//-----------------------------------------------------------------------
// <copyright file="CreateLocalFileIT.cs" company="GRAU DATA AG">
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
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;

    using CmisSync.Lib.Cmis.ConvenienceExtenders;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;

    using NUnit.Framework;

    [TestFixture, Category("Slow"), TestName("CreateLocalFile"), Timeout(180000)]
    public class CreateLocalFileIT : AbstractBaseSyncScenarioIT {

        [Test]
        public void OneLocalFileCreated([Values(false)]bool contentChanges) {
            this.ContentChangesActive = contentChanges;
            var filePath = Path.Combine(this.localRootDir.FullName, defaultFileName);
            var fileInfo = new FileInfo(filePath);
            using (StreamWriter sw = fileInfo.CreateText()) {
                sw.Write(defaultContent);
            }

            fileInfo.Refresh();
            Assert.That(fileInfo.Length, Is.EqualTo(defaultContent.Length));
            DateTime modificationDate = fileInfo.LastWriteTimeUtc;

            this.InitializeAndRunRepo();
            this.remoteRootDir.Refresh();
            var children = this.remoteRootDir.GetChildren();
            Assert.That(children.TotalNumItems, Is.EqualTo(1));
            var child = children.First();
            Assert.That(child, Is.InstanceOf(typeof(IDocument)));
            var doc = child as IDocument;
            Assert.That(doc.ContentStreamLength, Is.EqualTo(defaultContent.Length), "Remote content stream has wrong length");
            this.AssertThatContentHashIsEqualToExceptedIfSupported(doc, defaultContent);
            Assert.That(this.localRootDir.GetFiles().First().LastWriteTimeUtc, Is.EqualTo(modificationDate));
            AssertThatEventCounterIsZero();
            AssertThatFolderStructureIsEqual();
        }

        [Test]
        public void TwoLocalFilesCreatedWithCommonSubnamePart([Values(false)]bool contentChanges) {
            this.ContentChangesActive = contentChanges;
            string fileName1 = "gpio.h";
            string fileName2 = "io.h";
            var filePath = Path.Combine(this.localRootDir.FullName, fileName1);
            var fileInfo = new FileInfo(filePath);
            using (StreamWriter sw = fileInfo.CreateText()) {
                sw.Write(defaultContent);
            }

            var filePath2 = Path.Combine(this.localRootDir.FullName, fileName2);
            var fileInfo2 = new FileInfo(filePath2);
            using (StreamWriter sw = fileInfo2.CreateText()) {
                sw.Write(defaultContent);
            }

            this.InitializeAndRunRepo();
            this.remoteRootDir.Refresh();
            var children = this.remoteRootDir.GetChildren();
            Assert.That(children.TotalNumItems, Is.EqualTo(2));
            AssertThatEventCounterIsZero();
            AssertThatFolderStructureIsEqual();
        }

        [Test]
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

            AssertThatFolderStructureIsEqual();
        }

        [Test]
        public void CreateFileWithOldModificationDate() {
            this.InitializeAndRunRepo();
            var file = new FileInfo(Path.Combine(this.localRootDir.FullName, "oldFile.bin"));
            using (var stream = file.CreateText()) {
                stream.WriteLine("text");
            };
            var oldDate = DateTime.UtcNow - TimeSpan.FromHours(2);
            file.LastWriteTimeUtc = oldDate;
            this.repo.Run();
            this.remoteRootDir.Refresh();
            var remoteFile = this.remoteRootDir.GetChildren().First() as IDocument;
            Assert.That(remoteFile.LastModificationDate, Is.EqualTo(oldDate).Within(2).Seconds);

            this.AddStartNextSyncEvent();

            using (var stream = file.AppendText()) {
                stream.WriteLine("blubb");
            };

            oldDate = DateTime.UtcNow - TimeSpan.FromHours(1);
            file.LastWriteTimeUtc = oldDate;
            this.repo.Run();
            remoteFile.Refresh();
            Assert.That(remoteFile.LastModificationDate, Is.EqualTo(oldDate).Within(2).Seconds);
        }

        [Test]
        public void OneLocalFileCreatedAndModificationDateIsSynced() {
            if (!this.session.IsServerAbleToUpdateModificationDate()) {
                Assert.Ignore("Server does not support the synchronization of modification dates");
            }

            var filePath = Path.Combine(this.localRootDir.FullName, defaultFileName);
            var fileInfo = new FileInfo(filePath);
            using (StreamWriter sw = fileInfo.CreateText()) {
                sw.Write(defaultContent);
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

        [Test, Timeout(360000), MaxTime(300000)]
        public void OneFileIsCopiedAFewTimes([Values(true, false)]bool contentChanges, [Values(1,2,5,10)]int times) {
            this.ContentChangesActive = contentChanges;
            FileSystemInfoFactory fsFactory = new FileSystemInfoFactory();
            var fileNames = new List<string>();
            string fileName = "file";
            this.remoteRootDir.CreateDocument(fileName + ".bin", defaultContent);
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
                Assert.That(syncedFileInfo.Length, Is.EqualTo(defaultContent.Length));
                if (localFile.FullName.Equals(file.FullName)) {
                    Assert.That(syncedFileInfo.Uuid, Is.EqualTo(uuid));
                } else {
                    Assert.That(syncedFileInfo.Uuid, Is.Not.Null);
                    Assert.That(syncedFileInfo.Uuid, Is.Not.EqualTo(uuid));
                }
            }

            AssertThatEventCounterIsZero();
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

    }
}