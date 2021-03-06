﻿//-----------------------------------------------------------------------
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

namespace CmisSync.Lib.Consumer.SituationSolver {
    using System;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;

    using CmisSync.Lib.Cmis.ConvenienceExtenders;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Exceptions;
    using CmisSync.Lib.HashAlgorithm;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;

    using DataSpace.Common.Streams;
    using DataSpace.Common.Transmissions;

    using DotCMIS.Client;
    using DotCMIS.Exceptions;

    using log4net;

    /// <summary>
    /// Abstract enhanced solver.
    /// </summary>
    public abstract class AbstractEnhancedSolver : ISolver {
        /// <summary>
        /// The file operations logger.
        /// </summary>
        protected static readonly ILog OperationsLogger = LogManager.GetLogger("OperationsLogger");

        private static readonly ILog Logger = LogManager.GetLogger(typeof(AbstractEnhancedSolver));

        private bool? serverCanModifyDateTimes = null;

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
        protected bool ServerCanModifyDateTimes {
            get {
                if (this.serverCanModifyDateTimes == null) {
                    try {
                        this.serverCanModifyDateTimes = this.Session.IsServerAbleToUpdateModificationDate();
                    } catch (CmisBaseException) {
                        return false;
                    }
                }

                return this.serverCanModifyDateTimes.GetValueOrDefault();
            }
        }

        /// <summary>
        /// Gets the transmission storage.
        /// </summary>
        /// <value>The transmission storage.</value>
        protected IFileTransmissionStorage TransmissionStorage { get; private set; }

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

        private void SaveCacheFile(
            IFileInfo target,
            IDocument remoteDocument,
            byte[] hash,
            long length,
            Transmission transmissionEvent)
        {
            if (this.TransmissionStorage == null) {
                return;
            }

            target.Refresh();
            IFileTransmissionObject obj = new FileTransmissionObject(transmissionEvent.Type, target, remoteDocument);
            obj.ChecksumAlgorithmName = "SHA-1";
            obj.LastChecksum = hash;
            obj.LastContentSize = length;

            this.TransmissionStorage.SaveObject(obj);
        }

        private bool LoadCacheFile(IFileInfo target, IDocument remoteDocument, IFileSystemInfoFactory fsFactory) {
            if (this.TransmissionStorage == null) {
                return false;
            }

            IFileTransmissionObject obj = this.TransmissionStorage.GetObjectByRemoteObjectId(remoteDocument.Id);
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
            } catch (Exception) {
                localFile.Delete();
                return false;
            }
        }

        protected byte[] DownloadCacheFile(
            IFileInfo target,
            IDocument remoteDocument,
            Transmission transmission,
            IFileSystemInfoFactory fsFactory)
        {
            if (!this.LoadCacheFile(target, remoteDocument, fsFactory)) {
                if (target.Exists) {
                    target.Delete();
                }
            }

            using (var hashAlg = new SHA1Reuse()) {
                using (var filestream = target.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
                using (var downloader = CmisSync.Lib.FileTransmission.ContentTaskUtils.CreateDownloader()) {
                    try {
                        downloader.DownloadFile(
                            remoteDocument,
                            filestream,
                            transmission,
                            hashAlg,
                            (byte[] checksumUpdate, long length) => this.SaveCacheFile(target, remoteDocument, checksumUpdate, length, transmission));
                        if (this.TransmissionStorage != null) {
                            this.TransmissionStorage.RemoveObjectByRemoteObjectId(remoteDocument.Id);
                        }
                    } catch (Exception ex) {
                        transmission.FailedException = ex;
                        throw;
                    }
                }

                target.Refresh();
                return hashAlg.Hash;
            }
        }

        protected byte[] DownloadChanges(
            IFileInfo target,
            IDocument remoteDocument,
            IMappedObject obj,
            IFileSystemInfoFactory fsFactory,
            ITransmissionFactory transmissionFactory,
            ILog logger)
        {
            if (logger == null) {
                throw new ArgumentNullException("logger");
            }

            if (fsFactory == null) {
                throw new ArgumentNullException("fsFactory");
            }

            if (transmissionFactory == null) {
                throw new ArgumentNullException("transmissionFactory");
            }

            if (obj == null) {
                throw new ArgumentNullException("obj");
            }

            // Download changes
            byte[] hash = null;

            var cacheFile = fsFactory.CreateDownloadCacheFileInfo(target);
            var targetFullName = target.FullName;
            var transmission = transmissionFactory.CreateTransmission(TransmissionType.DownloadModifiedFile, targetFullName, cacheFile.FullName);
            hash = this.DownloadCacheFile(cacheFile, remoteDocument, transmission, fsFactory);
            obj.ChecksumAlgorithmName = "SHA-1";

            try {
                var backupFile = fsFactory.CreateFileInfo(targetFullName + ".bak.sync");
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
                    var conflictFileName = conflictFile.FullName;
                    backupFile.MoveTo(conflictFileName);
                    OperationsLogger.Info(string.Format("Updated local content of \"{0}\" with content of remote document {1} and created conflict file {2}", targetFullName, remoteDocument.Id, conflictFileName));
                } else {
                    backupFile.Delete();
                    OperationsLogger.Info(string.Format("Updated local content of \"{0}\" with content of remote document {1}", targetFullName, remoteDocument.Id));
                }
            } catch (Exception ex) {
                transmission.FailedException = ex;
                throw;
            }

            transmission.Status = Status.Finished;
            return hash;
        }

        /// <summary>
        /// Uploads the file content to the remote document.
        /// </summary>
        /// <returns>The SHA-1 hash of the uploaded file content.</returns>
        /// <param name="localFile">Local file.</param>
        /// <param name="doc">Remote document.</param>
        /// <param name="transmission">File Transmission.</param>
        protected byte[] UploadFile(IFileInfo localFile, IDocument doc, Transmission transmission) {
            if (transmission == null) {
                throw new ArgumentNullException("transmission");
            }

            if (localFile == null) {
                throw new ArgumentNullException("localFile");
            }

            using (var file = localFile.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete)) {
                byte[] hash = null;
                using (var uploader = FileTransmission.ContentTaskUtils.CreateUploader())
                using (var hashAlg = new SHA1Managed()) {
                    try {
                        uploader.UploadFile(doc, file, transmission, hashAlg);
                        hash = hashAlg.Hash;
                    } catch (Exception ex) {
                        transmission.FailedException = ex;
                        throw;
                    }
                }

                transmission.Status = Status.Finished;
                return hash;
            }
        }

        /// <summary>
        /// Writes the or use UUID if supported.
        /// </summary>
        /// <returns>The or use UUID if supported.</returns>
        /// <param name="info">Info.</param>
        protected Guid WriteOrUseUuidIfSupported(IFileSystemInfo info) {
            if (info == null) {
                throw new ArgumentNullException("info");
            }

            Guid uuid = Guid.Empty;
            if (info.IsExtendedAttributeAvailable()) {
                try {
                    Guid? localUuid = info.Uuid;
                    if (localUuid == null || this.Storage.GetObjectByGuid((Guid)localUuid) != null) {
                        uuid = Guid.NewGuid();
                        try {
                            info.Uuid = uuid;
                        } catch (RestoreModificationDateException restoreException) {
                            Logger.Debug("Could not retore the last modification date of " + info.FullName, restoreException);
                        }
                    } else {
                        uuid = localUuid ?? Guid.NewGuid();
                    }
                } catch (ExtendedAttributeException ex) {
                    throw new RetryException(ex.Message, ex);
                }
            }

            return uuid;
        }

        /// <summary>
        /// Gets the parent of a IFileInfo or a IDirectoryInfo instance.
        /// </summary>
        /// <returns>The parent.</returns>
        /// <param name="fileInfo">File info.</param>
        protected IDirectoryInfo GetParent(IFileSystemInfo fileInfo) {
            return fileInfo is IDirectoryInfo ? (fileInfo as IDirectoryInfo).Parent : (fileInfo as IFileInfo).Directory;
        }

        /// <summary>
        /// Determines whether one parent of the given instance is read only.
        /// </summary>
        /// <returns><c>true</c> if one of the instance's parents is read only; otherwise, <c>false</c>.</returns>
        /// <param name="localFileSystemInfo">Local file system info.</param>
        protected bool IsParentReadOnly(IFileSystemInfo localFileSystemInfo) {
            var parent = this.GetParent(localFileSystemInfo);
            while (parent != null && parent.Exists) {
                string parentId = Storage.GetRemoteId(parent);
                if (parentId != null) {
                    var remoteObject = this.Session.GetObject(parentId);
                    if (remoteObject.IsReadOnly()) {
                        return true;
                    }

                    break;
                }

                parent = this.GetParent(parent);
            }

            return false;
        }

        /// <summary>
        /// Ensures the that local file name contains legal characters.
        /// If the given file contains UTF-8 only character and the given exception has been returned from the server on creating a file/folder,
        /// an interaction exception is thrown with a hint about the problem. Otherwise nothing happens.
        /// </summary>
        /// <param name="localFile">Local file which produces a CmisConstraintException on the server.</param>
        /// <param name="e">The returned CmisConstraintException returned by the server.</param>
        protected void EnsureThatLocalFileNameContainsLegalCharacters(IFileSystemInfo localFile, CmisConstraintException e) {
            if (localFile == null) {
                throw new ArgumentNullException("localFile");
            }

            var name = localFile.Name;
            if (!Utils.IsValidISO885915(name)) {
                OperationsLogger.Warn(string.Format("Server denied creation of {0}, perhaps because it contains a UTF-8 character", name), e);
                throw new InteractionNeededException(string.Format("Server denied creation of {0}", name), e) {
                    Title = string.Format("Server denied creation of {0}", name),
                    Description = string.Format("Server denied creation of {0}, perhaps because it contains a UTF-8 character", localFile.FullName)
                };
            }
        }
    }
}