//-----------------------------------------------------------------------
// <copyright file="RemoteObjectChangedTest.cs" company="GRAU DATA AG">
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
    public class RemoteObjectChangedTest
    {
        [Test, Category("Fast"), Category("Solver")]
        public void DefaultConstructorTest()
        {
            new RemoteObjectChanged();
        }

        [Ignore]
        [Test, Category("Fast"), Category("Solver")]
        public void RemoteFolderChanged()
        {
            DateTime creationDate = DateTime.UtcNow;
            string folderName = "a";
            string path = Path.Combine(Path.GetTempPath(), folderName);
            string id = "id";
            string parentId = "papa";
            string lastChangeToken = "token";

            var session = new Mock<ISession>();

            var storage = new Mock<IMetaDataStorage>();

            var dirInfo = new Mock<IDirectoryInfo>();
            dirInfo.Setup(d => d.FullName).Returns(path);
            dirInfo.Setup(d => d.Name).Returns(folderName);
            dirInfo.Setup(d => d.Parent).Returns(Mock.Of<IDirectoryInfo>());

            storage.AddLocalFolder(dirInfo.Object, id);

            Mock<IFolder> remoteObject = MockOfIFolderUtil.CreateRemoteFolderMock(id, folderName, path, parentId, lastChangeToken);
            remoteObject.Setup(f => f.LastModificationDate).Returns((DateTime?)creationDate);

            var solver = new RemoteObjectChanged();

            solver.Solve(session.Object, storage.Object, dirInfo.Object, remoteObject.Object);

            storage.Verify(
                s => s.SaveMappedObject(
                It.Is<IMappedObject>(f =>
                                 f.RemoteObjectId == id &&
                                 f.Name == folderName &&
                                 f.ParentId == parentId &&
                                 f.LastChangeToken == lastChangeToken &&
                                 f.LastRemoteWriteTimeUtc == creationDate &&
                                 f.Type == MappedObjectType.Folder)),
                Times.Once());
            dirInfo.VerifySet(d => d.LastWriteTimeUtc = It.Is<DateTime>(date => date.Equals(creationDate)), Times.Once());
        }
    }
}