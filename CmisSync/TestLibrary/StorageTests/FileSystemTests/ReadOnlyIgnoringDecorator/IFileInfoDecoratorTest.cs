//-----------------------------------------------------------------------
// <copyright file="IFileInfoDecoratorTest.cs" company="GRAU DATA AG">
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
#if !__MonoCS__
    using FileInfo = Alphaleonis.Win32.Filesystem.FileInfo;
    using DirectoryInfo = Alphaleonis.Win32.Filesystem.DirectoryInfo;
#endif

    using CmisSync.Lib.Storage.FileSystem;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class IFileInfoDecoratorTest {
        [Test, Category("Fast")]
        public void Delete(
            [Values(true, false)]bool isItselfReadOnly,
            [Values(true, false)]bool isParentReadOnly)
        {
            var parentDir = new Mock<IDirectoryInfo>(MockBehavior.Strict);
            parentDir.SetupProperty(d => d.ReadOnly, isParentReadOnly);
            var decoratedFile = new Mock<IFileInfo>(MockBehavior.Strict);
            decoratedFile.SetupProperty(f => f.ReadOnly, isItselfReadOnly);
            decoratedFile.Setup(f => f.Directory).Returns(parentDir.Object);
            decoratedFile.Setup(f => f.Delete()).Callback(() => {
                Assert.That(decoratedFile.Object.ReadOnly, Is.False);
                Assert.That(parentDir.Object.ReadOnly, Is.False);
            });

            var underTest = new ReadOnlyIgnoringFileInfoDecorator(decoratedFile.Object);
            underTest.Delete();

            decoratedFile.VerifySet(f => f.ReadOnly = false, isItselfReadOnly ? Times.Once() : Times.Never());
            parentDir.VerifySet(d => d.ReadOnly = false, isParentReadOnly ? Times.Once() : Times.Never());
            parentDir.VerifySet(d => d.ReadOnly = true, isParentReadOnly ? Times.Once() : Times.Never());
            Assert.That(parentDir.Object.ReadOnly, Is.EqualTo(isParentReadOnly));
        }

        [Test, Category("Fast")]
        public void Replace(
            [Values(true, false)]bool isItselfReadOnly,
            [Values(true, false)]bool isDestinationReadOnly,
            [Values(true, false)]bool ignoreMetaDataErrors,
            [Values(true, false)]bool isBackupFileReadOnly,
            [Values(true, false)]bool isBackupFileNull)
        {
            var destinationFile = new Mock<IFileInfo>(MockBehavior.Strict);
            destinationFile.SetupProperty(f => f.ReadOnly, isDestinationReadOnly);
            destinationFile.Setup(f => f.Exists).Returns(true);
            var backupFile = new Mock<IFileInfo>(MockBehavior.Strict);
            backupFile.SetupProperty(f => f.ReadOnly, isBackupFileReadOnly);
            var decoratedFile = new Mock<IFileInfo>(MockBehavior.Strict);
            decoratedFile.SetupProperty(f => f.ReadOnly, isItselfReadOnly);
            decoratedFile.Setup(f => f.Replace(destinationFile.Object, backupFile.Object, ignoreMetaDataErrors)).Returns(decoratedFile.Object);
            decoratedFile.Setup(f => f.Replace(destinationFile.Object, null, ignoreMetaDataErrors)).Returns(decoratedFile.Object);

            var underTest = new ReadOnlyIgnoringFileInfoDecorator(decoratedFile.Object);
            var result = underTest.Replace(destinationFile.Object, isBackupFileNull ? null : backupFile.Object, ignoreMetaDataErrors);

            Assert.That(result.ReadOnly, Is.EqualTo(isItselfReadOnly));
            Assert.That(underTest.ReadOnly, Is.EqualTo(isItselfReadOnly));
            Assert.That(destinationFile.Object.ReadOnly, Is.False);
            if (!isBackupFileNull) {
                Assert.That(backupFile.Object.ReadOnly, Is.EqualTo(isDestinationReadOnly));
            }
        }

        [Test, Category("Medium")]
        public void MoveTo(
            [Values(true, false)]bool sourceParentIsReadOnly,
            [Values(true, false)]bool targetParentIsReadOnly,
            [Values(true, false)]bool isItselfReadOnly)
        {
            string oldName = "oldFile";
            string newName = "newFile";
            var sourceParent = new DirectoryInfoWrapper(new DirectoryInfo(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())));
            var targetParent = new DirectoryInfoWrapper(new DirectoryInfo(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())));
            var decoratedFileInfo = new FileInfoWrapper(new FileInfo(Path.Combine(sourceParent.FullName, oldName)));

            try {
                sourceParent.Create();
                targetParent.Create();

                var underTest = new ReadOnlyIgnoringFileInfoDecorator(decoratedFileInfo);
                using (underTest.Open(FileMode.CreateNew));
                sourceParent.ReadOnly = sourceParentIsReadOnly;
                targetParent.ReadOnly = targetParentIsReadOnly;
                underTest.ReadOnly = isItselfReadOnly;
                underTest.MoveTo(Path.Combine(targetParent.FullName, newName));

                Assert.That(underTest.ReadOnly, Is.EqualTo(isItselfReadOnly));
                Assert.That(sourceParent.ReadOnly, Is.EqualTo(sourceParentIsReadOnly));
                Assert.That(targetParent.ReadOnly, Is.EqualTo(targetParentIsReadOnly));
            } finally {
                decoratedFileInfo.Refresh();
                sourceParent.Refresh();
                targetParent.Refresh();
                if (decoratedFileInfo.Exists) {
                    decoratedFileInfo.ReadOnly = false;
                }

                if (sourceParent.Exists) {
                    sourceParent.ReadOnly = false;
                    sourceParent.Delete(true);
                }

                if (targetParent.Exists) {
                    targetParent.ReadOnly = false;
                    targetParent.Delete(true);
                }
            }
        }

        [Test, Category("Fast")]
        public void ToStringReturnsWrappedToString() {
            string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var hiddenFileInfo = new FileInfoWrapper(new FileInfo(path));
            var underTest = new ReadOnlyIgnoringFileInfoDecorator(hiddenFileInfo);
            Assert.That(underTest.ToString(), Is.EqualTo(path));
        }
    }
}