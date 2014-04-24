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
using System;
using System.Collections.Generic;
using System.IO;

using CmisSync.Lib.Sync.Solver;
using CmisSync.Lib.Storage;

using DotCMIS.Client;
using DotCMIS.Data;
using DotCMIS.Enums;
using DotCMIS.Exceptions;

using Moq;

using NUnit.Framework;
using TestLibrary.TestUtils;
using CmisSync.Lib.Data;

namespace TestLibrary.SyncStrategiesTests.SolverTests
{
    [TestFixture]
    public class LocalObjectDeletedTest
    {
        private Mock<ISession> Session;
        private Mock<IMetaDataStorage> Storage;

        [SetUp]
        public void SetUp()
        {
            Session = new Mock<ISession>(MockBehavior.Strict);
            Storage = new Mock<IMetaDataStorage>(MockBehavior.Strict);
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
            //var remoteDocument = new Mock<ICmisObject>(MockBehavior.Strict);
            //remoteDocument.Setup(d => d.Id).Returns(remoteDocumentId);

            //string remoteParentFolderId = "parentFolder";
            //var remoteParentFolder = new Mock<ICmisObject>(MockBehavior.Strict);
            //remoteParentFolder.Setup(f => f.Id).Returns(remoteParentFolderId);

            Session.When(() => remoteObjectDeleted
                ).Setup(s => s.Delete(
                It.Is<IObjectId>((id) => id.Id == remoteDocumentId))
                ).Throws(new InvalidOperationException());
            Session.When(() => remoteObjectDeleted
                ).Setup(s => s.Delete(
                It.Is<IObjectId>((id) => id.Id == remoteDocumentId),
                It.IsAny<bool>())
                ).Throws(new InvalidOperationException());
            Session.When(() => !remoteObjectDeleted
                ).Setup(s => s.Delete(
                It.Is<IObjectId>((id) => id.Id == remoteDocumentId))
                ).Callback(() => remoteObjectDeleted = true);
            Session.When(() => !remoteObjectDeleted
                ).Setup(s => s.Delete(
                It.Is<IObjectId>((id) => id.Id == remoteDocumentId),
                It.IsAny<bool>())
                ).Callback(() => remoteObjectDeleted = true);

            //Session.Setup(s => s.GetObject(
            //    It.Is<IObjectId>((id) => id.Id == remoteParentFolderId))
            //    ).Returns(remoteParentFolder.Object);
            //Session.When(() => !remoteObjectDeleted
            //    ).Setup(s => s.GetObject(
            //    It.Is<IObjectId>((id) => id.Id == remoteDocumentId))
            //    ).Returns(remoteDocument.Object);
            //Session.When(() => remoteObjectDeleted
            //    ).Setup(s => s.GetObject(
            //    It.Is<IObjectId>((id) => id.Id == remoteDocumentId))
            //    ).Throws(new InvalidOperationException());

            try
            {
                var solver = new LocalObjectDeleted();
                var docId = new Mock<IObjectId>(MockBehavior.Strict);
                docId.Setup(d => d.Id).Returns(remoteDocumentId);
                Storage.AddLocalFile(tempFile, remoteDocumentId);
                Storage.Setup(s => s.RemoveObject(It.IsAny<IMappedObject>()));

                solver.Solve(Session.Object, Storage.Object, new FileSystemInfoFactory().CreateFileInfo(tempFile), docId.Object);

                Storage.Verify( s => s.RemoveObject(It.Is<IMappedObject>(o => o.RemoteObjectId == remoteDocumentId)), Times.Once());
                Assert.IsTrue(remoteObjectDeleted);
            }
            finally
            {
            }
        }

        [Test, Category("Medium"), Category("Solver")]
        public void LocalFolderDeleted()
        {
            string tempFolder = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());

            bool remoteObjectDeleted = false;

            string remoteFolderId = "FolderId";
            //var remoteFolder = new Mock<ICmisObject>(MockBehavior.Strict);
            //remoteFolder.Setup(d => d.Id).Returns(remoteFolderId);

            //string remoteParentFolderId = "parentFolder";
            //var remoteParentFolder = new Mock<ICmisObject>(MockBehavior.Strict);
            //remoteParentFolder.Setup(f => f.Id).Returns(remoteParentFolderId);

            Session.When(() => remoteObjectDeleted
                ).Setup(s => s.Delete(
                It.Is<IObjectId>((id) => id.Id == remoteFolderId))
                ).Throws(new InvalidOperationException());
            Session.When(() => remoteObjectDeleted
                ).Setup(s => s.Delete(
                It.Is<IObjectId>((id) => id.Id == remoteFolderId),
                It.IsAny<bool>())
                ).Throws(new InvalidOperationException());
            Session.When(() => !remoteObjectDeleted
                ).Setup(s => s.Delete(
                It.Is<IObjectId>((id) => id.Id == remoteFolderId))
                ).Callback(() => remoteObjectDeleted = true);
            Session.When(() => !remoteObjectDeleted
                ).Setup(s => s.Delete(
                It.Is<IObjectId>((id) => id.Id == remoteFolderId),
                It.IsAny<bool>())
                ).Callback(() => remoteObjectDeleted = true);

            //Session.Setup(s => s.GetObject(
            //    It.Is<IObjectId>((id) => id.Id == remoteParentFolderId))
            //    ).Returns(remoteParentFolder.Object);
            //Session.When(() => !remoteObjectDeleted
            //    ).Setup(s => s.GetObject(
            //    It.Is<IObjectId>((id) => id.Id == remoteFolderId))
            //    ).Returns(remoteFolder.Object);
            //Session.When(() => remoteObjectDeleted
            //    ).Setup(s => s.GetObject(
            //    It.Is<IObjectId>((id) => id.Id == remoteFolderId))
            //    ).Throws(new InvalidOperationException());

            try
            {
                var solver = new LocalObjectDeleted();
                var docId = new Mock<IObjectId>(MockBehavior.Strict);
                docId.Setup(d => d.Id).Returns(remoteFolderId);
                Storage.AddLocalFolder(tempFolder, remoteFolderId);
                Storage.Setup(s => s.RemoveObject(It.IsAny<IMappedObject>()));

                solver.Solve(Session.Object, Storage.Object, new FileSystemInfoFactory().CreateDirectoryInfo(tempFolder), docId.Object);

                Storage.Verify( s => s.RemoveObject(It.Is<IMappedObject>(o => o.RemoteObjectId == remoteFolderId)), Times.Once());
                Assert.IsTrue(remoteObjectDeleted);
            }
            finally
            {
            }
        }

        [Test, Category("Medium"), Category("Solver")]
        [ExpectedException(typeof(CmisConnectionException))]
        public void LocalFileDeletedWhileNetworkError()
        {
            string tempFile = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());

            //bool remoteObjectDeleted = false;

            string remoteDocumentId = "DocumentId";

            Session.Setup(s => s.Delete(
                It.Is<IObjectId>((id) => id.Id == remoteDocumentId))
                ).Throws(new CmisConnectionException());
            Session.Setup(s => s.Delete(
                It.Is<IObjectId>((id) => id.Id == remoteDocumentId),
                It.IsAny<bool>())
                ).Throws(new CmisConnectionException());

            try
            {
                var solver = new LocalObjectDeleted();
                var docId = new Mock<IObjectId>(MockBehavior.Strict);
                docId.Setup(d => d.Id).Returns(remoteDocumentId);
                solver.Solve(Session.Object, Storage.Object, new FileSystemInfoFactory().CreateDirectoryInfo(tempFile), docId.Object);
            }
            finally
            {
            }
        }

        [Test, Category("Medium"), Category("Solver")]
        [ExpectedException(typeof(CmisRuntimeException))]
        public void LocalFileDeletedWhileServerError()
        {
            string tempFile = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());

            //bool remoteObjectDeleted = false;

            string remoteDocumentId = "DocumentId";

            Session.Setup(s => s.Delete(
                It.Is<IObjectId>((id) => id.Id == remoteDocumentId))
                ).Throws(new CmisRuntimeException());
            Session.Setup(s => s.Delete(
                It.Is<IObjectId>((id) => id.Id == remoteDocumentId),
                It.IsAny<bool>())
                ).Throws(new CmisRuntimeException());

            try
            {
                var solver = new LocalObjectDeleted();
                var docId = new Mock<IObjectId>(MockBehavior.Strict);
                docId.Setup(d => d.Id).Returns(remoteDocumentId);
                solver.Solve(Session.Object, Storage.Object, new FileSystemInfoFactory().CreateDirectoryInfo(tempFile), docId.Object);
            }
            finally
            {
            }
        }

    }
}

