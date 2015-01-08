using CmisSync.Lib.Storage.Database.Entities;
using System.Collections.Generic;


namespace TestLibrary.SelectiveIgnoreTests
{
    using System;

    using CmisSync.Lib.SelectiveIgnore;
    using CmisSync.Lib.Storage.Database;

    using Moq;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture]
    public class IgnoredEntitiesStorageTest
    {
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