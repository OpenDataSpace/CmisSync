//-----------------------------------------------------------------------
// <copyright file="RemoteObjectChangedTest.cs" company="GRAU DATA AG">
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
    using System.Text;

    using CmisSync.Lib.Data;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Storage;
    using CmisSync.Lib.Sync.Solver;

    using DotCMIS.Client;

    using Moq;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture]
    public class RemoteObjectChangedTest
    {
        [Test, Category("Fast"), Category("Solver")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorThrowsExceptionIfQueueIsNull()
        {
            new RemoteObjectChanged(null);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void ConstructorTakesQueue()
        {
            new RemoteObjectChanged(Mock.Of<ISyncEventQueue>());
        }

        [Test, Category("Fast"), Category("Solver")]
        public void RemoteFolderChanged()
        {
            DateTime creationDate = DateTime.UtcNow;
            string folderName = "a";
            string path = Path.Combine(Path.GetTempPath(), folderName);
            string id = "id";
            string parentId = "papa";
            string lastChangeToken = "token";
            string newChangeToken = "newToken";

            var storage = new Mock<IMetaDataStorage>();
            var queue = new Mock<ISyncEventQueue>();
            var dirInfo = new Mock<IDirectoryInfo>();
            dirInfo.Setup(d => d.FullName).Returns(path);
            dirInfo.Setup(d => d.Name).Returns(folderName);
            dirInfo.Setup(d => d.Parent).Returns(Mock.Of<IDirectoryInfo>());

            var mappedObject = new MappedObject(
                folderName,
                id,
                MappedObjectType.Folder,
                parentId,
                lastChangeToken)
            {
                Guid = Guid.NewGuid()
            };

            storage.AddMappedFolder(mappedObject);

            Mock<IFolder> remoteObject = MockOfIFolderUtil.CreateRemoteFolderMock(id, folderName, path, parentId, newChangeToken);
            remoteObject.Setup(f => f.LastModificationDate).Returns((DateTime?)creationDate);

            new RemoteObjectChanged(queue.Object).Solve(Mock.Of<ISession>(), storage.Object, dirInfo.Object, remoteObject.Object);

            storage.VerifySavedMappedObject(MappedObjectType.Folder, id, folderName, parentId, newChangeToken);
            dirInfo.VerifySet(d => d.LastWriteTimeUtc = It.Is<DateTime>(date => date.Equals(creationDate)), Times.Once());
            queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Never());
        }

        [Test, Category("Fast"), Category("Solver")]
        public void RemoteDocumentChanged()
        {
            DateTime creationDate = DateTime.UtcNow;
            string fileName = "a";
            string path = Path.Combine(Path.GetTempPath(), fileName);
            string id = "id";
            string parentId = "papa";
            string lastChangeToken = "token";
            string newChangeToken = "newToken";
            byte[] newContent = Encoding.UTF8.GetBytes("new Content");
            long oldContentSize = 234;
            long newContentSize = newContent.Length;
            byte[] expectedHash;
            using (var sha1 = new SHA1Managed()) {
                expectedHash = sha1.ComputeHash(newContent);
            }
            var queue = new Mock<ISyncEventQueue>();
            var storage = new Mock<IMetaDataStorage>();
            var mappedObject = new MappedObject(
                fileName,
                id,
                MappedObjectType.File,
                parentId,
                lastChangeToken,
                oldContentSize)
            {
                Guid = Guid.NewGuid(),
                LastLocalWriteTimeUtc = new DateTime(0),
                LastRemoteWriteTimeUtc = new DateTime(0)
            };

            storage.AddMappedFile(mappedObject, path);

            Mock<IDocument> remoteObject = MockOfIDocumentUtil.CreateRemoteDocumentMock(null, id, fileName, parentId, newContentSize, newContent, newChangeToken);
            remoteObject.Setup(r => r.LastModificationDate).Returns(creationDate);

            Mock<IFileInfo> localFile = new Mock<IFileInfo>();
            localFile.SetupProperty(f => f.LastWriteTimeUtc, new DateTime(0));
            localFile.Setup(f => f.Open(FileMode.Truncate, FileAccess.Write, FileShare.Read)).Returns(new MemoryStream());
            localFile.Setup(f => f.FullName).Returns(path);

            new RemoteObjectChanged(queue.Object).Solve(Mock.Of<ISession>(), storage.Object, localFile.Object, remoteObject.Object);

            storage.VerifySavedMappedObject(MappedObjectType.File, id, fileName, parentId, newChangeToken, true, creationDate, expectedHash);
            Assert.That(localFile.Object.LastWriteTimeUtc, Is.EqualTo(creationDate));
            queue.Verify(q => q.AddEvent(It.Is<FileTransmissionEvent>(e => e.Type == FileTransmissionType.DOWNLOAD_MODIFIED_FILE)), Times.Once());
        }
    }
}