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

namespace TestLibrary.TestUtils
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;

    using Moq;

    public static class MockOfIFileSystemInfoFactoryUtil
    {
        public static void AddIDirectoryInfo(this Mock<IFileSystemInfoFactory> fsFactory, IDirectoryInfo dirInfo)
        {
            fsFactory.Setup(f => f.CreateDirectoryInfo(dirInfo.FullName)).Returns(dirInfo);
        }

        public static Mock<IDirectoryInfo> AddDirectory(this Mock<IFileSystemInfoFactory> fsFactory, string path, bool exists = true)
        {
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

        public static Mock<IDirectoryInfo> AddDirectory(this Mock<IFileSystemInfoFactory> fsFactory, string path, Guid guid, bool exists = true)
        {
            var dir = fsFactory.AddDirectory(path, exists);
            dir.Setup(d => d.GetExtendedAttribute(MappedObject.ExtendedAttributeKey)).Returns(guid.ToString());
            return dir;
        }

        public static void SetupDirectories(this Mock<IDirectoryInfo> parent, params IDirectoryInfo[] dirs)
        {
            parent.Setup(p => p.GetDirectories()).Returns(dirs);
        }

        public static void SetupFiles(this Mock<IDirectoryInfo> parent, params IFileInfo[] files)
        {
            parent.Setup(p => p.GetFiles()).Returns(files);
        }

        public static void SetupFilesAndDirectories(this Mock<IDirectoryInfo> parent, params IFileSystemInfo[] children)
        {
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
        }

        public static void AddDirectoryWithParents(this Mock<IFileSystemInfoFactory> fsFactory, string path)
        {
            if (path.Length > 0) {
                fsFactory.AddDirectory(path);
            }

            int lastSeperator = path.LastIndexOf(Path.DirectorySeparatorChar.ToString());
            if (lastSeperator > 0) {
                fsFactory.AddDirectoryWithParents(path.Substring(lastSeperator));
            }
        }

        public static void AddIFileInfo(this Mock<IFileSystemInfoFactory> fsFactory, IFileInfo fileInfo)
        {
            fsFactory.Setup(f => f.CreateFileInfo(fileInfo.FullName)).Returns(fileInfo);
        }

        public static Mock<IFileInfo> AddFile(this Mock<IFileSystemInfoFactory> fsFactory, string path, bool exists = true)
        {
            Mock<IFileInfo> file = new Mock<IFileInfo>();
            file.Setup(f => f.Name).Returns(Path.GetFileName(path));
            file.Setup(f => f.FullName).Returns(path);
            file.Setup(f => f.Exists).Returns(exists);
            fsFactory.AddIFileInfo(file.Object);
            return file;
        }

        public static Mock<IFileInfo> AddFile(this Mock<IFileSystemInfoFactory> fsFactory, string path, Guid guid, bool exists = true)
        {
            var file = fsFactory.AddFile(path, exists);
            file.Setup(f => f.GetExtendedAttribute(MappedObject.ExtendedAttributeKey)).Returns(guid.ToString());
            return file;
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

        public static void SetupLastWriteTimeUtc(this Mock<IFileSystemInfo> fileSystemInfo, DateTime lastWriteTimeUtc) {
            fileSystemInfo.Setup(f => f.LastWriteTimeUtc).Returns(lastWriteTimeUtc);
        }

        public static void SetupGuid(this Mock<IFileSystemInfo> fileSystemInfo, Guid uuid) {
            fileSystemInfo.Setup(f => f.GetExtendedAttribute(MappedObject.ExtendedAttributeKey)).Returns(uuid.ToString());
        }

        public static void SetupLastWriteTimeUtc(this Mock<IFileInfo> fileInfo, DateTime lastWriteTimeUtc) {
            fileInfo.Setup(f => f.LastWriteTimeUtc).Returns(lastWriteTimeUtc);
        }

        public static void SetupGuid(this Mock<IFileInfo> fileInfo, Guid uuid) {
            fileInfo.Setup(f => f.GetExtendedAttribute(MappedObject.ExtendedAttributeKey)).Returns(uuid.ToString());
        }

        public static void SetupLastWriteTimeUtc(this Mock<IDirectoryInfo> dirInfo, DateTime lastWriteTimeUtc) {
            dirInfo.Setup(f => f.LastWriteTimeUtc).Returns(lastWriteTimeUtc);
        }

        public static void SetupGuid(this Mock<IDirectoryInfo> dirInfo, Guid uuid) {
            dirInfo.Setup(f => f.GetExtendedAttribute(MappedObject.ExtendedAttributeKey)).Returns(uuid.ToString());
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
    }
}