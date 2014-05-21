//-----------------------------------------------------------------------
// <copyright file="LocalObjectAddedTest.cs" company="GRAU DATA AG">
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
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;

    using CmisSync.Lib.Data;
    using CmisSync.Lib.Storage;
    using CmisSync.Lib.Sync.Solver;

    using DotCMIS.Client;
    using DotCMIS.Data;
    using DotCMIS.Enums;

    using Moq;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture]
    public class LocalObjectAddedTest
    {
        private Mock<ISession> session;
        private Mock<IMetaDataStorage> storage;

        [SetUp]
        public void SetUp()
        {
            this.session = new Mock<ISession>();
            this.storage = new Mock<IMetaDataStorage>();
        }

        [Test, Category("Fast"), Category("Solver")]
        public void DefaultConstructorTest()
        {
            new LocalObjectAdded();
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalFileAddedWithoutExtAttr() {
            string fileName = "fileName";
            string fileId = "fileId";
            string parentId = "parentId";
            string lastChangeToken = "token";
            bool extendedAttributes = false;

            Mock<IFileInfo> fileInfo = this.RunSolveFile(fileName, fileId, parentId, lastChangeToken, extendedAttributes);
            this.storage.VerifySavedMappedObject(MappedObjectType.File, fileId, fileName, parentId, lastChangeToken, extendedAttributes);
            this.session.Verify(
                s => s.CreateDocument(
                    It.Is<IDictionary<string, object>>(p => p.ContainsKey("cmis:name")),
                    It.Is<IObjectId>(o => o.Id == parentId),
                    It.Is<IContentStream>(st => st == null),
                    null,
                    null,
                    null,
                    null),
                Times.Once());
            fileInfo.Verify(d => d.SetExtendedAttribute(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalFileAddedWithExtAttr() {
            string fileName = "fileName";
            string fileId = "fileId";
            string parentId = "parentId";
            string lastChangeToken = "token";
            bool extendedAttributes = true;

            Mock<IFileInfo> fileInfo = this.RunSolveFile(fileName, fileId, parentId, lastChangeToken, extendedAttributes);
            this.storage.VerifySavedMappedObject(MappedObjectType.File, fileId, fileName, parentId, lastChangeToken, extendedAttributes);
            this.session.Verify(
                s => s.CreateDocument(
                    It.Is<IDictionary<string, object>>(p => p.ContainsKey("cmis:name")),
                    It.Is<IObjectId>(o => o.Id == parentId),
                    It.Is<IContentStream>(st => st == null),
                    null,
                    null,
                    null,
                    null),
                Times.Once());
            fileInfo.Verify(d => d.SetExtendedAttribute(It.IsAny<string>(), It.IsAny<string>()), Times.Once());
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalFolderAddedWithoutExtAttr()
        {
            string folderName = "a";
            string id = "id";
            string parentId = "papa";
            string lastChangeToken = "token";
            bool extendedAttributes = false;

            var dirInfo = this.RunSolveFolder(folderName, id, parentId, lastChangeToken, extendedAttributes);

            this.storage.VerifySavedMappedObject(MappedObjectType.Folder, id, folderName, parentId, lastChangeToken, extendedAttributes);
            this.session.Verify(s => s.CreateFolder(It.Is<IDictionary<string, object>>(p => p.ContainsKey("cmis:name")), It.Is<IObjectId>(o => o.Id == parentId)), Times.Once());
            dirInfo.Verify(d => d.SetExtendedAttribute(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalFolderAddedWithExtendedAttributes()
        {
            string folderName = "a";

            string id = "id";
            string parentId = "papa";
            string lastChangeToken = "token";
            bool extendedAttributes = true;
            var dirInfo = this.RunSolveFolder(folderName, id, parentId, lastChangeToken, extendedAttributes);

            this.storage.VerifySavedMappedObject(MappedObjectType.Folder, id, folderName, parentId, lastChangeToken, extendedAttributes);
            this.session.Verify(s => s.CreateFolder(It.Is<IDictionary<string, object>>(p => p.ContainsKey("cmis:name")), It.Is<IObjectId>(o => o.Id == parentId)), Times.Once());
            dirInfo.Verify(d => d.SetExtendedAttribute(It.Is<string>(k => k == MappedObject.ExtendedAttributeKey), It.Is<string>(v => !v.Equals(Guid.Empty))), Times.Once());
        }

        private IDirectoryInfo SetupParentFolder(string parentId)
        {
            var parentDirInfo = Mock.Of<IDirectoryInfo>(d =>
                                                  d.FullName == Path.GetTempPath() &&
                                                  d.Name == Path.GetFileName(Path.GetTempPath()));

            this.storage.Setup(s => s.GetObjectByLocalPath(It.Is<IDirectoryInfo>(d => d.FullName == Path.GetTempPath()))).Returns(
                             Mock.Of<IMappedObject>(o =>
                             o.RemoteObjectId == parentId));
            return parentDirInfo;
        }

        private Mock<IFileInfo> RunSolveFile(string fileName, string fileId, string parentId, string lastChangeToken, bool extendedAttributes)
        {
            var parentDirInfo = this.SetupParentFolder(parentId);

            var parents = new List<IFolder>();
            parents.Add(Mock.Of<IFolder>(f => f.Id == parentId));

            string path = Path.Combine(Path.GetTempPath(), fileName);
            var futureRemoteDoc = Mock.Of<IDocument>(f =>
                                                f.Name == fileName &&
                                                f.Id == fileId &&
                                                f.Parents == parents &&
                                                f.ChangeToken == lastChangeToken);
            var futureRemoteDocId = Mock.Of<IObjectId>(o =>
                                                    o.Id == fileId);

            this.session.Setup(s => s.CreateDocument(
                It.Is<IDictionary<string, object>>(p => (string)p["cmis:name"] == fileName),
                It.Is<IObjectId>(o => o.Id == parentId),
                null,
                null,
                null,
                null,
                null)).Returns(futureRemoteDocId);
            this.session.Setup(s => s.GetObject(It.Is<IObjectId>(o => o == futureRemoteDocId))).Returns(futureRemoteDoc);

            var fileInfo = new Mock<IFileInfo>();
            fileInfo.Setup(d => d.FullName).Returns(path);
            fileInfo.Setup(d => d.Name).Returns(fileName);
            fileInfo.Setup(d => d.Exists).Returns(true);
            fileInfo.Setup(d => d.IsExtendedAttributeAvailable()).Returns(extendedAttributes);

            fileInfo.Setup(d => d.Directory).Returns(parentDirInfo);
            var solver = new LocalObjectAdded();

            solver.Solve(this.session.Object, this.storage.Object, fileInfo.Object, null);
            return fileInfo;
        }

        private Mock<IDirectoryInfo> RunSolveFolder(string folderName, string id, string parentId, string lastChangeToken, bool extendedAttributes)
        {
            string path = Path.Combine(Path.GetTempPath(), folderName);
            var futureRemoteFolder = Mock.Of<IFolder>(f =>
                                                   f.Name == folderName &&
                                                   f.Id == id &&
                                                   f.ParentId == parentId &&
                                                   f.ChangeToken == lastChangeToken);
            var futureRemoteFolderId = Mock.Of<IObjectId>(o =>
                                                       o.Id == id);

            this.session.Setup(s => s.CreateFolder(It.Is<IDictionary<string, object>>(p => (string)p["cmis:name"] == folderName), It.Is<IObjectId>(o => o.Id == parentId))).Returns(futureRemoteFolderId);
            this.session.Setup(s => s.GetObject(It.Is<IObjectId>(o => o == futureRemoteFolderId))).Returns(futureRemoteFolder);

            var dirInfo = new Mock<IDirectoryInfo>();
            dirInfo.Setup(d => d.FullName).Returns(path);
            dirInfo.Setup(d => d.Name).Returns(folderName);
            dirInfo.Setup(d => d.Exists).Returns(true);
            dirInfo.Setup(d => d.IsExtendedAttributeAvailable()).Returns(extendedAttributes);

            var parentDirInfo = this.SetupParentFolder(parentId);
            dirInfo.Setup(d => d.Parent).Returns(parentDirInfo);
            var solver = new LocalObjectAdded();

            solver.Solve(this.session.Object, this.storage.Object, dirInfo.Object, null);
            return dirInfo;
        }
    }
}
