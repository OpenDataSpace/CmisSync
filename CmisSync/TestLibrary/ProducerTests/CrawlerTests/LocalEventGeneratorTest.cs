//-----------------------------------------------------------------------
// <copyright file="LocalEventGeneratorTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.ProducerTests.CrawlerTests {
    using System;
    using System.Collections.Generic;
    using System.IO;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Producer.Crawler;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;

    using Moq;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture]
    public class LocalEventGeneratorTest {
        private static DateTime zeroDate = new DateTime(0);

        [Test, Category("Fast")]
        public void RenameOnSubFolder() {
            var storage = new Mock<IMetaDataStorage>();
            var fsFactory = new Mock<IFileSystemInfoFactory>();
            var underTest = new LocalEventGenerator(storage.Object, fsFactory.Object);
            Dictionary<string, Tuple<AbstractFolderEvent, AbstractFolderEvent>> eventMap = new Dictionary<string, Tuple<AbstractFolderEvent, AbstractFolderEvent>>();

            Guid rootGuid = Guid.NewGuid();
            var rootName = "root";
            var rootPath = Path.Combine(Path.GetTempPath(), rootName);
            var rootObjectId = "rootId";
            ObjectTree<IFileSystemInfo> rootTree = this.CreateTreeFromPathAndGuid(rootName, rootPath, rootGuid);

            Guid subFolderGuid = Guid.NewGuid();
            var subName = "A";
            var subPath = Path.Combine(rootPath, subName);
            var subFolderId = "subId";
            ObjectTree<IFileSystemInfo> subFolder = this.CreateTreeFromPathAndGuid(subName, subPath, subFolderGuid);
            rootTree.Children.Add(subFolder);

            Guid subSubFolderGuid = Guid.NewGuid();
            var subSubName = "B";
            var subSubPath = Path.Combine(subPath, subSubName);
            var subSubFolderId = "subId";
            ObjectTree<IFileSystemInfo> subSubFolder = this.CreateTreeFromPathAndGuid(subSubName, subSubPath, subSubFolderGuid);
            subFolder.Children.Add(subSubFolder);

            IDictionary<Guid, IMappedObject> storedObjectsForLocal = new Dictionary<Guid, IMappedObject>();
            var rootMappedObject = this.CreateStoredObjectMock(rootGuid, rootObjectId, rootName, null);
            storedObjectsForLocal.Add(rootGuid, rootMappedObject);
            var subMappedObject = this.CreateStoredObjectMock(subFolderGuid, subFolderId, subName, rootObjectId);
            storedObjectsForLocal.Add(subFolderGuid, subMappedObject);
            var subSubMappedObject = this.CreateStoredObjectMock(subSubFolderGuid, subSubFolderId, "oldsubsubName", subSubFolderId);
            storedObjectsForLocal.Add(subSubFolderGuid, subSubMappedObject);

            storage.Setup(s => s.GetLocalPath(rootMappedObject)).Returns(rootPath);
            storage.Setup(s => s.GetLocalPath(subMappedObject)).Returns(subPath);
            storage.Setup(s => s.GetLocalPath(subSubMappedObject)).Returns(subSubPath);

            ISet<IMappedObject> handledLocalStoredObjects = new HashSet<IMappedObject>();
            IList<AbstractFolderEvent> creationEvents = new List<AbstractFolderEvent>();
            underTest.CreateEvents(storedObjectsForLocal, rootTree, eventMap, handledLocalStoredObjects, ref creationEvents);
            foreach (var handledObjects in handledLocalStoredObjects) {
                storedObjectsForLocal.Remove(handledObjects.Guid);
            }

            storedObjectsForLocal.Remove(rootMappedObject.Guid);
            Assert.That(creationEvents, Is.Empty);
            Assert.That(storedObjectsForLocal, Is.Empty);
            Assert.That(eventMap.Count, Is.EqualTo(1));
            Assert.That(eventMap[subSubFolderId], Is.Not.Null);
            Assert.That(eventMap[subSubFolderId].Item1.Local, Is.EqualTo(MetaDataChangeType.CHANGED));
        }

        private ObjectTree<IFileSystemInfo> CreateTreeFromPathAndGuid(string name, string path, Guid guid) {
            var localTree = new ObjectTree<IFileSystemInfo>();
            var fsInfo = new Mock<IDirectoryInfo>();
            fsInfo.SetupGuid(guid);
            fsInfo.Setup(f => f.FullName).Returns(path);
            fsInfo.Setup(f => f.Name).Returns(name);
            fsInfo.Setup(f => f.LastWriteTimeUtc).Returns(zeroDate);
            localTree.Item = fsInfo.Object;
            localTree.Children = new List<IObjectTree<IFileSystemInfo>>();
            return localTree;
        }

        private IMappedObject CreateStoredObjectMock(Guid guid, string remoteId, string name, string parentId) {
            var mock = new Mock<IMappedObject>();
            mock.Setup(m => m.ParentId).Returns(parentId);
            mock.Setup(m => m.Guid).Returns(guid);
            mock.Setup(m => m.RemoteObjectId).Returns(remoteId);
            mock.Setup(m => m.Name).Returns(name);
            mock.Setup(m => m.LastLocalWriteTimeUtc).Returns(zeroDate);
            return mock.Object;
        }
    }
}
