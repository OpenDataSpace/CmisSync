//-----------------------------------------------------------------------
// <copyright file="PathMatcher.cs" company="GRAU DATA AG">
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
    /// Path matcher.
    /// </summary>
    public class PathMatcher : IPathMatcher
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Data.PathMatcher"/> class.
        /// </summary>
        /// <param name="localTargetRootPath">Local target root path.</param>
        /// <param name="remoteTargetRootPath">Remote target root path.</param>
        public PathMatcher(string localTargetRootPath, string remoteTargetRootPath)
        {
            if (string.IsNullOrEmpty(localTargetRootPath))
            {
                throw new ArgumentException("Given local path is null or empty");
            }

            if (string.IsNullOrEmpty(remoteTargetRootPath))
            {
                throw new ArgumentException("Given remote path is null or empty");
            }

            if (!localTargetRootPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                localTargetRootPath += Path.DirectorySeparatorChar.ToString();
            }

            if (!remoteTargetRootPath.EndsWith("/"))
            {
                remoteTargetRootPath += "/";
            }

            this.LocalTargetRootPath = localTargetRootPath;
            this.RemoteTargetRootPath = remoteTargetRootPath;
        }

        /// <summary>
        /// Gets the local target root path.
        /// </summary>
        /// <value>The local target root path.</value>
        public string LocalTargetRootPath { get; private set; }

        /// <summary>
        /// Gets the remote target root path.
        /// </summary>
        /// <value>The remote target root path.</value>
        public string RemoteTargetRootPath { get; private set; }

        /// <summary>
        /// Determines whether this instance can create local path for specified remotePath.
        /// </summary>
        /// <returns>true if possible</returns>
        /// <param name="remoteFolder">Remote folder.</param>
        public bool CanCreateLocalPath(IFolder remoteFolder)
        {
            return this.CanCreateLocalPath(remoteFolder.Path);
        }

        /// <summary>
        /// Determines whether this instance can create local path for specified remotePath.
        /// </summary>
        /// <returns>true if possible</returns>
        /// <param name="remoteDocument">Remote document.</param>
        public bool CanCreateLocalPath(IDocument remoteDocument)
        {
            foreach (string remotePath in remoteDocument.Paths)
            {
                if (this.CanCreateLocalPath(remotePath))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines whether this instance can create remote path for specified localFile.
        /// </summary>
        /// <returns>true if possible</returns>
        /// <param name="localFile">Local file.</param>
        public bool CanCreateRemotePath(FileInfo localFile)
        {
            return this.CanCreateRemotePath(localFile.FullName);
        }

        /// <summary>
        /// Determines whether this instance can create remote path for specified localFile.
        /// </summary>
        /// <returns>true if possible</returns>
        /// <param name="localDirectory">Local directory.</param>
        public bool CanCreateRemotePath(DirectoryInfo localDirectory)
        {
            return this.CanCreateRemotePath(localDirectory.FullName);
        }

        /// <summary>
        /// Matches the specified localPath and remotePath.
        /// </summary>
        /// <param name="localPath">Local path.</param>
        /// <returns>true if matches</returns>
        /// <param name="remoteFolder">Remote folder.</param>
        public bool Matches(string localPath, IFolder remoteFolder)
        {
            return this.Matches(localPath, remoteFolder.Path);
        }

        /// <summary>
        /// Matches the specified localPath and remotePath.
        /// </summary>
        /// <returns>true if matches</returns>
        /// <param name="localFolder">Local folder.</param>
        /// <param name="remoteFolder">Remote folder.</param>
        public bool Matches(IDirectoryInfo localFolder, IFolder remoteFolder)
        {
            return this.Matches(localFolder.FullName, remoteFolder.Path);
        }

        /// <summary>
        /// Creates the corresponding local path.
        /// </summary>
        /// <returns>The local path.</returns>
        /// <param name="remoteFolder">Remote folder.</param>
        public string CreateLocalPath(IFolder remoteFolder)
        {
            return this.CreateLocalPath(remoteFolder.Path);
        }

        /// <summary>
        /// Creates the corresponding local path.
        /// </summary>
        /// <returns>The local path.</returns>
        /// <param name="remoteDocument">Remote document.</param>
        public string CreateLocalPath(IDocument remoteDocument)
        {
            return this.CreateLocalPaths(remoteDocument)[0];
        }

        /// <summary>
        /// Creates the remote pat.
        /// </summary>
        /// <returns>The remote pat.</returns>
        /// <param name="localDirectory">Local directory.</param>
        public string CreateRemotePath(DirectoryInfo localDirectory)
        {
            return this.CreateRemotePath(localDirectory.FullName);
        }

        /// <summary>
        /// Creates the corresponding remote path.
        /// </summary>
        /// <returns>The remote path.</returns>
        /// <param name="localFile">Local file.</param>
        public string CreateRemotePath(FileInfo localFile)
        {
            return this.CreateRemotePath(localFile.FullName);
        }

        /// <summary>
        /// Determines whether this instance can create local path for specified remotePath.
        /// </summary>
        /// <returns>true if possible</returns>
        /// <param name="remotePath">Remote path.</param>
        public bool CanCreateLocalPath(string remotePath)
        {
            if(!remotePath.EndsWith("/")) {
                remotePath += "/";
            }

            return remotePath.StartsWith(this.RemoteTargetRootPath);
        }

        /// <summary>
        /// Determines whether this instance can create remote path for specified localFile.
        /// </summary>
        /// <returns>true if possible</returns>
        /// <param name="localPath">Local path.</param>
        public bool CanCreateRemotePath(string localPath)
        {
            if(!localPath.EndsWith(Path.DirectorySeparatorChar.ToString())) {
                localPath += Path.DirectorySeparatorChar.ToString();
            }

            return localPath.StartsWith(this.LocalTargetRootPath);
        }

        /// <summary>
        /// Matches the specified localPath and remotePath.
        /// </summary>
        /// <param name="localPath">Local path.</param>
        /// <param name="remotePath">Remote path.</param>
        /// <returns>true if the paths matches</returns>
        public bool Matches(string localPath, string remotePath)
        {
            if (!this.CanCreateRemotePath(localPath))
            {
                throw new ArgumentOutOfRangeException(string.Format("The given local path \"{0}\"does not start with the correct path \"{1}\"", localPath, this.LocalTargetRootPath));
            }

            return localPath.Equals(this.CreateLocalPath(remotePath));
        }

        /// <summary>
        /// Creates the corresponding local paths.
        /// </summary>
        /// <returns>The local paths.</returns>
        /// <param name="remoteDocument">Remote document.</param>
        public List<string> CreateLocalPaths(IDocument remoteDocument)
        {
            if (!this.CanCreateLocalPath(remoteDocument))
            {
                throw new ArgumentOutOfRangeException(string.Format("Given remote document with Paths \"{0}\" has no path in the remote target folder \"{1}\"", remoteDocument.Paths, this.RemoteTargetRootPath));
            }

            List<string> localPaths = new List<string>();
            foreach (string remotePath in remoteDocument.Paths)
            {
                try
                {
                    localPaths.Add(this.CreateLocalPath(remotePath));
                }
                catch (ArgumentException) {
                }
            }

            return localPaths;
        }

        /// <summary>
        /// Creates the corresponding local path.
        /// </summary>
        /// <returns>The local path.</returns>
        /// <param name="remotePath">Remote path.</param>
        public string CreateLocalPath(string remotePath)
        {
            if (!this.CanCreateLocalPath(remotePath))
            {
                throw new ArgumentOutOfRangeException(string.Format("Given remote object with Path \"{0}\" is not in the remote target folder \"{1}\"", remotePath, this.RemoteTargetRootPath));
            }

            if(this.RemoteTargetRootPath.Equals(remotePath + "/")) {
                return this.LocalTargetRootPath;
            }
                
            string relativePath = remotePath.Substring(this.RemoteTargetRootPath.Length);
            relativePath = (relativePath.Length > 0 && relativePath[0] == '/') ? relativePath.Substring(1) : relativePath;
            return Path.Combine(this.LocalTargetRootPath, Path.Combine(relativePath.Split('/')));
        }

        /// <summary>
        /// Creates the corresponding remote path.
        /// </summary>
        /// <returns>The remote path.</returns>
        /// <param name="localPath">Local path.</param>
        public string CreateRemotePath(string localPath)
        {
            if (!this.CanCreateRemotePath(localPath))
            {
                throw new ArgumentOutOfRangeException(string.Format("Given local path \"{0}\" does not start with the correct path \"{1}\"", localPath, this.LocalTargetRootPath));
            }

            string relativePath = localPath.Substring(this.LocalTargetRootPath.Length);
            if (relativePath.Length == 0)
            {
                return this.RemoteTargetRootPath;
            }

            relativePath = (relativePath.Length > 0 && relativePath[0] == Path.DirectorySeparatorChar) ? relativePath.Substring(1) : relativePath;
            relativePath = relativePath.Replace(Path.DirectorySeparatorChar, '/');
            return string.Format("{0}{1}", this.RemoteTargetRootPath, relativePath);
        }

        /// <summary>
        /// Gets the relative local path.
        /// </summary>
        /// <returns>The relative local path.</returns>
        /// <param name="localPath">Local path.</param>
        public string GetRelativeLocalPath(string localPath)
        {
            if(!this.CanCreateRemotePath(localPath))
            {
                throw new ArgumentOutOfRangeException(string.Format("Given local path \"{0}\" does not start with the correct path \"{1}\"", localPath, this.LocalTargetRootPath));
            }

            if(this.LocalTargetRootPath.Equals(localPath + Path.DirectorySeparatorChar.ToString())) {
                return ".";
            }

            string relativePath = localPath.Substring(this.LocalTargetRootPath.Length);
            if (relativePath.StartsWith(Path.DirectorySeparatorChar.ToString())) {
                relativePath = relativePath.Substring(1);
            }

            if (relativePath.Length == 0)
            {
                return ".";
            }
            else
            {
                return relativePath;
            }
        }
    }
}
