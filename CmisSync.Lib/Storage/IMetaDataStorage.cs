using System;
using System.IO;
using System.Collections.Generic;

using DotCMIS.Client;

using CmisSync.Lib.Data;

namespace CmisSync.Lib.Storage
{
    public interface IMetaDataStorage
    {

        MappedFolder RootFolder { get; set; }

        IPathMatcher Matcher { get; }

        /// <summary>
        /// Add a file to the storage.
        /// </summary>
        void AddFile(FileInfo localFile, IDocument remoteFile);

        /// <summary>
        /// Add a folder to the storage.
        /// </summary>
        void AddFolder(DirectoryInfo localFolder, IFolder remoteFolder);

        /// <summary>
        /// Remove a file from the storage.
        /// </summary>
        void RemoveFile(string path);


        /// <summary>
        /// move a file from the storage.
        /// </summary>
        void MoveFile(string oldPath, string newPath);


        /// <summary>
        /// Remove a folder from the storage.
        /// </summary>
        void RemoveFolder(string path);


        /// <summary>
        /// move a folder from the storage.
        /// </summary>
        void MoveFolder(string oldPath, string newPath);


        /// <summary>
        /// Get the time at which the file was last modified.
        /// This is the time on the CMIS server side, in UTC. Client-side time does not matter.
        /// </summary>
        DateTime? GetServerSideModificationDate(string path);


        /// <summary>
        /// Set the last modification date of a file.
        /// This is the time on the CMIS server side, in UTC. Client-side time does not matter.
        /// </summary>
        void SetFileServerSideModificationDate(IDocument remoteDocument);

        /// <summary>
        /// Checks whether the storage contains a given file.
        /// </summary>
        bool ContainsFile(string path);

        /// <summary>
        /// <returns>path field in files table for <paramref name="id"/></returns>
        /// </summary>
        string GetFilePath(string id);

        /// <summary>
        /// Checks whether the storage contains a given folder.
        /// </summary>
        bool ContainsFolder(string path);

        /// <summary>
        /// Checks whether the storage contains a given folder.
        /// </summary>
        bool ContainsFolder(DirectoryInfo folder);

        /// <summary>
        /// <returns>path field in folders table for <paramref name="id"/></returns>
        /// </summary>
        string GetFolderPath(string id);

        bool TryGetMappedFolder (DirectoryInfo localFolder, out MappedFolder savedFolder);

        bool TryGetMappedObjectByRemoteId(string remoteId, out AbstractMappedObject savedObject);

        bool TryGetMappedFileByRemoteId(string remoteId, out MappedFile savedFile);

        bool TryGetMappedFolderByRemoteId(string remoteId, out MappedFolder savedFolder);

        /// <summary>
        /// Check whether a file's content has changed locally since it was last synchronized.
        /// This happens when the user edits a file on the local computer.
        /// This method does not communicate with the CMIS server, it just checks whether the checksum has changed.
        /// </summary>
        bool LocalFileHasChanged(string path);

        string CreatePathFromRemoteFolder (IFolder remoteFolder);
    }
}

