//-----------------------------------------------------------------------
// <copyright file="LocalObjectDeletedRemoteObjectRenamedOrMovedTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.ConsumerTests.SituationSolverTests
{
    using System;
    using System.IO;

    using CmisSync.Lib.Consumer.SituationSolver;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;

    using Moq;

    using NUnit.Framework;

    using TestUtils;

    [TestFixture]
    public class LocalObjectDeletedRemoteObjectRenamedOrMovedTest
    {
        private static readonly string RemoteId = "remoteId";
        private LocalObjectDeletedRemoteObjectRenamedOrMoved underTest;
        private Mock<ISession> session;
        private Mock<IFileSystemInfo> deletedFsObject;
        private Mock<IMetaDataStorage> storage;
        private IMappedObject mappedObject;
        private Mock<IObjectId> remoteObject;

        [SetUp]
        public void SetUp() {
            this.session = new Mock<ISession>();
            this.storage = new Mock<IMetaDataStorage>();
            this.remoteObject = new Mock<IObjectId>(MockBehavior.Strict);
            this.remoteObject.Setup(o => o.Id).Returns(RemoteId);
            this.deletedFsObject = new Mock<IFileSystemInfo>();
            this.mappedObject = Mock.Of<IMappedObject>(o => o.RemoteObjectId == RemoteId);
            this.underTest = new LocalObjectDeletedRemoteObjectRenamedOrMoved(
                this.session.Object,
                this.storage.Object);
        }

        [Test, Category("Fast")]
        public void DeletesStoredMappedObjectOfDeletedFileAndThrowsException() {
            this.storage.AddMappedFile(this.mappedObject);
            Assert.Throws<IOException>(() => this.underTest.Solve(this.deletedFsObject.Object, this.remoteObject.Object));
            this.storage.Verify(s => s.RemoveObject(this.mappedObject), Times.Once());
        }

        [Test, Category("Fast")]
        public void DeletesStoredMappedObjectOfDeletedFolderAndThrowsException() {
            this.storage.AddMappedFolder(this.mappedObject);
            Assert.Throws<IOException>(() => this.underTest.Solve(this.deletedFsObject.Object, this.remoteObject.Object));
            this.storage.Verify(s => s.RemoveObject(this.mappedObject), Times.Once());
        }
    }
}