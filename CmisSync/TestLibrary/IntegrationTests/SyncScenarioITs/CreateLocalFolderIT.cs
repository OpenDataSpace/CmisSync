//-----------------------------------------------------------------------
// <copyright file="CreateLocalFolderIT.cs" company="GRAU DATA AG">
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
    using System.Linq;

    using DotCMIS.Client;

    using NUnit.Framework;

    [TestFixture, Category("Slow"), TestName("CreateLocalFolder"), Timeout(180000)]
    public class CreateLocalFolderIT : AbstractBaseSyncScenarioIT {
        [Test]
        public void OneLocalFolderCreatedBeforeSyncIsInitialized() {
            this.localRootDir.CreateSubdirectory(defaultFolderName);

            InitializeAndRunRepo();
            var children = this.remoteRootDir.GetChildren();
            Assert.That(children.First().Name, Is.EqualTo(defaultFolderName));
            AssertThatFolderStructureIsEqual();
            AssertThatEventCounterIsZero();
        }

        [Test]
        public void OneLocalFolderCreatedAfterSyncInitialized([Values(true, false)]bool contentChanges) {
            this.ContentChangesActive = contentChanges;
            InitializeAndRunRepo();

            this.localRootDir.CreateSubdirectory(defaultFolderName);
            WaitUntilQueueIsNotEmpty();
            AddStartNextSyncEvent();
            repo.Run();
            WaitForRemoteChanges();
            AddStartNextSyncEvent();
            repo.Run();

            var children = this.remoteRootDir.GetChildren();
            Assert.That(children.TotalNumItems, Is.EqualTo(1));
            var remoteFolder = children.First() as IFolder;
            Assert.That(remoteFolder.Name, Is.EqualTo(defaultFolderName));
            Assert.That(remoteFolder.GetChildren().TotalNumItems, Is.EqualTo(0));
            AssertThatFolderStructureIsEqual();
            AssertThatEventCounterIsZero();
        }
    }
}