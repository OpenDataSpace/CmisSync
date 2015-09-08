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

namespace TestLibrary.StorageTests.DataBaseTests {
    using System;
    using System.IO;
    using System.Linq;
#if !__MonoCS__
    using FileInfo = Alphaleonis.Win32.Filesystem.FileInfo;
    using DirectoryInfo = Alphaleonis.Win32.Filesystem.DirectoryInfo;
    using Path = Alphaleonis.Win32.Filesystem.Path;
#endif
    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.PathMatcher;
    using CmisSync.Lib.Storage.FileSystem;
    using CmisSync.Lib.Storage.Database;

    using DBreeze;

    using Moq;

    using Newtonsoft.Json;

    using NUnit.Framework;

    [TestFixture]
    public class MetaDataStorageTest : IDisposable {
        private readonly IPathMatcher matcher = Mock.Of<IPathMatcher>();
        private DBreezeEngine engine;

        [TestFixtureSetUp]
        public void InitCustomSerializator() {
            // Use Newtonsoft.Json as Serializator
            DBreeze.Utils.CustomSerializator.Serializator = JsonConvert.SerializeObject;
            DBreeze.Utils.CustomSerializator.Deserializator = JsonConvert.DeserializeObject;
        }

        [SetUp]
        public void SetUp() {
            this.engine = new DBreezeEngine(new DBreezeConfiguration { Storage = DBreezeConfiguration.eStorage.MEMORY });
        }

        [TearDown]
        public void TearDown() {
            this.engine.Dispose();
            this.engine = null;
        }

        [Test, Category("Fast")]
        public void ConstructorFailsWithNoEngineAndNoMatcher([Values(true, false)]bool withValidation) {
            Assert.Throws<ArgumentNullException>(() => new MetaDataStorage(null, null, withValidation));
        }

        [Test, Category("Fast")]
        public void ConstructorFailsWithNoEngine([Values(true, false)]bool withValidation) {
            Assert.Throws<ArgumentNullException>(() => new MetaDataStorage(null, this.matcher, withValidation));
        }

        [Test, Category("Fast")]
        public void ConstructorFailsWithNoMatcher([Values(true, false)]bool withValidation) {
            Assert.Throws<ArgumentNullException>(() => new MetaDataStorage(this.engine, null, withValidation));
        }

        [Test, Category("Fast")]
        public void ConstructorTakesEngineWithoutFailure([Values(true, false)]bool withValidation) {
            new MetaDataStorage(this.engine, this.matcher, withValidation);
        }

        [Test, Category("Fast")]
        public void SetAndGetContentChangeToken([Values(true, false)]bool withValidation) {
            string token = "token";
            var storage = new MetaDataStorage(this.engine, this.matcher, withValidation);
            storage.ChangeLogToken = token;
            Assert.That(storage.ChangeLogToken, Is.EqualTo(token));
        }

        [Test, Category("Fast")]
        public void GetTokenFromEmptyStorageMustBeNull([Values(true, false)]bool withValidation) {
            var storage = new MetaDataStorage(this.engine, this.matcher, withValidation);
            Assert.That(storage.ChangeLogToken, Is.Null);
        }

        [Test, Category("Fast")]
        public void GetObjectByIdWithNotExistingIdMustReturnNull([Values(true, false)]bool withValidation) {
            var storage = new MetaDataStorage(this.engine, this.matcher, withValidation);
            Assert.That(storage.GetObjectByRemoteId("DOESNOTEXIST"), Is.Null);
        }

        [Test, Category("Fast")]
        public void GetObjectByPathThrowsExceptionOnNullArgument([Values(true, false)]bool withValidation) {
            var storage = new MetaDataStorage(this.engine, this.matcher, withValidation);
            Assert.Throws<ArgumentNullException>(() => storage.GetObjectByLocalPath(null));
        }

        [Test, Category("Fast")]
        public void GetObjectByPathThrowsExceptionIfLocalPathDoesNotMatchToSyncPath([Values(true, false)]bool withValidation) {
            var matcher = new Mock<IPathMatcher>();
            string localpath = Path.GetTempPath();
            var folder = Mock.Of<IDirectoryInfo>(
                f =>
                f.FullName == localpath);
            matcher.Setup(m => m.CanCreateRemotePath(It.Is<string>(f => f == localpath))).Returns(false);
            var storage = new MetaDataStorage(this.engine, matcher.Object, withValidation);

            Assert.Throws<ArgumentException>(() => storage.GetObjectByLocalPath(folder));
        }

        [Test, Category("Fast")]
        public void GetObjectByPathWithNotExistingEntryMustReturnNull([Values(true, false)]bool withValidation) {
            var matcher = new Mock<IPathMatcher>();
            string testfilename = "test";
            string testpath = Path.Combine(Path.GetTempPath(), testfilename);
            matcher.Setup(m => m.CanCreateRemotePath(It.Is<string>(f => f == testpath))).Returns(true);
            matcher.Setup(m => m.GetRelativeLocalPath(It.Is<string>(f => f == testpath))).Returns(testfilename);
            var storage = new MetaDataStorage(this.engine, matcher.Object, withValidation);

            var path = Mock.Of<IFileSystemInfo>(p => p.FullName == testpath);
            Assert.That(storage.GetObjectByLocalPath(path), Is.Null);
        }

        [Test, Category("Fast")]
        public void GetObjectByPath([Values(true, false)]bool withValidation) {
            var matcher = new Mock<IPathMatcher>();
            matcher.Setup(m => m.LocalTargetRootPath).Returns(Path.GetTempPath());
            matcher.Setup(m => m.CanCreateRemotePath(It.Is<string>(f => f == Path.Combine(Path.GetTempPath(), "a")))).Returns(true);
            matcher.Setup(m => m.GetRelativeLocalPath(It.Is<string>(p => p == Path.Combine(Path.GetTempPath(), "a")))).Returns("a");
            var storage = new MetaDataStorage(this.engine, matcher.Object, withValidation);
            var folder = Mock.Of<IDirectoryInfo>(
                f =>
                f.FullName == Path.Combine(Path.GetTempPath(), "a"));
            var mappedFolder = new MappedObject("a", "remoteId", MappedObjectType.Folder, null, null) {
                Guid = Guid.NewGuid(),
            };
            storage.SaveMappedObject(mappedFolder);

            var obj = storage.GetObjectByLocalPath(folder);

            Assert.That(obj, Is.EqualTo(mappedFolder));
        }

        [Test, Category("Fast")]
        public void GetObjectByPathWithHierarchie([Values(true, false)]bool withValidation) {
            var matcher = new PathMatcher(Path.GetTempPath(), "/");
            var storage = new MetaDataStorage(this.engine, matcher, withValidation);
            var root = Mock.Of<IDirectoryInfo>(f => f.FullName == Path.GetTempPath());
            var folder = Mock.Of<IDirectoryInfo>(
                f =>
                f.FullName == Path.Combine(Path.GetTempPath(), "a"));
            var mappedRoot = new MappedObject("/", "rootId", MappedObjectType.Folder, null, null);
            var mappedFolder = new MappedObject("a", "remoteId", MappedObjectType.Folder, "rootId", null) {
                Guid = Guid.NewGuid(),
            };
            storage.SaveMappedObject(mappedRoot);
            storage.SaveMappedObject(mappedFolder);

            var obj = storage.GetObjectByLocalPath(folder);

            Assert.That(storage.GetObjectByLocalPath(root), Is.EqualTo(mappedRoot));
            Assert.That(obj, Is.EqualTo(mappedFolder));
        }

        [Test, Category("Fast")]
        public void GetChildrenOfNonExistingParentMustThrowException([Values(true, false)]bool withValidation) {
            var storage = new MetaDataStorage(this.engine, this.matcher, withValidation);
            Assert.Throws<EntryNotFoundException>(() => storage.GetChildren(Mock.Of<IMappedObject>(o => o.RemoteObjectId == "DOESNOTEXIST")));
        }

        [Test, Category("Fast")]
        public void GetChildrenThrowsExceptionOnNullArgument([Values(true, false)]bool withValidation) {
            var storage = new MetaDataStorage(this.engine, this.matcher, withValidation);
            Assert.Throws<ArgumentNullException>(() => storage.GetChildren(null));
        }

        [Test, Category("Fast")]
        public void GetChildrenThrowsExceptionIfMappedObjectDoesNotContainsId([Values(true, false)]bool withValidation) {
            var storage = new MetaDataStorage(this.engine, this.matcher, withValidation);
            Assert.Throws<ArgumentException>(() => storage.GetChildren(Mock.Of<IMappedObject>()));
        }

        [Test, Category("Fast")]
        public void GetChildrenReturnsEmptyListIfNoChildrenAreAvailable([Values(true, false)]bool withValidation) {
            var storage = new MetaDataStorage(this.engine, this.matcher, withValidation);
            var folder = new MappedObject("name", "id", MappedObjectType.Folder, null, null);
            storage.SaveMappedObject(folder);

            Assert.That(storage.GetChildren(folder).Count == 0);
        }

        [Test, Category("Fast")]
        public void SaveMappedObjectThrowsExceptionOnNullArgument([Values(true, false)]bool withValidation) {
            var storage = new MetaDataStorage(this.engine, this.matcher, withValidation);
            Assert.Throws<ArgumentNullException>(() => storage.SaveMappedObject(null));
        }

        [Test, Category("Fast")]
        public void SaveMappedObjectThrowsExceptionOnNonExistingIdInObject([Values(true, false)]bool withValidation) {
            var storage = new MetaDataStorage(this.engine, this.matcher, withValidation);
            Assert.Throws<ArgumentException>(() => storage.SaveMappedObject(Mock.Of<IMappedObject>()));
        }

        [Test, Category("Fast")]
        public void SaveFolderObjectAndGetObjectReturnEqualObject([Values(true, false)]bool withValidation) {
            var storage = new MetaDataStorage(this.engine, this.matcher, withValidation);
            string remoteId = "remoteId";
            var folder = new MappedObject("folder", remoteId, MappedObjectType.Folder, null, null) {
                Description = "desc",
                Guid = Guid.NewGuid(),
            };

            storage.SaveMappedObject(folder);
            var obj = storage.GetObjectByRemoteId(remoteId);

            Assert.That(obj.Equals(folder));
        }

        [Test, Category("Fast")]
        public void StoreDateOnStoringMappedObject([Values(true, false)]bool withValidation) {
            var underTest = new MetaDataStorage(this.engine, this.matcher, withValidation);
            IMappedObject obj = new MappedObject("obj", "remoteId", MappedObjectType.File, null, null);
            Assert.That(obj.LastTimeStoredInStorage, Is.Null);
            underTest.SaveMappedObject(obj);
            obj = underTest.GetObjectByRemoteId("remoteId");
            Assert.That(obj.LastTimeStoredInStorage, Is.EqualTo(DateTime.UtcNow).Within(1).Seconds);
        }

        [Test, Category("Fast")]
        public void SaveFileObjectAndGetObjectReturnsEqualObject([Values(true, false)]bool withValidation) {
            var storage = new MetaDataStorage(this.engine, this.matcher, withValidation);
            string remoteId = "remoteId";
            var file = new MappedObject("file", remoteId, MappedObjectType.File, null, null) {
                Description = "desc",
                Guid = Guid.NewGuid(),
                LastChecksum = new byte[20]
            };

            storage.SaveMappedObject(file);
            var obj = storage.GetObjectByRemoteId(remoteId);

            Assert.That(obj.LastChecksum, Is.Not.Null);
            Assert.That(obj.Equals(file));
        }

        [Test, Category("Fast")]
        public void RemoveObjectThrowsExceptionOnNullArgument([Values(true, false)]bool withValidation) {
            var storage = new MetaDataStorage(this.engine, this.matcher, withValidation);
            Assert.Throws<ArgumentNullException>(() => storage.RemoveObject(null));
        }

        [Test, Category("Fast")]
        public void RemoveObjectThrowsExceptionOnNonExistingIdInObject([Values(true, false)]bool withValidation) {
            var storage = new MetaDataStorage(this.engine, this.matcher, withValidation);
            Assert.Throws<ArgumentException>(() => storage.RemoveObject(Mock.Of<IMappedObject>()));
        }

        [Test, Category("Fast")]
        public void RemoveObjectTest([Values(true, false)]bool withValidation) {
            string remoteId = "remoteId";
            var storage = new MetaDataStorage(this.engine, this.matcher, withValidation);
            var obj = new MappedObject("name", remoteId, MappedObjectType.Folder, null, null);
            storage.SaveMappedObject(obj);

            storage.RemoveObject(obj);

            Assert.That(storage.GetObjectByRemoteId(remoteId), Is.Null);
        }

        [Test, Category("Fast")]
        public void RemoveObjectRemovesChildrenAsWell([Values(true, false)]bool withValidation) {
            string remoteId = "remoteId";
            string childId = "childId";
            string subChildId = "subchildId";
            var storage = new MetaDataStorage(this.engine, this.matcher, withValidation);
            var obj = new MappedObject("name", remoteId, MappedObjectType.Folder, null, null);
            var child = new MappedObject("child", childId, MappedObjectType.Folder, remoteId, null);
            var subchild = new MappedObject("subchild", subChildId, MappedObjectType.File, childId, null);
            storage.SaveMappedObject(obj);
            storage.SaveMappedObject(child);
            storage.SaveMappedObject(subchild);

            storage.RemoveObject(obj);

            Assert.That(storage.GetObjectByRemoteId(remoteId), Is.Null);
            Assert.That(storage.GetObjectByRemoteId(childId), Is.Null);
            Assert.That(storage.GetObjectByRemoteId(subChildId), Is.Null);
        }

        [Test, Category("Fast")]
        public void RemoveObjectDoesNotTouchParents([Values(true, false)]bool withValidation) {
            string remoteId = "remoteId";
            string childId = "childId";
            string subChildId = "subchildId";
            var storage = new MetaDataStorage(this.engine, this.matcher, withValidation);
            var obj = new MappedObject("name", remoteId, MappedObjectType.Folder, null, null);
            var child = new MappedObject("child", childId, MappedObjectType.Folder, remoteId, null);
            var subchild = new MappedObject("subchild", subChildId, MappedObjectType.File, childId, null);
            storage.SaveMappedObject(obj);
            storage.SaveMappedObject(child);
            storage.SaveMappedObject(subchild);

            storage.RemoveObject(child);

            Assert.That(storage.GetObjectByRemoteId(remoteId), Is.EqualTo(obj));
            Assert.That(storage.GetObjectByRemoteId(childId), Is.Null);
            Assert.That(storage.GetObjectByRemoteId(subChildId), Is.Null);
        }

        [Test, Category("Fast")]
        public void GetLocalPathThrowsExceptionOnNullArgument([Values(true, false)]bool withValidation) {
            var storage = new MetaDataStorage(this.engine, this.matcher, withValidation);
            Assert.Throws<ArgumentNullException>(() => storage.GetLocalPath(null));
        }

        [Test, Category("Fast")]
        public void GetLocalPathThrowsExceptionOnNonExistingIdInObject([Values(true, false)]bool withValidation) {
            var storage = new MetaDataStorage(this.engine, this.matcher, withValidation);
            Assert.Throws<ArgumentException>(() => storage.GetLocalPath(Mock.Of<IMappedObject>()));
        }

        [Test, Category("Fast")]
        public void GetLocalPath([Values(true, false)]bool withValidation) {
            var matcher = new Mock<IPathMatcher>();
            matcher.Setup(m => m.LocalTargetRootPath).Returns(Path.GetTempPath());
            var storage = new MetaDataStorage(this.engine, matcher.Object, withValidation);
            string id = "remoteId";
            var rootFolder = new MappedObject("name", id, MappedObjectType.Folder, null, null);
            storage.SaveMappedObject(rootFolder);

            string path = storage.GetLocalPath(rootFolder);

            Assert.That(path, Is.EqualTo(Path.Combine(Path.GetTempPath(), "name")));
        }

        [Test, Category("Fast")]
        public void GetLocalPathOfNonExistingEntryReturnsNull([Values(true, false)]bool withValidation) {
            var matcher = new Mock<IPathMatcher>();
            matcher.Setup(m => m.LocalTargetRootPath).Returns(Path.GetTempPath());
            var storage = new MetaDataStorage(this.engine, matcher.Object, withValidation);
            string id = "nonExistingId";
            var rootFolder = new MappedObject("name", "otherId", MappedObjectType.Folder, null, null);
            var otherFolder = new MappedObject("name", id, MappedObjectType.Folder, "otherId", null);
            storage.SaveMappedObject(rootFolder);

            Assert.That(storage.GetLocalPath(otherFolder), Is.Null);
        }

        [Test, Category("Fast")]
        public void GetRemotePathThrowsExceptionOnNullArgument([Values(true, false)]bool withValidation) {
            var storage = new MetaDataStorage(this.engine, this.matcher, withValidation);
            Assert.Throws<ArgumentNullException>(() => storage.GetRemotePath(null));
        }

        [Test, Category("Fast")]
        public void GetRemotePathThrowsExceptionOnNonExistingIdInObject([Values(true, false)]bool withValidation) {
            var storage = new MetaDataStorage(this.engine, this.matcher, withValidation);
            Assert.Throws<ArgumentException>(() => storage.GetRemotePath(Mock.Of<IMappedObject>()));
        }

        [Test, Category("Fast")]
        public void GetRemotePath([Values(true, false)]bool withValidation) {
            var matcher = new Mock<IPathMatcher>();
            matcher.Setup(m => m.RemoteTargetRootPath).Returns("/");
            var storage = new MetaDataStorage(this.engine, matcher.Object, withValidation);
            var remoteFolder = new MappedObject("remoteFolder", "remoteId", MappedObjectType.Folder, null, null);
            storage.SaveMappedObject(remoteFolder);

            string remotePath = storage.GetRemotePath(remoteFolder);

            Assert.That(remotePath, Is.EqualTo("/remoteFolder"));
        }

        [Test, Category("Fast")]
        public void GetRemotePathWithCorrectSlashes([Values(true, false)]bool withValidation) {
            var matcher = new Mock<IPathMatcher>();
            matcher.Setup(m => m.RemoteTargetRootPath).Returns("/");
            var storage = new MetaDataStorage(this.engine, matcher.Object, withValidation);
            var remoteRootFolder = new MappedObject("/", "rootId", MappedObjectType.Folder, null, null);
            var remoteFolder = new MappedObject("remoteFolder", "remoteId", MappedObjectType.Folder, "rootId", null);
            storage.SaveMappedObject(remoteRootFolder);
            storage.SaveMappedObject(remoteFolder);

            string remotePath = storage.GetRemotePath(remoteFolder);

            Assert.That(remotePath, Is.EqualTo("/remoteFolder"));
        }

        [Test, Category("Fast")]
        public void FindRootFolder([Values(true, false)]bool withValidation) {
            string id = "id";
            string path = Path.GetTempPath();
            var fsInfo = new DirectoryInfoWrapper(new DirectoryInfo(path));
            var matcher = new PathMatcher(path, "/");
            var storage = new MetaDataStorage(this.engine, matcher, withValidation);
            var rootFolder = new MappedObject("/", id, MappedObjectType.Folder, null, "token");
            storage.SaveMappedObject(rootFolder);

            Assert.That(storage.GetObjectByRemoteId(id), Is.Not.Null, "Not findable by ID");
            Assert.That(storage.GetObjectByLocalPath(fsInfo), Is.Not.Null, "Not findable by path");
        }

        [Test, Category("Fast")]
        public void SaveRenamedMappedObjectOverridesExistingEntry([Values(true, false)]bool withValidation) {
            string id = "id";
            string oldName = "my";
            string newName = "newMy";
            string path = Path.GetTempPath();
            string parentId = "ParentId";
            string oldToken = "oldToken";
            string newToken = "newToken";
            var matcher = new PathMatcher(path, "/");
            var storage = new MetaDataStorage(this.engine, matcher, withValidation);
            var rootFolder = new MappedObject("/", parentId, MappedObjectType.Folder, null, "token");
            storage.SaveMappedObject(rootFolder);
            var folder = new MappedObject(oldName, id, MappedObjectType.Folder, parentId, oldToken);
            storage.SaveMappedObject(folder);

            var savedObject = storage.GetObjectByRemoteId(id);
            savedObject.Name = newName;
            savedObject.LastChangeToken = newToken;
            storage.SaveMappedObject(savedObject);

            Assert.That(storage.GetObjectByLocalPath(Mock.Of<IDirectoryInfo>(d => d.FullName == Path.Combine(path, oldName))), Is.Null);
            Assert.That(storage.GetObjectByLocalPath(Mock.Of<IDirectoryInfo>(d => d.FullName == Path.Combine(path, newName))), Is.EqualTo(savedObject));
        }

        [Test, Category("Fast")]
        public void ToLinePrintReturnsEmptyStringOnEmptyDB([Values(true, false)]bool withValidation) {
            var storage = new MetaDataStorage(this.engine, Mock.Of<IPathMatcher>(), withValidation);
            Assert.That(storage.ToFindString(), Is.EqualTo(string.Empty));
        }

        [Test, Category("Fast")]
        public void ToLinePrintReturnsOneLineIfOnlyRootFolderIsInDB([Values(true, false)]bool withValidation) {
            var storage = new MetaDataStorage(this.engine, Mock.Of<IPathMatcher>(), withValidation);
            var rootFolder = new MappedObject("name", "rootId", MappedObjectType.Folder, null, "token");
            storage.SaveMappedObject(rootFolder);

            Assert.That(storage.ToFindString(), Is.EqualTo("name" + Environment.NewLine));
        }

        [Test, Category("Fast")]
        public void ToLinePrintReturnsOneLinePerEntry([Values(true, false)]bool withValidation) {
            var storage = new MetaDataStorage(this.engine, Mock.Of<IPathMatcher>(), withValidation);
            var rootFolder = new MappedObject("name", "rootId", MappedObjectType.Folder, null, "token");
            var child1Folder = new MappedObject("sub1", "subId1", MappedObjectType.Folder, "rootId", "token");
            var child2Folder = new MappedObject("sub2", "subId2", MappedObjectType.Folder, "rootId", "token");
            var child3Folder = new MappedObject("sub3", "subId3", MappedObjectType.Folder, "rootId", "token");
            var subChildFile = new MappedObject("file", "subId4", MappedObjectType.File, "subId1", "token");
            storage.SaveMappedObject(rootFolder);
            storage.SaveMappedObject(child1Folder);
            storage.SaveMappedObject(child2Folder);
            storage.SaveMappedObject(child3Folder);
            storage.SaveMappedObject(subChildFile);

            string src = storage.ToFindString();

            int count = src.Select((c, i) => src.Substring(i)).Count(sub => sub.StartsWith(Environment.NewLine));
            Assert.That(count, Is.EqualTo(5), string.Format("Newlines Counting {0}:{2} {1}", count, src, Environment.NewLine));
        }

        [Test, Category("Fast")]
        public void ToLinePrintReturnsOneLinePerNotFittingEntry() {
            var storage = new MetaDataStorage(this.engine, Mock.Of<IPathMatcher>(), false);
            var rootFolder = new MappedObject("name", "rootId", MappedObjectType.Folder, null, "token");
            var child1Folder = new MappedObject("sub1", "subId1", MappedObjectType.Folder, "WRONGID", "token");
            storage.SaveMappedObject(rootFolder);
            storage.SaveMappedObject(child1Folder);

            string src = storage.ToFindString();

            int count = src.Select((c, i) => src.Substring(i)).Count(sub => sub.StartsWith(Environment.NewLine));
            Assert.That(count, Is.EqualTo(2), string.Format("Newlines Counting {0}:{2} {1}", count, src, Environment.NewLine));
        }

        [Test, Category("Fast")]
        public void ValidateFolderStructureThrowsExceptionIfRootObjectIsMissingButOtherObjectsAreStored() {
            var storage = new MetaDataStorage(this.engine, Mock.Of<IPathMatcher>(), false);
            var child1Folder = new MappedObject("sub1", "subId1", MappedObjectType.Folder, "rootId", "token");
            storage.SaveMappedObject(child1Folder);

            Assert.Throws<InvalidDataException>(() => storage.ValidateObjectStructure());
        }

        [Test, Category("Fast")]
        public void ValidateFolderStructureThrowsExceptionIfParentObjectIsMissing() {
            var storage = new MetaDataStorage(this.engine, Mock.Of<IPathMatcher>(), false);
            var rootFolder = new MappedObject("name", "rootId", MappedObjectType.Folder, null, "token");
            var child1Folder = new MappedObject("sub1", "subId1", MappedObjectType.Folder, "WRONGID", "token");
            storage.SaveMappedObject(rootFolder);
            storage.SaveMappedObject(child1Folder);

            Assert.Throws<InvalidDataException>(() => storage.ValidateObjectStructure());
        }

        [Test, Category("Fast")]
        public void ValidateFolderStructureThrowsExceptionIfFileParentIdIsFileObject() {
            var storage = new MetaDataStorage(this.engine, Mock.Of<IPathMatcher>(), false);
            var rootFolder = new MappedObject("name", "rootId", MappedObjectType.Folder, null, "token");
            var child1File = new MappedObject("sub1", "subId1", MappedObjectType.File, "rootId", "token");
            var child2File = new MappedObject("sub2", "subId2", MappedObjectType.File, "sub1", "token");
            storage.SaveMappedObject(rootFolder);
            storage.SaveMappedObject(child1File);
            storage.SaveMappedObject(child2File);

            Assert.Throws<InvalidDataException>(() => storage.ValidateObjectStructure());
        }

        [Test, Category("Fast")]
        public void ValidateFolderStructureIsFineIfNoObjectIsStored([Values(true, false)]bool withValidation) {
            var storage = new MetaDataStorage(this.engine, Mock.Of<IPathMatcher>(), withValidation);
            storage.ValidateObjectStructure();
        }

        [Test, Category("Fast")]
        public void ValidateFolderStructureIsFineOnCleanFolderStructure([Values(true, false)]bool withValidation) {
            var storage = new MetaDataStorage(this.engine, Mock.Of<IPathMatcher>(), withValidation);
            var rootFolder = new MappedObject("name", "rootId", MappedObjectType.Folder, null, "token");
            var child1Folder = new MappedObject("sub1", "subId1", MappedObjectType.Folder, "rootId", "token");
            var child2File = new MappedObject("sub2", "subId2", MappedObjectType.File, "subId1", "token");
            storage.SaveMappedObject(rootFolder);
            storage.SaveMappedObject(child1Folder);
            storage.SaveMappedObject(child2File);

            storage.ValidateObjectStructure();
        }

        [Test, Category("Fast")]
        public void GetObjectByGuidReturnsNullIfNoEntryExists([Values(true, false)]bool withValidation) {
            var storage = new MetaDataStorage(this.engine, Mock.Of<IPathMatcher>(), withValidation);
            Assert.That(storage.GetObjectByGuid(Guid.NewGuid()), Is.Null);
        }

        [Test, Category("Fast")]
        public void GetObjectByGuidReturnsSavedObject([Values(true, false)]bool withValidation) {
            var storage = new MetaDataStorage(this.engine, Mock.Of<IPathMatcher>(), withValidation);
            var uuid = Guid.NewGuid();
            var file = new MappedObject("name" , "rootId", MappedObjectType.File, null, "token") { Guid = uuid };
            storage.SaveMappedObject(file);

            Assert.That(storage.GetObjectByGuid(uuid), Is.EqualTo(file));
        }

        [Test, Category("Fast")]
        public void GetObjectListReturnsNullIfNoEntryExists([Values(true, false)]bool withValidation) {
            IMetaDataStorage storage = new MetaDataStorage(this.engine, Mock.Of<IPathMatcher>(), withValidation);
            Assert.That(storage.GetObjectList(), Is.Null);
        }

        [Test, Category("Fast")]
        public void GetObjectListReturnsOneItemInList([Values(true, false)]bool withValidation) {
            var storage = new MetaDataStorage(this.engine, Mock.Of<IPathMatcher>(), withValidation);
            var rootFolder = new MappedObject("name", "rootId", MappedObjectType.Folder, null, "token");
            storage.SaveMappedObject(rootFolder);

            var list = storage.GetObjectList();
            Assert.That(list.First(), Is.EqualTo(rootFolder));
            Assert.That(list.Count, Is.EqualTo(1));
        }

        [Test, Category("Fast")]
        public void GetObjectList([Values(true, false)]bool withValidation) {
            var storage = new MetaDataStorage(this.engine, Mock.Of<IPathMatcher>(), withValidation);
            var rootFolder = new MappedObject("name", "rootId", MappedObjectType.Folder, null, "token");
            var anotherFolder = new MappedObject("other", "anotherId", MappedObjectType.Folder, "rootId", "token");
            storage.SaveMappedObject(rootFolder);
            storage.SaveMappedObject(anotherFolder);

            var list = storage.GetObjectList();
            Assert.That(list.Count, Is.EqualTo(2));
            Assert.That(list.Contains(rootFolder));
            Assert.That(list.Contains(anotherFolder));
        }

        [Test, Category("Fast")]
        public void ThrowOnDublicateGuid([Values(true, false)]bool withValidation) {
            var storage = new MetaDataStorage(this.engine, Mock.Of<IPathMatcher>(), withValidation);
            var rootFolder = new MappedObject("name", "rootId", MappedObjectType.Folder, null, "token");
            var child1 = new MappedObject("sub1", "subId1", MappedObjectType.File, "rootId", "token");
            child1.Guid = Guid.NewGuid();
            var child2 = new MappedObject("sub2", "subId2", MappedObjectType.File, "rootId", "token");
            child2.Guid = child1.Guid;
            storage.SaveMappedObject(rootFolder);
            storage.SaveMappedObject(child1);
            Assert.Throws<DublicateGuidException>(() => storage.SaveMappedObject(child2));
        }

        #region builerplatecode
        public void Dispose() {
            if (this.engine != null) {
                this.engine.Dispose();
                this.engine = null;
            }
        }
        #endregion
    }
}