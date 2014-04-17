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
    }
}
