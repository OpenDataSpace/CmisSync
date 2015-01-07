//-----------------------------------------------------------------------
// <copyright file="RenameIT.cs" company="GRAU DATA AG">
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
    using System.IO;
    using System.Linq;
    using System.Threading;

    using CmisSync.Lib.Cmis.ConvenienceExtenders;
    using CmisSync.Lib.Events;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture, Timeout(900000), TestName("RenameIT")]
    public class RenameIT : BaseFullRepoTest
    {
        [Test, Category("Slow"), Category("SelectiveIgnore")]
        public void RenameRemoteIgnoredFolderRenamesAlsoLocalFolder([Values(true, false)]bool contentChanges) {
            this.ContentChangesActive = contentChanges;
            this.session.EnsureSelectiveIgnoreSupportIsAvailable();
            string folderName = "ignored";
            string newFolderName = "newName";
            var ignoredFolder = this.remoteRootDir.CreateFolder(folderName);
            this.InitializeAndRunRepo();

            ignoredFolder.Refresh();
            ignoredFolder.IgnoreAllChildren();

            Thread.Sleep(3000);
            this.AddStartNextSyncEvent();
            this.repo.Run();

            ignoredFolder.Rename(newFolderName);

            Thread.Sleep(3000);
            this.AddStartNextSyncEvent();
            this.repo.Run();

            Assert.That(this.localRootDir.GetDirectories().First().Name, Is.EqualTo(newFolderName));
        }

        [Test, Category("Slow"), Category("SelectiveIgnore")]
        public void RenameLocalIgnoredFolderRenamesAlsoRemoteFolder([Values(true, false)]bool contentChanges) {
            this.session.EnsureSelectiveIgnoreSupportIsAvailable();
            this.ContentChangesActive = contentChanges;
            string folderName = "ignored";
            string newFolderName = "newName";
            var ignoredFolder = this.remoteRootDir.CreateFolder(folderName);
            this.InitializeAndRunRepo();

            ignoredFolder.Refresh();
            ignoredFolder.IgnoreAllChildren();

            Thread.Sleep(3000);
            this.AddStartNextSyncEvent();
            this.repo.Run();

            var localFolder = this.localRootDir.GetDirectories().First();
            localFolder.MoveTo(Path.Combine(this.localRootDir.FullName, newFolderName));
            this.WaitUntilQueueIsNotEmpty();
            this.AddStartNextSyncEvent();
            this.repo.Run();

            Assert.That(this.localRootDir.GetDirectories().First().Name, Is.EqualTo(newFolderName));
            ignoredFolder.Refresh();
            Assert.That(ignoredFolder.Name, Is.EqualTo(newFolderName));
        }

        [Test, Category("Slow"), Category("SelectiveIgnore"), Ignore("TODO")]
        public void RenameLocalFolderToIgnoredRemoteFolderName([Values(true, false)]bool contentChanges) {
            this.ContentChangesActive = contentChanges;
            this.session.EnsureSelectiveIgnoreSupportIsAvailable();
            string folderName = "ignored";
            var ignoredFolder = this.remoteRootDir.CreateFolder(folderName);
            ignoredFolder.IgnoreAllChildren();
        }
    }
}