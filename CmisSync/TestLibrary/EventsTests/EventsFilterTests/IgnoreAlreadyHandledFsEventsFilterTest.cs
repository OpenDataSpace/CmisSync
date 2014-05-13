//-----------------------------------------------------------------------
// <copyright file="IgnoreAlreadyHandledFsEventsFilterTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.EventsTests.EventsFilterTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using CmisSync.Lib.Data;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Events.Filter;
    using CmisSync.Lib.Storage;

    using Moq;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture]
    public class IgnoreAlreadyHandledFsEventsFilterTest
    {
        [Test, Category("Fast")]
        public void ConstructorTakesStorage()
        {
            new IgnoreAlreadyHandledFsEventsFilter(Mock.Of<IMetaDataStorage>());
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorThrowsExceptionIfStorageIsNull()
        {
            new IgnoreAlreadyHandledFsEventsFilter(null);
        }

        [Test, Category("Fast")]
        public void ConstructorTakesStorageAndFsFactory()
        {
            new IgnoreAlreadyHandledFsEventsFilter(Mock.Of<IMetaDataStorage>(), Mock.Of<IFileSystemInfoFactory>());
        }

        [Test, Category("Fast")]
        public void FilterIgnoresNonFsEvents()
        {
            var filter = new IgnoreAlreadyHandledFsEventsFilter(Mock.Of<IMetaDataStorage>(), Mock.Of<IFileSystemInfoFactory>());
            Assert.That(filter.Handle(Mock.Of<ISyncEvent>()), Is.False);
        }

        [Test, Category("Fast")]
        public void FilterIgnoresNonExistingPaths()
        {
            var storage = new Mock<IMetaDataStorage>();
            var fsFactory = new Mock<IFileSystemInfoFactory>();
            var filter = new IgnoreAlreadyHandledFsEventsFilter(storage.Object, fsFactory.Object);
            var fsEvent = new FSEvent(WatcherChangeTypes.Created, "path");
            Assert.That(filter.Handle(fsEvent), Is.False);
        }

        [Test, Category("Fast")]
        public void FilterHandlesAlreadyExistingFolderEntries()
        {
            string path = "path";
            var storage = new Mock<IMetaDataStorage>();
            var fsFactory = new Mock<IFileSystemInfoFactory>();
            var filter = new IgnoreAlreadyHandledFsEventsFilter(storage.Object, fsFactory.Object);
            var fsEvent = new Mock<FSEvent>(WatcherChangeTypes.Created, path) { CallBase = true };
            fsEvent.Setup(e => e.IsDirectory()).Returns(true);
            fsFactory.AddDirectory(path);
            storage.Setup(s => s.GetObjectByLocalPath(It.Is<IFileSystemInfo>(p => p.FullName.Equals(path)))).Returns(Mock.Of<IMappedObject>());

            Assert.That(filter.Handle(fsEvent.Object), Is.True);
        }

        [Test, Category("Fast")]
        public void FilterHandlesAlreadyExistingFileEntries()
        {
            string path = "path";
            var storage = new Mock<IMetaDataStorage>();
            var fsFactory = new Mock<IFileSystemInfoFactory>();
            var filter = new IgnoreAlreadyHandledFsEventsFilter(storage.Object, fsFactory.Object);
            var fsEvent = new Mock<FSEvent>(WatcherChangeTypes.Created, path) { CallBase = true };
            fsEvent.Setup(e => e.IsDirectory()).Returns(false);
            fsFactory.AddFile(path);
            storage.Setup(s => s.GetObjectByLocalPath(It.Is<IFileSystemInfo>(p => p.FullName.Equals(path)))).Returns(Mock.Of<IMappedObject>());

            Assert.That(filter.Handle(fsEvent.Object), Is.True);
        }

        [Test, Category("Fast")]
        public void FilterDeleteFsEventsIfNoCorrespondingElementExistsInStorage()
        {
            string path = "path";
            var fsEvent = new Mock<FSEvent>(WatcherChangeTypes.Deleted, path) { CallBase = true };
            var filter = new IgnoreAlreadyHandledFsEventsFilter(Mock.Of<IMetaDataStorage>(), Mock.Of<IFileSystemInfoFactory>());
            fsEvent.Setup(e => e.IsDirectory()).Returns(false);

            Assert.That(filter.Handle(fsEvent.Object), Is.True);
        }
    }
}