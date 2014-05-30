//-----------------------------------------------------------------------
// <copyright file="LocalObjectMovedTest.cs" company="GRAU DATA AG">
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
    public class LocalObjectMovedTest
    {
        [Test, Category("Fast"), Category("Solver")]
        public void DefaultConstructorTest()
        {
            new LocalObjectMoved();
        }

        [Test, Category("Fast"), Category("Solver")]
        public void MoveObjectToSubfolder()
        {
            var solver = new LocalObjectMoved();
            var storage = new Mock<IMetaDataStorage>();
            var session = new Mock<ISession>();
            var remoteFolder = MockOfIFolderUtil.CreateRemoteFolderMock("folderId", "folder", "/folder", "rootId");
            var targetFolder = MockOfIFolderUtil.CreateRemoteFolderMock("targetId", "target", "/target", "rootId");
            var rootFolder = MockOfIFolderUtil.CreateRemoteFolderMock("rootId", "/", "/", null);
            session.AddRemoteObject(remoteFolder.Object);
            session.AddRemoteObject(targetFolder.Object);
            session.AddRemoteObject(rootFolder.Object);
            var localRootFolder = MockOfIFileSystemInfoFactoryUtil.CreateLocalFolder(Path.GetTempPath());
            var localTargetFolder = MockOfIFileSystemInfoFactoryUtil.CreateLocalFolder(Path.Combine(Path.GetTempPath(), "target"));
            var localFolder = MockOfIFileSystemInfoFactoryUtil.CreateLocalFolder(Path.Combine(Path.GetTempPath(), "target", "folder"));
            localFolder.Setup(f => f.Parent).Returns(localTargetFolder.Object);
            var mappedRootFolder = new MappedObject("/", "rootId", MappedObjectType.Folder, null, "changeToken");
            var mappedFolder = new MappedObject("folder", "folderId", MappedObjectType.Folder, "rootId", "changeToken");
            var mappedTargetFolder = new MappedObject("target", "targetId", MappedObjectType.Folder, "rootId", "changeToken");
            storage.Setup(s => s.GetObjectByLocalPath(It.Is<IDirectoryInfo>(d => d.Equals(localRootFolder.Object)))).Returns(mappedRootFolder);
            storage.Setup(s => s.GetObjectByLocalPath(It.Is<IDirectoryInfo>(d => d.Equals(localTargetFolder.Object)))).Returns(mappedTargetFolder);
            storage.Setup(s => s.GetObjectByRemoteId("folderId")).Returns(mappedFolder);

            solver.Solve(session.Object, storage.Object, localFolder.Object, remoteFolder.Object);

            remoteFolder.Verify(f => f.Move(rootFolder.Object, targetFolder.Object), Times.Once());
        }

        [Test, Category("Fast"), Category("Solver")]
        public void MoveObjectFromSubFolder()
        {
            var solver = new LocalObjectMoved();
            var storage = new Mock<IMetaDataStorage>();
            var session = new Mock<ISession>();
            var remoteFolder = MockOfIFolderUtil.CreateRemoteFolderMock("folderId", "folder", "/sub/folder", "subId");
            var subFolder = MockOfIFolderUtil.CreateRemoteFolderMock("subId", "sub", "/sub", "rootId");
            var rootFolder = MockOfIFolderUtil.CreateRemoteFolderMock("rootId", "/", "/", null);
            session.AddRemoteObject(remoteFolder.Object);
            session.AddRemoteObject(subFolder.Object);
            session.AddRemoteObject(rootFolder.Object);
            var localRootFolder = MockOfIFileSystemInfoFactoryUtil.CreateLocalFolder(Path.GetTempPath());
            var localSubFolder = MockOfIFileSystemInfoFactoryUtil.CreateLocalFolder(Path.Combine(Path.GetTempPath(), "sub"));
            var localFolder = MockOfIFileSystemInfoFactoryUtil.CreateLocalFolder(Path.Combine(Path.GetTempPath(), "folder"));
            localFolder.Setup(f => f.Parent).Returns(localRootFolder.Object);
            var mappedRootFolder = new MappedObject("/", "rootId", MappedObjectType.Folder, null, "changeToken");
            var mappedFolder = new MappedObject("folder", "folderId", MappedObjectType.Folder, "subId", "changeToken");
            var mappedSubFolder = new MappedObject("sub", "subId", MappedObjectType.Folder, "rootId", "changeToken");
            storage.Setup(s => s.GetObjectByLocalPath(It.Is<IDirectoryInfo>(d => d.Equals(localRootFolder.Object)))).Returns(mappedRootFolder);
            storage.Setup(s => s.GetObjectByLocalPath(It.Is<IDirectoryInfo>(d => d.Equals(localSubFolder.Object)))).Returns(mappedSubFolder);
            storage.Setup(s => s.GetObjectByRemoteId("folderId")).Returns(mappedFolder);

            solver.Solve(session.Object, storage.Object, localFolder.Object, remoteFolder.Object);

            remoteFolder.Verify(f => f.Move(subFolder.Object, rootFolder.Object), Times.Once());
        }
    }
}