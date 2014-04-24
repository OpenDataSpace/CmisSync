using System;
using System.IO;
using System.Collections.Generic;

using CmisSync.Lib.Storage;

using DotCMIS.Client;

namespace CmisSync.Lib.Data
{
    public class PathMatcher : AbstractPathMatcher
    {
        public PathMatcher (string localTargetRootPath, string remoteTargetRootPath) : base(localTargetRootPath, remoteTargetRootPath)
        {
        }

        public override bool CanCreateLocalPath (string remotePath)
        {
            return remotePath.StartsWith (RemoteTargetRootPath);
        }

        public override bool CanCreateRemotePath (string localPath)
        {
            return localPath.StartsWith (this.LocalTargetRootPath);
        }

        public override bool Matches (string localPath, string remotePath)
        {
            if (!localPath.StartsWith (this.LocalTargetRootPath))
                throw new ArgumentOutOfRangeException (String.Format ("The given local path \"{0}\"does not start with the correct path \"{1}\"", localPath, this.LocalTargetRootPath));
            return localPath.Equals (CreateLocalPath (remotePath));
        }

        public override List<string> CreateLocalPaths (IDocument remoteDocument)
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

        public override string CreateLocalPath(string remotePath)
        {
            if (!CanCreateLocalPath (remotePath))
                throw new ArgumentOutOfRangeException (String.Format ("Given remote object with Path \"{0}\" is not in the remote target folder \"{1}\"", remotePath, RemoteTargetRootPath));
            string relativePath = remotePath.Substring (this.RemoteTargetRootPath.Length);
            relativePath = (relativePath.Length > 0 && relativePath [0] == '/') ? relativePath.Substring (1) : relativePath;
            return Path.Combine (this.LocalTargetRootPath, Path.Combine (relativePath.Split ('/')));
        }

        public override string CreateRemotePath(string localPath)
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

        public override string GetRelativeLocalPath(string localPath)
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

