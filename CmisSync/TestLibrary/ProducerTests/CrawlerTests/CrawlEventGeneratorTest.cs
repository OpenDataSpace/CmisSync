//-----------------------------------------------------------------------
// <copyright file="CrawlEventGeneratorTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.ProducerTests.CrawlerTests {
    using System;

    using CmisSync.Lib.Producer.Crawler;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.FileSystem;

    using Moq;

    using NUnit.Framework;

    using TestUtils;

    [TestFixture]
    public class CrawlEventGeneratorTest {
        private CrawlEventGenerator underTest;
        private Mock<IMetaDataStorage> storage;
        private Mock<IFileSystemInfoFactory> fsFactory;

        [SetUp]
        public void SetUp() {
            this.storage = new Mock<IMetaDataStorage>();
            this.fsFactory = new Mock<IFileSystemInfoFactory>();
            this.underTest = new CrawlEventGenerator(this.storage.Object, this.fsFactory.Object);
        }

        [Test, Category("Fast")]
        public void ConstructorTakesStorage() {
            new CrawlEventGenerator(Mock.Of<IMetaDataStorage>());
        }

        [Test, Category("Fast")]
        public void ContructorTakesStorageAndFsFactory() {
            new CrawlEventGenerator(Mock.Of<IMetaDataStorage>(), Mock.Of<IFileSystemInfoFactory>());
        }

        [Test, Category("Fast")]
        public void ConstructorFailsIfStorageIsNull() {
            Assert.Throws<ArgumentNullException>(() => new CrawlEventGenerator(null));
        }
    }
}