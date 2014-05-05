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
        public void DefaultConstructorTest(){
            new LocalObjectAdded();
        }

        [Ignore]
        [Test, Category("Medium"), Category("Solver")]
        public void LocalFileAdded()
        {
            string tempFile = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());

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
                    It.IsAny<IObjectId>(),
                    It.IsAny<IContentStream>(),
                    It.IsAny<VersioningState?>())
                    ).Returns(docId.Object).Callback(() => remoteObjectCreated = true);
                Session.Setup(
                    s => s.GetObject(
                    It.Is<IObjectId>((id) => id.Equals(remoteParentFolderId)))
                    ).Returns(remoteParentFolder.Object);

                solver.Solve(Session.Object, Storage.Object, new FileInfo(tempFile), null);
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
            Assert.Fail("TODO");
        }
    }
}

