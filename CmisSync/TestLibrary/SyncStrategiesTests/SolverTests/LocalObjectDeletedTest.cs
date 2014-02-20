using System;

using CmisSync.Lib.Sync.Solver;
using CmisSync.Lib.Storage;

using Moq;

using NUnit.Framework;

namespace TestLibrary.SyncStrategiesTests.SolverTests
{
    [TestFixture]
    public class LocalObjectDeletedTest
    {
        [Test, Category("Fast"), Category("Solver")]
        public void DefaultConstructorTest()
        {
            new LocalObjectDeleted();
        }

        [Ignore]
        [Test, Category("Medium"), Category("Solver")]
        public void LocalFileDeleted()
        {
            Assert.Fail ("TODO");
        }

        [Ignore]
        [Test, Category("Medium"), Category("Solver")]
        public void LocalFolderDeleted()
        {
            Assert.Fail ("TODO");
        }

    }
}

