using System;

using CmisSync.Lib.Sync.Solver;
using CmisSync.Lib.Storage;

using Moq;

using NUnit.Framework;

namespace TestLibrary.SyncStrategiesTests.SolverTests
{
    [TestFixture]
    public class LocalObjectChangedTest
    {
        [Test, Category("Fast"), Category("Solver")]
        public void DefaultConstructorTest()
        {
            new LocalObjectChanged();
        }

        [Ignore]
        [Test, Category("Medium"), Category("Solver")]
        public void LocalFileChanged()
        {
            Assert.Fail ("TODO");
        }

        [Ignore]
        [Test, Category("Medium"), Category("Solver")]
        public void LocalFolderChanged()
        {
            Assert.Fail ("TODO");
        }
    }
}

