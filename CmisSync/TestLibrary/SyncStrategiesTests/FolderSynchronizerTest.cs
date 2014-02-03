using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using CmisSync.Lib.Events;
using CmisSync.Lib.Data;
using CmisSync.Lib.Storage;
using CmisSync.Lib.Sync.Strategy;

using DotCMIS.Client;

using NUnit.Framework;

using Moq;

namespace TestLibrary.SyncStrategiesTests
{
    [TestFixture]
    public class FolderSynchronizerTest
    {
        [Ignore]
        [Test, Category("Fast")]
        public void ConstructorTest() {
            Assert.Fail ("TODO");
        }

        [Test, Category("Fast")]
        public void IgnoreWrongEvents () {
            var session = new Mock<ISession>().Object;
            var matcher = new Mock<IPathMatcher>().Object;
            var storage = new Mock<MetaDataStorage>(matcher).Object;
            var queue = new Mock<ISyncEventQueue>().Object;
            var syncer = new FolderSynchronizer(queue, storage, session);
            var wrongEvent = new Mock<ISyncEvent>().Object;
            Assert.False(syncer.Handle(wrongEvent));
        }
    }
}

