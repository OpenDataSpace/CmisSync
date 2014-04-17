namespace TestLibrary.SyncStrategiesTests.SolverTests
{
    using System;
    using System.IO;

    using CmisSync.Lib.Sync.Solver;
    using CmisSync.Lib.Storage;

    using DotCMIS.Client;

    using NUnit.Framework;

    using Moq;

    [TestFixture]
    public class RemoteObjectAddedTest
    {
        [Test, Category("Fast"), Category("Solver")]
        public void DefaultConstructorTest()
        {
            new RemoteObjectAdded();
        }

        [Test, Category("Fast"), Category("Solver")]
        public void RemoteFolderAdded()
        {
            string path = Path.Combine(Path.GetTempPath(), "a");
            var session = new Mock<ISession>();

            var storage = new Mock<IMetaDataStorage>();

            var dirInfo = new Mock<IDirectoryInfo>();
            dirInfo.Setup(d => d.FullName).Returns(path);

            var solver = new RemoteObjectAdded();
            
            solver.Solve(session.Object, storage.Object, dirInfo.Object, null);
            dirInfo.Verify(d => d.Create(), Times.Once());
            Assert.Fail("verify that folder goes to db");
        }
    }
}

