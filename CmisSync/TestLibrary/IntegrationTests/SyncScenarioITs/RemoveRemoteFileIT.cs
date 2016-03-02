//-----------------------------------------------------------------------
// <copyright file="RemoveRemoteFileIT.cs" company="GRAU DATA AG">
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

    [TestFixture, Category("Slow"), TestName("RemoveRemoteFile"), Timeout(180000)]
    public class RemoveRemoteFileIT : AbstractBaseSyncScenarioIT {
        [Test]
        public void OneRemoteFileIsRemoved([Values(true, false)]bool contentChanges) {
            this.ContentChangesActive = contentChanges;
            var remoteFile = this.remoteRootDir.CreateDocument(defaultFileName, defaultContent);
            InitializeAndRunRepo();
            AssertThatFolderStructureIsEqual();

            remoteFile.Refresh();
            remoteFile.Delete(true);

            WaitForRemoteChanges();
            AddStartNextSyncEvent();
            repo.Run();

            Assert.That(this.localRootDir.GetFileSystemInfos, Is.Empty);
            AssertThatFolderStructureIsEqual();
            AssertThatEventCounterIsZero();
        }
    }
}