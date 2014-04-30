//-----------------------------------------------------------------------
// <copyright file="FileInfoWrapper.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Storage
{
    using System;
    using System.IO;

    /// <summary>
    /// Wrapper for FileInfo
    /// </summary>
    public class FileInfoWrapper : FileSystemInfoWrapper, IFileInfo
    {
        private FileInfo original;

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Storage.FileInfoWrapper"/> class.
        /// </summary>
        /// <param name="fileInfo">File info.</param>
        public FileInfoWrapper(FileInfo fileInfo) : base(fileInfo)
        {
            this.original = fileInfo;
        }

        /// <summary>
        /// Gets the parent directory.
        /// </summary>
        /// <value>The parent directory.</value>
        public IDirectoryInfo Directory {
            get {
                return new DirectoryInfoWrapper(this.original.Directory);
            }
        }

        /// <summary>
        /// Gets the file content length.
        /// </summary>
        /// <value>The length.</value>
        public long Length {
            get {
                return this.original.Length;
            }
        }

        /// <summary>
        /// Open the specified file.
        /// </summary>
        /// <param name="mode">Open mode.</param>
        /// <returns>Stream of the content.</returns>
        public Stream Open(FileMode mode)
        {
            return this.original.Open(mode);
        }

        /// <summary>
        /// Open the specified file with the open and access mode.
        /// </summary>
        /// <param name="mode">Open mode.</param>
        /// <param name="access">Access Mode.</param>
        /// <returns>Stream of the content</returns>
        public Stream Open(FileMode mode, FileAccess access)
        {
            return this.original.Open(mode, access);
        }

        /// <summary>
        /// Open the specified file with given open, access and share mode.
        /// </summary>
        /// <param name="mode">Open mode.</param>
        /// <param name="access">Access mode.</param>
        /// <param name="share">Share mode.</param>
        /// <returns>Stream of the content</returns>
        public Stream Open(FileMode mode, FileAccess access, FileShare share)
        {
            return this.original.Open(mode, access, share);
        }
    }
}
