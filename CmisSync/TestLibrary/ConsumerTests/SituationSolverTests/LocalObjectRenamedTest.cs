//-----------------------------------------------------------------------
// <copyright file="LocalObjectRenamedTest.cs" company="GRAU DATA AG">
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

    using CmisSync.Lib.Consumer.SituationSolver;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;

    using Moq;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture]
    public class LocalObjectRenamedTest
    {
        private readonly string oldName = "oldName";
        private readonly string newName = "newName";
        private readonly string id = "id";
        private readonly string newChangeToken = "newChange";
        private readonly DateTime modificationDate = DateTime.UtcNow;

        private Mock<IMetaDataStorage> storage;
        private Mock<ISession> session;
        private LocalObjectRenamed underTest;

        [SetUp]
        public void SetUp() {
            this.storage = new Mock<IMetaDataStorage>();
            this.session = new Mock<ISession>();
            this.underTest = new LocalObjectRenamed(this.session.Object, this.storage.Object);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void DefaultConstructorTest()
        {
            new LocalObjectRenamed(this.session.Object, this.storage.Object);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalFolderRenamed()
        {
            var newFolder = Mock.Of<IFolder>(
                f =>
                f.LastModificationDate == this.modificationDate &&
                f.Name == this.newName &&
                f.ChangeToken == this.newChangeToken);
            var remoteFolder = new Mock<IFolder>();
            remoteFolder.Setup(f => f.Name).Returns(this.oldName);
            remoteFolder.Setup(f => f.Id).Returns(this.id);
            remoteFolder.Setup(f => f.Rename(this.newName, true)).Returns(newFolder);
            var localFolder = new Mock<IDirectoryInfo>();
            localFolder.SetupProperty(f => f.LastWriteTimeUtc, this.modificationDate);
            localFolder.Setup(f => f.Name).Returns(this.newName);
            var mappedFolder = new Mock<IMappedObject>();
            mappedFolder.SetupAllProperties();
            mappedFolder.SetupProperty(f => f.Guid, Guid.NewGuid());
            mappedFolder.SetupProperty(f => f.Name, this.oldName);
            mappedFolder.SetupProperty(f => f.RemoteObjectId, this.id);
            mappedFolder.Setup(f => f.Type).Returns(MappedObjectType.Folder);

            this.storage.AddMappedFolder(mappedFolder.Object);

            this.underTest.Solve(localFolder.Object, remoteFolder.Object);

            remoteFolder.Verify(f => f.Rename(It.Is<string>(s => s == this.newName), It.Is<bool>(b => b == true)), Times.Once());

            this.storage.VerifySavedMappedObject(MappedObjectType.Folder, this.id, this.newName, null, this.newChangeToken, true, this.modificationDate);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalFileRenamed()
        {
            var newFile = Mock.Of<IDocument>(
                f =>
                f.LastModificationDate == this.modificationDate &&
                f.Name == this.newName &&
                f.ChangeToken == this.newChangeToken);
            var remoteFile = new Mock<IDocument>();
            remoteFile.Setup(f => f.Name).Returns(this.oldName);
            remoteFile.Setup(f => f.Id).Returns(this.id);
            remoteFile.Setup(f => f.Rename(this.newName, true)).Returns(newFile);
            var localFolder = new Mock<IFileInfo>();
            localFolder.SetupProperty(f => f.LastWriteTimeUtc, this.modificationDate);
            localFolder.Setup(f => f.Name).Returns(this.newName);
            var mappedFile = new Mock<IMappedObject>();
            mappedFile.SetupAllProperties();
            mappedFile.SetupProperty(f => f.Guid, Guid.NewGuid());
            mappedFile.SetupProperty(f => f.Name, this.oldName);
            mappedFile.SetupProperty(f => f.RemoteObjectId, this.id);
            mappedFile.Setup(f => f.Type).Returns(MappedObjectType.File);
            mappedFile.Setup(f => f.LastContentSize).Returns(0);

            this.storage.AddMappedFile(mappedFile.Object);

            this.underTest.Solve(localFolder.Object, remoteFile.Object);

            remoteFile.Verify(f => f.Rename(It.Is<string>(s => s == this.newName), It.Is<bool>(b => b == true)), Times.Once());

            this.storage.VerifySavedMappedObject(MappedObjectType.File, this.id, this.newName, null, this.newChangeToken, true, this.modificationDate, contentSize: 0);
        }
    }
}