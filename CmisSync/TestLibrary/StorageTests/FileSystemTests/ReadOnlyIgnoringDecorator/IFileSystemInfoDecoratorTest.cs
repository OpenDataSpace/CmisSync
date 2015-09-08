//-----------------------------------------------------------------------
// <copyright file="IFileSystemInfoDecoratorTest.cs" company="GRAU DATA AG">
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
﻿
namespace TestLibrary.StorageTests.FileSystemTests.ReadOnlyIgnoringDecorator {
    using System;
    using System.IO;
    using System.Security;
#if !__MonoCS__
    using FileInfo = Alphaleonis.Win32.Filesystem.FileInfo;
    using DirectoryInfo = Alphaleonis.Win32.Filesystem.DirectoryInfo;
#endif
    using CmisSync.Lib.Storage.FileSystem;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class IFileSystemInfoDecoratorTest {
        #region ReadAccess
        [Test, Category("Fast")]
        public void AbstractConstructorThrowsExceptionIfInputIsNull() {
            Assert.Throws<ArgumentNullException>(() => {
                new ReadOnlyIgnoringFileSystemInfoDecoratorImpl(null);
            });
        }

        [Test, Category("Fast")]
        public void GetFullName() {
            string fullName = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var fileSystemInfo = new Mock<IFileSystemInfo>(MockBehavior.Strict);
            fileSystemInfo.SetupGet(f => f.FullName).Returns(fullName);
            var underTest = new ReadOnlyIgnoringFileSystemInfoDecoratorImpl(fileSystemInfo.Object);

            Assert.That(underTest.FullName, Is.EqualTo(fullName));
            fileSystemInfo.VerifyGet(f => f.FullName, Times.Once());
        }

        [Test, Category("Fast")]
        public void GetName() {
            string name = Guid.NewGuid().ToString();
            var fileSystemInfo = new Mock<IFileSystemInfo>(MockBehavior.Strict);
            fileSystemInfo.SetupGet(f => f.Name).Returns(name);
            var underTest = new ReadOnlyIgnoringFileSystemInfoDecoratorImpl(fileSystemInfo.Object);

            Assert.That(underTest.Name, Is.EqualTo(name));
            fileSystemInfo.VerifyGet(f => f.Name, Times.Once());
        }

        [Test, Category("Fast")]
        public void DoesExists([Values(true, false)]bool exists) {
            var fileSystemInfo = new Mock<IFileSystemInfo>(MockBehavior.Strict);
            fileSystemInfo.SetupGet(f => f.Exists).Returns(exists);
            var underTest = new ReadOnlyIgnoringFileSystemInfoDecoratorImpl(fileSystemInfo.Object);

            Assert.That(underTest.Exists, Is.EqualTo(exists));
            fileSystemInfo.VerifyGet(f => f.Exists, Times.Once());
        }

        [Test, Category("Fast")]
        public void DoesSupportExtendedAttributes(
            [Values("", "value")]string extendedAttribute)
        {
            var fileSystemInfo = new Mock<IFileSystemInfo>(MockBehavior.Strict);
            var key = Guid.NewGuid().ToString();
            fileSystemInfo.Setup(f => f.GetExtendedAttribute(key)).Returns(extendedAttribute);
            var underTest = new ReadOnlyIgnoringFileSystemInfoDecoratorImpl(fileSystemInfo.Object);

            Assert.That(underTest.GetExtendedAttribute(key), Is.EqualTo(extendedAttribute));
            fileSystemInfo.Verify(f => f.GetExtendedAttribute(key), Times.Once());
            fileSystemInfo.Verify(f => f.GetExtendedAttribute(It.Is<string>(s => s != key)), Times.Never());
        }

        [Test, Category("Fast")]
        public void DoesSupportExtendedAttributes([Values(true, false)]bool supportsExtendendAttributes) {
            var fileSystemInfo = new Mock<IFileSystemInfo>(MockBehavior.Strict);
            fileSystemInfo.Setup(f => f.IsExtendedAttributeAvailable()).Returns(supportsExtendendAttributes);
            var underTest = new ReadOnlyIgnoringFileSystemInfoDecoratorImpl(fileSystemInfo.Object);

            Assert.That(underTest.IsExtendedAttributeAvailable(), Is.EqualTo(supportsExtendendAttributes));
            fileSystemInfo.Verify(f => f.IsExtendedAttributeAvailable(), Times.Once());
        }

        [Test, Category("Fast")]
        public void ToStringReturnsWrappedToString([Values(true, false)]bool isFile) {
            string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            IFileSystemInfo hiddenInfo = isFile ? (IFileSystemInfo)new FileInfoWrapper(new FileInfo(path)) : (IFileSystemInfo)new DirectoryInfoWrapper(new DirectoryInfo(path));
            var underTest = new ReadOnlyIgnoringFileSystemInfoDecoratorImpl(hiddenInfo);
            Assert.That(underTest.ToString(), Is.EqualTo(path));
        }
        #endregion

        #region ReadWriteAccess
        [Test, Category("Fast")]
        public void NonExistingFileInfoCannotBeModified() {
            var fileSystemInfo = this.CreateMock();
            fileSystemInfo.SetupGet(f => f.ReadOnly).Throws<FileNotFoundException>();

            var underTest = new ReadOnlyIgnoringFileSystemInfoDecoratorImpl(fileSystemInfo.Object);

            Assert.Throws<FileNotFoundException>(() => underTest.InnerDisableAndEnableReadOnlyForOperation(() => {
                Assert.Fail("This method should never be called on a non existing file/folder");
            }));
            fileSystemInfo.Verify(f => f.Refresh(), Times.Once());
        }

        [Test, Category("Fast")]
        public void DoNotModifyReadOnlyAttributesIfReadWritePermissionsAreSet() {
            var fileSystemInfo = this.CreateMock();
            fileSystemInfo.SetupGet(f => f.ReadOnly).Returns(true);
            fileSystemInfo.SetupSet(f => f.ReadOnly = false).Throws<SecurityException>();

            var underTest = new ReadOnlyIgnoringFileSystemInfoDecoratorImpl(fileSystemInfo.Object);

            Assert.Throws<SecurityException>(() => underTest.InnerDisableAndEnableReadOnlyForOperation(() => {
                Assert.Fail("This method should never be called on a readOnly file/folder");
            }));
            fileSystemInfo.Verify(f => f.Refresh(), Times.Once());
        }

        [Test, Category("Fast")]
        public void ModifyReadOnlyIfNeededForOperation() {
            var fileSystemInfo = this.CreateMock();
            fileSystemInfo.SetupProperty(f => f.ReadOnly, true);

            var underTest = new ReadOnlyIgnoringFileSystemInfoDecoratorImpl(fileSystemInfo.Object);

            underTest.InnerDisableAndEnableReadOnlyForOperation(() => {
                fileSystemInfo.VerifySet(f => f.ReadOnly = false, Times.Once());
                Assert.That(fileSystemInfo.Object.ReadOnly, Is.False);
            });
            fileSystemInfo.VerifySet(f => f.ReadOnly = true, Times.Once());
            fileSystemInfo.Verify(f => f.Refresh(), Times.Once());
        }

        [Test, Category("Fast")]
        public void IgnoreReadOnlyPropertyIfItIsFalse() {
            var fileSystemInfo = this.CreateMock();
            fileSystemInfo.SetupProperty(f => f.ReadOnly, false);

            var underTest = new ReadOnlyIgnoringFileSystemInfoDecoratorImpl(fileSystemInfo.Object);

            underTest.InnerDisableAndEnableReadOnlyForOperation(() => {
                fileSystemInfo.VerifySet(f => f.ReadOnly = false, Times.Never());
                Assert.That(fileSystemInfo.Object.ReadOnly, Is.False);
            });
            fileSystemInfo.VerifySet(f => f.ReadOnly = It.IsAny<bool>(), Times.Never());
            fileSystemInfo.Verify(f => f.Refresh(), Times.Once());
        }

        [Test, Category("Fast")]
        public void TryToResetTheReadOnlyPropertyAfterFailingWriteOperation() {
            var fileSystemInfo = this.CreateMock();
            fileSystemInfo.SetupProperty(f => f.ReadOnly, true);

            var underTest = new ReadOnlyIgnoringFileSystemInfoDecoratorImpl(fileSystemInfo.Object);

            Assert.Throws<IOException>(() => underTest.InnerDisableAndEnableReadOnlyForOperation(() => {
                fileSystemInfo.VerifySet(f => f.ReadOnly = false, Times.Once());
                Assert.That(fileSystemInfo.Object.ReadOnly, Is.False);
                throw new IOException("Generic exception to simulate that write operation failed");
            }));
            fileSystemInfo.VerifySet(f => f.ReadOnly = true, Times.Once());
            fileSystemInfo.Verify(f => f.Refresh(), Times.Once());
        }

        private Mock<IFileSystemInfo> CreateMock() {
            var fileSystemInfo = new Mock<IFileSystemInfo>(MockBehavior.Strict);
            fileSystemInfo.Setup(f => f.Refresh());
            return fileSystemInfo;
        }
        #endregion

        #region ImplementationOfAbstractClass
        private class ReadOnlyIgnoringFileSystemInfoDecoratorImpl : ReadOnlyIgnoringFileSystemInfoDecorator {
            public ReadOnlyIgnoringFileSystemInfoDecoratorImpl(IFileSystemInfo info) : base(info) {
            }

            public void InnerDisableAndEnableReadOnlyForOperation(Action writeOperation) {
                this.DisableAndEnableReadOnlyForOperation(writeOperation);
            }
        }
        #endregion
    }
}