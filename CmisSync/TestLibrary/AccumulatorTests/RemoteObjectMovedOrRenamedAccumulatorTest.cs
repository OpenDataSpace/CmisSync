//-----------------------------------------------------------------------
// <copyright file="RemoteObjectMovedOrRenamedAccumulatorTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.AccumulatorTests
{
    using System;
    using System.Collections.Generic;

    using CmisSync.Lib.Accumulator;
    using CmisSync.Lib.Data;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage;
    using CmisSync.Lib.Storage.Database;

    using DotCMIS.Client;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class RemoteObjectMovedOrRenamedAccumulatorTest
    {
        private readonly string localPath = "localPath";
        private readonly string remoteId = "remoteId";
        private readonly string parentId = "parentId";

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorThrowsExceptionOnNullQueue()
        {
            new RemoteObjectMovedOrRenamedAccumulator(null, Mock.Of<IMetaDataStorage>());
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorThrowsExceptionOnNullStorage()
        {
            new RemoteObjectMovedOrRenamedAccumulator(Mock.Of<ISyncEventQueue>(), null);
        }

        [Test, Category("Fast")]
        public void ConstructorWorksWithoutGivenFsFactory()
        {
            new RemoteObjectMovedOrRenamedAccumulator(Mock.Of<ISyncEventQueue>(), Mock.Of<IMetaDataStorage>());
        }

        [Test, Category("Fast")]
        public void ConstructorWorksIfFsFactoryIsNull()
        {
            new RemoteObjectMovedOrRenamedAccumulator(Mock.Of<ISyncEventQueue>(), Mock.Of<IMetaDataStorage>(), null);
        }

        [Test, Category("Fast")]
        public void ConstructorTakesFsFactoryInstance()
        {
            new RemoteObjectMovedOrRenamedAccumulator(Mock.Of<ISyncEventQueue>(), Mock.Of<IMetaDataStorage>(), Mock.Of<IFileSystemInfoFactory>());
        }

        [Test, Category("Fast")]
        public void PriorityIsEqualToHigherPriority()
        {
            var acc = new RemoteObjectMovedOrRenamedAccumulator(Mock.Of<ISyncEventQueue>(), Mock.Of<IMetaDataStorage>());
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
            var acc = new RemoteObjectMovedOrRenamedAccumulator(Mock.Of<ISyncEventQueue>(), storage.Object, fsFactory.Object);
            var remoteFolder = Mock.Of<IFolder>(
                f =>
                f.Id == this.remoteId &&
                f.ParentId == this.parentId);
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
            var acc = new RemoteObjectMovedOrRenamedAccumulator(Mock.Of<ISyncEventQueue>(), storage.Object, fsFactory.Object);
            var parents = new List<IFolder>();
            parents.Add(Mock.Of<IFolder>(f => f.Id == this.parentId));
            var remoteFile = Mock.Of<IDocument>(
                f =>
                f.Id == this.remoteId &&
                f.Parents == parents);
            var fileEvent = new FileEvent(remoteFile: remoteFile) { Remote = MetaDataChangeType.CREATED };

            Assert.That(acc.Handle(fileEvent), Is.False);
            Assert.That(fileEvent.LocalFile, Is.EqualTo(localPathInfo));
        }

        [Test, Category("Fast")]
        public void AccumulatesRemoteRenamedFolderEvents()
        {
            var storage = new Mock<IMetaDataStorage>();
            var storedObject = Mock.Of<IMappedObject>(
                o =>
                o.ParentId == this.parentId &&
                o.Name == "oldName");
            storage.Setup(s => s.GetObjectByRemoteId(this.remoteId)).Returns(storedObject);
            storage.Setup(s => s.GetLocalPath(storedObject)).Returns(this.localPath);
            var fsFactory = new Mock<IFileSystemInfoFactory>();
            var localPathInfo = Mock.Of<IDirectoryInfo>();
            fsFactory.Setup(f => f.CreateDirectoryInfo(this.localPath)).Returns(localPathInfo);
            var acc = new RemoteObjectMovedOrRenamedAccumulator(Mock.Of<ISyncEventQueue>(), storage.Object, fsFactory.Object);
            var remoteFolder = Mock.Of<IFolder>(
                f =>
                f.Id == this.remoteId &&
                f.ParentId == this.parentId &&
                f.Name == "newName");
            var folderEvent = new FolderEvent(remoteFolder: remoteFolder) {
                Remote = MetaDataChangeType.CREATED
            };

            Assert.That(acc.Handle(folderEvent), Is.False);
            Assert.That(folderEvent.LocalFolder, Is.EqualTo(localPathInfo));
        }

        [Test, Category("Fast")]
        public void AccumulatesRemoteRenamedFileEvents()
        {
            var storage = new Mock<IMetaDataStorage>();
            var storedObject = Mock.Of<IMappedObject>(
                o =>
                o.ParentId == this.parentId &&
                o.Name == "oldName");
            storage.Setup(s => s.GetObjectByRemoteId(this.remoteId)).Returns(storedObject);
            storage.Setup(s => s.GetLocalPath(storedObject)).Returns(this.localPath);
            var fsFactory = new Mock<IFileSystemInfoFactory>();
            var localPathInfo = Mock.Of<IFileInfo>();
            fsFactory.Setup(f => f.CreateFileInfo(this.localPath)).Returns(localPathInfo);
            var acc = new RemoteObjectMovedOrRenamedAccumulator(Mock.Of<ISyncEventQueue>(), storage.Object, fsFactory.Object);
            var parents = new List<IFolder>();
            parents.Add(Mock.Of<IFolder>(f => f.Id == this.parentId));
            var remoteFile = Mock.Of<IDocument>(
                f =>
                f.Id == this.remoteId &&
                f.Parents == parents &&
                f.Name == "newName");
            var fileEvent = new FileEvent(remoteFile: remoteFile) { Remote = MetaDataChangeType.CREATED };

            Assert.That(acc.Handle(fileEvent), Is.False);
            Assert.That(fileEvent.LocalFile, Is.EqualTo(localPathInfo));
        }

        [Test, Category("Fast")]
        public void IgnoresNonFileOrFolderEvents()
        {
            var acc = new RemoteObjectMovedOrRenamedAccumulator(Mock.Of<ISyncEventQueue>(), Mock.Of<IMetaDataStorage>());
            var e = new Mock<ISyncEvent>(MockBehavior.Strict);
            Assert.That(acc.Handle(e.Object), Is.False);
        }
    }
}