//-----------------------------------------------------------------------
// <copyright file="LocalObjectRenamedRemoteObjectRenamedTest.cs" company="GRAU DATA AG">
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
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;

    using Moq;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture]
    public class LocalObjectRenamedRemoteObjectRenamedTest
    {
        private readonly string fullNamePrefix = Path.Combine("full", "path");
        private readonly string oldName = "oldName";
        private readonly string folderId = "folderId";
        private readonly string fileId = "fileId";
        private readonly string newChangeToken = "newChange";
        private string newLocalName;
        private string newRemoteName;
        private LocalObjectRenamedRemoteObjectRenamed underTest;
        private Mock<ISession> session;
        private Mock<IMetaDataStorage> storage;
        private Mock<LocalObjectChangedRemoteObjectChanged> changeSolver;

        [SetUp]
        public void SetUp() {
            this.newLocalName = string.Empty;
            this.newRemoteName = string.Empty;
            this.session = new Mock<ISession>();
            this.session.SetupTypeSystem();
            this.storage = new Mock<IMetaDataStorage>();
            this.InitializeMappedFolderOnStorage();
            this.InitializeMappedFileOnStorage();
            var transmissionManager = new ActiveActivitiesManager();
            var fsFactory = Mock.Of<IFileSystemInfoFactory>();
            this.changeSolver = new Mock<LocalObjectChangedRemoteObjectChanged>(this.session.Object, this.storage.Object, null, transmissionManager, fsFactory);
            this.underTest = new LocalObjectRenamedRemoteObjectRenamed(this.session.Object, this.storage.Object, this.changeSolver.Object);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void DefaultConstructor() {
            new LocalObjectRenamedRemoteObjectRenamed(this.session.Object, this.storage.Object, this.changeSolver.Object);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalAndRemoteFolderRenamedToSameName()
        {
            this.newRemoteName = "newName";
            this.newLocalName = this.newRemoteName;
            var remoteFolder = this.CreateRemoteFolder(this.newRemoteName);
            var localFolder = this.CreateLocalFolder(this.newLocalName);

            this.underTest.Solve(localFolder.Object, remoteFolder.Object);

            this.storage.VerifySavedMappedObject(MappedObjectType.Folder, this.folderId, this.newLocalName, null, this.newChangeToken, true, null);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalNameWillBeChoosenIfLocalModificationIsShorterAgo()
        {
            this.newRemoteName = "newRemoteName";
            this.newLocalName = "newLocalName";
            DateTime remoteModification = DateTime.UtcNow;
            DateTime localModification = remoteModification.AddMinutes(1);
            var remoteFolder = this.CreateRemoteFolder(this.newRemoteName, remoteModification);
            var localFolder = this.CreateLocalFolder(this.newLocalName, localModification);
            remoteFolder.Setup(f => f.Rename(this.newLocalName, true)).Callback(() => remoteFolder.Setup(f => f.Name).Returns(this.newLocalName)).Returns(remoteFolder.Object);

            this.underTest.Solve(localFolder.Object, remoteFolder.Object);

            remoteFolder.Verify(f => f.Rename(this.newLocalName, true), Times.Once());
            this.storage.VerifySavedMappedObject(MappedObjectType.Folder, this.folderId, this.newLocalName, null, this.newChangeToken, true, null);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void RemoteNameWillBeChoosenIfRemoteModificationIsEqualToLocal()
        {
            this.newRemoteName = "newRemoteName";
            this.newLocalName = "newLocalName";
            DateTime modification = DateTime.UtcNow;
            var remoteFolder = this.CreateRemoteFolder(this.newRemoteName, modification);
            var localFolder = this.CreateLocalFolder(this.newLocalName, modification);
            localFolder.Setup(f => f.MoveTo(Path.Combine(this.fullNamePrefix, this.newRemoteName)));

            this.underTest.Solve(localFolder.Object, remoteFolder.Object);

            localFolder.Verify(f => f.MoveTo(Path.Combine(this.fullNamePrefix, this.newRemoteName)), Times.Once());
            this.storage.VerifySavedMappedObject(MappedObjectType.Folder, this.folderId, this.newRemoteName, null, this.newChangeToken, true, null);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void RemoteNameWillBeChoosenIfRemoteModificationIsShorterAgo()
        {
            this.newRemoteName = "newRemoteName";
            this.newLocalName = "newLocalName";
            DateTime localModification = DateTime.UtcNow;
            DateTime remoteModification = localModification.AddMinutes(1);
            var remoteFolder = this.CreateRemoteFolder(this.newRemoteName, remoteModification);
            var localFolder = this.CreateLocalFolder(this.newLocalName, localModification);
            localFolder.Setup(f => f.MoveTo(Path.Combine(this.fullNamePrefix, this.newRemoteName)));

            this.underTest.Solve(localFolder.Object, remoteFolder.Object);

            localFolder.Verify(f => f.MoveTo(Path.Combine(this.fullNamePrefix, this.newRemoteName)), Times.Once());
            this.storage.VerifySavedMappedObject(MappedObjectType.Folder, this.folderId, this.newRemoteName, null, this.newChangeToken, true, null);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalAndRemoteFileAreRenamedToSameFilename() {
            this.newRemoteName = "newName";
            this.newLocalName = this.newRemoteName;
            var remoteFile = this.CreateRemoteFile(this.newRemoteName);
            var localFile = this.CreateLocalFile(this.newLocalName);

            this.underTest.Solve(localFile.Object, remoteFile.Object);

            this.storage.VerifySavedMappedObject(MappedObjectType.File, this.fileId, this.newLocalName, null, this.newChangeToken, true, null, null, null, 0);
        }

        [Test, Category("Fast"), Category("Solver"), Ignore("TODO")]
        public void RemoteAndLocalFileAreRenamedToDifferentFilenames() {
            Assert.Fail("TODO");
        }

        private Mock<IFolder> CreateRemoteFolder(string name, DateTime? modificationDate = null) {
            var remoteFolder = new Mock<IFolder>(MockBehavior.Strict);
            remoteFolder.Setup(f => f.LastModificationDate).Returns(modificationDate == null ? DateTime.UtcNow : (DateTime)modificationDate);
            remoteFolder.Setup(f => f.Name).Returns(name);
            remoteFolder.Setup(f => f.Id).Returns(this.folderId);
            remoteFolder.Setup(f => f.ChangeToken).Returns(this.newChangeToken);
            return remoteFolder;
        }

        private Mock<IDirectoryInfo> CreateLocalFolder(string name, DateTime? modificationDate = null) {
            var localFolder = new Mock<IDirectoryInfo>(MockBehavior.Strict);
            localFolder.SetupProperty(f => f.LastWriteTimeUtc, modificationDate == null ? DateTime.UtcNow : (DateTime)modificationDate);
            localFolder.Setup(f => f.Name).Returns(name);
            localFolder.Setup(f => f.FullName).Returns(Path.Combine(this.fullNamePrefix, name));
            localFolder.Setup(f => f.Parent).Returns(Mock.Of<IDirectoryInfo>(d => d.FullName == this.fullNamePrefix));
            return localFolder;
        }

        private Mock<IDocument> CreateRemoteFile(string name, DateTime? modificationDate = null) {
            var remoteFile = new Mock<IDocument>(MockBehavior.Strict);
            remoteFile.Setup(f => f.LastModificationDate).Returns(modificationDate == null ? DateTime.UtcNow : (DateTime)modificationDate);
            remoteFile.Setup(f => f.Name).Returns(name);
            remoteFile.Setup(f => f.Id).Returns(this.fileId);
            remoteFile.Setup(f => f.ChangeToken).Returns(this.newChangeToken);
            return remoteFile;
        }

        private Mock<IFileInfo> CreateLocalFile(string name, DateTime? modificationDate = null) {
            var localFile = new Mock<IFileInfo>(MockBehavior.Strict);
            localFile.SetupProperty(f => f.LastWriteTimeUtc, modificationDate == null ? DateTime.UtcNow : (DateTime)modificationDate);
            localFile.Setup(f => f.Name).Returns(name);
            localFile.Setup(f => f.FullName).Returns(Path.Combine(this.fullNamePrefix, name));
            localFile.Setup(f => f.Directory).Returns(Mock.Of<IDirectoryInfo>(d => d.FullName == this.fullNamePrefix));
            return localFile;
        }

        private void InitializeMappedFolderOnStorage() {
            var mappedFolder = new Mock<IMappedObject>();
            mappedFolder.SetupAllProperties();
            mappedFolder.SetupProperty(f => f.Guid, Guid.NewGuid());
            mappedFolder.SetupProperty(f => f.Name, this.oldName);
            mappedFolder.SetupProperty(f => f.RemoteObjectId, this.folderId);
            mappedFolder.Setup(f => f.Type).Returns(MappedObjectType.Folder);

            this.storage.AddMappedFolder(mappedFolder.Object);
        }

        private void InitializeMappedFileOnStorage() {
            var mappedFile = new Mock<IMappedObject>();
            mappedFile.SetupAllProperties();
            mappedFile.SetupProperty(f => f.Guid, Guid.NewGuid());
            mappedFile.SetupProperty(f => f.Name, this.oldName);
            mappedFile.SetupProperty(f => f.RemoteObjectId, this.fileId);
            mappedFile.SetupProperty(f => f.LastChangeToken, this.newChangeToken);
            mappedFile.Setup(f => f.Type).Returns(MappedObjectType.File);

            this.storage.AddMappedFile(mappedFile.Object);
        }
    }
}