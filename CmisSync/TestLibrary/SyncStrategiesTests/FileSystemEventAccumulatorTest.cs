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
        private static readonly string path = "/path";
        private static readonly string id = "myId";


        [Test, Category("Fast")]
        public void ConstructorTest () {
            var queue = new Mock<ISyncEventQueue>();
            var session = new Mock<ISession>();
            var storage = new Mock<IMetaDataStorage> ();
            var accumulator = new FileSystemEventAccumulator (queue.Object, session.Object, storage.Object);
        }

        [Test, Category("Fast")]
        public void FileEventWithoutObjectId () {
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

        [Test, Category("Fast")]
        public void FileEventForRemovedFile () {
            var queue = new Mock<ISyncEventQueue>();
            var session = new Mock<ISession>();
            session.SetupSessionDefaultValues();
            session.Setup(s => s.GetObject(id)).Throws(new CmisObjectNotFoundException());

            var storage = new Mock<IMetaDataStorage> ();
            storage.AddLocalFile(path, id);

            var fileEvent = new FileEvent(new FileInfoWrapper(new FileInfo(path)));

            var accumulator = new FileSystemEventAccumulator (queue.Object, session.Object, storage.Object);
            Assert.That(accumulator.Handle(fileEvent), Is.False);
            Assert.That(fileEvent.RemoteFile, Is.Null);
        }

        [Test, Category("Fast")]
        public void FileEventWithIDocument () {
            var queue = new Mock<ISyncEventQueue>();
            var session = new Mock<ISession>();
            var storage = new Mock<IMetaDataStorage>();
            var accumulator = new FileSystemEventAccumulator (queue.Object, session.Object, storage.Object);
            var fileEvent = new FileEvent(new Mock<IFileInfo>().Object, null, new Mock<IDocument>().Object); 
            accumulator.Handle(fileEvent);
            session.Verify(s => s.GetObject(It.IsAny<string>()), Times.Never());
        }

        [Test, Category("Fast")]
        public void FolderEventWithIFolder () {
            var queue = new Mock<ISyncEventQueue>();
            var session = new Mock<ISession>();
            var storage = new Mock<IMetaDataStorage>();
            var accumulator = new FileSystemEventAccumulator (queue.Object, session.Object, storage.Object);
            var fileEvent = new FolderEvent(new Mock<IDirectoryInfo>().Object, new Mock<IFolder>().Object); 
            accumulator.Handle(fileEvent);
            session.Verify(s => s.GetObject(It.IsAny<string>()), Times.Never());
        }

        [Test, Category("Fast")]
        public void FolderEventWithoutObjectId () {
            var queue = new Mock<ISyncEventQueue>();

            var session = new Mock<ISession>();
            session.SetupSessionDefaultValues();
            IFolder remote = MockUtil.CreateRemoteFolderMock(id).Object;
            session.Setup(s => s.GetObject(id)).Returns(remote);

            var storage = new Mock<IMetaDataStorage> ();
            storage.AddLocalFolder(path, id);

            var folderEvent = new FolderEvent(new DirectoryInfoWrapper(new DirectoryInfo(path)));
            var accumulator = new FileSystemEventAccumulator (queue.Object, session.Object, storage.Object);

            Assert.That(accumulator.Handle(folderEvent), Is.False);
            Assert.That(folderEvent.RemoteFolder, Is.Not.Null);
        }

        [Test, Category("Fast")]
        public void FolderEventForRemovedFolder () {
            var queue = new Mock<ISyncEventQueue>();
            var session = new Mock<ISession>();
            session.SetupSessionDefaultValues();
            session.Setup(s => s.GetObject(id)).Throws(new CmisObjectNotFoundException());

            var storage = new Mock<IMetaDataStorage> ();
            storage.AddLocalFolder(path, id);

            var folderEvent = new FolderEvent(new DirectoryInfoWrapper(new DirectoryInfo(path)));

            var accumulator = new FileSystemEventAccumulator (queue.Object, session.Object, storage.Object);
            Assert.That(accumulator.Handle(folderEvent), Is.False);
            Assert.That(folderEvent.RemoteFolder, Is.Null);
        }
    }
}
