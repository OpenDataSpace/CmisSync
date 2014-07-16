//-----------------------------------------------------------------------
// <copyright file="RemoteObjectMovedTest.cs" company="GRAU DATA AG">
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
    using System.Collections.Generic;
    using System.IO;

    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.PathMatcher;
    using CmisSync.Lib.Storage.FileSystem;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Consumer.SituationSolver;

    using DotCMIS.Client;

    using Moq;

    using NUnit.Framework;

    using TestLibrary.TestUtils;
    [TestFixture]
    public class RemoteObjectMovedTest
    {
        [Test, Category("Fast"), Category("Solver")]
        public void DefaultConstructorTest()
        {
            new RemoteObjectMoved();
        }

        [Test, Category("Fast"), Category("Solver")]
        public void MoveFolderToNewLocation()
        {
            DateTime modifiedDate = DateTime.UtcNow.AddMinutes(1);
            string oldFolderName = "a";
            string subFolderName = "sub";
            string subFolderId = "sub";
            string newFolderName = "b";
            string oldPath = Path.Combine(Path.GetTempPath(), oldFolderName);
            string oldRemotePath = "/" + oldFolderName;
            string newPath = Path.Combine(Path.GetTempPath(), subFolderName, newFolderName);
            string id = "id";
            string parentId = "papa";
            string lastChangeToken = "token";

            var session = new Mock<ISession>();
            var matcher = new Mock<IPathMatcher>();
            var storage = new Mock<IMetaDataStorage>();
            storage.Setup(s => s.Matcher).Returns(matcher.Object);

            var dirInfo = new Mock<IDirectoryInfo>();
            dirInfo.Setup(d => d.FullName).Returns(oldPath);
            dirInfo.Setup(d => d.Name).Returns(oldFolderName);
            dirInfo.Setup(d => d.Parent).Returns(Mock.Of<IDirectoryInfo>(p => p.FullName == Path.GetTempPath()));

            Mock<IFolder> remoteObject = MockOfIFolderUtil.CreateRemoteFolderMock(id, newFolderName, newPath, subFolderId, lastChangeToken);
            remoteObject.Setup(f => f.LastModificationDate).Returns((DateTime?)modifiedDate);

            var mappedFolder = Mock.Of<IMappedObject>(
                f =>
                f.Name == oldFolderName &&
                f.RemoteObjectId == id &&
                f.LastChangeToken == "oldToken" &&
                f.LastRemoteWriteTimeUtc == DateTime.UtcNow &&
                f.Type == MappedObjectType.Folder &&
                f.ParentId == parentId);
            var mappedSubFolder = Mock.Of<IMappedObject>(
                f =>
                f.Name == subFolderName &&
                f.RemoteObjectId == subFolderId &&
                f.LastChangeToken == "oldToken" &&
                f.Type == MappedObjectType.Folder &&
                f.ParentId == parentId);
            storage.AddMappedFolder(mappedFolder, oldPath, oldRemotePath);
            storage.AddMappedFolder(mappedSubFolder, Path.Combine(Path.GetTempPath(), subFolderName), "/" + subFolderName);
            matcher.Setup(m => m.CreateLocalPath(It.Is<IFolder>(f => f == remoteObject.Object))).Returns(newPath);
            var solver = new RemoteObjectMoved();

            solver.Solve(session.Object, storage.Object, dirInfo.Object, remoteObject.Object);

            dirInfo.Verify(d => d.MoveTo(It.Is<string>(p => p.Equals(newPath))), Times.Once());

            storage.Verify(
                s => s.SaveMappedObject(
                It.Is<IMappedObject>(f => this.VerifySavedFolder(f, MappedObjectType.Folder, id, newFolderName, subFolderId, lastChangeToken, modifiedDate))),
                Times.Once());
        }

        [Test, Category("Fast"), Category("Solver")]
        public void MoveFileToNewLocation()
        {
            DateTime modifiedDate = DateTime.UtcNow.AddMinutes(1);
            string oldFileName = "a";
            string subFolderName = "sub";
            string subFolderId = "sub";
            string newFileName = "b";
            string oldPath = Path.Combine(Path.GetTempPath(), oldFileName);
            string oldRemotePath = "/" + oldFileName;
            string newPath = Path.Combine(Path.GetTempPath(), subFolderName, newFileName);
            string id = "id";
            string parentId = "papa";
            string lastChangeToken = "token";

            var session = new Mock<ISession>();
            var matcher = new Mock<IPathMatcher>();
            var storage = new Mock<IMetaDataStorage>();
            storage.Setup(s => s.Matcher).Returns(matcher.Object);

            var fileInfo = new Mock<IFileInfo>();
            fileInfo.Setup(d => d.FullName).Returns(oldPath);
            fileInfo.Setup(d => d.Name).Returns(oldFileName);
            fileInfo.Setup(d => d.Directory).Returns(Mock.Of<IDirectoryInfo>(p => p.FullName == Path.GetTempPath()));

            var fileParents = new List<IFolder>();
            fileParents.Add(Mock.Of<IFolder>(f => f.Id == subFolderId));
            Mock<IDocument> remoteObject = MockOfIDocumentUtil.CreateRemoteDocumentMock(null, id, newFileName, (string)null, changeToken: lastChangeToken);
            remoteObject.Setup(f => f.Parents).Returns(fileParents);
            remoteObject.Setup(f => f.LastModificationDate).Returns((DateTime?)modifiedDate);

            var mappedFile = Mock.Of<IMappedObject>(
                f =>
                f.Name == oldFileName &&
                f.RemoteObjectId == id &&
                f.LastChangeToken == "oldToken" &&
                f.LastRemoteWriteTimeUtc == DateTime.UtcNow &&
                f.Type == MappedObjectType.File &&
                f.ParentId == parentId);
            var mappedSubFolder = Mock.Of<IMappedObject>(
                f =>
                f.Name == subFolderName &&
                f.RemoteObjectId == subFolderId &&
                f.LastChangeToken == "oldToken" &&
                f.Type == MappedObjectType.Folder &&
                f.ParentId == parentId);
            storage.AddMappedFolder(mappedFile, oldPath, oldRemotePath);
            storage.AddMappedFolder(mappedSubFolder, Path.Combine(Path.GetTempPath(), subFolderName), "/" + subFolderName);
            matcher.Setup(m => m.CreateLocalPath(It.Is<IDocument>(f => f == remoteObject.Object))).Returns(newPath);
            var solver = new RemoteObjectMoved();

            solver.Solve(session.Object, storage.Object, fileInfo.Object, remoteObject.Object);

            fileInfo.Verify(d => d.MoveTo(It.Is<string>(p => p.Equals(newPath))), Times.Once());

            storage.Verify(
                s => s.SaveMappedObject(
                It.Is<IMappedObject>(f => this.VerifySavedFolder(f, MappedObjectType.File, id, newFileName, subFolderId, lastChangeToken, modifiedDate))),
                Times.Once());
        }

        [Test, Category("Fast"), Category("Solver")]
        public void DoNotMoveFolderToSameLocation()
        {
            DateTime modifiedDate = DateTime.UtcNow.AddMinutes(1);
            string oldFolderName = "a";
            string subFolderName = "sub";
            string subFolderId = "sub";
            string newFolderName = "b";
            string oldPath = Path.Combine(Path.GetTempPath(), oldFolderName);
            string oldRemotePath = "/" + oldFolderName;
            string id = "id";
            string parentId = "papa";
            string lastChangeToken = "token";

            var session = new Mock<ISession>();
            var matcher = new Mock<IPathMatcher>();
            var storage = new Mock<IMetaDataStorage>();
            storage.Setup(s => s.Matcher).Returns(matcher.Object);

            var dirInfo = new Mock<IDirectoryInfo>();
            dirInfo.Setup(d => d.FullName).Returns(oldPath);
            dirInfo.Setup(d => d.Name).Returns(oldFolderName);
            dirInfo.Setup(d => d.Parent).Returns(Mock.Of<IDirectoryInfo>(p => p.FullName == Path.GetTempPath()));

            Mock<IFolder> remoteObject = MockOfIFolderUtil.CreateRemoteFolderMock(id, newFolderName, oldPath, subFolderId, lastChangeToken);
            remoteObject.Setup(f => f.LastModificationDate).Returns((DateTime?)modifiedDate);

            var mappedFolder = Mock.Of<IMappedObject>(
                f =>
                f.Name == oldFolderName &&
                f.RemoteObjectId == id &&
                f.LastChangeToken == "oldToken" &&
                f.LastRemoteWriteTimeUtc == DateTime.UtcNow &&
                f.Type == MappedObjectType.Folder &&
                f.ParentId == parentId);
            var mappedSubFolder = Mock.Of<IMappedObject>(
                f =>
                f.Name == subFolderName &&
                f.RemoteObjectId == subFolderId &&
                f.LastChangeToken == "oldToken" &&
                f.Type == MappedObjectType.Folder &&
                f.ParentId == parentId);
            storage.AddMappedFolder(mappedFolder, oldPath, oldRemotePath);
            storage.AddMappedFolder(mappedSubFolder, Path.Combine(Path.GetTempPath(), subFolderName), "/" + subFolderName);
            matcher.Setup(m => m.CreateLocalPath(It.Is<IFolder>(f => f == remoteObject.Object))).Returns(oldPath);
            var solver = new RemoteObjectMoved();

            solver.Solve(session.Object, storage.Object, dirInfo.Object, remoteObject.Object);

            dirInfo.Verify(d => d.MoveTo(It.IsAny<string>()), Times.Never());

            storage.Verify(s => s.SaveMappedObject(It.IsAny<IMappedObject>()), Times.Never());
        }

        private bool VerifySavedFolder(IMappedObject folder, MappedObjectType type, string id, string name, string parentId, string changeToken, DateTime modifiedTime)
        {
            Assert.That(folder.Type, Is.EqualTo(type));
            Assert.That(folder.RemoteObjectId, Is.EqualTo(id));
            Assert.That(folder.ParentId, Is.EqualTo(parentId));
            Assert.That(folder.Name, Is.EqualTo(name));
            Assert.That(folder.LastChangeToken, Is.EqualTo(changeToken));
            Assert.That(folder.LastRemoteWriteTimeUtc, Is.EqualTo(modifiedTime));
            return true;
        }
    }
}