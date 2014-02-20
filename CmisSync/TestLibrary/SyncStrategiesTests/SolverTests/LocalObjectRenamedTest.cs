using System;

using CmisSync.Lib.Sync.Solver;
using CmisSync.Lib.Storage;

using Moq;

using NUnit.Framework;

namespace TestLibrary.SyncStrategiesTests.SolverTests
{
    [TestFixture]
    public class LocalObjectRenamedTest
    {
        [Test, Category("Fast"), Category("Solver")]
        public void DefaultConstructorTest()
        {
            new LocalObjectRenamed();
        }

        [Ignore]
        [Test, Category("Medium"), Category("Solver")]
        public void LocalFileRenamed()
        {
            Assert.Fail ("TODO");
        }

        [Ignore]
        [Test, Category("Medium"), Category("Solver")]
        public void LocalFolderRenamed()
        {
            Assert.Fail ("TODO");
        }
    }
}

