//-----------------------------------------------------------------------
// <copyright file="MetaDataStorageTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.StorageTests
{
    using System;
    using System.IO;

    using CmisSync.Lib.Data;
    using CmisSync.Lib.Storage;

    using DBreeze;

    using Moq;

    using Newtonsoft.Json;

    using NUnit.Framework;

    [TestFixture]
    public class MetaDataStorageTest
    {
        private readonly IPathMatcher matcher = Mock.Of<IPathMatcher>();
        private DBreezeEngine engine;

        [TestFixtureSetUp]
        public void InitCustomSerializator()
        {
            // Use Newtonsoft.Json as Serializator
            DBreeze.Utils.CustomSerializator.Serializator = JsonConvert.SerializeObject; 
            DBreeze.Utils.CustomSerializator.Deserializator = JsonConvert.DeserializeObject;
        }

        [SetUp]
        public void SetUp()
        {
            this.engine = new DBreezeEngine(new DBreezeConfiguration { Storage = DBreezeConfiguration.eStorage.MEMORY });
        }

        [TearDown]
        public void TearDown()
        {
            this.engine.Dispose();
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorFailsWithNoEngineAndNoMatcher()
        {
            new MetaDataStorage(null, null);
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorFailsWithNoEngine()
        {
            new MetaDataStorage(null, this.matcher);
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorFailsWithNoMatcher()
        {
            new MetaDataStorage(this.engine, null);
        }

        [Test, Category("Fast")]
        public void ConstructorTakesEngineWithoutFailure()
        {
            new MetaDataStorage(this.engine, this.matcher);
        }

        [Test, Category("Fast")]
        public void SetAndGetContentChangeToken()
        {
            string token = "token";
            var storage = new MetaDataStorage(this.engine, this.matcher);
            storage.ChangeLogToken = token;
            Assert.That(storage.ChangeLogToken, Is.EqualTo(token));
        }

        [Test, Category("Fast")]
        public void GetTokenFromEmptyStorageMustBeNull()
        {
            var storage = new MetaDataStorage(this.engine, this.matcher);
            Assert.That(storage.ChangeLogToken, Is.Null);
        }

        [Test, Category("Fast")]
        public void GetObjectByIdWithNotExistingIdMustReturnNull()
        {
            var storage = new MetaDataStorage(this.engine, this.matcher);
            Assert.That(storage.GetObjectByRemoteId("DOESNOTEXIST"), Is.Null);
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetObjectByPathThrowsExceptionOnNullArgument()
        {
            var storage = new MetaDataStorage(this.engine, this.matcher);
            storage.GetObjectByLocalPath(null);
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentException))]
        public void GetObjectByPathThrowsExceptionIfLocalPathDoesNotMatchToSyncPath()
        {
            var matcher = new Mock<IPathMatcher>();
            string localpath = Path.GetTempPath();
            var folder = Mock.Of<IDirectoryInfo>(f =>
                                                 f.FullName == localpath);
            matcher.Setup(m => m.CanCreateRemotePath(It.Is<string>(f => f == localpath))).Returns(false);
            var storage = new MetaDataStorage(this.engine, matcher.Object);

            storage.GetObjectByLocalPath(folder);
        }

        [Test, Category("Fast")]
        public void GetObjectByPathWithNotExistingEntryMustReturnNull()
        {
            var matcher = new Mock<IPathMatcher>();
            string testfilename = "test";
            string testpath = Path.Combine(Path.GetTempPath(), testfilename);
            matcher.Setup(m => m.CanCreateRemotePath(It.Is<string>(f => f == testpath))).Returns(true);
            matcher.Setup(m => m.GetRelativeLocalPath(It.Is<string>(f => f == testpath))).Returns(testfilename);
            var storage = new MetaDataStorage(this.engine, matcher.Object);

            var path = Mock.Of<IFileSystemInfo>(p =>
                                                p.FullName == testpath);
            Assert.That(storage.GetObjectByLocalPath(path), Is.Null);
        }

        [Test, Category("Fast")]
        public void GetObjectByPath()
        {
            var matcher = new Mock<IPathMatcher>();
            matcher.Setup(m => m.LocalTargetRootPath).Returns(Path.GetTempPath());
            matcher.Setup(m => m.CanCreateRemotePath(It.Is<string>(f => f == Path.Combine(Path.GetTempPath(), "a")))).Returns(true);
            matcher.Setup(m => m.GetRelativeLocalPath(It.Is<string>(p => p == Path.Combine(Path.GetTempPath(), "a")))).Returns("a");
            var storage = new MetaDataStorage(this.engine, matcher.Object);
            var folder = Mock.Of<IDirectoryInfo>(f =>
                                                 f.FullName == Path.Combine(Path.GetTempPath(), "a"));
            var mappedFolder = new MappedObject("a", "remoteId", MappedObjectType.Folder, null, null)
            {
                Guid = Guid.NewGuid(),
            };
            storage.SaveMappedObject(mappedFolder);

            var obj = storage.GetObjectByLocalPath(folder);

            Assert.That(obj, Is.EqualTo(mappedFolder));
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(EntryNotFoundException))]
        public void GetChildrenOfNonExistingParentMustThrowException()
        {
            var storage = new MetaDataStorage(this.engine, this.matcher);
            storage.GetChildren(Mock.Of<IMappedObject>(o => o.RemoteObjectId == "DOESNOTEXIST"));
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetChildrenThrowsExceptionOnNullArgument()
        {
            var storage = new MetaDataStorage(this.engine, this.matcher);
            storage.GetChildren(null);
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentException))]
        public void GetChildrenThrowsExceptionIfMappedObjectDoesNotContainsId()
        {
            var storage = new MetaDataStorage(this.engine, this.matcher);
            storage.GetChildren(Mock.Of<IMappedObject>());
        }

        [Test, Category("Fast")]
        public void GetChildrenReturnsEmptyListIfNoChildrenAreAvailable()
        {
            var storage = new MetaDataStorage(this.engine, this.matcher);
            var folder = new MappedObject("name", "id", MappedObjectType.Folder, null, null);
            storage.SaveMappedObject(folder);

            Assert.That(storage.GetChildren(folder).Count == 0);
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SaveMappedObjectThrowsExceptionOnNullArgument()
        {
            var storage = new MetaDataStorage(this.engine, this.matcher);
            storage.SaveMappedObject(null);
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentException))]
        public void SaveMappedObjectThrowsExceptionOnNonExistingIdInObject()
        {
            var storage = new MetaDataStorage(this.engine, this.matcher);
            storage.SaveMappedObject(Mock.Of<IMappedObject>());
        }

        [Test, Category("Fast")]
        public void SaveObjectAndGetObjectReturnEqualObject()
        {
            var storage = new MetaDataStorage(this.engine, this.matcher);
            string remoteId = "remoteId";
            var folder = new MappedObject("folder", remoteId, MappedObjectType.Folder, null, null)
            {
                Description = "desc",
                Guid = Guid.NewGuid(),
            };

            storage.SaveMappedObject(folder);
            var obj = storage.GetObjectByRemoteId(remoteId);

            Assert.That(obj.Equals(folder));
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RemoveObjectThrowsExceptionOnNullArgument()
        {
            var storage = new MetaDataStorage(this.engine, this.matcher);
            storage.RemoveObject(null);
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentException))]
        public void RemoveObjectThrowsExceptionOnNonExistingIdInObject()
        {
            var storage = new MetaDataStorage(this.engine, this.matcher);
            storage.RemoveObject(Mock.Of<IMappedObject>());
        }

        [Test, Category("Fast")]
        public void RemoveObjectTest()
        {
            string remoteId = "remoteId";
            var storage = new MetaDataStorage(this.engine, this.matcher);
            var obj = new MappedObject("name", remoteId, MappedObjectType.Folder, null, null);
            storage.SaveMappedObject(obj);

            storage.RemoveObject(obj);

            Assert.That(storage.GetObjectByRemoteId(remoteId), Is.Null);
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetLocalPathThrowsExceptionOnNullArgument()
        {
            var storage = new MetaDataStorage(this.engine, this.matcher);
            storage.GetLocalPath(null);
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentException))]
        public void GetLocalPathThrowsExceptionOnNonExistingIdInObject()
        {
            var storage = new MetaDataStorage(this.engine, this.matcher);
            storage.GetLocalPath(Mock.Of<IMappedObject>());
        }

        [Test, Category("Fast")]
        public void GetLocalPath()
        {
            var matcher = new Mock<IPathMatcher>();
            matcher.Setup(m => m.LocalTargetRootPath).Returns(Path.GetTempPath());
            var storage = new MetaDataStorage(this.engine, matcher.Object);
            string id = "remoteId";
            var rootFolder = new MappedObject("name", id, MappedObjectType.Folder, null, null);
            storage.SaveMappedObject(rootFolder);

            string path = storage.GetLocalPath(rootFolder);

            Assert.That(path, Is.EqualTo(Path.Combine(Path.GetTempPath(), "name")));
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetRemotePathThrowsExceptionOnNullArgument()
        {
            var storage = new MetaDataStorage(this.engine, this.matcher);
            storage.GetRemotePath(null);
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentException))]
        public void GetRemotePathThrowsExceptionOnNonExistingIdInObject()
        {
            var storage = new MetaDataStorage(this.engine, this.matcher);
            storage.GetRemotePath(Mock.Of<IMappedObject>());
        }

        [Test, Category("Fast")]
        public void GetRemotePath()
        {
            var matcher = new Mock<IPathMatcher>();
            matcher.Setup(m => m.RemoteTargetRootPath).Returns("/");
            var storage = new MetaDataStorage(this.engine, matcher.Object);
            var remoteFolder = new MappedObject("remoteFolder", "remoteId", MappedObjectType.Folder, null, null);
            storage.SaveMappedObject(remoteFolder);

            string remotePath = storage.GetRemotePath(remoteFolder);

            Assert.That(remotePath, Is.EqualTo("/remoteFolder"));
        }

        [Test, Category("Fast")]
        public void FindRootFolder()
        {
            string id = "id";
            string path = Path.GetTempPath();
            var fsInfo = new DirectoryInfoWrapper(new DirectoryInfo(path));
            var matcher = new PathMatcher(path, "/");
            var storage = new MetaDataStorage(this.engine, matcher);
            var rootFolder = new MappedObject("/", id, MappedObjectType.Folder, null, "token");
            storage.SaveMappedObject(rootFolder);
            
            Assert.That(storage.GetObjectByRemoteId(id), Is.Not.Null, "Not findable by ID");
            Assert.That(storage.GetObjectByLocalPath(fsInfo), Is.Not.Null, "Not findable by path");
        }
    }
}
