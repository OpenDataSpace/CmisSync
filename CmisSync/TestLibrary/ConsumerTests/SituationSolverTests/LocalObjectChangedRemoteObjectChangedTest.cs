//-----------------------------------------------------------------------
// <copyright file="LocalObjectChangedRemoteObjectChangedTest.cs" company="GRAU DATA AG">
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

    using CmisSync.Lib.Consumer.SituationSolver;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;

    using Moq;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture]
    public class LocalObjectChangedRemoteObjectChangedTest
    {
        private Mock<ISession> session;
        private Mock<IMetaDataStorage> storage;
        private LocalObjectChangedRemoteObjectChanged underTest;

        [SetUp]
        public void SetUp() {
            this.session = new Mock<ISession>();
            this.storage = new Mock<IMetaDataStorage>();
            this.underTest = new LocalObjectChangedRemoteObjectChanged(this.session.Object, this.storage.Object);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void ConstructorTakesSessionAndStorage() {
            new LocalObjectChangedRemoteObjectChanged(Mock.Of<ISession>(), Mock.Of<IMetaDataStorage>());
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalAndRemoteFolderAreChanged() {
            string folderId = "folderId";
            string folderName = "folderName";
            string parentId = "parentId";
            string lastChangeToken = "lastChangeToken";
            string newChangeToken = "newChangeToken";
            DateTime lastLocalModification = DateTime.UtcNow.AddDays(1);
            DateTime lastRemoteModification = DateTime.UtcNow.AddHours(1);
            var localFolder = Mock.Of<IDirectoryInfo>(f => f.LastWriteTimeUtc == lastLocalModification);
            var remoteFolder = Mock.Of<IFolder>(f => f.LastModificationDate == lastRemoteModification && f.Id == folderId && f.ChangeToken == newChangeToken);
            var mappedFolder = Mock.Of<IMappedObject>(
                o =>
                o.Name == folderName &&
                o.RemoteObjectId == folderId &&
                o.LastChangeToken == lastChangeToken &&
                o.LastLocalWriteTimeUtc == DateTime.UtcNow &&
                o.LastRemoteWriteTimeUtc == DateTime.UtcNow &&
                o.ParentId == parentId &&
                o.Type == MappedObjectType.Folder &&
                o.Guid == Guid.NewGuid());
            storage.AddMappedFolder(mappedFolder);

            this.underTest.Solve(localFolder, remoteFolder, ContentChangeType.NONE, ContentChangeType.NONE);

            this.storage.VerifySavedMappedObject(MappedObjectType.Folder, folderId, folderName, parentId, newChangeToken, lastLocalModification: lastLocalModification, lastRemoteModification: lastRemoteModification);
        }
    }
}