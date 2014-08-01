//-----------------------------------------------------------------------
// <copyright file="IFileConvenienceExtendersTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.StorageTests.DataBaseTests.EntitiesTests
{
    using System;
    using System.IO;
    using System.Security.Cryptography;

    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;

    using Moq;

    using NUnit.Framework;

    using TestUtils;

    [TestFixture]
    public class IFileConvenienceExtendersTest
    {
        private static readonly string fileName = "fileName";
        private static readonly string fullName = Path.Combine(Path.GetTempPath(), fileName);
        private Mock<IFileInfo> fileInfo;
        private int length = 1024;
        private byte[] content;
        private byte[] othercontent;
        private DateTime modificationDate;
        private byte[] expectedHash;

        [SetUp]
        public void SetUp() {
            this.content = new byte[length];
            this.othercontent = new byte[length];
            using (RandomNumberGenerator random = RandomNumberGenerator.Create()) {
                random.GetBytes(this.content);
            }

            this.expectedHash = SHA1.Create().ComputeHash(this.content);

            this.modificationDate = DateTime.UtcNow;

            this.fileInfo = new Mock<IFileInfo>();
            this.fileInfo.Setup(f => f.Name).Returns(fileName);
            this.fileInfo.Setup(f => f.FullName).Returns(fullName);
            this.fileInfo.SetupLastWriteTimeUtc(this.modificationDate);
            this.fileInfo.Setup(f => f.Length).Returns(length);
            this.fileInfo.Setup(f => f.Exists).Returns(true);
        }

        [Test, Category("Fast")]
        public void ContentCheckReturnsTrueIfContentSizeIsDifferent() {
            var obj = Mock.Of<IMappedObject>(
                o =>
                o.LastContentSize == this.length + 1);
            fileInfo.Object.IsContentChangedTo(obj);
        }

        [Test, Category("Fast")]
        public void ContentCheckThrowsExceptionIfGivenObjectIsNull() {
            Assert.Throws<ArgumentNullException>(() => fileInfo.Object.IsContentChangedTo(null));
        }

        [Test, Category("Fast")]
        public void ContentCheckThrowsExceptionIfOwnFileDoesNotExists() {
            this.fileInfo.Setup(f => f.Exists).Returns(false);
            Assert.Throws<FileNotFoundException>(() => fileInfo.Object.IsContentChangedTo(Mock.Of<IMappedObject>()));
        }

        [Test, Category("Fast")]
        public void ContentCheckThrowsExceptionIfLastContentSizeIsNegative() {
            var obj = Mock.Of<IMappedObject>(
                o =>
                o.LastContentSize == -1);
            Assert.Throws<ArgumentOutOfRangeException>(() => fileInfo.Object.IsContentChangedTo(obj));
        }

        [Test, Category("Fast")]
        public void ContentCheckReturnTrueIfLastContentHashIsNull() {
            var obj = Mock.Of<IMappedObject>(
                o =>
                o.LastContentSize == length && o.LastChecksum == (byte[])null);
            Assert.That(fileInfo.Object.IsContentChangedTo(obj), Is.True);
        }

        [Test, Category("Fast")]
        public void ContentCheckReturnsFalseIfLengthAndModificationDateAreEqualAndModificationDateIsHintForChange() {
            var obj = Mock.Of<IMappedObject>(
                o =>
                o.LastContentSize == length &&
                o.LastChecksum == this.expectedHash &&
                o.LastLocalWriteTimeUtc == this.modificationDate);
            this.fileInfo.SetupLastWriteTimeUtc(this.modificationDate);
            using (var contentStream = new MemoryStream(this.content)) {
                Assert.That(this.fileInfo.Object.IsContentChangedTo(obj, true), Is.False);
            }
        }

        [Test, Category("Fast")]
        public void ContentCheckReturnsTrueIfLengthAndModificationDateAreNotEqualAndModificationDateIsHintForChange() {
            var obj = Mock.Of<IMappedObject>(
                o =>
                o.LastContentSize == length &&
                o.LastChecksum == this.expectedHash &&
                o.LastLocalWriteTimeUtc == this.modificationDate);
            this.fileInfo.SetupLastWriteTimeUtc(this.modificationDate.AddDays(1));
            using (var contentStream = new MemoryStream(this.othercontent)) {
                this.fileInfo.Setup(f => f.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete)).Returns(contentStream);
                Assert.That(this.fileInfo.Object.IsContentChangedTo(obj, true), Is.True);
            }
        }

        [Test, Category("Fast")]
        public void ContentCheckReturnsFalseIfContentIsEqual() {
            var obj = Mock.Of<IMappedObject>(
                o =>
                o.LastContentSize == length &&
                o.LastChecksum == this.expectedHash &&
                o.LastLocalWriteTimeUtc == this.modificationDate);
            this.fileInfo.SetupLastWriteTimeUtc(this.modificationDate.AddDays(1));
            using (var contentStream = new MemoryStream(this.content)) {
                this.fileInfo.Setup(f => f.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete)).Returns(contentStream);
                Assert.That(this.fileInfo.Object.IsContentChangedTo(obj), Is.False);
            }
        }

        [Test, Category("Fast")]
        public void ContentCheckReturnTrueIfContentIsDifferentButModificationDateIsEqual() {
            var obj = Mock.Of<IMappedObject>(
                o =>
                o.LastContentSize == length &&
                o.LastChecksum == this.expectedHash &&
                o.LastLocalWriteTimeUtc == this.modificationDate);
            this.fileInfo.SetupLastWriteTimeUtc(this.modificationDate);
            using (var contentStream = new MemoryStream(this.othercontent)) {
                this.fileInfo.Setup(f => f.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete)).Returns(contentStream);
                Assert.That(this.fileInfo.Object.IsContentChangedTo(obj), Is.True);
            }
        }
    }
}