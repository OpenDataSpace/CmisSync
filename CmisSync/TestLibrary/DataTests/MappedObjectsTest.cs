//-----------------------------------------------------------------------
// <copyright file="MappedObjectsTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.DataTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using CmisSync.Lib.Data;
    using CmisSync.Lib.Storage;

    using DotCMIS.Client;

    using Moq;

    using NUnit.Framework;

    using TestUtils;
    
    [TestFixture]
    public class MappedObjectsTest
    {
        private readonly string localRootPathName = "folder";
        private readonly string localRootPath = Path.Combine("local", "test", "folder");
        private readonly string localFileName = "file.test";
        private readonly string localFilePath = Path.Combine("local", "test", "folder", "file.test");

        [Test, Category("Fast"), Category("MappedObjects")]
        public void ConstructorTakesData()
        {
            var data = new MappedObject("name", "remoteId", MappedObjectType.File, "parentId", "changeToken") {
                LastChecksum = new byte[20]
            };

            var file = new MappedObject(data);

            Assert.That(data, Is.EqualTo(file));
        }

        [Test, Category("Fast"), Category("MappedObjects")]
        public void ConstructorSetsDefaultParamsToNull()
        {
            var file = new MappedObject("name", "remoteId", MappedObjectType.File, "parentId", "changeToken");
            Assert.IsNull(file.ChecksumAlgorithmName);
            Assert.IsNull(file.Description);
            Assert.IsNull(file.LastChecksum);
            Assert.IsNull(file.LastLocalWriteTimeUtc);
            Assert.IsNull(file.LastRemoteWriteTimeUtc);
            Assert.AreEqual(-1, file.LastContentSize);
            Assert.That(file.Ignored, Is.False);
            Assert.That(file.ActualOperation, Is.EqualTo(OperationType.No));
            Assert.That(file.Retries, Is.Empty);
        }

        [Test, Category("Fast"), Category("MappedObjects")]
        public void ConstructorTakesName()
        {
            var obj = new MappedObject("name", "remoteId", MappedObjectType.File, null, null);
            Assert.That(obj.Name, Is.EqualTo("name"));
        }

        [Test, Category("Fast"), Category("MappedObjects")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorThrowsExceptionOnEmptyName()
        {
            new MappedObject(string.Empty, "remoteId", MappedObjectType.File, null, null);
        }

        [Test, Category("Fast"), Category("MappedObjects")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorThrowsExceptionIfNameIsNull()
        {
            new MappedObject(null, "remoteId", MappedObjectType.File, null, null);
        }

        [Test, Category("Fast"), Category("MappedObjects")]
        public void ConstructorTakesRemoteId()
        {
            var obj = new MappedObject("name", "remoteId", MappedObjectType.File, null, null);
            Assert.That(obj.RemoteObjectId, Is.EqualTo("remoteId"));
        }

        [Test, Category("Fast"), Category("MappedObjects")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorThrowsExceptionOnEmptyRemoteId()
        {
            new MappedObject("name", string.Empty, MappedObjectType.File, null, null);
        }

        [Test, Category("Fast"), Category("MappedObjects")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorThrowsExceptionIfRemoteIdIsNull()
        {
            new MappedObject("name", null, MappedObjectType.File, null, null);
        }

        [Test, Category("Fast"), Category("MappedObjects")]
        [ExpectedException(typeof(ArgumentException))]
        public void ConstructorThrowsExceptionIfTypeIsUnknown()
        {
            new MappedObject("name", "remoteId", MappedObjectType.Unkown, null, null);
        }

        [Test, Category("Fast"), Category("MappedObjects")]
        public void ConstructorTakesFileType()
        {
            var obj = new MappedObject("name", "remoteId", MappedObjectType.File, null, null);
            Assert.That(obj.Type, Is.EqualTo(MappedObjectType.File));
        }

        [Test, Category("Fast"), Category("MappedObjects")]
        public void ConstructorTakesFolderType()
        {
            var obj = new MappedObject("name", "remoteId", MappedObjectType.Folder, null, null);
            Assert.That(obj.Type, Is.EqualTo(MappedObjectType.Folder));
        }

        [Test, Category("Fast"), Category("MappedObjects")]
        public void ConstructorTakesNullParentId()
        {
            var obj = new MappedObject("name", "id", MappedObjectType.File, null, null);
            Assert.That(obj.ParentId, Is.Null);
        }

        [Test, Category("Fast"), Category("MappedObjects")]
        public void ConstructorTakesParentId()
        {
            var obj = new MappedObject("name", "id", MappedObjectType.File, "parentId", null);
            Assert.That(obj.ParentId, Is.EqualTo("parentId"));
        }

        [Test, Category("Fast"), Category("MappedObjects")]
        public void ConstructorTakesChangeLogToken()
        {
            var obj = new MappedObject("name", "id", MappedObjectType.File, "parentId", "changes");
            Assert.That(obj.LastChangeToken, Is.EqualTo("changes"));
        }

        [Test, Category("Fast"), Category("MappedObjects")]
        public void HashAlgorithmProperty()
        {
            var file = new MappedObject("name", "remoteId", MappedObjectType.File, null, null) { ChecksumAlgorithmName = "MD5" };
            Assert.AreEqual("MD5", file.ChecksumAlgorithmName);

            file.ChecksumAlgorithmName = "SHA1";
            Assert.AreEqual("SHA1", file.ChecksumAlgorithmName);
        }

        [Test, Category("Fast"), Category("MappedObjects")]
        public void DescriptionProperty()
        {
            var file = new MappedObject("name", "remoteId", MappedObjectType.File, null, null) { Description = "desc" };
            Assert.AreEqual("desc", file.Description);

            file.Description = "other desc";
            Assert.AreEqual("other desc", file.Description);
        }

        [Test, Category("Fast"), Category("MappedObjects")]
        public void IgnoredProperty()
        {
            var obj = new MappedObject("name", "id", MappedObjectType.File, null, null) { Ignored = true };
            Assert.That(obj.Ignored, Is.True);
        }

        [Test, Category("Fast"), Category("MappedObjects")]
        public void RetriesDictionaryProperty()
        {
            var dict = new Dictionary<OperationType, int>();
            dict.Add(OperationType.Download, 1);
            var obj = new MappedObject("name", "id", MappedObjectType.File, null, null) { Retries = dict };
            Assert.That(obj.Retries, Is.EqualTo(dict));
            Assert.That(obj.Retries[OperationType.Download], Is.EqualTo(1));
        }

        [Test, Category("Fast"), Category("MappedObjects")]
        public void IFolderConstructor()
        {
            string folderName = "a";
            string path = Path.Combine(Path.GetTempPath(), folderName);
            string id = "id";
            string parentId = "papa";
            string lastChangeToken = "token";
            Mock<IFolder> remoteObject = MockOfIFolderUtil.CreateRemoteFolderMock(id, folderName, path, parentId, lastChangeToken);
            MappedObject mappedObject = new MappedObject(remoteObject.Object);
            Assert.That(mappedObject.RemoteObjectId, Is.EqualTo(id), "RemoteObjectId incorrect");
            Assert.That(mappedObject.Name, Is.EqualTo(folderName), "Name incorrect");
            Assert.That(mappedObject.ParentId, Is.EqualTo(parentId), "ParentId incorrect");
            Assert.That(mappedObject.LastChangeToken, Is.EqualTo(lastChangeToken), "LastChangeToken incorrect");
            Assert.That(mappedObject.Type, Is.EqualTo(MappedObjectType.Folder), "Type incorrect");
        }

        private Mock<IFileSystemInfoFactory> CreateFactoryWithLocalPathInfos()
        {
            return MappedObjectMockUtils.CreateFsFactory(this.localRootPath, this.localRootPathName, this.localFilePath, this.localFileName);
        }

        public class MappedObjectMockUtils
        {
            public static Mock<IFileSystemInfoFactory> CreateFsFactory(string localRootPath, string localRootPathName, string localFilePath = null, string localFileName = null)
            {
                var factory = new Mock<IFileSystemInfoFactory>();
                var dirinfo = new Mock<IDirectoryInfo>();
                dirinfo.Setup(dir => dir.Name).Returns(localRootPathName);
                dirinfo.Setup(dir => dir.Exists).Returns(true);
                factory.Setup(f => f.CreateDirectoryInfo(It.Is<string>(path => path == localRootPath))).Returns(dirinfo.Object);
                var fileInfo = new Mock<IFileInfo>();
                fileInfo.Setup(file => file.Name).Returns(localFileName);
                fileInfo.Setup(file => file.Exists).Returns(true);
                factory.Setup(f => f.CreateFileInfo(It.Is<string>(path => path == localFilePath))).Returns(fileInfo.Object);
                return factory;
            }
        }
    }
}
