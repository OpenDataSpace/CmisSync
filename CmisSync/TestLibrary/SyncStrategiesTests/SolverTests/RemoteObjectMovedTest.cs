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

            var storage = new Mock<IMetaDataStorage>();

            var dirInfo = new Mock<IDirectoryInfo>();
            dirInfo.Setup(d => d.FullName).Returns(oldPath);
            dirInfo.Setup(d => d.Name).Returns(oldFolderName);
            dirInfo.Setup(d => d.Parent).Returns(Mock.Of<IDirectoryInfo>(p => p.FullName == Path.GetTempPath()));

            Mock<IFolder> remoteObject = MockSessionUtil.CreateRemoteFolderMock(id, newPath, subFolderId, lastChangeToken);
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

            var solver = new RemoteObjectMoved();

            solver.Solve(session.Object, storage.Object, dirInfo.Object, remoteObject.Object);

            dirInfo.Verify(d => d.MoveTo(It.Is<string>(p => p.Equals(newPath))), Times.Once());

            storage.Verify(
                s => s.SaveMappedObject(
                It.Is<IMappedObject>(f => this.VerifySavedFolder(f, id, newFolderName, subFolderId, lastChangeToken, modifiedDate))),
                Times.Once());
        }

        private bool VerifySavedFolder(IMappedObject folder, string id, string name, string parentId, string changeToken, DateTime modifiedTime)
        {
            Assert.That(folder.Type, Is.EqualTo(MappedObjectType.Folder));
            Assert.That(folder.RemoteObjectId, Is.EqualTo(id));
            Assert.That(folder.ParentId, Is.EqualTo(parentId));
            Assert.That(folder.Name, Is.EqualTo(name));
            Assert.That(folder.LastChangeToken, Is.EqualTo(changeToken));
            Assert.That(folder.LastRemoteWriteTimeUtc, Is.EqualTo(modifiedTime));
            return true;
        }
    }
}