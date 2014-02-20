using System;

using CmisSync.Lib.Sync.Solver;
using CmisSync.Lib.Storage;

using Moq;

using NUnit.Framework;

namespace TestLibrary.SyncStrategiesTests.SolverTests
{
    [TestFixture]
    public class LocalObjectMovedTest
    {
        [Test, Category("Fast"), Category("Solver")]
        public void DefaultConstructorTest()
        {
            new LocalObjectMoved();
        }

        [Ignore]
        [Test, Category("Medium"), Category("Solver")]
        public void LocalFileMoved()
        {
            Assert.Fail ("TODO");
        }

        [Ignore]
        [Test, Category("Medium"), Category("Solver")]
        public void LocalFolderMoved()
        {
            Assert.Fail ("TODO");
        }
    }
}

