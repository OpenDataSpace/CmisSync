using System;

using DotCMIS.Client;
using DotCMIS.Exceptions;

using CmisSync.Lib.Data;
using CmisSync.Lib.Events;
using CmisSync.Lib.Storage;
using CmisSync.Lib.Sync.Strategy;

using NUnit.Framework;

using Moq;
using System.IO;
using TestLibrary.TestUtils;
namespace TestLibrary.SyncStrategiesTests {

    [TestFixture]
    public class LocalObjectFetcherTest 
    {
        [Test, Category("Fast")]
        public void ConstructorTest () {
            var matcher = new Mock<IPathMatcher>();
            new LocalObjectFetcher (matcher.Object);
        }

        [Test, Category("Fast")]
        public void IgnoresOtherEvents() {
            var matcher = new Mock<IPathMatcher>();

            var syncEvent = new Mock<ISyncEvent>();
            var fetcher = new LocalObjectFetcher (matcher.Object);

            Assert.That(fetcher.Handle(syncEvent.Object), Is.False);
        }

        [Test, Category("Fast")]
        public void FetchLocalFolder () {
            var localPath = Path.GetTempPath();
            var remotePath = Path.Combine(Path.GetTempPath(), "a");

            var matcher = new Mock<IPathMatcher>();
            matcher.Setup(m => m.CreateLocalPath(remotePath)).Returns(localPath);

            var remoteFolder = new Mock<IFolder>();
            remoteFolder.Setup(f => f.Path).Returns(remotePath);

            var folderEvent = new FolderEvent(remoteFolder: remoteFolder.Object);
            var fetcher = new LocalObjectFetcher (matcher.Object);

            Assert.That(fetcher.Handle(folderEvent), Is.False);
            Assert.That(folderEvent.LocalFolder, Is.Not.Null);
            Assert.That(folderEvent.LocalFolder.FullName, Is.EqualTo(localPath));
        }
        
        [Test, Category("Fast")]
        public void FetchOnlyIfLocalFolderNull() {
            var matcher = new Mock<IPathMatcher>();

            var remoteFolder = new Mock<IFolder>();

            var folder = new Mock<IDirectoryInfo>();

            var folderEvent = new FolderEvent(remoteFolder: remoteFolder.Object, localFolder: folder.Object);
            var fetcher = new LocalObjectFetcher (matcher.Object);

            Assert.That(fetcher.Handle(folderEvent), Is.False);
            Assert.That(folderEvent.LocalFolder, Is.EqualTo(folder.Object));
        }

        [Test, Category("Fast")]
        public void FetchLocalFile () {
            var localPath = Path.GetTempPath();
            var remotePath = Path.Combine(Path.GetTempPath(), "a");

            var matcher = new Mock<IPathMatcher>();
            matcher.Setup(m => m.CreateLocalPath(remotePath)).Returns(localPath);

            var remoteFile = new Mock<IDocument>();
            remoteFile.Setup(f => f.Paths).Returns(new string[] {remotePath});

            var fileEvent = new FileEvent(remoteFile: remoteFile.Object);
            var fetcher = new LocalObjectFetcher (matcher.Object);

            Assert.That(fetcher.Handle(fileEvent), Is.False);
            Assert.That(fileEvent.LocalFile, Is.Not.Null);
            Assert.That(fileEvent.LocalFile.FullName, Is.EqualTo(localPath));
        }

        [Test, Category("Fast")]
        public void FetchOnlyIfLocalFileNull() {
            var matcher = new Mock<IPathMatcher>();

            var remoteFile = new Mock<IDocument>();

            var file = new Mock<IFileInfo>();

            var fileEvent = new FileEvent(remoteFile: remoteFile.Object, localFile: file.Object);
            var fetcher = new LocalObjectFetcher (matcher.Object);

            Assert.That(fetcher.Handle(fileEvent), Is.False);
            Assert.That(fileEvent.LocalFile, Is.EqualTo(file.Object));
        }
    }
}
