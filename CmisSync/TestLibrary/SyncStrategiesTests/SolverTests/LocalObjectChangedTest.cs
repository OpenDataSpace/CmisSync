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
using DotCMIS.Data;

namespace TestLibrary.SyncStrategiesTests.SolverTests
{
    using System;
    using System.IO;
    using System.Security.Cryptography;

    using CmisSync.Lib.Data;
    using CmisSync.Lib.Events;
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
            new LocalObjectChanged(Mock.Of<ISyncEventQueue>());
        }

        [Test, Category("Fast"), Category("Solver")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorThrowsExceptionIfQueueIsNull() {
            new LocalObjectChanged(null);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalFolderChanged()
        {
            var modificationDate = DateTime.UtcNow;
            var storage = new Mock<IMetaDataStorage>();
            var localDirectory = Mock.Of<IDirectoryInfo>(
                f =>
                f.LastWriteTimeUtc == modificationDate.AddMinutes(1));
            var queue = new Mock<ISyncEventQueue>();

            var mappedObject = new MappedObject(
                "name",
                "remoteId",
                MappedObjectType.Folder,
                "parentId",
                "changeToken")
            {
                Guid = Guid.NewGuid(),
                LastRemoteWriteTimeUtc = modificationDate.AddMinutes(1)
            };
            storage.AddMappedFolder(mappedObject);

            new LocalObjectChanged(queue.Object).Solve(Mock.Of<ISession>(), storage.Object, localDirectory, Mock.Of<IFolder>());

            storage.VerifySavedMappedObject(
                MappedObjectType.Folder,
                "remoteId",
                mappedObject.Name,
                mappedObject.ParentId,
                mappedObject.LastChangeToken,
                true,
                localDirectory.LastWriteTimeUtc);
            queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Never());
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalFileModificationDateChanged()
        {
            string path = "path";
            var modificationDate = DateTime.UtcNow;
            var storage = new Mock<IMetaDataStorage>();
            int fileLength = 20;
            byte[] content = new byte[fileLength];
            byte[] expectedHash = SHA1Managed.Create().ComputeHash(content);
            var queue = new Mock<ISyncEventQueue>();
            var localFile = new Mock<IFileInfo>();
            localFile.SetupProperty(f => f.LastWriteTimeUtc, modificationDate.AddMinutes(1));
            localFile.Setup(f => f.Length).Returns(fileLength);
            localFile.Setup(f => f.FullName).Returns(path);

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
                LastRemoteWriteTimeUtc = modificationDate.AddMinutes(1),
                LastLocalWriteTimeUtc = modificationDate,
                LastChecksum = expectedHash,
                ChecksumAlgorithmName = "SHA1"
            };

            storage.AddMappedFile(mappedObject, path);

            new LocalObjectChanged(Mock.Of<ISyncEventQueue>()).Solve(Mock.Of<ISession>(), storage.Object, localFile.Object, Mock.Of<IDocument>());

            storage.VerifySavedMappedObject(
                MappedObjectType.File,
                "remoteId",
                mappedObject.Name,
                mappedObject.ParentId,
                mappedObject.LastChangeToken,
                true,
                localFile.Object.LastWriteTimeUtc,
                expectedHash,
                fileLength);
            queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Never());
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalFileContentChanged()
        {
            var modificationDate = DateTime.UtcNow;
            var newModificationDate = modificationDate.AddHours(1);
            var newChangeToken = "newChangeToken";
            var storage = new Mock<IMetaDataStorage>();
            int fileLength = 20;
            byte[] content = new byte[fileLength];
            byte[] expectedHash = SHA1Managed.Create().ComputeHash(content);
            var queue = new Mock<ISyncEventQueue>();

            var localFile = new Mock<IFileInfo>();
            localFile.SetupProperty(f => f.LastWriteTimeUtc, modificationDate.AddMinutes(1));
            localFile.Setup(f => f.Length).Returns(fileLength);
            localFile.Setup(f => f.FullName).Returns("path");
            localFile.Setup(
                f =>
                f.Open(FileMode.Open, FileAccess.Read, FileShare.Read)).Returns(() => { return new MemoryStream(content);});

            var mappedObject = new MappedObject(
                "name",
                "remoteId",
                MappedObjectType.File,
                "parentId",
                "changeToken",
                fileLength)
            {
                Guid = Guid.NewGuid(),
                LastRemoteWriteTimeUtc = modificationDate.AddMinutes(1),
                LastLocalWriteTimeUtc = modificationDate,
                LastChecksum = new byte[20],
                ChecksumAlgorithmName = "SHA1"
            };
            storage.AddMappedFile(mappedObject, "path");
            var remoteFile = MockOfIDocumentUtil.CreateRemoteDocumentMock(null, "remoteId", "name", "parentId", fileLength, new byte[20]);
            using (var uploadedContent = new MemoryStream()) {
                remoteFile.Setup(r => r.SetContentStream(It.IsAny<IContentStream>(), true, true)).Callback<IContentStream, bool, bool>(
                    (s, o, r) =>
                    { s.Stream.CopyTo(uploadedContent);
                    remoteFile.Setup(f => f.LastModificationDate).Returns(newModificationDate);
                    remoteFile.Setup(f => f.ChangeToken).Returns(newChangeToken);
                }
                );

                new LocalObjectChanged(queue.Object).Solve(Mock.Of<ISession>(), storage.Object, localFile.Object, remoteFile.Object);

                storage.VerifySavedMappedObject(
                    MappedObjectType.File,
                    "remoteId",
                    mappedObject.Name,
                    mappedObject.ParentId,
                    newChangeToken,
                    true,
                    localFile.Object.LastWriteTimeUtc,
                    expectedHash,
                    fileLength);
                remoteFile.VerifySetContentStream();
                queue.Verify(q => q.AddEvent(It.Is<FileTransmissionEvent>(e => e.Path == localFile.Object.FullName && e.Type == FileTransmissionType.UPLOAD_MODIFIED_FILE)), Times.Once());
                Assert.That(uploadedContent.ToArray(), Is.EqualTo(content));
                Assert.That(localFile.Object.LastWriteTimeUtc, Is.EqualTo(newModificationDate));
            }
        }
    }
}