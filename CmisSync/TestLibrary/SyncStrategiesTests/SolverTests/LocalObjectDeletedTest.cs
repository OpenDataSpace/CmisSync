//-----------------------------------------------------------------------
// <copyright file="LocalObjectDeletedTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.SyncStrategiesTests.SolverTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using CmisSync.Lib.Data;
    using CmisSync.Lib.Storage;
    using CmisSync.Lib.Sync.Solver;

    using DotCMIS.Client;
    using DotCMIS.Data;
    using DotCMIS.Enums;
    using DotCMIS.Exceptions;

    using Moq;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture]
    public class LocalObjectDeletedTest
    {
        private Mock<ISession> session;
        private Mock<IMetaDataStorage> storage;

        [SetUp]
        public void SetUp()
        {
            this.session = new Mock<ISession>(MockBehavior.Strict);
            this.storage = new Mock<IMetaDataStorage>(MockBehavior.Strict);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void DefaultConstructorTest()
        {
            new LocalObjectDeleted();
        }

        [Test, Category("Medium"), Category("Solver")]
        public void LocalFileDeleted()
        {
            string tempFile = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());

            bool remoteObjectDeleted = false;

            string remoteDocumentId = "DocumentId";

            this.session.When(
                () => remoteObjectDeleted).Setup(
                s => s.Delete(It.Is<IObjectId>((id) => id.Id == remoteDocumentId))).Throws(new InvalidOperationException());
            this.session.When(
                () => remoteObjectDeleted).Setup(
                s => s.Delete(It.Is<IObjectId>((id) => id.Id == remoteDocumentId), It.IsAny<bool>())).Throws(new InvalidOperationException());
            this.session.When(() => !remoteObjectDeleted).Setup(
                s => s.Delete(It.Is<IObjectId>((id) => id.Id == remoteDocumentId))).Callback(() => remoteObjectDeleted = true);
            this.session.When(
                () => !remoteObjectDeleted).Setup(
                s => s.Delete(It.Is<IObjectId>((id) => id.Id == remoteDocumentId), It.IsAny<bool>())).Callback(() => remoteObjectDeleted = true);

            var docId = new Mock<IObjectId>(MockBehavior.Strict);
            docId.Setup(d => d.Id).Returns(remoteDocumentId);
            this.storage.AddLocalFile(tempFile, remoteDocumentId);
            this.storage.Setup(s => s.RemoveObject(It.IsAny<IMappedObject>()));

            new LocalObjectDeleted().Solve(this.session.Object, this.storage.Object, new FileSystemInfoFactory().CreateFileInfo(tempFile), docId.Object);

            this.storage.Verify(s => s.RemoveObject(It.Is<IMappedObject>(o => o.RemoteObjectId == remoteDocumentId)), Times.Once());
            Assert.IsTrue(remoteObjectDeleted);
        }

        [Test, Category("Medium"), Category("Solver")]
        public void LocalFolderDeleted()
        {
            string tempFolder = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());

            bool remoteObjectDeleted = false;

            string remoteFolderId = "FolderId";

            this.session.When(
                () => remoteObjectDeleted).Setup(
                s => s.Delete(It.Is<IObjectId>((id) => id.Id == remoteFolderId))).Throws(new InvalidOperationException());
            this.session.When(
                () => remoteObjectDeleted).Setup(
                s => s.Delete(It.Is<IObjectId>((id) => id.Id == remoteFolderId), It.IsAny<bool>())).Throws(new InvalidOperationException());
            this.session.When(
                () => !remoteObjectDeleted).Setup(s => s.Delete(It.Is<IObjectId>((id) => id.Id == remoteFolderId))).Callback(() => remoteObjectDeleted = true);
            this.session.When(
                () => !remoteObjectDeleted).Setup(
                s => s.Delete(It.Is<IObjectId>((id) => id.Id == remoteFolderId), It.IsAny<bool>())).Callback(() => remoteObjectDeleted = true);

            var docId = new Mock<IObjectId>(MockBehavior.Strict);
            docId.Setup(d => d.Id).Returns(remoteFolderId);
            this.storage.AddLocalFolder(tempFolder, remoteFolderId);
            this.storage.Setup(s => s.RemoveObject(It.IsAny<IMappedObject>()));

            new LocalObjectDeleted().Solve(this.session.Object, this.storage.Object, new FileSystemInfoFactory().CreateDirectoryInfo(tempFolder), docId.Object);

            this.storage.Verify(s => s.RemoveObject(It.Is<IMappedObject>(o => o.RemoteObjectId == remoteFolderId)), Times.Once());
            Assert.IsTrue(remoteObjectDeleted);
        }

        [Test, Category("Medium"), Category("Solver")]
        [ExpectedException(typeof(CmisConnectionException))]
        public void LocalFileDeletedWhileNetworkError()
        {
            string tempFile = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());
            string remoteDocumentId = "DocumentId";
            this.SetupSessionExceptionOnDeletion(remoteDocumentId, new CmisConnectionException());
            this.storage.AddMappedFile(Mock.Of<IMappedObject>(o => o.RemoteObjectId == remoteDocumentId));
            var docId = Mock.Of<IObjectId>(d => d.Id == remoteDocumentId);

            new LocalObjectDeleted().Solve(this.session.Object, this.storage.Object, new FileSystemInfoFactory().CreateFileInfo(tempFile), docId);
        }

        [Test, Category("Medium"), Category("Solver")]
        [ExpectedException(typeof(CmisRuntimeException))]
        public void LocalFileDeletedWhileServerError()
        {
            string tempFile = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());
            string remoteDocumentId = "DocumentId";
            this.storage.AddMappedFile(Mock.Of<IMappedObject>(o => o.RemoteObjectId == remoteDocumentId));
            this.SetupSessionExceptionOnDeletion(remoteDocumentId, new CmisRuntimeException());
            var docId = Mock.Of<IObjectId>(d => d.Id == remoteDocumentId);

            new LocalObjectDeleted().Solve(this.session.Object, this.storage.Object, new FileSystemInfoFactory().CreateFileInfo(tempFile), docId);
        }

        private void SetupSessionExceptionOnDeletion(string remoteId, Exception ex) {
            this.session.Setup(
                s => s.Delete(
                It.Is<IObjectId>((id) => id.Id == remoteId))).Throws(ex);
            this.session.Setup(
                s => s.Delete(
                It.Is<IObjectId>((id) => id.Id == remoteId),
                It.IsAny<bool>())).Throws(ex);
        }
    }
}