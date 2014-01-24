using System;
using System.IO;
using System.Collections.Generic;

using DotCMIS.Client;

using CmisSync.Lib.Data;

namespace CmisSync.Lib.Storage
{
    public class MetaDataStorage : IMetaDataStorage
    {

        public MappedFolder RootFolder { get; set; }

        // Key is the remote object id, the value is the saved MappedObject containing the id
        private Dictionary<string, AbstractMappedObject> MappedObjects = new Dictionary<string, AbstractMappedObject>();

        private string RemoteSyncTargetPath { get; set; }
        private string LocalSyncTargetPath { get; set; }

        public IPathMatcher Matcher { get; private set;}

        public MetaDataStorage (IPathMatcher matcher)
        {
            if(matcher == null)
                throw new ArgumentNullException("Given Path matcher is null");
            Matcher = matcher;
        }

        /// <summary>
        /// Add a file to the storage.
        /// </summary>
        public void AddFile(FileInfo localFile, IDocument remoteFile) {
/*            if(!Matcher.Matches(localFile, remoteFile))
                throw new ArgumentException(String.Format("Given file paths are not matching each other: local=\"{0}\" remote=\"{1}\"", localFile.FullName, remoteFile.Paths));
*/
        }

        /// <summary>
        /// Add a folder to the storage.
        /// </summary>
        public void AddFolder(DirectoryInfo localFolder, IFolder remoteFolder) {
            if(!Matcher.Matches(localFolder, remoteFolder))
                throw new ArgumentException(String.Format("Given folder paths are not matching each other: local=\"{0}\" remote=\"{1}\"", localFolder.FullName, remoteFolder.Path));
            if(!ContainsFolder(localFolder.Parent.FullName))
                throw new ArgumentException();
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
        public void MoveFolder(string oldPath, string newPath) {
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
            MappedFile file;
            if(TryGetMappedFileByRemoteId(remoteDocument.Id, out file)) {
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
            MappedFile file;
            if( TryGetMappedFileByRemoteId(id, out file) ) {
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
        /// Checks whether the storage contains a given folder.
        /// </summary>
        public bool ContainsFolder(DirectoryInfo folder) {
            return ContainsFolder(folder.FullName);
        }

        /// <summary>
        /// <returns>path field in folders table for <paramref name="id"/></returns>
        /// </summary>
        public string GetFolderPath(string id) {
            MappedFolder folder;
            if( TryGetMappedFolderByRemoteId(id, out folder)) {
                return folder.GetLocalPath();
            } else {
                return null;
            }
        }

        public bool TryGetMappedFolder (DirectoryInfo localFolder, out MappedFolder savedFolder)
        {
            throw new NotImplementedException ();
        }

        public bool TryGetMappedObjectByRemoteId(string remoteId, out AbstractMappedObject savedObject)
        {
            return MappedObjects.TryGetValue(remoteId, out savedObject);
        }

        public bool TryGetMappedFileByRemoteId(string remoteId, out MappedFile savedFile)
        {
            AbstractMappedObject value;
            if(MappedObjects.TryGetValue(remoteId, out value)) {
                if(value is MappedFile) {
                    savedFile = value as MappedFile;
                    return true;
                }
            }
            savedFile = null;
            return false;
        }

        public bool TryGetMappedFolderByRemoteId(string remoteId, out MappedFolder savedFolder)
        {
            AbstractMappedObject value;
            if(MappedObjects.TryGetValue(remoteId, out value)) {
                if(value is MappedFolder) {
                    savedFolder = value as MappedFolder;
                    return true;
                }
            }
            savedFolder = null;
            return false;
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

