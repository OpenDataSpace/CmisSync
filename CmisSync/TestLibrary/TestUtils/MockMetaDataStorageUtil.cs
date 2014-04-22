using System;
using System.IO;
using System.Collections.Generic;

using CmisSync.Lib;
using CmisSync.Lib.Events;
using CmisSync.Lib.Sync.Strategy;
using CmisSync.Lib.Data;

using DotCMIS.Client;
using DotCMIS.Data;
using DotCMIS.Data.Extensions;
using DotCMIS.Binding.Services;
using CmisSync.Lib.Storage;
using Moq;

namespace TestLibrary.TestUtils
{
    public static class MockMetaDataStorageUtil
    {
        public static Mock<IMetaDataStorage> GetMetaStorageMockWithToken(string token = "lastToken")
        {
            var storage = new Mock<IMetaDataStorage>();
            storage.Setup (db => db.ChangeLogToken ).Returns (token);
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

        public static void AddLocalFile(this Mock<IMetaDataStorage> db, IFileInfo path, string id )
        {
            var file = Mock.Of<IMappedObject>( f =>
                                              f.RemoteObjectId == id &&
                                              f.Name == path.Name &&
                                              f.Type == MappedObjectType.File);
            db.AddMappedFile(file, path.FullName);
        }

        public static Mock<IMappedObject> AddLocalFolder( this Mock<IMetaDataStorage> db, string path, string id)
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
            db.Setup( foo => foo.GetObjectByLocalPath(It.Is<IFileInfo>(f => f.FullName == localPath))).Returns(file);
            db.Setup( foo => foo.GetObjectByRemoteId(It.Is<string>(s => s == file.RemoteObjectId))).Returns(file);
            db.Setup( foo => foo.GetLocalPath(It.Is<IMappedObject>(o => o.Equals(file)))).Returns(localPath);
            db.Setup( foo => foo.GetRemotePath(It.Is<IMappedObject>(o => o.Equals(file)))).Returns(remotePath);
        }

        public static void AddMappedFolder(this Mock<IMetaDataStorage> db, IMappedObject folder, string localPath = null, string remotePath = null) {
            db.Setup( foo => foo.GetObjectByLocalPath(It.Is<IDirectoryInfo>(s => s.Name == folder.Name))).Returns(folder);
            db.Setup( foo => foo.GetObjectByRemoteId(It.Is<string>(s => s == folder.RemoteObjectId))).Returns(folder);
            db.Setup( foo => foo.GetLocalPath(It.Is<IMappedObject>(o => o.Equals(folder)))).Returns(localPath);
            db.Setup( foo => foo.GetRemotePath(It.Is<IMappedObject>(o => o.Equals(folder)))).Returns(remotePath);
        }
    }
}
