//-----------------------------------------------------------------------
// <copyright file="MoveIT.cs" company="GRAU DATA AG">
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

namespace TestLibrary.IntegrationTests.SelectiveIgnoreTests {
    using System;
    using System.IO;
    using System.Threading;

    using CmisSync.Lib.Cmis.ConvenienceExtenders;
    using CmisSync.Lib.Events;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture, TestName("MoveIT"), Category("Slow"), Category("SelectiveIgnore"), Timeout(180000)]
    public class MoveIT : BaseFullRepoTest {
        [Test]
        public void MoveRemoteFolderTreeInsideIgnoredFolder([Values(true, false)]bool contentChanges) {
            this.ContentChangesActive = contentChanges;
            this.session.EnsureSelectiveIgnoreSupportIsAvailable();
            var ignoredFolder = this.remoteRootDir.CreateFolder("ignored");
            var anotherFolderTree = this.remoteRootDir.CreateFolder("A");
            anotherFolderTree.CreateFolder("B");

            string tree = @"
.
├── ignored
└── A
    └── B";
            Assert.That(new FolderTree(this.remoteRootDir, "."), Is.EqualTo(new FolderTree(tree)));
            this.InitializeAndRunRepo(swallowExceptions: true);
            Assert.That(new FolderTree(this.localRootDir, "."), Is.EqualTo(new FolderTree(tree)));

            ignoredFolder.Refresh();
            ignoredFolder.IgnoreAllChildren();

            anotherFolderTree.Refresh();
            anotherFolderTree.Move(this.remoteRootDir, ignoredFolder);

            this.WaitForRemoteChanges();
            this.AddStartNextSyncEvent();
            this.repo.Run();

            string localTree = @"
.
└── ignored";
            Assert.That(new FolderTree(this.localRootDir, "."), Is.EqualTo(new FolderTree(localTree)));
        }

        [Test]
        public void MoveLocalFolderTreeInsideIgnoredFolder([Values(true, false)]bool contentChanges) {
            this.ContentChangesActive = contentChanges;
            this.session.EnsureSelectiveIgnoreSupportIsAvailable();
            var ignoredFolder = this.remoteRootDir.CreateFolder("ignored");
            var anotherFolderTree = this.remoteRootDir.CreateFolder("A");
            anotherFolderTree.CreateFolder("B");
            string tree = @"
.
├── ignored
└── A
    └── B";
            Assert.That(new FolderTree(this.remoteRootDir, "."), Is.EqualTo(new FolderTree(tree)));
            this.InitializeAndRunRepo();
            this.localRootDir.Refresh();
            Assert.That(new FolderTree(this.localRootDir, "."), Is.EqualTo(new FolderTree(tree)));

            ignoredFolder.Refresh();
            ignoredFolder.IgnoreAllChildren();

            this.WaitForRemoteChanges();
            this.AddStartNextSyncEvent();
            this.repo.Run();

            var localDirs = this.localRootDir.GetDirectories();
            var localA = localDirs[0].Name == "A" ? localDirs[0] : localDirs[1];
            var localIgnored = localDirs[0].Name == "ignored" ? localDirs[0] : localDirs[1];
            localA.MoveTo(Path.Combine(localIgnored.FullName, localA.Name));
            this.WaitUntilQueueIsNotEmpty();
            this.AddStartNextSyncEvent();
            this.repo.Run();

            string localTree = @"
.
└── ignored
    └── A
        └── B";
            string remoteTree = @"
.
└── ignored";
            this.localRootDir.Refresh();
            this.remoteRootDir.Refresh();
            Assert.That(new FolderTree(this.localRootDir, "."), Is.EqualTo(new FolderTree(localTree)));
            Assert.That(new FolderTree(this.remoteRootDir, "."), Is.EqualTo(new FolderTree(remoteTree)));
       }
    }
}