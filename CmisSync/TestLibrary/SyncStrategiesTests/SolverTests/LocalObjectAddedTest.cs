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
        public void LocalFolderAdded()
        {
            string folderName = "a";
            string path = Path.Combine(Path.GetTempPath(), folderName);
            string id = "id";
            string parentId = "papa";
            string lastChangeToken = "token";
            bool extendedAttributes = false;

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

            var parentDirInfo = Mock.Of<IDirectoryInfo>(d =>
                                                        d.FullName == Path.GetTempPath() &&
                                                        d.Name == Path.GetFileName(Path.GetTempPath()));
            dirInfo.Setup(d => d.Parent).Returns(parentDirInfo);
            this.storage.Setup(s => s.GetObjectByLocalPath(It.Is<IDirectoryInfo>(d => d.FullName == Path.GetTempPath()))).Returns(
                Mock.Of<IMappedObject>(o =>
                                   o.RemoteObjectId == parentId));
            var solver = new LocalObjectAdded();

            solver.Solve(this.session.Object, this.storage.Object, dirInfo.Object, null);

            this.storage.Verify(s => s.SaveMappedObject(It.Is<IMappedObject>(f => this.VerifySavedMappedObject(f, id, folderName, parentId, lastChangeToken, extendedAttributes))), Times.Once());
            this.session.Verify(s => s.CreateFolder(It.Is<IDictionary<string, object>>(p => p.ContainsKey("cmis:name")), It.Is<IObjectId>(o => o.Id == parentId)), Times.Once());
            dirInfo.Verify(d => d.SetExtendedAttribute(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalFolderAddedWithExtendedAttributes()
        {
            string folderName = "a";
            string path = Path.Combine(Path.GetTempPath(), folderName);
            string id = "id";
            string parentId = "papa";
            string lastChangeToken = "token";
            bool extendedAttributes = true;
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

            var parentDirInfo = Mock.Of<IDirectoryInfo>(d =>
                                                        d.FullName == Path.GetTempPath() &&
                                                        d.Name == Path.GetFileName(Path.GetTempPath()));
            dirInfo.Setup(d => d.Parent).Returns(parentDirInfo);
            this.storage.Setup(s => s.GetObjectByLocalPath(It.Is<IDirectoryInfo>(d => d.FullName == Path.GetTempPath()))).Returns(
                Mock.Of<IMappedObject>(o =>
                                   o.RemoteObjectId == parentId));
            var solver = new LocalObjectAdded();

            solver.Solve(this.session.Object, this.storage.Object, dirInfo.Object, null);

            this.storage.Verify(s => s.SaveMappedObject(It.Is<IMappedObject>(f => this.VerifySavedMappedObject(f, id, folderName, parentId, lastChangeToken, extendedAttributes))), Times.Once());
            this.session.Verify(s => s.CreateFolder(It.Is<IDictionary<string, object>>(p => p.ContainsKey("cmis:name")), It.Is<IObjectId>(o => o.Id == parentId)), Times.Once());
            dirInfo.Verify(d => d.SetExtendedAttribute(It.Is<string>(k => k == MappedObject.ExtendedAttributeKey), It.Is<string>(v => !v.Equals(Guid.Empty))), Times.Once());
        }

        private bool VerifySavedMappedObject(IMappedObject o, string remoteId, string name, string parentId, string changeToken, bool extendedAttributeAvailable)
        {
            Assert.That(o.RemoteObjectId, Is.EqualTo(remoteId));
            Assert.That(o.Name, Is.EqualTo(name));
            Assert.That(o.ParentId, Is.EqualTo(parentId));
            Assert.That(o.LastChangeToken, Is.EqualTo(changeToken));
            Assert.That(o.Type, Is.EqualTo(MappedObjectType.Folder));
            if (extendedAttributeAvailable) {
                Assert.That(o.Guid.Equals(Guid.Empty), Is.False);
            } else {
                Assert.That(o.Guid.Equals(Guid.Empty), Is.True);
            }

            return true;
        }
    }
}
