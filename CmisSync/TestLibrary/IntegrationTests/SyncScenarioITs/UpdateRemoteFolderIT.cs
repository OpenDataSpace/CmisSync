//-----------------------------------------------------------------------
// <copyright file="UpdateRemoteFolderIT.cs" company="GRAU DATA AG">
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

    using CmisSync.Lib.Cmis.ConvenienceExtenders;

    using NUnit.Framework;

    [TestFixture, Category("Slow"), TestName("UpdateRemoteFolder"), Timeout(180000)]
    public class UpdateRemoteFolderIT : AbstractBaseSyncScenarioIT {
        private readonly string targetFolderName = "target";
        [Test]
        public void RemoteFolderIsRenamed([Values(true, false)]bool contentChanges) {
            this.ContentChangesActive = contentChanges;
            var remoteFolder = this.remoteRootDir.CreateFolder(defaultFolderName);
            InitializeAndRunRepo(swallowExceptions: true);

            remoteFolder.Refresh();
            remoteFolder.Rename(targetFolderName, true);
            WaitForRemoteChanges();
            AddStartNextSyncEvent();
            repo.Run();

            Assert.That(this.localRootDir.GetFileSystemInfos().Length, Is.EqualTo(1));
            Assert.That(this.localRootDir.GetDirectories()[0].Name, Is.EqualTo(targetFolderName));
            AssertThatFolderStructureIsEqual();
            AssertThatEventCounterIsZero();
        }

        [Test]
        public void RemoteFolderIsMovedIntoAnotherRemoteFolder([Values(true, false)]bool contentChanges) {
            this.ContentChangesActive = contentChanges;
            var remoteFolder = this.remoteRootDir.CreateFolder(defaultFolderName);
            var remoteTargetFolder = this.remoteRootDir.CreateFolder(targetFolderName);
            InitializeAndRunRepo();

            remoteFolder.Move(this.remoteRootDir, remoteTargetFolder);
            WaitForRemoteChanges();
            AddStartNextSyncEvent();
            repo.Run();

            Assert.That(this.localRootDir.GetFileSystemInfos().Length, Is.EqualTo(1));
            Assert.That(this.localRootDir.GetDirectories()[0].Name, Is.EqualTo(targetFolderName));
            Assert.That(this.localRootDir.GetDirectories()[0].GetFileSystemInfos().Length, Is.EqualTo(1));
            Assert.That(this.localRootDir.GetDirectories()[0].GetDirectories()[0].Name, Is.EqualTo(defaultFolderName));
            AssertThatFolderStructureIsEqual();
            AssertThatEventCounterIsZero();
        }

        [Test]
        public void OneRemoteFolderIsRenamedToLowerCase([Values(true, false)]bool contentChanges) {
            this.ContentChangesActive = contentChanges;
            string oldFolderName = "A";
            string newFolderName = oldFolderName.ToLower();
            var folder = this.remoteRootDir.CreateFolder(oldFolderName);

            InitializeAndRunRepo();

            folder.Refresh();
            folder.Rename(newFolderName);
            WaitForRemoteChanges();
            AddStartNextSyncEvent();
            repo.Run();

            AssertThatEventCounterIsZero();
            AssertThatFolderStructureIsEqual();
        }
    }
}