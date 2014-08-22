//-----------------------------------------------------------------------
// <copyright file="FileSystemInfoFactory.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Storage.FileSystem
{
    using System;
    using System.IO;

    /// <summary>
    /// Wrapps all interfaced methods and calls the Systems.IO classes
    /// </summary>
    public class FileSystemInfoFactory : IFileSystemInfoFactory
    {
        /// <summary>
        /// Creates the directory info.
        /// </summary>
        /// <returns>The directory info.</returns>
        /// <param name="path">For this path.</param>
        public IDirectoryInfo CreateDirectoryInfo(string path) {
            return new DirectoryInfoWrapper(new DirectoryInfo(path));
        }

        /// <summary>
        /// Creates the file info.
        /// </summary>
        /// <returns>The file info.</returns>
        /// <param name="path">For this path.</param>
        public IFileInfo CreateFileInfo(string path) {
            return new FileInfoWrapper(new FileInfo(path));
        }

        /// <summary>
        /// Creates a conflict file info for the given file.
        /// </summary>
        /// <returns>The conflict file info.</returns>
        /// <param name="file">File for which a new conflict file should be created.</param>
        public IFileInfo CreateConflictFileInfo(IFileInfo file) {
            if (!file.Exists) {
                return file;
            }

            string user = Environment.UserName;
            string extension = Path.GetExtension(file.FullName);
            string suffix = file.Name.Substring(0, file.Name.Length - extension.Length);
            string filename = string.Format("{0}_{1}-version{2}", suffix, user, extension);

            IFileInfo conflictFile = this.CreateFileInfo(Path.Combine(file.Directory.FullName, filename));
            if (!conflictFile.Exists) {
                return conflictFile;
            }

            int index = 1;
            do
            {
                filename = string.Format("{0}_{1}-version ({2}){3}", suffix, user, index.ToString(), extension);
                conflictFile = this.CreateFileInfo(Path.Combine(file.Directory.FullName, filename));
                index++;
            }
            while (conflictFile.Exists);
            return conflictFile;
        }

        /// <summary>
        /// Determines whether the path is an existing directory or an existing file or does not exist.
        /// </summary>
        /// <returns><c>true</c> if this path points to a directory;<c>false</c> if this path points to a file; otherwise if nothing exists on the path <c>null</c>.</returns>
        /// <param name="path">Full path.</param>
        public bool? IsDirectory(string path) {
            if (this.CreateFileInfo(path).Exists) {
                return false;
            }

            if (this.CreateDirectoryInfo(path).Exists) {
                return true;
            }

            return null;
        }

        public IFileInfo CreateDownloadCacheFileInfo(IFileInfo file) {
            if (!file.Exists) {
                throw new FileNotFoundException(string.Format("Given file {0} does not exists", file.FullName));
            }

            Guid? uuid = file.Uuid;
            if (uuid == null | uuid == Guid.Empty) {
                return this.CreateFileInfo(file.FullName + ".sync");
            } else {
                return this.CreateFileInfo(Path.Combine(Path.GetTempPath(), uuid.ToString() + ".sync"));
            }
        }
    }
}