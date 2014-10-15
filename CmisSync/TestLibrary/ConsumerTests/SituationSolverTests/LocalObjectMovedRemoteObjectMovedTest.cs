//-----------------------------------------------------------------------
// <copyright file="LocalObjectMovedRemoteObjectMovedTest.cs" company="GRAU DATA AG">
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
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;

    using Moq;

    using NUnit.Framework;

    using TestUtils;

    [TestFixture]
    public class LocalObjectMovedRemoteObjectMovedTest : IsTestWithConfiguredLog4Net
    {
        private readonly string remoteObjectId = "remoteObjectId";
        private readonly string newRemoteParentId = "newRemoteParentId";
        private readonly string oldRemoteParentId = "oldRemoteParentId";
        private readonly string oldName = "oldName";
        private readonly string newParentName = "newParentName";
        private readonly string newChangeToken = "newChangeToken";
        private readonly string oldChangeToken = "oldChangeToken";
        private readonly string newLocalName = "newLocalName";
        private readonly string newRemoteName = "newRemoteName";

        private Guid oldParentUuid;
        private Guid newParentUuid;
        private Guid localUuid;
        private Mock<ISession> session;
        private Mock<IMetaDataStorage> storage;
        private Mock<IDirectoryInfo> oldLocalParentFolder;
        private MappedObject mappedParent;
        private LocalObjectMovedRemoteObjectMoved underTest;
        private string newParentPath;
        private DateTime remoteModification;
        private DateTime localModification;
        private DateTime oldModification;

        [SetUp]
        public void SetUp() {
            this.newParentPath = Path.Combine(Path.GetTempPath(), this.newParentName);
            this.oldParentUuid = Guid.NewGuid();
            this.newParentUuid = Guid.NewGuid();
            this.localUuid = Guid.NewGuid();
            this.session = new Mock<ISession>();
            this.session.SetupTypeSystem();
            this.storage = new Mock<IMetaDataStorage>();
            this.oldLocalParentFolder = new Mock<IDirectoryInfo>();
            this.oldLocalParentFolder.SetupGuid(this.oldParentUuid);
            this.mappedParent = new MappedObject("parent", this.oldRemoteParentId, MappedObjectType.Folder, null, this.oldChangeToken) {
                Guid = this.oldParentUuid
            };
            this.storage.AddMappedFolder(this.mappedParent);
            this.underTest = new LocalObjectMovedRemoteObjectMoved(this.session.Object, this.storage.Object);
            var mappedNewLocalParent = new MappedObject(this.newParentName, this.newRemoteParentId, MappedObjectType.Folder, null, this.oldChangeToken) {
                Guid = this.newParentUuid
            };
            this.storage.AddMappedFolder(mappedNewLocalParent);
            this.localModification = DateTime.UtcNow;
            this.remoteModification = this.localModification;
            this.oldModification = this.localModification - TimeSpan.FromDays(1);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void ConstructorTakesValidInput() {
            new LocalObjectMovedRemoteObjectMoved(this.session.Object, this.storage.Object);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void FolderMovedToEqualFolderAndNamesAreEqual() {
            this.SetupOldMappedFolder();
            this.remoteModification = this.localModification - TimeSpan.FromMinutes(30);
            var newLocalParent = this.CreateNewLocalParent(this.newParentUuid);
            var localFolder = this.CreateLocalFolder(this.oldName, newLocalParent, this.localModification);
            var remoteFolder = MockOfIFolderUtil.CreateRemoteFolderMock(this.remoteObjectId, this.oldName, "/" + this.oldName, this.newRemoteParentId, this.newChangeToken);
            remoteFolder.SetupLastModificationDate(remoteModification);

            this.underTest.Solve(localFolder.Object, remoteFolder.Object, ContentChangeType.NONE, ContentChangeType.NONE);

            remoteFolder.VerifyUpdateLastModificationDate(this.localModification);
            this.VerifySavedFolder(this.oldName, this.localModification);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void FolderMovedToEqualFolderAndRemoteNameChanged() {
            this.SetupOldMappedFolder();
            this.remoteModification = this.localModification - TimeSpan.FromMinutes(30);
            var newLocalParent = this.CreateNewLocalParent(this.newParentUuid, this.newParentPath);
            var localFolder = this.CreateLocalFolder(this.oldName, newLocalParent, this.localModification);
            localFolder.SetupMoveTo(Path.Combine(this.newParentPath, this.newRemoteName));
            var remoteFolder = MockOfIFolderUtil.CreateRemoteFolderMock(this.remoteObjectId, this.newRemoteName, "/" + this.newRemoteName, this.newRemoteParentId, this.newChangeToken);
            remoteFolder.SetupLastModificationDate(remoteModification);

            this.underTest.Solve(localFolder.Object, remoteFolder.Object, ContentChangeType.NONE, ContentChangeType.NONE);

            remoteFolder.Verify(r => r.Rename(It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
            remoteFolder.VerifyUpdateLastModificationDate(this.localModification);
            this.VerifySavedFolder(this.newRemoteName, this.localModification);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void FolderMovedToEqualFolderAndLocalNameChanged() {
            this.SetupOldMappedFolder();
            this.remoteModification = this.localModification - TimeSpan.FromMinutes(30);
            var newLocalParent = this.CreateNewLocalParent(this.newParentUuid);
            var localFolder = this.CreateLocalFolder(this.newLocalName, newLocalParent, this.localModification);
            var remoteFolder = MockOfIFolderUtil.CreateRemoteFolderMock(this.remoteObjectId, this.oldName, "/" + this.oldName, this.newRemoteParentId, "change");
            remoteFolder.Setup(
                r =>
                r.Rename(this.newLocalName, true))
                .Returns(remoteFolder.Object).Callback(() => remoteFolder.SetupChangeToken(this.newChangeToken));
            remoteFolder.SetupLastModificationDate(remoteModification);

            this.underTest.Solve(localFolder.Object, remoteFolder.Object, ContentChangeType.NONE, ContentChangeType.NONE);

            remoteFolder.Verify(r => r.Rename(this.newLocalName, true), Times.Once());
            localFolder.Verify(f => f.MoveTo(It.IsAny<string>()), Times.Never());
            remoteFolder.VerifyUpdateLastModificationDate(this.localModification);
            this.VerifySavedFolder(this.newLocalName, this.localModification);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void FolderMovedToEqualFolderAndBothNamesAreChangedAndLocalModificationIsNewer() {
            this.SetupOldMappedFolder();
            this.remoteModification = this.localModification - TimeSpan.FromMinutes(30);
            var newLocalParent = this.CreateNewLocalParent(this.newParentUuid);
            var localFolder = this.CreateLocalFolder(this.newLocalName, newLocalParent, localModification);
            var remoteFolder = MockOfIFolderUtil.CreateRemoteFolderMock(this.remoteObjectId, this.newRemoteName, "/" + this.oldName, this.newRemoteParentId, "change");
            remoteFolder.Setup(
                r =>
                r.Rename(this.newLocalName, true))
                .Returns(remoteFolder.Object).Callback(() => remoteFolder.SetupChangeToken(this.newChangeToken));
            remoteFolder.SetupLastModificationDate(remoteModification);

            this.underTest.Solve(localFolder.Object, remoteFolder.Object, ContentChangeType.NONE, ContentChangeType.NONE);

            remoteFolder.Verify(r => r.Rename(this.newLocalName, true), Times.Once());
            localFolder.Verify(f => f.MoveTo(It.IsAny<string>()), Times.Never());
            remoteFolder.VerifyUpdateLastModificationDate(localModification);
            this.VerifySavedFolder(this.newLocalName, localModification);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void FolderMovedToEqualFolderAndBothNamesAreChangedAndRemoteModificationIsNewer() {
            this.SetupOldMappedFolder();
            this.localModification = this.remoteModification - TimeSpan.FromMinutes(30);
            var newLocalParent = this.CreateNewLocalParent(this.newParentUuid, this.newParentPath);
            var localFolder = this.CreateLocalFolder(this.newLocalName, newLocalParent, localModification);
            var remoteFolder = MockOfIFolderUtil.CreateRemoteFolderMock(this.remoteObjectId, this.newRemoteName, "/" + this.oldName, this.newRemoteParentId, this.newChangeToken);
            remoteFolder.SetupLastModificationDate(remoteModification);

            this.underTest.Solve(localFolder.Object, remoteFolder.Object, ContentChangeType.NONE, ContentChangeType.NONE);

            remoteFolder.Verify(r => r.Rename(It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
            localFolder.Verify(f => f.MoveTo(Path.Combine()), Times.Never());
            this.VerifySavedFolder(this.newRemoteName, remoteModification);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void FilesMovedToEqualFolderAndNamesAndContentAreEqual() {
            this.SetupOldMappedFile();
            this.remoteModification = this.localModification - TimeSpan.FromMinutes(30);
            var newLocalParent = this.CreateNewLocalParent(this.newParentUuid);
            var localFile = this.CreateLocalFile(this.oldName, newLocalParent, this.localModification);
            var remoteFile = MockOfIDocumentUtil.CreateRemoteDocumentMock(null, remoteObjectId, this.oldName, newRemoteParentId, 0, new byte[0], newChangeToken);
            remoteFile.SetupUpdateModificationDate(remoteModification);

            this.underTest.Solve(localFile.Object, remoteFile.Object, ContentChangeType.NONE, ContentChangeType.NONE);

            remoteFile.VerifyUpdateLastModificationDate(this.localModification);
            this.VerifySavedFile(this.oldName, this.localModification, 0);
        }

        [Ignore]
        [Test, Category("Fast"), Category("Solver")]
        public void FilesMovedToEqualFolderAndRemoteNameChangedButContentStaysEqual() {
            Assert.Fail("TODO");
        }

        [Ignore]
        [Test, Category("Fast"), Category("Solver")]
        public void FolderMovedIntoDifferentFoldersButNameStaysEqual() {
            Assert.Fail("TODO");
        }

        [Ignore]
        [Test, Category("Fast"), Category("Solver")]
        public void FolderMovedIntoDifferentFoldersAndRemoteNameChanged() {
            Assert.Fail("TODO");
        }

        [Ignore]
        [Test, Category("Fast"), Category("Solver")]
        public void FolderMovedIntoDifferentFoldersAndLocalNameChanged() {
            Assert.Fail("TODO");
        }

        [Ignore]
        [Test, Category("Fast"), Category("Solver")]
        public void FolderMovedIntoDifferentFoldersAndBothNamesChanged() {
            Assert.Fail("TODO");
        }

        private Mock<IDirectoryInfo> CreateLocalFolder(string name, IDirectoryInfo parent, DateTime modificationDate) {
            var folder = Mock.Of<IDirectoryInfo>(
                d =>
                d.Uuid == this.localUuid &&
                d.Name == name &&
                d.Parent == parent &&
                d.LastWriteTimeUtc == modificationDate);
            return Mock.Get(folder);
        }

        private Mock<IFileInfo> CreateLocalFile(string name, IDirectoryInfo parent, DateTime modificationDate)
        {
            var file = Mock.Of<IFileInfo>(
                f =>
                f.Uuid == this.localUuid &&
                f.Name == name &&
                f.Directory == parent &&
                f.LastWriteTimeUtc == modificationDate);
            return Mock.Get(file);
        }

        private IDirectoryInfo CreateNewLocalParent(Guid uuid, string fullName = null)
        {
            return Mock.Of<IDirectoryInfo>(
                d =>
                d.Uuid == uuid &&
                d.FullName == fullName);
        }

        private void SetupOldMappedFolder()
        {
            this.SetupOldMappedObject(true);
        }

        private void SetupOldMappedFile()
        {
            this.SetupOldMappedObject(false);
        }

        private void SetupOldMappedObject(bool isFolder)
        {
            var mappedObject = new MappedObject(
                this.oldName,
                this.remoteObjectId,
                isFolder ? MappedObjectType.Folder : MappedObjectType.File,
                this.oldRemoteParentId,
                this.oldChangeToken) {
                Guid = this.localUuid,
                LastLocalWriteTimeUtc = this.oldModification,
                LastRemoteWriteTimeUtc = this.oldModification,
                LastContentSize = isFolder ? -1 : 0
            };
            if (isFolder)
            {
                this.storage.AddMappedFolder(mappedObject);
            }
            else
            {
                this.storage.AddMappedFile(mappedObject);
            }
        }

        private void VerifySavedFolder(string newName, DateTime modificationDate) {
            this.VerifySavedObject(newName, modificationDate, true);
        }

        private void VerifySavedFile(string newName, DateTime modificationDate, long contentSize) {
            this.VerifySavedObject(newName, modificationDate, false, contentSize);
        }

        private void VerifySavedObject(string newName, DateTime modificationDate, bool isFolder, long contentSize = -1) {
            this.storage.VerifySavedMappedObject(
                isFolder ? MappedObjectType.Folder : MappedObjectType.File,
                this.remoteObjectId,
                newName,
                this.newRemoteParentId,
                this.newChangeToken,
                true,
                modificationDate,
                modificationDate,
                null,
                contentSize);
        }
    }
}