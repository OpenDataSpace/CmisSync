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

namespace TestLibrary.IntegrationTests.SelectiveIgnoreTests {
    using System;
    using System.IO;
    using System.Threading;

    using CmisSync.Lib.Cmis.ConvenienceExtenders;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.SelectiveIgnore;

    using DotCMIS.Client;
    using DotCMIS.Exceptions;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture, Timeout(60000), TestName("FileCRUD"), Category("Slow"), Category("SelectiveIgnore")]
    public class FileCrud : BaseFullRepoTest {
        [Test]
        public void LocalFileIsCreatedInIgnoredFolder([Values(true, false)]bool contentChanges) {
            this.session.EnsureSelectiveIgnoreSupportIsAvailable();
            this.ContentChangesActive = contentChanges;
            var folder = this.remoteRootDir.CreateFolder("ignored");

            this.InitializeAndRunRepo();

            folder.Refresh();
            folder.IgnoreAllChildren();

            this.WaitForRemoteChanges();
            this.AddStartNextSyncEvent();
            this.repo.Run();

            var localFolder = this.localRootDir.GetDirectories()[0].FullName;
            var fileInfo = new FileInfo(Path.Combine(localFolder, "file.txt"));
            using (StreamWriter sw = fileInfo.CreateText()) {
                sw.WriteLine("content");
            }

            this.WaitUntilQueueIsNotEmpty();
            this.AddStartNextSyncEvent();
            this.repo.Run();

            folder.Refresh();
            Assert.Throws<CmisObjectNotFoundException>(() => this.session.GetObjectByPath(folder.Path + "/file.txt"), "The file shouldn't exist on the server, but it does");
        }

        [Test]
        public void LocalFileIsChangedInIgnoredFolder([Values(true, false)]bool contentChanges) {
            this.session.EnsureSelectiveIgnoreSupportIsAvailable();
            this.ContentChangesActive = contentChanges;
            var folder = this.remoteRootDir.CreateFolder("ignored");
            var file = folder.CreateDocument("file.txt", "content");

            var oldRemoteLength = file.ContentStreamLength;

            this.InitializeAndRunRepo();

            folder.Refresh();
            folder.IgnoreAllChildren();

            this.WaitForRemoteChanges();
            this.AddStartNextSyncEvent();
            this.repo.Run();

            var localFolder = this.localRootDir.GetDirectories()[0].FullName;
            var fileInfo = new FileInfo(Path.Combine(localFolder, "file.txt"));
            using (StreamWriter sw = fileInfo.CreateText()) {
                sw.WriteLine("content");
                sw.WriteLine("content");
            }

            this.WaitUntilQueueIsNotEmpty();
            this.AddStartNextSyncEvent();
            this.repo.Run();

            folder.Refresh();
            file.Refresh();
            Assert.That(file.ContentStreamLength, Is.EqualTo(oldRemoteLength));
        }

        [Test]
        public void LocalFileIsRenamedInIgnoredFolder([Values(true, false)]bool contentChanges) {
            this.session.EnsureSelectiveIgnoreSupportIsAvailable();
            this.ContentChangesActive = contentChanges;
            var folder = this.remoteRootDir.CreateFolder("ignored");
            var oldName = "file.txt";
            var file = folder.CreateDocument(oldName, "content");

            this.InitializeAndRunRepo();

            folder.Refresh();
            folder.IgnoreAllChildren();

            this.WaitForRemoteChanges();
            this.AddStartNextSyncEvent();
            this.repo.Run();

            var localFolder = this.localRootDir.GetDirectories()[0].FullName;
            new FileInfo(Path.Combine(localFolder, oldName)).MoveTo(Path.Combine(localFolder, "anotherName.txt"));

            this.WaitUntilQueueIsNotEmpty();
            this.AddStartNextSyncEvent();
            this.repo.Run();

            folder.Refresh();
            file.Refresh();
            Assert.That(file.Name, Is.EqualTo(oldName));
        }

        [Test]
        public void LocalFileIsDeletedInIgnoredFolder([Values(true, false)]bool contentChanges) {
            this.session.EnsureSelectiveIgnoreSupportIsAvailable();
            this.ContentChangesActive = contentChanges;
            var folder = this.remoteRootDir.CreateFolder("ignored");
            var file = folder.CreateDocument("file.txt", "content");

            var oldRemoteLength = file.ContentStreamLength;

            this.InitializeAndRunRepo();

            this.WaitForRemoteChanges();
            folder.Refresh();
            folder.IgnoreAllChildren();

            this.AddStartNextSyncEvent();
            this.repo.Run();

            var localFolder = this.localRootDir.GetDirectories()[0].FullName;
            var fileInfo = new FileInfo(Path.Combine(localFolder, "file.txt"));
            fileInfo.Delete();

            this.WaitUntilQueueIsNotEmpty();
            this.AddStartNextSyncEvent();
            this.repo.Run();

            folder.Refresh();
            file.Refresh();
            Assert.That(file.ContentStreamLength, Is.EqualTo(oldRemoteLength));
        }
    }
}