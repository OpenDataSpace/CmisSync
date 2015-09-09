//-----------------------------------------------------------------------
// <copyright file="MockOfIFileSystemInfoFactoryUtil.cs" company="GRAU DATA AG">
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

namespace TestLibrary.TestUtils {
    using System;
    using System.Collections.Generic;
    using System.IO;

    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;

    using Moq;

    public static class MockOfIFileSystemInfoFactoryUtil {
        public static void AddIDirectoryInfo(this Mock<IFileSystemInfoFactory> fsFactory, IDirectoryInfo dirInfo) {
            fsFactory.Setup(f => f.CreateDirectoryInfo(dirInfo.FullName)).Returns(dirInfo);
        }

        public static Mock<IDirectoryInfo> AddDirectory(this Mock<IFileSystemInfoFactory> fsFactory, string path, bool exists = true) {
            if (path.EndsWith("/")) {
                throw new ArgumentException("FileName gives last tuple of path not ending on / so path should not end with /");
            }

            Mock<IDirectoryInfo> dir = new Mock<IDirectoryInfo>();
            dir.Setup(d => d.FullName).Returns(path);
            dir.Setup(d => d.Name).Returns(Path.GetFileName(path));
            dir.Setup(d => d.Exists).Returns(exists);
            fsFactory.AddIDirectoryInfo(dir.Object);
            return dir;
        }

        public static Mock<IDirectoryInfo> AddDirectory(this Mock<IFileSystemInfoFactory> fsFactory, string path, Guid guid, bool exists = true) {
            return fsFactory.AddDirectory(path, exists).SetupGuid(guid);
        }

        public static void SetupDirectories(this Mock<IDirectoryInfo> parent, params IDirectoryInfo[] dirs) {
            parent.Setup(p => p.GetDirectories()).Returns(dirs);
        }

        public static void SetupFiles(this Mock<IDirectoryInfo> parent, params IFileInfo[] files) {
            parent.Setup(p => p.GetFiles()).Returns(files);
        }

        public static void SetupMoveTo(this Mock<IDirectoryInfo> folder, string path = null) {
            var setup = path == null ? folder.Setup(f => f.MoveTo(It.IsAny<string>())) : folder.Setup(f => f.MoveTo(path));
            setup.Callback<string>(
                (p) => {
                folder.Setup(f => f.FullName).Returns(p);
                folder.Setup(f => f.Name).Returns(Path.GetFileName(p));
            });
        }

        public static Mock<IFileInfo> SetupDownloadCacheFile(this Mock<IFileSystemInfoFactory> fsFactory, IFileInfo expectedInput = null, Guid? expectedUuid = null) {
            var downloadFile = new Mock<IFileInfo>();
            if (expectedInput == null) {
                fsFactory.Setup(factory => factory.CreateDownloadCacheFileInfo(It.IsAny<IFileInfo>())).Returns(downloadFile.Object);
            } else {
                fsFactory.Setup(factory => factory.CreateDownloadCacheFileInfo(expectedInput)).Returns(downloadFile.Object);
            }

            if (expectedUuid == null) {
                fsFactory.Setup(factory => factory.CreateDownloadCacheFileInfo(It.IsAny<Guid>())).Returns(downloadFile.Object);
            } else {
                fsFactory.Setup(factory => factory.CreateDownloadCacheFileInfo((Guid)expectedUuid)).Returns(downloadFile.Object);
            }

            return downloadFile;
        }

        public static Mock<IFileInfo> SetupStream(this Mock<IFileInfo> file, byte[] content) {
            file.Setup(f => f.Length).Returns(content.Length);
            file.Setup(f => f.Open(FileMode.Open, FileAccess.Read, It.IsAny<FileShare>())).Returns(() => new MemoryStream(content));
            return file;
        }

        public static Mock<IFileInfo> SetupReadOnly(this Mock<IFileInfo> file, bool isReadOnly) {
            file.SetupProperty(f => f.ReadOnly, isReadOnly);
            return file;
        }

        public static Mock<IFileSystemInfo> SetupReadOnly(this Mock<IFileSystemInfo> file, bool isReadOnly) {
            file.SetupProperty(f => f.ReadOnly, isReadOnly);
            return file;
        }

        public static Mock<IDirectoryInfo> SetupReadOnly(this Mock<IDirectoryInfo> dir, bool isReadOnly) {
            dir.SetupProperty(d => d.ReadOnly, isReadOnly);
            return dir;
        }

        public static Mock<IFileInfo> SetupName(this Mock<IFileInfo> file, string name) {
            file.SetupGet(f => f.Name).Returns(name);
            return file;
        }

        public static Mock<IFileSystemInfo> SetupName(this Mock<IFileSystemInfo> file, string name) {
            file.SetupGet(f => f.Name).Returns(name);
            return file;
        }

        public static Mock<IDirectoryInfo> SetupName(this Mock<IDirectoryInfo> dir, string name) {
            dir.SetupGet(f => f.Name).Returns(name);
            return dir;
        }

        public static Mock<IFileInfo> SetupExists(this Mock<IFileInfo> file, bool exists = true) {
            file.SetupGet(f => f.Exists).Returns(exists);
            return file;
        }

        public static Mock<IFileSystemInfo> SetupExists(this Mock<IFileSystemInfo> file, bool exists = true) {
            file.SetupGet(f => f.Exists).Returns(exists);
            return file;
        }

        public static Mock<IDirectoryInfo> SetupExists(this Mock<IDirectoryInfo> dir, bool exists = true) {
            dir.SetupGet(d => d.Exists).Returns(exists);
            return dir;
        }

        public static Mock<IFileInfo> SetupSymlink(this Mock<IFileInfo> file, bool isSymlink = false) {
            file.SetupGet(f => f.IsSymlink).Returns(isSymlink);
            return file;
        }

        public static Mock<IFileSystemInfo> SetupSymlink(this Mock<IFileSystemInfo> file, bool isSymlink = false) {
            file.SetupGet(f => f.IsSymlink).Returns(isSymlink);
            return file;
        }

        public static Mock<IDirectoryInfo> SetupSymlink(this Mock<IDirectoryInfo> dir, bool isSymlink = false) {
            dir.SetupGet(d => d.IsSymlink).Returns(isSymlink);
            return dir;
        }

        public static Mock<IFileInfo> SetupFullName(this Mock<IFileInfo> file, string fullName) {
            file.SetupGet(f => f.FullName).Returns(fullName);
            return file;
        }

        public static Mock<IFileSystemInfo> SetupFullName(this Mock<IFileSystemInfo> file, string fullName) {
            file.SetupGet(f => f.FullName).Returns(fullName);
            return file;
        }

        public static Mock<IDirectoryInfo> SetupFullName(this Mock<IDirectoryInfo> dir, string fullName) {
            dir.SetupGet(d => d.FullName).Returns(fullName);
            return dir;
        }

        public static Mock<IDirectoryInfo> SetupFilesAndDirectories(this Mock<IDirectoryInfo> parent, params IFileSystemInfo[] children) {
            List<IDirectoryInfo> dirs = new List<IDirectoryInfo>();
            List<IFileInfo> files = new List<IFileInfo>();
            foreach (var child in children) {
                if (child is IFileInfo) {
                    files.Add(child as IFileInfo);
                } else if (child is IDirectoryInfo) {
                    dirs.Add(child as IDirectoryInfo);
                }
            }

            parent.SetupFiles(files.ToArray());
            parent.SetupDirectories(dirs.ToArray());
            return parent;
        }

        public static void AddDirectoryWithParents(this Mock<IFileSystemInfoFactory> fsFactory, string path) {
            if (path.Length > 0) {
                fsFactory.AddDirectory(path);
            }

            int lastSeperator = path.LastIndexOf(Path.DirectorySeparatorChar.ToString());
            if (lastSeperator > 0) {
                fsFactory.AddDirectoryWithParents(path.Substring(lastSeperator));
            }
        }

        public static void AddIFileInfo(this Mock<IFileSystemInfoFactory> fsFactory, IFileInfo fileInfo, bool exists = true) {
            fsFactory.Setup(f => f.CreateFileInfo(fileInfo.FullName)).Returns(fileInfo);
            if (exists) {
                fsFactory.Setup(f => f.IsDirectory(fileInfo.FullName)).Returns(false);
            }
        }

        public static Mock<IFileInfo> AddFile(this Mock<IFileSystemInfoFactory> fsFactory, string path, bool exists = true) {
            Mock<IFileInfo> file = new Mock<IFileInfo>().SetupName(Path.GetFileName(path)).SetupFullName(path).SetupExists(exists);
            fsFactory.AddIFileInfo(file.Object, exists);
            return file;
        }

        public static Mock<IFileInfo> AddFile(this Mock<IFileSystemInfoFactory> fsFactory, string path, Guid guid, bool exists = true) {
            return fsFactory.AddFile(path, exists).SetupGuid(guid);
        }

        public static Mock<IDirectoryInfo> CreateLocalFolder(string path, List<string> fileNames = null, List<string> folderNames = null) {
            var localFolder = new Mock<IDirectoryInfo>();
            localFolder.Setup(f => f.FullName).Returns(path);
            var fileList = new List<IFileInfo>();
            if (fileNames != null) {
                foreach (var name in fileNames) {
                    var file = new Mock<IFileInfo>();
                    file.Setup(d => d.Name).Returns(name);
                    fileList.Add(file.Object);
                }
            }

            localFolder.Setup(f => f.GetFiles()).Returns(fileList.ToArray());
            var folderList = new List<IDirectoryInfo>();
            if (folderNames != null) {
                foreach (var name in folderNames) {
                    var folder = new Mock<IDirectoryInfo>();
                    folder.Setup(d => d.Name).Returns(name);
                    folderList.Add(folder.Object);
                }
            }

            localFolder.Setup(f => f.GetDirectories()).Returns(folderList.ToArray());
            localFolder.Setup(f => f.Name).Returns(Path.GetFileName(path));
            return localFolder;
        }

        public static Mock<IFileSystemInfo> SetupLastWriteTimeUtc(this Mock<IFileSystemInfo> fileSystemInfo, DateTime lastWriteTimeUtc) {
            fileSystemInfo.Setup(f => f.LastWriteTimeUtc).Returns(lastWriteTimeUtc);
            return fileSystemInfo;
        }

        public static Mock<IFileSystemInfo> SetupGuid(this Mock<IFileSystemInfo> fileSystemInfo, Guid uuid) {
            fileSystemInfo.Setup(f => f.Uuid).Returns(uuid);
            return fileSystemInfo;
        }

        public static Mock<IFileInfo> SetupLastWriteTimeUtc(this Mock<IFileInfo> fileInfo, DateTime lastWriteTimeUtc) {
            fileInfo.Setup(f => f.LastWriteTimeUtc).Returns(lastWriteTimeUtc);
            return fileInfo;
        }

        public static Mock<IFileInfo> SetupGuid(this Mock<IFileInfo> fileInfo, Guid uuid) {
            fileInfo.Setup(f => f.Uuid).Returns(uuid);
            return fileInfo;
        }

        public static Mock<IDirectoryInfo> SetupLastWriteTimeUtc(this Mock<IDirectoryInfo> dirInfo, DateTime lastWriteTimeUtc) {
            dirInfo.Setup(f => f.LastWriteTimeUtc).Returns(lastWriteTimeUtc);
            return dirInfo;
        }

        public static Mock<IDirectoryInfo> SetupGuid(this Mock<IDirectoryInfo> dirInfo, Guid uuid) {
            dirInfo.Setup(f => f.Uuid).Returns(uuid);
            return dirInfo;
        }

        public static Mock<IFileInfo> SetupOpenThrows<TException>(this Mock<IFileInfo> fileInfo, TException exception = default(TException)) where TException : System.Exception {
            fileInfo.Setup(f => f.Open(It.IsAny<FileMode>())).Throws(exception);
            fileInfo.Setup(f => f.Open(It.IsAny<FileMode>(), It.IsAny<FileAccess>())).Throws(exception);
            fileInfo.Setup(f => f.Open(It.IsAny<FileMode>(), It.IsAny<FileAccess>(), It.IsAny<FileShare>())).Throws(exception);
            return fileInfo;
        }

        public static void VerifyThatLocalFileObjectLastWriteTimeUtcIsNeverModified(this Mock<IFileSystemInfo> fsInfo) {
            fsInfo.VerifySet(o => o.LastWriteTimeUtc = It.IsAny<DateTime>(), Times.Never());
        }

        public static void VerifyThatLocalFileObjectLastWriteTimeUtcIsNeverModified(this Mock<IFileInfo> fsInfo) {
            fsInfo.VerifySet(o => o.LastWriteTimeUtc = It.IsAny<DateTime>(), Times.Never());
        }

        public static void VerifyThatLocalFileObjectLastWriteTimeUtcIsNeverModified(this Mock<IDirectoryInfo> fsInfo) {
            fsInfo.VerifySet(o => o.LastWriteTimeUtc = It.IsAny<DateTime>(), Times.Never());
        }

        public static void VerifyThatReadOnlyPropertyIsSet(this Mock<IDirectoryInfo> fsInfo, bool to, bool iff = true) {
            fsInfo.VerifySet(d => d.ReadOnly = to, iff ? Times.Once() : Times.Never());
            fsInfo.VerifySet(d => d.ReadOnly = !to, Times.Never());
        }

        public static void VerifyThatReadOnlyPropertyIsSet(this Mock<IFileInfo> fsInfo, bool to, bool iff = true) {
            fsInfo.VerifySet(d => d.ReadOnly = to, iff ? Times.Once() : Times.Never());
            fsInfo.VerifySet(d => d.ReadOnly = !to, Times.Never());
        }
    }
}