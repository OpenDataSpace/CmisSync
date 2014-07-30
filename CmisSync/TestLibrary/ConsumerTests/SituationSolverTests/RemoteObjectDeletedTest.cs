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
namespace TestLibrary.ConsumerTests.SituationSolverTests
{
    using System;
    using System.IO;

    using CmisSync.Lib.Consumer.SituationSolver;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;

    using Moq;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture]
    public class RemoteObjectDeletedTest
    {
        private Mock<ISession> session;
        private Mock<IMetaDataStorage> storage;
        private RemoteObjectDeleted underTest;

        [SetUp]
        public void SetUp() {
            this.session = new Mock<ISession>();
            this.storage = new Mock<IMetaDataStorage>();
            this.underTest = new RemoteObjectDeleted(this.session.Object, this.storage.Object);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void DefaultConstructorTest()
        {
            new RemoteObjectDeleted(this.session.Object, this.storage.Object);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void RemoteFolderDeleted()
        {
            string path = Path.Combine(Path.GetTempPath(), "a");
            Mock<IMappedObject> folder = this.storage.AddLocalFolder(path, "id");
            var dirInfo = new Mock<IDirectoryInfo>();
            dirInfo.Setup(d => d.FullName).Returns(path);

            this.underTest.Solve(dirInfo.Object, null);

            dirInfo.Verify(d => d.Delete(false), Times.Once());
            this.storage.Verify(s => s.RemoveObject(It.Is<IMappedObject>(o => o == folder.Object)), Times.Once());
        }

        [Test, Category("Fast"), Category("Solver")]
        public void RemoteFileDeleted()
        {
            DateTime lastModified = DateTime.UtcNow;
            string path = Path.Combine(Path.GetTempPath(), "a");
            var fileInfo = new Mock<IFileInfo>();
            fileInfo.Setup(f => f.FullName).Returns(path);
            fileInfo.Setup(f => f.LastWriteTimeUtc).Returns(lastModified);
            var mappedObject = new MappedObject("a", "id", MappedObjectType.File, "parentId", "changeToken", 0) {
                LastLocalWriteTimeUtc = lastModified
            };
            this.storage.AddMappedFile(mappedObject, path);

            this.underTest.Solve(fileInfo.Object, null);

            fileInfo.Verify(f => f.Delete(), Times.Once());
            this.storage.Verify(s => s.RemoveObject(mappedObject), Times.Once());
        }

        [Test, Category("Fast"), Category("Solver")]
        public void RemoteFileDeletedButLocalFileHasBeenChangedBeforeBeingHandled()
        {
            string path = Path.Combine(Path.GetTempPath(), "a");
            Mock<IMappedObject> file = this.storage.AddLocalFile(path, "id");
            var fileInfo = new Mock<IFileInfo>();
            fileInfo.Setup(f => f.FullName).Returns(path);

            this.underTest.Solve(fileInfo.Object, null);

            fileInfo.Verify(f => f.Delete(), Times.Never());
            fileInfo.Verify(f => f.SetExtendedAttribute(MappedObject.ExtendedAttributeKey, null, true), Times.Once());
            this.storage.Verify(s => s.RemoveObject(It.Is<IMappedObject>(o => o == file.Object)), Times.Once());
        }

        [Test, Category("Fast"), Category("Solver")]
        public void RemoteFileDeletedButLocalFileDoesNotExistsInStorage()
        {
            string path = Path.Combine(Path.GetTempPath(), "a");
            var fileInfo = new Mock<IFileInfo>();
            fileInfo.Setup(f => f.FullName).Returns(path);

            this.underTest.Solve(fileInfo.Object, null);

            fileInfo.Verify(f => f.Delete(), Times.Never());
            this.storage.Verify(s => s.RemoveObject(It.IsAny<IMappedObject>()), Times.Never());
        }

        [Test, Category("Fast"), Category("Solver")]
        [ExpectedException(typeof(IOException))]
        public void RemoteFolderDeletedButNotAllContainingFilesAreSyncedYet()
        {
            string path = Path.Combine(Path.GetTempPath(), "a");
            string fileName = "fileName";
            string filePath = Path.Combine(path, fileName);
            string syncedFileName = "syncedFileName";
            string syncedFilePath = Path.Combine(path, syncedFileName);
            DateTime lastModified = DateTime.UtcNow;
            Guid syncedFileGuid = Guid.NewGuid();
            Mock<IMappedObject> folder = this.storage.AddLocalFolder(path, "id");
            var dirInfo = new Mock<IDirectoryInfo>();
            dirInfo.Setup(d => d.FullName).Returns(path);
            var fileInfo = new Mock<IFileInfo>();
            fileInfo.Setup(f => f.FullName).Returns(filePath);
            fileInfo.Setup(f => f.Name).Returns(fileName);
            var syncedFileInfo = new Mock<IFileInfo>();
            syncedFileInfo.Setup(s => s.FullName).Returns(syncedFilePath);
            syncedFileInfo.Setup(s => s.Name).Returns(syncedFileName);
            syncedFileInfo.Setup(s => s.GetExtendedAttribute(MappedObject.ExtendedAttributeKey)).Returns(syncedFileGuid.ToString());
            syncedFileInfo.Setup(s => s.LastWriteTimeUtc).Returns(lastModified);
            dirInfo.SetupFiles(fileInfo.Object, syncedFileInfo.Object);
            var mappedSyncedFile = new MappedObject(syncedFileName, "id", MappedObjectType.File, "parentId", "changeToken", 0) { Guid = syncedFileGuid, LastLocalWriteTimeUtc = lastModified };
            this.storage.AddMappedFile(mappedSyncedFile, syncedFilePath);

            try {
                this.underTest.Solve(dirInfo.Object, null);
            } catch (IOException) {
                dirInfo.Verify(d => d.Delete(true), Times.Never());
                syncedFileInfo.Verify(s => s.Delete(), Times.Once());
                fileInfo.Verify(f => f.Delete(), Times.Never());
                this.storage.Verify(s => s.RemoveObject(It.Is<IMappedObject>(o => o == folder.Object)), Times.Once());
                throw;
            }
        }
    }
}