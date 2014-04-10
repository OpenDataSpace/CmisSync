using System;
using System.IO;

using CmisSync.Lib.Sync.Solver;
using CmisSync.Lib.Storage;
using CmisSync.Lib.Data;
using TestLibrary.TestUtils;

using DotCMIS.Client;

using Moq;

using NUnit.Framework;

namespace TestLibrary.SyncStrategiesTests.SolverTests
{
    [TestFixture]
    public class RemoteObjectDeletedTest
    {
        [Test, Category("Fast"), Category("Solver")]
        public void DefaultConstructorTest()
        {
            new RemoteObjectDeleted();
        }

        [Test, Category("Medium"), Category("Solver")]
        public void RemoteFolderDeleted()
        {
            string path = Path.Combine(Path.GetTempPath(), "a");
            var session = new Mock<ISession>();
            var storage = new Mock<IMetaDataStorage>();
            Mock<MappedFolder> folder = storage.AddLocalFolder(path, "id");
            var dirInfo = new Mock<IDirectoryInfo>();
            dirInfo.Setup(d => d.FullName).Returns(path);
            var solver = new RemoteObjectDeleted();
            solver.Solve(session.Object, storage.Object, dirInfo.Object, null);            

            dirInfo.Verify(d => d.Delete(true), Times.Once());
            folder.Verify(f => f.Remove(), Times.Once());
        }
    }
}

