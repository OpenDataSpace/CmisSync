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

namespace TestLibrary.TestUtils
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using CmisSync.Lib;
    using CmisSync.Lib.Data;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Storage;
    using CmisSync.Lib.Sync.Strategy;

    using DotCMIS.Binding.Services;
    using DotCMIS.Client;
    using DotCMIS.Data;
    using DotCMIS.Data.Extensions;

    using Moq;

    using NUnit.Framework;

    public static class MockMetaDataStorageUtil
    {
        public static Mock<IMetaDataStorage> GetMetaStorageMockWithToken(string token = "lastToken")
        {
            var storage = new Mock<IMetaDataStorage>();
            storage.Setup(db => db.ChangeLogToken).Returns(token);
            return storage;
        }

        public static void AddLocalFile(this Mock<IMetaDataStorage> db, string path, string id)
        {
            var file = Mock.Of<IFileInfo>(f =>
                                          f.Name == Path.GetFileName(path) &&
                                          f.FullName == path &&
                                          f.Exists == true);
            db.AddLocalFile(file, id);
        }

        public static void AddLocalFile(this Mock<IMetaDataStorage> db, IFileInfo path, string id)
        {
            var file = Mock.Of<IMappedObject>(f =>
                                              f.RemoteObjectId == id &&
                                              f.Name == path.Name &&
                                              f.Type == MappedObjectType.File);
            db.AddMappedFile(file, path.FullName);
        }

        public static Mock<IMappedObject> AddLocalFolder(this Mock<IMetaDataStorage> db, string path, string id)
        {
            var folder = Mock.Of<IDirectoryInfo>(d =>
                                                 d.FullName == path &&
                                                 d.Name == Path.GetDirectoryName(path));
            return db.AddLocalFolder(folder, id);
        }

        public static Mock<IMappedObject> AddLocalFolder(this Mock<IMetaDataStorage> db, IDirectoryInfo path, string id)
        {
            var folder = new Mock<IMappedObject>();
            folder.Setup(f => f.Name).Returns(path.Name);
            folder.Setup(f => f.RemoteObjectId).Returns(id);
            folder.Setup(f => f.Type).Returns(MappedObjectType.Folder);
            db.AddMappedFolder(folder.Object, path.FullName);
            return folder;
        }

        public static void AddMappedFile(this Mock<IMetaDataStorage> db, IMappedObject file, string localPath = null, string remotePath = null)
        {
            db.Setup(foo => foo.GetObjectByLocalPath(It.Is<IFileInfo>(f => f.FullName == localPath))).Returns(file);
            db.Setup(foo => foo.GetObjectByRemoteId(It.Is<string>(s => s == file.RemoteObjectId))).Returns(file);
            db.Setup(foo => foo.GetLocalPath(It.Is<IMappedObject>(o => o.Equals(file)))).Returns(localPath);
            db.Setup(foo => foo.GetRemotePath(It.Is<IMappedObject>(o => o.Equals(file)))).Returns(remotePath);
        }

        // Don't use this method twice per test
        public static void AddMappedFolder(this Mock<IMetaDataStorage> db, IMappedObject folder, string localPath = null, string remotePath = null) {
            db.Setup(foo => foo.GetObjectByLocalPath(It.IsAny<IDirectoryInfo>())).Returns(folder);
            db.Setup(foo => foo.GetObjectByRemoteId(It.Is<string>(s => s == folder.RemoteObjectId))).Returns(folder);
            db.Setup(foo => foo.GetLocalPath(It.Is<IMappedObject>(o => o.Equals(folder)))).Returns(localPath);
            db.Setup(foo => foo.GetRemotePath(It.Is<IMappedObject>(o => o.Equals(folder)))).Returns(remotePath);
        }

        public static void VerifySavedMappedObject(this Mock<IMetaDataStorage> db, MappedObjectType type, string remoteId, string name, string parentId, string changeToken, bool extendedAttributeAvailable = true, DateTime? lastModification = null)
        {
            VerifySavedMappedObject(db, type, remoteId, name, parentId, changeToken, Times.Once(), extendedAttributeAvailable, lastModification);
        }

        public static void VerifySavedMappedObject(this Mock<IMetaDataStorage> db, MappedObjectType type, string remoteId, string name, string parentId, string changeToken, Times times, bool extendedAttributeAvailable = true, DateTime? lastModification = null)
        {
            db.Verify(s => s.SaveMappedObject(It.Is<IMappedObject>(o => VerifyMappedObject(o, type, remoteId, name, parentId, changeToken, times, extendedAttributeAvailable, lastModification))), times);
        }

        private static bool VerifyMappedObject(IMappedObject o, MappedObjectType type, string remoteId, string name, string parentId, string changeToken, Times times, bool extendedAttributeAvailable, DateTime? lastModification)
        {
            Assert.That(o.RemoteObjectId, Is.EqualTo(remoteId));
            Assert.That(o.Name, Is.EqualTo(name));
            Assert.That(o.ParentId, Is.EqualTo(parentId));
            Assert.That(o.LastChangeToken, Is.EqualTo(changeToken));
            Assert.That(o.Type, Is.EqualTo(type));
            if (extendedAttributeAvailable) {
                Assert.That(o.Guid, Is.Not.EqualTo(Guid.Empty));
            }
            else
            {
                Assert.That(o.Guid, Is.EqualTo(Guid.Empty));
            }

            if (lastModification != null) {
                Assert.That(o.LastLocalWriteTimeUtc, Is.EqualTo(lastModification));
                Assert.That(o.LastRemoteWriteTimeUtc, Is.EqualTo(lastModification));
            }

            return true;
        }
    }
}
