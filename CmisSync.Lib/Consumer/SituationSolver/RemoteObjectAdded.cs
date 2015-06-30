//-----------------------------------------------------------------------
// <copyright file="RemoteObjectAdded.cs" company="GRAU DATA AG">
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
    using CmisSync.Lib.FileTransmission;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Storage.Database;
    using CmisSync.Lib.Storage.Database.Entities;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;
  
    using log4net;
 
    /// <summary>
    /// Solver to handle a new object which has been found on the server
    /// </summary>
    public class RemoteObjectAdded : AbstractEnhancedSolver {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(RemoteObjectAdded));

        private IFileSystemInfoFactory fsFactory;
        private ITransmissionFactory manager;

        /// <summary>
        /// Initializes a new instance of the <see cref="CmisSync.Lib.Consumer.SituationSolver.RemoteObjectAdded"/> class.
        /// </summary>
        /// <param name="session">Cmis session.</param>
        /// <param name="storage">Meta data storage.</param>
        /// <param name="transmissionStorage">Transmission progress storage.</param>
        /// <param name="transmissionManager">Transmission manager.</param>
        /// <param name="fsFactory">File system factory.</param>
        public RemoteObjectAdded(
            ISession session,
            IMetaDataStorage storage,
            IFileTransmissionStorage transmissionStorage,
            ITransmissionFactory transmissionManager,
            IFileSystemInfoFactory fsFactory = null) : base(session, storage, transmissionStorage) {
            if (transmissionManager == null) {
                throw new ArgumentNullException("transmissionManager");
            }

            this.fsFactory = fsFactory ?? new FileSystemInfoFactory();
            this.manager = transmissionManager;
        }

        /// <summary>
        /// Adds the Object to Disk and Database
        /// </summary>
        /// <param name='localFile'>
        /// Local file.
        /// </param>
        /// <param name='remoteId'>
        /// Remote Object (already fetched).
        /// </param>
        /// <param name="localContent">Hint if the local content has been changed.</param>
        /// <param name="remoteContent">Information if the remote content has been changed.</param>
        /// <exception cref='ArgumentException'>
        /// Is thrown when remoteId is not prefetched.
        /// </exception>
        public override void Solve(
            IFileSystemInfo localFile,
            IObjectId remoteId,
            ContentChangeType localContent = ContentChangeType.NONE,
            ContentChangeType remoteContent = ContentChangeType.NONE)
        {
            if (localFile is IDirectoryInfo) {
                if (!(remoteId is IFolder)) {
                    throw new ArgumentException("remoteId has to be a prefetched Folder");
                }

                var remoteFolder = remoteId as IFolder;
                IDirectoryInfo localFolder = localFile as IDirectoryInfo;
                localFolder.Create();

                Guid uuid = Guid.Empty;
                if (localFolder.IsExtendedAttributeAvailable()) {
                    uuid = Guid.NewGuid();
                    try {
                        localFolder.Uuid = uuid;
                    } catch (RestoreModificationDateException) {
                    }
                }

                if (remoteFolder.LastModificationDate != null) {
                    try {
                        localFolder.LastWriteTimeUtc = (DateTime)remoteFolder.LastModificationDate;
                    } catch (IOException e) {
                        Logger.Info("Directory modification date could not be synced", e);
                    }
                }

                var mappedObject = new MappedObject(remoteFolder);
                mappedObject.Guid = uuid;
                mappedObject.LastRemoteWriteTimeUtc = remoteFolder.LastModificationDate;
                mappedObject.LastLocalWriteTimeUtc = localFolder.LastWriteTimeUtc;
                mappedObject.Ignored = remoteFolder.AreAllChildrenIgnored();
                this.Storage.SaveMappedObject(mappedObject);
                OperationsLogger.Info(string.Format("New local folder {0} created and mapped to remote folder {1}", localFolder.FullName, remoteId.Id));
            } else if (localFile is IFileInfo) {
                if (!(remoteId is IDocument)) {
                    throw new ArgumentException("remoteId has to be a prefetched Document");
                }

                Guid guid = Guid.NewGuid();
                byte[] localFileHash = null;
                DateTime? lastLocalFileModificationDate = null;
                var file = localFile as IFileInfo;
                if (file.Exists) {
                    Guid? uuid = file.Uuid;
                    if (uuid != null) {
                        if (this.Storage.GetObjectByGuid((Guid)uuid) != null) {
                            throw new ArgumentException("This file has already been synced => force crawl sync");
                        }
                    }

                    lastLocalFileModificationDate = file.LastWriteTimeUtc;
                    if (this.MergeExistingFileWithRemoteFile(file, remoteId as IDocument, guid, out localFileHash)) {
                        return;
                    }

                    Logger.Debug(string.Format("This file {0} conflicts with remote file => conflict file will could be produced after download", file.FullName));
                }

                var cacheFile = this.fsFactory.CreateDownloadCacheFileInfo(guid);

                IDocument remoteDoc = remoteId as IDocument;
                var transmission = this.manager.CreateTransmission(TransmissionType.DOWNLOAD_NEW_FILE, localFile.FullName, cacheFile.FullName);
                byte[] hash = DownloadCacheFile(cacheFile, remoteDoc, transmission, this.fsFactory);

                try {
                    cacheFile.Uuid = guid;
                } catch(RestoreModificationDateException e) {
                    Logger.Debug("Could not retore the last modification date of " + cacheFile.FullName, e);
                }

                try {
                    cacheFile.MoveTo(file.FullName);
                } catch (IOException e) {
                    file.Refresh();
                    if (file.Exists) {
                        if (localFileHash == null ||
                            lastLocalFileModificationDate == null ||
                            !lastLocalFileModificationDate.Equals(file.LastWriteTimeUtc)) {
                            lastLocalFileModificationDate = file.LastWriteTimeUtc;
                            using (var f = file.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete)) {
                                localFileHash = SHA1Managed.Create().ComputeHash(f);
                            }
                        }

                        if (localFileHash.SequenceEqual(hash)) {
                            file.Uuid = guid;
                            try {
                                cacheFile.Delete();
                            } catch(IOException) {
                            }
                        } else {
                            IFileInfo conflictFile = this.fsFactory.CreateConflictFileInfo(file);
                            try {
                                IFileInfo targetFile = cacheFile.Replace(file, conflictFile, true);
                                try {
                                    targetFile.Uuid = guid;
                                } catch (RestoreModificationDateException restoreException) {
                                    Logger.Debug("Could not retore the last modification date of " + targetFile.FullName, restoreException);
                                }
                            } catch (Exception ex) {
                                transmission.FailedException = ex;
                                throw;
                            }

                            try {
                                conflictFile.Uuid = null;
                            } catch(RestoreModificationDateException restoreException) {
                                Logger.Debug("Could not retore the last modification date of " + conflictFile.FullName, restoreException);
                            }
                        }
                    } else {
                        transmission.FailedException = e;
                        throw;
                    }
                }

                file.Refresh();
                if (remoteDoc.LastModificationDate != null) {
                    try {
                        file.LastWriteTimeUtc = (DateTime)remoteDoc.LastModificationDate;
                    } catch(IOException e) {
                        Logger.Debug("Cannot set last modification date", e);
                    }
                }

                MappedObject mappedObject = new MappedObject(
                    file.Name,
                    remoteDoc.Id,
                    MappedObjectType.File,
                    remoteDoc.Parents[0].Id,
                    remoteDoc.ChangeToken,
                    remoteDoc.ContentStreamLength ?? 0)
                {
                    Guid = guid,
                    LastLocalWriteTimeUtc = file.LastWriteTimeUtc,
                    LastRemoteWriteTimeUtc = remoteDoc.LastModificationDate,
                    LastChecksum = hash,
                    ChecksumAlgorithmName = "SHA-1"
                };
                this.Storage.SaveMappedObject(mappedObject);
                OperationsLogger.Info(string.Format("New local file {0} created and mapped to remote file {1}", file.FullName, remoteId.Id));
                transmission.Status = TransmissionStatus.FINISHED;
            }
        }

        private bool MergeExistingFileWithRemoteFile(IFileInfo file, IDocument remoteDoc, Guid guid, out byte[] localHash) {
            byte[] remoteHash = remoteDoc.ContentStreamHash();
            localHash = null;
            if (file.Length.Equals(remoteDoc.ContentStreamLength)) {
                using (var f = file.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete)) {
                    localHash = SHA1Managed.Create().ComputeHash(f);
                }

                if (remoteHash != null) {
                    if (localHash != null && localHash.SequenceEqual(remoteHash)) {
                        if (remoteDoc.LastModificationDate != null) {
                            try {
                                file.LastWriteTimeUtc = (DateTime)remoteDoc.LastModificationDate;
                            } catch(IOException e) {
                                Logger.Debug("Cannot set last modification date", e);
                            }
                        }

                        file.Uuid = guid;
                        MappedObject mappedObject = new MappedObject(
                            file.Name,
                            remoteDoc.Id,
                            MappedObjectType.File,
                            remoteDoc.Parents[0].Id,
                            remoteDoc.ChangeToken,
                            remoteDoc.ContentStreamLength ?? file.Length)
                        {
                            Guid = guid,
                            LastLocalWriteTimeUtc = file.LastWriteTimeUtc,
                            LastRemoteWriteTimeUtc = remoteDoc.LastModificationDate,
                            LastChecksum = localHash,
                            ChecksumAlgorithmName = "SHA-1"
                        };
                        this.Storage.SaveMappedObject(mappedObject);
                        return true;
                    }
                }
            }

            return false;
        }
    }
}