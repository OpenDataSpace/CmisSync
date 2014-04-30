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

namespace CmisSync.Lib.Storage
{
    using System;
    using System.IO;

    /// <summary>
    /// Interface to enable mocking of FileInfo
    /// </summary>
    public interface IFileInfo : IFileSystemInfo
    {
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
        /// <param name="open">Open mode.</param>
        /// <returns>Stream of the content.</returns>
        Stream Open(FileMode open);

        /// <summary>
        /// Open the specified file with the open and access mode.
        /// </summary>
        /// <param name="open">Open mode.</param>
        /// <param name="access">Access Mode.</param>
        /// <returns>Stream of the content</returns>
        Stream Open(FileMode open, FileAccess access);

        /// <summary>
        /// Open the specified file with given open, access and share mode.
        /// </summary>
        /// <param name="open">Open mode.</param>
        /// <param name="access">Access mode.</param>
        /// <param name="share">Share mode.</param>
        /// <returns>Stream of the content</returns>
        Stream Open(FileMode open, FileAccess access, FileShare share);
    }
}
