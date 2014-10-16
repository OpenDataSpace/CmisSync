//-----------------------------------------------------------------------
// <copyright file="IgnoredEntityTests.cs" company="GRAU DATA AG">
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

namespace TestLibrary.SelectiveIgnoreTests
{
    using System;
    using System.IO;

    using CmisSync.Lib.PathMatcher;
    using CmisSync.Lib.SelectiveIgnore;

    using DotCMIS.Client;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class IgnoredEntityTest
    {
        [Test, Category("Fast"), Category("SelectiveIgnore")]
        public void ConstructorTakesFolderAndMatcher() {
            string localPath = Path.Combine(Path.GetTempPath(), "path");
            var folder = Mock.Of<IFolder>(f => f.Id == "folderId");
            var matcher = new Mock<IPathMatcher>();
            matcher.Setup(m => m.CanCreateLocalPath(folder)).Returns(true);
            matcher.Setup(m => m.CreateLocalPath(folder)).Returns(localPath);

            var underTest = new IgnoredEntity(folder, matcher.Object);

            Assert.That(underTest.ObjectId, Is.EqualTo(folder.Id));
            Assert.That(underTest.LocalPath, Is.EqualTo(localPath));
        }

        [Test, Category("Fast"), Category("SelectiveIgnore")]
        public void ConstructorFailsIfMatcherCannotCreateLocalPath() {
            var folder = Mock.Of<IFolder>(f => f.Id == "folderId");
            var matcher = new Mock<IPathMatcher>();
            matcher.Setup(m => m.CanCreateLocalPath(folder)).Returns(false);

            Assert.Throws<ArgumentException>(() => new IgnoredEntity(folder, matcher.Object));
        }

        [Test, Category("Fast"), Category("SelectiveIgnore")]
        public void ConstructorFailsIfFolderIsNull() {
            Assert.Throws<ArgumentNullException>(() => new IgnoredEntity(null, Mock.Of<IPathMatcher>()));
        }

        [Test, Category("Fast"), Category("SelectiveIgnore")]
        public void ConstructorFailsIfMatcherIsNull() {
            Assert.Throws<ArgumentNullException>(() => new IgnoredEntity(Mock.Of<IFolder>(), null));
        }
    }
}