//-----------------------------------------------------------------------
// <copyright file="RemoveLocalFileIT.cs" company="GRAU DATA AG">
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
    using System.Threading;

    using CmisSync.Lib.Cmis.ConvenienceExtenders;

    using NUnit.Framework;

    [TestFixture, Category("Slow"), TestName("RemoveLocalFile"), Timeout(180000)]
    public class RemoveLocalFileIT : AbstractBaseSyncScenarioIT {
        [Test]
        public void OneLocalFileIsRemoved() {
            var filePath = Path.Combine(this.localRootDir.FullName, defaultFileName);
            this.remoteRootDir.CreateDocument(defaultFileName, defaultContent);

            InitializeAndRunRepo();

            // Stabilize test by waiting for all delayed fs events
            Thread.Sleep(500);

            // Process the delayed fs events
            repo.Run();
            AssertThatFolderStructureIsEqual();

            new FileInfo(filePath).Delete();

            WaitUntilQueueIsNotEmpty();
            repo.Run();

            this.remoteRootDir.Refresh();
            Assert.That(this.remoteRootDir.GetChildren().TotalNumItems, Is.EqualTo(0));
            AssertThatFolderStructureIsEqual();
            AssertThatEventCounterIsZero();
        }
    }
}