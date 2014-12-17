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

namespace TestLibrary.IntegrationTests.SelectiveIgnoreTests
{
    using System;
    using System.Threading;

    using CmisSync.Lib.Cmis.ConvenienceExtenders;
    using CmisSync.Lib.Events;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture, Timeout(900000), TestName("MoveIT")]
    public class MoveIT : BaseFullRepoTest
    {
        [Test, Category("Slow"), Category("SelectiveIgnore")]
        public void MoveRemoteFolderTreeInsideIgnoredFolder() {
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

            Assert.That(new FolderTree(this.localRootDir, "."), Is.EqualTo(new FolderTree(tree)));

            ignoredFolder.Refresh();
            ignoredFolder.IgnoreAllChildren();

            anotherFolderTree.Refresh();
            anotherFolderTree.Move(this.remoteRootDir, ignoredFolder);

            Thread.Sleep(3000);
            this.repo.SingleStepQueue.AddEvent(new StartNextSyncEvent(true));
            this.repo.Run();

            string localTree = @"
.
└── ignored";
            Assert.That(new FolderTree(this.localRootDir, "."), Is.EqualTo(new FolderTree(localTree)));
        }
    }
}