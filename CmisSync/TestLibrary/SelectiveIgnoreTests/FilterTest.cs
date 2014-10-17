//-----------------------------------------------------------------------
// <copyright file="FilterTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.SelectiveIgnoreTests
{
    using System;
    using System.Collections.ObjectModel;
    using System.IO;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.SelectiveIgnore;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;
    using DotCMIS.Enums;
    using DotCMIS.Exceptions;

    using Moq;

    using NUnit.Framework;

    using TestUtils;

    [TestFixture]
    public class FilterTest
    {
        private readonly string ignoredObjectId = "ignoredObjectId";
        private readonly string ignoredPath = Path.Combine(Path.GetTempPath(), "IgnoredLocalPath");
        private Mock<ISession> session;
        private Mock<IMetaDataStorage> storage;
        private ObservableCollection<IIgnoredEntity> ignores;
        private SelectiveIgnoreFilter underTest;

        [Test, Category("Fast"), Category("SelectiveIgnore")]
        public void ConstructorFailsIfCollectionIsNull() {
            Assert.Throws<ArgumentNullException>(
                () => new SelectiveIgnoreFilter(
                null,
                Mock.Of<ISession>(),
                Mock.Of<IMetaDataStorage>()));
        }

        [Test, Category("Fast"), Category("SelectiveIgnore")]
        public void ConstructorFailsIfSessionIsNull() {
            Assert.Throws<ArgumentNullException>(
                () => new SelectiveIgnoreFilter(
                new ObservableCollection<IIgnoredEntity>(),
                null,
                Mock.Of<IMetaDataStorage>()));
        }

        [Test, Category("Fast"), Category("SelectiveIgnore")]
        public void ConstructorFailsIfStorageIsNull() {
            Assert.Throws<ArgumentNullException>(
                () => new SelectiveIgnoreFilter(
                new ObservableCollection<IIgnoredEntity>(),
                Mock.Of<ISession>(),
                null));
        }

        [Test, Category("Fast"), Category("SelectiveIgnore")]
        public void ConstructorTest() {
            new SelectiveIgnoreFilter(
                new ObservableCollection<IIgnoredEntity>(),
                Mock.Of<ISession>(),
                Mock.Of<IMetaDataStorage>());
        }

        [Test, Category("Fast"), Category("SelectiveIgnore")]
        public void FilterLocalFSAddedEvents() {
            this.SetupMocks();
            var fileEvent = new FSEvent(WatcherChangeTypes.Created, Path.Combine(this.ignoredPath, "file.txt"), false);

            Assert.That(this.underTest.Handle(fileEvent), Is.True);
        }

        [Test, Category("Fast"), Category("SelectiveIgnore")]
        public void DoNotFilterValidLocalFSAddedEvents() {
            this.SetupMocks();
            var fileEvent = new FSEvent(WatcherChangeTypes.Created, Path.Combine(Path.GetTempPath(), "file.txt"), false);

            Assert.That(this.underTest.Handle(fileEvent), Is.False);
        }

        [Test, Category("Fast"), Category("SelectiveIgnore")]
        public void DoNotFilterValidLocalFileAddedEvents() {
            this.SetupMocks();
            IFileInfo file = Mock.Of<IFileInfo>(f => f.FullName == Path.Combine(Path.GetTempPath(), "file.txt"));
            var fileEvent = new FileEvent(file) { Local = MetaDataChangeType.CREATED };

            Assert.That(this.underTest.Handle(fileEvent), Is.False);
        }

        [Test, Category("Fast"), Category("SelectiveIgnore")]
        public void FilterLocalFSDeletionEvents() {
            this.SetupMocks();
            var fileEvent = new FSEvent(WatcherChangeTypes.Deleted, Path.Combine(this.ignoredPath, "file.txt"), false);

            Assert.That(this.underTest.Handle(fileEvent), Is.True);
        }

        [Test, Category("Fast"), Category("SelectiveIgnore")]
        public void FilterLocalFSRenamedEvents() {
            this.SetupMocks();
            var fileEvent = new FSMovedEvent(Path.Combine(this.ignoredPath, "old_file.txt"), Path.Combine(this.ignoredPath, "file.txt"), false);

            Assert.That(this.underTest.Handle(fileEvent), Is.True);
        }

        [Test, Category("Fast"), Category("SelectiveIgnore")]
        public void FilterRemoteObjectCreatedEvents() {
            this.SetupMocks();
            string objectId = "objectId";
            var doc = Mock.Of<IDocument>(d => d.Parents[0].Id == this.ignoredObjectId);
            this.session.Setup(s => s.GetObject(objectId)).Returns(doc);
            var fileEvent = new FileEvent(null, doc) { Remote = MetaDataChangeType.CREATED };

            Assert.That(this.underTest.Handle(fileEvent), Is.True);
        }

        [Test, Category("Fast"), Category("SelectiveIgnore"), Ignore("TODO")]
        public void FilterRemoteObjectDeletedEvents() {
            this.SetupMocks();
            string objectId = "objectId";
            this.session.Setup(s => s.GetObject(objectId)).Throws<CmisObjectNotFoundException>();
            var fileEvent = new FileEvent(null, null) { Remote = MetaDataChangeType.DELETED };

            Assert.That(this.underTest.Handle(fileEvent), Is.True);
        }

        private void SetupMocks() {
            this.session = new Mock<ISession>();
            this.storage = new Mock<IMetaDataStorage>();
            this.ignores = new ObservableCollection<IIgnoredEntity>();
            this.ignores.Add(Mock.Of<IIgnoredEntity>(i => i.LocalPath == this.ignoredPath && i.ObjectId == this.ignoredObjectId));
            this.ignores.CollectionChanged += (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) => Assert.Fail("Collection should not be changed");
            this.underTest = new SelectiveIgnoreFilter(this.ignores, this.session.Object, this.storage.Object);
        }
    }
}