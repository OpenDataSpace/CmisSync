//-----------------------------------------------------------------------
// <copyright file="LocalObjectMovedRemoteObjectMovedTest.cs" company="GRAU DATA AG">
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
using CmisSync.Lib.Storage.Database.Entities;

namespace TestLibrary.ConsumerTests.SituationSolverTests
{
    using System;

    using CmisSync.Lib.Consumer.SituationSolver;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;

    using Moq;

    using NUnit.Framework;

    using TestUtils;

    [TestFixture]
    public class LocalObjectMovedRemoteObjectMovedTest : IsTestWithConfiguredLog4Net
    {
        private readonly string remoteObjectId = "remoteObjectId";
        private readonly string oldRemoteParentId = "oldRemoteParentId";
        private readonly string newRemoteParentId = "newRemoteParentId";
        private Mock<ISession> session;
        private Mock<IMetaDataStorage> storage;

        [SetUp]
        public void SetUp() {
            this.session = new Mock<ISession>();
            this.storage = new Mock<IMetaDataStorage>();
        }

        [Test, Category("Fast"), Category("Solver")]
        public void ConstructorTakesValidInput() {
            new LocalObjectMovedRemoteObjectMoved(this.session.Object, this.storage.Object, true);
        }

        [Ignore]
        [Test, Category("Fast"), Category("Solver")]
        public void BothFoldersAreMovedIntoTheSameFolder() {
            string newChangeToken = "newChangeToken";
            string folderName = "folder";
            var underTest = new LocalObjectMovedRemoteObjectMoved(this.session.Object, this.storage.Object, true);
            var remoteFolder = MockOfIFolderUtil.CreateRemoteFolderMock(this.remoteObjectId, folderName, "/" + folderName, this.newRemoteParentId, newChangeToken);
            underTest.Solve(Mock.Of<IDirectoryInfo>(), remoteFolder.Object, ContentChangeType.NONE, ContentChangeType.NONE);
        }
    }
}