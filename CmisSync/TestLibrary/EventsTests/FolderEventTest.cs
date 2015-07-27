//-----------------------------------------------------------------------
// <copyright file="FolderEventTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.EventsTests {
    using System;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;

    using Moq;

    using NUnit.Framework;

    using TestUtils;

    [TestFixture, Category("Fast")]
    public class FolderEventTest {
        [Test]
        public void ConstructorFailsIfLocalFolderAndRemoteFolderAreNull() {
            Assert.Throws<ArgumentNullException>(() => new FolderEvent());
        }

        [Test]
        public void ConstructorWithLocalFolder() {
            var localFolder = Mock.Of<IDirectoryInfo>();

            var underTest = new FolderEvent(localFolder);

            Assert.That(underTest.LocalFolder, Is.EqualTo(localFolder));
        }

        [Test]
        public void ConstructorWithRemoteFolder() {
            var remoteFolder = Mock.Of<IFolder>();

            var underTest = new FolderEvent(null, remoteFolder);

            Assert.That(underTest.RemoteFolder, Is.EqualTo(remoteFolder));
        }

        [Test]
        public void ConstructorTakesLocalAndRemoteFolder() {
            var localFolder = Mock.Of<IDirectoryInfo>();
            var remoteFolder = Mock.Of<IFolder>();

            var underTest = new FolderEvent(localFolder, remoteFolder);

            Assert.That(underTest.LocalFolder, Is.EqualTo(localFolder));
            Assert.That(underTest.RemoteFolder, Is.EqualTo(remoteFolder));
        }

        [Test]
        public void LocalPathIsFilterable() {
            var path = "localpath";
            var localFolder = Mock.Of<IDirectoryInfo>(f => f.FullName == path);

            var underTest = new FolderEvent(localFolder);

            Assert.That(underTest.LocalPath, Is.EqualTo(path));
        }

        [Test]
        public void LocalPathReturnsNullIfLocalFolderIsNotSet() {
            var remoteFolder = Mock.Of<IFolder>();

            var underTest = new FolderEvent(null, remoteFolder);

            Assert.That(underTest is IFilterableLocalPathEvent);
            Assert.That(underTest.LocalPath, Is.Null);
        }

        [Test]
        public void IsIFilterableRemoteObjectEvent() {
            var remoteFolder = Mock.Of<IFolder>();

            var underTest = new FolderEvent(null, remoteFolder);

            Assert.That(underTest is IFilterableRemoteObjectEvent);
        }
    }
}