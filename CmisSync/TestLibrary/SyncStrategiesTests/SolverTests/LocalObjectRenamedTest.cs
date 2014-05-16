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

namespace TestLibrary.SyncStrategiesTests.SolverTests
{
    using System;

    using CmisSync.Lib.Data;
    using CmisSync.Lib.Storage;
    using CmisSync.Lib.Sync.Solver;

    using DotCMIS.Client;

    using Moq;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture]
    public class LocalObjectRenamedTest
    {
        [Test, Category("Fast"), Category("Solver")]
        public void DefaultConstructorTest()
        {
            new LocalObjectRenamed();
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalFolderRenamed()
        {
            string oldName = "oldName";
            string newName = "newName";
            string id = "id";
            string newChangeToken = "newChange";
            var modificationDate = DateTime.UtcNow;
            var newFolder = Mock.Of<IFolder>(
                f =>
                f.LastModificationDate == modificationDate &&
                f.Name == newName &&
                f.ChangeToken == newChangeToken);
            var remoteFolder = new Mock<IFolder>();
            remoteFolder.Setup(f => f.Name).Returns(oldName);
            remoteFolder.Setup(f => f.Id).Returns(id);
            remoteFolder.Setup(f => f.Rename(newName, true)).Returns(newFolder);
            var localFolder = new Mock<IDirectoryInfo>();
            localFolder.SetupProperty(f => f.LastWriteTimeUtc, modificationDate);
            localFolder.Setup(f => f.Name).Returns(newName);
            var mappedFolder = new Mock<IMappedObject>();
            mappedFolder.SetupAllProperties();
            mappedFolder.SetupProperty(f => f.Name, oldName);
            mappedFolder.SetupProperty(f => f.RemoteObjectId, id);
            mappedFolder.Setup(f => f.Type).Returns(MappedObjectType.Folder);

            var storage = new Mock<IMetaDataStorage>();
            storage.AddMappedFolder(mappedFolder.Object);

            var solver = new LocalObjectRenamed();

            solver.Solve(Mock.Of<ISession>(), storage.Object, localFolder.Object, remoteFolder.Object);

            remoteFolder.Verify(f => f.Rename(It.Is<string>(s => s == newName), It.Is<bool>(b => b == true)), Times.Once());

            storage.Verify(
                s =>
                s.SaveMappedObject(It.Is<IMappedObject>(o => this.VerifySavedObject(o, MappedObjectType.Folder, id, newName, newChangeToken, modificationDate))),
                Times.Once());
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalFileRenamed()
        {
            string oldName = "oldName";
            string newName = "newName";
            string id = "id";
            string newChangeToken = "newChange";
            var modificationDate = DateTime.UtcNow;
            var newFile = Mock.Of<IDocument>(
                f =>
                f.LastModificationDate == modificationDate &&
                f.Name == newName &&
                f.ChangeToken == newChangeToken);
            var remoteFile = new Mock<IDocument>();
            remoteFile.Setup(f => f.Name).Returns(oldName);
            remoteFile.Setup(f => f.Id).Returns(id);
            remoteFile.Setup(f => f.Rename(newName, true)).Returns(newFile);
            var localFolder = new Mock<IFileInfo>();
            localFolder.SetupProperty(f => f.LastWriteTimeUtc, modificationDate);
            localFolder.Setup(f => f.Name).Returns(newName);
            var mappedFile = new Mock<IMappedObject>();
            mappedFile.SetupAllProperties();
            mappedFile.SetupProperty(f => f.Name, oldName);
            mappedFile.SetupProperty(f => f.RemoteObjectId, id);
            mappedFile.Setup(f => f.Type).Returns(MappedObjectType.File);

            var storage = new Mock<IMetaDataStorage>();
            storage.AddMappedFile(mappedFile.Object);

            var solver = new LocalObjectRenamed();

            solver.Solve(Mock.Of<ISession>(), storage.Object, localFolder.Object, remoteFile.Object);

            remoteFile.Verify(f => f.Rename(It.Is<string>(s => s == newName), It.Is<bool>(b => b == true)), Times.Once());

            storage.Verify(
                s =>
                s.SaveMappedObject(It.Is<IMappedObject>(o => this.VerifySavedObject(o, MappedObjectType.File, id, newName, newChangeToken, modificationDate))),
                Times.Once());
        }

        private bool VerifySavedObject(IMappedObject o, MappedObjectType type, string id, string name, string changeToken, DateTime modificationDate)
        {
            Assert.That(o.Type, Is.EqualTo(type));
            Assert.That(o.Name, Is.EqualTo(name));
            Assert.That(o.LastChangeToken, Is.EqualTo(changeToken));
            Assert.That(o.LastLocalWriteTimeUtc, Is.EqualTo(modificationDate));
            Assert.That(o.LastRemoteWriteTimeUtc, Is.EqualTo(modificationDate));
            return true;
        }
    }
}