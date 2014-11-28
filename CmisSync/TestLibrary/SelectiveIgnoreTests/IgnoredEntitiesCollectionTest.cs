//-----------------------------------------------------------------------
// <copyright file="IgnoredEntitiesCollectionTest.cs" company="GRAU DATA AG">
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
using System.Collections.Generic;

namespace TestLibrary.SelectiveIgnoreTests
{
    using System;
    using System.IO;

    using CmisSync.Lib.SelectiveIgnore;

    using DotCMIS.Client;

    using Moq;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture]
    public class IgnoredEntitiesCollectionTest
    {
        private string objectId;
        private string localPath;

        [SetUp]
        public void Init() {
            this.objectId = Guid.NewGuid().ToString();
            this.localPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        }

        [Test, Category("Fast"), Category("SelectiveIgnore")]
        public void AddElement() {
            var underTest = new IgnoredEntitiesCollection();

            underTest.Add(Mock.Of<IIgnoredEntity>(o => o.ObjectId == this.objectId && o.LocalPath == this.localPath));

            Assert.That(underTest.IsIgnoredId(objectId), Is.EqualTo(IgnoredState.IGNORED));
        }

        [Test, Category("Fast"), Category("SelectiveIgnore")]
        public void IgnoreCheckOnLocalPath() {
            var underTest = new IgnoredEntitiesCollection();

            underTest.Add(Mock.Of<IIgnoredEntity>(o => o.ObjectId == this.objectId && o.LocalPath == this.localPath));

            Assert.That(underTest.IsIgnoredPath(this.localPath), Is.EqualTo(IgnoredState.IGNORED));
        }

        [Test, Category("Fast"), Category("SelectiveIgnore")]
        public void IgnoreInheritedCheckOnLocalPath() {
            var underTest = new IgnoredEntitiesCollection();

            underTest.Add(Mock.Of<IIgnoredEntity>(o => o.ObjectId == this.objectId && o.LocalPath == this.localPath));

            Assert.That(underTest.IsIgnoredPath(Path.Combine(this.localPath, Guid.NewGuid().ToString())), Is.EqualTo(IgnoredState.INHERITED));
        }

        [Test, Category("Fast"), Category("SelectiveIgnore")]
        public void RemoveElement() {
            var underTest = new IgnoredEntitiesCollection();
            Assert.That(underTest.IsIgnoredId(this.objectId), Is.EqualTo(IgnoredState.NOT_IGNORED));

            underTest.Add(Mock.Of<IIgnoredEntity>(o => o.ObjectId == this.objectId && o.LocalPath == this.localPath));
            underTest.Remove(Mock.Of<IIgnoredEntity>(o => o.ObjectId == this.objectId && o.LocalPath == this.localPath));

            Assert.That(underTest.IsIgnoredId(this.objectId), Is.EqualTo(IgnoredState.NOT_IGNORED));
        }

        [Test, Category("Fast"), Category("SelectiveIgnore")]
        public void RemoveElementById() {
            var underTest = new IgnoredEntitiesCollection();
            Assert.That(underTest.IsIgnoredId(this.objectId), Is.EqualTo(IgnoredState.NOT_IGNORED));

            underTest.Add(Mock.Of<IIgnoredEntity>(o => o.ObjectId == this.objectId && o.LocalPath == this.localPath));
            underTest.Remove(this.objectId);

            Assert.That(underTest.IsIgnoredId(this.objectId), Is.EqualTo(IgnoredState.NOT_IGNORED));
        }

        [Test, Category("Fast"), Category("SelectiveIgnore")]
        public void IgnoreCheckOfFolder() {
            var underTest = new IgnoredEntitiesCollection();

            underTest.Add(Mock.Of<IIgnoredEntity>(o => o.ObjectId == this.objectId && o.LocalPath == this.localPath));

            Assert.That(underTest.IsIgnored(Mock.Of<IFolder>(f => f.Id == this.objectId)), Is.EqualTo(IgnoredState.IGNORED));
        }

        [Test, Category("Fast"), Category("SelectiveIgnore")]
        public void IgnoreCheckOfSubFolder() {
            var underTest = new IgnoredEntitiesCollection();
            var folder = new Mock<IFolder>();
            folder.Setup(f => f.Id).Returns(Guid.NewGuid().ToString());
            var parent = Mock.Of<IFolder>(f => f.Id == this.objectId);
            folder.Setup(f => f.FolderParent).Returns(parent);
            underTest.Add(Mock.Of<IIgnoredEntity>(o => o.ObjectId == this.objectId && o.LocalPath == this.localPath));

            Assert.That(underTest.IsIgnored(folder.Object), Is.EqualTo(IgnoredState.INHERITED));
        }

        [Test, Category("Fast"), Category("SelectiveIgnore")]
        public void IgnoreCheckOfDocument() {
            var underTest = new IgnoredEntitiesCollection();

            underTest.Add(Mock.Of<IIgnoredEntity>(o => o.ObjectId == this.objectId && o.LocalPath == this.localPath));

            Assert.That(underTest.IsIgnored(Mock.Of<IDocument>(f => f.Id == this.objectId)), Is.EqualTo(IgnoredState.IGNORED));
        }

        [Test, Category("Fast"), Category("SelectiveIgnore")]
        public void IgnoreCheckOfSubDocument() {
            var underTest = new IgnoredEntitiesCollection();
            var doc = new Mock<IDocument>();
            doc.Setup(f => f.Id).Returns(Guid.NewGuid().ToString());
            var parents = new List<IFolder>();
            parents.Add(Mock.Of<IFolder>(o => o.Id == this.objectId));
            doc.Setup(f => f.Parents).Returns(parents);
            underTest.Add(Mock.Of<IIgnoredEntity>(o => o.ObjectId == this.objectId && o.LocalPath == this.localPath));

            Assert.That(underTest.IsIgnored(doc.Object), Is.EqualTo(IgnoredState.INHERITED));
        }
    }
}