//-----------------------------------------------------------------------
// <copyright file="LocalSituationDetectionTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.ConsumerTests {
    using System;
    using System.IO;

    using CmisSync.Lib.Consumer;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Producer.Watcher;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;

    using Moq;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture]
    public class LocalSituationDetectionTest {
        private LocalSituationDetection underTest = new LocalSituationDetection();

        [Test, Category("Fast"), Category("SituationDetection")]
        public void NoChangeOnFile() {
            var fileEvent = new FileEvent(Mock.Of<IFileInfo>()) { Local = MetaDataChangeType.NONE };
            Assert.That(this.underTest.Analyse(Mock.Of<IMetaDataStorage>(), fileEvent), Is.EqualTo(SituationType.NOCHANGE));
        }

        [Test, Category("Fast"), Category("SituationDetection")]
        public void FileAddedDetection() {
            var fileInfo = Mock.Of<IFileInfo>(f => f.Exists == true);
            var fileEvent = new FileEvent(fileInfo) { Local = MetaDataChangeType.CREATED };

            Assert.That(this.underTest.Analyse(Mock.Of<IMetaDataStorage>(), fileEvent), Is.EqualTo(SituationType.ADDED));
        }

        [Test, Category("Fast"), Category("SituationDetection")]
        public void FileContentChanged() {
            var fileInfo = Mock.Of<IFileInfo>(f => f.Exists == true);
            var fileEvent = new FileEvent(fileInfo) { Local = MetaDataChangeType.NONE, LocalContent = ContentChangeType.CHANGED };

            Assert.That(this.underTest.Analyse(Mock.Of<IMetaDataStorage>(), fileEvent), Is.EqualTo(SituationType.CHANGED));
        }

        [Test, Category("Fast"), Category("SituationDetection")]
        public void FileRemoved() {
            var fileInfo = Mock.Of<IFileInfo>(f => f.Exists == false);
            var fileEvent = new FileEvent(fileInfo) { Local = MetaDataChangeType.DELETED };

            Assert.That(this.underTest.Analyse(Mock.Of<IMetaDataStorage>(), fileEvent), Is.EqualTo(SituationType.REMOVED));
        }

        [Test, Category("Fast"), Category("SituationDetection")]
        public void FileRenamed() {
            string parentId = "parentId";
            var storage = new Mock<IMetaDataStorage>();
            Guid fileUuid = Guid.NewGuid();
            Guid parentUuid = Guid.NewGuid();
            var parentDirectoryInfo = Mock.Of<IDirectoryInfo>(d => d.Uuid == parentUuid);
            var fileInfo = Mock.Of<IFileInfo>(f => f.Exists == true && f.Uuid == fileUuid && f.Directory == parentDirectoryInfo);
            var fileEvent = new FileEvent(fileInfo) { Local = MetaDataChangeType.MOVED };
            var mappedFile = Mock.Of<IMappedObject>(o => o.Guid == fileUuid && o.ParentId == parentId);
            var mappedParent = Mock.Of<IMappedObject>(o => o.Guid == parentUuid);
            storage.Setup(s => s.GetObjectByGuid(fileUuid)).Returns(mappedFile);
            storage.Setup(s => s.GetObjectByRemoteId(parentId)).Returns(mappedParent);

            Assert.That(this.underTest.Analyse(storage.Object, fileEvent), Is.EqualTo(SituationType.RENAMED));
        }

        [Test, Category("Fast"), Category("SituationDetection")]
        public void FileMovedWithoutStorageEntry() {
            var fileInfo = Mock.Of<IFileInfo>(f => f.Exists == true);
            var fileEvent = new FileEvent(fileInfo) { Local = MetaDataChangeType.MOVED };

            Assert.That(this.underTest.Analyse(Mock.Of<IMetaDataStorage>(), fileEvent), Is.EqualTo(SituationType.MOVED));
        }

        [Test, Category("Fast"), Category("SituationDetection")]
        public void FileMovedWithStorageEntry() {
            string parentId = "parentId";
            var storage = new Mock<IMetaDataStorage>();
            Guid fileUuid = Guid.NewGuid();
            Guid oldParentUuid = Guid.NewGuid();
            Guid newParentUuid = Guid.NewGuid();
            var parentDirectoryInfo = Mock.Of<IDirectoryInfo>(d => d.Uuid == newParentUuid);
            var fileInfo = Mock.Of<IFileInfo>(f => f.Exists == true && f.Uuid == fileUuid && f.Directory == parentDirectoryInfo);
            var fileEvent = new FileEvent(fileInfo) { Local = MetaDataChangeType.MOVED };
            var mappedFile = Mock.Of<IMappedObject>(o => o.Guid == fileUuid && o.ParentId == parentId);
            var mappedParent = Mock.Of<IMappedObject>(o => o.Guid == oldParentUuid);
            storage.Setup(s => s.GetObjectByGuid(fileUuid)).Returns(mappedFile);
            storage.Setup(s => s.GetObjectByRemoteId(parentId)).Returns(mappedParent);

            Assert.That(this.underTest.Analyse(Mock.Of<IMetaDataStorage>(), fileEvent), Is.EqualTo(SituationType.MOVED));
        }

        [Test, Category("Fast"), Category("SituationDetection")]
        public void FolderAdded() {
            var folderEvent = new FolderEvent(Mock.Of<IDirectoryInfo>()) { Local = MetaDataChangeType.CREATED };

            Assert.That(this.underTest.Analyse(Mock.Of<IMetaDataStorage>(), folderEvent), Is.EqualTo(SituationType.ADDED));
        }

        [Test, Category("Fast"), Category("SituationDetection")]
        public void FolderRemoved() {
            var folderEvent = new FolderEvent(Mock.Of<IDirectoryInfo>()) { Local = MetaDataChangeType.DELETED };

            Assert.That(this.underTest.Analyse(Mock.Of<IMetaDataStorage>(), folderEvent), Is.EqualTo(SituationType.REMOVED));
        }

        [Test, Category("Fast"), Category("SituationDetection")]
        public void FolderRenamed() {
            var storage = new Mock<IMetaDataStorage>();
            Guid guid = Guid.NewGuid();
            var dirInfo = Mock.Of<IDirectoryInfo>(
                d =>
                d.Name == "newName" &&
                d.FullName == Path.Combine(Path.GetTempPath(), "newName") &&
                d.Uuid == guid);
            storage.Setup(s => s.GetObjectByGuid(guid)).Returns(Mock.Of<IMappedObject>());
            var folderEvent = new FolderEvent(dirInfo) { Local = MetaDataChangeType.CHANGED };

            Assert.That(this.underTest.Analyse(storage.Object, folderEvent), Is.EqualTo(SituationType.RENAMED));
        }

        [Test, Category("Fast"), Category("SituationDetection")]
        public void FolderChanged() {
            var storage = new Mock<IMetaDataStorage>();
            Guid guid = Guid.NewGuid();
            var dirInfo = Mock.Of<IDirectoryInfo>(
                d =>
                d.Name == "Name" &&
                d.FullName == Path.Combine(Path.GetTempPath(), "Name") &&
                d.Uuid == guid);
            storage.Setup(s => s.GetObjectByLocalPath(dirInfo)).Returns(Mock.Of<IMappedObject>());
            var folderEvent = new FolderEvent(dirInfo) { Local = MetaDataChangeType.CHANGED };

            Assert.That(this.underTest.Analyse(storage.Object, folderEvent), Is.EqualTo(SituationType.CHANGED));
        }

        [Test, Category("Fast"), Category("SituationDetection")]
        public void FolderMoved() {
            var folderEvent = new FolderMovedEvent(Mock.Of<IDirectoryInfo>(), Mock.Of<IDirectoryInfo>(), null, null) { Local = MetaDataChangeType.MOVED };

            Assert.That(this.underTest.Analyse(Mock.Of<IMetaDataStorage>(), folderEvent), Is.EqualTo(SituationType.MOVED));
        }
    }
}