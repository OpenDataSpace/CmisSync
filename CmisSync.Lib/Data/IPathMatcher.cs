//-----------------------------------------------------------------------
// <copyright file="IPathMatcher.cs" company="GRAU DATA AG">
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
namespace CmisSync.Lib.Data
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    
    using CmisSync.Lib.Storage;
    
    using DotCMIS.Client;

    /// <summary>
    /// Interface of a path matcher.
    /// A Path matcher is used to simplify the relation between a local and a remote file/folder.
    /// </summary>
    public interface IPathMatcher
    {
        /// <summary>
        /// Gets the local target root path.
        /// </summary>
        /// <value>The local target root path.</value>
        string LocalTargetRootPath { get; }

        /// <summary>
        /// Gets the remote target root path.
        /// </summary>
        /// <value>The remote target root path.</value>
        string RemoteTargetRootPath { get; }

        /// <summary>
        /// Determines whether this instance can create local path for specified remotePath.
        /// </summary>
        /// <returns><c>true</c> if this instance can create local path the specified remotePath; otherwise, <c>false</c>.</returns>
        /// <param name="remotePath">Remote path.</param>
        bool CanCreateLocalPath(string remotePath);

        /// <summary>
        /// Determines whether this instance can create local path for specified remoteFolder.
        /// </summary>
        /// <returns><c>true</c> if this instance can create local path the specified remoteFolder; otherwise, <c>false</c>.</returns>
        /// <param name="remoteFolder">Remote folder.</param>
        bool CanCreateLocalPath(IFolder remoteFolder);

        /// <summary>
        /// Determines whether this instance can create local path for specified remoteDocument.
        /// </summary>
        /// <returns><c>true</c> if this instance can create local path the specified remoteDocument; otherwise, <c>false</c>.</returns>
        /// <param name="remoteDocument">Remote document.</param>
        bool CanCreateLocalPath(IDocument remoteDocument);

        /// <summary>
        /// Determines whether this instance can create remote path for specified localFile.
        /// </summary>
        /// <returns><c>true</c> if this instance can create remote path for specified localFile; otherwise, <c>false</c>.</returns>
        /// <param name="localFile">Local file.</param>
        bool CanCreateRemotePath(FileInfo localFile);

        /// <summary>
        /// Determines whether this instance can create remote path for specified localDirectory.
        /// </summary>
        /// <returns><c>true</c> if this instance can create remote path for specified localDirectory; otherwise, <c>false</c>.</returns>
        /// <param name="localDirectory">Local directory.</param>
        bool CanCreateRemotePath(DirectoryInfo localDirectory);

        /// <summary>
        /// Determines whether this instance can create remote path for specified localPath.
        /// </summary>
        /// <returns><c>true</c> if this instance can create remote path for specified localPath; otherwise, <c>false</c>.</returns>
        /// <param name="localPath">Local path.</param>
        bool CanCreateRemotePath(string localPath);

        /// <summary>
        /// Matches the specified localPath and remotePath.
        /// </summary>
        /// <param name="localPath">Local path.</param>
        /// <param name="remotePath">Remote path.</param>
        /// <returns><c>true</c> if both paths matches</returns>
        bool Matches(string localPath, string remotePath);

        /// <summary>
        /// Matches the specified localPath and remoteFolder.
        /// </summary>
        /// <param name="localPath">Local path.</param>
        /// <param name="remoteFolder">Remote folder.</param>
        /// <returns><c>true</c> if both paths matches</returns>
        bool Matches(string localPath, IFolder remoteFolder);

        /// <summary>
        /// Matches the specified localFolder and remoteFolder.
        /// </summary>
        /// <param name="localFolder">Local folder.</param>
        /// <param name="remoteFolder">Remote folder.</param>
        /// <returns><c>true</c> if both paths matches</returns>
        bool Matches(IDirectoryInfo localFolder, IFolder remoteFolder);

        /// <summary>
        /// Creates the corresponding local path.
        /// </summary>
        /// <returns>The local path.</returns>
        /// <param name="remoteFolder">Remote folder.</param>
        string CreateLocalPath(IFolder remoteFolder);

        /// <summary>
        /// Creates the corresponding local path.
        /// </summary>
        /// <returns>The local path.</returns>
        /// <param name="remoteDocument">Remote document.</param>
        string CreateLocalPath(IDocument remoteDocument);

        /// <summary>
        /// Creates the corresponding local paths.
        /// </summary>
        /// <returns>The local paths.</returns>
        /// <param name="remoteDocument">Remote document.</param>
        List<string> CreateLocalPaths(IDocument remoteDocument);

        /// <summary>
        /// Creates the corresponding local path.
        /// </summary>
        /// <returns>The local path.</returns>
        /// <param name="remotePath">Remote path.</param>
        string CreateLocalPath(string remotePath);

        /// <summary>
        /// Creates the corresponding remote path.
        /// </summary>
        /// <returns>The remote path.</returns>
        /// <param name="localDirectory">Local directory.</param>
        string CreateRemotePath(DirectoryInfo localDirectory);

        /// <summary>
        /// Creates the corresponding remote path.
        /// </summary>
        /// <returns>The remote path.</returns>
        /// <param name="localFile">Local file.</param>
        string CreateRemotePath(FileInfo localFile);

        /// <summary>
        /// Creates the corresponding remote path.
        /// </summary>
        /// <returns>The remote path.</returns>
        /// <param name="localPath">Local path.</param>
        string CreateRemotePath(string localPath);

        /// <summary>
        /// Gets the relative local path.
        /// </summary>
        /// <returns>The relative local path.</returns>
        /// <param name="localPath">Local path.</param>
        string GetRelativeLocalPath(string localPath);
    }
}
