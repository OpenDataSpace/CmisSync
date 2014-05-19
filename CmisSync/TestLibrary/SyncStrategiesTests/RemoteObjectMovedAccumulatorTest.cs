//-----------------------------------------------------------------------
// <copyright file="RemoteObjectMovedAccumulatorTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.SyncStrategiesTests
{
    using System;
    using System.Collections.Generic;

    using CmisSync.Lib.Data;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Storage;
    using CmisSync.Lib.Sync.Strategy;

    using DotCMIS.Client;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class RemoteObjectMovedAccumulatorTest
    {
        private readonly string localPath = "localPath";
        private readonly string remoteId = "remoteId";

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorThrowsExceptionOnNullQueue()
        {
            new RemoteObjectMovedAccumulator(null, Mock.Of<IMetaDataStorage>());
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorThrowsExceptionOnNullStorage()
        {
            new RemoteObjectMovedAccumulator(Mock.Of<ISyncEventQueue>(), null);
        }

        [Test, Category("Fast")]
        public void ConstructorWorksWithoutGivenFsFactory()
        {
            new RemoteObjectMovedAccumulator(Mock.Of<ISyncEventQueue>(), Mock.Of<IMetaDataStorage>());
        }

        [Test, Category("Fast")]
        public void ConstructorWorksIfFsFactoryIsNull()
        {
            new RemoteObjectMovedAccumulator(Mock.Of<ISyncEventQueue>(), Mock.Of<IMetaDataStorage>(), null);
        }

        [Test, Category("Fast")]
        public void ConstructorTakesFsFactoryInstance()
        {
            new RemoteObjectMovedAccumulator(Mock.Of<ISyncEventQueue>(), Mock.Of<IMetaDataStorage>(), Mock.Of<IFileSystemInfoFactory>());
        }

        [Test, Category("Fast")]
        public void PriorityIsEqualToHigherPriority()
        {
            var acc = new RemoteObjectMovedAccumulator(Mock.Of<ISyncEventQueue>(), Mock.Of<IMetaDataStorage>());
            Assert.That(acc.Priority, Is.EqualTo(EventHandlerPriorities.HIGHER));
        }

        [Test, Category("Fast")]
        public void AccumulatesRemoteMovedFolderEvents()
        {
            var storage = new Mock<IMetaDataStorage>();
            var storedObject = Mock.Of<IMappedObject>(
                o =>
                o.ParentId == "oldParentId");
            storage.Setup(s => s.GetObjectByRemoteId(this.remoteId)).Returns(storedObject);
            storage.Setup(s => s.GetLocalPath(storedObject)).Returns(this.localPath);
            var fsFactory = new Mock<IFileSystemInfoFactory>();
            var localPathInfo = Mock.Of<IDirectoryInfo>();
            fsFactory.Setup(f => f.CreateDirectoryInfo(this.localPath)).Returns(localPathInfo);
            var acc = new RemoteObjectMovedAccumulator(Mock.Of<ISyncEventQueue>(), storage.Object, fsFactory.Object);
            var remoteFolder = Mock.Of<IFolder>(
                f =>
                f.Id == this.remoteId &&
                f.ParentId == "parentId");
            var folderEvent = new FolderEvent(remoteFolder: remoteFolder) {
                Remote = MetaDataChangeType.CREATED
            };

            Assert.That(acc.Handle(folderEvent), Is.False);
            Assert.That(folderEvent.LocalFolder, Is.EqualTo(localPathInfo));
        }

        [Test, Category("Fast")]
        public void AccumulatesRemoteMovedFileEvents()
        {
            var storage = new Mock<IMetaDataStorage>();
            var storedObject = Mock.Of<IMappedObject>(
                o =>
                o.ParentId == "oldParentId");
            storage.Setup(s => s.GetObjectByRemoteId(this.remoteId)).Returns(storedObject);
            storage.Setup(s => s.GetLocalPath(storedObject)).Returns(this.localPath);
            var fsFactory = new Mock<IFileSystemInfoFactory>();
            var localPathInfo = Mock.Of<IFileInfo>();
            fsFactory.Setup(f => f.CreateFileInfo(this.localPath)).Returns(localPathInfo);
            var acc = new RemoteObjectMovedAccumulator(Mock.Of<ISyncEventQueue>(), storage.Object, fsFactory.Object);
            var parents = new List<IFolder>();
            parents.Add(Mock.Of<IFolder>(f => f.Id == "parentId"));
            var remoteFile = Mock.Of<IDocument>(
                f =>
                f.Id == this.remoteId &&
                f.Parents == parents);
            var fileEvent = new FileEvent(remoteFile: remoteFile) {Remote = MetaDataChangeType.CREATED};

            Assert.That(acc.Handle(fileEvent), Is.False);
            Assert.That(fileEvent.LocalFile, Is.Not.Null);
        }

        [Test, Category("Fast")]
        public void IgnoresNonFileOrFolderEvents()
        {
            var acc = new RemoteObjectMovedAccumulator(Mock.Of<ISyncEventQueue>(), Mock.Of<IMetaDataStorage>());
            var e = new Mock<ISyncEvent>(MockBehavior.Strict);
            Assert.That(acc.Handle(e.Object), Is.False);
        }
    }
}

