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

