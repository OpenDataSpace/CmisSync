using System;

using CmisSync.Lib.Sync.Solver;
using CmisSync.Lib.Storage;

using Moq;

using NUnit.Framework;

namespace TestLibrary.SyncStrategiesTests.SolverTests
{
    [TestFixture]
    public class RemoteObjectRenamedTest
    {
        [Test, Category("Fast"), Category("Solver")]
        public void DefaultConstructorTest()
        {
            new RemoteObjectRenamed();
        }

        [Ignore]
        [Test, Category("Medium"), Category("Solver")]
        public void RemoteDocumentRenamed()
        {
            Assert.Fail("TODO");
        }

        [Ignore]
        [Test, Category("Medium"), Category("Solver")]
        public void RemoteFolderRenamed()
        {
            Assert.Fail("TODO");
        }
    }
}

