

namespace TestLibrary.StorageTests.FileSystemTests {
    using System;
    using System.IO;

    using NUnit.Framework;
    using CmisSync.Lib.Storage.FileSystem;
    using TestLibrary.TestUtils;

    [TestFixture]
    public class DirectoryInfoWrapperTest {
        [Test, Category("Medium")]
        public void CanMoveRenameDeleteFlag([Values(true, false)]bool parentIsReadOnly, [Values(true, false)]bool folderIsReadOnly) {
            EnsureThisRunsOnNTFS();
            var parent = new DirectoryInfoWrapper(new DirectoryInfo(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())));
            var underTest = new DirectoryInfoWrapper(new DirectoryInfo(Path.Combine(parent.FullName, "testFolder")));
            try {
                parent.Create();
                underTest.Create();
                parent.ReadOnly = parentIsReadOnly;
                underTest.ReadOnly = folderIsReadOnly;
                Assert.That(parent.CanMoveOrRenameOrDelete, Is.True);
                Assert.That(underTest.CanMoveOrRenameOrDelete, Is.True);
                underTest.CanMoveOrRenameOrDelete = false;
                Assert.That(underTest.CanMoveOrRenameOrDelete, Is.False);
                Assert.That(parent.CanMoveOrRenameOrDelete, Is.True);
                Assert.That(underTest.ReadOnly, Is.EqualTo(folderIsReadOnly));
                Assert.That(parent.ReadOnly, Is.EqualTo(parentIsReadOnly));
                underTest.CanMoveOrRenameOrDelete = true;
                Assert.That(underTest.CanMoveOrRenameOrDelete, Is.True);
                Assert.That(parent.CanMoveOrRenameOrDelete, Is.True);
                Assert.That(underTest.ReadOnly, Is.EqualTo(folderIsReadOnly));
                Assert.That(parent.ReadOnly, Is.EqualTo(parentIsReadOnly));
            } finally {
                if (underTest.Exists) {
                    underTest.CanMoveOrRenameOrDelete = true;
                    underTest.ReadOnly = false;
                    underTest.Delete(true);
                }
                if (parent.Exists) {
                    parent.ReadOnly = false;
                    parent.Delete(false);
                }
            }
        }

        [Test, Category("Medium")]
        public void CanMoveRenameDeleteFlagIsNotChangedByChangingReadOnlyFlag() {
            EnsureThisRunsOnNTFS();
        }

        private void EnsureThisRunsOnNTFS() {
            var driveFormat = new DriveInfo(new DirectoryInfo(Path.GetTempPath()).Root.Name).DriveFormat;
            if (!driveFormat.Equals("NTFS", StringComparison.OrdinalIgnoreCase)) {
                Assert.Ignore(string.Format("Actual FS is not NTFS: {}", driveFormat));
            }
        }
    }
}
