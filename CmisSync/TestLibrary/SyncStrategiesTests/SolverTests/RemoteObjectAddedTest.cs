namespace TestLibrary.SyncStrategiesTests.SolverTests
{
    using System;

    using CmisSync.Lib.Sync.Solver;

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

        }
    }
}

