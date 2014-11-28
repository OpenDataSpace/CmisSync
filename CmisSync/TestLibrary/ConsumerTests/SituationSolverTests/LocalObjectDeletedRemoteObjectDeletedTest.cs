//-----------------------------------------------------------------------
// <copyright file="LocalObjectDeletedRemoteObjectDeletedTest.cs" company="GRAU DATA AG">
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
    using System.Collections.Generic;
    using System.IO;

    using CmisSync.Lib.Consumer.SituationSolver;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;
    using DotCMIS.Data;
    using DotCMIS.Enums;
    using DotCMIS.Exceptions;

    using Moq;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture]
    public class LocalObjectDeletedRemoteObjectDeletedTest
    {
        private Mock<IMetaDataStorage> storage;
        private Mock<ISession> session;
        private LocalObjectDeletedRemoteObjectDeleted underTest;

        [SetUp]
        public void SetUp() {
            this.storage = new Mock<IMetaDataStorage>();
            this.session = new Mock<ISession>();
            this.session.SetupTypeSystem();
            this.underTest = new LocalObjectDeletedRemoteObjectDeleted(this.session.Object, this.storage.Object);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void DefaultConstructorTest()
        {
            new LocalObjectDeletedRemoteObjectDeleted(this.session.Object, this.storage.Object);
        }

        [Test, Category("Fast"), Category("Solver")]
        public void LocalFileDeleted()
        {
            string tempFile = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());

            string remoteDocumentId = "DocumentId";

            this.storage.AddLocalFile(tempFile, remoteDocumentId);

            this.underTest.Solve(new FileSystemInfoFactory().CreateFileInfo(tempFile), null);

            this.storage.Verify(s => s.RemoveObject(It.Is<IMappedObject>(o => o.RemoteObjectId == remoteDocumentId)), Times.Once());
        }
    }
}