//-----------------------------------------------------------------------
// <copyright file="SymlinkFilterTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.FilterTests {
    using System;

    using CmisSync.Lib.Filter;
    using CmisSync.Lib.Storage.FileSystem;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class SymlinkFilterTest {
        [Test, Category("Fast")]
        public void DetectSymlinksCorrectly([Values(true, false)]bool exists, [Values(true, false)]bool isSymlink) {
            var underTest = new SymlinkFilter();
            string path = "path";
            string reason;
            var fileInfo = new Mock<IFileSystemInfo>(MockBehavior.Strict);
            fileInfo.Setup(f => f.Exists).Returns(exists);
            fileInfo.Setup(f => f.IsSymlink).Returns(isSymlink);
            fileInfo.Setup(f => f.FullName).Returns(path);
            var result = underTest.IsSymlink(fileInfo.Object, out reason);
            Assert.That(result, Is.EqualTo(exists && isSymlink));
            Assert.That(reason, Is.Not.Null);
            if (result) {
                Assert.That(reason, Is.StringContaining(path));
                fileInfo.Verify(f => f.FullName, Times.Once());
            } else {
                Assert.That(reason, Is.EqualTo(string.Empty));
                fileInfo.Verify(f => f.FullName, Times.Never());
            }

            fileInfo.Verify(f => f.Exists, Times.AtMostOnce());
            fileInfo.Verify(f => f.IsSymlink, Times.AtMostOnce());
        }
    }
}