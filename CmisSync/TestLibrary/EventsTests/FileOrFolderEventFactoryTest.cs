//-----------------------------------------------------------------------
// <copyright file="FileOrFolderEventFactoryTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.EventsTests
{
    using System;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Storage;

    using DotCMIS.Client;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class FileOrFolderEventFactoryTest
    {
        [Test, Category("Fast")]
        public void CreateFileAddedEvent()
        {
            var ev = FileOrFolderEventFactory.CreateEvent(Mock.Of<IDocument>(), null, MetaDataChangeType.CREATED);
            Assert.That(ev is FileEvent);
            Assert.That((ev as FileEvent).Remote, Is.EqualTo(MetaDataChangeType.CREATED));
        }

        [Test, Category("Fast")]
        public void CreateFileEvent()
        {
            var ev = FileOrFolderEventFactory.CreateEvent(null, Mock.Of<IFileInfo>());
            Assert.That(ev is FileEvent);
        }

        [Test, Category("Fast")]
        public void CreateFolderMovedEvent()
        {
            var ev = FileOrFolderEventFactory.CreateEvent(Mock.Of<IFolder>(), null, MetaDataChangeType.MOVED, oldRemotePath: "oldPath");
            Assert.That(ev is FolderMovedEvent);
        }
    }
}

