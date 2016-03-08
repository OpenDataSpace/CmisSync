//-----------------------------------------------------------------------
// <copyright file="RemoveLocalFolderIT.cs" company="GRAU DATA AG">
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

    using NUnit.Framework;

    [TestFixture, Category("Slow"), TestName("RemoveLocalFolder"), Timeout(180000)]
    public class RemoveLocalFolderIT : AbstractBaseSyncScenarioIT {
        [Test]
        public void OneLocalFolderRemoved() {
            this.localRootDir.CreateSubdirectory(defaultFolderName);
            InitializeAndRunRepo();
            AssertThatFolderStructureIsEqual();

            this.localRootDir.GetDirectories().First().Delete();
            WaitUntilQueueIsNotEmpty();
            repo.Run();

            this.remoteRootDir.Refresh();
            Assert.That(this.remoteRootDir.GetChildren(), Is.Empty);
            AssertThatFolderStructureIsEqual();
            AssertThatEventCounterIsZero();
        }
    }
}