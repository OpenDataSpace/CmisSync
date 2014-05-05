using System;
using System.IO;
using System.Collections.Generic;

using CmisSync.Lib.Cmis;
using CmisSync.Lib.Events;

using DotCMIS.Client;

namespace CmisSync.Lib.Sync.Strategy
{
    /*
    public class SimpleFileSynchronizer : AbstractFileSynchronizer
    {
        public SimpleFileSynchronizer ()
        {
        }


                    /// <summary>
            /// Download a single file from the CMIS server for sync.
            /// </summary>
            private bool SyncDownloadFile(IDocument remoteDocument, string localFolder, IList<string> remoteFiles = null)
            {
                string fileName = remoteDocument.Name;
                string filePath = Path.Combine(localFolder, fileName);

                // If this file does not have a filename, ignore it.
                // It sometimes happen on IBM P8 CMIS server, not sure why.
                if (remoteDocument.ContentStreamFileName == null)
                {
                    //TODO Possibly the file content has been changed to 0, this case should be handled
                    Logger.Warn("Skipping download of '" + fileName + "' with null content stream in " + localFolder);
                    return true;
                }

                if (null != remoteFiles)
                {
                    remoteFiles.Add(fileName);
                }

                // Check if file extension is allowed
                if (!Utils.WorthSyncing(fileName))
                {
                    Logger.Info("Ignore the unworth syncing remote file: " + fileName);
                    return true;
                }

                bool success = true;

                try
                {
                    if (File.Exists(filePath))
                    {
                        // Check modification date stored in database and download if remote modification date if different.
                        DateTime? serverSideModificationDate = ((DateTime)remoteDocument.LastModificationDate).ToUniversalTime();
                        DateTime? lastDatabaseUpdate = database.GetServerSideModificationDate(filePath);

                        if (lastDatabaseUpdate == null)
                        {
                            Logger.Info("Downloading file absent from database: " + filePath);
                            success = DownloadFile(remoteDocument, localFolder);
                        }
                        else
                        {
                            // If the file has been modified since last time we downloaded it, then download again.
                            if (serverSideModificationDate > lastDatabaseUpdate)
                            {
                                Logger.Info("Downloading modified file: " + fileName);
                                success = DownloadFile(remoteDocument, localFolder);
                            }
                            else if(serverSideModificationDate == lastDatabaseUpdate)
                            {
                                //  check chunked upload
                                FileInfo fileInfo = new FileInfo(filePath);
                                if (remoteDocument.ContentStreamLength < fileInfo.Length)
                                {
                                    success = UpdateFile(filePath, remoteDocument);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (database.ContainsFile(filePath))
                        {
                            long retries = database.GetOperationRetryCounter(filePath, Database.OperationType.DELETE);
                            if(retries <= repoinfo.MaxDeletionRetries) {
                                // File has been recently removed locally, so remove it from server too.
                                Logger.Info("Removing locally deleted file on server: " + filePath);
                                try{
                                    remoteDocument.DeleteAllVersions();
                                    // Remove it from database.
                                    database.RemoveFile(filePath);
                                    database.SetOperationRetryCounter(filePath, 0, Database.OperationType.DELETE);
                                } catch(CmisBaseException ex)
                                {
                                    Logger.Warn("Could not delete remote file: ", ex);
                                    database.SetOperationRetryCounter(filePath, retries+1, Database.OperationType.DELETE);
                                    throw;
                                }
                            } else {
                                Logger.Info(String.Format("Skipped deletion of remote file {0} because of too many failed retries ({1} max={2})", filePath, retries, repoinfo.MaxDeletionRetries));
                            }
                        }
                        else
                        {
                            // New remote file, download it.
                            Logger.Info("New remote file: " + filePath);
                            success = DownloadFile(remoteDocument, localFolder);
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Warn(String.Format("Exception while download file to {0}: {1}", filePath, Utils.ToLogString(e)));
                    success = false;
                }

                return success;
            }

                    /// <summary>
            /// Upload new version of file.
            /// </summary>
            private bool UpdateFile(string filePath, IDocument remoteFile)
            {
                long retries = database.GetOperationRetryCounter(filePath, Database.OperationType.UPLOAD);
                if(retries >= repoinfo.MaxUploadRetries)
                {
                    Logger.Info(String.Format("Skipping updating file content on repository, because of too many failed retries({0}): {1}", retries, filePath));
                    return true;
                }
                FileTransmissionEvent transmissionEvent = new FileTransmissionEvent(FileTransmissionType.UPLOAD_MODIFIED_FILE, filePath);
                try
                {
                    Logger.Info("## Updating " + filePath);
                    using (Stream localfile = File.OpenRead(filePath))
                    {
                        // Ignore files with null or empty content stream.
                        if ((localfile == null) && (localfile.Length == 0))
                        {
                            Logger.Info("Skipping update of file with null or empty content stream: " + filePath);
                            return true;
                        }
                        this.Queue.AddEvent(transmissionEvent);
                        IFileUploader uploader;
                        if (repoinfo.ChunkSize <= 0)
                            uploader = new SimpleFileUploader();
                        else
                            uploader = new ChunkedUploader(repoinfo.ChunkSize);
                        using (var hashAlg = new SHA1Managed()) {
                            IDocument lastState;
                            if(uploadProgresses.TryGetValue(filePath, out lastState)){
                                if(lastState.ChangeToken == remoteFile.ChangeToken && lastState.ContentStreamLength != null) {
                                    localfile.Seek((long) lastState.ContentStreamLength, SeekOrigin.Begin);
                                } else {
                                    uploadProgresses.Remove(filePath);
                                }
                            }
                            try{
                                uploader.UploadFile(remoteFile,localfile,transmissionEvent, hashAlg);
                                uploadProgresses.Remove(filePath);
                            }catch(UploadFailedException uploadException){
                                if(!uploadException.LastSuccessfulDocument.Equals(remoteFile)) {
                                    uploadProgresses.Add(filePath, uploadException.LastSuccessfulDocument);
                                }
                                throw;
                            }
                        }
                        // Update timestamp in database.
                        database.SetFileServerSideModificationDate(filePath, ((DateTime)remoteFile.LastModificationDate).ToUniversalTime());

                        // Update checksum
                        database.RecalculateChecksum(filePath);
                        transmissionEvent.ReportProgress(new TransmissionProgressEventArgs(){Completed = true});

                        // TODO Update metadata?
                    }
                    return true;
                }
                catch (Exception e)
                {
                    retries++;
                    database.SetOperationRetryCounter(filePath, retries, Database.OperationType.UPLOAD);
                    transmissionEvent.ReportProgress(new TransmissionProgressEventArgs(){Aborted = true});
                    Logger.Warn(String.Format("Updating content of {0} failed {1} times: ", filePath, retries), e);
                    return false;
                }
            }

        
            /// <summary>
            /// Upload a single file to the CMIS server.
            /// </summary>
            private bool UploadFile(string filePath, IFolder remoteFolder)
            {
                using (new ActivityListenerResource(activityListener))
                {
                    long retries = database.GetOperationRetryCounter(filePath, Database.OperationType.UPLOAD);
                    if(retries > this.repoinfo.MaxUploadRetries) {
                        Logger.Info(String.Format("Skipping uploading file absent on repository, because of too many failed retries({0}): {1}", retries, filePath));
                        return true;
                    }
                    FileTransmissionEvent transmissionEvent = new FileTransmissionEvent(FileTransmissionType.UPLOAD_NEW_FILE, filePath);
                    try{
                        IDocument remoteDocument = null;
                        Boolean success = false;
                        byte[] filehash = { };
                        try
                        {
                            Logger.Info("Uploading: " + filePath);
                            Queue.AddEvent(transmissionEvent);
                            // Prepare properties
                            string fileName = Path.GetFileName(filePath);
                            Dictionary<string, object> properties = new Dictionary<string, object>();
                            properties.Add(PropertyIds.Name, fileName);
                            properties.Add(PropertyIds.ObjectTypeId, "cmis:document");
                            properties.Add(PropertyIds.CreationDate, ((long)(File.GetCreationTimeUtc(filePath) - new DateTime(1970, 1, 1)).TotalMilliseconds).ToString());

                            // Prepare content stream
                            using (SHA1 hashAlg = new SHA1Managed())
                            using (Stream file = File.OpenRead(filePath))
                            {
                                IFileUploader uploader;
                                if(repoinfo.ChunkSize <= 0)
                                    uploader = new SimpleFileUploader();
                                else
                                    uploader = new ChunkedUploader(repoinfo.ChunkSize);

                                try {
                                    try {
                                        // Create Document
                                        remoteDocument = remoteFolder.CreateDocument(properties, null, null);
                                        Logger.Debug(String.Format("CMIS::Document Id={0} Name={1}",
                                                                   remoteDocument.Id, fileName));
                                        Dictionary<string, string[]> metadata = CmisUtils.FetchMetadata(remoteDocument, session.GetTypeDefinition(remoteDocument.ObjectType.Id));
                                        // Create database entry for this empty file to force content update if setContentStream will fail.
                                        database.AddFile(filePath, remoteDocument.Id, remoteDocument.LastModificationDate, metadata, new byte[hashAlg.HashSize]);
                                    } catch(Exception) {
                                        string reason = Utils.IsValidISO88591(fileName)?String.Empty:" Reason: Upload perhaps failed because of an invalid ISO 8859-1 character";
                                        Logger.Info(String.Format("Could not create the remote document {0} as target for local document {1}{2}", fileName, filePath, reason));
                                        throw;
                                    }
                                    // Upload
                                    try{
                                        IDocument lastState;
                                        if(uploadProgresses.TryGetValue(filePath, out lastState)){
                                            if(lastState.ChangeToken == remoteDocument.ChangeToken && lastState.ContentStreamLength != null) {
                                                file.Seek((long) lastState.ContentStreamLength, SeekOrigin.Begin);
                                            } else {
                                                uploadProgresses.Remove(filePath);
                                            }
                                        }
                                        uploader.UploadFile(remoteDocument, file, transmissionEvent, hashAlg);
                                    }catch(UploadFailedException uploadException) {
                                        // Check if upload was partly successful and save this state
                                        if(!uploadException.LastSuccessfulDocument.Equals(remoteDocument)) {
                                            uploadProgresses.Add(filePath, uploadException.LastSuccessfulDocument);
                                        }
                                        throw;
                                    }
                                    uploadProgresses.Remove(filePath);
                                    filehash = hashAlg.Hash;
                                    success = true;
                                }catch (Exception ex) {
                                    Logger.Fatal("Upload failed: " + filePath + " " + ex);
                                    throw;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            if (e is FileNotFoundException ||
                                e is IOException)
                            {
                                Logger.Warn("File deleted while trying to upload it, reverting.");
                                // File has been deleted while we were trying to upload/checksum/add.
                            // This can typically happen in Windows Explore when creating a new text file and giving it a name.
                            // In this case, revert the upload.
                                if (remoteDocument != null)
                                {
                                    remoteDocument.DeleteAllVersions();
                                }
                            }
                            else
                            {
                                throw;
                            }
                        }

                        // Metadata.
                        if (success)
                        {
                            Logger.Info("Uploaded: " + filePath);

                            // Get metadata. Some metadata has probably been automatically added by the server.
                            Dictionary<string, string[]> metadata = CmisUtils.FetchMetadata(remoteDocument, session.GetTypeDefinition(remoteDocument.ObjectType.Id));

                            // Create database entry for this file.
                            database.AddFile(filePath, remoteDocument.Id, remoteDocument.LastModificationDate, metadata, filehash);
                            CmisUtils.SetLastModifiedDate(remoteDocument, filePath, metadata);
                            Queue.AddEvent(new RecentChangedEvent(filePath));
                            transmissionEvent.ReportProgress(new TransmissionProgressEventArgs(){Completed = true});
                        } else {
                            transmissionEvent.ReportProgress(new TransmissionProgressEventArgs(){Aborted = true});
                        }
                        return success;
                    }
                    catch(Exception e)
                    {
                        retries++;
                        database.SetOperationRetryCounter(filePath, retries, Database.OperationType.UPLOAD);
                        Logger.Warn(String.Format("Uploading of {0} failed {1} times: ", filePath, retries), e);
                        transmissionEvent.ReportProgress(new TransmissionProgressEventArgs(){ Aborted=true, FailedException = e});
                        return false;
                    }
                }
            }

                    /// <summary>
            /// Download a single file from the CMIS server.
            /// </summary>
            private bool DownloadFile(IDocument remoteDocument, string localFolder)
            {
                RequestFileDownload(remoteDocument, localFolder);
                using (new ActivityListenerResource(activityListener))
                {
                    string fileName = remoteDocument.Name;
                    string filepath = Path.Combine(localFolder, fileName);
                    string tmpfilepath = filepath + ".sync";
                    FileTransmissionEvent transmissionEvent = new FileTransmissionEvent(FileTransmissionType.DOWNLOAD_NEW_FILE, filepath, tmpfilepath);

                    // Skip if invalid file name. See https://github.com/nicolas-raoul/CmisSync/issues/196
                    if (Utils.IsInvalidFileName(fileName))
                    {
                        Logger.Info("Skipping download of file with illegal filename: " + fileName);
                        return true;
                    }

                    try
                    {
                        long failedCounter = database.GetOperationRetryCounter(filepath,Database.OperationType.DOWNLOAD);
                        if( failedCounter > repoinfo.MaxDownloadRetries)
                        {
                            Logger.Info(String.Format("Skipping download of file {0} because of too many failed ({1}) downloads", filepath, failedCounter));
                            return true;
                        }
                        // Break and warn if download target exists as folder.
                        if (Directory.Exists(filepath))
                        {
                            throw new IOException(String.Format("Cannot download file \"{0}\", because a folder with this name exists locally", filepath));
                        }

                        // Break and warn if download target exists as folder.
                        if (Directory.Exists(tmpfilepath))
                        {
                            throw new IOException(String.Format("Cannot download file \"{0}\", because a folder with this name exists locally", tmpfilepath));
                        }

                        if (File.Exists(tmpfilepath))
                        {
                            DateTime? remoteDate = remoteDocument.LastModificationDate;
                            if (null == remoteDate)
                            {
                                File.Delete(tmpfilepath);
                            }
                            else
                            {
                                remoteDate = ((DateTime)remoteDate).ToUniversalTime();
                                DateTime? serverDate = database.GetDownloadServerSideModificationDate(filepath);
                                if (remoteDate != serverDate)
                                {
                                    File.Delete(tmpfilepath);
                                }
                            }
                        }
                        database.SetDownloadServerSideModificationDate(filepath, remoteDocument.LastModificationDate);
                        Tasks.IFileDownloader downloader;
                        if (repoinfo.DownloadChunkSize <= 0){
                            Logger.Debug("Simple File Downloader");
                            downloader = new Tasks.SimpleFileDownloader();
                        }else {
                            Logger.Debug("Chunked File Downloader");
                            downloader = new Tasks.ChunkedDownloader(repoinfo.DownloadChunkSize);
                        }
                            

                        // Download file.
                        Boolean success = false;
                        byte[] filehash = { };

                        try{
                            HashAlgorithm hashAlg = new SHA1Managed();
                            Logger.Debug("Creating local download file: " + tmpfilepath);
                            using (Stream file = new FileStream(tmpfilepath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)){
                                this.Queue.AddEvent(transmissionEvent);
                                downloader.DownloadFile(remoteDocument, file, transmissionEvent, hashAlg);
                                filehash = hashAlg.Hash;
                                success = true;
                                file.Close();
                            }
                        }
                        catch (ObjectDisposedException ex)
                        {
                            Logger.Error(String.Format("Download aborted: {0}", fileName), ex);
                            transmissionEvent.ReportProgress(new TransmissionProgressEventArgs(){Aborted = true, FailedException = ex});
                            return false;
                        }
                        catch (System.IO.DirectoryNotFoundException ex)
                        {
                            Logger.Warn(String.Format("Download failed because of a missing folder in the file path: {0}" , ex.Message ));
                            success = false;
                        }
                        catch (Exception ex)
                        {
                            Logger.Error("Download failed: " + fileName + " " + ex);
                            success = false;
                            Logger.Debug("Removing temp download file: "+ tmpfilepath);
                            File.Delete(tmpfilepath);
                            success = false;
                            if(ex is CmisBaseException)
                            {
                                database.SetOperationRetryCounter(filepath,database.GetOperationRetryCounter(filepath,Database.OperationType.DOWNLOAD)+1,Database.OperationType.DOWNLOAD);
                            }
                        }

                        if (success)
                        {
                            Logger.Info(String.Format("Downloaded remote object({0}): {1}", remoteDocument.Id, fileName));

                            // Get metadata.
                            Dictionary<string, string[]> metadata = null;
                            try
                            {
                                metadata = CmisUtils.FetchMetadata(remoteDocument, session.GetTypeDefinition(remoteDocument.ObjectType.Id));
                            }
                            catch (Exception e)
                            {
                                Logger.Info("Exception while fetching metadata: " + fileName + " " + Utils.ToLogString(e));
                                // Remove temporary local document to avoid it being considered a new document.
                                Logger.Debug("Removing local temp file: " + tmpfilepath);
                                File.Delete(tmpfilepath);
                                transmissionEvent.ReportProgress(new TransmissionProgressEventArgs(){Aborted = true, FailedException = e});
                                return false;
                            }

                            // If file exists, check it.
                            if (File.Exists(filepath))
                            {
                                if (database.LocalFileHasChanged(filepath))
                                {
                                    Logger.Info("Conflict with file: " + fileName + ", backing up locally modified version and downloading server version");
                                    // Rename locally modified file.
                                    //String ext = Path.GetExtension(filePath);
                                    //String filename = Path.GetFileNameWithoutExtension(filePath);
                                    String dir = Path.GetDirectoryName(filepath);

                                    String newFileName = Utils.FindNextConflictFreeFilename(filepath, repoinfo.User);
                                    String newFilePath = Path.Combine(dir, newFileName);
                                    Logger.Debug(String.Format("Moving local file {0} file to new file {1}", filepath, newFilePath));
                                    File.Move(filepath, newFilePath);
                                    Queue.AddEvent(new FileConflictEvent(FileConflictType.CONTENT_MODIFIED,dir,newFilePath));
                                    Logger.Debug(String.Format("Moving temporary local download file {0} to target file {1}", tmpfilepath, filepath));
                                    File.Move(tmpfilepath, filepath);
                                    CmisUtils.SetLastModifiedDate(remoteDocument, filepath, metadata);
                                    Queue.AddEvent(new RecentChangedEvent(filepath));
                                    repo.OnConflictResolved();
                                }
                                else
                                {
                                    Logger.Debug("Removing local file: " + filepath);
                                    File.Delete(filepath);
                                    Logger.Debug(String.Format("Moving temporary local download file {0} to target file {1}", tmpfilepath, filepath));
                                    File.Move(tmpfilepath, filepath);
                                    CmisUtils.SetLastModifiedDate(remoteDocument, filepath, metadata);
                                }
                            }
                            else
                            {
                                Logger.Debug(String.Format("Moving temporary local download file {0} to target file {1}", tmpfilepath, filepath));
                                File.Move(tmpfilepath, filepath);
                                CmisUtils.SetLastModifiedDate(remoteDocument, filepath, metadata);
                            }

                            // Create database entry for this file.
                            database.AddFile(filepath, remoteDocument.Id, remoteDocument.LastModificationDate, metadata, filehash);
                            Queue.AddEvent(new RecentChangedEvent(filepath, remoteDocument.LastModificationDate));

                            Logger.Debug("Added to database: " + fileName);
                        }
                        transmissionEvent.ReportProgress(new TransmissionProgressEventArgs(){Completed = true});
                        return success;
                    }
                    catch (IOException e)
                    {
                        Logger.Warn("Exception while file operation: " + Utils.ToLogString(e));
                        transmissionEvent.ReportProgress(new TransmissionProgressEventArgs(){Aborted = true, FailedException = e});
                        return false;
                    }
                }
            }
    }*/
}

