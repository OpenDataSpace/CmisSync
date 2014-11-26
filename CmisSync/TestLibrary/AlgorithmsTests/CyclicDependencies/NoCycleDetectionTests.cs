//-----------------------------------------------------------------------
// <copyright file="NoCycleDetectionTests.cs" company="GRAU DATA AG">
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

namespace TestLibrary.AlgorithmsTests.CyclicDependenciesTests
{
    using System;
    using System.Collections.Generic;

    using CmisSync.Lib.Algorithms.CyclicDependencies;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Producer.Crawler;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class NoCycleDetectionTests
    {
        private CycleDetector underTest;
        private Mock<IMetaDataStorage> storage;

        [SetUp]
        public void Init() {
            this.storage = new Mock<IMetaDataStorage>();
            this.underTest = new CycleDetector(this.storage.Object);
        }

        /// <summary>
        /// Linear renames: A → B → C
        /// </summary>
        [Test, Category("Fast")]
        public void LinearRenames() {
            string objectId = Guid.NewGuid().ToString();
            var collection = new CrawlEventCollection();
            collection.mergableEvents = new Dictionary<string, Tuple<AbstractFolderEvent, AbstractFolderEvent>>();
            var doc1 = Mock.Of<IFileInfo>(d => d.Name == "C" && d.Uuid == Guid.NewGuid());
            var doc2 = Mock.Of<IDocument>(d => d.Name == "B" && d.Id == objectId);
            FileEvent event1 = new FileEvent(doc1, null);
            FileEvent event2 = new FileEvent(null, doc2);
            collection.mergableEvents.Add(objectId, new Tuple<AbstractFolderEvent, AbstractFolderEvent>(event1, event2));
            Assert.That(underTest.Detect(collection), Is.Empty);
        }

        [Test, Category("Fast")]
        public void NoRenameOrMove() {
            Assert.That(underTest.Detect(new CrawlEventCollection()), Is.Empty);
        }
    }
}