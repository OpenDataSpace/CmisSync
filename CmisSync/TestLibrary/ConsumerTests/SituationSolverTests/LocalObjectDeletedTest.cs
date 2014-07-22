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

namespace TestLibrary.ConsumerTests.SituationSolverTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Consumer.SituationSolver;

    using DotCMIS.Client;
    using DotCMIS.Data;
    using DotCMIS.Enums;
    using DotCMIS.Exceptions;

    using Moq;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture]
    public class LocalObjectDeletedTest : IsTestWithConfiguredLog4Net
    {
        private Mock<ISession> session;
        private Mock<IMetaDataStorage> storage;
        private LocalObjectDeleted underTest;

        [SetUp]
        public void SetUp()
        {
            this.session = new Mock<ISession>(MockBehavior.Strict);
            this.storage = new Mock<IMetaDataStorage>(MockBehavior.Strict);
            this.underTest = new LocalObjectDeleted(this.session.Object, this.storage.Object);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void DefaultConstructorTest()
        {
            new LocalObjectDeleted(this.session.Object, this.storage.Object);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalFileDeleted()
        {
            string tempFile = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());

            string remoteDocumentId = "DocumentId";

            this.session.Setup(s => s.Delete(It.Is<IObjectId>((id) => id.Id == remoteDocumentId), true));

            var docId = new Mock<IObjectId>(MockBehavior.Strict);
            docId.Setup(d => d.Id).Returns(remoteDocumentId);
            this.storage.AddLocalFile(tempFile, remoteDocumentId);
            this.storage.Setup(s => s.RemoveObject(It.IsAny<IMappedObject>()));

            this.underTest.Solve(new FileSystemInfoFactory().CreateFileInfo(tempFile), docId.Object);

            this.storage.Verify(s => s.RemoveObject(It.Is<IMappedObject>(o => o.RemoteObjectId == remoteDocumentId)), Times.Once());
            this.session.Verify(s => s.Delete(It.Is<IObjectId>((id) => id.Id == remoteDocumentId), true), Times.Once());
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalFolderDeleted()
        {
            string tempFolder = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());

            string remoteFolderId = "FolderId";

            var folder = new Mock<IFolder>();
            folder.Setup(d => d.Id).Returns(remoteFolderId);
            folder.Setup(f => f.DeleteTree(false, UnfileObject.DeleteSinglefiled, true)).Returns(new List<string>());
            this.session.AddRemoteObject(folder.Object);
            this.storage.AddLocalFolder(tempFolder, remoteFolderId);
            this.storage.Setup(s => s.RemoveObject(It.IsAny<IMappedObject>()));

            this.underTest.Solve(new FileSystemInfoFactory().CreateDirectoryInfo(tempFolder), folder.Object);

            this.storage.Verify(s => s.RemoveObject(It.Is<IMappedObject>(o => o.RemoteObjectId == remoteFolderId)), Times.Once());
            folder.Verify(f => f.DeleteTree(false, UnfileObject.DeleteSinglefiled, true), Times.Once());
        }

        [Test, Category("Fast"), Category("Solver")]
        [ExpectedException(typeof(CmisConnectionException))]
        public void LocalFileDeletedWhileNetworkError()
        {
            string tempFile = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());
            string remoteDocumentId = "DocumentId";
            this.SetupSessionExceptionOnDeletion(remoteDocumentId, new CmisConnectionException());
            this.storage.AddMappedFile(Mock.Of<IMappedObject>(o => o.RemoteObjectId == remoteDocumentId));
            var docId = Mock.Of<IObjectId>(d => d.Id == remoteDocumentId);

            this.underTest.Solve(new FileSystemInfoFactory().CreateFileInfo(tempFile), docId);
        }

        [Test, Category("Fast"), Category("Solver")]
        [ExpectedException(typeof(CmisRuntimeException))]
        public void LocalFileDeletedWhileServerError()
        {
            string tempFile = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());
            string remoteDocumentId = "DocumentId";
            this.storage.AddMappedFile(Mock.Of<IMappedObject>(o => o.RemoteObjectId == remoteDocumentId));
            this.SetupSessionExceptionOnDeletion(remoteDocumentId, new CmisRuntimeException());
            var docId = Mock.Of<IObjectId>(d => d.Id == remoteDocumentId);

            this.underTest.Solve(new FileSystemInfoFactory().CreateFileInfo(tempFile), docId);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalFileDeletedWithoutPermissionToDeleteOnServer()
        {
            string tempFile = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());
            string remoteDocumentId = "DocumentId";
            this.storage.AddMappedFile(Mock.Of<IMappedObject>(o => o.RemoteObjectId == remoteDocumentId));
            this.SetupSessionExceptionOnDeletion(remoteDocumentId, new CmisPermissionDeniedException());
            var docId = Mock.Of<IObjectId>(d => d.Id == remoteDocumentId);

            this.underTest.Solve(new FileSystemInfoFactory().CreateFileInfo(tempFile), docId);

            this.storage.Verify(s => s.RemoveObject(It.IsAny<IMappedObject>()), Times.Never());
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