using System;

using DotCMIS.Client;
using DotCMIS.Exceptions;

using CmisSync.Lib.Events;
using CmisSync.Lib.Storage;
using CmisSync.Lib.Sync.Strategy;

using NUnit.Framework;

using Moq;
using System.IO;
using TestLibrary.TestUtils;
namespace TestLibrary.SyncStrategiesTests {

    [TestFixture]
    public class FileSystemEventAccumulatorTest 
    {
        [Test, Category("Fast")]
        public void ConstructorTest () {
            var queue = new Mock<ISyncEventQueue>();
            var session = new Mock<ISession>();
            var storage = new Mock<IMetaDataStorage> ();
            var accumulator = new FileSystemEventAccumulator (queue.Object, session.Object, storage.Object);
        }

        [Test, Category("Fast")]
        public void FileEventWithoutObjectId () {
            string path = "/path";
            string id = "myId";
            var queue = new Mock<ISyncEventQueue>();
            var session = new Mock<ISession>();
            session.SetupSessionDefaultValues();
            IDocument remote = MockUtil.CreateRemoteObjectMock(null, id).Object;
            session.Setup(s => s.GetObject(id)).Returns(remote);
            var storage = new Mock<IMetaDataStorage> ();
            storage.AddLocalFile(path, id);
            var fileEvent = new FileEvent(new FileInfoWrapper(new FileInfo(path)));
            var accumulator = new FileSystemEventAccumulator (queue.Object, session.Object, storage.Object);
            Assert.That(accumulator.Handle(fileEvent), Is.False);
            Assert.That(fileEvent.RemoteFile, Is.Not.Null);
        }
    }
}
