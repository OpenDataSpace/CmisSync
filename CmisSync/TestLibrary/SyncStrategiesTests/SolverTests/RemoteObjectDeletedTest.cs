using System;

using CmisSync.Lib.Sync.Solver;
using CmisSync.Lib.Storage;

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

        [Ignore]
        [Test, Category("Medium"), Category("Solver")]
        public void RemoteDocumentDeleted()
        {
            Assert.Fail("TODO");
        }

        [Ignore]
        [Test, Category("Medium"), Category("Solver")]
        public void RemoteFolderDeleted()
        {
            Assert.Fail("TODO");
        }
    }
}

