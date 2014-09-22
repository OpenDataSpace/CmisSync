//-----------------------------------------------------------------------
// <copyright file="AbstractEnhancedSolver.cs" company="GRAU DATA AG">
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

namespace CmisSync.Lib.Consumer.SituationSolver
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;

    using CmisSync.Lib.Cmis.ConvenienceExtenders;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.FileTransmission;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;

    using log4net;

    /// <summary>
    /// Abstract enhanced solver.
    /// </summary>
    public abstract class AbstractEnhancedSolver : ISolver
    {
        protected static readonly ILog OperationsLogger = LogManager.GetLogger("OperationsLogger");

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Consumer.SituationSolver.AbstractEnhancedSolver"/> class.
        /// </summary>
        /// <param name="session">Cmis Session.</param>
        /// <param name="storage">Meta Data Storage.</param>
        /// <param name="serverCanModifyCreationAndModificationDate">Enables the last modification date sync.</param>
        public AbstractEnhancedSolver(
            ISession session,
            IMetaDataStorage storage)
        {
            this.Storage = storage;
            if (session == null) {
                throw new ArgumentNullException("session");
            }

            if (storage == null) {
                throw new ArgumentNullException("storage");
            }

            this.Session = session;
            this.Storage = storage;
            this.ServerCanModifyDateTimes = this.Session.IsServerAbleToUpdateModificationDate();
        }

        /// <summary>
        /// Gets the session.
        /// </summary>
        /// <value>The session.</value>
        protected ISession Session { get; private set; }

        /// <summary>
        /// Gets the storage.
        /// </summary>
        /// <value>The storage.</value>
        protected IMetaDataStorage Storage { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this cmis server can modify date times.
        /// </summary>
        /// <value><c>true</c> if server can modify date times; otherwise, <c>false</c>.</value>
        protected bool ServerCanModifyDateTimes { get; private set; }

        /// <summary>
        /// Solve the specified situation by using localFile and remote object.
        /// </summary>
        /// <param name="localFileSystemInfo">Local filesystem info instance.</param>
        /// <param name="remoteId">Remote identifier or object.</param>
        /// <param name="localContent">Signalizes how the local content has been modified.</param>
        /// <param name="remoteContent">Signalizes how the remote content has been modified.</param>
        public abstract void Solve(
            IFileSystemInfo localFileSystemInfo,
            IObjectId remoteId,
            ContentChangeType localContent,
            ContentChangeType remoteContent);

        /// <summary>
        /// Uploads the file content to the remote document.
        /// </summary>
        /// <returns>The SHA-1 hash of the uploaded file content.</returns>
        /// <param name="localFile">Local file.</param>
        /// <param name="doc">Remote document.</param>
        /// <param name="transmissionManager">Transmission manager.</param>
        protected static byte[] UploadFile(IFileInfo localFile, IDocument doc, ActiveActivitiesManager transmissionManager) {
            byte[] hash = null;
            IFileUploader uploader = FileTransmission.ContentTaskUtils.CreateUploader();
            FileTransmissionEvent transmissionEvent = new FileTransmissionEvent(FileTransmissionType.UPLOAD_MODIFIED_FILE, localFile.FullName);
            transmissionManager.AddTransmission(transmissionEvent);
            transmissionEvent.ReportProgress(new TransmissionProgressEventArgs { Started = true });
            using (var hashAlg = new SHA1Managed())
                using (var file = localFile.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete)) {
                try {
                    uploader.UploadFile(doc, file, transmissionEvent, hashAlg);
                } catch(Exception ex) {
                    transmissionEvent.ReportProgress(new TransmissionProgressEventArgs { FailedException = ex });
                    throw;
                }

                hash = hashAlg.Hash;
            }

            transmissionEvent.ReportProgress(new TransmissionProgressEventArgs { Completed = true });
            return hash;
        }

        protected static byte[] DownloadChanges(IFileInfo target, IDocument remoteDocument, IMappedObject obj, IFileSystemInfoFactory fsFactory, ActiveActivitiesManager transmissonManager, ILog logger) {
            // Download changes
            byte[] lastChecksum = obj.LastChecksum;
            byte[] hash = null;
            var cacheFile = fsFactory.CreateDownloadCacheFileInfo(target);
            var transmissionEvent = new FileTransmissionEvent(FileTransmissionType.DOWNLOAD_MODIFIED_FILE, target.FullName, cacheFile.FullName);
            transmissonManager.AddTransmission(transmissionEvent);
            using (SHA1 hashAlg = new SHA1Managed())
                using (var filestream = cacheFile.Open(FileMode.Create, FileAccess.Write, FileShare.None))
                    using (IFileDownloader download = ContentTaskUtils.CreateDownloader()) {
                try {
                    download.DownloadFile(remoteDocument, filestream, transmissionEvent, hashAlg);
                } catch(Exception ex) {
                    transmissionEvent.ReportProgress(new TransmissionProgressEventArgs { FailedException = ex });
                    throw;
                }

                obj.ChecksumAlgorithmName = "SHA-1";
                hash = hashAlg.Hash;
            }

            var backupFile = fsFactory.CreateFileInfo(target.FullName + ".bak.sync");
            Guid? uuid = target.Uuid;
            cacheFile.Replace(target, backupFile, true);
            try {
                target.Uuid = uuid;
            } catch (RestoreModificationDateException e) {
                logger.Debug("Failed to restore modification date of original file", e);
            }

            try {
                backupFile.Uuid = null;
            } catch (RestoreModificationDateException e) {
                logger.Debug("Failed to restore modification date of backup file", e);
            }

            byte[] checksumOfOldFile = null;
            using (var oldFileStream = backupFile.Open(FileMode.Open, FileAccess.Read, FileShare.None)) {
                checksumOfOldFile = SHA1Managed.Create().ComputeHash(oldFileStream);
            }

            if (!lastChecksum.SequenceEqual(checksumOfOldFile)) {
                var conflictFile = fsFactory.CreateConflictFileInfo(target);
                backupFile.MoveTo(conflictFile.FullName);
                OperationsLogger.Info(string.Format("Updated local content of \"{0}\" with content of remote document {1} and created conflict file {2}", target.FullName, remoteDocument.Id, conflictFile.FullName));
            } else {
                backupFile.Delete();
                OperationsLogger.Info(string.Format("Updated local content of \"{0}\" with content of remote document {1}", target.FullName, remoteDocument.Id));
            }

            transmissionEvent.ReportProgress(new TransmissionProgressEventArgs { Completed = true });
            return hash;
        }
    }
}