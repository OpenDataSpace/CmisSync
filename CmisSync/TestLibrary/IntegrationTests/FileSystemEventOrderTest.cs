//-----------------------------------------------------------------------
// <copyright file="FileSystemEventOrderTest.cs" company="GRAU DATA AG">
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

#if !__COCOA__
namespace TestLibrary.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Storage;
    using CmisSync.Lib.Producer.Watcher;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class FileSystemEventOrderTest
    {
        private string path;
        private List<FileSystemEventArgs> list;

        [SetUp]
        public void SetUp() {
            this.path = Path.Combine(ITUtils.GetConfig()[1].ToString(), Path.GetRandomFileName());
            Directory.CreateDirectory(this.path);
            this.list = new List<FileSystemEventArgs>();
        }

        [TearDown]
        public void DeleteTempDirectory() {
            Directory.Delete(this.path, true);
        }

        [Test, Category("Medium"), Timeout(20000), Repeat(5), Category("Erratic")]
        public void MoveFolderInsideTheWatchedFolder() {
            string oldName = Path.GetRandomFileName();
            string newName = Path.GetRandomFileName();
            using (FileSystemWatcher fsWatcher = new FileSystemWatcher(this.path)) {
                fsWatcher.IncludeSubdirectories = true;
                fsWatcher.Filter = "*";
                fsWatcher.InternalBufferSize = 4 * 1024 * 16;
                fsWatcher.NotifyFilter = NotifyFilters.Size | NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite | NotifyFilters.Security;
                fsWatcher.Created += (object sender, FileSystemEventArgs e) => this.list.Add(e);
                fsWatcher.Changed += (object sender, FileSystemEventArgs e) => this.list.Add(e);
                fsWatcher.Deleted += (object sender, FileSystemEventArgs e) => this.list.Add(e);
                fsWatcher.Renamed += (object sender, RenamedEventArgs e) => this.list.Add(e);
                DirectoryInfo dirA = new DirectoryInfo(Path.Combine(this.path, Path.GetRandomFileName()));
                dirA.Create();
                DirectoryInfo dirB = new DirectoryInfo(Path.Combine(this.path, Path.GetRandomFileName()));
                dirB.Create();
                DirectoryInfo movingDir = new DirectoryInfo(Path.Combine(dirA.FullName, oldName));
                movingDir.Create();
                fsWatcher.EnableRaisingEvents = true;
                movingDir.MoveTo(Path.Combine(dirB.FullName, newName));
                while (this.list.Count < 2) {
                    Thread.Sleep(100);
                }

                Assert.That(this.list.Count, Is.GreaterThanOrEqualTo(2));
                Assert.That(this.list[0].ChangeType, Is.EqualTo(WatcherChangeTypes.Deleted));
                Assert.That(this.list[0].FullPath, Is.EqualTo(Path.Combine(dirA.FullName, oldName)));
                Assert.That(this.list[1].ChangeType, Is.EqualTo(WatcherChangeTypes.Created));
                Assert.That(this.list[1].FullPath, Is.EqualTo(Path.Combine(dirB.FullName, newName)));
            }
        }

        [Test, Category("Medium"), Timeout(20000), Repeat(5), Category("Erratic")]
        public void MoveFileInsideTheWatchedFolder() {
            string oldName = Path.GetRandomFileName();
            string newName = Path.GetRandomFileName();
            using (FileSystemWatcher fsWatcher = new FileSystemWatcher(this.path)) {
                fsWatcher.IncludeSubdirectories = true;
                fsWatcher.Filter = "*";
                fsWatcher.InternalBufferSize = 4 * 1024 * 16;
                fsWatcher.NotifyFilter = NotifyFilters.Size | NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite | NotifyFilters.Security;
                fsWatcher.Created += (object sender, FileSystemEventArgs e) => this.list.Add(e);
                fsWatcher.Changed += (object sender, FileSystemEventArgs e) => this.list.Add(e);
                fsWatcher.Deleted += (object sender, FileSystemEventArgs e) => this.list.Add(e);
                fsWatcher.Renamed += (object sender, RenamedEventArgs e) => this.list.Add(e);
                DirectoryInfo dirA = new DirectoryInfo(Path.Combine(this.path, Path.GetRandomFileName()));
                dirA.Create();
                DirectoryInfo dirB = new DirectoryInfo(Path.Combine(this.path, Path.GetRandomFileName()));
                dirB.Create();
                FileInfo movingFile = new FileInfo(Path.Combine(dirA.FullName, oldName));
                using (movingFile.Create()){
                }

                fsWatcher.EnableRaisingEvents = true;
                movingFile.MoveTo(Path.Combine(dirB.FullName, newName));
                while (this.list.Count < 2) {
                    Thread.Sleep(100);
                }

                Assert.That(this.list.Count, Is.GreaterThanOrEqualTo(2));
                Assert.That(this.list[0].ChangeType, Is.EqualTo(WatcherChangeTypes.Deleted));
                Assert.That(this.list[0].FullPath, Is.EqualTo(Path.Combine(dirA.FullName, oldName)));
                Assert.That(this.list[1].ChangeType, Is.EqualTo(WatcherChangeTypes.Created));
                Assert.That(this.list[1].FullPath, Is.EqualTo(Path.Combine(dirB.FullName, newName)));
            }
        }
    }
}
#endif