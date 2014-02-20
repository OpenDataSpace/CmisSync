using System;

using CmisSync.Lib.Sync.Solver;
using CmisSync.Lib.Storage;

using Moq;

using NUnit.Framework;

namespace TestLibrary.SyncStrategiesTests.SolverTests
{
    [TestFixture]
    public class RemoteObjectChangedTest
    {
        [Test, Category("Fast"), Category("Solver")]
        public void DefaultConstructorTest()
        {
            new RemoteObjectChanged();
        }

        [Ignore]
        [Test, Category("Medium"), Category("Solver")]
        public void RemoteDocumentChanged()
        {
            Assert.Fail("TODO");
        }

        [Ignore]
        [Test, Category("Medium"), Category("Solver")]
        public void RemoteFolderChanged()
        {
            Assert.Fail("TODO");
        }
    }
}

