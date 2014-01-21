using System;

using CmisSync.Lib.Sync.Strategy;

using DotCMIS.Client;

using NUnit.Framework;

using Moq;

namespace TestLibrary.SyncStrategiesTests.SituationDetectionTests
{
    [TestFixture]
    public class RemoteSituationDetectionTest
    {
        [Ignore]
        [Test, Category("Fast")]
        public void NoChangeDetectionTest()
        {
            Assert.Fail ();
        }
    }
}

