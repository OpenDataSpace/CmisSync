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

namespace TestLibrary.ConsumerTests
{
    using System;
    using System.IO;

    using CmisSync.Lib.Consumer;
    using CmisSync.Lib.Data;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Storage;
    using CmisSync.Lib.Sync.Strategy;

    using DotCMIS.Client;

    using Moq;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture]
    public class LocalSituationDetectionTest
    {
        [Test, Category("Fast"), Category("SituationDetection")]
        public void NoChangeOnFileDetection()
        {
            var fileEvent = new FileEvent(Mock.Of<IFileInfo>()) { Local = MetaDataChangeType.NONE };
            Assert.That(new LocalSituationDetection().Analyse(Mock.Of<IMetaDataStorage>(), fileEvent), Is.EqualTo(SituationType.NOCHANGE));
        }

        [Test, Category("Fast"), Category("SituationDetection")]
        public void FileAddedDetection()
        {
            var fileInfo = Mock.Of<IFileInfo>(f => f.Exists == true);
            var fileEvent = new FileEvent(fileInfo) { Local = MetaDataChangeType.CREATED };

            Assert.That(new LocalSituationDetection().Analyse(Mock.Of<IMetaDataStorage>(), fileEvent), Is.EqualTo(SituationType.ADDED));
        }

        [Test, Category("Fast"), Category("SituationDetection")]
        public void FileContentChangedDetection()
        {
            var fileInfo = Mock.Of<IFileInfo>(f => f.Exists == true);
            var FileEvent = new FileEvent(fileInfo) { Local = MetaDataChangeType.NONE, LocalContent = ContentChangeType.CHANGED };

            Assert.That(new LocalSituationDetection().Analyse(Mock.Of<IMetaDataStorage>(), FileEvent), Is.EqualTo(SituationType.CHANGED));
        }

        [Test, Category("Fast"), Category("SituationDetection")]
        public void FileRemovedDetection()
        {
            var fileInfo = Mock.Of<IFileInfo>(f => f.Exists == false);
            var fileEvent = new FileEvent(fileInfo) { Local = MetaDataChangeType.DELETED };

            Assert.That(new LocalSituationDetection().Analyse(Mock.Of<IMetaDataStorage>(), fileEvent), Is.EqualTo(SituationType.REMOVED));
        }

        [Test, Category("Fast"), Category("SituationDetection")]
        public void FolderAddedDetection()
        {
            var folderEvent = new FolderEvent(Mock.Of<IDirectoryInfo>()) { Local = MetaDataChangeType.CREATED };

            Assert.That(new LocalSituationDetection().Analyse(Mock.Of<IMetaDataStorage>(), folderEvent), Is.EqualTo(SituationType.ADDED));
        }

        [Test, Category("Fast"), Category("SituationDetection")]
        public void FolderRemovedDetection()
        {
            var folderEvent = new FolderEvent(Mock.Of<IDirectoryInfo>()) { Local = MetaDataChangeType.DELETED };

            Assert.That(new LocalSituationDetection().Analyse(Mock.Of<IMetaDataStorage>(), folderEvent), Is.EqualTo(SituationType.REMOVED));
        }

        [Test, Category("Fast"), Category("SituationDetection")]
        public void FolderRenamedDetection()
        {
            var storage = new Mock<IMetaDataStorage>();
            Guid guid = Guid.NewGuid();
            var dirInfo = Mock.Of<IDirectoryInfo>(
                d =>
                d.Name == "newName" &&
                d.FullName == Path.Combine(Path.GetTempPath(), "newName") &&
                d.GetExtendedAttribute(MappedObject.ExtendedAttributeKey) == guid.ToString());
            storage.Setup(s => s.GetObjectByGuid(guid)).Returns(Mock.Of<IMappedObject>());
            var folderEvent = new FolderEvent(dirInfo) { Local = MetaDataChangeType.CHANGED };

            Assert.That(new LocalSituationDetection().Analyse(storage.Object, folderEvent), Is.EqualTo(SituationType.RENAMED));
        }

        [Test, Category("Fast"), Category("SituationDetection")]
        public void FolderChangedDetection()
        {
            var storage = new Mock<IMetaDataStorage>();
            Guid guid = Guid.NewGuid();
            var dirInfo = Mock.Of<IDirectoryInfo>(
                d =>
                d.Name == "Name" &&
                d.FullName == Path.Combine(Path.GetTempPath(), "Name") &&
                d.GetExtendedAttribute(MappedObject.ExtendedAttributeKey) == guid.ToString());
            storage.Setup(s => s.GetObjectByLocalPath(dirInfo)).Returns(Mock.Of<IMappedObject>());
            var folderEvent = new FolderEvent(dirInfo) { Local = MetaDataChangeType.CHANGED };

            Assert.That(new LocalSituationDetection().Analyse(storage.Object, folderEvent), Is.EqualTo(SituationType.CHANGED));
        }

        [Test, Category("Fast"), Category("SituationDetection")]
        public void FolderMovedDetection()
        {
            var folderEvent = new FolderMovedEvent(Mock.Of<IDirectoryInfo>(), Mock.Of<IDirectoryInfo>(), null, null) { Local = MetaDataChangeType.MOVED };

            Assert.That(new LocalSituationDetection().Analyse(Mock.Of<IMetaDataStorage>(), folderEvent), Is.EqualTo(SituationType.MOVED));
        }
    }
}