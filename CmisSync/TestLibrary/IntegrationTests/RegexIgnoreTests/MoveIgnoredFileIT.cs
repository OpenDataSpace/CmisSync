//-----------------------------------------------------------------------
// <copyright file="MoveIgnoredFileIT.cs" company="GRAU DATA AG">
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

﻿namespace TestLibrary.IntegrationTests.RegexIgnoreTests {
    using System;
    using System.IO;
    using System.Linq;

    using CmisSync.Lib.Cmis.ConvenienceExtenders;

    using DotCMIS.Client;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture,  TestName("MoveIgnoredFile"), Category("RegexIgnore"), Category("Slow"), Timeout(180000)]
    public class MoveIgnoredFileIT : BaseRegexIgnoreTest {
        private static readonly string fileName = "file.bin";

        [Test]
        public void MoveLocalIgnoredFileToNotIgnoredLocalFolder([Values(true, false)]bool contentChanges) {
            this.ContentChangesActive = contentChanges;
            string notIgnoredName = "normalFolder";
            var ignoredLocalFolder = CreateIgnoredLocalDirectoryPath();
            var notIgnoredLocalFolder = localRootDir.CreateSubdirectory(Path.Combine(this.localRootDir.FullName, notIgnoredName));
            var file = new FileInfo(Path.Combine(ignoredLocalFolder.FullName, fileName));
            using (file.Create());
            this.InitializeAndRunRepo();
            this.remoteRootDir.Refresh();
            var remoteChildren = this.remoteRootDir.GetChildren();
            Assert.That(remoteChildren.TotalNumItems, Is.EqualTo(1));
            var notIgnoredRemoteFolder = remoteChildren.First() as IFolder;
            Assert.That(notIgnoredRemoteFolder.Name, Is.EqualTo(notIgnoredName));
            Assert.That(notIgnoredRemoteFolder.GetChildren().TotalNumItems, Is.EqualTo(0));

            file.MoveTo(Path.Combine(notIgnoredLocalFolder.FullName, fileName));
            this.WaitUntilQueueIsNotEmpty();
            this.WaitForRemoteChanges();
            this.AddStartNextSyncEvent();
            this.repo.Run();

            notIgnoredRemoteFolder.Refresh();
            remoteChildren = notIgnoredRemoteFolder.GetChildren();
            Assert.That(remoteChildren.TotalNumItems, Is.EqualTo(1));
            Assert.That(remoteChildren.First().Name, Is.EqualTo(fileName));
            this.remoteRootDir.Refresh();
            Assert.That(this.remoteRootDir.GetChildren().TotalNumItems, Is.EqualTo(1));
            Assert.That(this.remoteRootDir.GetChildren().First().Name, Is.EqualTo(notIgnoredName));
        }

        [Test]
        public void MoveLocalIgnoredFileToAnotherLocalIgnoredFolder([Values(true, false)]bool contentChanges) {
            this.ContentChangesActive = contentChanges;
            string anotherIgnoredName = ".AnotherIgnoredFolder";
            var ignoredLocalFolder = CreateIgnoredLocalDirectoryPath();
            var anotherIgnoredLocalFolder = CreateIgnoredLocalDirectoryPath(anotherIgnoredName);
            var file = new FileInfo(Path.Combine(ignoredLocalFolder.FullName, fileName));
            using (file.Create());
            this.InitializeAndRunRepo();
            this.remoteRootDir.Refresh();
            Assert.That(this.remoteRootDir.GetChildren().TotalNumItems, Is.EqualTo(0));

            file.MoveTo(Path.Combine(anotherIgnoredLocalFolder.FullName, fileName));
            this.WaitUntilQueueIsNotEmpty();
            this.WaitForRemoteChanges();
            this.AddStartNextSyncEvent();
            this.repo.Run();

            this.remoteRootDir.Refresh();
            Assert.That(this.remoteRootDir.GetChildren().TotalNumItems, Is.EqualTo(0));
        }

        [Test, Ignore("TODO")]
        public void MoveLocalFileToLocalIgnoredFolder() {
        }
    }
}