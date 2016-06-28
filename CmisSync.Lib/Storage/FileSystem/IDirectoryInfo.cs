//-----------------------------------------------------------------------
// <copyright file="IDirectoryInfo.cs" company="GRAU DATA AG">
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
    /// <summary>
    /// Interface to enable mocking of DirectoryInfo
    /// </summary>
    public interface IDirectoryInfo : IFileSystemInfo {
        /// <summary>
        /// Gets the parent.
        /// </summary>
        /// <value>The parent.</value>
        IDirectoryInfo Parent { get; }

        /// <summary>
        /// Gets the root of the directory.
        /// </summary>
        /// <value>The root.</value>
        IDirectoryInfo Root { get; }

        /// <summary>
        /// Gets or sets the ACL to mark the directory as move, rename and delete protected.
        /// This property works on NTFS only.
        /// </summary>
        bool CanMoveOrRenameOrDelete { get; set; }

        /// <summary>
        /// Creates a directory.
        /// </summary>
        void Create();

        /// <summary>
        /// Gets the child directories.
        /// </summary>
        /// <returns>The directories.</returns>
        IDirectoryInfo[] GetDirectories();

        /// <summary>
        /// Gets the containing files.
        /// </summary>
        /// <returns>The files.</returns>
        IFileInfo[] GetFiles();

        /// <summary>
        /// Delete the specified directory recursive if <c>true</c>.
        /// </summary>
        /// <param name="recursive">Deletes recursive if set to <c>true</c>.</param>
        void Delete(bool recursive);

        /// <summary>
        /// Moves the directory to the destination directory path.
        /// </summary>
        /// <param name="destDirName">Destination directory path.</param>
        void MoveTo(string destDirName);

        /// <summary>
        /// Tries to set permission to read write access to the directory and its children
        /// </summary>
        void TryToSetReadWritePermissionRecursively();
    }
}