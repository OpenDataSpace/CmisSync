namespace CmisSync.Lib.Data
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    
    using CmisSync.Lib.Storage;
    
    using DotCMIS.Client;
    
    public interface IPathMatcher
    {
        bool CanCreateLocalPath (string remotePath);

        bool CanCreateLocalPath (IFolder remoteFolder);

        bool CanCreateLocalPath (IDocument remoteDocument);

        bool CanCreateRemotePath (FileInfo localFile);

        bool CanCreateRemotePath (DirectoryInfo localDirectory);

        bool CanCreateRemotePath (string localPath);

        bool Matches (string localPath, string remotePath);

        bool Matches (string localPath, IFolder remoteFolder);

        bool Matches (IDirectoryInfo localFolder, IFolder remoteFolder);

        string CreateLocalPath (IFolder remoteFolder);

        string CreateLocalPath (IDocument remoteDocument);

        List<string> CreateLocalPaths (IDocument remoteDocument);

        string CreateLocalPath (string remotePath);

        string CreateRemotePath (DirectoryInfo localDirectory);

        string CreateRemotePath (FileInfo localFile);

        string CreateRemotePath (string localPath);

        string GetRelativeLocalPath(string localPath);

        string LocalTargetRootPath { get; }

        string RemoteTargetRootPath { get; }
    }
}

