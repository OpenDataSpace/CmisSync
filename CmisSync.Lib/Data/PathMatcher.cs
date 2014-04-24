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
using System;
using System.IO;
using System.Collections.Generic;

using CmisSync.Lib.Storage;

using DotCMIS.Client;

namespace CmisSync.Lib.Data
{
    public class PathMatcher : IPathMatcher
    {
                
        public string LocalTargetRootPath { get; private set; }

        public string RemoteTargetRootPath { get; private set; }

        public PathMatcher (string localTargetRootPath, string remoteTargetRootPath)
        {
            if (String.IsNullOrEmpty (localTargetRootPath))
                throw new ArgumentException ("Given local path is null or empty");
            if (String.IsNullOrEmpty (remoteTargetRootPath))
                throw new ArgumentException ("Given remote path is null or empty");
            LocalTargetRootPath = localTargetRootPath;
            RemoteTargetRootPath = remoteTargetRootPath;
        }
        
                public bool CanCreateLocalPath (IFolder remoteFolder)
        {
            return CanCreateLocalPath (remoteFolder.Path);
        }

        public bool CanCreateLocalPath (IDocument remoteDocument)
        {
            foreach (string remotePath in remoteDocument.Paths) {
                if (CanCreateLocalPath (remotePath))
                    return true;
            }
            return false;
        }

        public bool CanCreateRemotePath (FileInfo localFile)
        {
            return CanCreateRemotePath (localFile.FullName);
        }

        public bool CanCreateRemotePath (DirectoryInfo localDirectory)
        {
            return CanCreateRemotePath (localDirectory.FullName);
        }


        public bool Matches (string localPath, IFolder remoteFolder)
        {
            return Matches (localPath, remoteFolder.Path);
        }

        public bool Matches (IDirectoryInfo localFolder, IFolder remoteFolder)
        {
            return Matches (localFolder.FullName, remoteFolder.Path);
        }

        public string CreateLocalPath (IFolder remoteFolder)
        {
            return CreateLocalPath (remoteFolder.Path);
        }

        public string CreateLocalPath (IDocument remoteDocument)
        {
            return CreateLocalPaths (remoteDocument) [0];
        }


        public string CreateRemotePath (DirectoryInfo localDirectory)
        {
            return CreateRemotePath (localDirectory.FullName);
        }

        public string CreateRemotePath (FileInfo localFile)
        {
            return CreateRemotePath (localFile.FullName);
        }

        public bool CanCreateLocalPath (string remotePath)
        {
            return remotePath.StartsWith (RemoteTargetRootPath);
        }

        public bool CanCreateRemotePath (string localPath)
        {
            return localPath.StartsWith (this.LocalTargetRootPath);
        }

        public bool Matches (string localPath, string remotePath)
        {
            if (!localPath.StartsWith (this.LocalTargetRootPath))
                throw new ArgumentOutOfRangeException (String.Format ("The given local path \"{0}\"does not start with the correct path \"{1}\"", localPath, this.LocalTargetRootPath));
            return localPath.Equals (CreateLocalPath (remotePath));
        }

        public List<string> CreateLocalPaths (IDocument remoteDocument)
        {
            if (!CanCreateLocalPath (remoteDocument)) 
                throw new ArgumentOutOfRangeException (String.Format ("Given remote document with Paths \"{0}\" has no path in the remote target folder \"{1}\"", remoteDocument.Paths, RemoteTargetRootPath));
            List<string> localPaths = new List<string> ();
            foreach (string remotePath in remoteDocument.Paths) {
                try {
                    localPaths.Add (CreateLocalPath (remotePath));
                } catch (ArgumentException) {
                }
            }
            return localPaths;
        }

        public string CreateLocalPath(string remotePath)
        {
            if (!CanCreateLocalPath (remotePath))
                throw new ArgumentOutOfRangeException (String.Format ("Given remote object with Path \"{0}\" is not in the remote target folder \"{1}\"", remotePath, RemoteTargetRootPath));
            string relativePath = remotePath.Substring (this.RemoteTargetRootPath.Length);
            relativePath = (relativePath.Length > 0 && relativePath [0] == '/') ? relativePath.Substring (1) : relativePath;
            return Path.Combine (this.LocalTargetRootPath, Path.Combine (relativePath.Split ('/')));
        }

        public string CreateRemotePath(string localPath)
        {
            if (!CanCreateRemotePath (localPath))
                throw new ArgumentOutOfRangeException (String.Format ("Given local path \"{0}\" does not start with the correct path \"{1}\"", localPath, this.LocalTargetRootPath));
            string relativePath = localPath.Substring (this.LocalTargetRootPath.Length);
            if (relativePath.Length == 0)
                return this.RemoteTargetRootPath;
            relativePath = (relativePath.Length > 0 && relativePath [0] == Path.DirectorySeparatorChar) ? relativePath.Substring (1) : relativePath;
            relativePath = relativePath.Replace (Path.DirectorySeparatorChar, '/');
            return String.Format ("{0}/{1}", this.RemoteTargetRootPath, relativePath);
        }

        public string GetRelativeLocalPath(string localPath)
        {
            if(!CanCreateRemotePath(localPath))
            {
                throw new ArgumentOutOfRangeException(String.Format("Given local path \"{0}\" does not start with the correct path \"{1}\"",localPath, this.LocalTargetRootPath));
            }
            string relativePath = localPath.Substring (this.LocalTargetRootPath.Length);
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

