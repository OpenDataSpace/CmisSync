namespace CmisSync.Lib.Data
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    
    using CmisSync.Lib.Storage;
    
    using DotCMIS.Client;
    
    public abstract class AbstractPathMatcher : IPathMatcher
    {
        public string LocalTargetRootPath { get; private set; }

        public string RemoteTargetRootPath { get; private set; }

        public AbstractPathMatcher (string localTargetRootPath, string remoteTargetRootPath)
        {
            if (String.IsNullOrEmpty (localTargetRootPath))
                throw new ArgumentException ("Given local path is null or empty");
            if (String.IsNullOrEmpty (remoteTargetRootPath))
                throw new ArgumentException ("Given remote path is null or empty");
            LocalTargetRootPath = localTargetRootPath;
            RemoteTargetRootPath = remoteTargetRootPath;
        }

        public abstract bool CanCreateLocalPath (string remotePath);

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

        public abstract bool CanCreateRemotePath (string localPath);

        public abstract bool Matches (string localPath, string remotePath);

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

        public abstract List<string> CreateLocalPaths (IDocument remoteDocument);

        public abstract string CreateLocalPath (string remotePath);

        public string CreateRemotePath (DirectoryInfo localDirectory)
        {
            return CreateRemotePath (localDirectory.FullName);
        }

        public string CreateRemotePath (FileInfo localFile)
        {
            return CreateRemotePath (localFile.FullName);
        }

        public abstract string CreateRemotePath (string localPath);

        public abstract string GetRelativeLocalPath(string localPath);

    }
}

