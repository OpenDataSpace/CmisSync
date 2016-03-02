//-----------------------------------------------------------------------
// <copyright file="RemoveRemoteFolderIT.cs" company="GRAU DATA AG">
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

    using CmisSync.Lib.Cmis.ConvenienceExtenders;

    using DotCMIS.Client;

    using NUnit.Framework;

    [TestFixture, Category("Slow"), TestName("RemoveRemoteFolder"), Timeout(180000)]
    public class RemoveRemoteFolderIT : AbstractBaseSyncScenarioIT {
        [Test]
        public void OneRemoteFolderIsDeleted([Values(true, false)]bool contentChanges) {
            this.ContentChangesActive = contentChanges;
            this.remoteRootDir.CreateFolder(defaultFolderName);
            InitializeAndRunRepo();
            AssertThatFolderStructureIsEqual();

            (this.remoteRootDir.GetChildren().First() as IFolder).DeleteTree(true, null, true);

            WaitForRemoteChanges();
            AddStartNextSyncEvent();
            repo.Run();

            Assert.That(this.localRootDir.GetFileSystemInfos, Is.Empty);
            AssertThatFolderStructureIsEqual();
            AssertThatEventCounterIsZero();
        }
    }
}