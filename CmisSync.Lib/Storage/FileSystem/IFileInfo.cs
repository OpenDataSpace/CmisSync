//-----------------------------------------------------------------------
// <copyright file="IFileInfo.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Storage.FileSystem {
    using System;
    using System.IO;

    /// <summary>
    /// Interface to enable mocking of FileInfo
    /// </summary>
    public interface IFileInfo : IFileSystemInfo {
        /// <summary>
        /// Gets the parent directory.
        /// </summary>
        /// <value>The parent directory.</value>
        IDirectoryInfo Directory { get; }

        /// <summary>
        /// Gets the file content length.
        /// </summary>
        /// <value>The length.</value>
        long Length { get; }

        /// <summary>
        /// Open the specified file.
        /// </summary>
        /// <param name="mode">Open mode.</param>
        /// <returns>Stream of the content.</returns>
        Stream Open(FileMode mode);

        /// <summary>
        /// Open the specified file with the open and access mode.
        /// </summary>
        /// <param name="mode">Open mode.</param>
        /// <param name="access">Access Mode.</param>
        /// <returns>Stream of the content</returns>
        Stream Open(FileMode mode, FileAccess access);

        /// <summary>
        /// Open the specified file with given open, access and share mode.
        /// </summary>
        /// <param name="mode">Open mode.</param>
        /// <param name="access">Access mode.</param>
        /// <param name="share">Share mode.</param>
        /// <returns>Stream of the content</returns>
        Stream Open(FileMode mode, FileAccess access, FileShare share);

        /// <summary>
        /// Moves to target file.
        /// </summary>
        /// <param name="target">Target file name.</param>
        void MoveTo(string target);

        /// <summary>
        /// Replaces the contents of a specified destinationFile with the file described by the current IFileInfo object, deleting the original file, and creating a backup of the replaced file.
        /// Also specifies whether to ignore merge errors.
        /// </summary>
        /// <param name="destinationFile">Destination file.</param>
        /// <param name="destinationBackupFileName">Destination backup file name.</param>
        /// <param name="ignoreMetadataErrors"><c>true</c> to ignore merge errors (such as attributes and ACLs) from the replaced file to the replacement file; otherwise <c>false</c>.</param>
        /// <returns>A IFileInfo object that encapsulates information about the file described by the destFileName parameter.</returns>
        IFileInfo Replace(IFileInfo destinationFile, IFileInfo destinationBackupFileName, bool ignoreMetadataErrors);

        /// <summary>
        /// Deletes the file on the fs.
        /// </summary>
        void Delete();
    }
}