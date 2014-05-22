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

namespace TestLibrary.SyncStrategiesTests.SolverTests
{
    using System;
    using System.IO;

    using CmisSync.Lib.Data;
    using CmisSync.Lib.Storage;
    using CmisSync.Lib.Sync.Solver;

    using DotCMIS.Client;

    using Moq;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture]
    public class RemoteObjectRenamedTest
    {
        [Test, Category("Fast"), Category("Solver")]
        public void DefaultConstructorTest()
        {
            new RemoteObjectRenamed();
        }

        [Test, Category("Fast"), Category("Solver")]
        public void RenameFolder()
        {
            DateTime modifiedDate = DateTime.UtcNow.AddMinutes(1);
            string oldFolderName = "a";
            string newFolderName = "b";
            string oldPath = Path.Combine(Path.GetTempPath(), oldFolderName);
            string oldRemotePath = "/" + oldFolderName;
            string newPath = Path.Combine(Path.GetTempPath(), newFolderName);
            string id = "id";
            string parentId = "papa";
            string lastChangeToken = "token";

            var session = new Mock<ISession>();

            var storage = new Mock<IMetaDataStorage>();

            var dirInfo = new Mock<IDirectoryInfo>();
            dirInfo.Setup(d => d.FullName).Returns(oldPath);
            dirInfo.Setup(d => d.Name).Returns(oldFolderName);
            dirInfo.SetupProperty(d => d.LastWriteTimeUtc);
            dirInfo.Setup(d => d.Parent).Returns(Mock.Of<IDirectoryInfo>(p => p.FullName == Path.GetTempPath()));

            Mock<IFolder> remoteObject = MockSessionUtil.CreateRemoteFolderMock(id, newPath, parentId, lastChangeToken);
            remoteObject.Setup(f => f.LastModificationDate).Returns((DateTime?)modifiedDate);

            var mappedFolder = Mock.Of<IMappedObject>(
                f =>
                f.Name == oldFolderName &&
                f.RemoteObjectId == id &&
                f.LastChangeToken == "oldToken" &&
                f.LastRemoteWriteTimeUtc == DateTime.UtcNow &&
                f.Type == MappedObjectType.Folder &&
                f.ParentId == parentId);
            storage.AddMappedFolder(mappedFolder, oldPath, oldRemotePath);

            var solver = new RemoteObjectRenamed();

            solver.Solve(session.Object, storage.Object, dirInfo.Object, remoteObject.Object);

            dirInfo.Verify(d => d.MoveTo(It.Is<string>(p => p.Equals(newPath))), Times.Once());

            dirInfo.VerifySet(d => d.LastWriteTimeUtc = It.Is<DateTime>(date => date.Equals(modifiedDate)), Times.Once());
            storage.Verify(
                s => s.SaveMappedObject(
                It.Is<IMappedObject>(f => this.VerifySavedObject(f, MappedObjectType.Folder, id, newFolderName, parentId, lastChangeToken, modifiedDate))),
                Times.Once());
        }

        [Test, Category("Fast")]
        public void RenameFile()
        {
            DateTime modifiedDate = DateTime.UtcNow.AddMinutes(1);
            string oldFileName = "a";
            string newFileName = "b";
            string oldPath = Path.Combine(Path.GetTempPath(), oldFileName);
            string oldRemotePath = "/" + oldFileName;
            string newPath = Path.Combine(Path.GetTempPath(), newFileName);
            string id = "id";
            string parentId = "root";
            string lastChangeToken = "token";
            var storage = new Mock<IMetaDataStorage>();
            var fileInfo = new Mock<IFileInfo>();
            fileInfo.Setup(f => f.FullName).Returns(oldPath);
            fileInfo.Setup(f => f.Name).Returns(oldFileName);
            fileInfo.SetupProperty(f => f.LastWriteTimeUtc);
            fileInfo.Setup(f => f.Directory).Returns(Mock.Of<IDirectoryInfo>(p => p.FullName == Path.GetTempPath()));

            var remoteObject = MockSessionUtil.CreateRemoteDocumentMock(null, id, newFileName, lastChangeToken);
            remoteObject.Setup(f => f.LastModificationDate).Returns((DateTime?)modifiedDate);

            var mappedFile = Mock.Of<IMappedObject>(
                f =>
                f.Name == oldFileName &&
                f.RemoteObjectId == id &&
                f.LastChangeToken == "oldToken" &&
                f.LastRemoteWriteTimeUtc == DateTime.UtcNow &&
                f.Type == MappedObjectType.File &&
                f.ParentId == parentId);
            storage.AddMappedFolder(mappedFile, oldPath, oldRemotePath);

            var solver = new RemoteObjectRenamed();

            solver.Solve(null, storage.Object, fileInfo.Object, remoteObject.Object);

            fileInfo.Verify(d => d.MoveTo(It.Is<string>(p => p.Equals(newPath))), Times.Once());

            fileInfo.VerifySet(d => d.LastWriteTimeUtc = It.Is<DateTime>(date => date.Equals(modifiedDate)), Times.Once());
            storage.Verify(
                s => s.SaveMappedObject(
                It.Is<IMappedObject>(f => this.VerifySavedObject(f, MappedObjectType.File, id, newFileName, parentId, lastChangeToken, modifiedDate))),
                Times.Once());
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
    }
}