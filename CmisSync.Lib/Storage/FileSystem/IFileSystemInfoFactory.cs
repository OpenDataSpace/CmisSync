//-----------------------------------------------------------------------
// <copyright file="IFileSystemInfoFactory.cs" company="GRAU DATA AG">
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

    /// <summary>
    /// Interface for Creating IFileSystemInfo Implementations
    /// </summary>
    public interface IFileSystemInfoFactory
    {
        /// <summary>
        /// Creates a directory info.
        /// </summary>
        /// <returns>The directory info.</returns>
        /// <param name="path">Full path to the directory.</param>
        IDirectoryInfo CreateDirectoryInfo(string path);

        /// <summary>
        /// Creates a file info.
        /// </summary>
        /// <returns>The file info.</returns>
        /// <param name="fileName">Full path with file name.</param>
        IFileInfo CreateFileInfo(string fileName);

        /// <summary>
        /// Creates a conflict file info for the given file.
        /// </summary>
        /// <returns>The conflict file info.</returns>
        /// <param name="file">File for which a new conflict file should be created.</param>
        IFileInfo CreateConflictFileInfo(IFileInfo file);

        /// <summary>
        /// Determines whether the path is an existing directory or an existing file or does not exist.
        /// </summary>
        /// <returns><c>true</c> if this path points to a directory;<c>false</c> if this path points to a file; otherwise if nothing exists on the path <c>null</c>.</returns>
        /// <param name="path">Full path.</param>
        bool? IsDirectory(string path);
    }
}