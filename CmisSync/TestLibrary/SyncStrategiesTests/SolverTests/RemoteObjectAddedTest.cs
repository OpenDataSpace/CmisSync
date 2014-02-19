using System;

using CmisSync.Lib.Sync.Solver;

using NUnit.Framework;

using Moq;

namespace TestLibrary.SyncStrategiesTests.SolverTests
{
    [TestFixture]
    public class RemoteObjectAddedTest
    {
        [Test, Category("Fast"), Category("Solver")]
        public void DefaultConstructorTest()
        {
            new RemoteObjectAdded();
        }

        [Ignore]
        [Test, Category("Medium"), Category("Solver")]
        public void RemoteDocumentAdded()
        {
            Assert.Fail("TODO");
        }

        [Ignore]
        [Test, Category("Medium"), Category("Solver")]
        public void RemoteFolderAdded()
        {
            Assert.Fail("TODO");
        }
    }
}

