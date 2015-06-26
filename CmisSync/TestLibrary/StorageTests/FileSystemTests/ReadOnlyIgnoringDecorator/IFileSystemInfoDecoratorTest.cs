using System.Security;


namespace TestLibrary.StorageTests.FileSystemTests.ReadOnlyIgnoringDecorator {
    using System;
    using System.IO;

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