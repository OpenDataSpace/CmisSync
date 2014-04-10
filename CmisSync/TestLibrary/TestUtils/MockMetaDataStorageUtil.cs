using System;
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
    public static class MockMetaDataStorageUtil {
        public static Mock<IMetaDataStorage> GetMetaStorageMockWithToken(string token = "lastToken"){
            var storage = new Mock<IMetaDataStorage>();
            storage.Setup (db => db.ChangeLogToken ).Returns (token);
            return storage;
        }
        

        public static void AddLocalFile(this Mock<IMetaDataStorage> db, string path, string id) {
            var file = Mock.Of<IFileInfo>(f =>
                                          f.FullName == path &&
                                          f.Exists == true);
            db.AddLocalFile(file, id);
        }

        public static void AddLocalFile(this Mock<IMetaDataStorage> db, IFileInfo path, string id ) {
            var file = Mock.Of<IMappedFile>( f =>
                                            f.RemoteObjectId == id &&
                                            f.GetLocalPath() == path.FullName &&
                                            f.ExistsLocally() == path.Exists);
            db.AddMappedFile(file);
        }

        public static Mock<IMappedFolder> AddLocalFolder( this Mock<IMetaDataStorage> db, string path, string id) {
            var folder = Mock.Of<IDirectoryInfo>(d => d.FullName == path);
            return db.AddLocalFolder(folder, id);
        }

        public static Mock<IMappedFolder> AddLocalFolder(this Mock<IMetaDataStorage> db, IDirectoryInfo path, string id ) {
            var folder = new Mock<IMappedFolder>();
            folder.Setup(f => f.GetLocalPath()).Returns(path.FullName);
            folder.Setup (f => f.RemoteObjectId).Returns(id);
            db.AddMappedFolder(folder.Object);
            return folder;
        }

        public static void AddMappedFile(this Mock<IMetaDataStorage> db, IMappedFile file) {
            db.Setup( foo => foo.GetObjectByLocalPath(It.Is<IFileInfo>(s => s.FullName == file.GetLocalPath()))).Returns(file);
            db.Setup( foo => foo.GetObjectByRemoteId(It.Is<string>(s => s == file.RemoteObjectId))).Returns(file);
        }

        public static void AddMappedFolder(this Mock<IMetaDataStorage> db, IMappedFolder folder) {
            db.Setup( foo => foo.GetObjectByLocalPath(It.Is<IDirectoryInfo>(s => s.FullName == folder.GetLocalPath()))).Returns(folder);
            db.Setup( foo => foo.GetObjectByRemoteId(It.Is<string>(s => s == folder.RemoteObjectId))).Returns(folder);
        }
    }
}
