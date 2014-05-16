//-----------------------------------------------------------------------
// <copyright file="RemoteObjectAddedTest.cs" company="GRAU DATA AG">
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
    public class RemoteObjectAddedTest
    {
        [Test, Category("Fast"), Category("Solver")]
        public void DefaultConstructorTest()
        {
            new RemoteObjectAdded();
        }

        [Test, Category("Fast"), Category("Solver")]
        public void RemoteFolderAdded()
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
            dirInfo.SetupAllProperties();
            dirInfo.Setup(d => d.FullName).Returns(path);
            dirInfo.Setup(d => d.Name).Returns(folderName);
            dirInfo.Setup(d => d.Parent).Returns(Mock.Of<IDirectoryInfo>());
            dirInfo.Setup(d => d.IsExtendedAttributeAvailable()).Returns(false);

            Mock<IFolder> remoteObject = MockSessionUtil.CreateRemoteFolderMock(id, path, parentId, lastChangeToken);
            remoteObject.Setup(f => f.LastModificationDate).Returns((DateTime?)creationDate);

            var solver = new RemoteObjectAdded();

            solver.Solve(session.Object, storage.Object, dirInfo.Object, remoteObject.Object);

            dirInfo.Verify(d => d.Create(), Times.Once());
            storage.VerifySavedMappedObject(MappedObjectType.Folder, id, folderName, parentId, lastChangeToken, false, creationDate);
            dirInfo.VerifySet(d => d.LastWriteTimeUtc = It.Is<DateTime>(date => date.Equals(creationDate)), Times.Once());
            dirInfo.Verify(d => d.SetExtendedAttribute(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [Test, Category("Fast"), Category("Solver")]
        public void RemoteFolderAddedAndExtendedAttributsAreAvailable()
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
            dirInfo.SetupAllProperties();
            dirInfo.Setup(d => d.FullName).Returns(path);
            dirInfo.Setup(d => d.Name).Returns(folderName);
            dirInfo.Setup(d => d.Parent).Returns(Mock.Of<IDirectoryInfo>());
            dirInfo.Setup(d => d.IsExtendedAttributeAvailable()).Returns(true);

            Mock<IFolder> remoteObject = MockSessionUtil.CreateRemoteFolderMock(id, path, parentId, lastChangeToken);
            remoteObject.Setup(f => f.LastModificationDate).Returns((DateTime?)creationDate);

            var solver = new RemoteObjectAdded();

            solver.Solve(session.Object, storage.Object, dirInfo.Object, remoteObject.Object);
            dirInfo.Verify(d => d.Create(), Times.Once());
            storage.VerifySavedMappedObject(MappedObjectType.Folder, id, folderName, parentId, lastChangeToken, true, creationDate);
            dirInfo.VerifySet(d => d.LastWriteTimeUtc = It.Is<DateTime>(date => date.Equals(creationDate)), Times.Once());
            dirInfo.Verify(d => d.SetExtendedAttribute(It.Is<string>(k => k.Equals(MappedObject.ExtendedAttributeKey)), It.IsAny<string>()), Times.Once());
        }
    }
}
