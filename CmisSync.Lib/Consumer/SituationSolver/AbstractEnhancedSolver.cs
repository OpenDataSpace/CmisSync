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
    using CmisSync.Lib.Streams;

    using DotCMIS.Client;

    using log4net;

    /// <summary>
    /// Abstract enhanced solver.
    /// </summary>
    public abstract class AbstractEnhancedSolver : ISolver
    {
        /// <summary>
        /// The file operations logger.
        /// </summary>
        protected static readonly ILog OperationsLogger = LogManager.GetLogger("OperationsLogger");

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Consumer.SituationSolver.AbstractEnhancedSolver"/> class.
        /// </summary>
        /// <param name="session">Cmis Session.</param>
        /// <param name="storage">Meta Data Storage.</param>
        /// <param name="transmissionStorage">File Transmission Storage.</param>
        public AbstractEnhancedSolver(
            ISession session,
            IMetaDataStorage storage,
            IFileTransmissionStorage transmissionStorage = null)
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
            this.TransmissionStorage = transmissionStorage;
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

        private IFileTransmissionStorage TransmissionStorage { get; set; }

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

        private IDocument CreateRemotePWCDocument(IDocument remoteDocument) {
            try {
                if (!string.IsNullOrEmpty(remoteDocument.VersionSeriesCheckedOutId)) {
                    remoteDocument.CancelCheckOut();
                    remoteDocument.Refresh();
                }
                remoteDocument.CheckOut();
                remoteDocument.Refresh();
                return Session.GetObject(remoteDocument.VersionSeriesCheckedOutId) as IDocument;
            } catch (Exception ex) {
                return null;
            }
        }

        private IDocument LoadRemotePWCDocument(IDocument remoteDocument) {
            if (TransmissionStorage == null) {
                return CreateRemotePWCDocument(remoteDocument);
            }

            IFileTransmissionObject obj = TransmissionStorage.GetObjectByRemoteObjectId(remoteDocument.Id);
            if (obj == null) {
                return CreateRemotePWCDocument(remoteDocument);
            }

            if (obj.RemoteObjectPWCId != remoteDocument.VersionSeriesCheckedOutId) {
                return CreateRemotePWCDocument(remoteDocument);
            }

            IDocument remotePWCDocument = Session.GetObject(remoteDocument.VersionSeriesCheckedOutId) as IDocument;
            if (remotePWCDocument == null) {
                return CreateRemotePWCDocument(remoteDocument);
            }

            if (remotePWCDocument.ChangeToken != obj.LastChangeToken) {
                return CreateRemotePWCDocument(remoteDocument);
            }

            return remotePWCDocument;
        }

        private void SaveRemotePWCDocument(IFileInfo localFile, IDocument remoteDocument, IDocument remotePWCDocument, FileTransmissionEvent transmissionEvent) {
        }

        /// <summary>
        /// Uploads the file content to the remote document.
        /// </summary>
        /// <returns>The SHA-1 hash of the uploaded file content.</returns>
        /// <param name="localFile">Local file.</param>
        /// <param name="doc">Remote document.</param>
        /// <param name="transmissionManager">Transmission manager.</param>
        protected byte[] UploadFile(IFileInfo localFile, ref IDocument doc, FileTransmissionEvent transmissionEvent) {
            IDocument docPWC = LoadRemotePWCDocument(doc);

            byte[] hash = null;
            IFileUploader uploader = FileTransmission.ContentTaskUtils.CreateUploader();
            transmissionEvent.ReportProgress(new TransmissionProgressEventArgs { Started = true });
            using (var hashAlg = new SHA1Managed()) {
                try {
                    using (var file = localFile.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete)) {
                        if (docPWC == null) {
                            uploader.UploadFile(doc, file, transmissionEvent, hashAlg);
                        } else {
                            using (NonClosingHashStream hashstream = new NonClosingHashStream(file, hashAlg, CryptoStreamMode.Read)) {
                                int bufsize = 8 * 1024;
                                byte[] buffer = new byte[bufsize];
                                for (long offset = 0; offset < docPWC.ContentStreamLength; ) {
                                    int readsize = bufsize;
                                    if (readsize + offset > docPWC.ContentStreamLength) {
                                        readsize = (int)(docPWC.ContentStreamLength.GetValueOrDefault() - offset);
                                    }
                                    readsize = hashstream.Read(buffer, 0, readsize);
                                    offset += readsize;
                                    if (readsize == 0) {
                                        break;
                                    }
                                }
                            }
                            file.Position = docPWC.ContentStreamLength.GetValueOrDefault();
                            uploader.UploadFile(docPWC, file, transmissionEvent, hashAlg, false);
                        }
                        hash = hashAlg.Hash;
                    }
                } catch (FileTransmission.AbortException ex) {
                    SaveRemotePWCDocument(localFile, doc,docPWC, transmissionEvent);
                    transmissionEvent.ReportProgress(new TransmissionProgressEventArgs { FailedException = ex });
                    throw;
                }
                catch (Exception ex) {
                    transmissionEvent.ReportProgress(new TransmissionProgressEventArgs { FailedException = ex });
                    throw;
                }
            }

            transmissionEvent.ReportProgress(new TransmissionProgressEventArgs { Completed = true });
            return hash;
        }

        private void SaveCacheFile(IFileInfo target, IDocument remoteDocument, byte[] hash, FileTransmissionEvent transmissionEvent)
        {
            if (TransmissionStorage == null) {
                return;
            }

            IFileTransmissionObject obj = new FileTransmissionObject(transmissionEvent.Type, target, remoteDocument);
            obj.ChecksumAlgorithmName = "SHA-1";
            obj.LastChecksum = hash;

            TransmissionStorage.SaveObject(obj);
        }

        private bool LoadCacheFile(IFileInfo target, IDocument remoteDocument, IFileSystemInfoFactory fsFactory) {
            if (TransmissionStorage == null) {
                return false;
            }

            IFileTransmissionObject obj = TransmissionStorage.GetObjectByRemoteObjectId(remoteDocument.Id);
            if (obj == null) {
                return false;
            }

            IFileInfo localFile = fsFactory.CreateFileInfo(obj.LocalPath);
            if (!localFile.Exists) {
                return false;
            }

            if (obj.LastChangeToken != remoteDocument.ChangeToken || localFile.Length != obj.LastContentSize) {
                localFile.Delete();
                return false;
            }

            try {
                byte[] localHash;
                using (var f = localFile.Open(FileMode.Open, FileAccess.Read, FileShare.None)) {
                    localHash = SHA1Managed.Create().ComputeHash(f);
                }
                if (!localHash.SequenceEqual(obj.LastChecksum)) {
                    localFile.Delete();
                    return false;
                }

                if (target.FullName != obj.LocalPath) {
                    if (target.Exists) {
                        Guid? uuid = target.Uuid;
                        if (uuid != null) {
                            localFile.Uuid = uuid;
                        }

                        target.Delete();
                    }

                    localFile.MoveTo(target.FullName);
                    target.Refresh();
                }

                return true;
            } catch (Exception ex) {
                localFile.Delete();
                return false;
            }
        }

        protected byte[] DownloadCacheFile(IFileInfo target, IDocument remoteDocument, FileTransmissionEvent transmissionEvent, IFileSystemInfoFactory fsFactory) {
            if (!LoadCacheFile(target, remoteDocument, fsFactory)) {
                if (target.Exists) {
                    target.Delete();
                }
            }

            using (SHA1 hashAlg = new SHA1Managed()) {
                using (var filestream = target.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
                using (IFileDownloader download = ContentTaskUtils.CreateDownloader()) {
                    try {
                        download.DownloadFile(remoteDocument, filestream, transmissionEvent, hashAlg);
                        if (TransmissionStorage != null) {
                            TransmissionStorage.RemoveObjectByRemoteObjectId(remoteDocument.Id);
                        }
                        return hashAlg.Hash;
                    } catch (FileTransmission.AbortException ex) {
                        target.Refresh();
                        SaveCacheFile(target, remoteDocument, hashAlg.Hash, transmissionEvent);
                        transmissionEvent.ReportProgress(new TransmissionProgressEventArgs { FailedException = ex });
                        throw;
                    } catch (Exception ex) {
                        transmissionEvent.ReportProgress(new TransmissionProgressEventArgs { FailedException = ex });
                        throw;
                    }
                }
            }
        }

        protected byte[] DownloadChanges(IFileInfo target, IDocument remoteDocument, IMappedObject obj, IFileSystemInfoFactory fsFactory, ActiveActivitiesManager transmissonManager, ILog logger) {
            // Download changes
            byte[] hash = null;

            var cacheFile = fsFactory.CreateDownloadCacheFileInfo(target);
            var transmissionEvent = new FileTransmissionEvent(FileTransmissionType.DOWNLOAD_MODIFIED_FILE, target.FullName, cacheFile.FullName);
            transmissonManager.AddTransmission(transmissionEvent);
            hash = DownloadCacheFile(cacheFile, remoteDocument, transmissionEvent, fsFactory);
            obj.ChecksumAlgorithmName = "SHA-1";

            try {
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

                if (!obj.LastChecksum.SequenceEqual(checksumOfOldFile)) {
                    var conflictFile = fsFactory.CreateConflictFileInfo(target);
                    backupFile.MoveTo(conflictFile.FullName);
                    OperationsLogger.Info(string.Format("Updated local content of \"{0}\" with content of remote document {1} and created conflict file {2}", target.FullName, remoteDocument.Id, conflictFile.FullName));
                } else {
                    backupFile.Delete();
                    OperationsLogger.Info(string.Format("Updated local content of \"{0}\" with content of remote document {1}", target.FullName, remoteDocument.Id));
                }
            } catch(Exception ex) {
                transmissionEvent.ReportProgress(new TransmissionProgressEventArgs { FailedException = ex });
                throw;
            }

            transmissionEvent.ReportProgress(new TransmissionProgressEventArgs { Completed = true });
            return hash;
        }
    }
}