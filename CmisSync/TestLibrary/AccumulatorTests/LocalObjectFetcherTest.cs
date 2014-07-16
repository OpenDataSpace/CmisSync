//-----------------------------------------------------------------------
// <copyright file="LocalObjectFetcherTest.cs" company="GRAU DATA AG">
//
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General private License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//
//   This program is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//   GNU General private License for more details.
//
//   You should have received a copy of the GNU General private License
//   along with this program. If not, see http://www.gnu.org/licenses/.
//
// </copyright>
//-----------------------------------------------------------------------

namespace TestLibrary.AccumulatorTests
{
    using System;
    using System.IO;

    using CmisSync.Lib.Accumulator;
    using CmisSync.Lib.Data;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.PathMatcher;
    using CmisSync.Lib.Storage;
    using CmisSync.Lib.Producer.Watcher;

    using DotCMIS.Client;
    using DotCMIS.Exceptions;

    using Moq;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture]
    public class LocalObjectFetcherTest 
    {
        [Test, Category("Fast")]
        public void ConstructorTest() {
            var matcher = new Mock<IPathMatcher>();
            new LocalObjectFetcher(matcher.Object);
        }

        [Test, Category("Fast")]
        public void IgnoresOtherEvents() {
            var matcher = new Mock<IPathMatcher>();

            var syncEvent = new Mock<ISyncEvent>();
            var fetcher = new LocalObjectFetcher(matcher.Object);

            Assert.That(fetcher.Handle(syncEvent.Object), Is.False);
        }

        [Test, Category("Fast")]
        public void FetchLocalFolder() {
            var localPath = Path.GetTempPath();
            var remotePath = Path.Combine(Path.GetTempPath(), "a");

            var matcher = new Mock<IPathMatcher>();
            matcher.Setup(m => m.CreateLocalPath(remotePath)).Returns(localPath);
            matcher.Setup(m => m.CanCreateLocalPath(remotePath)).Returns(true);

            var remoteFolder = new Mock<IFolder>();
            remoteFolder.Setup(f => f.Path).Returns(remotePath);

            var folderEvent = new FolderEvent(remoteFolder: remoteFolder.Object);
            var fetcher = new LocalObjectFetcher(matcher.Object);

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
            var fetcher = new LocalObjectFetcher(matcher.Object);

            Assert.That(fetcher.Handle(folderEvent), Is.False);
            Assert.That(folderEvent.LocalFolder, Is.EqualTo(folder.Object));
        }

        [Test, Category("Fast")]
        public void FetchLocalFile() {
            var localPath = Path.GetTempPath();
            var remotePath = Path.Combine(Path.GetTempPath(), "a");

            var matcher = new Mock<IPathMatcher>();
            matcher.Setup(m => m.CreateLocalPath(remotePath)).Returns(localPath);
            matcher.Setup(m => m.CanCreateLocalPath(remotePath)).Returns(true);

            var remoteFile = new Mock<IDocument>();
            remoteFile.Setup(f => f.Paths).Returns(new string[] { remotePath });

            var fileEvent = new FileEvent(remoteFile: remoteFile.Object);
            var fetcher = new LocalObjectFetcher(matcher.Object);

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
            var fetcher = new LocalObjectFetcher(matcher.Object);

            Assert.That(fetcher.Handle(fileEvent), Is.False);
            Assert.That(fileEvent.LocalFile, Is.EqualTo(file.Object));
        }

        [Test, Category("Fast")]
        public void DropFileEventIfPathMatcherCannotCreateLocalPath() {
            var remotePath = Path.Combine(Path.GetTempPath(), "a");

            var matcher = new Mock<IPathMatcher>();
            matcher.Setup(m => m.CanCreateLocalPath(remotePath)).Returns(false);

            var remoteFile = new Mock<IDocument>();
            remoteFile.Setup(f => f.Paths).Returns(new string[] { remotePath });

            var fileEvent = new FileEvent(remoteFile: remoteFile.Object);
            var fetcher = new LocalObjectFetcher(matcher.Object);

            Assert.That(fetcher.Handle(fileEvent), Is.True);
            Assert.That(fileEvent.LocalFile, Is.Null);
        }

        [Test, Category("Fast")]
        public void DropFolderEventIfPathMatcherCannotCreateLocalPath() {
            var remotePath = Path.Combine(Path.GetTempPath(), "a");

            var matcher = new Mock<IPathMatcher>();
            matcher.Setup(m => m.CanCreateLocalPath(remotePath)).Returns(false);

            var remoteFolder = new Mock<IFolder>();
            remoteFolder.Setup(f => f.Path).Returns(remotePath);

            var folderEvent = new FolderEvent(remoteFolder: remoteFolder.Object);
            var fetcher = new LocalObjectFetcher(matcher.Object);

            Assert.That(fetcher.Handle(folderEvent), Is.True);
            Assert.That(folderEvent.LocalFolder, Is.Null);
        }
    }
}
