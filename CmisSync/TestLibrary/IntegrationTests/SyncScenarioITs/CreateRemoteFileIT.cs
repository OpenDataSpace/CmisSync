//-----------------------------------------------------------------------
// <copyright file="CreateRemoteFileIT.cs" company="GRAU DATA AG">
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

    using CmisSync.Lib.Cmis.ConvenienceExtenders;

    using DotCMIS.Client;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture, Category("Slow"), TestName("CreateRemoteFile"), Timeout(180000)]
    public class CreateRemoteFileIT : AbstractBaseSyncScenarioIT {
        [Test]
        public void OneRemoteFileCreatedBeforeSyncIsInitialized() {
            var doc = this.remoteRootDir.CreateDocument(defaultFileName, defaultContent);

            InitializeAndRunRepo();

            AssertThatOneFileIsSynced(doc);
        }

        [Test]
        public void OneRemoteFileCreatedAfterSyncIsInitialized([Values(true, false)]bool contentChanges) {
            this.ContentChangesActive = contentChanges;
            InitializeAndRunRepo();

            var doc = this.remoteRootDir.CreateDocument(defaultFileName, defaultContent);
            WaitForRemoteChanges();
            AddStartNextSyncEvent();
            repo.Run();

            AssertThatOneFileIsSynced(doc);
        }

        [Test]
        public void OneEmptyRemoteFileCreated([Values(true, false)]bool contentChanges) {
            this.ContentChangesActive = contentChanges;
            InitializeAndRunRepo();

            var doc = this.remoteRootDir.CreateDocument(defaultFileName, (string)null);
            WaitForRemoteChanges();
            AddStartNextSyncEvent();
            repo.Run();

            AssertThatOneFileIsSynced(doc, string.Empty);
        }

        // Timeout is set to 10 minutes for 10 x 1 MB file
        [Test, Timeout(600000), MaxTime(600000)]
        public void CreateMultipleRemoteFiles([Values(10)]int fileNumber) {
            string content = new string('A', 1024 * 1024);
            for (int i = 0; i < fileNumber; ++i) {
                string fileName = "file" + i.ToString();
                this.remoteRootDir.CreateDocument(fileName, content);
            }

            InitializeAndRunRepo();

            var localFiles = this.localRootDir.GetFileSystemInfos();
            Assert.That(localFiles.Length, Is.EqualTo(fileNumber));
            foreach (var localFile in localFiles) {
                Assert.That(localFile, Is.InstanceOf(typeof(FileInfo)));
                Assert.That((localFile as FileInfo).Length, Is.EqualTo(content.Length));
            }

            var remoteFiles = this.remoteRootDir.GetChildren();
            Assert.That(remoteFiles.TotalNumItems, Is.EqualTo(fileNumber));
            foreach (IDocument remoteFile in remoteFiles.OfType<IDocument>()) {
                Assert.That(remoteFile.ContentStreamLength, Is.EqualTo(content.Length));
                remoteFile.AssertThatIfContentHashExistsItIsEqualTo(content);
            }

            AssertThatEventCounterIsZero();
            AssertThatFolderStructureIsEqual();
        }

        private void AssertThatOneFileIsSynced(IDocument doc, string content = null) {
            content = content ?? defaultContent;
            var children = this.localRootDir.GetFileSystemInfos();
            Assert.That(children.Length, Is.EqualTo(1));
            var child = children.First();
            Assert.That(child, Is.InstanceOf(typeof(FileInfo)));
            Assert.That((child as FileInfo).Length, Is.EqualTo(content.Length));
            doc.Refresh();
            doc.AssertThatIfContentHashExistsItIsEqualTo(content);
            AssertThatEventCounterIsZero();
        }
    }
}