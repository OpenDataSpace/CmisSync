//-----------------------------------------------------------------------
// <copyright file="MoveFolderStructureToSyncFolderIT.cs" company="GRAU DATA AG">
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

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture, Category("Slow"), TestName("MoveFolderStructureToSyncFolder"), Timeout(180000)]
    public class MoveFolderStructureToSyncFolderIT : AbstractBaseSyncScenarioIT {
        private string tempPath;
        private DirectoryInfo dirWithContent;
        private DirectoryInfo subDir;
        private FileInfo testFile;
        private FileInfo subTestFile;
        [SetUp]
        public void CreateTempFolder() {
            tempPath = Path.Combine(this.localRootDir.Parent.FullName, Guid.NewGuid().ToString());
            dirWithContent = Directory.CreateDirectory(tempPath);
            subDir = dirWithContent.CreateSubdirectory("subDir");
            testFile = new FileInfo(Path.Combine(dirWithContent.FullName, "testFile.bin"));
            subTestFile = new FileInfo(Path.Combine(subDir.FullName, "subTestFile.bin"));
            using(testFile.Create());
            using(subTestFile.Create());
        }

        [TearDown]
        public void DeleteTempFolderIfExists() {
            if (Directory.Exists(tempPath)) {
                Directory.Delete(tempPath, true);
            }
        }

        [Test]
        public void MoveFolderWithContentToSyncPath([Values(true, false)]bool contentChanges) {
            ContentChangesActive = contentChanges;
            InitializeAndRunRepo(swallowExceptions: true);
            Directory.Move(dirWithContent.FullName, Path.Combine(localRootDir.FullName, dirWithContent.Name));
            WaitUntilQueueIsNotEmpty();
            AddStartNextSyncEvent();
            repo.Run();
            AssertThatFolderStructureIsEqual();
        }
    }
}