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
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using CmisSync.Lib.Sync.Solver;
using CmisSync.Lib.Storage;

using DotCMIS.Client;

using Moq;

using NUnit.Framework;
using DotCMIS.Data;
using DotCMIS.Enums;

namespace TestLibrary.SyncStrategiesTests.SolverTests
{
    [TestFixture]
    public class LocalObjectAddedTest
    {
        private Mock<ISession> Session;
        private Mock<IMetaDataStorage> Storage;

        [SetUp]
        public void SetUp()
        {
            Session = new Mock<ISession>();
            Storage = new Mock<IMetaDataStorage>();
        }

        [Test, Category("Fast"), Category("Solver")]
        public void DefaultConstructorTest()
        {
            new LocalObjectAdded();
        }

        [Ignore]
        [Test, Category("Medium"), Category("Solver")]
        public void LocalFileAdded()
        {
            string tempFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            bool remoteObjectCreated = false;
            var docId = new Mock<IObjectId>();
            docId.Setup(d => d.Id).Returns("DocumentId");
            string remoteParentFolderId = "parentFolder";
            var remoteParentFolder = new Mock<ICmisObject>();
            try{
                using(File.Create(tempFile));
                var solver = new LocalObjectAdded();
                remoteParentFolder.Setup( f => f.Id).Returns(remoteParentFolderId);
                Session.Setup(
                    s => s.CreateDocument(
                    It.IsAny<IDictionary<string,object>>(),
                    It.Is<IObjectId>(id => id.Id == remoteParentFolderId),
                    It.IsAny<IContentStream>(),
                    It.IsAny<VersioningState?>())
                    ).Returns(docId.Object).Callback(() => remoteObjectCreated = true);
                Session.Setup(
                    s => s.GetObject(
                    It.Is<IObjectId>((id) => id.Equals(remoteParentFolderId)))
                    ).Returns(remoteParentFolder.Object);

                solver.Solve(Session.Object, Storage.Object, new Mock<IFileInfo>().Object, null);
                Assert.IsTrue(remoteObjectCreated);
                Assert.Fail("TODO");
            }finally{
                File.Delete(tempFile);
            }
        }

        [Ignore]
        [Test, Category("Medium"), Category("Solver")]
        public void LocalFolderAdded()
        {
            string tempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            var folderId = new Mock<IObjectId>();
            folderId.Setup(f => f.Id).Returns("FolderId");
            try{
                Directory.CreateDirectory(tempFolder);
                Assert.Fail("TODO");
            } finally {
                Directory.Delete(tempFolder);
            }
        }
    }
}

