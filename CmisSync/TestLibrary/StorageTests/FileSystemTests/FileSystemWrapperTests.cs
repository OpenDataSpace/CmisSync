//-----------------------------------------------------------------------
// <copyright file="FileSystemWrapperTests.cs" company="GRAU DATA AG">
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

namespace TestLibrary.StorageTests.FileSystemTests {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    using CmisSync.Lib.Storage.FileSystem;

    using Moq;

    using NUnit.Framework;

    using TestLibrary.ProducerTests.WatcherTests;

    [TestFixture]
    public class FileSystemWrapperTests {
        private static readonly IFileSystemInfoFactory Factory = new FileSystemInfoFactory();
        private DirectoryInfo testFolder;
        private DirectoryInfo testFolderOnOtherFS = null;

        [SetUp]
        public void Init() {
            string tempPath = Path.GetTempPath();
            var tempFolder = new DirectoryInfo(tempPath);
            Assert.That(tempFolder.Exists, Is.True);
            this.testFolder = tempFolder.CreateSubdirectory("DSSFileSystemWrapperTest");
            this.testFolder.Attributes &= ~FileAttributes.ReadOnly;
        }

        [TearDown]
        public void Cleanup() {
            this.RemoveReadOnlyFlagRecursive(this.testFolder);
            this.testFolder.Delete(true);
            if (this.testFolderOnOtherFS != null) {
                this.testFolderOnOtherFS.Refresh();
                if (this.testFolderOnOtherFS.Exists) {
                    this.testFolderOnOtherFS.Delete(true);
                }

                this.testFolderOnOtherFS = null;
            }
        }

        [Test, Category("Medium")]
        public void FileInfoConstruction() {
            string fileName = "test1";
            string fullPath = Path.Combine(this.testFolder.FullName, fileName);
            IFileInfo fileInfo = Factory.CreateFileInfo(fullPath);
            Assert.That(fileInfo, Is.Not.Null);
        }

        [Test, Category("Medium")]
        public void DirectoryInfoConstruction() {
            string fileName = "test1";
            string fullPath = Path.Combine(this.testFolder.FullName, fileName);
            IDirectoryInfo fileInfo = Factory.CreateDirectoryInfo(fullPath);
            Assert.That(fileInfo, Is.Not.Null);
        }

        [Test, Category("Medium")]
        public void FullPath() {
            string fileName = "test1";
            string fullPath = Path.Combine(this.testFolder.FullName, fileName);
            IFileSystemInfo fileInfo = Factory.CreateFileInfo(fullPath);
            Assert.That(fileInfo.FullName, Is.EqualTo(fullPath));
        }

        [Test, Category("Medium")]
        public void Exists() {
            string fileName = "test1";
            string fullPath = Path.Combine(this.testFolder.FullName, fileName);
            IFileSystemInfo fileInfo = Factory.CreateFileInfo(fullPath);
            Assert.That(fileInfo.Exists, Is.EqualTo(false));
            using (var stream = new FileInfo(fullPath).Create()) {
                fileInfo = Factory.CreateFileInfo(fullPath);
                Assert.That(fileInfo.Exists, Is.EqualTo(true));
            }
        }

        [Test, Category("Medium")]
        public void Refresh() {
            string fileName = "test1";
            string fullPath = Path.Combine(this.testFolder.FullName, fileName);
            IFileSystemInfo fileInfo = Factory.CreateFileInfo(fullPath);

            // trigger lacy loading
            Assert.That(fileInfo.Exists, Is.EqualTo(false));
            var stream = new FileInfo(fullPath).Create();
            stream.Dispose();
            fileInfo.Refresh();
            Assert.That(fileInfo.Exists, Is.EqualTo(true));
        }

        [Test, Category("Medium")]
        public void Name() {
            string fileName = "test1";
            string fullPath = Path.Combine(this.testFolder.FullName, fileName);
            IFileSystemInfo fileInfo = Factory.CreateFileInfo(fullPath);
            Assert.That(fileInfo.Name, Is.EqualTo(fileName));
        }

        [Test, Category("Medium")]
        public void Attributes() {
            string fileName = "test1";
            string fullPath = Path.Combine(this.testFolder.FullName, fileName);
            this.testFolder.CreateSubdirectory(fileName);
            IFileSystemInfo fileInfo = Factory.CreateDirectoryInfo(fullPath);
            Assert.That(fileInfo.Attributes & FileAttributes.Directory, Is.EqualTo(FileAttributes.Directory));
        }

        [Test, Category("Medium")]
        public void Create() {
            string fileName = "test1";
            string fullPath = Path.Combine(this.testFolder.FullName, fileName);
            IDirectoryInfo dirInfo = Factory.CreateDirectoryInfo(fullPath);
            dirInfo.Create();
            Assert.That(dirInfo.Exists, Is.True);
        }

        [Test, Category("Medium")]
        public void CreateDirectoryWrapper() {
            string fileName = "test1";
            string fullPath = Path.Combine(this.testFolder.FullName, fileName);
            IFileInfo fileInfo = Factory.CreateFileInfo(fullPath);
            Assert.That(fileInfo.Directory.FullName, Is.EqualTo(this.testFolder.FullName));
        }

        [Test, Category("Medium")]
        public void Parent() {
            string fileName = "test1";
            string fullPath = Path.Combine(this.testFolder.FullName, fileName);
            IDirectoryInfo dirInfo = Factory.CreateDirectoryInfo(fullPath);
            Assert.That(dirInfo.Parent.FullName, Is.EqualTo(this.testFolder.FullName));
        }

        [Test, Category("Medium")]
        public void GetDirectoriesFor2() {
            string folder1 = "folder1";
            string folder2 = "folder2";
            this.testFolder.CreateSubdirectory(folder1);
            this.testFolder.CreateSubdirectory(folder2);
            IDirectoryInfo dirInfo = Factory.CreateDirectoryInfo(this.testFolder.FullName);
            Assert.That(dirInfo.GetDirectories().Length, Is.EqualTo(2));
            Assert.That(dirInfo.GetDirectories()[0].Name, Is.EqualTo(folder1));
            Assert.That(dirInfo.GetDirectories()[1].Name, Is.EqualTo(folder2));
        }

        [Test, Category("Medium")]
        public void GetDirectoriesFor0() {
            IDirectoryInfo dirInfo = Factory.CreateDirectoryInfo(this.testFolder.FullName);
            Assert.That(dirInfo.GetDirectories().Length, Is.EqualTo(0));
        }

        [Test, Category("Medium")]
        public void GetFilesFor2() {
            string file1 = "file1";
            string file2 = "file2";
            string fullPath1 = Path.Combine(this.testFolder.FullName, file1);
            string fullPath2 = Path.Combine(this.testFolder.FullName, file2);
            var stream = new FileInfo(fullPath1).Create();
            stream.Dispose();
            var stream2 = new FileInfo(fullPath2).Create();
            stream2.Dispose();
            IDirectoryInfo dirInfo = Factory.CreateDirectoryInfo(this.testFolder.FullName);
            Assert.That(dirInfo.GetFiles().Length, Is.EqualTo(2));
            Assert.That(dirInfo.GetFiles()[0].Name, Is.EqualTo(file1));
            Assert.That(dirInfo.GetFiles()[1].Name, Is.EqualTo(file2));
        }

        [Test, Category("Medium")]
        public void GetFilesFor0() {
            IDirectoryInfo dirInfo = Factory.CreateDirectoryInfo(this.testFolder.FullName);
            Assert.That(dirInfo.GetFiles().Length, Is.EqualTo(0));
        }

        [Test, Category("Medium")]
        public void DeleteTrue() {
            string fileName = "test1";
            string fullPath = Path.Combine(this.testFolder.FullName, fileName);
            IDirectoryInfo dirInfo = Factory.CreateDirectoryInfo(fullPath);
            dirInfo.Create();
            Assert.That(dirInfo.Exists, Is.True);
            dirInfo.Delete(true);
            dirInfo.Refresh();
            Assert.That(dirInfo.Exists, Is.False);
        }

        [Test, Category("Medium")]
        public void DeleteFile() {
            string fileName = "toBeDeleted";
            string fullPath = Path.Combine(this.testFolder.FullName, fileName);
            IFileInfo fileInfo = Factory.CreateFileInfo(fullPath);
            using (fileInfo.Open(FileMode.CreateNew)) {
            }

            Assert.That(fileInfo.Exists, Is.True);
            fileInfo.Delete();
            fileInfo.Refresh();
            Assert.That(fileInfo.Exists, Is.False);
        }

        [Test, Category("Medium")]
        public void ReplaceFileContent() {
            string sourceFile = "source";
            string targetFile = "target";
            string backupFile = "source.bak";
            IFileInfo sourceInfo = Factory.CreateFileInfo(Path.Combine(this.testFolder.FullName, sourceFile));
            IFileInfo targetInfo = Factory.CreateFileInfo(Path.Combine(this.testFolder.FullName, targetFile));
            IFileInfo backupInfo = Factory.CreateFileInfo(Path.Combine(this.testFolder.FullName, backupFile));
            using (var stream = sourceInfo.Open(FileMode.CreateNew, FileAccess.Write)) {
                stream.Write(new byte[2], 0, 2);
            }

            sourceInfo.Refresh();
            Assert.That(sourceInfo.Exists, Is.True);
            Assert.That(sourceInfo.Length, Is.EqualTo(2));
            using (var stream = targetInfo.Open(FileMode.CreateNew, FileAccess.Write)) {
                stream.Write(new byte[5], 0, 5);
            }

            targetInfo.Refresh();
            Assert.That(targetInfo.Exists, Is.True);
            Assert.That(targetInfo.Length, Is.EqualTo(5));

            var newFileInfo = sourceInfo.Replace(targetInfo, backupInfo, true);

            sourceInfo.Refresh();
            targetInfo.Refresh();
            backupInfo.Refresh();
            Assert.That(sourceInfo.Exists, Is.False);
            Assert.That(targetInfo.Length, Is.EqualTo(2));
            Assert.That(backupInfo.Exists, Is.True);
            Assert.That(backupInfo.Length, Is.EqualTo(5));
            Assert.That(newFileInfo.FullName, Is.EqualTo(targetInfo.FullName));
        }

        [Test, Category("Medium")]
        public void ReplaceFileContentAndExtendedAttributes() {
            this.SkipIfExtendedAttributesAreNotAvailable();

            string sourceFile = "source";
            string targetFile = "target";
            string backupFile = "source.bak";
            IFileInfo sourceInfo = Factory.CreateFileInfo(Path.Combine(this.testFolder.FullName, sourceFile));
            IFileInfo targetInfo = Factory.CreateFileInfo(Path.Combine(this.testFolder.FullName, targetFile));
            IFileInfo backupInfo = Factory.CreateFileInfo(Path.Combine(this.testFolder.FullName, backupFile));
            using (var stream = sourceInfo.Open(FileMode.CreateNew, FileAccess.Write)) {
                stream.Write(new byte[2], 0, 2);
            }

            sourceInfo.SetExtendedAttribute("test", sourceFile, false);
            sourceInfo.Refresh();
            using (var stream = targetInfo.Open(FileMode.CreateNew, FileAccess.Write)) {
                stream.Write(new byte[5], 0, 5);
            }

            targetInfo.SetExtendedAttribute("test", targetFile, false);
            targetInfo.Refresh();

            var newFileInfo = sourceInfo.Replace(targetInfo, backupInfo, true);
            Assert.That(newFileInfo.GetExtendedAttribute("test"), Is.EqualTo(sourceFile));
            backupInfo.Refresh();
            Assert.That(backupInfo.Exists, Is.True);
            Assert.That(backupInfo.GetExtendedAttribute("test"), Is.EqualTo(targetFile));
        }

        [Test, Category("Medium")]
        public void SetUuidOnFolderToNull() {
            this.SkipIfExtendedAttributesAreNotAvailable();
            var underTest = Factory.CreateDirectoryInfo(Path.Combine(this.testFolder.FullName, "folder"));
            underTest.Create();

            underTest.Uuid = null;

            Assert.That(underTest.Uuid, Is.Null);
        }

        [Test, Category("Medium")]
        public void SetUuidOnFolder() {
            this.SkipIfExtendedAttributesAreNotAvailable();
            var underTest = Factory.CreateDirectoryInfo(Path.Combine(this.testFolder.FullName, "folder"));
            underTest.Create();

            Guid uuid = Guid.NewGuid();
            underTest.Uuid = uuid;

            Assert.That(underTest.Uuid, Is.EqualTo(uuid));
        }

        [Test, Category("Medium")]
        public void GetUuidFromFolder() {
            this.SkipIfExtendedAttributesAreNotAvailable();
            var underTest = Factory.CreateDirectoryInfo(Path.Combine(this.testFolder.FullName, "folder"));
            underTest.Create();

            Guid uuid = Guid.NewGuid();

            underTest.SetExtendedAttribute("DSS-UUID", uuid.ToString(), false);

            Assert.That(underTest.Uuid, Is.EqualTo(uuid));
        }

        [Test, Category("Medium")]
        public void UuidIsNullIfStoredStringIsNotAnUuid() {
            this.SkipIfExtendedAttributesAreNotAvailable();
            var underTest = Factory.CreateDirectoryInfo(Path.Combine(this.testFolder.FullName, "folder"));
            underTest.Create();

            underTest.SetExtendedAttribute("DSS-UUID", "stuff", false);

            Assert.That(underTest.Uuid, Is.Null);
        }

        [Test, Category("Medium")]
        public void UuidIsNullIfNothingIsStored() {
            this.SkipIfExtendedAttributesAreNotAvailable();
            var underTest = Factory.CreateDirectoryInfo(Path.Combine(this.testFolder.FullName, "folder"));
            underTest.Create();

            Assert.That(underTest.Uuid, Is.Null);
        }

        [Test, Category("Medium")]
        public void CreatesFirstConflictFile() {
            string fileName = "test1.txt";
            string fullPath = Path.Combine(this.testFolder.FullName, fileName);
            var fileInfo = Factory.CreateFileInfo(fullPath);
            using (new FileInfo(fullPath).Create()) {
            }

            fileInfo = Factory.CreateFileInfo(fullPath);

            var conflictFile = Factory.CreateConflictFileInfo(fileInfo);

            Assert.That(conflictFile.Exists, Is.False);
            Assert.That(conflictFile.Directory.FullName, Is.EqualTo(fileInfo.Directory.FullName));
            Assert.That(Path.GetExtension(conflictFile.FullName), Is.EqualTo(Path.GetExtension(fileInfo.FullName)), "The file extension must be kept the same as in the original file");
            Assert.That(conflictFile.Name, Is.Not.EqualTo(fileInfo.Name));
        }

        [Test, Category("Medium")]
        public void IsDirectoryTrue() {
            string fullPath = Path.GetTempPath();
            Assert.That(Factory.IsDirectory(fullPath), Is.True);
        }

        [Test, Category("Medium")]
        public void IsDirectoryFalse() {
            string fullPath = Path.GetTempFileName();
            Assert.That(Factory.IsDirectory(fullPath), Is.False);
        }

        [Test, Category("Medium")]
        public void IsDirectoryNull() {
            string fullPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Assert.That(Factory.IsDirectory(fullPath), Is.Null);
        }

        [Test, Category("Medium")]
        public void IsDirectoryRequirements() {
            string path = Path.GetTempPath();
            Assert.That(Factory.CreateFileInfo(path).Exists, Is.False);
            Assert.That(Factory.CreateDirectoryInfo(path).Exists, Is.True);
        }

        [Test, Category("Medium")]
        public void DirectoryMoveTo() {
            string oldPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            string newPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            var folder = Factory.CreateDirectoryInfo(oldPath);
            folder.Create();
            folder.MoveTo(newPath);
            Assert.That(folder.Exists, Is.True);
            Assert.That(folder.FullName.TrimEnd(Path.DirectorySeparatorChar), Is.EqualTo(newPath));
            Assert.That(Factory.CreateDirectoryInfo(oldPath).Exists, Is.False);
        }

        [Test, Category("Medium")]
        public void DirectoryMoveToAlsoMovesExtendedAttributes() {
            string oldPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            string newPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            var folder = Factory.CreateDirectoryInfo(oldPath);
            folder.Create();
            BaseWatcherTest.IgnoreIfExtendedAttributesAreNotAvailable(oldPath);
            Assert.That(folder.GetExtendedAttribute("test"), Is.Null);
            folder.SetExtendedAttribute("test", "test", false);
            folder.MoveTo(newPath);
            Assert.That(folder.GetExtendedAttribute("test"), Is.EqualTo("test"));
            Assert.That(Factory.CreateDirectoryInfo(newPath).GetExtendedAttribute("test"), Is.EqualTo("test"));
        }

        [Test, Category("Medium")]
        public void FileMoveTo() {
            string oldPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            string newPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            var file = Factory.CreateFileInfo(oldPath);
            using (file.Open(FileMode.CreateNew)) {
            }

            file.MoveTo(newPath);
            Assert.That(file.Exists, Is.True);
            Assert.That(file.FullName, Is.EqualTo(newPath));
            Assert.That(Factory.CreateFileInfo(oldPath).Exists, Is.False);
        }

        [Test, Category("Medium")]
        public void FileMoveToAlsoMovesExtendedAttributes() {
            string oldPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            string newPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            var file = Factory.CreateFileInfo(oldPath);
            using (file.Open(FileMode.CreateNew)) {
            }

            BaseWatcherTest.IgnoreIfExtendedAttributesAreNotAvailable(oldPath);
            Assert.That(file.GetExtendedAttribute("test"), Is.Null);
            file.SetExtendedAttribute("test", "test", false);
            file.MoveTo(newPath);
            Assert.That(file.GetExtendedAttribute("test"), Is.EqualTo("test"));
            Assert.That(Factory.CreateFileInfo(newPath).GetExtendedAttribute("test"), Is.EqualTo("test"));
        }

        [Test, Category("Medium")]
        public void SetModificationDate() {
            string path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            var file = Factory.CreateFileInfo(path);
            using (file.Open(FileMode.CreateNew)) {
            }

            var nearFutureTime = DateTime.UtcNow.AddHours(1);
            file.LastWriteTimeUtc = nearFutureTime;
            Assert.That(file.LastWriteTimeUtc, Is.EqualTo(nearFutureTime).Within(1).Seconds);
        }

        [Test, Category("Medium")]
        public void SetOldModificationDate() {
            string path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            var file = Factory.CreateFileInfo(path);
            using (file.Open(FileMode.CreateNew)) {
            }

            var veryOldDate = new DateTime(1500, 1, 1);
            file.LastWriteTimeUtc = veryOldDate;
        }

        [Test, Category("Medium")]
        public void SetFutureModificationDate() {
            string path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            var file = Factory.CreateFileInfo(path);
            using (file.Open(FileMode.CreateNew)) {
            }

            var veryFuturisticDate = new DateTime(6000, 1, 1);
            file.LastWriteTimeUtc = veryFuturisticDate;
        }

        [Test, Category("Fast")]
        public void CreateDownloadCacheFileInfo([Values(true, false)]bool extendedAttributesAvailable) {
            Guid? uuid = extendedAttributesAvailable ? (Guid?)Guid.NewGuid() : null;
            string fileName = "file";
            IFileInfo file = Mock.Of<IFileInfo>(
                f =>
                f.Uuid == uuid &&
                f.Exists == true &&
                f.FullName == Path.Combine(Path.GetTempPath(), Path.GetRandomFileName(), fileName) &&
                f.Name == fileName);

            var cacheFile = Factory.CreateDownloadCacheFileInfo(file);
            if (extendedAttributesAvailable) {
                // Ensure that the path does not maps to temp path to avoid problems with extended attribute support
                Assert.That(cacheFile.FullName.Contains(Path.GetTempPath()), Is.False);
                Assert.That(cacheFile.Name, Is.EqualTo(uuid.ToString() + ".sync"));
            } else {
                Assert.That(cacheFile.FullName, Is.EqualTo(file.FullName + ".sync"));
                Assert.That(cacheFile.Name, Is.EqualTo(file.Name + ".sync"));
            }
        }

        [Test, Category("Fast")]
        public void CreateDownloadCacheFailsIfOriginalFileCannotBeAccessed() {
            var fileMock = new Mock<IFileInfo>();
            fileMock.Setup(f => f.Exists).Returns(true);
            fileMock.Setup(f => f.Uuid).Throws<ExtendedAttributeException>();
            Assert.Throws<ExtendedAttributeException>(() => Factory.CreateDownloadCacheFileInfo(fileMock.Object));
        }

        [Test, Category("Fast")]
        public void CreateDownloadCacheFailsIfFileDoesNotExists() {
            var file = Mock.Of<IFileInfo>(f => f.Exists == false);
            Assert.Throws<FileNotFoundException>(() => Factory.CreateDownloadCacheFileInfo(file));
        }

        [Test, Category("Fast")]
        public void CreateDownloadCacheCreatesIdenticalFileNames() {
            Guid uuid = Guid.NewGuid();
            var file = Mock.Of<IFileInfo>(
                f =>
                f.Uuid == uuid &&
                f.Exists == true);

            var cacheFile1 = Factory.CreateDownloadCacheFileInfo(file);
            var cacheFile2 = Factory.CreateDownloadCacheFileInfo(file);

            Assert.That(cacheFile1.Name, Is.EqualTo(cacheFile2.Name));
            Assert.That(cacheFile1.FullName, Is.EqualTo(cacheFile2.FullName));
        }

        [Test, Category("Fast")]
        public void CreateDownloadCacheCreatesIdenticalFileNamesIfUuidIsAvailableAndOriginalNamesAreDifferent() {
            Guid uuid = Guid.NewGuid();
            var file1 = Mock.Of<IFileInfo>(
                f =>
                f.Uuid == uuid &&
                f.Name == "name1" &&
                f.Exists == true);

            var file2 = Mock.Of<IFileInfo>(
                f =>
                f.Uuid == uuid &&
                f.Name == "name2" &&
                f.Exists == true);

            var cacheFile1 = Factory.CreateDownloadCacheFileInfo(file1);
            var cacheFile2 = Factory.CreateDownloadCacheFileInfo(file2);

            Assert.That(cacheFile1.Name, Is.EqualTo(cacheFile2.Name));
            Assert.That(cacheFile1.FullName, Is.EqualTo(cacheFile2.FullName));
        }

        [Test, Category("Fast")]
        public void CreateDownloadCacheWithGivenUuid() {
            Guid uuid = Guid.NewGuid();
            var cacheFile = Factory.CreateDownloadCacheFileInfo(uuid);
            Assert.That(cacheFile.Name, Is.EqualTo(uuid.ToString() + ".sync"));
        }

        [Test, Category("Fast")]
        public void CreateDownloadCacheWithEmptyUuidThrowsException() {
            Assert.Throws<ArgumentException>(() => Factory.CreateDownloadCacheFileInfo(Guid.Empty));
        }

        [Test, Category("Medium")]
        public void CreateNextConflictFile([Values(10)]int conflictFiles) {
            var filePaths = new HashSet<string>();
            var fileName = "file.txt";
            var file = Factory.CreateFileInfo(Path.Combine(this.testFolder.FullName, fileName));
            filePaths.Add(file.FullName);
            using (file.Open(FileMode.CreateNew)) {
            }

            for (int i = 1; i <= conflictFiles; i++) {
                var conflictFile = Factory.CreateConflictFileInfo(file);
                Assert.That(filePaths.Contains(conflictFile.FullName), Is.False);
                filePaths.Add(conflictFile.FullName);
                using (conflictFile.Open(FileMode.CreateNew)) {
                }

                Assert.That(filePaths.Count, Is.EqualTo(i + 1));
            }

            Assert.That(this.testFolder.GetFiles().Length, Is.EqualTo(conflictFiles + 1));
        }

        [Test, Category("Medium")]
        public void FileReadOnly() {
            var file = Factory.CreateFileInfo(Path.Combine(this.testFolder.FullName, "testfile.txt"));
            using (file.Open(FileMode.CreateNew)) {
            }

            Assert.That(file.ReadOnly, Is.False);
            file.ReadOnly = true;
            Assert.That(file.ReadOnly, Is.True);
            file.ReadOnly = false;
            Assert.That(file.ReadOnly, Is.False);
        }

        [Test, Category("Medium")]
        public void DirectoryReadOnly() {
            var dir = Factory.CreateDirectoryInfo(this.testFolder.FullName);
            Assert.That(dir.ReadOnly, Is.False);
            dir.ReadOnly = true;
            Assert.That(dir.ReadOnly, Is.True);
            dir.ReadOnly = false;
            Assert.That(dir.ReadOnly, Is.False);
        }

        [Test, Category("Medium")]
        public void RenameOfReadOnlyDirFails() {
            var dir = Factory.CreateDirectoryInfo(Path.Combine(this.testFolder.FullName, "cat"));
            dir.Create();
            dir.ReadOnly = true;
            dir.Parent.ReadOnly = true;
#if __MonoCS__
            Assert.Throws<UnauthorizedAccessException>(() => Directory.Move(dir.FullName, Path.Combine(this.testFolder.FullName, "dog")));
#else
            Assert.Throws<IOException>(() => dir.MoveTo(Path.Combine(this.testFolder.FullName, "dog")));
#endif
            dir.Refresh();
            Assert.That(dir.Name, Is.EqualTo("cat"));
        }

        [Test, Category("Medium")]
        public void CreateFileInReadOnlyDirFails() {
            var dir = Factory.CreateDirectoryInfo(Path.Combine(this.testFolder.FullName, "cat"));
            dir.Create();
            var file = Factory.CreateFileInfo(Path.Combine(dir.FullName, "file.bin"));
            dir.ReadOnly = true;
            Assert.Throws<UnauthorizedAccessException>(() => {
                using (file.Open(FileMode.CreateNew)) { }
            });
        }

        [Test, Category("Medium")]
        public void CreateFileInReadWriteSubFolderOfReadOnlyFolder() {
            var dir = Factory.CreateDirectoryInfo(Path.Combine(this.testFolder.FullName, "cat"));
            dir.Create();
            var parent = Factory.CreateDirectoryInfo(this.testFolder.FullName);
            parent.ReadOnly = true;
            var file = Factory.CreateFileInfo(Path.Combine(dir.FullName, "file.bin"));
            using (file.Open(FileMode.CreateNew)) { }

            dir.Refresh();
            file.Refresh();
            Assert.That(dir.ReadOnly, Is.False);
            Assert.That(file.ReadOnly, Is.False);
            Assert.That(parent.ReadOnly, Is.True);
        }

        [Test, Category("Medium")]
        public void ReadOnlyAttributeIsNotInheritedToChildDirectories() {
            var subdir = Factory.CreateDirectoryInfo(Path.Combine(this.testFolder.FullName, "cat"));
            subdir.Create();
            var underTest = Factory.CreateDirectoryInfo(this.testFolder.FullName);

            underTest.ReadOnly = true;

            Assert.That(underTest.ReadOnly, Is.True);
            subdir.Refresh();
            Assert.That(subdir.ReadOnly, Is.False);
        }

        [Test, Category("Medium")]
        public void WritingToReadOnlyFileMustFail() {
            var file = Factory.CreateFileInfo(Path.Combine(this.testFolder.FullName, "file.txt"));
            using (file.Open(FileMode.CreateNew, FileAccess.Write, FileShare.None)) { }
            file.ReadOnly = true;

            Assert.Throws<UnauthorizedAccessException>(() => {
                using(file.Open(FileMode.Open, FileAccess.Write, FileShare.None)) { }
            });
        }

        [Test, Category("Medium")]
        public void ReadingFromReadOnlyFileMustWork() {
            var file = Factory.CreateFileInfo(Path.Combine(this.testFolder.FullName, "file.txt"));
            using (file.Open(FileMode.CreateNew, FileAccess.Write, FileShare.None)) { }
            file.ReadOnly = true;
            using(file.Open(FileMode.Open, FileAccess.Read, FileShare.None)) { }
        }

        [Test, Category("Medium")]
        public void SetUuidToReadOnlyFileShouldFail() {
            this.SkipIfExtendedAttributesAreNotAvailable();
            var file = Factory.CreateFileInfo(Path.Combine(this.testFolder.FullName, "file.txt"));
            using (file.Open(FileMode.CreateNew, FileAccess.Write, FileShare.None)) { }
            var uuid = Guid.NewGuid();
            file.Uuid = uuid;
            file.ReadOnly = true;
            try {
                file.Uuid = Guid.NewGuid();
                Assert.Fail("Setting a Uuid to a read only file must fail, but didn't");
            } catch (IOException) {
            }

            Assert.That(file.Uuid, Is.EqualTo(uuid));
            Assert.That(file.ReadOnly, Is.True);
        }

        [Test, Category("Medium")]
        public void SetModificationDateToReadOnlyDirectoryCanFail() {
            var past = DateTime.UtcNow - TimeSpan.FromHours(1);
            var dir = Factory.CreateDirectoryInfo(Path.Combine(this.testFolder.FullName, "cat"));
            dir.Create();
            dir.LastWriteTimeUtc = past;
            dir.ReadOnly = true;
            try {
                dir.LastWriteTimeUtc = DateTime.UtcNow;
                Assert.That(dir.LastWriteTimeUtc, Is.EqualTo(DateTime.UtcNow).Within(1).Seconds);
            } catch (UnauthorizedAccessException) {
                Assert.That(dir.LastWriteTimeUtc, Is.EqualTo(past).Within(1).Seconds);
            }

            Assert.That(dir.ReadOnly, Is.True);
        }

        [Test, Category("Medium")]
        public void SetModificationDateToReadOnlyFileMustFail() {
            var past = DateTime.UtcNow - TimeSpan.FromHours(1);
            var file = Factory.CreateFileInfo(Path.Combine(this.testFolder.FullName, "file"));
            using (file.Open(FileMode.CreateNew)) { }
            file.LastWriteTimeUtc = past;
            file.ReadOnly = true;
            try {
                file.LastWriteTimeUtc = DateTime.UtcNow;
                Assert.Fail("Setting last write time utc must fail on a read only file, but didn't");
            } catch (UnauthorizedAccessException) {
            }

            Assert.That(file.LastWriteTimeUtc, Is.EqualTo(past).Within(1).Seconds);
            Assert.That(file.ReadOnly, Is.True);
        }

#if !__MonoCS__
        [Test, Category("Fast")]
        public void AclUser() {
            var account = new System.Security.Principal.NTAccount(Environment.UserName);
            Assert.That(account.ToString().Contains(Environment.UserName));
            Assert.That(account.IsValidTargetType(typeof(System.Security.Principal.SecurityIdentifier)));
            var securityAccount = account.Translate(typeof(System.Security.Principal.SecurityIdentifier)) as System.Security.Principal.SecurityIdentifier;
            Assert.That(securityAccount.IsAccountSid(), Is.True);
        }
#endif

        [Ignore("Windows only and needs second partition as target fs")]
        [Test, Category("Medium")]
        public void ReplaceFile([Values("E:\\\\")]string targetFS) {
            this.testFolderOnOtherFS = new DirectoryInfo(Path.Combine(targetFS, Guid.NewGuid().ToString()));
            if (!this.testFolderOnOtherFS.Exists) {
                this.testFolderOnOtherFS.Create();
            }

            if (!new DirectoryInfoWrapper(this.testFolderOnOtherFS).IsExtendedAttributeAvailable()) {
                Assert.Ignore();
            }

            string fileName = "testFile";
            var sourceFile = Factory.CreateFileInfo(Path.Combine(this.testFolder.FullName, fileName));
            using (sourceFile.Open(FileMode.CreateNew));
            var uuid = Guid.NewGuid();
            sourceFile.Uuid = uuid;
            var targetFile = Factory.CreateFileInfo(Path.Combine(this.testFolderOnOtherFS.FullName, fileName));
            var backupFile = Factory.CreateFileInfo(targetFile.FullName + ".bak");
            using (targetFile.Open(FileMode.CreateNew));

            var resultFile = sourceFile.Replace(targetFile, backupFile, true);

            sourceFile.Refresh();
            targetFile.Refresh();
            backupFile.Refresh();
            Assert.That(resultFile.Uuid, Is.EqualTo(uuid));
            Assert.That(targetFile.Uuid, Is.EqualTo(uuid));
            Assert.That(backupFile.Uuid, Is.Null);
            Assert.That(resultFile.FullName, Is.EqualTo(targetFile.FullName));
            Assert.That(sourceFile.Exists, Is.False);
            Assert.That(this.testFolderOnOtherFS.GetFileSystemInfos().Length, Is.EqualTo(2));
            Assert.That(this.testFolder.GetFileSystemInfos().Length, Is.EqualTo(0));
        }

        private void SkipIfExtendedAttributesAreNotAvailable() {
            if (!Factory.CreateDirectoryInfo(this.testFolder.FullName).IsExtendedAttributeAvailable()) {
                Assert.Ignore("Extended Attributes are not available => test skipped.");
            }
        }

        private void RemoveReadOnlyFlagRecursive(FileSystemInfo info) {
            info.Refresh();
            if (info is FileInfo) {
                new FileInfoWrapper(info as FileInfo).ReadOnly = false;
            } else if (info is DirectoryInfo) {
                new DirectoryInfoWrapper(info as DirectoryInfo).ReadOnly = false;
                foreach (var child in (info as DirectoryInfo).GetFileSystemInfos()) {
                    this.RemoveReadOnlyFlagRecursive(child);
                }
            }
        }
    }
}