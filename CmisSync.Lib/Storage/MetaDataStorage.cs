using System;
using System.IO;
using System.Collections.Generic;

using DotCMIS.Client;

using CmisSync.Lib.Data;

namespace CmisSync.Lib.Storage
{
    public class MetaDataStorage
    {

        public SyncFolder RootFolder { get; set; }

        private Dictionary<string, SyncFolder> AllFolder = new Dictionary<string, SyncFolder>();
        private Dictionary<string, SyncFile> AllFiles = new Dictionary<string, SyncFile>();

        private string RemoteSyncTargetPath { get; set; }
        private string LocalSyncTargetPath { get; set; }

        public MetaDataStorage (string remoteSyncTargetPath, string localSyncTargetPath)
        {

        }

        /// <summary>
        /// Add a file to the storage.
        /// If checksum is not null, it will be used for the storage entry
        /// </summary>
        public void AddFile(FileInfo localFile, IDocument remoteFile) {

        }

        /// <summary>
        /// Add a folder to the storage.
        /// </summary>
        public void AddFolder(DirectoryInfo localFolder, IFolder remoteFolder) {

        }

        /// <summary>
        /// Remove a file from the storage.
        /// </summary>
        public void RemoveFile(string path) {
            throw new NotImplementedException();
        }


        /// <summary>
        /// move a file from the storage.
        /// </summary>
        public void MoveFile(string oldPath, string newPath) {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Remove a folder from the storage.
        /// </summary>
        public void RemoveFolder(string path) {
            throw new NotImplementedException();
        }


        /// <summary>
        /// move a folder from the storage.
        /// </summary>
        void MoveFolder(string oldPath, string newPath) {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Get the time at which the file was last modified.
        /// This is the time on the CMIS server side, in UTC. Client-side time does not matter.
        /// </summary>
        public DateTime? GetServerSideModificationDate(string path) {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Set the last modification date of a file.
        /// This is the time on the CMIS server side, in UTC. Client-side time does not matter.
        /// </summary>
        public void SetFileServerSideModificationDate(IDocument remoteDocument) {
            SyncFile file;
            if(this.AllFiles.TryGetValue(remoteDocument.Id, out file)) {
                file.LastRemoteWriteTimeUtc = ((DateTime)remoteDocument.LastModificationDate).ToUniversalTime();
            } else {
                throw new ArgumentException();
            }
        }

        /// <summary>
        /// Checks whether the storage contains a given file.
        /// </summary>
        public bool ContainsFile(string path) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// <returns>path field in files table for <paramref name="id"/></returns>
        /// </summary>
        public string GetFilePath(string id) {
            SyncFile file;
            if( this.AllFiles.TryGetValue(id, out file) ) {
                return file.GetLocalPath();
            }else {
                return null;
            }
        }

        /// <summary>
        /// Checks whether the storage contains a given folder.
        /// </summary>
        public bool ContainsFolder(string path) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// <returns>path field in folders table for <paramref name="id"/></returns>
        /// </summary>
        public string GetFolderPath(string id) {
            SyncFolder folder;
            if( this.AllFolder.TryGetValue(id, out folder)) {
                return folder.GetLocalPath();
            } else {
                return null;
            }
        }

        /// <summary>
        /// Check whether a file's content has changed locally since it was last synchronized.
        /// This happens when the user edits a file on the local computer.
        /// This method does not communicate with the CMIS server, it just checks whether the checksum has changed.
        /// </summary>
        public bool LocalFileHasChanged(string path) {
            throw new NotImplementedException();
        }

        public string CreatePathFromRemoteFolder (IFolder remoteFolder)
        {
            string remotePath = remoteFolder.Path;
            if(remotePath.Length <= RemoteSyncTargetPath.Length)
                return null;
            string relativePath = remotePath.Substring(RemoteSyncTargetPath.Length);
            if (relativePath[0] == '/')
            {
                relativePath = relativePath.Substring(1);
            }
            return Path.Combine(RemoteSyncTargetPath, relativePath).Replace('/', Path.DirectorySeparatorChar);
        }
    }
}

