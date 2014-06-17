//-----------------------------------------------------------------------
// <copyright file="LocalObjectChangedTest.cs" company="GRAU DATA AG">
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
    using System.IO;
    using System.Security.Cryptography;

    using CmisSync.Lib.Data;
    using CmisSync.Lib.Storage;
    using CmisSync.Lib.Sync.Solver;

    using DotCMIS.Client;

    using Moq;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture]
    public class LocalObjectChangedTest
    {
        [Test, Category("Fast"), Category("Solver")]
        public void DefaultConstructorTest()
        {
            new LocalObjectChanged();
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalFolderChanged()
        {
            var modificationDate = DateTime.UtcNow;
            var storage = new Mock<IMetaDataStorage>();
            var localDirectory = Mock.Of<IDirectoryInfo>(
                f =>
                f.LastWriteTimeUtc == modificationDate.AddMinutes(1));

            var mappedObject = new MappedObject(
                "name",
                "remoteId",
                MappedObjectType.Folder,
                "parentId",
                "changeToken")
            {
                Guid = Guid.NewGuid(),
                LastRemoteWriteTimeUtc = modificationDate
            };
            storage.AddMappedFolder(mappedObject);

            new LocalObjectChanged().Solve(Mock.Of<ISession>(), storage.Object, localDirectory, Mock.Of<IFolder>());

            storage.VerifySavedMappedObject(
                MappedObjectType.Folder,
                "remoteId",
                mappedObject.Name,
                mappedObject.ParentId,
                mappedObject.LastChangeToken,
                true,
                localDirectory.LastWriteTimeUtc);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalFileModificationDateChanged()
        {
            var modificationDate = DateTime.UtcNow;
            var storage = new Mock<IMetaDataStorage>();
            int fileLength = 20;
            byte[] content = new byte[fileLength];
            byte[] expectedHash = SHA1Managed.Create().ComputeHash(content);

            var localFile = new Mock<IFileInfo>();
            localFile.SetupProperty(f => f.LastWriteTimeUtc, modificationDate.AddMinutes(1));
            localFile.Setup(f => f.Length).Returns(fileLength);

            localFile.Setup(
                f =>
                f.Open(System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read)).Returns(new MemoryStream(content));

            var mappedObject = new MappedObject(
                "name",
                "remoteId",
                MappedObjectType.File,
                "parentId",
                "changeToken",
                fileLength)
            {
                Guid = Guid.NewGuid(),
                LastRemoteWriteTimeUtc = modificationDate,
                LastLocalWriteTimeUtc = modificationDate,
                LastChecksum = expectedHash
            };

            storage.AddMappedFile(mappedObject);

            new LocalObjectChanged().Solve(Mock.Of<ISession>(), storage.Object, localFile.Object, Mock.Of<IDocument>());

            storage.VerifySavedMappedObject(
                MappedObjectType.File,
                "remoteId",
                mappedObject.Name,
                mappedObject.ParentId,
                mappedObject.LastChangeToken,
                true,
                localFile.Object.LastWriteTimeUtc,
                expectedHash);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalFileContentChanged()
        {
            var modificationDate = DateTime.UtcNow;
            var storage = new Mock<IMetaDataStorage>();
            int fileLength = 20;
            byte[] content = new byte[fileLength];
            byte[] expectedHash = SHA1Managed.Create().ComputeHash(content);

            var localFile = new Mock<IFileInfo>();
            localFile.SetupProperty(f => f.LastWriteTimeUtc, modificationDate.AddMinutes(1));
            localFile.Setup(f => f.Length).Returns(fileLength);

            localFile.Setup(
                f =>
                f.Open(System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read)).Returns(new MemoryStream(content));

            var mappedObject = new MappedObject(
                "name",
                "remoteId",
                MappedObjectType.File,
                "parentId",
                "changeToken",
                fileLength)
            {
                Guid = Guid.NewGuid(),
                LastRemoteWriteTimeUtc = modificationDate,
                LastLocalWriteTimeUtc = modificationDate,
                LastChecksum = new byte[20]
            };
            storage.AddMappedFile(mappedObject);
            var remoteFile = MockOfIDocumentUtil.CreateRemoteDocumentMock(null, "remoteId", "name", "parentId", fileLength, new byte[20]);

            new LocalObjectChanged().Solve(Mock.Of<ISession>(), storage.Object, localFile.Object, remoteFile.Object);

            storage.VerifySavedMappedObject(
                MappedObjectType.File,
                "remoteId",
                mappedObject.Name,
                mappedObject.ParentId,
                mappedObject.LastChangeToken,
                true,
                localFile.Object.LastWriteTimeUtc,
                expectedHash);
            remoteFile.VerifySetContentStream(content);
        }
    }
}