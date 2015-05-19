//-----------------------------------------------------------------------
// <copyright file="MappedObjectsTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.StorageTests.DataBaseTests.EntitiesTests {
    using System;
    using System.Collections.Generic;
    using System.IO;

    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;

    using Moq;

    using NUnit.Framework;

    using TestUtils;
    
    [TestFixture]
    public class MappedObjectsTest {
        private readonly string localRootPathName = "folder";
        private readonly string localRootPath = Path.Combine("local", "test", "folder");
        private readonly string localFileName = "file.test";
        private readonly string localFilePath = Path.Combine("local", "test", "folder", "file.test");

        [Test, Category("Fast"), Category("MappedObjects")]
        public void ConstructorTakesData(
            [Values(true, false)]bool ignored,
            [Values(true, false)]bool readOnly,
            [Values(MappedObjectType.File, MappedObjectType.Folder)]MappedObjectType type)
        {
            var data = new MappedObject("name", "remoteId", type, "parentId", "changeToken") {
                LastChecksum = new byte[20],
                Ignored = ignored,
                Guid = Guid.NewGuid(),
                LastLocalWriteTimeUtc = DateTime.Now,
                LastRemoteWriteTimeUtc = DateTime.UtcNow,
                Description = "desc",
                LastContentSize = type == MappedObjectType.File ? 2345 : 0,
                IsReadOnly = readOnly,
                LastTimeStoredInStorage = DateTime.UtcNow
            };

            var file = new MappedObject(data);

            Assert.That(data, Is.EqualTo(file));
            Assert.That(file.LastTimeStoredInStorage, Is.EqualTo(data.LastTimeStoredInStorage));
        }

        [Test, Category("Fast"), Category("MappedObjects")]
        public void ConstructorSetsDefaultParamsToNull([Values(MappedObjectType.File, MappedObjectType.Folder)]MappedObjectType type) {
            var obj = new MappedObject("name", "remoteId", type, "parentId", "changeToken");
            Assert.IsNull(obj.ChecksumAlgorithmName);
            Assert.IsNull(obj.Description);
            Assert.IsNull(obj.LastChecksum);
            Assert.IsNull(obj.LastLocalWriteTimeUtc);
            Assert.IsNull(obj.LastRemoteWriteTimeUtc);
            Assert.AreEqual(-1, obj.LastContentSize);
            Assert.That(obj.Ignored, Is.False);
            Assert.That(obj.ActualOperation, Is.EqualTo(OperationType.No));
            Assert.That(obj.Retries, Is.Empty);
            Assert.That(obj.IsReadOnly, Is.False);
            Assert.That(obj.LastTimeStoredInStorage, Is.Null);
        }

        [Test, Category("Fast"), Category("MappedObjects")]
        public void ConstructorTakesName([Values(MappedObjectType.File, MappedObjectType.Folder)]MappedObjectType type) {
            var obj = new MappedObject("name", "remoteId", type, null, null);
            Assert.That(obj.Name, Is.EqualTo("name"));
        }

        [Test, Category("Fast"), Category("MappedObjects")]
        public void ConstructorThrowsExceptionOnEmptyName([Values(MappedObjectType.File, MappedObjectType.Folder)]MappedObjectType type) {
            Assert.Throws<ArgumentNullException>(() => new MappedObject(string.Empty, "remoteId", type, null, null));
        }

        [Test, Category("Fast"), Category("MappedObjects")]
        public void ConstructorThrowsExceptionIfNameIsNull([Values(MappedObjectType.File, MappedObjectType.Folder)]MappedObjectType type) {
            Assert.Throws<ArgumentNullException>(() => new MappedObject(null, "remoteId", type, null, null));
        }

        [Test, Category("Fast"), Category("MappedObjects")]
        public void ConstructorTakesRemoteId([Values(MappedObjectType.File, MappedObjectType.Folder)]MappedObjectType type) {
            var obj = new MappedObject("name", "remoteId", type, null, null);
            Assert.That(obj.RemoteObjectId, Is.EqualTo("remoteId"));
        }

        [Test, Category("Fast"), Category("MappedObjects")]
        public void ConstructorThrowsExceptionOnEmptyRemoteId([Values(MappedObjectType.File, MappedObjectType.Folder)]MappedObjectType type) {
            Assert.Throws<ArgumentNullException>(() => new MappedObject("name", string.Empty, type, null, null));
        }

        [Test, Category("Fast"), Category("MappedObjects")]
        public void ConstructorThrowsExceptionIfRemoteIdIsNull([Values(MappedObjectType.File, MappedObjectType.Folder)]MappedObjectType type) {
            Assert.Throws<ArgumentNullException>(() => new MappedObject("name", null, type, null, null));
        }

        [Test, Category("Fast"), Category("MappedObjects")]
        public void ConstructorThrowsExceptionIfTypeIsUnknown() {
            Assert.Throws<ArgumentException>(() => new MappedObject("name", "remoteId", MappedObjectType.Unkown, null, null));
        }

        [Test, Category("Fast"), Category("MappedObjects")]
        public void ConstructorTakesType([Values(MappedObjectType.File, MappedObjectType.Folder)]MappedObjectType type) {
            var obj = new MappedObject("name", "remoteId", type, null, null);
            Assert.That(obj.Type, Is.EqualTo(type));
        }

        [Test, Category("Fast"), Category("MappedObjects")]
        public void ConstructorTakesParentId(
            [Values("parentId", null)]string parentId,
            [Values(MappedObjectType.File, MappedObjectType.Folder)]MappedObjectType type) {
            var obj = new MappedObject("name", "id", type, parentId, null);
            Assert.That(obj.ParentId, Is.EqualTo(parentId));
        }

        [Test, Category("Fast"), Category("MappedObjects")]
        public void ConstructorTakesChangeLogToken([Values(MappedObjectType.File, MappedObjectType.Folder)]MappedObjectType type) {
            var obj = new MappedObject("name", "id", type, "parentId", "changes");
            Assert.That(obj.LastChangeToken, Is.EqualTo("changes"));
        }

        [Test, Category("Fast"), Category("MappedObjects")]
        public void HashAlgorithmProperty() {
            var file = new MappedObject("name", "remoteId", MappedObjectType.File, null, null) { ChecksumAlgorithmName = "MD5" };
            Assert.AreEqual("MD5", file.ChecksumAlgorithmName);

            file.ChecksumAlgorithmName = "SHA-1";
            Assert.AreEqual("SHA-1", file.ChecksumAlgorithmName);
        }

        [Test, Category("Fast"), Category("MappedObjects")]
        public void DescriptionProperty([Values(MappedObjectType.File, MappedObjectType.Folder)]MappedObjectType type) {
            var file = new MappedObject("name", "remoteId", type, null, null) { Description = "desc" };
            Assert.AreEqual("desc", file.Description);

            file.Description = "other desc";
            Assert.AreEqual("other desc", file.Description);
        }

        [Test, Category("Fast"), Category("MappedObjects")]
        public void IgnoredProperty([Values(MappedObjectType.File, MappedObjectType.Folder)]MappedObjectType type) {
            var obj = new MappedObject("name", "id", type, null, null) { Ignored = true };
            Assert.That(obj.Ignored, Is.True);
        }

        [Test, Category("Fast"), Category("MappedObjects")]
        public void ReadOnlyProperty([Values(MappedObjectType.File, MappedObjectType.Folder)]MappedObjectType type) {
            var obj = new MappedObject("name", "id", type, null, null) { IsReadOnly = true };
            Assert.That(obj.IsReadOnly, Is.True);
        }

        [Test, Category("Fast"), Category("MappedObjects")]
        public void RetriesDictionaryProperty() {
            var dict = new Dictionary<OperationType, int>();
            dict.Add(OperationType.Download, 1);
            var obj = new MappedObject("name", "id", MappedObjectType.File, null, null) { Retries = dict };
            Assert.That(obj.Retries, Is.EqualTo(dict));
            Assert.That(obj.Retries[OperationType.Download], Is.EqualTo(1));
        }

        [Test, Category("Fast"), Category("MappedObjects")]
        public void LastTimeStoredInStorageProperty([Values(MappedObjectType.File, MappedObjectType.Folder)]MappedObjectType type) {
            var date = DateTime.Now;
            var underTest = new MappedObject("name", "id", type, null, null) {
                LastTimeStoredInStorage = date
            };
            Assert.That(underTest.LastTimeStoredInStorage, Is.EqualTo(date));
        }

        [Test, Category("Fast"), Category("MappedObjects")]
        public void IFolderConstructor() {
            string folderName = "a";
            string path = Path.Combine(Path.GetTempPath(), folderName);
            string id = "id";
            string parentId = "papa";
            string lastChangeToken = "token";
            Mock<IFolder> remoteObject = MockOfIFolderUtil.CreateRemoteFolderMock(id, folderName, path, parentId, lastChangeToken);
            MappedObject mappedObject = new MappedObject(remoteObject.Object);
            Assert.That(mappedObject.RemoteObjectId, Is.EqualTo(id), "RemoteObjectId incorrect");
            Assert.That(mappedObject.Name, Is.EqualTo(folderName), "Name incorrect");
            Assert.That(mappedObject.ParentId, Is.EqualTo(parentId), "ParentId incorrect");
            Assert.That(mappedObject.LastChangeToken, Is.EqualTo(lastChangeToken), "LastChangeToken incorrect");
            Assert.That(mappedObject.Type, Is.EqualTo(MappedObjectType.Folder), "Type incorrect");
        }

        private Mock<IFileSystemInfoFactory> CreateFactoryWithLocalPathInfos() {
            return MappedObjectMockUtils.CreateFsFactory(this.localRootPath, this.localRootPathName, this.localFilePath, this.localFileName);
        }

        public class MappedObjectMockUtils {
            public static Mock<IFileSystemInfoFactory> CreateFsFactory(string localRootPath, string localRootPathName, string localFilePath = null, string localFileName = null) {
                var factory = new Mock<IFileSystemInfoFactory>();
                var dirinfo = new Mock<IDirectoryInfo>();
                dirinfo.Setup(dir => dir.Name).Returns(localRootPathName);
                dirinfo.Setup(dir => dir.Exists).Returns(true);
                factory.Setup(f => f.CreateDirectoryInfo(It.Is<string>(path => path == localRootPath))).Returns(dirinfo.Object);
                var fileInfo = new Mock<IFileInfo>();
                fileInfo.Setup(file => file.Name).Returns(localFileName);
                fileInfo.Setup(file => file.Exists).Returns(true);
                factory.Setup(f => f.CreateFileInfo(It.Is<string>(path => path == localFilePath))).Returns(fileInfo.Object);
                return factory;
            }
        }
    }
}