//-----------------------------------------------------------------------
// <copyright file="FileTransmissionObjectsTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.StorageTests.DataBaseTests.EntitiesTests
{
    using System;
    using System.Collections.Generic;
    using System.Security.Cryptography;
    using System.IO;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.PathMatcher;
    using CmisSync.Lib.Storage.FileSystem;
    using CmisSync.Lib.Storage.Database.Entities;

    using DotCMIS.Client;

    using Moq;

    using NUnit.Framework;
    
    [TestFixture]
    public class FileTransmissionObjectsTest
    {
        private Mock<IFileInfo> LocalFile;

        [SetUp]
        public void SetUp()
        {
            LocalFile = new Mock<IFileInfo>();
            LocalFile.SetupAllProperties();
            LocalFile.Setup(f => f.Length).Returns(1024);
            LocalFile.Setup(f => f.Name).Returns("FileTransmissionObjectsTest.file");
            LocalFile.Setup(f => f.FullName).Returns(Path.Combine(Path.GetTempPath(), LocalFile.Object.Name));
            LocalFile.Setup(f => f.Exists).Returns(true);
            LocalFile.Object.LastWriteTimeUtc = DateTime.UtcNow;
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test, Category("Fast"), Category("FileTransmissionObjects")]
        public void ConstructorTakesData()
        {
            var remoteFile = new Mock<IDocument>();
            remoteFile.Setup(m => m.Paths).Returns(new List<string>() { "/RemoteFile" });
            remoteFile.Setup(m => m.Id).Returns("RemoteId");
            remoteFile.Setup(m => m.ChangeToken).Returns("ChangeToken");
            remoteFile.Setup(m => m.LastModificationDate).Returns(LocalFile.Object.LastWriteTimeUtc);
            var obj = new FileTransmissionObject(FileTransmissionType.UPLOAD_NEW_FILE, LocalFile.Object, remoteFile.Object);
            Assert.AreEqual(FileTransmissionType.UPLOAD_NEW_FILE, obj.Type);
            Assert.AreEqual(LocalFile.Object.FullName, obj.LocalPath);
            Assert.AreEqual(LocalFile.Object.Length, obj.LastContentSize);
            Assert.AreEqual(null, obj.LastChecksum);
            Assert.AreEqual(null, obj.ChecksumAlgorithmName);
            Assert.AreEqual(LocalFile.Object.LastWriteTimeUtc, obj.LastLocalWriteTimeUtc);
            Assert.AreEqual("RemoteId", obj.RemoteObjectId);
            Assert.AreEqual("ChangeToken", obj.LastChangeToken);
            Assert.AreEqual(LocalFile.Object.LastWriteTimeUtc, obj.LastRemoteWriteTimeUtc);
            var obj2 = new FileTransmissionObject(FileTransmissionType.UPLOAD_NEW_FILE, LocalFile.Object, remoteFile.Object);
            Assert.IsTrue(obj.Equals(obj2));

            obj.ChecksumAlgorithmName = "SHA1";
            Assert.AreEqual("SHA1", obj.ChecksumAlgorithmName);
            obj.LastChecksum = new byte[32];
            using (var random = RandomNumberGenerator.Create())
            {
                random.GetBytes(obj.LastChecksum);
            }
            Assert.IsFalse(obj.Equals(obj2));

            obj2.ChecksumAlgorithmName = "SHA1";
            obj2.LastChecksum = new byte[32];
            using (var random = RandomNumberGenerator.Create())
            {
                random.GetBytes(obj.LastChecksum);
            }
            Assert.IsFalse(obj.Equals(obj2));

            Buffer.BlockCopy(obj2.LastChecksum, 0, obj.LastChecksum, 0, 32);
            Assert.IsTrue(obj.Equals(obj2));
        }

        [Test, Category("Fast"), Category("FileTransmissionObjects")]
        public void ConstructorThrowsExceptionIfLocalFileIsInvalid()
        {
            //Local file is null
            Assert.Throws<ArgumentNullException>(() => new FileTransmissionObject(FileTransmissionType.UPLOAD_NEW_FILE, null, Mock.Of<IDocument>()));

            //Local file does not exist
            LocalFile.Setup(f => f.Exists).Returns(false);
            Assert.Throws<ArgumentException>(() => new FileTransmissionObject(FileTransmissionType.UPLOAD_NEW_FILE, LocalFile.Object, Mock.Of<IDocument>()));
        }

        [Test, Category("Fast"), Category("FileTransmissionObjects")]
        public void ConstructorThrowsExceptionIfRemoteFileIsInvalid()
        {
            //Remote file is null
            Assert.Throws<ArgumentNullException>(() => new FileTransmissionObject(FileTransmissionType.UPLOAD_NEW_FILE, LocalFile.Object, null));

            //RemoteObjectId for remote file is null
            var remoteFile = new Mock<IDocument>();
            Assert.Throws<ArgumentNullException>(() => new FileTransmissionObject(FileTransmissionType.UPLOAD_NEW_FILE, LocalFile.Object, remoteFile.Object));

            //RemoteObjectId for remote file is empty
            remoteFile.Setup(m => m.Id).Returns(string.Empty);
            Assert.Throws<ArgumentException>(() => new FileTransmissionObject(FileTransmissionType.UPLOAD_NEW_FILE, LocalFile.Object, remoteFile.Object));
        }
    }
}
