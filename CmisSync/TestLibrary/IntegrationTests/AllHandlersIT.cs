using System;
using System.IO;
using System.Collections.Generic;

using CmisSync.Lib;
using CmisSync.Lib.Events;
using CmisSync.Lib.Storage;
using Strategy = CmisSync.Lib.Sync.Strategy;
using CmisSync.Lib.Sync.Strategy;

using DotCMIS.Client;
using DotCMIS.Data;
using DotCMIS.Data.Extensions;
using DotCMIS.Binding.Services;

using NUnit.Framework;

using Moq;

using TestLibrary.TestUtils;

namespace TestLibrary.IntegrationTests
{
    [TestFixture]
    public class AllHandlersIT
    {
        private readonly bool isPropertyChangesSupported = false;
        private readonly string repoId = "repoId";
        private readonly int maxNumberOfContentChanges = 1000;
        
        [Test, Category("Fast")]
        public void RemoteSecurityChangeOfExistingFile ()
        {
            var session = new Mock<ISession>();
            var database = new Mock<IDatabase>();
            var storage = new Mock<IMetaDataStorage>();

            var manager = new SyncEventManager();
            SingleStepEventQueue queue = new SingleStepEventQueue(manager);

            var observer = new ObservableHandler();
            manager.AddEventHandler(observer);

            var changes = new ContentChanges (session.Object, database.Object, queue, maxNumberOfContentChanges, isPropertyChangesSupported);
            manager.AddEventHandler(changes);

            var transformer = new ContentChangeEventTransformer(queue, database.Object);
            manager.AddEventHandler(transformer);

            var accumulator = new ContentChangeEventAccumulator(session.Object, queue);
            manager.AddEventHandler(accumulator);

            /* FileSystemWatcher is not mockable
            var fsWatcher = new Mock<FileSystemWatcher>();
            fsWatcher.Setup(f=>f.Path).Returns("/tmp");
            var watcher = new Strategy.Watcher(fsWatcher.Object, queue);
            manager.AddEventHandler(watcher);
            */

            var localDetection = new LocalSituationDetection();
            var remoteDetection = new RemoteSituationDetection(session.Object);
            var syncMechanism = new SyncMechanism(localDetection, remoteDetection, queue, session.Object, storage.Object);


        }



    }
}

