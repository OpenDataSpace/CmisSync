//-----------------------------------------------------------------------
// <copyright file="FileCrud.cs" company="GRAU DATA AG">
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

    using CmisSync.Lib.Cmis.ConvenienceExtenders;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.SelectiveIgnore;

    using DotCMIS.Client;
    using DotCMIS.Exceptions;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture, Timeout(900000), TestName("SelectiveIgnore")]
    public class FileCrud : BaseFullRepoTest
    {
        [Test, Category("Slow"), Category("SelectiveIgnore")]
        public void LocalFileIsCreatedInIgnoredFolder() {
            this.session.EnsureSelectiveIgnoreSupportIsAvailable();
            var folder = this.remoteRootDir.CreateFolder("ignored");

            this.repo.Initialize();
            this.repo.Run();

            folder.Refresh();
            folder.IgnoreAllChildren();

            this.repo.SingleStepQueue.AddEvent(new StartNextSyncEvent());
            this.repo.Run();

            var localFolder = this.localRootDir.GetDirectories()[0].FullName;
            var fileInfo = new FileInfo(Path.Combine(localFolder, "file.txt"));
            using (StreamWriter sw = fileInfo.CreateText()) {
                sw.WriteLine("content");
            }

            this.WaitUntilQueueIsNotEmpty();
            this.repo.SingleStepQueue.AddEvent(new StartNextSyncEvent());
            this.repo.Run();

            folder.Refresh();
            Assert.Throws<CmisObjectNotFoundException>(() => this.session.GetObjectByPath(folder.Path + "/file.txt"));
        }
    }
}