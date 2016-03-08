//-----------------------------------------------------------------------
// <copyright file="CreateRemoteFolderIT.cs" company="GRAU DATA AG">
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

    using NUnit.Framework;

    [TestFixture, Category("Slow"), TestName("CreateRemoteFolder"), Timeout(180000)]
    public class CreateRemoteFolderIT : AbstractBaseSyncScenarioIT {
        [Test]
        public void OneRemoteFolderCreatedBeforeSyncInit() {
            this.remoteRootDir.CreateFolder(defaultFolderName);

            InitializeAndRunRepo();

            AssertThatOnlyOneFolderIsCreatedAndBothSidesAreInSync();
        }

        [Test]
        public void OneRemoteFolderCreatedAfterSyncInit([Values(true, false)]bool contentChanges) {
            this.ContentChangesActive = contentChanges;

            InitializeAndRunRepo();
            this.remoteRootDir.Refresh();
            this.remoteRootDir.CreateFolder(defaultFolderName);
            WaitForRemoteChanges();
            AddStartNextSyncEvent();
            repo.Run();

            AssertThatOnlyOneFolderIsCreatedAndBothSidesAreInSync();
        }

        private void AssertThatOnlyOneFolderIsCreatedAndBothSidesAreInSync() {
            var localDirs = this.localRootDir.GetFileSystemInfos();
            Assert.That(localDirs.Length, Is.EqualTo(1));
            var localDir = localDirs.First() as DirectoryInfo;
            Assert.That(localDir.Name, Is.EqualTo(defaultFolderName));
            Assert.That(localDir.GetFileSystemInfos(), Is.Empty);
            AssertThatFolderStructureIsEqual();
            AssertThatEventCounterIsZero();
        }
    }
}