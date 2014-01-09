using System;
using System.IO;

using DotCMIS.Client;

using CmisSync.Lib.Events;

namespace CmisSync.Lib.Sync
{
    public abstract class AbstractFileSynchronizer : ReportingSyncEventHandler
    {
        /// <summary>
        /// Gets the path on which the File Synchronizer is working on.
        /// </summary>
        /// <value>
        /// The path.
        /// </value>
        public FileInfo Path { get; private set; }
        /// <summary>
        /// Gets the session.
        /// </summary>
        /// <value>
        /// The session.
        /// </value>
        protected ISession Session { get; private set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Sync.AbstractFileSynchronizer"/> class.
        /// </summary>
        /// <param name='path'>
        /// Path where the File Synchronizer should work on.
        /// </param>
        /// <param name='session'>
        /// Session which should be used for remote access.
        /// </param>
        /// <param name='queue'>
        /// Queue where the status/failures should be reported to.
        /// </param>
        public AbstractFileSynchronizer (FileInfo path, ISession session, SyncEventQueue queue) : base(queue)
        {
            if (path == null)
                throw new ArgumentException ("Given path cannot be null");
            if (session == null)
                throw new ArgumentNullException ("The given session must not be null");
            Path = path;
            Session = session;
        }

        public abstract void start ();

        public abstract void abort ();

        public abstract void pause ();

        public abstract void resume ();

        public abstract void inform ();

        /*
        /// <summary>
        /// Download a single file from the CMIS server for sync.
        /// </summary>
        private bool SyncDownloadFile (IDocument remoteDocument, string localFolder)
        {
            string fileName = remoteDocument.Name;
            string filePath = Path.Combine (localFolder, fileName);

            // If this file does not have a filename, ignore it.
            // It sometimes happen on IBM P8 CMIS server, not sure why.
            if (remoteDocument.ContentStreamFileName == null) {
                //TODO Possibly the file content has been changed to 0, this case should be handled
                Logger.Warn ("Skipping download of '" + fileName + "' with null content stream in " + localFolder);
                return true;
            }

            bool success = true;

            try {
                if (File.Exists (filePath)) {
                    // Check modification date stored in database and download if remote modification date if different.
                    DateTime? serverSideModificationDate = ((DateTime)remoteDocument.LastModificationDate).ToUniversalTime ();
                    DateTime? lastDatabaseUpdate = database.GetServerSideModificationDate (filePath);

                    if (lastDatabaseUpdate == null) {
                        Logger.Info ("Downloading file absent from database: " + filePath);
                        success = DownloadFile (remoteDocument, localFolder);
                    } else {
                        // If the file has been modified since last time we downloaded it, then download again.
                        if (serverSideModificationDate > lastDatabaseUpdate) {
                            Logger.Info ("Downloading modified file: " + fileName);
                            success = DownloadFile (remoteDocument, localFolder);
                        } else if (serverSideModificationDate == lastDatabaseUpdate) {
                            //  check chunked upload
                            FileInfo fileInfo = new FileInfo (filePath);
                            if (remoteDocument.ContentStreamLength < fileInfo.Length) {
                                success = UpdateFile (filePath, remoteDocument);
                            }
                        }
                    }
                } else {
                    if (database.ContainsFile (filePath)) {
                        long retries = database.GetOperationRetryCounter (filePath, Database.OperationType.DELETE);
                        if (retries <= repoinfo.MaxDeletionRetries) {
                            // File has been recently removed locally, so remove it from server too.
                            Logger.Info ("Removing locally deleted file on server: " + filePath);
                            try {
                                remoteDocument.DeleteAllVersions ();
                                // Remove it from database.
                                database.RemoveFile (filePath);
                                database.SetOperationRetryCounter (filePath, 0, Database.OperationType.DELETE);
                            } catch (CmisBaseException ex) {
                                Logger.Warn ("Could not delete remote file: ", ex);
                                database.SetOperationRetryCounter (filePath, retries + 1, Database.OperationType.DELETE);
                                throw;
                            }
                        } else {
                            Logger.Info (String.Format ("Skipped deletion of remote file {0} because of too many failed retries ({1} max={2})", filePath, retries, repoinfo.MaxDeletionRetries));
                        }
                    } else {
                        // New remote file, download it.
                        Logger.Info ("New remote file: " + filePath);
                        success = DownloadFile (remoteDocument, localFolder);
                    }
                }
            } catch (Exception e) {
                Logger.Warn (String.Format ("Exception while download file to {0}: {1}", filePath, Utils.ToLogString (e)));
                success = false;
            }

            return success;
        }*/
    }
}

