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
    public class LocalObjectMovedTest
    {
        private readonly string rootId = "rootId";
        private Mock<ISession> session;
        private Mock<IMetaDataStorage> storage;
        private Mock<IFolder> remoteRootFolder;
        private Mock<IDirectoryInfo> localRootFolder;
        private MappedObject mappedRootFolder;
        private LocalObjectMoved underTest;

        [SetUp]
        public void SetUp() {
            this.storage = new Mock<IMetaDataStorage>();
            this.session = new Mock<ISession>();
            this.remoteRootFolder = MockOfIFolderUtil.CreateRemoteFolderMock(this.rootId, "/", "/", null);
            this.session.AddRemoteObject(this.remoteRootFolder.Object);
            this.localRootFolder = MockOfIFileSystemInfoFactoryUtil.CreateLocalFolder(Path.GetTempPath());
            this.mappedRootFolder = new MappedObject("/", this.rootId, MappedObjectType.Folder, null, "changeToken") { Guid = Guid.NewGuid() };
            this.storage.Setup(s => s.GetObjectByLocalPath(It.Is<IDirectoryInfo>(d => d.Equals(this.localRootFolder.Object)))).Returns(this.mappedRootFolder);
            this.underTest = new LocalObjectMoved(this.session.Object, this.storage.Object);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void DefaultConstructorTest()
        {
            new LocalObjectMoved(this.session.Object, this.storage.Object);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void MoveObjectToSubfolder()
        {
            var remoteFolder = MockOfIFolderUtil.CreateRemoteFolderMock("folderId", "folder", "/folder", this.rootId);
            var remoteTargetFolder = MockOfIFolderUtil.CreateRemoteFolderMock("targetId", "target", "/target", this.rootId);
            this.session.AddRemoteObjects(remoteFolder.Object, remoteTargetFolder.Object);
            var localTargetFolder = MockOfIFileSystemInfoFactoryUtil.CreateLocalFolder(Path.Combine(Path.GetTempPath(), "target"));
            var localFolder = MockOfIFileSystemInfoFactoryUtil.CreateLocalFolder(Path.Combine(Path.GetTempPath(), "target", "folder"));
            localFolder.Setup(f => f.Parent).Returns(localTargetFolder.Object);
            var mappedFolder = new MappedObject("folder", "folderId", MappedObjectType.Folder, this.rootId, "changeToken") { Guid = Guid.NewGuid() };
            var mappedTargetFolder = new MappedObject("target", "targetId", MappedObjectType.Folder, this.rootId, "changeToken") { Guid = Guid.NewGuid() };
            this.storage.Setup(s => s.GetObjectByLocalPath(It.Is<IDirectoryInfo>(d => d.Equals(localTargetFolder.Object)))).Returns(mappedTargetFolder);
            this.storage.Setup(s => s.GetObjectByRemoteId("folderId")).Returns(mappedFolder);
            remoteFolder.Setup(f => f.Move(this.remoteRootFolder.Object, remoteTargetFolder.Object)).Callback(() => { remoteFolder.Setup(r => r.ChangeToken).Returns("changeToken"); }).Returns(remoteFolder.Object);

            this.underTest.Solve(localFolder.Object, remoteFolder.Object);

            remoteFolder.Verify(f => f.Move(this.remoteRootFolder.Object, remoteTargetFolder.Object), Times.Once());
            remoteFolder.VerifyUpdateLastModificationDate(localFolder.Object.LastWriteTimeUtc);
            this.storage.VerifySavedMappedObject(MappedObjectType.Folder, "folderId", "folder", "targetId", "changeToken", true);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void MoveObjectFromSubFolder()
        {
            var remoteFolder = MockOfIFolderUtil.CreateRemoteFolderMock("folderId", "folder", "/sub/folder", "subId");
            var subFolder = MockOfIFolderUtil.CreateRemoteFolderMock("subId", "sub", "/sub", this.rootId);
            this.session.AddRemoteObjects(remoteFolder.Object, subFolder.Object);
            var localSubFolder = MockOfIFileSystemInfoFactoryUtil.CreateLocalFolder(Path.Combine(Path.GetTempPath(), "sub"));
            var localFolder = MockOfIFileSystemInfoFactoryUtil.CreateLocalFolder(Path.Combine(Path.GetTempPath(), "folder"));
            localFolder.Setup(f => f.Parent).Returns(this.localRootFolder.Object);
            var mappedFolder = new MappedObject("folder", "folderId", MappedObjectType.Folder, "subId", "changeToken") { Guid = Guid.NewGuid() };
            var mappedSubFolder = new MappedObject("sub", "subId", MappedObjectType.Folder, this.rootId, "changeToken") { Guid = Guid.NewGuid() };
            this.storage.Setup(s => s.GetObjectByLocalPath(It.Is<IDirectoryInfo>(d => d.Equals(localSubFolder.Object)))).Returns(mappedSubFolder);
            this.storage.Setup(s => s.GetObjectByRemoteId("folderId")).Returns(mappedFolder);
            remoteFolder.Setup(f => f.Move(subFolder.Object, this.remoteRootFolder.Object)).Callback(() => {
                remoteFolder.Setup(r => r.ChangeToken).Returns("changeToken");
            }).Returns(remoteFolder.Object);

            this.underTest.Solve(localFolder.Object, remoteFolder.Object);

            remoteFolder.Verify(f => f.Move(subFolder.Object, this.remoteRootFolder.Object), Times.Once());
            remoteFolder.VerifyUpdateLastModificationDate(localFolder.Object.LastWriteTimeUtc);
            this.storage.VerifySavedMappedObject(MappedObjectType.Folder, "folderId", "folder", "rootId", "changeToken");
        }

        [Test, Category("Fast"), Category("Solver")]
        public void MoveRenamedObject()
        {
            string newFolderName = "newFolder";
            var remoteFolder = MockOfIFolderUtil.CreateRemoteFolderMock("folderId", "folder", "/folder", this.rootId);
            var targetFolder = MockOfIFolderUtil.CreateRemoteFolderMock("targetId", "target", "/target", this.rootId);
            this.session.AddRemoteObjects(remoteFolder.Object, targetFolder.Object);
            var localTargetFolder = MockOfIFileSystemInfoFactoryUtil.CreateLocalFolder(Path.Combine(Path.GetTempPath(), "target"));
            var localFolder = MockOfIFileSystemInfoFactoryUtil.CreateLocalFolder(Path.Combine(Path.GetTempPath(), "target", newFolderName));
            localFolder.Setup(f => f.Parent).Returns(localTargetFolder.Object);
            var mappedFolder = new MappedObject("folder", "folderId", MappedObjectType.Folder, this.rootId, "changeToken") { Guid = Guid.NewGuid() };
            var mappedTargetFolder = new MappedObject("target", "targetId", MappedObjectType.Folder, this.rootId, "changeToken") { Guid = Guid.NewGuid() };
            this.storage.Setup(s => s.GetObjectByLocalPath(It.Is<IDirectoryInfo>(d => d.Equals(localTargetFolder.Object)))).Returns(mappedTargetFolder);
            this.storage.Setup(s => s.GetObjectByRemoteId("folderId")).Returns(mappedFolder);
            remoteFolder.Setup(f => f.Move(this.remoteRootFolder.Object, targetFolder.Object)).Returns(remoteFolder.Object);
            remoteFolder.Setup(f => f.Rename(newFolderName, true)).Callback(() => {
                remoteFolder.Setup(f => f.Name).Returns(newFolderName);
                remoteFolder.Setup(f => f.ChangeToken).Returns("changeToken");
            }).Returns(remoteFolder.Object);

            this.underTest.Solve(localFolder.Object, remoteFolder.Object);

            remoteFolder.Verify(f => f.Move(this.remoteRootFolder.Object, targetFolder.Object), Times.Once());
            remoteFolder.Verify(f => f.Rename(newFolderName, true), Times.Once());
            remoteFolder.VerifyUpdateLastModificationDate(localFolder.Object.LastWriteTimeUtc);
            this.storage.VerifySavedMappedObject(MappedObjectType.Folder, "folderId", newFolderName, "targetId", "changeToken");
        }
    }
}