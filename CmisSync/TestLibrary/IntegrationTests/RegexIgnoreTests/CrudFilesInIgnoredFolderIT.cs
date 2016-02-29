//-----------------------------------------------------------------------
// <copyright file="CrudFilesInIgnoredFolderIT.cs" company="GRAU DATA AG">
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

    using CmisSync.Lib.Cmis.ConvenienceExtenders;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture, TestName("CrudFilesInIgnoredFolder"), Category("RegexIgnore"), Category("Slow"), Timeout(180000)]
    public class CrudFilesInIgnoredFolderIT : BaseRegexIgnoreTest {
        private static readonly string ignoredName = ".ignored";
        private static readonly string fileName = "file.bin";
        [Test]
        public void CreateFileInIgnoredFolder([Values(true, false)]bool contentChanges) {
            this.ContentChangesActive = contentChanges;
            var ignoredLocalFolder = CreateIgnoredLocalDirectoryPath(ignoredName);
            this.InitializeAndRunRepo();
            Assert.That(remoteRootDir.GetChildren().TotalNumItems, Is.EqualTo(0));
            var file = new FileInfo(Path.Combine(ignoredLocalFolder.FullName, fileName));
            using (file.Create());
            this.WaitUntilQueueIsNotEmpty();
            this.AddStartNextSyncEvent();
            this.repo.Run();
            remoteRootDir.Refresh();
            Assert.That(remoteRootDir.GetChildren().TotalNumItems, Is.EqualTo(0));
        }

        [Test]
        public void CreateFileInIgnoredFolderWhichAlsoExistsOnServer([Values(true, false)]bool contentChanges) {
            this.ContentChangesActive = contentChanges;
            var ignoredLocalFolder = CreateIgnoredLocalDirectoryPath(ignoredName);
            var remoteIgnoredFolder = remoteRootDir.CreateFolder(ignoredName);
            this.InitializeAndRunRepo();
            var file = new FileInfo(Path.Combine(ignoredLocalFolder.FullName, fileName));
            using (file.Create());
            this.WaitUntilQueueIsNotEmpty();
            this.AddStartNextSyncEvent();
            this.repo.Run();
            remoteRootDir.Refresh();
            remoteIgnoredFolder.Refresh();
            Assert.That(remoteIgnoredFolder.GetChildren().TotalNumItems, Is.EqualTo(0));
        }

        [Test]
        public void CreateRemoteFileInIgnoredFolder([Values(true, false)]bool contentChanges) {
            this.ContentChangesActive = contentChanges;
            var ignoredLocalFolder = CreateIgnoredLocalDirectoryPath(ignoredName);
            this.InitializeAndRunRepo();
            var remoteIgnoredFolder = remoteRootDir.CreateFolder(ignoredName);
            remoteIgnoredFolder.CreateDocument(fileName, string.Empty);
            this.WaitForRemoteChanges();
            this.AddStartNextSyncEvent();
            this.repo.Run();
            remoteRootDir.Refresh();
            Assert.That(ignoredLocalFolder.GetFileSystemInfos(), Is.Empty);
        }
    }
}