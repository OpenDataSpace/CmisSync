//-----------------------------------------------------------------------
// <copyright file="RemoteObjectDeletedTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.ConsumerTests.SituationSolverTests {
    using System;
    using System.Collections.Generic;
    using System.IO;

    using CmisSync.Lib.Consumer.SituationSolver;
    using CmisSync.Lib.Filter;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;

    using Moq;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture]
    public class RemoteObjectDeletedTest {
        private readonly string name = "a";
        private readonly string path = Path.Combine(Path.GetTempPath(), "a");
        private Mock<ISession> session;
        private Mock<IMetaDataStorage> storage;
        private RemoteObjectDeleted underTest;
        private Mock<IFilterAggregator> filters;
        private IgnoredFileNamesFilter fileNameFilter;
        private IgnoredFolderNameFilter folderNameFilter;

        [Test, Category("Fast"), Category("Solver")]
        public void DefaultConstructorTest() {
            var session = new Mock<ISession>();
            session.SetupTypeSystem();
            new RemoteObjectDeleted(session.Object, Mock.Of<IMetaDataStorage>(), Mock.Of<IFilterAggregator>());
        }

        [Test, Category("Fast"), Category("Solver")]
        public void ConstructorThrowsExceptionIfFiltersAreNull() {
            var session = new Mock<ISession>();
            session.SetupTypeSystem();
            Assert.Throws<ArgumentNullException>(() => new RemoteObjectDeleted(session.Object, Mock.Of<IMetaDataStorage>(), null));
        }

        [Test, Category("Fast"), Category("Solver")]
        public void RemoteFolderDeleted() {
            this.SetUpTestMocks();
            Mock<IMappedObject> folder = this.storage.AddLocalFolder(this.path, "id");
            var dirInfo = new Mock<IDirectoryInfo>();
            dirInfo.Setup(d => d.FullName).Returns(this.path);

            this.underTest.Solve(dirInfo.Object, null);

            dirInfo.Verify(d => d.Delete(false), Times.Once());
            this.storage.Verify(s => s.RemoveObject(It.Is<IMappedObject>(o => o == folder.Object)), Times.Once());
        }

        [Test, Category("Fast"), Category("Solver")]
        public void RemoteFileDeleted() {
            this.SetUpTestMocks();
            DateTime lastModified = DateTime.UtcNow;
            var fileInfo = new Mock<IFileInfo>();
            fileInfo.Setup(f => f.FullName).Returns(this.path);
            fileInfo.Setup(f => f.LastWriteTimeUtc).Returns(lastModified);
            fileInfo.Setup(f => f.Exists).Returns(true);
            fileInfo.Setup(f => f.Delete()).Callback(() => fileInfo.Setup(f1 => f1.Refresh()).Callback(() => fileInfo.Setup(f3 => f3.Exists).Returns(false)));
            var mappedObject = new MappedObject("a", "id", MappedObjectType.File, "parentId", "changeToken", 0) {
                LastLocalWriteTimeUtc = lastModified
            };
            this.storage.AddMappedFile(mappedObject, this.path);

            this.underTest.Solve(fileInfo.Object, null);

            fileInfo.Verify(f => f.Delete(), Times.Once());
            this.storage.Verify(s => s.RemoveObject(mappedObject), Times.Once());
        }

        [Test, Category("Fast"), Category("Solver")]
        public void RemoteFileDeletedButLocalFileHasBeenChangedBeforeBeingHandled() {
            this.SetUpTestMocks();
            Mock<IMappedObject> file = this.storage.AddLocalFile(this.path, "id");
            var fileInfo = new Mock<IFileInfo>();
            fileInfo.Setup(f => f.FullName).Returns(this.path);

            this.underTest.Solve(fileInfo.Object, null);

            fileInfo.Verify(f => f.Delete(), Times.Never());
            fileInfo.VerifySet(f => f.Uuid = null, Times.Once());
            this.storage.Verify(s => s.RemoveObject(It.Is<IMappedObject>(o => o == file.Object)), Times.Once());
        }

        [Test, Category("Fast"), Category("Solver")]
        public void RemoteFileDeletedButLocalFileDoesNotExistsInStorage() {
            this.SetUpTestMocks();
            var fileInfo = new Mock<IFileInfo>();
            fileInfo.Setup(f => f.FullName).Returns(this.path);

            this.underTest.Solve(fileInfo.Object, null);

            fileInfo.Verify(f => f.Delete(), Times.Never());
            this.storage.VerifyThatNoObjectIsManipulated();
        }

        [Test, Category("Fast"), Category("Solver")]
        public void RemoteFolderDeletedButNotAllContainingFilesAreSyncedYet() {
            this.SetUpTestMocks();
            string fileName = "fileName";
            string filePath = Path.Combine(this.path, fileName);
            string syncedFileName = "syncedFileName";
            string syncedFilePath = Path.Combine(this.path, syncedFileName);
            DateTime lastModified = DateTime.UtcNow;
            Guid syncedFileGuid = Guid.NewGuid();
            Mock<IMappedObject> folder = this.storage.AddLocalFolder(this.path, "id");
            var dirInfo = new Mock<IDirectoryInfo>();
            dirInfo.Setup(d => d.FullName).Returns(this.path);
            var fileInfo = new Mock<IFileInfo>();
            fileInfo.Setup(f => f.FullName).Returns(filePath);
            fileInfo.Setup(f => f.Name).Returns(fileName);
            var syncedFileInfo = new Mock<IFileInfo>();
            syncedFileInfo.Setup(s => s.FullName).Returns(syncedFilePath);
            syncedFileInfo.Setup(s => s.Name).Returns(syncedFileName);
            syncedFileInfo.SetupGuid(syncedFileGuid);
            syncedFileInfo.Setup(s => s.LastWriteTimeUtc).Returns(lastModified);
            dirInfo.SetupFiles(fileInfo.Object, syncedFileInfo.Object);
            var mappedSyncedFile = new MappedObject(syncedFileName, "id", MappedObjectType.File, "parentId", "changeToken", 0) { Guid = syncedFileGuid, LastLocalWriteTimeUtc = lastModified };
            this.storage.AddMappedFile(mappedSyncedFile, syncedFilePath);

            Assert.Throws<IOException>(() => this.underTest.Solve(dirInfo.Object, null));

            dirInfo.Verify(d => d.Delete(true), Times.Never());
            syncedFileInfo.Verify(s => s.Delete(), Times.Once());
            fileInfo.Verify(f => f.Delete(), Times.Never());
            this.storage.Verify(s => s.RemoveObject(It.Is<IMappedObject>(o => o == folder.Object)), Times.Once());
        }

        [Test, Category("Fast"), Category("Solver")]
        public void RemoteFolderDeletedAndOnlyIgnoredFilesAndFoldersAreNotSyncedYet() {
            this.SetUpTestMocks();
            string ignoredFile = "ignoredFile";
            string ignoredFolder = "ignoredFolder";
            var fileWildcard = new List<string>();
            fileWildcard.Add(ignoredFile);
            this.fileNameFilter.Wildcards = fileWildcard;
            var folderWildcard = new List<string>();
            folderWildcard.Add(ignoredFolder);
            this.folderNameFilter.Wildcards = folderWildcard;
            Mock<IMappedObject> folder = this.storage.AddLocalFolder(this.path, "id");
            var dirInfo = new Mock<IDirectoryInfo>();
            dirInfo.Setup(d => d.Name).Returns(this.name);
            var ignoredFileInfo = new Mock<IFileInfo>();
            ignoredFileInfo.Setup(f => f.Name).Returns(ignoredFile);
            var ignoredFolderInfo = new Mock<IDirectoryInfo>();
            ignoredFolderInfo.Setup(f => f.Name).Returns(ignoredFolder);
            dirInfo.SetupFilesAndDirectories(ignoredFileInfo.Object, ignoredFolderInfo.Object);

            this.underTest.Solve(dirInfo.Object, null);

            dirInfo.Verify(d => d.Delete(false), Times.Once());
            ignoredFileInfo.Verify(f => f.Delete(), Times.Once());
            ignoredFolderInfo.Verify(f => f.Delete(true), Times.Once());
            this.storage.Verify(s => s.RemoveObject(It.Is<IMappedObject>(o => o == folder.Object)), Times.Once());
        }

        [Test, Category("Fast"), Category("Solver"), Category("SelectiveIgnore")]
        public void RemoteFolderDeletedAndHasBeenFlaggedToBeIgnored() {
            this.SetUpTestMocks();
            var dirInfo = new Mock<IDirectoryInfo>();
            dirInfo.Setup(d => d.Name).Returns(this.name);
            Mock<IMappedObject> folder = this.storage.AddLocalFolder(this.path, "id");
            folder.Setup(f => f.Ignored).Returns(true);

            this.underTest.Solve(dirInfo.Object, null);

            dirInfo.Verify(d => d.Delete(true), Times.Once());
            this.storage.Verify(s => s.RemoveObject(folder.Object), Times.Once());
        }


        private void SetUpTestMocks() {
            this.session = new Mock<ISession>();
            this.session.SetupTypeSystem();
            this.storage = new Mock<IMetaDataStorage>();
            this.filters = new Mock<IFilterAggregator>();
            this.fileNameFilter = new IgnoredFileNamesFilter();
            this.folderNameFilter = new IgnoredFolderNameFilter();
            this.filters.Setup(f => f.FileNamesFilter).Returns(fileNameFilter);
            this.filters.Setup(f => f.FolderNamesFilter).Returns(folderNameFilter);
            this.underTest = new RemoteObjectDeleted(this.session.Object, this.storage.Object, this.filters.Object);
        }
    }
}