//-----------------------------------------------------------------------
// <copyright file="MockMetaDataStorageUtil.cs" company="GRAU DATA AG">
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

namespace TestLibrary.TestUtils {
    using System;
    using System.Collections.Generic;
    using System.IO;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Producer.Watcher;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Binding.Services;
    using DotCMIS.Client;
    using DotCMIS.Data;
    using DotCMIS.Data.Extensions;

    using Moq;

    using NUnit.Framework;

    public static class MockMetaDataStorageUtil {
        public static Mock<IMetaDataStorage> GetMetaStorageMockWithToken(string token = "lastToken") {
            var storage = new Mock<IMetaDataStorage>();
            storage.Setup(db => db.ChangeLogToken).Returns(token);
            return storage;
        }

        public static Mock<IMappedObject> AddLocalFile(this Mock<IMetaDataStorage> db, string path, string id) {
            var file = Mock.Of<IFileInfo>(
                f =>
                f.Name == Path.GetFileName(path) &&
                f.FullName == path &&
                f.Exists == true);
            return db.AddLocalFile(file, id);
        }

        public static void AddLocalFile(this Mock<IMetaDataStorage> db, string path, string id, Guid uuid) {
            var file = new Mock<IMappedObject>();
            file.SetupAllProperties();
            file.Setup(o => o.Type).Returns(MappedObjectType.File);
            file.Object.RemoteObjectId = id;
            file.Object.Name = Path.GetFileName(path);
            file.Object.Guid = uuid;
            db.AddMappedFile(file.Object, path);
        }

        public static Mock<IMappedObject> AddLocalFile(this Mock<IMetaDataStorage> db, IFileInfo path, string id) {
            var file = new Mock<IMappedObject>();
            file.SetupAllProperties();
            file.Setup(o => o.Type).Returns(MappedObjectType.File);
            file.Object.RemoteObjectId = id;
            file.Object.Name = path.Name;
            db.AddMappedFile(file.Object, path.FullName);
            return file;
        }

        public static Mock<IMappedObject> AddLocalFolder(this Mock<IMetaDataStorage> db, string path, string id) {
            var folder = Mock.Of<IDirectoryInfo>(
                d =>
                d.FullName == path &&
                d.Name == Path.GetDirectoryName(path));
            return db.AddLocalFolder(folder, id);
        }

        public static void AddLocalFolder(this Mock<IMetaDataStorage> storage, string path, string id, Guid uuid) {
            var folder = new Mock<IMappedObject>();
            folder.SetupAllProperties();
            folder.Setup(o => o.Type).Returns(MappedObjectType.Folder);
            folder.Object.Name = Path.GetDirectoryName(path);
            folder.Object.RemoteObjectId = id;
            folder.Object.Guid = uuid;
            storage.AddMappedFolder(folder.Object, path);
        }

        public static Mock<IMappedObject> AddLocalFolder(this Mock<IMetaDataStorage> db, IDirectoryInfo path, string id) {
            var folder = new Mock<IMappedObject>();
            folder.SetupAllProperties();
            folder.Setup(o => o.Type).Returns(MappedObjectType.Folder);
            folder.Object.Name = path.Name;
            folder.Object.RemoteObjectId = id;
            db.AddMappedFolder(folder.Object, path.FullName);
            return folder;
        }

        public static void AddMappedFile(
            this Mock<IMetaDataStorage> db,
            IMappedObject file,
            string localPath = null,
            string remotePath = null)
        {
            db.Setup(foo => foo.GetObjectByLocalPath(It.Is<IFileInfo>(f => f.FullName == localPath))).Returns(file);
            db.Setup(foo => foo.GetObjectByRemoteId(It.Is<string>(s => s == file.RemoteObjectId))).Returns(file);
            db.Setup(foo => foo.GetLocalPath(It.Is<IMappedObject>(o => o.Equals(file)))).Returns(localPath);
            db.Setup(foo => foo.GetRemotePath(It.Is<IMappedObject>(o => o.Equals(file)))).Returns(remotePath);
            if (!file.Guid.Equals(Guid.Empty)) {
                db.Setup(foo => foo.GetObjectByGuid(It.Is<Guid>(g => g.Equals(file.Guid)))).Returns(file);
            }
        }

        // Don't use this method twice per test
        public static void AddMappedFolder(
            this Mock<IMetaDataStorage> db,
            IMappedObject folder,
            string localPath = null,
            string remotePath = null)
        {
            db.Setup(foo => foo.GetObjectByLocalPath(It.Is<IFileSystemInfo>(d => d.FullName == localPath))).Returns(folder);
            db.Setup(foo => foo.GetObjectByLocalPath(It.Is<IDirectoryInfo>(d => d.FullName == localPath))).Returns(folder);
            db.Setup(foo => foo.GetObjectByLocalPath(It.Is<IFileInfo>(d => d.FullName == localPath))).Returns(folder);
            db.Setup(foo => foo.GetObjectByRemoteId(It.Is<string>(s => s == folder.RemoteObjectId))).Returns(folder);
            db.Setup(foo => foo.GetLocalPath(It.Is<IMappedObject>(o => o.Equals(folder)))).Returns(localPath);
            db.Setup(foo => foo.GetRemotePath(It.Is<IMappedObject>(o => o.Equals(folder)))).Returns(remotePath);
            if (!folder.Guid.Equals(Guid.Empty)) {
                db.Setup(foo => foo.GetObjectByGuid(It.Is<Guid>(g => g.Equals(folder.Guid)))).Returns(folder);
            }
        }

        public static void VerifyThatNoObjectIsManipulated(this Mock<IMetaDataStorage> db) {
            db.Verify(s => s.SaveMappedObject(It.IsAny<IMappedObject>()), Times.Never());
            db.Verify(s => s.RemoveObject(It.IsAny<IMappedObject>()), Times.Never());
        }

        public static void VerifySavedMappedObject(
            this Mock<IMetaDataStorage> db,
            MappedObjectType type,
            string remoteId,
            string name,
            string parentId,
            string changeToken,
            bool extendedAttributeAvailable = true,
            DateTime? lastLocalModification = null,
            DateTime? lastRemoteModification = null,
            byte[] checksum = null,
            long contentSize = -1,
            bool ignored = false,
            bool readOnly = false)
        {
            VerifySavedMappedObject(
                db,
                type,
                remoteId,
                name,
                parentId,
                changeToken,
                Times.Once(),
                extendedAttributeAvailable,
                lastLocalModification,
                lastRemoteModification,
                checksum,
                contentSize,
                ignored,
                readOnly);
        }

        public static void VerifySavedMappedObject(
            this Mock<IMetaDataStorage> db,
            MappedObjectType type,
            string remoteId,
            string name,
            string parentId,
            string changeToken,
            Times times,
            bool extendedAttributeAvailable = true,
            DateTime? lastLocalModification = null,
            DateTime? lastRemoteModification = null,
            byte[] checksum = null,
            long contentSize = -1,
            bool ignored = false,
            bool readOnly = false)
        {
            db.Verify(
                s =>
                s.SaveMappedObject(
                    It.Is<IMappedObject>(
                        o => VerifyMappedObject(
                            o,
                            type,
                            remoteId,
                            name,
                            parentId,
                            changeToken,
                            extendedAttributeAvailable,
                            lastLocalModification,
                            lastRemoteModification,
                            checksum,
                            contentSize,
                            ignored,
                            readOnly))),
                times);
        }

        private static bool VerifyMappedObject(
            IMappedObject o,
            MappedObjectType type,
            string remoteId,
            string name,
            string parentId,
            string changeToken,
            bool extendedAttributeAvailable,
            DateTime? lastLocalModification,
            DateTime? lastRemoteModification,
            byte[] checksum,
            long contentSize,
            bool ignored,
            bool readOnly)
        {
            Assert.That(o.RemoteObjectId, Is.EqualTo(remoteId), "Object remote Id is wrong");
            Assert.That(o.Name, Is.EqualTo(name), "Object name is wrong");
            Assert.That(o.ParentId, Is.EqualTo(parentId), "Object parent Id is wrong");
            Assert.That(o.LastChangeToken, Is.EqualTo(changeToken), "Object change token is wrong");
            Assert.That(o.Type, Is.EqualTo(type), "Object type is wrong");
            Assert.That(o.Ignored, Is.EqualTo(ignored), "Object ignore flag is wrong");
            Assert.That(o.IsReadOnly, Is.EqualTo(readOnly));
            if (extendedAttributeAvailable) {
                Assert.That(o.Guid, Is.Not.EqualTo(Guid.Empty), "Given Guid must not be empty");
            } else {
                Assert.That(o.Guid, Is.EqualTo(Guid.Empty), "Given Guid must be empty");
            }

            if (lastLocalModification != null) {
                Assert.That(o.LastLocalWriteTimeUtc, Is.EqualTo(lastLocalModification), "Last local modification date is wrong");
            }

            if (lastRemoteModification != null) {
                Assert.That(o.LastRemoteWriteTimeUtc, Is.EqualTo(lastRemoteModification), "Last remote modification date is wrong");
            }

            if (checksum != null) {
                Assert.That(o.ChecksumAlgorithmName, Is.EqualTo("SHA-1"));
                Assert.That(o.LastChecksum, Is.EqualTo(checksum), "Given checksum is not equal to last saved checksum");
            }

            if (type == MappedObjectType.File) {
                Assert.That(o.LastContentSize, Is.GreaterThanOrEqualTo(0), "Last content size is wrong");
                Assert.That(o.LastContentSize, Is.EqualTo(contentSize), "Last content size is wrong");
            }

            if (type == MappedObjectType.Folder) {
                Assert.That(o.LastContentSize, Is.EqualTo(-1), "Folder content size is wrong");
            }

            return true;
        }
    }
}