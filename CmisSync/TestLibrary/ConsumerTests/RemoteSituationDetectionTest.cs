//-----------------------------------------------------------------------
// <copyright file="RemoteSituationDetectionTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.ConsumerTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using CmisSync.Lib.Consumer;
    using CmisSync.Lib.Data;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Storage;
    using CmisSync.Lib.Sync.Strategy;

    using DotCMIS.Client;
    using DotCMIS.Exceptions;

    using Moq;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture]
    public class RemoteSituationDetectionTest
    {
        private readonly IObjectId objectId = Mock.Of<IObjectId>(ob => ob.Id == "objectId");
        private readonly string remotePath = "/object/path";
        private Mock<IMetaDataStorage> storageMock;
        private string remoteChangeToken = "changeToken";

        [SetUp]
        public void SetUp() {
            this.storageMock = new Mock<IMetaDataStorage>();
        }

        [Test, Category("Fast")]
        public void ConstructorWithSession() {
            new RemoteSituationDetection();
        }

        [Test, Category("Fast")]
        public void NoChangeDetectionForFile()
        {
            var lastModificationDate = DateTime.Now;
            var remoteObject = new Mock<IDocument>();
            var fileEvent = new FileEvent(remoteFile: remoteObject.Object);
            fileEvent.Remote = MetaDataChangeType.NONE;

            var detector = new RemoteSituationDetection();

            Assert.AreEqual(SituationType.NOCHANGE, detector.Analyse(this.storageMock.Object, fileEvent));
        }

        [Test, Category("Fast")]
        public void NoChangeDetectionForFileOnAddedEvent()
        {
            var lastModificationDate = DateTime.Now;
            var remoteObject = new Mock<IDocument>();
            var remotePaths = new List<string>();
            remotePaths.Add(this.remotePath);
            remoteObject.Setup(remote => remote.ChangeToken).Returns(this.remoteChangeToken);
            remoteObject.Setup(remote => remote.Id).Returns(this.objectId.Id);
            remoteObject.Setup(remote => remote.LastModificationDate).Returns(lastModificationDate);
            remoteObject.Setup(remote => remote.Paths).Returns(remotePaths);
            var file = Mock.Of<IMappedObject>(f =>
                                              f.LastRemoteWriteTimeUtc == lastModificationDate &&
                                              f.RemoteObjectId == this.objectId.Id &&
                                              f.LastChangeToken == this.remoteChangeToken &&
                                              f.Type == MappedObjectType.File);
            this.storageMock.AddMappedFile(file);
            var fileEvent = new FileEvent(remoteFile: remoteObject.Object) { Remote = MetaDataChangeType.CREATED };

            var detector = new RemoteSituationDetection();

            Assert.AreEqual(SituationType.NOCHANGE, detector.Analyse(this.storageMock.Object, fileEvent));
        }

        [Test, Category("Fast")]
        public void NoChangeDetectedForFolder()
        {
            var remoteObject = new Mock<IFolder>();
            var folderEvent = new FolderEvent(remoteFolder: remoteObject.Object);
            folderEvent.Remote = MetaDataChangeType.NONE;

            var detector = new RemoteSituationDetection();

            Assert.AreEqual(SituationType.NOCHANGE, detector.Analyse(this.storageMock.Object, folderEvent));
        }

        [Test, Category("Fast")]
        public void FileAddedDetection()
        {
            var remoteObject = new Mock<IDocument>();

            var fileEvent = new FileEvent(remoteFile: remoteObject.Object);
            fileEvent.Remote = MetaDataChangeType.CREATED;

            var detector = new RemoteSituationDetection();

            Assert.AreEqual(SituationType.ADDED, detector.Analyse(this.storageMock.Object, fileEvent));
        }

        [Test, Category("Fast")]
        public void FolderAddedDetection()
        {
            var remoteObject = new Mock<IFolder>();
            var folderEvent = new FolderEvent(remoteFolder: remoteObject.Object);
            folderEvent.Remote = MetaDataChangeType.CREATED;

            var detector = new RemoteSituationDetection();

            Assert.AreEqual(SituationType.ADDED, detector.Analyse(this.storageMock.Object, folderEvent));
        }

        [Test, Category("Fast")]
        public void FileRemovedDetection()
        {
            var remoteObject = new Mock<IDocument>();

            var fileEvent = new FileEvent(remoteFile: remoteObject.Object);
            fileEvent.Remote = MetaDataChangeType.DELETED;

            var detector = new RemoteSituationDetection();

            Assert.AreEqual(SituationType.REMOVED, detector.Analyse(this.storageMock.Object, fileEvent));
        }

        [Test, Category("Fast")]
        public void FolderRemovedDetection()
        {
            var remoteObject = new Mock<IFolder>();
            var folderEvent = new FolderEvent(remoteFolder: remoteObject.Object);
            folderEvent.Remote = MetaDataChangeType.DELETED;

            var detector = new RemoteSituationDetection();

            Assert.AreEqual(SituationType.REMOVED, detector.Analyse(this.storageMock.Object, folderEvent));
        }

        [Test, Category("Fast")]
        public void FolderMovedDetectionOnFolderMovedEvent()
        {
            var remoteObject = new Mock<IFolder>();
            var folderEvent = new FolderMovedEvent(null, null, null, remoteObject.Object) { Remote = MetaDataChangeType.MOVED };

            var detector = new RemoteSituationDetection();

            Assert.AreEqual(SituationType.MOVED, detector.Analyse(this.storageMock.Object, folderEvent));
        }

        [Test, Category("Fast")]
        public void FolderMovedDetectionOnChangeEvent()
        {
            string folderName = "old";
            string oldLocalPath = Path.Combine(Path.GetTempPath(), folderName);
            string remoteId = "remoteId";
            string oldParentId = "oldParentId";
            string newParentId = "newParentId";
            var remoteFolder = new Mock<IFolder>();
            remoteFolder.Setup(f => f.Name).Returns(folderName);
            remoteFolder.Setup(f => f.Path).Returns("/new/" + folderName);
            remoteFolder.Setup(f => f.Id).Returns(remoteId);
            remoteFolder.Setup(f => f.ParentId).Returns(newParentId);
            var mappedParentFolder = Mock.Of<IMappedObject>(p =>
                                                            p.RemoteObjectId == oldParentId &&
                                                            p.Type == MappedObjectType.Folder);
            var mappedFolder = this.storageMock.AddLocalFolder(oldLocalPath, remoteId);
            mappedFolder.Setup(f => f.Name).Returns(folderName);
            mappedFolder.Setup(f => f.ParentId).Returns(mappedParentFolder.RemoteObjectId);
            var folderEvent = new FolderEvent(remoteFolder: remoteFolder.Object) { Remote = MetaDataChangeType.CHANGED };

            var detector = new RemoteSituationDetection();

            Assert.AreEqual(SituationType.MOVED, detector.Analyse(this.storageMock.Object, folderEvent));
        }

        [Test, Category("Fast")]
        public void FileMovedDetectionOnChangeEvent()
        {
            string fileName = "old";
            string oldLocalPath = Path.Combine(Path.GetTempPath(), fileName);
            string remoteId = "remoteId";
            string oldParentId = "oldParentId";
            string newParentId = "newParentId";
            var remoteFile = new Mock<IDocument>();
            remoteFile.Setup(f => f.Name).Returns(fileName);
            remoteFile.SetupPath("/new/" + fileName);
            remoteFile.Setup(f => f.Id).Returns(remoteId);
            remoteFile.SetupParent(Mock.Of<IFolder>(p => p.Id == newParentId));
            var mappedParentFolder = Mock.Of<IMappedObject>(p =>
                p.RemoteObjectId == oldParentId &&
                p.Type == MappedObjectType.Folder);
            var mappedFile = this.storageMock.AddLocalFile(oldLocalPath, remoteId);
            mappedFile.Setup(f => f.Name).Returns(fileName);
            mappedFile.Setup(f => f.ParentId).Returns(mappedParentFolder.RemoteObjectId);
            var fileEvent = new FileEvent(remoteFile: remoteFile.Object) { Remote = MetaDataChangeType.CHANGED };

            var detector = new RemoteSituationDetection();

            Assert.AreEqual(SituationType.MOVED, detector.Analyse(this.storageMock.Object, fileEvent));
        }

        [Test, Category("Fast")]
        public void FolderRenameDetectionOnChangeEvent()
        {
            string remoteId = "remoteId";
            string oldName = "old";
            string newName = "new";
            var remoteFolder = new Mock<IFolder>();
            remoteFolder.Setup(f => f.Name).Returns(newName);
            remoteFolder.Setup(f => f.Id).Returns(remoteId);
            var mappedFolder = Mock.Of<IMappedObject>(f =>
                                                      f.RemoteObjectId == remoteId &&
                                                      f.Name == oldName &&
                                                      f.Type == MappedObjectType.Folder);
            this.storageMock.AddMappedFolder(mappedFolder);
            var folderEvent = new FolderEvent(remoteFolder: remoteFolder.Object) { Remote = MetaDataChangeType.CHANGED };

            var detector = new RemoteSituationDetection();

            Assert.AreEqual(SituationType.RENAMED, detector.Analyse(this.storageMock.Object, folderEvent));
        }

        [Test, Category("Fast")]
        public void FileRenameDetectionOnChangeEvent()
        {
            string remoteId = "remoteId";
            string oldName = "old";
            string newName = "new";
            var remoteFile = new Mock<IDocument>();
            remoteFile.Setup(f => f.Name).Returns(newName);
            remoteFile.Setup(f => f.Id).Returns(remoteId);
            var mappedFile = Mock.Of<IMappedObject>(f =>
                f.RemoteObjectId == remoteId &&
                f.Name == oldName &&
                f.Type == MappedObjectType.File);
            this.storageMock.AddMappedFile(mappedFile);
            var folderEvent = new FileEvent(remoteFile: remoteFile.Object) { Remote = MetaDataChangeType.CHANGED };

            var detector = new RemoteSituationDetection();

            Assert.AreEqual(SituationType.RENAMED, detector.Analyse(this.storageMock.Object, folderEvent));
        }
    }
}