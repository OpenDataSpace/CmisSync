//-----------------------------------------------------------------------
// <copyright file="DescendantsCrawlerTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.SyncStrategiesTests
{
    using System;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Storage;
    using CmisSync.Lib.Sync.Strategy;

    using DotCMIS.Client;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class DescendantsCrawlerTest
    {
        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorThrowsExceptionIfLocalFolderIsNull()
        {
            new DescendantsCrawler(Mock.Of<ISyncEventQueue>(), Mock.Of<IFolder>(), null, Mock.Of<IMetaDataStorage>());
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorThrowsExceptionIfRemoteFolderIsNull()
        {
            new DescendantsCrawler(Mock.Of<ISyncEventQueue>(), null, Mock.Of<IDirectoryInfo>(), Mock.Of<IMetaDataStorage>());
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorThrowsExceptionIfQueueIsNull()
        {
            new DescendantsCrawler(null, Mock.Of<IFolder>(), Mock.Of<IDirectoryInfo>(), Mock.Of<IMetaDataStorage>());
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorThrowsExceptionIfStorageIsNull()
        {
            new DescendantsCrawler(Mock.Of<ISyncEventQueue>(), Mock.Of<IFolder>(), Mock.Of<IDirectoryInfo>(), null);
        }

        [Test, Category("Fast")]
        public void ConstructorWorksWithoutFsInfoFactory()
        {
            new DescendantsCrawler(Mock.Of<ISyncEventQueue>(), Mock.Of<IFolder>(), Mock.Of<IDirectoryInfo>(), Mock.Of<IMetaDataStorage>());
        }

        [Test, Category("Fast")]
        public void ConstructorTakesFsInfoFactory()
        {
            new DescendantsCrawler(Mock.Of<ISyncEventQueue>(), Mock.Of<IFolder>(), Mock.Of<IDirectoryInfo>(), Mock.Of<IMetaDataStorage>(), Mock.Of<IFileSystemInfoFactory>());
        }

        [Test, Category("Fast")]
        public void PriorityIsNormal()
        {
            var crawler = new DescendantsCrawler(Mock.Of<ISyncEventQueue>(), Mock.Of<IFolder>(), Mock.Of<IDirectoryInfo>(), Mock.Of<IMetaDataStorage>());
            Assert.That(crawler.Priority == EventHandlerPriorities.NORMAL);
        }
    }
}