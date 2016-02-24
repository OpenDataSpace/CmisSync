//-----------------------------------------------------------------------
// <copyright file="RenameIgnoredFolderIT.cs" company="GRAU DATA AG">
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
namespace TestLibrary.IntegrationTests.RegexIgnoreTests {
    using System;
    using System.IO;
    using System.Linq;

    using CmisSync.Lib.Cmis.ConvenienceExtenders;

    using DotCMIS.Client;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture, TestName("RenameIgnoredFolder"), Category("RegexIgnore"), Category("Slow"), Timeout(180000)]
    public class RenameIgnoredFolderIT : BaseFullRepoTest {
        [Test]
        public void RenameLocalIgnoredFolderToNotIgnoredFolder(
            [Values(true, false)]bool contentChanges,
            [Values(true, false)]bool withFileInside)
        {
            string ignoredName = ".Ignored";
            string fileName = "file.bin";
            string normalName = "NotIgnoredFolder";
            this.ContentChangesActive = contentChanges;
            var ignoredLocalFolder = this.localRootDir.CreateSubdirectory(ignoredName);
            var file = new FileInfo(Path.Combine(ignoredLocalFolder.FullName, fileName));
            if (withFileInside) {
                using (file.Create());
            }

            this.InitializeAndRunRepo();
            Assert.That(this.remoteRootDir.GetChildren().TotalNumItems, Is.EqualTo(0));

            ignoredLocalFolder.MoveTo(Path.Combine(this.localRootDir.FullName, normalName));

            this.WaitUntilQueueIsNotEmpty();
            this.AddStartNextSyncEvent();
            this.repo.Run();

            this.remoteRootDir.Refresh();
            var remoteFolder = this.remoteRootDir.GetChildren().First() as IFolder;
            Assert.That(remoteFolder.Name, Is.EqualTo(normalName));
            if (withFileInside) {
                Assert.That(remoteFolder.GetChildren().First().Name, Is.EqualTo(fileName));
            } else {
                Assert.That(remoteFolder.GetChildren().TotalNumItems, Is.EqualTo(0));
            }
        }

        [Test]
        public void RenameRemoteIgnoredFolderToNotIgnoredFolder(
            [Values(true, false)]bool contentChanges,
            [Values(true, false)]bool withFileInside)
        {
            string ignoredName = ".Ignored";
            string fileName = "file.bin";
            string normalName = "NotIgnoredFolder";
            this.ContentChangesActive = contentChanges;
            var ignoredRemoteFolder = this.remoteRootDir.CreateFolder(ignoredName);
            if (withFileInside) {
                ignoredRemoteFolder.CreateDocument(fileName, string.Empty);
            }

            this.InitializeAndRunRepo();
            Assert.That(this.localRootDir.GetFileSystemInfos(), Is.Empty);

            ignoredRemoteFolder.Rename(normalName);

            this.WaitForRemoteChanges();
            this.AddStartNextSyncEvent();
            this.repo.Run();

            var localFolder = this.localRootDir.GetDirectories().First();
            Assert.That(localFolder.Name, Is.EqualTo(normalName));
            if (withFileInside) {
                Assert.That(localFolder.GetFiles().First().Name, Is.EqualTo(fileName));
            } else {
                Assert.That(localFolder.GetFileSystemInfos(), Is.Empty);
            }
        }
    }
}