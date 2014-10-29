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
    using System.IO;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.PathMatcher;
    using CmisSync.Lib.Storage.Database.Entities;

    using DotCMIS.Client;

    using Moq;

    using NUnit.Framework;
    
    [TestFixture]
    public class FileTransmissionObjectsTest
    {
        private string LocalPath = null;

        [SetUp]
        public void SetUp()
        {
            LocalPath = Path.GetTempFileName();
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(LocalPath))
            {
                File.Delete(LocalPath);
            }
        }

        [Test, Category("Fast"), Category("FileTransmissionObjects")]
        public void ConstructorTakesData()
        {
            var remoteFile = new Mock<IDocument>();
            remoteFile.Setup(m => m.Paths).Returns(new List<string>() { "/RemoteFile" });
            remoteFile.Setup(m=>m.Id).Returns("RemoteId");
            remoteFile.Setup(m => m.ChangeToken).Returns("ChangeToken");
            remoteFile.Setup(m => m.LastModificationDate).Returns(File.GetLastWriteTimeUtc(LocalPath));
            var matcher = new Mock<IPathMatcher>();
            matcher.Setup(m => m.CanCreateLocalPath(remoteFile.Object)).Returns(true);
            matcher.Setup(m => m.CanCreateRemotePath(LocalPath)).Returns(true);
            matcher.Setup(m => m.Matches(LocalPath, remoteFile.Object.Paths[0])).Returns(true);
            matcher.Setup(m => m.GetRelativeLocalPath(LocalPath)).Returns(LocalPath);
            var obj = new FileTransmissionObject(FileTransmissionType.UPLOAD_NEW_FILE, LocalPath, remoteFile.Object, matcher.Object);
            Assert.AreEqual(LocalPath, obj.RelativePath);
            Assert.AreEqual(FileTransmissionType.UPLOAD_NEW_FILE, obj.Type);
            Assert.AreEqual("RemoteId", obj.RemoteObjectId);
            Assert.AreEqual("ChangeToken", obj.LastChangeToken);
            Assert.AreEqual(null, obj.LastChecksum);
            Assert.AreEqual(null, obj.ChecksumAlgorithmName);
            Assert.AreEqual(File.GetLastWriteTimeUtc(LocalPath), obj.LastLocalWriteTimeUtc);
            Assert.AreEqual(File.GetLastWriteTimeUtc(LocalPath), obj.LastRemoteWriteTimeUtc);
            var obj2 = new FileTransmissionObject(FileTransmissionType.UPLOAD_NEW_FILE, LocalPath, remoteFile.Object, matcher.Object);
            Assert.IsTrue(obj.Equals(obj2));
        }

        [Test, Category("Fast"), Category("FileTransmissionObjects")]
        public void ConstructorThrowsExceptionIfLocalPathIsInvalid()
        {
            //Local path is null
            Assert.Throws<ArgumentNullException>(() => new FileTransmissionObject(FileTransmissionType.UPLOAD_NEW_FILE, null, Mock.Of<IDocument>(), Mock.Of<IPathMatcher>()));

            //Local path is empty
            Assert.Throws<ArgumentException>(() => new FileTransmissionObject(FileTransmissionType.UPLOAD_NEW_FILE, string.Empty, Mock.Of<IDocument>(), Mock.Of<IPathMatcher>()));
        }

        [Test, Category("Fast"), Category("FileTransmissionObjects")]
        public void ConstructorThrowsExceptionIfRemoteFileIsInvalid()
        {
            //Remote file is null
            Assert.Throws<ArgumentNullException>(() => new FileTransmissionObject(FileTransmissionType.UPLOAD_NEW_FILE, LocalPath, null, Mock.Of<IPathMatcher>()));

            //Paths for remote file is null
            var remoteFile = new Mock<IDocument>();
            Assert.Throws<ArgumentNullException>(() => new FileTransmissionObject(FileTransmissionType.UPLOAD_NEW_FILE, LocalPath, remoteFile.Object, Mock.Of<IPathMatcher>()));

            //Paths for remote file is zero size
            remoteFile.Setup(m => m.Paths).Returns(new List<string>());
            Assert.Throws<ArgumentException>(() => new FileTransmissionObject(FileTransmissionType.UPLOAD_NEW_FILE, LocalPath, remoteFile.Object, Mock.Of<IPathMatcher>()));

            //Paths[0] for remote file is null
            remoteFile.Setup(m => m.Paths).Returns(new List<string>() { null });
            Assert.Throws<ArgumentNullException>(() => new FileTransmissionObject(FileTransmissionType.UPLOAD_NEW_FILE, LocalPath, remoteFile.Object, Mock.Of<IPathMatcher>()));

            //Paths[0] for remote file is empty
            remoteFile.Setup(m => m.Paths).Returns(new List<string>() { string.Empty });
            Assert.Throws<ArgumentException>(() => new FileTransmissionObject(FileTransmissionType.UPLOAD_NEW_FILE, LocalPath, remoteFile.Object, Mock.Of<IPathMatcher>()));
        }

        [Test, Category("Fast"), Category("FileTransmissionObjects")]
        public void ConstructorThrowsExceptionIfMatcherIsInvalid()
        {
            var remoteFile = new Mock<IDocument>();
            remoteFile.Setup(m => m.Paths).Returns(new List<string>() { "/RemoteFile" });
            //Matcher is null
            Assert.Throws<ArgumentNullException>(() => new FileTransmissionObject(FileTransmissionType.UPLOAD_NEW_FILE, LocalPath, remoteFile.Object, null));
        }

        [Test, Category("Fast"), Category("FileTransmissionObjects")]
        [ExpectedException(typeof(ArgumentException))]
        public void ConstructorThrowsExceptionIfLocalPathDoesNotMatchMatcher()
        {
            var remoteFile = new Mock<IDocument>();
            remoteFile.Setup(m => m.Paths).Returns(new List<string>() { "/RemoteFile" });
            var matcher = new Mock<IPathMatcher>();
            matcher.Setup(m => m.CanCreateLocalPath(remoteFile.Object)).Returns(true);
            matcher.Setup(m => m.CanCreateRemotePath(LocalPath)).Returns(false);
            new FileTransmissionObject(FileTransmissionType.UPLOAD_NEW_FILE, LocalPath, remoteFile.Object, matcher.Object);
        }

        [Test, Category("Fast"), Category("FileTransmissionObjects")]
        [ExpectedException(typeof(ArgumentException))]
        public void ConstructorThrowsExceptionIfRemoteFileDoesNotMatchMatcher()
        {
            var remoteFile = new Mock<IDocument>();
            remoteFile.Setup(m => m.Paths).Returns(new List<string>() { "/RemoteFile" });
            var matcher = new Mock<IPathMatcher>();
            matcher.Setup(m => m.CanCreateLocalPath(remoteFile.Object)).Returns(false);
            matcher.Setup(m => m.CanCreateRemotePath(LocalPath)).Returns(true);
            new FileTransmissionObject(FileTransmissionType.UPLOAD_NEW_FILE, LocalPath, remoteFile.Object, matcher.Object);
        }

        [Test, Category("Fast"), Category("FileTransmissionObjects")]
        [ExpectedException(typeof(ArgumentException))]
        public void ConstructorThrowsExceptionIfLocalPathDoesNotMatchRemoteFile()
        {
            var remoteFile = new Mock<IDocument>();
            remoteFile.Setup(m => m.Paths).Returns(new List<string>() { "/RemoteFile" });
            var matcher = new Mock<IPathMatcher>();
            matcher.Setup(m => m.CanCreateLocalPath(remoteFile.Object)).Returns(true);
            matcher.Setup(m => m.CanCreateRemotePath(LocalPath)).Returns(true);
            matcher.Setup(m => m.Matches(LocalPath, remoteFile.Object.Paths[0])).Returns(false);
            new FileTransmissionObject(FileTransmissionType.UPLOAD_NEW_FILE, LocalPath, remoteFile.Object, matcher.Object);
        }

        [Test, Category("Fast"), Category("FileTransmissionObjects")]
        public void ConstructorThrowsExceptionIfLocalPathIsNotFile()
        {
            var remoteFile = new Mock<IDocument>();
            remoteFile.Setup(m => m.Paths).Returns(new List<string>() { "/RemoteFile" });
            var matcher = new Mock<IPathMatcher>();
            matcher.Setup(m => m.CanCreateLocalPath(remoteFile.Object)).Returns(true);

            //Local path does not exist
            string localPath = LocalPath + ".NoExist";
            matcher.Setup(m => m.CanCreateRemotePath(localPath)).Returns(true);
            matcher.Setup(m => m.Matches(localPath, remoteFile.Object.Paths[0])).Returns(true);
            Assert.Throws<ArgumentException>(() => new FileTransmissionObject(FileTransmissionType.UPLOAD_NEW_FILE, localPath, remoteFile.Object, matcher.Object));

            //Local path is a directory
            localPath = Path.GetDirectoryName(LocalPath);
            matcher.Setup(m => m.CanCreateRemotePath(localPath)).Returns(true);
            matcher.Setup(m => m.Matches(localPath, remoteFile.Object.Paths[0])).Returns(true);
            Assert.Throws<ArgumentException>(() => new FileTransmissionObject(FileTransmissionType.UPLOAD_NEW_FILE, localPath, remoteFile.Object, matcher.Object));
        }
    }
}
