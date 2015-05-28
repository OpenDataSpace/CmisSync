//-----------------------------------------------------------------------
// <copyright file="IgnoredEntitiesStorageTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.SelectiveIgnoreTests {
    using System;
    using System.Collections.Generic;

    using CmisSync.Lib.SelectiveIgnore;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.Database.Entities;

    using Moq;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture]
    public class IgnoredEntitiesStorageTest {
        [Test, Category("Fast"), Category("SelectiveIgnore")]
        public void ConstructorFailsIfStorageIsNull() {
            Assert.Throws<ArgumentNullException>(() => new IgnoredEntitiesStorage(Mock.Of<IIgnoredEntitiesCollection>(), null));
        }

        [Test, Category("Fast"), Category("SelectiveIgnore")]
        public void ConstructorFailsIfCollectionIsNull() {
            Assert.Throws<ArgumentNullException>(() => new IgnoredEntitiesStorage(null, Mock.Of<IMetaDataStorage>()));
        }

        [Test, Category("Fast"), Category("SelectiveIgnore")]
        public void ConstructorTakesStorageAndCollection() {
            new IgnoredEntitiesStorage(Mock.Of<IIgnoredEntitiesCollection>(), Mock.Of<IMetaDataStorage>());
        }

        [Test, Category("Fast"), Category("SelectiveIgnore")]
        public void StorageWrapsCollectionsAdd() {
            var collection = new Mock<IIgnoredEntitiesCollection>();
            var storage = new Mock<IMetaDataStorage>(MockBehavior.Strict);
            var entry = Mock.Of<IIgnoredEntity>();
            var underTest = new IgnoredEntitiesStorage(collection.Object, storage.Object);

            underTest.Add(entry);

            collection.Verify(c => c.Add(entry), Times.Once);
        }

        [Test, Category("Fast"), Category("SelectiveIgnore")]
        public void StorageWrapsCollectionsRemove() {
            var collection = new Mock<IIgnoredEntitiesCollection>();
            var storage = new Mock<IMetaDataStorage>(MockBehavior.Strict);
            var entry = "entry";
            var underTest = new IgnoredEntitiesStorage(collection.Object, storage.Object);

            underTest.Remove(entry);

            collection.Verify(c => c.Remove(entry), Times.Once);
        }

        [Test, Category("Fast"), Category("SelectiveIgnore")]
        public void StorageDeletesAllChildrenOfStoredEntry() {
            var objectId = Guid.NewGuid().ToString();
            var subObjectId = Guid.NewGuid().ToString();
            var collection = new Mock<IIgnoredEntitiesCollection>();
            var storage = new Mock<IMetaDataStorage>(MockBehavior.Strict);
            var folder = Mock.Of<IMappedObject>(f => f.Type == MappedObjectType.Folder && f.RemoteObjectId == objectId);
            var subFolder = Mock.Of<IMappedObject>(f => f.RemoteObjectId == subObjectId && f.ParentId == objectId);
            var children = new List<IMappedObject>();
            children.Add(subFolder);
            storage.AddMappedFolder(folder);
            storage.Setup(s => s.GetChildren(folder)).Returns(children);
            storage.Setup(s => s.RemoveObject(subFolder));
            storage.AddMappedFolder(subFolder);
            var entry = Mock.Of<IIgnoredEntity>(e => e.ObjectId == objectId);
            var underTest = new IgnoredEntitiesStorage(collection.Object, storage.Object);

            underTest.AddOrUpdateEntryAndDeleteAllChildrenFromStorage(entry);

            collection.Verify(c => c.Add(entry), Times.Once);
            storage.Verify(s => s.RemoveObject(subFolder), Times.Once);
        }
    }
}