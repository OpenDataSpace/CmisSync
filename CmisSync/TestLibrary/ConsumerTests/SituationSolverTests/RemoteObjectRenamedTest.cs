//-----------------------------------------------------------------------
// <copyright file="RemoteObjectRenamedTest.cs" company="GRAU DATA AG">
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
    public class RemoteObjectRenamedTest {
        private Mock<ISession> session;
        private Mock<IMetaDataStorage> storage;
        private RemoteObjectRenamed underTest;

        [Test, Category("Fast"), Category("Solver")]
        public void DefaultConstructorTest() {
            var session = new Mock<ISession>();
            session.SetupTypeSystem();
            new RemoteObjectRenamed(session.Object, Mock.Of<IMetaDataStorage>());
        }

        [Test, Category("Fast"), Category("Solver")]
        public void RenameFolder([Values(true, false)]bool childrenAreIgnored) {
            this.SetUpMocks();
            DateTime modifiedDate = DateTime.UtcNow.AddMinutes(1);
            string oldFolderName = "a";
            string newFolderName = "b";
            string oldPath = Path.Combine(Path.GetTempPath(), oldFolderName);
            string oldRemotePath = "/" + oldFolderName;
            string newPath = Path.Combine(Path.GetTempPath(), newFolderName);
            string id = "id";
            string parentId = "papa";
            string lastChangeToken = "token";

            var dirInfo = new Mock<IDirectoryInfo>();
            dirInfo.Setup(d => d.FullName).Returns(oldPath);
            dirInfo.Setup(d => d.Name).Returns(oldFolderName);
            dirInfo.SetupProperty(d => d.LastWriteTimeUtc);
            dirInfo.Setup(d => d.Parent).Returns(Mock.Of<IDirectoryInfo>(p => p.FullName == Path.GetTempPath()));

            Mock<IFolder> remoteObject = MockOfIFolderUtil.CreateRemoteFolderMock(id, newFolderName, newPath, parentId, lastChangeToken, childrenAreIgnored);
            remoteObject.Setup(f => f.LastModificationDate).Returns((DateTime?)modifiedDate);

            var mappedFolder = Mock.Of<IMappedObject>(
                f =>
                f.Name == oldFolderName &&
                f.RemoteObjectId == id &&
                f.LastChangeToken == "oldToken" &&
                f.LastRemoteWriteTimeUtc == DateTime.UtcNow &&
                f.Type == MappedObjectType.Folder &&
                f.ParentId == parentId &&
                f.Guid == Guid.NewGuid() &&
                f.LastContentSize == -1);
            this.storage.AddMappedFolder(mappedFolder, oldPath, oldRemotePath);

            this.underTest.Solve(dirInfo.Object, remoteObject.Object);

            dirInfo.Verify(d => d.MoveTo(It.Is<string>(p => p.Equals(newPath))), Times.Once());

            dirInfo.VerifySet(d => d.LastWriteTimeUtc = It.Is<DateTime>(date => date.Equals(modifiedDate)), Times.Once());
            this.storage.VerifySavedMappedObject(MappedObjectType.Folder, id, newFolderName, parentId, lastChangeToken, lastLocalModification: modifiedDate, lastRemoteModification: modifiedDate, ignored: childrenAreIgnored);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void RenameFolderToLowerCaseAndIOExceptionIsHandled([Values(true, false)]bool childrenAreIgnored) {
            this.SetUpMocks();
            DateTime modifiedDate = DateTime.UtcNow.AddMinutes(1);
            string oldFolderName = "A";
            string newFolderName = "a";
            string oldPath = Path.Combine(Path.GetTempPath(), oldFolderName);
            string oldRemotePath = "/" + oldFolderName;
            string newPath = Path.Combine(Path.GetTempPath(), newFolderName);
            string id = "id";
            string parentId = "papa";
            string lastChangeToken = "token";

            var dirInfo = new Mock<IDirectoryInfo>();
            dirInfo.Setup(d => d.FullName).Returns(oldPath);
            dirInfo.Setup(d => d.Name).Returns(oldFolderName);
            dirInfo.SetupProperty(d => d.LastWriteTimeUtc);
            dirInfo.Setup(d => d.Parent).Returns(Mock.Of<IDirectoryInfo>(p => p.FullName == Path.GetTempPath()));

            Mock<IFolder> remoteObject = MockOfIFolderUtil.CreateRemoteFolderMock(id, newFolderName, newPath, parentId, lastChangeToken, childrenAreIgnored);
            remoteObject.Setup(f => f.LastModificationDate).Returns((DateTime?)modifiedDate);

            var mappedFolder = Mock.Of<IMappedObject>(
                f =>
                f.Name == oldFolderName &&
                f.RemoteObjectId == id &&
                f.LastChangeToken == "oldToken" &&
                f.LastRemoteWriteTimeUtc == DateTime.UtcNow &&
                f.Type == MappedObjectType.Folder &&
                f.ParentId == parentId &&
                f.LastContentSize == -1 &&
                f.Guid == Guid.NewGuid());
            this.storage.AddMappedFolder(mappedFolder, oldPath, oldRemotePath);
            dirInfo.Setup(d => d.MoveTo(It.Is<string>(p => p.Equals(newPath)))).Throws<IOException>();

            this.underTest.Solve(dirInfo.Object, remoteObject.Object);

            dirInfo.Verify(d => d.MoveTo(It.Is<string>(p => p.Equals(newPath))), Times.Once());

            dirInfo.VerifySet(d => d.LastWriteTimeUtc = It.Is<DateTime>(date => date.Equals(modifiedDate)), Times.Once());
            this.storage.VerifySavedMappedObject(MappedObjectType.Folder, id, oldFolderName, parentId, lastChangeToken, lastLocalModification: modifiedDate, lastRemoteModification: modifiedDate, ignored: childrenAreIgnored);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void RenameFolderAndLocalIOExceptionIsThrownOnMove() {
            this.SetUpMocks();
            DateTime modifiedDate = DateTime.UtcNow.AddMinutes(1);
            string oldFolderName = "a";
            string newFolderName = "b";
            string oldPath = Path.Combine(Path.GetTempPath(), oldFolderName);
            string oldRemotePath = "/" + oldFolderName;
            string newPath = Path.Combine(Path.GetTempPath(), newFolderName);
            string id = "id";
            string parentId = "papa";
            string lastChangeToken = "token";

            var dirInfo = new Mock<IDirectoryInfo>();
            dirInfo.Setup(d => d.FullName).Returns(oldPath);
            dirInfo.Setup(d => d.Name).Returns(oldFolderName);
            dirInfo.SetupProperty(d => d.LastWriteTimeUtc);
            dirInfo.Setup(d => d.Parent).Returns(Mock.Of<IDirectoryInfo>(p => p.FullName == Path.GetTempPath()));

            Mock<IFolder> remoteObject = MockOfIFolderUtil.CreateRemoteFolderMock(id, newFolderName, newPath, parentId, lastChangeToken);
            remoteObject.Setup(f => f.LastModificationDate).Returns((DateTime?)modifiedDate);

            var mappedFolder = Mock.Of<IMappedObject>(
                f =>
                f.Name == oldFolderName &&
                f.RemoteObjectId == id &&
                f.LastChangeToken == "oldToken" &&
                f.LastRemoteWriteTimeUtc == DateTime.UtcNow &&
                f.Type == MappedObjectType.Folder &&
                f.ParentId == parentId);
            this.storage.AddMappedFolder(mappedFolder, oldPath, oldRemotePath);
            dirInfo.Setup(d => d.MoveTo(It.Is<string>(p => p.Equals(newPath)))).Throws<IOException>();

            Assert.Throws<IOException>(() => this.underTest.Solve(dirInfo.Object, remoteObject.Object));

            dirInfo.Verify(d => d.MoveTo(It.Is<string>(p => p.Equals(newPath))), Times.Once());

            dirInfo.VerifySet(d => d.LastWriteTimeUtc = It.Is<DateTime>(date => date.Equals(modifiedDate)), Times.Never());
            this.storage.VerifyThatNoObjectIsManipulated();
        }

        [Test, Category("Fast")]
        public void RenameFile() {
            this.SetUpMocks();
            DateTime modifiedDate = DateTime.UtcNow.AddMinutes(1);
            string oldFileName = "a";
            string newFileName = "b";
            string oldPath = Path.Combine(Path.GetTempPath(), oldFileName);
            string oldRemotePath = "/" + oldFileName;
            string newPath = Path.Combine(Path.GetTempPath(), newFileName);
            string id = "id";
            string parentId = "root";
            string lastChangeToken = "token";
            long fileSize = 1234567890;
            var fileInfo = new Mock<IFileInfo>();
            fileInfo.Setup(f => f.FullName).Returns(oldPath);
            fileInfo.Setup(f => f.Name).Returns(oldFileName);
            fileInfo.SetupProperty(f => f.LastWriteTimeUtc);
            fileInfo.Setup(f => f.Directory).Returns(Mock.Of<IDirectoryInfo>(p => p.FullName == Path.GetTempPath()));

            var remoteObject = MockOfIDocumentUtil.CreateRemoteDocumentMock(null, id, newFileName, (string)null, changeToken: lastChangeToken);
            remoteObject.Setup(f => f.LastModificationDate).Returns((DateTime?)modifiedDate);

            var mappedFile = Mock.Of<IMappedObject>(
                f =>
                f.Name == oldFileName &&
                f.RemoteObjectId == id &&
                f.LastChangeToken == "oldToken" &&
                f.LastRemoteWriteTimeUtc == DateTime.UtcNow &&
                f.Type == MappedObjectType.File &&
                f.ParentId == parentId &&
                f.Guid == Guid.NewGuid() &&
                f.LastContentSize == fileSize);
            this.storage.AddMappedFolder(mappedFile, oldPath, oldRemotePath);

            this.underTest.Solve(fileInfo.Object, remoteObject.Object);

            fileInfo.Verify(d => d.MoveTo(It.Is<string>(p => p.Equals(newPath))), Times.Once());

            fileInfo.VerifySet(d => d.LastWriteTimeUtc = It.Is<DateTime>(date => date.Equals(modifiedDate)), Times.Once());
            this.storage.VerifySavedMappedObject(MappedObjectType.File, id, newFileName, parentId, lastChangeToken, true, modifiedDate, modifiedDate, contentSize: fileSize);
        }

        private bool VerifySavedObject(IMappedObject obj, MappedObjectType type, string id, string name, string parentId, string changeToken, DateTime modifiedTime)
        {
            Assert.That(obj.Type, Is.EqualTo(type));
            Assert.That(obj.RemoteObjectId, Is.EqualTo(id));
            Assert.That(obj.ParentId, Is.EqualTo(parentId));
            Assert.That(obj.Name, Is.EqualTo(name));
            Assert.That(obj.LastChangeToken, Is.EqualTo(changeToken));
            Assert.That(obj.LastRemoteWriteTimeUtc, Is.EqualTo(modifiedTime));
            Assert.That(obj.LastLocalWriteTimeUtc, Is.EqualTo(modifiedTime));
            return true;
        }

        private void SetUpMocks() {
            this.session = new Mock<ISession>();
            this.session.SetupTypeSystem();
            this.storage = new Mock<IMetaDataStorage>();
            this.underTest = new RemoteObjectRenamed(this.session.Object, this.storage.Object);
        }
    }
}